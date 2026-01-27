using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentFTP;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;

namespace PSMultiTools.PS5.Tools;

public partial class PS5LibraryGrabber : Window
{

    public string ConsoleIP = "";
    public string ConsoleFTPPort = "";

    public FtpConfig NewFtpConfig = new() { EncryptionMode = FtpEncryptionMode.None, SslProtocols = SslProtocols.None, DataConnectionEncryption = false, ValidateAnyCertificate = true };

    public PS5LibraryGrabber()
    {
        InitializeComponent();
        Loaded += PS5LibraryGrabber_Loaded;
    }

    private void PS5LibraryGrabber_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ConsoleIP))
            PS5IPTextBox.Text = ConsoleIP;

        if (!string.IsNullOrEmpty(ConsoleFTPPort))
            PS5FTPPortTextBox.Text = ConsoleFTPPort;
    }

    private async void BrowseFolderButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a folder where the decrypted libraries should be saved" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedDirectoryTextBox.Text = FBDResult;
        }
    }

    private async void GrabButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(PS5IPTextBox.Text) && !string.IsNullOrEmpty(PS5FTPPortTextBox.Text) && !string.IsNullOrEmpty(SelectedDirectoryTextBox.Text))
        {

            // List libraries to download
            List<FtpListItem> FilesToDownload = [];
            try
            {
                using var NewFtpClient = new AsyncFtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", int.Parse(PS5FTPPortTextBox.Text), NewFtpConfig);
                await NewFtpClient.Connect();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LogTextBox.Text += "Getting listing from /system/common/lib\n";
                });

                FtpListItem[] CommonLibListing = await NewFtpClient.GetListing("/system/common/lib", FtpListOption.Auto);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LogTextBox.Text += "Getting listing from /system/priv/lib\n";
                });

                FtpListItem[] PrivLibListing = await NewFtpClient.GetListing("/system/priv/lib", FtpListOption.Auto);

                // Filter by .sbin, .sprx, .self
                string[] SignedFiles = [".sbin", ".sprx", ".self"];
                List<FtpListItem> FilesToDownloadFromCommon = [.. CommonLibListing
                    .Where(i => i.Type == FtpObjectType.File)
                    .Where(i => SignedFiles.Contains(Path.GetExtension(i.FullName), StringComparer.OrdinalIgnoreCase))
                    .OrderBy(i => i.FullName)];

                List<FtpListItem> FilesToDownloadFromPriv = [.. PrivLibListing
                    .Where(i => i.Type == FtpObjectType.File)
                    .Where(i => SignedFiles.Contains(Path.GetExtension(i.FullName), StringComparer.OrdinalIgnoreCase))
                    .OrderBy(i => i.FullName)];

                //Combine the lists
                FilesToDownload = FilesToDownloadFromCommon.Concat(FilesToDownloadFromPriv).OrderBy(i => i.FullName).ToList();

                await NewFtpClient.Disconnect();
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LogTextBox.Text += $"{ex.Message}\n";
                });
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                LogTextBox.Text += "Starting download :\n";
                ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                LogTextBoxScrollViewer?.ScrollToEnd();
            });

            // Download found libraries
            if (FilesToDownload.Count != 0)
            {
                foreach (var FileToDownload in FilesToDownload)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        LogTextBox.Text += $"Downloading {FileToDownload.Name} ...\n";
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    });

                    string destinationPath = Path.Combine(SelectedDirectoryTextBox.Text, FileToDownload.Name);

                    // Download each library with SELF command
                    if (!File.Exists(destinationPath))
                    {
                        DownloadDecryptedFromFTP(PS5IPTextBox.Text, int.Parse(PS5FTPPortTextBox.Text), destinationPath, FileToDownload.FullName);

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            LogTextBox.Text += $"Downloading {FileToDownload.Name} completed\n";
                            ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                    else
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            LogTextBox.Text += $"{FileToDownload.Name} already downloaded. Skipping ...\n";
                            ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                }
            }

            await Dispatcher.UIThread.Invoke(async () =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Done", $"Done! All files downloaded to {SelectedDirectoryTextBox.Text}", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                await box.ShowWindowAsync();
            });
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please check your input.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async void DownloadDecryptedFromFTP(string PS5IP, int PS5Port, string downloadTo, string remotePath)
    {
        try
        {
            using var NewFtpClient = new AsyncFtpClient(PS5IP, "anonymous", "anonymous", PS5Port, NewFtpConfig);
            await NewFtpClient.Connect();

            //// Send SELF command (not required as enabled by default)
            //var SELFReply = await NewFtpClient.Execute("SELF");

            //// Output reply to log
            //await Dispatcher.UIThread.InvokeAsync(() =>
            //{
            //    LogTextBox.Text += $"SELF command reply: {SELFReply.Code} {SELFReply.InfoMessages}\n";
            //});

            // Download the file
            await NewFtpClient.DownloadFile(downloadTo, remotePath, FtpLocalExists.Overwrite, FtpVerify.None);

            // Disconnect
            await NewFtpClient.Disconnect();
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                LogTextBox.Text += $"{ex.Message}\n";
            });
        }
    }

}