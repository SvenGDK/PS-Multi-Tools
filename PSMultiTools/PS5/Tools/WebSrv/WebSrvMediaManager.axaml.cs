using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentFTP;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Security.Authentication;

namespace PSMultiTools.PS5.Tools.WebSrv;

public partial class WebSrvMediaManager : Window
{

    private readonly FtpConfig NewFtpConfig = new() { EncryptionMode = FtpEncryptionMode.None, SslProtocols = SslProtocols.None, DataConnectionEncryption = false, ValidateAnyCertificate = true };

    public string MediaPath = "";
    public string PS5IP = "";
    public int PS5PORT = 0;

    private struct MediaListViewItem
    {
        public string RemoteFilePath { get; set; }
    }

    public WebSrvMediaManager()
    {
        InitializeComponent();

        Loaded += WebSrvMediaManager_Loaded; ;
    }

    private void WebSrvMediaManager_Loaded(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(MediaPath))
        {
            ListMediaFiles();
        }
    }

    private async void ListMediaFiles()
    {
        InstalledMediaListBox.Items.Clear();

        try
        {
            using var NewFtpClient = new AsyncFtpClient(PS5IP, "anonymous", "anonymous", PS5PORT, NewFtpConfig);

            // Connect
            await NewFtpClient.Connect();

            // Enumerate files in the media directory
            foreach (var FTPItem in await NewFtpClient.GetListing(MediaPath))
            {
                var NewMediaLVItem = new MediaListViewItem() { RemoteFilePath = FTPItem.FullName };
                InstalledMediaListBox.Items.Add(NewMediaLVItem);
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

    private async void AddContentButton_Click(object? sender, RoutedEventArgs e)
    {
        var OFD = new OpenFileDialog() { Title = "Select your Media files", AllowMultiple = true };
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
                    await NewFtpClient.UploadFiles(OFDResult, MediaPath, FtpRemoteExists.OverwriteInPlace, false, FtpVerify.None, FtpError.None);

                    var box = MessageBoxManager.GetMessageBoxStandard("Success", "Files uploaded with success!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                else
                {
                    await NewFtpClient.UploadFile(OFDResult[0], MediaPath, FtpRemoteExists.OverwriteInPlace, false, FtpVerify.None);

                    var box = MessageBoxManager.GetMessageBoxStandard("Success", "File uploaded with success!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }

                // Disonnect
                await NewFtpClient.Disconnect();
            }
            catch (Exception ex)
            {
                var box2 = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box2.ShowWindowAsync();
            }

            // Refresh
            ListMediaFiles();
        }
    }

    private async void RemoveContentButton_Click(object? sender, RoutedEventArgs e)
    {
        if (InstalledMediaListBox.SelectedItem is not null)
        {
            MediaListViewItem SelectedMediaFile = (MediaListViewItem)InstalledMediaListBox.SelectedItem;

            var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Do you really want to delete the selected file from the WebSrv ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box.ShowWindowDialogAsync(this);
            if (boxresult == ButtonResult.Yes)
            {

                try
                {
                    using var NewFtpClient = new AsyncFtpClient(PS5IP, "anonymous", "anonymous", PS5PORT, NewFtpConfig);

                    // Connect
                    await NewFtpClient.Connect();

                    // Remove
                    await NewFtpClient.DeleteFile(SelectedMediaFile.RemoteFilePath);

                    // Disonnect
                    await NewFtpClient.Disconnect();
                }
                catch (Exception ex)
                {
                    var box2 = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box2.ShowWindowAsync();
                }

                var box3 = MessageBoxManager.GetMessageBoxStandard("Success", "File removed from the WebSrv.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box3.ShowWindowAsync();

                // Refresh
                ListMediaFiles();
            }

        }
    }

}