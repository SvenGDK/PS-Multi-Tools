using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using FluentFTP;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using PSMultiTools.Dialogs;
using System;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using static PSMultiTools.Classes.Structures;

namespace PSMultiTools.MultiPlatformTools;

public partial class FTPBrowser : Window
{
    public string ConsoleIP = "";
    public string ConsoleFTPPort = "";
    public string CurrentPath = "";

    public bool IsConnected = false;
    public FtpConfig NewFtpConfig = new() { EncryptionMode = FtpEncryptionMode.None, SslProtocols = SslProtocols.None, DataConnectionEncryption = false, ValidateAnyCertificate = true };

    public Point DragStartPoint;
    bool IsDragging;
    int MousePointerID;

    public MenuItem DownloadMenuItem = new() { Header = "Download selected file or folder", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/download.png"))) } };
    public MenuItem UploadFileMenuItem = new() { Header = "Upload a file", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/upload.png"))) } };
    public MenuItem UploadFolderMenuItem = new() { Header = "Upload a folder", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/upload.png"))) } };
    public MenuItem DeleteMenuItem = new() { Header = "Delete selected file or folder", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/delete.png"))) } };
    public MenuItem RenameMenuItem = new() { Header = "Rename selected file or folder", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/rename.png"))) } };
    public MenuItem NewDirectoryMenuItem = new() { Header = "Create a new directory", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/new-folder.png"))) } };

    public FTPBrowser()
    {
        InitializeComponent();

        Loaded += FTPBrowser_Loaded;

        RenameMenuItem.Click += RenameMenuItem_Click;
        DownloadMenuItem.Click += DownloadMenuItem_Click;
        UploadFileMenuItem.Click += UploadFileMenuItem_Click;
        UploadFolderMenuItem.Click += UploadFolderMenuItem_Click;
        NewDirectoryMenuItem.Click += NewDirectoryMenuItem_Click;
        DeleteMenuItem.Click += DeleteMenuItem_Click;

        LocalTreeView.PointerPressed += LocalTreeView_PointerPressed;
        LocalTreeView.PointerMoved += LocalTreeView_PointerMoved;
        LocalTreeView.PointerReleased += LocalTreeView_PointerReleased;

        FTPItemsListBox.PointerPressed += FTPItemsListBox_PointerPressed;
    }

    private void FTPBrowser_Loaded(object? sender, RoutedEventArgs e)
    {
        // Set up context menu
        var FTPContentListViewContextMenu = new ContextMenu();
        FTPContentListViewContextMenu.Items.Add(DownloadMenuItem);
        FTPContentListViewContextMenu.Items.Add(UploadFileMenuItem);
        FTPContentListViewContextMenu.Items.Add(UploadFolderMenuItem);
        FTPContentListViewContextMenu.Items.Add(DeleteMenuItem);
        FTPContentListViewContextMenu.Items.Add(RenameMenuItem);
        FTPContentListViewContextMenu.Items.Add(NewDirectoryMenuItem);
        FTPItemsListBox.ContextMenu = FTPContentListViewContextMenu;

        // Set IP & Port
        if (!string.IsNullOrEmpty(ConsoleIP))
        {
            ConsoleIPTextBox.Text = ConsoleIP;
        }
        if (!string.IsNullOrEmpty(ConsoleFTPPort))
        {
            PortTextBox.Text = ConsoleFTPPort;
        }

        // Set up local files explorer
        foreach (var FoundDrive in DriveInfo.GetDrives())
        {
            if (FoundDrive.IsReady)
            {
                var DriveItem = new TreeViewItem() { Header = FoundDrive.Name, Tag = FoundDrive.RootDirectory.FullName };
                DriveItem.Items.Add(null);
                DriveItem.Expanded += FolderExpanded;
                LocalTreeView.Items.Add(DriveItem);
            }
        }
    }

    private async void ConnectButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ConsoleIPTextBox.Text) && !string.IsNullOrEmpty(PortTextBox.Text))
        {
            // Set IP & Port if still missing
            if (string.IsNullOrEmpty(ConsoleIP))
            {
                ConsoleIP = ConsoleIPTextBox.Text;
            }
            if (string.IsNullOrEmpty(ConsoleFTPPort))
            {
                ConsoleFTPPort = PortTextBox.Text;
            }

            // Connect & list
            if (ConnectButton.Content!.ToString() == "Connect and list content")
            {
                if (await ListDirectoryContent("/"))
                {
                    IsConnected = true;
                    ConnectButton.Content = "Disconnect";
                    CurrentPath = "/";
                    CurrentDirTextBlock.Text = "Current directory : " + CurrentPath;
                }
            }
            else
            {
                IsConnected = false;
                FTPItemsListBox.Items.Clear();
                ConnectButton.Content = "Connect and list content";
                FTPStatusTextBlock.Text = "";
                CurrentPath = "/";
                CurrentDirTextBlock.Text = "Current directory : " + CurrentPath;
            }
        }
    }

    #region FTP Uploader

    private async Task<bool> UploadFileAsync(string LocalFilePath, string RemoteDestinationPath)
    {
        try
        {
            using var NewFtpClient = new AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsoleFTPPort), NewFtpConfig);
            // Connect
            await NewFtpClient.Connect();

            // Upload progress
            var UPProgress = new Progress<FtpProgress>(p =>
            {
                if (p.Progress == 100d)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        FTPTransferProgressBar.Value = 0d;
                        FTPStatusTextBlock.Text = "Uploading finished";
                    });
                }
                else
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        FTPTransferProgressBar.Value = p.Progress;
                        FTPStatusTextBlock.Text = "Uploading - " + p.Progress.ToString("F0") + "%";
                    });
                }
            });

            await NewFtpClient.UploadFile(LocalFilePath, RemoteDestinationPath, FtpRemoteExists.OverwriteInPlace, false, FtpVerify.None, UPProgress);

            // Disconnect
            await NewFtpClient.Disconnect();
            return true;
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not upload the selected file." + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return false;
        }
    }

    private async Task<bool> UploadFolderAsync(string LocalDirectoryPath, string RemoteDestinationPath)
    {
        try
        {
            using var NewFtpClient = new AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsoleFTPPort), NewFtpConfig);
            // Connect
            await NewFtpClient.Connect();

            // Upload progress
            var UPProgress = new Progress<FtpProgress>(p =>
            {
                if (p.Progress == 100d)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        FTPTransferProgressBar.Value = 0d;
                        FTPStatusTextBlock.Text = "Uploading finished";
                    });
                }
                else
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        FTPTransferProgressBar.Value = p.Progress;
                        FTPStatusTextBlock.Text = "Uploading - " + p.Progress.ToString("F0") + "%";
                    });
                }
            });

            await NewFtpClient.UploadDirectory(LocalDirectoryPath, RemoteDestinationPath, FtpFolderSyncMode.Update, FtpRemoteExists.OverwriteInPlace, FtpVerify.None, null, UPProgress);

            // Disconnect
            await NewFtpClient.Disconnect();
            return true;
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not upload the selected folder." + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return false;
        }
    }

    #endregion

    #region FTP Downloader

    private async Task<bool> DownloadFileAsync(string LocalFilePath, string RemoteFilePath)
    {
        try
        {
            using var NewFtpClient = new AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsoleFTPPort), NewFtpConfig);
            // Connect
            await NewFtpClient.Connect();

            // Download progress
            var DLProgress = new Progress<FtpProgress>(p =>
            {
                if (p.Progress == 1d)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        FTPTransferProgressBar.Value = 0d;
                        FTPStatusTextBlock.Text = "Download finished";
                    });
                }
                else
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        FTPTransferProgressBar.Value = p.Progress * 100d;
                        FTPStatusTextBlock.Text = "Downloading - " + (p.Progress * 100d).ToString("F0") + "%";
                    });
                }
            });

            await NewFtpClient.DownloadFile(LocalFilePath, RemoteFilePath, FtpLocalExists.Overwrite, FtpVerify.None, DLProgress);

            // Disconnect
            await NewFtpClient.Disconnect();
            return true;
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file." + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return false;
        }
    }

    private async Task<bool> DownloadFolderAsync(string LocalDirectoryPath, string RemoteDirectoryPath)
    {
        try
        {
            using var NewFtpClient = new AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsoleFTPPort), NewFtpConfig);
            // Connect
            await NewFtpClient.Connect();

            // Download progress
            var DLProgress = new Progress<FtpProgress>(p =>
            {
                if (p.Progress == 100d)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        FTPTransferProgressBar.Value = 0d;
                        FTPStatusTextBlock.Text = "Download finished";
                    });
                }
                else
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        FTPTransferProgressBar.Value = p.Progress;
                        FTPStatusTextBlock.Text = "Downloading - " + p.Progress.ToString("F0") + "%";
                    });
                }
            });

            await NewFtpClient.DownloadDirectory(LocalDirectoryPath, RemoteDirectoryPath, FtpFolderSyncMode.Update, FtpLocalExists.Overwrite, FtpVerify.None, null, DLProgress);

            // Disconnect
            await NewFtpClient.Disconnect();
            return true;
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected folder." + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return false;
        }
    }

    #endregion

    #region Context Menu Actions

    private async void RenameMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (FTPItemsListBox.SelectedItem is not null)
        {
            FTPListViewItem SelectedFTPLVItem = (FTPListViewItem)FTPItemsListBox.SelectedItem;

            if (!(SelectedFTPLVItem.FileOrDirName == ".."))
            {
                var NewInputDialog = new InputDialog() { Title = "FTP Browser" };
                NewInputDialog.InputDialogTitleTextBlock.Text = "Enter a new name:";
                NewInputDialog.NewValueTextBox.Text = SelectedFTPLVItem.FileOrDirName;

                var NewName = await NewInputDialog.ShowDialog<string>(this);
                if (NewName != null)
                {
                    if (SelectedFTPLVItem.FileOrDirType == "Folder")
                    {
                        if (CurrentPath == "/")
                        {
                            await RenameContent(CurrentPath + SelectedFTPLVItem.FileOrDirName, CurrentPath + NewName, false);
                        }
                        else
                        {
                            await RenameContent(CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName, CurrentPath + "/" + NewName, false);
                        }
                    }
                    else if (CurrentPath == "/")
                    {
                        await RenameContent(CurrentPath + SelectedFTPLVItem.FileOrDirName, CurrentPath + NewName, true);
                    }
                    else
                    {
                        await RenameContent(CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName, CurrentPath + "/" + NewName, true);
                    }

                    await ListDirectoryContent(CurrentPath + "/");
                }
            }
        }
    }

    private async void DownloadMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (FTPItemsListBox.SelectedItem is not null)
        {
            FTPListViewItem SelectedFTPLVItem = (FTPListViewItem)FTPItemsListBox.SelectedItem;
            var FBD = new OpenFolderDialog() { Title = "Select a folder where the file/folder should be downloaded." };
            var FBDResult = await FBD.ShowAsync(this);

            if (FBDResult != null)
            {
                string DestinationPath = FBDResult + @"\" + SelectedFTPLVItem.FileOrDirName;
                if (CurrentPath == "/")
                {
                    if (SelectedFTPLVItem.FileOrDirType == "Folder")
                    {
                        await DownloadFolderAsync(DestinationPath, CurrentPath + SelectedFTPLVItem.FileOrDirName);
                    }
                    else
                    {
                        await DownloadFileAsync(DestinationPath, CurrentPath + SelectedFTPLVItem.FileOrDirName);
                    }
                }
                else if (SelectedFTPLVItem.FileOrDirType == "Folder")
                {
                    await DownloadFolderAsync(DestinationPath, CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName);
                }
                else
                {
                    await DownloadFileAsync(DestinationPath, CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName);
                }
            }
        }
    }

    private async void UploadFileMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (IsConnected)
        {
            var OFD = new OpenFileDialog() { AllowMultiple = true };
            var OFDResult = await OFD.ShowAsync(this);
            if (OFDResult != null && OFDResult.Length > 0)
            {

                LockUI();

                string DestinationPath;
                if (OFDResult.Length > 1)
                {
                    foreach (var SelectedItem in OFDResult)
                    {
                        string FileName = Path.GetFileName(SelectedItem);

                        if (CurrentPath.EndsWith('/'))
                        {
                            DestinationPath = CurrentPath + FileName;
                        }
                        else
                        {
                            DestinationPath = CurrentPath + "/" + FileName;
                        }

                        await UploadFileAsync(SelectedItem, DestinationPath);
                    }
                }
                else
                {
                    if (CurrentPath.EndsWith('/'))
                    {
                        DestinationPath = CurrentPath + Path.GetFileName(OFDResult[0]);
                    }
                    else
                    {
                        DestinationPath = CurrentPath + "/" + Path.GetFileName(OFDResult[0]);
                    }

                    await UploadFileAsync(OFDResult[0], DestinationPath);
                }

                await ListDirectoryContent(CurrentPath + "/");

                LockUI();
            }
        }
    }

    private async void UploadFolderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (IsConnected)
        {
            var FBD = new OpenFolderDialog();
            var FBDResult = await FBD.ShowAsync(this);

            if (FBDResult != null)
            {

                LockUI();

                string DestinationPath;
                string FolderName = Path.GetFileName(FBDResult);

                if (CurrentPath.EndsWith('/'))
                {
                    DestinationPath = CurrentPath + FolderName;
                }
                else
                {
                    DestinationPath = CurrentPath + "/" + FolderName;
                }

                await UploadFolderAsync(FBDResult, DestinationPath);
                await ListDirectoryContent(CurrentPath + "/");

                LockUI();
            }
        }
    }

    private async void NewDirectoryMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (IsConnected && FTPItemsListBox.SelectedItem != null)
        {
            FTPListViewItem SelectedFTPLVItem = (FTPListViewItem)FTPItemsListBox.SelectedItem;

            if (!(SelectedFTPLVItem.FileOrDirName == ".."))
            {
                var NewInputDialog = new InputDialog() { Title = "FTP Browser" };
                NewInputDialog.InputDialogTitleTextBlock.Text = "New folder name:";
                var NewFolderName = await NewInputDialog.ShowDialog<string>(this);

                if (NewFolderName != null)
                {
                    {
                        if (CurrentPath == "/")
                        {
                            await CreateDirectory(CurrentPath + NewFolderName);
                        }
                        else
                        {
                            await CreateDirectory(CurrentPath + "/" + NewFolderName);
                        }

                        await ListDirectoryContent(CurrentPath + "/");
                    }
                }
            }
        }
    }

    private async void DeleteMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (FTPItemsListBox.SelectedItem is not null)
        {
            FTPListViewItem SelectedFTPLVItem = (FTPListViewItem)FTPItemsListBox.SelectedItem;

            if (SelectedFTPLVItem.FileOrDirType == "Folder")
            {
                if (CurrentPath == "/")
                {
                    await DeleteContent(CurrentPath + SelectedFTPLVItem.FileOrDirName, false);
                }
                else
                {
                    await DeleteContent(CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName, false);
                }

                await ListDirectoryContent(CurrentPath + "/");
            }
            else
            {
                if (CurrentPath == "/")
                {
                    await DeleteContent(CurrentPath + SelectedFTPLVItem.FileOrDirName, true);
                }
                else
                {
                    await DeleteContent(CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName, true);
                }

                await ListDirectoryContent(CurrentPath + "/");
            }

        }
    }

    #endregion

    #region General FTP Functions

    private async Task<bool> ListDirectoryContent(string DirectoryPath)
    {
        // Clear list
        FTPItemsListBox.Items.Clear();

        try
        {

            if (PS3ModeCheckBox.IsChecked == true)
            {
                NewFtpConfig.DataConnectionType = FtpDataConnectionType.PASV;
            }

            using var NewFtpClient = new AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsoleFTPPort), NewFtpConfig);

            // Connect
            await NewFtpClient.Connect();
            FtpListItem[] DirectoryListing = await NewFtpClient.GetListing(DirectoryPath);

            // Add a return option
            var ReturnFTPLVItem = new FTPListViewItem() { FileOrDirName = "..", FileOrDirType = "Folder" };
            FTPItemsListBox.Items.Add(ReturnFTPLVItem);

            // List directory
            foreach (var FTPItem in DirectoryListing)
            {
                var NewFTPLVItem = new FTPListViewItem();

                switch (FTPItem.Type)
                {
                    case FtpObjectType.Directory:
                        {
                            NewFTPLVItem.FileOrDirType = "Folder";
                            break;
                        }
                    case FtpObjectType.File:
                        {
                            NewFTPLVItem.FileOrDirType = "File";
                            break;
                        }
                    case FtpObjectType.Link:
                        {
                            NewFTPLVItem.FileOrDirType = "Link";
                            break;
                        }
                }

                NewFTPLVItem.FileOrDirName = FTPItem.Name;
                NewFTPLVItem.FileOrDirSize = Utils.HumanReadableBytes(FTPItem.Size);
                NewFTPLVItem.FileOrDirPermissions = FTPItem.RawPermissions;
                NewFTPLVItem.FileOrDirOwner = FTPItem.RawOwner;

                if (!(NewFTPLVItem.FileOrDirName == "."))
                {
                    FTPItemsListBox.Items.Add(NewFTPLVItem);
                }
            }

            // Disonnect
            await NewFtpClient.Disconnect();
            return true;
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not list the remote content." + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return false;
        }
    }

    private async Task<bool> RenameContent(string RemotePath, string RenameTo, bool IsFile)
    {
        try
        {
            using var NewFtpClient = new AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsoleFTPPort), NewFtpConfig);
            // Connect
            await NewFtpClient.Connect();

            if (IsFile == true)
            {
                // Rename the file
                await NewFtpClient.MoveFile(RemotePath, RenameTo, FtpRemoteExists.NoCheck);
            }
            else
            {
                // Rename the directory
                await NewFtpClient.MoveDirectory(RemotePath, RenameTo, FtpRemoteExists.NoCheck);
            }

            // Disconnect
            await NewFtpClient.Disconnect();
            return true;
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not rename the selected file or folder." + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return false;
        }
    }

    private async Task<bool> CreateDirectory(string DirectoryName)
    {
        try
        {
            using var NewFtpClient = new AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsoleFTPPort), NewFtpConfig);
            // Connect
            await NewFtpClient.Connect();

            // Create the directory
            if (await NewFtpClient.CreateDirectory(DirectoryName, true) == true)
            {
                FTPStatusTextBlock.Text = DirectoryName + " created.";
            }
            else
            {
                FTPStatusTextBlock.Text = "Could not create the directory " + DirectoryName;
            }

            // Disconnect
            await NewFtpClient.Disconnect();
            return true;
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not create the directory " + DirectoryName + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return false;
        }
    }

    private async Task<bool> DeleteContent(string FileOrDirectoryName, bool IsFile)
    {
        try
        {
            using var NewFtpClient = new AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", Convert.ToInt32(ConsoleFTPPort), NewFtpConfig);

            // Connect
            await NewFtpClient.Connect();

            if (IsFile == true)
            {
                // Delete the file
                await NewFtpClient.DeleteFile(FileOrDirectoryName);
            }
            else
            {
                // Delete the directory
                await NewFtpClient.DeleteDirectory(FileOrDirectoryName);
            }

            // Disconnect
            await NewFtpClient.Disconnect();
            return true;
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not delete the selected file or folder." + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return false;
        }
    }

    #endregion

    #region Drag & Drop

    private void FTPItemsListView_DragEnter(object? sender, DragEventArgs e)
    {
        var formats = e.Data.GetDataFormats();
        if (formats.Contains(DataFormat.File.Identifier) || formats.Contains("TreeViewItem"))
        {
            e.DragEffects = DragDropEffects.Copy;
            e.Handled = true;
        }
        else
        {
            e.DragEffects |= DragDropEffects.None;
            e.Handled = true;
        }
    }

    private async void FTPItemsListView_Drop(object? sender, DragEventArgs e)
    {

        var formats = e.Data.GetDataFormats();
        if (formats.Contains(DataFormat.File.Identifier))
        {
            string[] DroppedFilesOrFolders = (string[])e.Data.Get(DataFormats.Files)!;

            LockUI();

            if (DroppedFilesOrFolders.Length > 1)
            {
                // Mulitple files/folders
                foreach (string DroppedFileOrFolder in DroppedFilesOrFolders)
                {
                    var DroppedAttr = File.GetAttributes(DroppedFileOrFolder);
                    if ((DroppedAttr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        string FolderName = Path.GetFileName(DroppedFileOrFolder);
                        string DestinationPath;

                        if (CurrentPath.EndsWith('/'))
                        {
                            DestinationPath = CurrentPath + FolderName;
                        }
                        else
                        {
                            DestinationPath = CurrentPath + "/" + FolderName;
                        }

                        await UploadFolderAsync(DroppedFileOrFolder, DestinationPath);
                    }
                    else
                    {
                        string FileName = Path.GetFileName(DroppedFileOrFolder);
                        string DestinationPath;

                        if (CurrentPath.EndsWith('/'))
                        {
                            DestinationPath = CurrentPath + FileName;
                        }
                        else
                        {
                            DestinationPath = CurrentPath + "/" + FileName;
                        }

                        await UploadFileAsync(DroppedFileOrFolder, DestinationPath);
                    }
                }
            }
            else
            {
                // Single file/folder
                var DroppedAttr = File.GetAttributes(DroppedFilesOrFolders[0]);
                if ((DroppedAttr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    string FolderName = Path.GetFileName(DroppedFilesOrFolders[0]);
                    string DestinationPath;

                    if (CurrentPath.EndsWith('/'))
                    {
                        DestinationPath = CurrentPath + FolderName;
                    }
                    else
                    {
                        DestinationPath = CurrentPath + "/" + FolderName;
                    }

                    await UploadFolderAsync(DroppedFilesOrFolders[0], DestinationPath);
                }
                else
                {
                    string FileName = Path.GetFileName(DroppedFilesOrFolders[0]);
                    string DestinationPath;

                    if (CurrentPath.EndsWith('/'))
                    {
                        DestinationPath = CurrentPath + FileName;
                    }
                    else
                    {
                        DestinationPath = CurrentPath + "/" + FileName;
                    }

                    await UploadFileAsync(DroppedFilesOrFolders[0], DestinationPath);
                }
            }

            // Reload current directory and unlock UI
            await ListDirectoryContent(CurrentPath + "/");
            LockUI();
        }
        else if (formats.Contains("TreeViewItem"))
        {
            TreeViewItem DroppedTreeViewItem = (TreeViewItem)e.Data.Get("TreeViewItem")!;
            string DroppedFileOrFolderName = DroppedTreeViewItem.Header!.ToString()!;
            string DroppedFileOrFolderPath = DroppedTreeViewItem.Tag!.ToString()!;
            var DroppedAttr = File.GetAttributes(DroppedFileOrFolderPath);

            string DestinationPath;
            if (CurrentPath.EndsWith('/'))
            {
                DestinationPath = CurrentPath + DroppedFileOrFolderName;
            }
            else
            {
                DestinationPath = CurrentPath + "/" + DroppedFileOrFolderName;
            }

            LockUI();

            if ((DroppedAttr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                await UploadFolderAsync(DroppedFileOrFolderPath, DestinationPath);
            }
            else
            {
                await UploadFileAsync(DroppedFileOrFolderPath, DestinationPath);
            }

            // Reload current directory and unlock UI
            await ListDirectoryContent(CurrentPath + "/");
            LockUI();
        }
    }

    private void LocalTreeView_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        IsDragging = false;
        MousePointerID = 0;
    }

    private void LocalTreeView_PointerMoved(object? sender, PointerEventArgs e)
    {
        var p = e.GetCurrentPoint((Visual?)(sender as IInputElement));
        if (p.Pointer.Id != MousePointerID) return;
        if (!p.Properties.IsLeftButtonPressed) return;

        var control = sender as IInputElement;
        var top = GetTopLevel(control as Visual);
        var tapSize = top?.PlatformSettings!.GetTapSize(PointerType.Mouse) ?? new Size(4, 4);

        if (!IsDragging && (Math.Abs(p.Position.X - DragStartPoint.X) > tapSize.Width || Math.Abs(p.Position.Y - DragStartPoint.Y) > tapSize.Height))
        {
            IsDragging = true;

            if (LocalTreeView.SelectedItem != null)
            {
                TreeViewItem? SelectedTreeViewItem = LocalTreeView.SelectedItem as TreeViewItem;
                if (SelectedTreeViewItem is not null)
                {
                    var NewDataObject = new DataObject();
                    NewDataObject.Set("TreeViewItem", SelectedTreeViewItem);
                    DragDrop.DoDragDrop(e, NewDataObject, DragDropEffects.Copy);
                }
            }
        }
    }

    private async void LocalTreeView_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var p = e.GetCurrentPoint((Visual?)(sender as IInputElement));
        if (p.Properties.IsLeftButtonPressed)
        {
            DragStartPoint = p.Position;
            MousePointerID = p.Pointer.Id;
        }
    }

    #endregion

    #region Local Explorer

    private async void FolderExpanded(object? sender, RoutedEventArgs e)
    {
        if (sender is not TreeViewItem ExpandedItem)
            return;

        if (ExpandedItem.Items.Count == 1 && ExpandedItem.Items[0] is null)
        {
            ExpandedItem.Items.Clear();
            string ExpandedItemFullPath = ExpandedItem.Tag!.ToString()!;

            try
            {
                string[] ExpandedItemDirectories = Directory.GetDirectories(ExpandedItemFullPath);
                foreach (string ExpandedItemDirectory in ExpandedItemDirectories)
                {
                    var NewTreeViewItemSubItem = new TreeViewItem() { Header = Path.GetFileName(ExpandedItemDirectory), Tag = ExpandedItemDirectory };

                    NewTreeViewItemSubItem.Expanded += FolderExpanded;

                    try
                    {
                        bool HasSubDirs = Directory.GetDirectories(ExpandedItemDirectory).Length > 0;
                        bool HasFiles = Directory.GetFiles(ExpandedItemDirectory).Length > 0;
                        if (HasSubDirs || HasFiles)
                        {
                            NewTreeViewItemSubItem.Items.Add(null);
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    ExpandedItem.Items.Add(NewTreeViewItemSubItem);
                }

                string[] ExpandedItemFullPathFiles = Directory.GetFiles(ExpandedItemFullPath);
                foreach (string ExpandedItemFullPathFile in ExpandedItemFullPathFiles)
                {
                    var NewTreeViewItemFileItem = new TreeViewItem() { Header = Path.GetFileName(ExpandedItemFullPathFile), Tag = ExpandedItemFullPathFile };
                    ExpandedItem.Items.Add(NewTreeViewItemFileItem);
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
    }

    #endregion

    private async void FTPItemsListBox_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            if (FTPItemsListBox.SelectedItem is not null)
            {
                FTPListViewItem SelectedFTPLVItem = (FTPListViewItem)FTPItemsListBox.SelectedItem;

                if (SelectedFTPLVItem.FileOrDirType == "Folder")
                {
                    if (SelectedFTPLVItem.FileOrDirName == "..")
                    {
                        if (!(CurrentPath == "/"))
                        {
                            CurrentPath = CurrentPath[..CurrentPath.LastIndexOf('/')];
                            await ListDirectoryContent(CurrentPath);
                        }
                    }
                    else
                    {
                        if (CurrentPath == "/")
                        {
                            CurrentPath = "/" + SelectedFTPLVItem.FileOrDirName;
                        }
                        else
                        {
                            CurrentPath = CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName;
                        }
                        await ListDirectoryContent(CurrentPath);
                    }
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Download Request", $"Download {SelectedFTPLVItem.FileOrDirName} ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult = await box.ShowWindowDialogAsync(this);
                    if (boxresult == ButtonResult.Yes)
                    {
                        var FBD = new OpenFolderDialog() { Title = "Select a folder where the file/folder should be downloaded." };
                        var FBDResult = await FBD.ShowAsync(this);

                        if (FBDResult != null)
                        {
                            string DestinationPath = FBDResult + @"\" + SelectedFTPLVItem.FileOrDirName;
                            if (CurrentPath == "/")
                            {
                                await DownloadFileAsync(DestinationPath, CurrentPath + SelectedFTPLVItem.FileOrDirName);
                            }
                            else
                            {
                                await DownloadFileAsync(DestinationPath, CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName);
                            }
                        }
                    }
                }

                CurrentDirTextBlock.Text = "Current directory : " + CurrentPath;
            }
        }
    }

    private void LockUI()
    {
        if (Dispatcher.UIThread.CheckAccess() == false)
        {
            Dispatcher.UIThread.Invoke(() => { if (ConsoleIPTextBox.IsEnabled) { ConsoleIPTextBox.IsEnabled = false; PortTextBox.IsEnabled = false; ConnectButton.IsEnabled = false; FTPItemsListBox.IsEnabled = false; } else { ConsoleIPTextBox.IsEnabled = true; PortTextBox.IsEnabled = true; ConnectButton.IsEnabled = true; FTPItemsListBox.IsEnabled = true; } });
        }
        else if (ConsoleIPTextBox.IsEnabled)
        {
            ConsoleIPTextBox.IsEnabled = false;
            PortTextBox.IsEnabled = false;
            ConnectButton.IsEnabled = false;
            FTPItemsListBox.IsEnabled = false;
        }
        else
        {
            ConsoleIPTextBox.IsEnabled = true;
            PortTextBox.IsEnabled = true;
            ConnectButton.IsEnabled = true;
            FTPItemsListBox.IsEnabled = true;
        }
    }

}