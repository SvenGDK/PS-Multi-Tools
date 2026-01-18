using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Newtonsoft.Json;
using PSMultiTools.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSMultiTools.PS5.Tools;

public partial class PS5PKGSender : Window
{

    public string ConsoleIP = "";

    private string HostIPAddress = "";
    private string? HostedPKGFileName;
    private string? HostedPKGFullPath;
    private bool PortOpened = false;

    private ScrollViewer? LogTextBoxScrollViewer;
    private HttpListener NewHttpListener = new() { IgnoreWriteExceptions = true };
    private CancellationTokenSource? NewHttpListenerCTS;

    public PS5PKGSender()
    {
        InitializeComponent();

        Loaded += PS5PKGSender_Loaded;
        Closing += PS5PKGSender_Closing;
    }

    private void PS5PKGSender_Loaded(object? sender, RoutedEventArgs e)
    {
        LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();

        if (!HttpListener.IsSupported)
        {
            LogTextBox.Text += "Unable to start a web server on this PC.\nYou will not be able to send .pkg files from your PC.\n";
            StartWebServerButton.IsEnabled = false;
            BrowsePKGButton.IsEnabled = false;
            SendPKGFromPCButton.IsEnabled = false;
        }
        else
        {
            string PCHostName = Dns.GetHostName();
            if (!string.IsNullOrEmpty(PCHostName))
            {
                LogTextBox.Text += "HttpListener is supported - You can send .pkg files directly from your PC\n";
                LogTextBox.Text += "Using host name : " + PCHostName + "\n";

                if (Dns.GetHostEntry(PCHostName).AddressList.Length > 0)
                {
                    string LocalIP = "";
                    foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                    {
                        if (address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            LocalIP = address.ToString();
                            break;
                        }
                    }

                    HostIPAddress = LocalIP;
                    LogTextBox.Text += "Using local IP address : " + HostIPAddress + "\n";
                }
                else
                {
                    LogTextBox.Text += "Could not find the local IP address.\nYou will not be able to send .pkg files from your PC.\n";
                }
            }
        }
    }

    private void PS5PKGSender_Closing(object? sender, CancelEventArgs e)
    {
        if (NewHttpListener.IsListening)
        {
            try
            {
                if (PortOpened)
                {
                    ClosePortForWebServer();
                }
                NewHttpListener.Stop();
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            }
            catch { }
        }
    }

    private async void BrowsePKGButton_Click(object? sender, RoutedEventArgs e)
    {
        var pkgFileFilter = new FileDialogFilter
        {
            Name = "PKG File",
            Extensions = ["pkg"]
        };
        var OFD = new OpenFileDialog() { Title = "Select a PKG file", Filters = { pkgFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedPKGFileTextBox.Text = OFDResult[0];
        }
    }

    private string GetJSONForPKG(string FileURL)
    {
        var PKGLinkAsJSON = new PS5URLPKG();

        if (FileURL.Contains("http://") | FileURL.Contains("https://"))
        {
            // Create JSON with given URL
            PKGLinkAsJSON.url = FileURL;
            return JsonConvert.SerializeObject(PKGLinkAsJSON);
        }
        else
        {
            // Create JSON with local IP address & file name
            PKGLinkAsJSON.url = "http://" + HostIPAddress + ":19397/" + FileURL;
            return JsonConvert.SerializeObject(PKGLinkAsJSON);
        }
    }

    private async void SendPKGFromPCButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedPKGFileTextBox.Text))
        {
            string SelectedPKG = Path.GetFileName(SelectedPKGFileTextBox.Text);
            string InstallResponseValue = "";

            LogTextBox.Text += "Sending " + SelectedPKG + "\n";
            LogTextBoxScrollViewer?.ScrollToEnd();

            if (UseDPICheckBox.IsChecked == true)
            {
                try
                {
                    using (var SenderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { ReceiveTimeout = 5000 })
                    {
                        // Connect
                        SenderSocket.Connect(ConsoleIP, 9090);

                        LogTextBox.Text += "Connected to etaHEN DIP" + "\n";

                        // Send JSON
                        byte[] InstallMessage = Encoding.UTF8.GetBytes(GetJSONForPKG(SelectedPKG)); // Creates a JSON with URL for the selected file
                        int SentBytes = SenderSocket.Send(InstallMessage, SocketFlags.None);

                        LogTextBox.Text += "Bytes sent : " + SentBytes.ToString() + "\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();

                        // Get JSON response
                        byte[] ReceiveBuffer = new byte[1024];
                        int ReceivedBytes = SenderSocket.Receive(ReceiveBuffer, SocketFlags.None);
                        var InstallResponse = JsonConvert.DeserializeObject<PS5PKGInstallResponse>(Encoding.UTF8.GetString(ReceiveBuffer, 0, ReceivedBytes));

                        LogTextBox.Text += "Bytes received : " + ReceivedBytes.ToString() + "\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();

                        if (!string.IsNullOrEmpty(InstallResponse.res))
                        {
                            InstallResponseValue = InstallResponse.res;
                        }

                        LogTextBox.Text += "etaHEN DIP response : " + InstallResponseValue + "\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();

                        // Close the connection
                        SenderSocket.Close();

                        LogTextBox.Text += "Disconnected from etaHEN DIP" + "\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    }

                    if (InstallResponseValue == "0")
                    {
                        LogTextBox.Text += "Success! PKG is installing on the PS5.";
                    }
                    else
                    {
                        LogTextBox.Text += "Error: Could not install the selected PKG!";
                    }
                }
                catch (Exception ex)
                {
                    LogTextBox.Text += ex.Message;
                }
            }
            else
            {
                try
                {
                    using var NewHttpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
                    var PS5RequestURL = $"http://{ConsoleIP}:12800/upload";
                    var Boundary = "----DirectPackageInstallerBoundary";
                    using var NewMultipartFormDataContent = new MultipartFormDataContent(Boundary) { { new StringContent(string.Empty), "\"file\"", "\"\"" } };

                    LogTextBox.Text += "http://" + HostIPAddress + ":19397/" + SelectedPKG + "\n";

                    using var NewMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes("http://" + HostIPAddress + ":19397/" + SelectedPKG));
                    NewMultipartFormDataContent.Add(new StreamContent(NewMemoryStream), "\"url\"");

                    var Response = await NewHttpClient.PostAsync(PS5RequestURL, NewMultipartFormDataContent);
                    using var NewMS = new MemoryStream();
                    await Response.Content.CopyToAsync(NewMS);

                    var Result = Encoding.UTF8.GetString(NewMS.ToArray());
                    if (Result.Contains("SUCCESS:"))
                    {
                        LogTextBox.Text += "Success! PKG is now installing.\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    }
                    else
                    {
                        LogTextBox.Text += "Failed to install the PKG!\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    }
                }
                catch (Exception ex) { LogTextBox.Text += $"Error while sending the PKG!\n{ex.Message}\n"; }
            }
        }
        else
        {
            LogTextBox.Text += "Error: No .pkg file selected.\n";
        }
    }

    private async void SendPKGFromURLButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(PKGFileURLTextBox.Text))
        {
            // Check if link is valid
            if (await Utils.IsURLValid(PKGFileURLTextBox.Text))
            {
                if (UseDPICheckBox.IsChecked == true)
                {
                    LogTextBox.Text += "Sending " + PKGFileURLTextBox.Text + "\n";
                    LogTextBoxScrollViewer?.ScrollToEnd();

                    string InstallResponseValue = "";
                    using (var SenderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { ReceiveTimeout = 5000 })
                    {
                        // Connect
                        SenderSocket.Connect(ConsoleIP, 9090);

                        LogTextBox.Text += "Connected to etaHEN DIP" + "\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();

                        // Send JSON
                        byte[] InstallMessage = Encoding.UTF8.GetBytes(GetJSONForPKG(PKGFileURLTextBox.Text));
                        int SentBytes = SenderSocket.Send(InstallMessage, SocketFlags.None);

                        LogTextBox.Text += "Bytes sent : " + SentBytes.ToString() + "\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();

                        // Get JSON response
                        byte[] ReceiveBuffer = new byte[1024];
                        int ReceivedBytes = SenderSocket.Receive(ReceiveBuffer, SocketFlags.None);
                        var InstallResponse = JsonConvert.DeserializeObject<PS5PKGInstallResponse>(Encoding.UTF8.GetString(ReceiveBuffer, 0, ReceivedBytes));

                        LogTextBox.Text += "Bytes received : " + ReceivedBytes.ToString() + "\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();

                        if (!string.IsNullOrEmpty(InstallResponse.res))
                        {
                            InstallResponseValue = InstallResponse.res;
                        }

                        LogTextBox.Text += "etaHEN DIP response : " + InstallResponseValue + "\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();

                        // Close the connection
                        SenderSocket.Close();

                        LogTextBox.Text += "Disconnected from etaHEN DIP\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    }

                    if (InstallResponseValue == "0")
                    {
                        LogTextBox.Text += "Success! PKG is installing on the PS5.\n";
                    }
                    else
                    {
                        LogTextBox.Text += "Error: Could not install the selected PKG!\n";
                    }
                }
                else
                {
                    // Check if Direct Package Installer V2 is service is active
                    bool isActive = false;
                    using var NewTcpClient = new TcpClient();
                    try
                    {
                        var NewIAsyncResult = NewTcpClient.BeginConnect(ConsoleIP, 12800, null, null);
                        bool PortOpen = NewIAsyncResult.AsyncWaitHandle.WaitOne(1000);

                        if (!PortOpen)
                        {
                            isActive = false;
                        }

                        NewTcpClient.EndConnect(NewIAsyncResult);
                        isActive = true;
                    }
                    catch (Exception)
                    {
                        isActive = false;
                    }

                    // Send PKG if active
                    if (isActive)
                    {
                        LogTextBox.Clear();
                        LogTextBox.Text += "PKG has been send to the PS5.\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();

                        try
                        {
                            using var NewHttpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
                            var PS5RequestURL = $"http://{ConsoleIP}:12800/upload";
                            var Boundary = "----DirectPackageInstallerBoundary";
                            using var NewMultipartFormDataContent = new MultipartFormDataContent(Boundary) { { new StringContent(string.Empty), "\"file\"", "\"\"" } };

                            using var NewMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(PKGFileURLTextBox.Text));
                            NewMultipartFormDataContent.Add(new StreamContent(NewMemoryStream), "\"url\"");

                            var Response = await NewHttpClient.PostAsync(PS5RequestURL, NewMultipartFormDataContent);
                            using var NewMS = new MemoryStream();
                            await Response.Content.CopyToAsync(NewMS);

                            var Result = Encoding.UTF8.GetString(NewMS.ToArray());
                            if (Result.Contains("SUCCESS:"))
                            {
                                LogTextBox.Text += "Success! PKG is now installing.\n";
                                LogTextBoxScrollViewer?.ScrollToEnd();
                            }
                            else
                            {
                                LogTextBox.Text += "Failed to install the PKG!\n";
                                LogTextBoxScrollViewer?.ScrollToEnd();
                            }
                        }
                        catch (Exception ex) { LogTextBox.Text += $"Error while sending the PKG!\n{ex.Message}\n"; }
                    }
                    else
                    {
                        LogTextBox.Text += "Error: Port not open. Please enable the Direct Package Installer v2 Service first in etaHEN.\n";
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    }
                }
            }
            else
            {
                LogTextBox.Text += "Error: The specified URL: " + PKGFileURLTextBox.Text + " seems not to be available.\nPlease check the URL and your internet connection if blocked.\n";
            }
        }
        else
        {
            LogTextBox.Text += "Error: No URL specified.";
        }
    }

    private async void StartWebServerButton_Click(object? sender, RoutedEventArgs e)
    {
        if (NewHttpListener.IsListening)
        {
            try
            {
                NewHttpListenerCTS?.Cancel();
                await Task.Delay(200);
                ClosePortForWebServer();
                PortOpened = false;
                NewHttpListener.Stop();
                NewHttpListener.Close();
                NewHttpListenerCTS?.Dispose();
                NewHttpListenerCTS = null;

                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LogTextBox.Text += "Web Server stopped.\n";
                    StartWebServerButton.Content = "Start local Web Server";
                });
            }
            catch (Exception ex)
            {
                LogTextBox.Text += "Error trying to stop the web server :\n" + ex.Message;
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(SelectedPKGFileTextBox.Text))
            {
                // Set working directory to the selected .pkg file's folder
                Directory.SetCurrentDirectory(Path.GetDirectoryName(SelectedPKGFileTextBox.Text)!);

                // Set hosted PKG
                HostedPKGFullPath = SelectedPKGFileTextBox.Text;
                HostedPKGFileName = Path.GetFileName(HostedPKGFullPath);

                try
                {
                    // Start listening
                    NewHttpListener = new() { IgnoreWriteExceptions = true };
                    NewHttpListenerCTS = new CancellationTokenSource();
                    NewHttpListener.Prefixes.Clear();
                    NewHttpListener.Prefixes.Add("http://*:19397/");
                    OpenPortForWebServer();
                    PortOpened = true;
                    NewHttpListener.Start();
                    _ = RunListenerLoopAsync(NewHttpListenerCTS.Token);
                }
                catch (Exception ex)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        LogTextBox.Text += "Error trying to start a web server :\n" + ex.Message + "\nPlease retry running PS Multi Tools as Administrator/Root.\n";
                    });
                    return;
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LogTextBox.Text += $"Switched to directory {Path.GetDirectoryName(SelectedPKGFileTextBox.Text)}\n";
                    LogTextBox.Text += "Web server started. Listening on port 19397\n";
                    LogTextBox.Text += "Hosting package: " + HostedPKGFileName + "\n";
                    LogTextBox.Text += "Please stop the web server when trying to install a .pkg file from a different location!\n";

                    StartWebServerButton.Content = "Stop local Web Server";
                });
            }
            else
            {
                LogTextBox.Text += "Please select a .pkg file before starting the web server.\n";
            }
        }
    }

    private async Task RunListenerLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                HttpListenerContext context;
                try
                {
                    context = await NewHttpListener.GetContextAsync().ConfigureAwait(false);
                }
                catch (HttpListenerException) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (ObjectDisposedException) when (token.IsCancellationRequested)
                {
                    break;
                }

                _ = Task.Run(() => HandleRequestAsync(context, token), token);
            }
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() => LogTextBox.Text += "Listener error: " + ex.Message + "\n");
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context, CancellationToken token)
    {
        var rawUrl = context.Request.Url!.AbsolutePath;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            LogTextBox.Text += "Received request: " + rawUrl + "\n";
            LogTextBoxScrollViewer?.ScrollToEnd();
        });

        if (rawUrl == "/")
        {
            await WriteResponseAsync(DefaultPage(), context.Response, token).ConfigureAwait(false);
        }
        else if (rawUrl == "/" + HostedPKGFileName)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                LogTextBox.Text += "Requested download: " + HostedPKGFileName + "\n";
                LogTextBoxScrollViewer?.ScrollToEnd();
            });

            if (HostedPKGFullPath != null && HostedPKGFileName != null)
            {
                await OfferPKGAsDownloadAsync(context, HostedPKGFullPath, HostedPKGFileName, token).ConfigureAwait(false);
            }
        }
        else
        {
            await WriteResponseAsync(Page404(), context.Response, token).ConfigureAwait(false);
        }
    }

    private async Task WriteResponseAsync(string text, HttpListenerResponse resp, CancellationToken token = default)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        resp.ContentType = "text/html; charset=utf-8";
        resp.ContentLength64 = bytes.Length;
        await resp.OutputStream.WriteAsync(bytes, token).ConfigureAwait(false);
        await resp.OutputStream.FlushAsync(token).ConfigureAwait(false);
        resp.Close();
    }

    private string DefaultPage()
    {
        var sb = new StringBuilder();
        sb.AppendLine("<HTML>");
        sb.AppendLine("<b style=\"font-size:32px\">PS Multi Tools v16.1</b>");
        sb.AppendLine("</br>");
        sb.AppendLine("<b style=\"font-size:24px\">PS5 PKG Web Server v1.0</b>");
        sb.AppendLine("</HTML>");
        return sb.ToString();
    }

    private string Page404()
    {
        var sb = new StringBuilder();
        sb.AppendLine("<HTML>");
        sb.AppendLine("<b style=\"font-size:32px\">PS Multi Tools v16.1</b>");
        sb.AppendLine("</br>");
        sb.AppendLine("<b style=\"font-size:24px\">Error 404 - There is nothing here.</b>");
        sb.AppendLine("</HTML>");
        return sb.ToString();
    }

    private async Task OfferPKGAsDownloadAsync(HttpListenerContext ctx, string fullPath, string fileName, CancellationToken token = default)
    {
        var response = ctx.Response;
        await using var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        response.ContentType = MediaTypeNames.Application.Octet;
        var safeName = fileName.Replace("\"", "");
        response.AddHeader("Content-disposition", $"attachment; filename=\"{safeName}\"");
        response.AddHeader("Accept-Ranges", "bytes");

        var rangeHeader = ctx.Request.Headers["Range"];
        long fileLength = fs.Length;

        if (!string.IsNullOrEmpty(rangeHeader) && rangeHeader.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
        {
            var range = rangeHeader[6..].Split('-');
            long start = string.IsNullOrEmpty(range[0]) ? 0 : long.Parse(range[0]);
            long end = range.Length > 1 && !string.IsNullOrEmpty(range[1]) ? long.Parse(range[1]) : fileLength - 1;
            if (start < 0) start = 0;
            if (end >= fileLength) end = fileLength - 1;
            if (start > end) { response.StatusCode = (int)HttpStatusCode.RequestedRangeNotSatisfiable; response.Close(); return; }

            long length = end - start + 1;
            response.StatusCode = (int)HttpStatusCode.PartialContent;
            response.AddHeader("Content-Range", $"bytes {start}-{end}/{fileLength}");
            response.ContentLength64 = length;

            fs.Seek(start, SeekOrigin.Begin);
            var buffer = new byte[64 * 1024];
            long remaining = length;
            while (remaining > 0 && !token.IsCancellationRequested)
            {
                int toRead = (int)Math.Min(buffer.Length, remaining);
                int read = await fs.ReadAsync(buffer.AsMemory(0, toRead), token).ConfigureAwait(false);
                if (read == 0) break;
                await response.OutputStream.WriteAsync(buffer.AsMemory(0, read), token).ConfigureAwait(false);
                remaining -= read;
            }
            await response.OutputStream.FlushAsync(token).ConfigureAwait(false);
        }
        else
        {
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentLength64 = fileLength;
            await fs.CopyToAsync(response.OutputStream, 64 * 1024, token).ConfigureAwait(false);
            await response.OutputStream.FlushAsync(token).ConfigureAwait(false);
        }

        try { response.Close(); } catch { }
    }

    private void OpenPortForWebServer()
    {
        if (OperatingSystem.IsWindows())
        {
            try
            {
                Type NewFWRuleType = Type.GetTypeFromProgID("HNetCfg.FWRule")!;
                dynamic FWRule = Activator.CreateInstance(NewFWRuleType)!;
                FWRule.Name = "PS Multi Tools PKG Sender Web Server";
                FWRule.Protocol = 6; // TCP
                FWRule.LocalPorts = "19397";
                FWRule.Direction = 1; // In
                FWRule.Action = 1; // Allow
                FWRule.Profiles = (int)NetFwTypeLib.NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_ALL;
                FWRule.Enabled = true;
                Type NewPolicyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2")!;
                NetFwTypeLib.INetFwPolicy2 FWPolicy = (NetFwTypeLib.INetFwPolicy2)Activator.CreateInstance(NewPolicyType)!;
                FWPolicy.Rules.Add(FWRule);

                Type NewFWRuleType2 = Type.GetTypeFromProgID("HNetCfg.FWRule")!;
                dynamic FWRule2 = Activator.CreateInstance(NewFWRuleType2)!;
                FWRule2.Name = "PS Multi Tools PKG Sender Web Server";
                FWRule2.Protocol = 6; // TCP
                FWRule2.LocalPorts = "19397";
                FWRule2.Direction = NetFwTypeLib.NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT; // Out
                FWRule2.Profiles = (int)NetFwTypeLib.NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_ALL;
                FWRule2.Action = 1; // Allow
                FWRule2.Enabled = true;
                Type NewPolicyType2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2")!;
                NetFwTypeLib.INetFwPolicy2 FWPolicy2 = (NetFwTypeLib.INetFwPolicy2)Activator.CreateInstance(NewPolicyType2)!;
                FWPolicy.Rules.Add(FWRule2);
            }
            catch (Exception ex)
            {
                LogTextBox.Text += ex.Message;
            }
        }
        else if (OperatingSystem.IsMacOS())
        {
            string PSMultiToolsPath = Path.Combine(Environment.CurrentDirectory, "PSMultiTools");

            if (!Path.IsPathRooted(PSMultiToolsPath))
                PSMultiToolsPath = Path.GetFullPath(PSMultiToolsPath);

            string addCmd = $"/usr/libexec/ApplicationFirewall/socketfilterfw --add \"{PSMultiToolsPath}\"";
            string unblockCmd = $"/usr/libexec/ApplicationFirewall/socketfilterfw --unblockapp \"{PSMultiToolsPath}\"";
            RunCommandAsAdmin(addCmd);
            RunCommandAsAdmin(unblockCmd);

            string pfRule = "pass in proto tcp from any to any port 19397";
            string pfCmd = $"echo \"{pfRule}\" | /usr/sbin/pfctl -a com.psmultitools -f - && /usr/sbin/pfctl -e";
            RunCommandAsAdmin(pfCmd);
        }
        else
        {
            RunElevated("ufw allow 19397/tcp");
            RunElevated("ufw allow out 19397/tcp");
        }
    }

    private void ClosePortForWebServer()
    {
        if (OperatingSystem.IsWindows())
        {
            try
            {
                Type NewPolicyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2")!;
                NetFwTypeLib.INetFwPolicy2 FWPolicy = (NetFwTypeLib.INetFwPolicy2)Activator.CreateInstance(NewPolicyType)!;
                List<NetFwTypeLib.INetFwRule> ExistingRules = [];

                // Get created rules
                foreach (NetFwTypeLib.INetFwRule ExistingRule in FWPolicy.Rules)
                {
                    if (ExistingRule.Name == "PS Multi Tools PKG Sender Web Server")
                    {
                        ExistingRules.Add(ExistingRule);
                    }
                }
                // Remove created rules
                foreach (NetFwTypeLib.INetFwRule ExistingRule in ExistingRules)
                {
                    FWPolicy.Rules.Remove(ExistingRule.Name);
                }
            }
            catch (Exception ex)
            {
                LogTextBox.Text += ex.Message;
            }
        }
        else if (OperatingSystem.IsMacOS())
        {
            string PSMultiToolsPath = Path.Combine(Environment.CurrentDirectory, "PSMultiTools");
            if (!Path.IsPathRooted(PSMultiToolsPath))
                PSMultiToolsPath = Path.GetFullPath(PSMultiToolsPath);

            string removeCmd = $"/usr/libexec/ApplicationFirewall/socketfilterfw --remove \"{PSMultiToolsPath}\"";
            RunCommandAsAdmin(removeCmd);

            string flushPf = "/usr/sbin/pfctl -a com.psmultitools -F all && /usr/sbin/pfctl -f /etc/pf.conf";
            RunCommandAsAdmin(flushPf);
        }
        else
        {
            RunElevated("ufw delete allow 19397/tcp");
            RunElevated("ufw delete allow out 19397/tcp");
        }
    }

    public static async void RunCommandAsAdmin(string shellCommand)
    {
        string escaped = shellCommand.Replace("\"", "\\\"");
        string appleScript = $"-e \"do shell script \\\"{escaped}\\\" with administrator privileges\"";

        ProcessStartInfo psi = new()
        {
            FileName = "/usr/bin/osascript",
            Arguments = appleScript,
            UseShellExecute = false
        };

        Process proc = new() { StartInfo = psi };
        await proc.WaitForExitAsync();
        proc.Close();
    }

    private static async void RunElevated(string shellCommand)
    {
        string wrapper = $"pkexec sh -c \"{shellCommand.Replace("\"", "\\\"")}\"";
        var psi = new ProcessStartInfo("/bin/sh", $"-lc \"{wrapper}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        Process proc = new() { StartInfo = psi };
        await proc.WaitForExitAsync();
        proc.Close();
    }

    #region Structures

    private struct PS5URLPKG
    {
        public string url { get; set; }
    }

    private struct PS5PKGInstallResponse
    {
        public string res { get; set; }
    }

    #endregion

}