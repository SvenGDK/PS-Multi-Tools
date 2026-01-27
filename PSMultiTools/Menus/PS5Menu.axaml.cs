using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FluentFTP;
using IniParser;
using IniParser.Model;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using PSMultiTools.Classes;
using PSMultiTools.Dialogs;
using PSMultiTools.MultiPlatformTools;
using PSMultiTools.PS5.Tools;
using PSMultiTools.PS5.Tools.Editors;
using PSMultiTools.PS5.Tools.GamePatches;
using PSMultiTools.PS5.Tools.PKGBuilder;
using PSMultiTools.PS5.Tools.WebSrv;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;
using Xilium.CefGlue.Avalonia;

namespace PSMultiTools.Menus;

public partial class PS5Menu : UserControl
{

    private readonly AvaloniaCefBrowser webMANwebsrvWebView = new() { Address = "about:blank" };

    public string SharedIPAddress = "";
    public string SharedFTPPort = "";
    public string SharedPayloadPort = "";

    public bool IswebMANWebSrvWebViewReady = false;
    public bool IswebMANWebSrvCommandExecuted = false;

    public PS5Menu()
    {
        InitializeComponent();

        Loaded += PS5Menu_Loaded;

        // WebMANWebView setup
        webMANwebsrvWebView.BrowserInitialized += WebMANWebView_BrowserInitialized;
        webMANwebsrvWebView.LoadingStateChange += WebMANWebView_LoadingStateChange;
        var browserWrapper = this.FindControl<Decorator>("WebMANWebViewWrapper");
        browserWrapper!.Child = webMANwebsrvWebView;

        IPTextBox.TextChanged += IPTextBox_TextChanged;
        FTPPortTextBox.TextChanged += FTPPortTextBox_TextChanged;
        PayloadPortTextBox.TextChanged += PayloadPortTextBox_TextChanged;
    }

    private void PS5Menu_Loaded(object? sender, RoutedEventArgs e)
    {
        // Load config if exists
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini")))
        {
            try
            {
                var PSMTConfigParser = new FileIniDataParser();
                IniData PSMTConfigData = PSMTConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini"));

                SharedIPAddress = PSMTConfigData["PS5 Tools"]["IP"];
                SharedFTPPort = PSMTConfigData["PS5 Tools"]["FTPPort"];
                SharedPayloadPort = PSMTConfigData["PS5 Tools"]["PayloadPort"];

                IPTextBox.Text = PSMTConfigData["PS5 Tools"]["IP"];
                FTPPortTextBox.Text = PSMTConfigData["PS5 Tools"]["FTPPort"];
                PayloadPortTextBox.Text = PSMTConfigData["PS5 Tools"]["PayloadPort"];
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }

    #region Menu TextBox Changed Events

    public static readonly RoutedEvent<RoutedEventArgs> IPChangedEvent = RoutedEvent.Register<PS5Menu, RoutedEventArgs>(nameof(IPChanged), RoutingStrategies.Bubble);
    public static readonly RoutedEvent<RoutedEventArgs> FTPPortChangedEvent = RoutedEvent.Register<PS5Menu, RoutedEventArgs>(nameof(FTPPortChanged), RoutingStrategies.Bubble);
    public static readonly RoutedEvent<RoutedEventArgs> PayloadPortChangedEvent = RoutedEvent.Register<PS5Menu, RoutedEventArgs>(nameof(PayloadPortChanged), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs> IPChanged
    {
        add => AddHandler(IPChangedEvent, value);
        remove => RemoveHandler(IPChangedEvent, value);
    }

    public event EventHandler<RoutedEventArgs> FTPPortChanged
    {
        add => AddHandler(FTPPortChangedEvent, value);
        remove => RemoveHandler(FTPPortChangedEvent, value);
    }

    public event EventHandler<RoutedEventArgs> PayloadPortChanged
    {
        add => AddHandler(PayloadPortChangedEvent, value);
        remove => RemoveHandler(PayloadPortChangedEvent, value);
    }

    private void RaiseIPChanged() => RaiseEvent(new RoutedEventArgs(IPChangedEvent, this));

    private void RaiseFTPPortChanged() => RaiseEvent(new RoutedEventArgs(FTPPortChangedEvent, this));

    private void RaisePayloadPortChanged() => RaiseEvent(new RoutedEventArgs(PayloadPortChangedEvent, this));

    private void IPTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrEmpty(IPTextBox.Text))
        {
            SharedIPAddress = IPTextBox.Text;
            RaiseIPChanged();
        }
    }

    private void FTPPortTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrEmpty(FTPPortTextBox.Text))
        {
            SharedFTPPort = FTPPortTextBox.Text;
            RaiseFTPPortChanged();
        }
    }

    private void PayloadPortTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrEmpty(PayloadPortTextBox.Text))
        {
            SharedPayloadPort = PayloadPortTextBox.Text;
            RaisePayloadPortChanged();
        }
    }

    #endregion

    #region Tools

    private void OpenLibraryGrabberMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPS5LibraryGrabber = new PS5LibraryGrabber() { ConsoleIP = SharedIPAddress, ConsoleFTPPort = SharedFTPPort, ShowActivated = true };
        NewPS5LibraryGrabber.Show();
    }

    private void PayloadDispatcherMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPayloadDispatcher = new PayloadDispatcher() { ConsoleIP = SharedIPAddress, ShowActivated = true };
        NewPayloadDispatcher.Show();
    }

    private void OpenROMPatcherMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPS5ROMPatcher = new PS5ROMPatcher() { ShowActivated = true };
        NewPS5ROMPatcher.Show();
    }

    private void OpenBackporterMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPS5Backporter = new PS5Backporter() { ShowActivated = true };
        NewPS5Backporter.Show();
    }

    private void OpenYT2JBToolboxMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPS5YT2JBToolbox = new PS5YT2JBToolbox() { ConsoleIP = SharedIPAddress, ConsoleFTPPort = SharedFTPPort, ConsolePayloadPort = SharedPayloadPort, ShowActivated = true };
        NewPS5YT2JBToolbox.Show();
    }

    private void SenderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPS5Sender = new PS5Sender() { ShowActivated = true };

        // Set values if SharedConsoleAddress is set
        NewPS5Sender.IPTextBox.Text = SharedIPAddress;
        NewPS5Sender.PortTextBox.Text = SharedPayloadPort;

        NewPS5Sender.Show();
    }

    private void OpenFTPBrowserMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewFTPBrowser = new FTPBrowser() { ShowActivated = true, ConsoleIP = SharedIPAddress, ConsoleFTPPort = SharedFTPPort };
        NewFTPBrowser.Show();
    }

    private void OpenBDBurnerMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewBDBurner = new BDBurner() { ShowActivated = true };
        NewBDBurner.Show();
    }

    private void OpenWebBrowserInstallerMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPS5WebBrowserAdder = new PS5WebBrowserAdder() { ShowActivated = true, ConsoleIP = SharedIPAddress };
        NewPS5WebBrowserAdder.Show();
    }

    private void OpenNotificationManagerMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPS5NotificationsManager = new PS5Notifications() { ShowActivated = true };
        NewPS5NotificationsManager.IPTextBox.Text = SharedIPAddress;
        NewPS5NotificationsManager.PortTextBox.Text = SharedFTPPort;
        NewPS5NotificationsManager.Show();
    }

    private async void ClearErrorHistoryMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedIPAddress) && !string.IsNullOrEmpty(SharedFTPPort))
        {
            Cursor = new Cursor(StandardCursorType.Wait);
            try
            {
                using (var conn = new FtpClient(SharedIPAddress, "anonymous", "anonymous", Convert.ToInt32(SharedFTPPort)))
                {
                    // Configurate the FTP connection
                    conn.Config.EncryptionMode = FtpEncryptionMode.None;
                    conn.Config.SslProtocols = SslProtocols.None;
                    conn.Config.DataConnectionEncryption = false;

                    // Connect
                    conn.Connect();

                    var ErrorFiles = new List<FtpListItem>();

                    // List disc directory
                    foreach (var item in conn.GetListing("/system_data/priv/error/history"))
                    {
                        switch (item.Type)
                        {
                            case FtpObjectType.File:
                                {
                                    ErrorFiles.Add(item);
                                    break;
                                }
                        }
                    }

                    foreach (var FTPFile in ErrorFiles)
                        // Delete the error file
                        conn.DeleteFile(FTPFile.FullName);

                    // Disonnect
                    conn.Disconnect();
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Info", "The error history on the console has been deleted." + Environment.NewLine + "Check your Settings->System Software->Error History on the PS5.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
            catch (Exception)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could delete the error history, please verify your connection.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }

            Cursor = new Cursor(StandardCursorType.Arrow);
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Cannot connect to the PS5", "Please set your IP:Port in the settings first.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private void OpenGP5ManagerMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewGP5Creator = new GP5Creator() { ShowActivated = true };
        NewGP5Creator.Show();
    }

    private void OpenRCODumperMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPS5RcoDumper = new PS5RcoDumper() { ShowActivated = true, ConsoleIP = SharedIPAddress, ConsolePort = SharedFTPPort };
        NewPS5RcoDumper.Show();
    }

    private void OpenRCOExtractorMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPS5RcoExtractor = new PS5RcoExtractor() { ShowActivated = true };
        NewPS5RcoExtractor.Show();
    }

    private void OpenParamEditorMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewParamEditor = new PS5ParamEditor() { ShowActivated = true };
        NewParamEditor.Show();
    }

    private void OpenPKGBuilderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPKGBuilder = new PS5PKGBuilder() { ShowActivated = true };
        NewPKGBuilder.Show();
    }

    private void OpenPKGExtractorMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPS5PKGExtractor = new PS5PKGExtractor() { ShowActivated = true };
        NewPS5PKGExtractor.Show();
    }

    private void OpenAudioConverterMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewAudioConverter = new PS5AT9Converter() { ShowActivated = true };
        NewAudioConverter.Show();
    }

    private void OpenGamePatchesMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPS5GamePatches = new PS5GamePatches() { ShowActivated = true };
        NewPS5GamePatches.Show();
    }

    private void OpenPKGMergerMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPS5PKGMerger = new PS5PKGMerger() { ShowActivated = true };
        NewPS5PKGMerger.Show();
    }

    private void OpenFTPGrabberMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewFTPGrabber = new PS5FTPGrabber() { ShowActivated = true, ConsoleIP = SharedIPAddress };
        NewFTPGrabber.Show();
    }

    private void OpenManifestEditorMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewManifestEditor = new PS5ManifestEditor() { ShowActivated = true };
        NewManifestEditor.Show();
    }

    private void OpenMakefSELFMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewMakefSELFWindow = new PS5MakefSELFs() { ShowActivated = true };
        NewMakefSELFWindow.Show();
    }

    private void OpenetaHENConfiguratorMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewetaHENConfigurator = new PS5etaHENConfigurator() { ShowActivated = true, ConsoleIP = SharedIPAddress, ConsolePort = SharedFTPPort };
        NewetaHENConfigurator.Show();
    }

    private void OpenShortcutPKGCreatorMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewShortcutPKGCreator = new GP5PKGBuilder() { ShowActivated = true };
        NewShortcutPKGCreator.Show();
    }

    private void PayloadBuilderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPS5PayloadBuilder = new PS5PayloadBuilder() { ShowActivated = true };
        NewPS5PayloadBuilder.Show();
    }

    private void OpenSELFDecrypterMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPS5SELFDecrypter = new PS5SELFDecrypter() { ShowActivated = true, PS5Host = SharedIPAddress, PS5Port = SharedPayloadPort };
        NewPS5SELFDecrypter.Show();
    }

    private async void CheckForUpdatesMenuItems_Click(object? sender, RoutedEventArgs e)
    {
        if (await Utils.IsPSMultiToolsUpdateAvailable())
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Update found", "An update is available, do you want to download it now ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box.ShowWindowAsync();
            if (boxresult == ButtonResult.Yes)
            {
                Utils.DownloadAndExecuteUpdater();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("No update found", "PS Multi Tools is up to date!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    private async void OpenPKGSenderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedIPAddress))
        {
            var NewPKGSender = new PS5PKGSender() { ShowActivated = true, ConsoleIP = SharedIPAddress };
            NewPKGSender.Show();
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Set IP Address first", "Please set your IP address in the settings first.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    private void OpenPortCheckerMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPS5PortChecker = new PS5PortChecker() { ShowActivated = true, PS5Host = SharedIPAddress };
        NewPS5PortChecker.Show();
    }

    private void OpenPSClassicsfPKGBuilderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPSClassicsfPKGBuilder = new PSClassicsfPKGBuilder() { ShowActivated = true };
        NewPSClassicsfPKGBuilder.Show();
    }

    private void OpenKLogViewerMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewLogWindow = new PS5Log() { ShowActivated = true };
        NewLogWindow.PS5IPTextBox.Text = SharedIPAddress;
        NewLogWindow.Show();
    }

    private void OpenPKGReaderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPS5PKGViewer = new PS5PKGViewer() { ShowActivated = true };
        NewPS5PKGViewer.Show();
    }

    private async void OpenDiscParamReaderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedIPAddress))
        {
            try
            {
                bool ParamJSONDownloaded = false;
                using (var NewFTPConnection = new FtpClient(SharedIPAddress, "anonymous", "anonymous", int.Parse(SharedFTPPort)))
                {

                    // Configurate the FTP connection
                    NewFTPConnection.Config.EncryptionMode = FtpEncryptionMode.None;
                    NewFTPConnection.Config.SslProtocols = SslProtocols.None;
                    NewFTPConnection.Config.DataConnectionEncryption = false;

                    // Connect
                    NewFTPConnection.Connect();

                    // Check if the disc is compatible
                    if (NewFTPConnection.DirectoryExists("/mnt/disc/bd"))
                    {
                        if (NewFTPConnection.FileExists("/mnt/disc/bd/param.json"))
                        {
                            // Get the param.json file
                            if (NewFTPConnection.DownloadFile(Path.Combine(Environment.CurrentDirectory, "Cache", "param.json"), "/mnt/disc/bd/param.json", FtpLocalExists.Overwrite, FtpVerify.None, null) == FtpStatus.Success)
                            {
                                ParamJSONDownloaded = true;
                            }
                            else
                            {
                                ParamJSONDownloaded = false;
                            }
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Unknown Disc", "Disc in tray is not compatible.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        }
                    }
                    else
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Unknown Disc", "Disc in tray is not compatible.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                    }

                    // Disonnect
                    NewFTPConnection.Disconnect();
                }

                if (ParamJSONDownloaded)
                {
                    var ParamJSONData = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "Cache", "param.json")).ToList();

                    // Remove unreadable stuff
                    ParamJSONData.RemoveRange(0, 6);

                    // Read the param.json file
                    string ParamJSONString = string.Join(Environment.NewLine, ParamJSONData);
                    ParamJSONString = string.Concat("{" + "\r\n", ParamJSONString);
                    var ParamDiscDetailsData = JsonConvert.DeserializeObject<PS5DiscParamClass.PS5DiscParam>(ParamJSONString);

                    if (ParamDiscDetailsData is not null && ParamDiscDetailsData.Disc is not null)
                    {

                        PS5DiscParamClass.Disc FirstDiscDetails = ParamDiscDetailsData.Disc[0];
                        string FirstDiscMasterDataID = FirstDiscDetails.MasterDataId!;
                        string FirstDiscRole = FirstDiscDetails.Role!;

                        var box = MessageBoxManager.GetMessageBoxStandard("Info", "Disc Master Data ID: " + FirstDiscMasterDataID + Environment.NewLine +
                            "Disc Role: " + FirstDiscRole + Environment.NewLine +
                            "Disc Number: " + ParamDiscDetailsData.DiscNumber.ToString() + Environment.NewLine +
                            "Disc Total: " + ParamDiscDetailsData.DiscTotal.ToString() + Environment.NewLine +
                            "Master Version: " + ParamDiscDetailsData.MasterVersion + Environment.NewLine +
                            "Pub Tools Version: " + ParamDiscDetailsData.PubtoolsVersion + Environment.NewLine +
                            "Required System Version: " + ParamDiscDetailsData.RequiredSystemSoftwareVersion,
                            ButtonEnum.Ok,
                            MsBox.Avalonia.Enums.Icon.Error);

                        await box.ShowWindowAsync();
                    }
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find any valid disc information.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }

            catch (Exception)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find any valid disc information.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Cannot connect to the PS5", "Please set your IP:Port in the settings first.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    private async void SpoofFWMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        // Check if an IP address was entered
        if (!string.IsNullOrWhiteSpace(SharedIPAddress))
        {
            IPAddress DeviceIP;
            int DevicePort;
            try
            {
                DeviceIP = IPAddress.Parse(SharedIPAddress);
                DevicePort = int.Parse(SharedPayloadPort);
            }
            catch (FormatException)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error sending payload", "Could not parse the set console IP. Please check your IP in the settings.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
                return;
            }

            string SelectedELF = Path.Combine(Environment.CurrentDirectory, "Tools", "PS5", "spoof.elf");
            try
            {
                using var SenderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { ReceiveTimeout = 3000 };
                // Connect
                SenderSocket.Connect(DeviceIP, DevicePort);
                // Send ELF
                SenderSocket.SendFile(SelectedELF);
                // Close the connection
                SenderSocket.Close();
            }
            catch (SocketException)
            {
                var box2 = MessageBoxManager.GetMessageBoxStandard("Error sending payload", "Could not send selected payload. Please make sure that your PS5 can receive payloads", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box2.ShowWindowAsync();
                return;
            }

            var box3 = MessageBoxManager.GetMessageBoxStandard("Info", "Spoofing payload has been sent" + Environment.NewLine + "You will need to eject and insert the disc back again to install it." + Environment.NewLine + "To reverse the firmware spoofing simply send this payload again.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box3.ShowWindowAsync();
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Cannot connect to the PS5", "Please set your IP:Port in the settings first.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    private async void Togglekstuff_Click(object? sender, RoutedEventArgs e)
    {
        // Check if an IP address was entered
        if (!string.IsNullOrWhiteSpace(SharedIPAddress))
        {
            IPAddress DeviceIP;
            int DevicePort;

            try
            {
                DeviceIP = IPAddress.Parse(SharedIPAddress);
                DevicePort = int.Parse(SharedPayloadPort);
            }
            catch (FormatException)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error sending payload", "Could not parse the set console IP. Please check your IP in the settings.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
                return;
            }

            string SelectedELF = Path.Combine(Environment.CurrentDirectory, "Tools", "PS5", "kstuff-toggle.elf");
            //var ELFFileInfo = new FileInfo(SelectedELF);
            try
            {
                using var SenderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { ReceiveTimeout = 3000 };
                // Connect
                SenderSocket.Connect(DeviceIP, DevicePort);
                // Send ELF
                SenderSocket.SendFile(SelectedELF);
                // Close the connection
                SenderSocket.Close();
            }
            catch (SocketException)
            {
                var box2 = MessageBoxManager.GetMessageBoxStandard("Error sending payload", "Could not send selected payload. Please make sure that your PS5 can receive payloads", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box2.ShowWindowAsync();
                return;
            }

            var box3 = MessageBoxManager.GetMessageBoxStandard("Info", "kstuff toggled!\nTo revert, simply click again.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box3.ShowWindowAsync();
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Cannot connect to the PS5", "Please set your IP:Port in the settings first.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    #endregion

    #region Downloads

    #region Homebrew

    private async void DownloadAppDumper_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/EchoStretch/ps5-app-dumper/releases/latest/download/ps5-app-dumper.zip") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadDumpRunner_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/EchoStretch/dump_runner/releases/latest/download/dump_runner.zip") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadDumpInstaller_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/EchoStretch/dump_installer/releases/latest/download/dump_installer.zip") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadAvatarChanger_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/AvatarChanger_v1.00.pkg") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadFTPS5_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/EchoStretch/FTPS5/releases/download/v1.4/ftps5-1.4.zip") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadPS5NetworkELFLoader650_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/PS2NetworkELFLoader/VMC0-PS5-6-50.card") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadPS5NetworkGameLoader650_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/PS2NetworkGameLoader/mast1c0re-ps2-network-game-loader-PS5-6-50.elf") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadInternetBrowserPKG_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/pkg/InternetBrowserPS5.pkg") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadStorePreviewPKG_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/pkg/StorePreviewPS5.pkg") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadFPKGiPKG_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/FPKGi_v1.01.1.pkg") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadGameHubPreviewPKG_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/pkg/GameHubPreviewPS5.pkg") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadDebugPKG_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/pkg/DebugSettingsPS5.pkg") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadPSMTPKG_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/pkg/PSMultiToolsHost.pkg") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadKStuff_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/EchoStretch/kstuff/releases/latest/download/kstuff.elf") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadKStuffToggle_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/EchoStretch/kstuff-toggle/releases/latest/download/Kstuff-Toggle.zip") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadLatestetaHEN_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/etaHEN/etaHEN/releases/download/2.5B/etaHEN-2.5B.bin") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadHomebrewStore_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/Store-R2-PS5.pkg") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadItemzflow_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/ItemzflowGameManager_v1.14.pkg") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadPS5Xplorer_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/PS5-Xplorer_v1.04.pkg") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadSELFDecrypter_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/hb/ps5-self-decrypter_v0.3.elf") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadELFLdrGitHub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/elfldr/releases/latest/download/elfldr-ps5.elf") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadFetchPKGPS5Github_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/fetchpkg/releases/latest/download/PS5.zip") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadFetchPKGWinGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/fetchpkg/releases/latest/download/Win-x86_64.zip") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadFetchPKGLinuxGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/fetchpkg/releases/latest/download/Linux-x86_64.zip") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadFTPSrvGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/ftpsrv/releases/latest/download/ftpsrv-ps5.elf") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadGDBSrvGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/gdbsrv/releases/latest/download/gdbsrv-ps5.elf") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadKLogSrvGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/klogsrv/releases/latest/download/klogsrv-ps5.elf") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadLinkDevGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/linkdev/releases/latest/download/LinkDev.zip") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadOffActGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/offact/releases/latest/download/OffAct.zip") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadSHSrvGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/shsrv/releases/latest/download/shsrv-ps5.elf") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadWebSrvGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/ps5-payload-dev/websrv/releases/latest/download/websrv-ps5.elf") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    #region JAR

    private async void DownloadByepervisor_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/byepervisor-1.0-SNAPSHOT.jar") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadDebugSettings_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/debugsettings-1.0-SNAPSHOT.jar") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadDumpClassPath_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/dumpclasspath-1.0-SNAPSHOT.jar") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadCurProc_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/dumpcurproc-1.0-SNAPSHOT.jar") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadJARFTPServer_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/ftpserver-1.0-SNAPSHOT.jar") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadJailbreak_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/jailbreak-1.0-SNAPSHOT.jar") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadKerneldump_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/kerneldump-1.0-SNAPSHOT.jar") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadJARKlogSrv_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/klogserver-1.0-SNAPSHOT.jar") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadListDirEnts_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/listdirents-1.0-SNAPSHOT.jar") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadMiniTennis_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/minitennis-1.0-SNAPSHOT.jar") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadPrintSysProps_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/printsysprops-1.0-SNAPSHOT.jar") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadUMTX1_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/umtx1-1.0-SNAPSHOT.jar") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadUMTX2_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5" };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/umtx2-1.0-SNAPSHOT.jar") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    #endregion

    #endregion

    #region Firmwares

    private async void DownloadRecoveryFW403_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/recovery/04.03/PS5UPDATE.PUP") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadSystemFW403_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/system/04.03/PS5UPDATE.PUP") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadRecoveryFW451_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/recovery/04.51/PS5UPDATE.PUP") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadSystemFW451_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/system/04.51/PS5UPDATE.PUP") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadRecoveryFW550_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/recovery/05.50/PS5UPDATE.PUP") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadSystemFW550_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/system/05.50/PS5UPDATE.PUP") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadRecoveryFW650_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/recovery/06.50/PS5UPDATE.PUP") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadSystemFW650_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/system/06.50/PS5UPDATE.PUP") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadRecoveryFW761_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/recovery/07.61/PS5UPDATE.PUP") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadSystemFW761_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/system/07.61/PS5UPDATE.PUP") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadRecoveryFW1001_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/recovery/10.01/PS5UPDATE.PUP") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadSystemFW1001_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/fw/system/10.01/PS5UPDATE.PUP") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    #endregion

    #endregion

    #region Exploits

    private void OpenMast1c0reGitHub_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/McCaulay/mast1c0re") { UseShellExecute = true });
    }

    private async void DownloadPS5IPV6Expl_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/ex/PS5-IPV6-Kernel-Exploit-1.03.zip") == false)
        {

            NewDownloader.Close();
        }
    }

    private void OpenKexGitHub_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/Cryptogenic/PS5-IPV6-Kernel-Exploit") { UseShellExecute = true });
    }

    private void OpenJARLoaderGitHub_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/hammer-83/ps5-jar-loader") { UseShellExecute = true });
    }

    private async void DownloadJARLoader_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/hammer-83/ps5-jar-loader/releases/download/v4.1.1/ps5-jar-loader-4.1.1.iso") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadUMTXJailbreak_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/PS5Dev/PS5-UMTX-Jailbreak/archive/refs/tags/v1.2.zip") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private void OpenUMTXJailbreakGitHub_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/PS5Dev/PS5-UMTX-Jailbreak") { UseShellExecute = true });
    }

    private async void DownloadAiOBDJB_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/Viktorious-x/ps5-bdjb-modified-ISOs/releases/download/Modified-ISO18/Auto_Jailbreak_all_in_one_PS5v23.iso") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private void OpenAiOBDJBGitHub_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/Viktorious-x/ps5-bdjb-modified-ISOs") { UseShellExecute = true });
    }

    private async void DownloadYT2JBBackup_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/Gezine/Y2JB/releases/download/Y2JB-1.2.1/Y2JB_backup_1.2.1.zip") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadYT2JB0Dat_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/Gezine/Y2JB/releases/download/Y2JB-1.2.1/Y2JB_download0_1.2.1.zip") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private void OpenYT2JBGitHub_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/Gezine/Y2JB") { UseShellExecute = true });
    }

    private async void DownloadYT2JBAutoloader_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/itsPLK/ps5_y2jb_autoloader/releases/download/v0.3/download0.dat") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadYT2JBAutoloaderUpdate_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/itsPLK/ps5_y2jb_autoloader/releases/download/v0.3/y2jb_update.zip") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private void OpenYT2JBAutoloaderGitHub_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/itsPLK/ps5_y2jb_autoloader") { UseShellExecute = true });
    }

    private async void DownloadNFlix256GBUSB_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/NetflixNHack/Netflix-N-Hack/releases/download/External-Drive/Netflix_PS5_EU_Ext.7z") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadNFlix256GB_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/NetflixNHack/Netflix-N-Hack/releases/download/1.01/Netflix.256GB.7z") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadNFlix500GB_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/NetflixNHack/Netflix-N-Hack/releases/download/1.01/Netflix.500GB.7z") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadNFlix1TB_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/NetflixNHack/Netflix-N-Hack/releases/download/1.01/Netflix.1000GB.7z") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadNFlix2TB_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/NetflixNHack/Netflix-N-Hack/releases/download/1.01/Netflix.2000GB.7z") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadNFlix2048GB_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/NetflixNHack/Netflix-N-Hack/releases/download/1.01/Netflix.2048GB.7z") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadNFlix4TB_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/NetflixNHack/Netflix-N-Hack/releases/download/1.01/Netflix.4000GB.7z") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadNFlixSystemBackup_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/NetflixNHack/Netflix-N-Hack/releases/download/1.01/Netflix_systembackup.zip") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private async void DownloadNFlixV6PKG_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps5/ex/NFlix/PS5_PPSA01615_v6.00.pkg") == false)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            NewDownloader.Close();
        }
    }

    private void OpenNFlixGitHub_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/itsPLK/ps5_y2jb_autoloader") { UseShellExecute = true });
    }

    #endregion

    #region WebMAN WebSrv

    public void NavigateTowebMANWebSrvUrl(string InputURL)
    {
        if (!string.IsNullOrEmpty(SharedIPAddress))
        {
            if (IswebMANWebSrvWebViewReady && IswebMANWebSrvCommandExecuted)
            {
                IswebMANWebSrvCommandExecuted = false;
                webMANwebsrvWebView.Address = InputURL;
            }
        }
    }

    private void WebMANWebView_BrowserInitialized()
    {
        IswebMANWebSrvWebViewReady = true;
    }

    private async void WebMANWebView_LoadingStateChange(object sender, Xilium.CefGlue.Common.Events.LoadingStateChangeEventArgs e)
    {
        if (!e.IsLoading)
        {
            IswebMANWebSrvCommandExecuted = true;
        }
    }

    private async void OpenPS5WebSrvInterface_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedIPAddress))
        {
            var NewwebMANMODWebGUI = new PS5webMANBrowser() { ShowActivated = true, WebMANWebSrvAddress = "http://" + SharedIPAddress + ":8080" };
            NewwebMANMODWebGUI.Show();
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("No IP Address", "Please set your PS5 IP address in the Settings.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    private async void BrowsePS5FileSystem_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedIPAddress))
        {
            var NewwebMANMODWebGUI = new PS5webMANBrowser() { ShowActivated = true, WebMANWebSrvAddress = "http://" + SharedIPAddress + ":8080/fs/" };
            NewwebMANMODWebGUI.Show();
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("No IP Address", "Please set your PS5 IP address in the Settings.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    private void ManagePS5Homebrew_Click(object? sender, RoutedEventArgs e)
    {
        var NewWebSrvHomebrewManager = new WebSrvHomebrewManager() { ShowActivated = true };
        NewWebSrvHomebrewManager.PS5IPTextBox.Text = SharedIPAddress;
        NewWebSrvHomebrewManager.Show();
    }

    private void ManagePS5GameROMs_Click(object? sender, RoutedEventArgs e)
    {
        var NewWebSrvHomebrewManager = new WebSrvHomebrewManager() { ShowActivated = true };
        NewWebSrvHomebrewManager.PS5IPTextBox.Text = SharedIPAddress;
        NewWebSrvHomebrewManager.Show();
    }

    #endregion

    #region shSrv

    private async void GetAuthID_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var NewTelnetClient = new TelnetClient(SharedIPAddress, 2323);

            Thread.Sleep(20000); // Wait 20sec until connected

            // Get welcome message to check if connected successfully
            string WelcomeMesssage = NewTelnetClient.Read();
            if (!string.IsNullOrEmpty(WelcomeMesssage))
            {

                NewTelnetClient.Write("authid");

                Thread.Sleep(500);

                string RetrievedAuthID = NewTelnetClient.Read();

                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Info", "AuthID: " + RetrievedAuthID, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                });
            }
            else
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error reading output", "ShSrv took to long to respond.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                });
                NewTelnetClient.Close();
            }

            // Disconnect
            NewTelnetClient.Close();
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "An error occurred: " + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            });     
        }
    }

    private async void GetConsoleInfo_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var NewTelnetClient = new TelnetClient(SharedIPAddress, 2323);

            Thread.Sleep(20000); // Wait 20sec until connected

            // Get welcome message and split console information output to values
            string WelcomeMesssage = NewTelnetClient.Read();
            if (!string.IsNullOrEmpty(WelcomeMesssage))
            {
                var WelcomeData = new Dictionary<string, string>();
                string[] SplittedLines = WelcomeMesssage.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

                foreach (string Line in SplittedLines)
                {
                    string[] LineParts = Line.Split(':');
                    if (LineParts.Length >= 2)
                    {
                        string DictKey = LineParts[0].Trim();
                        string DictValue = LineParts[1].Trim();
                        WelcomeData[DictKey] = DictValue;
                    }
                }

                string ConsoleModel = WelcomeData.TryGetValue("Model", out var value5) ? value5 : "";
                string ConsoleSerialNumber = WelcomeData.TryGetValue("S/N", out var value4) ? value4 : "";
                string ConsoleSoftwareVersion = WelcomeData.TryGetValue("S/W", out var value3) ? value3 : "";
                string ConsoleSoCTemp = WelcomeData.TryGetValue("SoC temp", out var value2) ? value2 : "";
                string ConsoleCPUTemp = WelcomeData.TryGetValue("CPU temp", out var value1) ? value1 : "";
                string ConsoleCPUFreq = WelcomeData.TryGetValue("CPU freq", out var value) ? value : "";

                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Info", "Model Number: " + ConsoleModel + Environment.NewLine +
                        "Serial Number: " + ConsoleSerialNumber + Environment.NewLine +
                        "Software Version: " + ConsoleSoftwareVersion + Environment.NewLine +
                        "SoC Temperature: " + ConsoleSoCTemp + Environment.NewLine +
                        "CPU Temperature: " + ConsoleCPUTemp + Environment.NewLine +
                        "CPU Frequency: " + ConsoleCPUFreq, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                });
            }
            else
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error reading output", "ShSrv took to long to respond.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                });
                NewTelnetClient.Close();
            }

            // Disconnect
            NewTelnetClient.Close();
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "An error occurred: " + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            });  
        }
    }

    #endregion

    private void SettingsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewSettings = new PSSettings() { ShowActivated = true };
        NewSettings.Show();
    }

}