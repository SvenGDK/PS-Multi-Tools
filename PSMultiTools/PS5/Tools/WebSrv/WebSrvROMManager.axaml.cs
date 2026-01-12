using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentFTP;
using IronSoftware.Drawing;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.IO;
using System.Security.Authentication;

namespace PSMultiTools.PS5.Tools.WebSrv;

public partial class WebSrvROMManager : Window
{

    private readonly FtpConfig NewFtpConfig = new() { EncryptionMode = FtpEncryptionMode.None, SslProtocols = SslProtocols.None, DataConnectionEncryption = false, ValidateAnyCertificate = true };

    public string PS5IP = "";
    public int PS5PORT = 0;
    public string ROMPath = "";

    private struct ROMListViewItem
    {
        public string RemoteFilePath { get; set; }
    }

    public WebSrvROMManager()
    {
        InitializeComponent();

        Loaded += WebSrvROMManager_Loaded;
        ROMImage.PointerPressed += ROMImage_PointerPressed;
        InstalledROMsListBox.SelectionChanged += InstalledROMsListView_SelectionChanged;
    }

    private void WebSrvROMManager_Loaded(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ROMPath))
        {
            ListROMs();
        }
    }

    private async void ListROMs()
    {
        InstalledROMsListBox.Items.Clear();

        try
        {
            using var NewFtpClient = new AsyncFtpClient(PS5IP, "anonymous", "anonymous", PS5PORT, NewFtpConfig);

            // Connect
            await NewFtpClient.Connect();

            // Enumerate files in the roms directory
            foreach (var FTPItem in await NewFtpClient.GetListing(ROMPath))
            {
                if (!FTPItem.FullName.EndsWith(".jpg") && !FTPItem.FullName.EndsWith(".png"))
                {
                    var NewROMLVItem = new ROMListViewItem() { RemoteFilePath = FTPItem.FullName };
                    InstalledROMsListBox.Items.Add(NewROMLVItem);
                }
            }

            // Disonnect
            await NewFtpClient.Disconnect();
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private void InstalledROMsListView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (InstalledROMsListBox.SelectedItem is not null)
        {
            ROMListViewItem SelectedROMLVItem = (ROMListViewItem)InstalledROMsListBox.SelectedItem;
            GetROMInfo(SelectedROMLVItem.RemoteFilePath);
        }
    }

    private async void GetROMInfo(string SelectedROMFile)
    {
        int LastPos = SelectedROMFile.LastIndexOf("/") + 1;
        string FileName = SelectedROMFile[LastPos..];

        string JPGImageFileName = FileName.Split('.')[0] + ".jpg";
        string PNGImageFileName = FileName.Split('.')[0] + ".png";
        string JPGImageFilePath = SelectedROMFile.Replace(FileName, JPGImageFileName);
        string PNGImageFilePath = SelectedROMFile.Replace(FileName, PNGImageFileName);

        try
        {
            using var NewFtpClient = new AsyncFtpClient(PS5IP, "anonymous", "anonymous", PS5PORT, NewFtpConfig);

            // Connect
            await NewFtpClient.Connect();

            ROMFileNameTextBox.Text = FileName;

            if (await NewFtpClient.FileExists(JPGImageFilePath))
            {
                try
                {
                    // Set homebrew icon
                    var TempBitmapImage = await AnyBitmap.FromUriAsync(new Uri("ftp://" + PS5IP + ":" + PS5PORT.ToString() + JPGImageFilePath));
                    ROMImage.Source = Utils.AnyBitmapToIImage(TempBitmapImage);
                }
                catch (Exception)
                {
                }
            }
            else if (await NewFtpClient.FileExists(PNGImageFilePath))
            {
                try
                {
                    // Set homebrew icon
                    var TempBitmapImage = await AnyBitmap.FromUriAsync(new Uri("ftp://" + PS5IP + ":" + PS5PORT.ToString() + PNGImageFilePath));
                    ROMImage.Source = Utils.AnyBitmapToIImage(TempBitmapImage);
                }
                catch (Exception)
                {
                }
            }
            else
            {
                ROMImage.Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/nothing.png")));
            }

            // Disonnect
            await NewFtpClient.Disconnect();
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async void AddROMsButton_Click(object? sender, RoutedEventArgs e)
    {
        var OFD = new OpenFileDialog() { Title = "Select your ROM files", AllowMultiple = true };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            try
            {
                using var NewFtpClient = new AsyncFtpClient(PS5IP, "anonymous", "anonymous", PS5PORT, NewFtpConfig);

                // Connect
                await NewFtpClient.Connect();

                // Upload the selected file(s)
                if (OFDResult.Length > 1)
                {
                    await NewFtpClient.UploadFiles(OFDResult, ROMPath, FtpRemoteExists.OverwriteInPlace, false, FtpVerify.None, FtpError.None);

                    var box = MessageBoxManager.GetMessageBoxStandard("Success", "Files uploaded with success!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                else
                {
                    await NewFtpClient.UploadFile(OFDResult[0], ROMPath, FtpRemoteExists.OverwriteInPlace, false, FtpVerify.None);

                    var box = MessageBoxManager.GetMessageBoxStandard("Success", "File uploaded with success!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }

                // Disonnect
                await NewFtpClient.Disconnect();
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }

            // Refresh
            ListROMs();
        }
    }

    private async void RemoveROMButton_Click(object? sender, RoutedEventArgs e)
    {
        if (InstalledROMsListBox.SelectedItem is not null)
        {
            ROMListViewItem SelectedROMFile = (ROMListViewItem)InstalledROMsListBox.SelectedItem;

            var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Do you really want to delete the selected ROM from the WebSrv ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box.ShowWindowDialogAsync(this);
            if (boxresult == ButtonResult.Yes)
            {

                try
                {
                    using var NewFtpClient = new AsyncFtpClient(PS5IP, "anonymous", "anonymous", PS5PORT, NewFtpConfig);

                    // Connect
                    await NewFtpClient.Connect();

                    // Remove
                    await NewFtpClient.DeleteFile(SelectedROMFile.RemoteFilePath);

                    // Disonnect
                    await NewFtpClient.Disconnect();
                }
                catch (Exception ex)
                {
                    var box2 = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box2.ShowWindowAsync();
                }

                var box3 = MessageBoxManager.GetMessageBoxStandard("Success", "ROM removed from the WebSrv.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box3.ShowWindowAsync();

                // Refresh
                ListROMs();
            }

        }
    }

    private async void ROMImage_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (InstalledROMsListBox.SelectedItem is not null && !string.IsNullOrEmpty(ROMFileNameTextBox.Text))
        {
            ROMListViewItem SelectedROMLVItem = (ROMListViewItem)InstalledROMsListBox.SelectedItem;

            var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Do you want to replace the icon for the selected ROM file ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box.ShowWindowDialogAsync(this);
            if (boxresult == ButtonResult.Yes)
            {

                string ROMFileNameWithoutExtension = Path.GetFileNameWithoutExtension(ROMFileNameTextBox.Text);

                var pngFileFilter = new FileDialogFilter
                {
                    Name = "PNG Image File",
                    Extensions = ["png"]
                };
                var OFD = new OpenFileDialog() { Title = "Select a new PNG file.", Filters = { pngFileFilter }, AllowMultiple = false };
                var OFDResult = await OFD.ShowAsync(this);

                if (OFDResult != null && OFDResult.Length > 0)
                {

                    string NewIconFileName = Path.GetFileName(OFDResult[0]);
                    string NewIconFileNameExtension = Path.GetExtension(OFDResult[0]);

                    try
                    {
                        using var NewFtpClient = new AsyncFtpClient(PS5IP, "anonymous", "anonymous", PS5PORT, NewFtpConfig);

                        // Connect
                        await NewFtpClient.Connect();

                        // Put the file in the roms directory
                        await NewFtpClient.UploadFile(OFDResult[0], ROMPath, FtpRemoteExists.OverwriteInPlace, false, FtpVerify.None);

                        // Check if the file name matches the ROM file name
                        if (!await NewFtpClient.FileExists(ROMPath + ROMFileNameWithoutExtension + NewIconFileNameExtension))
                        {
                            // If not then rename the file to the ROM's file name
                            await NewFtpClient.MoveFile(ROMPath + NewIconFileName, ROMPath + ROMFileNameWithoutExtension + NewIconFileNameExtension, FtpRemoteExists.OverwriteInPlace);
                        }

                        // Disonnect
                        await NewFtpClient.Disconnect();

                        var box2 = MessageBoxManager.GetMessageBoxStandard("Success", "Icon replaced!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box2.ShowWindowAsync();
                    }
                    catch (Exception ex)
                    {
                        var box2 = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box2.ShowWindowAsync();
                    }

                    // Refresh icon for selected ROM
                    GetROMInfo(SelectedROMLVItem.RemoteFilePath);
                }

            }

        }
    }

}