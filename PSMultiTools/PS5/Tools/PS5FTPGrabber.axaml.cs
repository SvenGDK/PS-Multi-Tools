using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FluentFTP;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace PSMultiTools.PS5.Tools;

public partial class PS5FTPGrabber : Window
{

    public string ConsoleIP = "";
    public string ConsolePort = "";

    public string SelectedPath = "";
    public string App0RemoteFolder = "";
    public string App0RemoteFolderName = "";
    public string DiscRemoteFolder = "";
    public string CustomRemoteFolder = "";

    public string AppMetadataRemoteFolder = "";
    public string SystemAppMetadataRemoteFolder = "";
    public string NPBindRemotePath = "";

    public int TotalFiles = 0;
    public int CopiedFiles = 0;

    public FtpConfig NewFtpConfig = new() { EncryptionMode = FtpEncryptionMode.None, SslProtocols = SslProtocols.None, DataConnectionEncryption = false, ValidateAnyCertificate = true };

    public PS5FTPGrabber()
    {
        InitializeComponent();
        SelectedFolderComboBox.SelectionChanged += SelectedFolderComboBox_SelectionChanged;
    }

    private async void DownloadButton_Click(object? sender, RoutedEventArgs e)
    {

        App0RemoteFolder = string.Empty;
        DiscRemoteFolder = string.Empty;
        CustomRemoteFolder = string.Empty;
        AppMetadataRemoteFolder = string.Empty;
        SystemAppMetadataRemoteFolder = string.Empty;
        NPBindRemotePath = string.Empty;

        Cursor = new Cursor(StandardCursorType.Wait);

        if (SelectedFolderComboBox.Text == "/mnt/sandbox/pfsmnt/")
        {

            if (MetadataDumpCheckBox.IsChecked == true)
            {

                if (await GetAppMetadata() == true)
                {

                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            ReceiveProgressBar.Value = 0;
                            ReceiveStatusTextBlock.Text = "Getting metadata, please wait ... 0/" + TotalFiles.ToString();
                            ReceiveProgressBar.Maximum = TotalFiles;
                        });
                    }
                    else
                    {
                        ReceiveProgressBar.Value = 0;
                        ReceiveStatusTextBlock.Text = "Getting metadata, please wait ... 0/" + TotalFiles.ToString();
                        ReceiveProgressBar.Maximum = TotalFiles;
                    }

                    LockUI();

                    await GetMetadata();

                    Cursor = new Cursor(StandardCursorType.Arrow);

                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            ReceiveProgressBar.Value = 0d;
                            ReceiveStatusTextBlock.Text = "Transfer Status: Dump completed!";
                        });
                    }
                    else
                    {
                        ReceiveProgressBar.Value = 0d;
                        ReceiveStatusTextBlock.Text = "Transfer Status: Dump completed!";
                    }
                }
                else
                {
                    Cursor = new Cursor(StandardCursorType.Arrow);
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find any metadata.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }

            else if (SELFDumpCheckBox.IsChecked == true)
            {

                if (string.IsNullOrEmpty(SelectedDirectoryTextBox.Text))
                {
                    // If no path is specified then create a "Dumps" folder
                    if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Dumps")))
                    {
                        Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Dumps"));
                    }
                    // Set the new folder
                    SelectedPath = Path.Combine(Environment.CurrentDirectory, "Dumps");
                }

                LockUI();

                await DumpSELFData();

                // Update status
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        ReceiveProgressBar.IsIndeterminate = false;
                        ReceiveStatusTextBlock.Text = "";
                    });
                }
                else
                {
                    ReceiveProgressBar.IsIndeterminate = false;
                    ReceiveStatusTextBlock.Text = "";
                }

                LockUI();
                Cursor = new Cursor(StandardCursorType.Arrow);

                if (SelectedPath == Path.Combine(Environment.CurrentDirectory, "Dumps"))
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "SELF files dumped!" + Environment.NewLine + "Open the 'Dumps' folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult = await box.ShowWindowDialogAsync(this);
                    if (boxresult == ButtonResult.Yes)
                    {
                        Utils.OpenFolder(Path.Combine(Environment.CurrentDirectory, "Dumps"));
                    }
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Info", "SELF files dumped!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
            }

            else if (await GetApp0(ConsoleIP) == true)
            {

                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        ReceiveProgressBar.Value = 0;
                        ReceiveStatusTextBlock.Text = "Starting, please wait ... 0/" + TotalFiles.ToString();
                        ReceiveProgressBar.Maximum = TotalFiles;
                    });
                }
                else
                {
                    ReceiveProgressBar.Value = 0;
                    ReceiveStatusTextBlock.Text = "Starting, please wait ... 0/" + TotalFiles.ToString();
                    ReceiveProgressBar.Maximum = TotalFiles;
                }

                LockUI();
                await GetAppData();
            }
            else
            {
                Cursor = new Cursor(StandardCursorType.Arrow);
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find any mounted game.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }

        else if (SelectedFolderComboBox.Text == "/mnt/disc/")
        {

            if (await FilesAvailable("/mnt/disc/") == true)
            {
                DiscRemoteFolder = "/mnt/disc/";

                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        ReceiveStatusTextBlock.Text = "Starting, please wait ... 0/" + TotalFiles.ToString();
                        ReceiveProgressBar.Maximum = TotalFiles;
                    });
                }
                else
                {
                    ReceiveStatusTextBlock.Text = "Starting, please wait ... 0/" + TotalFiles.ToString();
                    ReceiveProgressBar.Maximum = TotalFiles;
                }

                LockUI();
                await GetAppData();
            }
            else
            {
                Cursor = new Cursor(StandardCursorType.Arrow);
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "No disc inserted !", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }

        else
        {
            CustomRemoteFolder = SelectedFolderComboBox.Text!;

            if (await FilesAvailable(CustomRemoteFolder) == true)
            {
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        ReceiveStatusTextBlock.Text = "Starting, please wait ... 0/" + TotalFiles.ToString();
                        ReceiveProgressBar.Maximum = TotalFiles;
                    });
                }
                else
                {
                    ReceiveStatusTextBlock.Text = "Starting, please wait ... 0/" + TotalFiles.ToString();
                    ReceiveProgressBar.Maximum = TotalFiles;
                }

                LockUI();
                await GetAppData();
            }
            else
            {
                Cursor = new Cursor(StandardCursorType.Arrow);
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "No files available !", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }

    }

    public async Task<bool> GetApp0(string ConsoleIP)
    {
        try
        {
            using var NewFtpClient = new AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsolePort), NewFtpConfig);

            // Connect
            await NewFtpClient.Connect();

            // Get the app0 folder
            foreach (FtpListItem DirectoryInFTP in await NewFtpClient.GetListing("/mnt/sandbox/pfsmnt/"))
            {
                if (DirectoryInFTP.Name.EndsWith("app0"))
                {
                    App0RemoteFolderName = DirectoryInFTP.Name;
                    App0RemoteFolder = DirectoryInFTP.FullName;
                    break;
                }
            }

            // Disonnect
            await NewFtpClient.Disconnect();
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            });       
        }

        if (string.IsNullOrEmpty(App0RemoteFolder))
        {
            return false;
        }
        else
        {
            try
            {
                using var NewFtpClient = new AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsolePort), NewFtpConfig);

                // Connect
                await NewFtpClient.Connect();

                // Reset and get total files count
                TotalFiles = 0;
                CopiedFiles = 0;

                foreach (FtpListItem FileInFTP in await NewFtpClient.GetListing(App0RemoteFolder))
                    TotalFiles += 1;
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                });     
            }

            return true;
        }
    }

    public async Task<bool> GetAppMetadata()
    {
        string GameID = string.Empty;

        try
        {
            using var NewFtpClient = new AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsolePort), NewFtpConfig);

            // Connect
            await NewFtpClient.Connect();

            if (string.IsNullOrEmpty(App0RemoteFolderName))
            {
                // Get the app0 folder
                foreach (FtpListItem DirectoryInFTP in await NewFtpClient.GetListing("/mnt/sandbox/pfsmnt/"))
                {
                    if (DirectoryInFTP.Name.EndsWith("app0"))
                    {
                        App0RemoteFolderName = DirectoryInFTP.Name;
                        App0RemoteFolder = DirectoryInFTP.FullName;
                        break;
                    }
                }
            }

            GameID = App0RemoteFolderName.Split('-')[0];

            // Check if the metadata exists
            foreach (FtpListItem DirectoryInFTP in await NewFtpClient.GetListing("/user/appmeta/"))
            {
                if ((DirectoryInFTP.Name ?? "") == (GameID ?? ""))
                {
                    AppMetadataRemoteFolder = DirectoryInFTP.FullName;
                    break;
                }
            }
            foreach (FtpListItem DirectoryInFTP in await NewFtpClient.GetListing("/system_data/priv/appmeta/"))
            {
                if ((DirectoryInFTP.Name ?? "") == (GameID ?? ""))
                {
                    SystemAppMetadataRemoteFolder = DirectoryInFTP.FullName;
                    break;
                }
            }

            // Check for npbind.dat
            foreach (FtpListItem FileInFTP in await NewFtpClient.GetListing("/system_data/priv/appmeta/" + GameID + "/trophy2/"))
            {
                if (FileInFTP.Name == "npbind.dat")
                {
                    NPBindRemotePath = FileInFTP.FullName;
                    break;
                }
            }

            // Disonnect
            await NewFtpClient.Disconnect();
        }

        catch (Exception ex)
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            });      
        }

        if (!string.IsNullOrEmpty(AppMetadataRemoteFolder))
        {
            try
            {
                using var NewFtpClient = new AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsolePort), NewFtpConfig);

                // Connect
                await NewFtpClient.Connect();

                // Reset and get total files count
                TotalFiles = 3;
                CopiedFiles = 0;

                foreach (FtpListItem FileInFTP in await NewFtpClient.GetListing(AppMetadataRemoteFolder))
                    TotalFiles += 1;
                foreach (FtpListItem FileInFTP in await NewFtpClient.GetListing(SystemAppMetadataRemoteFolder))
                    TotalFiles += 1;

            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                });
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> FilesAvailable(string RemotePath)
    {
        using var NewFtpClient = new AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsolePort), NewFtpConfig);

        // Connect
        await NewFtpClient.Connect();

        // Reset and get total files count
        TotalFiles = 0;
        CopiedFiles = 0;

        foreach (var FileInFTP in await NewFtpClient.GetListing(RemotePath))
            TotalFiles += 1;

        // Disonnect
        await NewFtpClient.Disconnect();

        if (TotalFiles > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task GetMetadata()
    {
        await Task.Run(async () =>
        {
            using var NewFtpClient = new AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsolePort), NewFtpConfig);

            // Connect
            await NewFtpClient.Connect();

            // Get npbind.dat
            if (!string.IsNullOrEmpty(NPBindRemotePath))
            {

                if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory + "Cache")))
                {
                    Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory + "Cache"));
                }

                await NewFtpClient.DownloadFile(Path.Combine(Environment.CurrentDirectory + "Cache", "npbind.dat"), NPBindRemotePath, FtpLocalExists.Overwrite, FtpVerify.None);
            }

            if (File.Exists(Path.Combine(Environment.CurrentDirectory + "Cache", "npbind.dat")))
            {

                // Get NPWR id
                string NPWR = string.Empty;
                using (var WindowsCMD = new Process())
                {
                    WindowsCMD.StartInfo.FileName = OperatingSystem.IsWindows() ? "cmd" : "/bin/sh";
                    WindowsCMD.StartInfo.Arguments = OperatingSystem.IsWindows() ? "/c strings /accepteula -nobanner \"" + Path.Combine(Environment.CurrentDirectory + "Cache", "npbind.dat") + "\" | findstr NPWR" : $"-c strings \"" + Path.Combine(Environment.CurrentDirectory + "Cache", "npbind.dat") + "\"  | grep -F \"NPWR\"";
                    WindowsCMD.StartInfo.RedirectStandardOutput = true;
                    WindowsCMD.StartInfo.UseShellExecute = false;
                    WindowsCMD.StartInfo.CreateNoWindow = true;
                    WindowsCMD.Start();

                    var OutputReader = WindowsCMD.StandardOutput;
                    string[] ProcessOutput = OutputReader.ReadToEnd().Split([""], StringSplitOptions.RemoveEmptyEntries);

                    await WindowsCMD.WaitForExitAsync();
                    WindowsCMD.Close();

                    if (ProcessOutput.Length > -1)
                    {
                        NPWR = ProcessOutput[0].Trim();
                    }
                }

                // Get uds & trophy ucp files
                if (!string.IsNullOrEmpty(NPWR))
                {

                    // Check if folder exists for uds and get the file
                    foreach (FtpListItem DirectoryInFTP in await NewFtpClient.GetListing("/user/np_uds/nobackup/conf/"))
                    {
                        if (DirectoryInFTP.Name == NPWR)
                        {
                            await NewFtpClient.DownloadFile(Path.Combine(SelectedPath, "sce_sys", "uds", "uds.ucp"), DirectoryInFTP.FullName + "/uds.ucp");
                            break;
                        }
                    }

                    if (File.Exists(Path.Combine(SelectedPath, "sce_sys", "uds", "uds.ucp")))
                    {
                        if (!File.Exists(Path.Combine(SelectedPath, "sce_sys", "uds", "uds00.ucp")))
                        {
                            File.Move(Path.Combine(SelectedPath, "sce_sys", "uds", "uds.ucp"), Path.Combine(SelectedPath, "sce_sys", "uds", "uds00.ucp"));
                        }
                    }

                    // Check if folder exists for trophy and get the file
                    foreach (FtpListItem DirectoryInFTP in await NewFtpClient.GetListing("/user/trophy2/nobackup/conf/"))
                    {
                        if (DirectoryInFTP.Name == NPWR)
                        {
                            await NewFtpClient.DownloadFile(Path.Combine(SelectedPath, "sce_sys", "trophy2"), DirectoryInFTP.FullName + "/TROPHY.UCP");
                            break;
                        }
                    }

                    if (File.Exists(Path.Combine(SelectedPath, "sce_sys", "trophy2", "TROPHY.UCP")))
                    {
                        if (!File.Exists(Path.Combine(SelectedPath, "sce_sys", "trophy2", "TROPHY.UCP")))
                        {
                            File.Move(Path.Combine(SelectedPath, "sce_sys", "trophy2", "TROPHY.UCP"), Path.Combine(SelectedPath, "sce_sys", "trophy2", "trophy00.UCP"));
                        }
                    }

                }

            }

            // Get the appmeta
            if (!string.IsNullOrEmpty(AppMetadataRemoteFolder))
            {
                // Download progress
                var DLProgress = new Progress<FtpProgress>(async p =>
                {
                    if (p.Progress == 100)
                    {
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {

                            });
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {

                        });
                    }
                });

                await NewFtpClient.DownloadDirectory(Path.Combine(SelectedPath, "sce_sys"), AppMetadataRemoteFolder, FtpFolderSyncMode.Update, FtpLocalExists.Overwrite, FtpVerify.None, null, DLProgress);
            }
            if (!string.IsNullOrEmpty(SystemAppMetadataRemoteFolder))
            {
                // Download progress
                var DLProgress = new Progress<FtpProgress>(async p =>
                {
                    if (p.Progress == 100)
                    {
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {

                            });
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {

                        });
                    }
                });

                await NewFtpClient.DownloadDirectory(Path.Combine(SelectedPath, "sce_sys"), SystemAppMetadataRemoteFolder, FtpFolderSyncMode.Update, FtpLocalExists.Overwrite, FtpVerify.None, null, DLProgress);
            }

            // Disonnect
            await NewFtpClient.Disconnect();
        });
    }

    public async Task GetAppData()
    {
        await Task.Run(async () =>
        {
            try
            {
                using var NewFtpClient = new AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsolePort), NewFtpConfig);

                // Connect
                await NewFtpClient.Connect();

                // Download files and folders
                if (!string.IsNullOrEmpty(App0RemoteFolder))
                {
                    // Download progress
                    var DLProgress = new Progress<FtpProgress>(async p =>
                    {
                        if (p.Progress == 100d)
                        {
                            if (Dispatcher.UIThread.CheckAccess() == false)
                            {
                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    ReceiveProgressBar.Value = 0;
                                    Cursor = new Cursor(StandardCursorType.Arrow);
                                });
                            }
                            else
                            {
                                ReceiveProgressBar.Value = 0;
                                Cursor = new Cursor(StandardCursorType.Arrow);
                            }
                        }
                        else
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                ReceiveStatusTextBlock.Text = p.RemotePath + " copied. " + CopiedFiles.ToString() + "/" + TotalFiles.ToString();
                                ReceiveProgressBar.Value += 1;
                            });
                        }
                    });

                    await NewFtpClient.DownloadDirectory(SelectedPath, App0RemoteFolder, FtpFolderSyncMode.Update, FtpLocalExists.Overwrite, FtpVerify.None, null, DLProgress);
                }
                if (!string.IsNullOrEmpty(DiscRemoteFolder))
                {
                    // Download progress
                    var DLProgress = new Progress<FtpProgress>(p =>
                    {
                        if (p.Progress == 100)
                        {
                            if (Dispatcher.UIThread.CheckAccess() == false)
                            {
                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    ReceiveProgressBar.Value = 0;
                                    Cursor = new Cursor(StandardCursorType.Arrow);
                                });
                            }
                            else
                            {
                                ReceiveProgressBar.Value = 0;
                                Cursor = new Cursor(StandardCursorType.Arrow);
                            }
                        }
                        else
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                ReceiveStatusTextBlock.Text = p.RemotePath + " copied. " + CopiedFiles.ToString() + "/" + TotalFiles.ToString();
                                ReceiveProgressBar.Value += 1;
                            });
                        }
                    });

                    await NewFtpClient.DownloadDirectory(SelectedPath, DiscRemoteFolder, FtpFolderSyncMode.Update, FtpLocalExists.Overwrite, FtpVerify.None, null, DLProgress);
                }
                if (!string.IsNullOrEmpty(CustomRemoteFolder))
                {
                    // Download progress
                    var DLProgress = new Progress<FtpProgress>(p =>
                    {
                        if (p.Progress == 100)
                        {
                            if (Dispatcher.UIThread.CheckAccess() == false)
                            {
                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    ReceiveProgressBar.Value = 0;
                                    Cursor = new Cursor(StandardCursorType.Arrow);
                                });
                            }
                            else
                            {
                                ReceiveProgressBar.Value = 0;
                                Cursor = new Cursor(StandardCursorType.Arrow);
                            }
                        }
                        else
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                ReceiveStatusTextBlock.Text = p.RemotePath + " copied. " + CopiedFiles.ToString() + "/" + TotalFiles.ToString();
                                ReceiveProgressBar.Value += 1;
                            });
                        }
                    });

                    await NewFtpClient.DownloadDirectory(SelectedPath, CustomRemoteFolder, FtpFolderSyncMode.Update, FtpLocalExists.Overwrite, FtpVerify.None, null, DLProgress);
                }

                // Proceed with dumping metadata if full dump is selected
                if (FullDumpCheckBox.IsChecked == true)
                {

                    Cursor = new Cursor(StandardCursorType.Wait);

                    if (await GetAppMetadata() == true)
                    {

                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                ReceiveProgressBar.Value = 0;
                                ReceiveStatusTextBlock.Text = "Getting metadata, please wait ... 0/" + TotalFiles.ToString();
                                ReceiveProgressBar.Maximum = TotalFiles;
                            });
                        }
                        else
                        {
                            ReceiveProgressBar.Value = 0;
                            ReceiveStatusTextBlock.Text = "Getting metadata, please wait ... 0/" + TotalFiles.ToString();
                            ReceiveProgressBar.Maximum = TotalFiles;
                        }

                        await GetMetadata();
                    }
                    else
                    {
                        Cursor = new Cursor(StandardCursorType.Arrow);
                        var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find any metadata.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                    }
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Success", "Dump completed.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                    LockUI();
                }

                // Disonnect
                await NewFtpClient.Disconnect();
            }

            catch (Exception)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error reading data.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        });
    }

    public async Task DumpSELFData()
    {
        await Task.Run(async () =>
        {
            int Read;
            int TotalBytesRead = 0;
            byte[] Buffer = new byte[1024];
            var EndpointIPAddress = IPAddress.Parse(ConsoleIP);
            var NewIPEndPoint = new IPEndPoint(EndpointIPAddress, 9023);

            // Change progress bar
            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() => ReceiveProgressBar.IsIndeterminate = true);
            }
            else
            {
                ReceiveProgressBar.IsIndeterminate = true;
            }

            using var ReceiverSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            ReceiverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            ReceiverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);

            // Connect to the SELF dumper
            ReceiverSocket.Connect(NewIPEndPoint);

            // Create the dump
            using (var NewFileStream = new FileStream(Path.Combine(SelectedPath, "self-dump.tar"), FileMode.Create, FileAccess.ReadWrite))
            {
                do
                {
                    // Receive from socket
                    Read = ReceiverSocket.Receive(Buffer);

                    // Continue if data is present
                    if (Read > 0)
                    {

                        // Write to file & update TotalBytesRead
                        NewFileStream.Write(Buffer, 0, Read);
                        TotalBytesRead += Read;

                        // Update the status text
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() => ReceiveStatusTextBlock.Text = "Creating self-dump.tar. " + Utils.HumanReadableBytes(TotalBytesRead) + " received.");
                        }
                        else
                        {
                            ReceiveStatusTextBlock.Text = "Creating self-dump.tar. " + Utils.HumanReadableBytes(TotalBytesRead) + " received.";
                        }

                    }
                }

                while (Read > 0);
            }

            // Close the connection
            ReceiverSocket.Close();
        });
    }

    private async void BrowseFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog();
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedDirectoryTextBox.Text = FBDResult;
            SelectedPath = FBDResult;
        }
    }

    private void SelectedFolderComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SelectedFolderComboBox.SelectedItem is not null)
        {
            // Make options available if first entry is selected (should be "/mnt/sandbox/pfsmnt/")
            if (SelectedFolderComboBox.SelectedIndex == 0)
            {
                FullDumpCheckBox.IsEnabled = true;
                MetadataDumpCheckBox.IsEnabled = true;
                SELFDumpCheckBox.IsEnabled = true;
            }
            else
            {
                FullDumpCheckBox.IsEnabled = false;
                MetadataDumpCheckBox.IsEnabled = false;
                SELFDumpCheckBox.IsEnabled = false;
            }
        }
    }

    public void LockUI()
    {
        if (SelectedFolderComboBox.IsEnabled)
        {
            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    SelectedFolderComboBox.IsEnabled = false;
                    FullDumpCheckBox.IsEnabled = false;
                    MetadataDumpCheckBox.IsEnabled = false;
                    SelectedDirectoryTextBox.IsEnabled = false;
                    BrowseFolderButton.IsEnabled = false;
                    DownloadButton.IsEnabled = false;
                });
            }
            else
            {
                SelectedFolderComboBox.IsEnabled = false;
                FullDumpCheckBox.IsEnabled = false;
                MetadataDumpCheckBox.IsEnabled = false;
                SelectedDirectoryTextBox.IsEnabled = false;
                BrowseFolderButton.IsEnabled = false;
                DownloadButton.IsEnabled = false;
            }
        }
        else if (Dispatcher.UIThread.CheckAccess() == false)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                SelectedFolderComboBox.IsEnabled = true;
                FullDumpCheckBox.IsEnabled = true;
                MetadataDumpCheckBox.IsEnabled = true;
                SelectedDirectoryTextBox.IsEnabled = true;
                BrowseFolderButton.IsEnabled = true;
                DownloadButton.IsEnabled = true;
            });
        }
        else
        {
            SelectedFolderComboBox.IsEnabled = true;
            FullDumpCheckBox.IsEnabled = true;
            MetadataDumpCheckBox.IsEnabled = true;
            SelectedDirectoryTextBox.IsEnabled = true;
            BrowseFolderButton.IsEnabled = true;
            DownloadButton.IsEnabled = true;
        }
    }

}