using Avalonia.Controls;
using Avalonia.Threading;
using FluentFTP;
using IronSoftware.Drawing;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace PSMultiTools.Dialogs;

public partial class Uploader : Window
{

    public required string? Console = null; //currently unused - will be used when support for other libraries will be added
    public required string? ConsoleIP = null;
    public required string? ConsoleFTPPort = null;

    public AnyBitmap? BackupIcon = null;
    public required string? BackupPath = null;
    public required string? RemoteDestinationPath = null;

    private CancellationTokenSource? UploadCTS;
    private readonly FtpConfig NewFTPConfig = new() { EncryptionMode = FtpEncryptionMode.None, SslProtocols = SslProtocols.None, DataConnectionEncryption = false, ValidateAnyCertificate = true };

    public Uploader()
    {
        InitializeComponent();
        Loaded += Uploader_Loaded;
        Closing += Uploader_Closing;
    }

    private async void Uploader_Closing(object? sender, WindowClosingEventArgs e)
    {
        // Cancel upload if window got closed
        if (UploadCTS != null)
        {
            await UploadCTS.CancelAsync();
            UploadCTS.Dispose();
            UploadCTS = null;
        }
    }

    private async void Uploader_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Update UI
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (BackupIcon != null)
            {
                UploadProgressTextBlock.Text = Title;
                BackupIconImage.Source = new Avalonia.Media.Imaging.Bitmap(BackupIcon.GetStream());
            }
        });

        // Check input
        if (Console != null && ConsoleIP != null && ConsoleFTPPort != null && BackupPath != null && RemoteDestinationPath != null)
        {
            // Create a cancellation token
            UploadCTS?.Dispose();
            UploadCTS = new CancellationTokenSource();
            var UploadCTSToken = UploadCTS.Token;

            // Start uploading
            try
            {
                var UploadResult = await UploadFolderAsync(ConsoleIP, int.Parse(ConsoleFTPPort), BackupPath, RemoteDestinationPath, UploadCTSToken);
                if (UploadCTSToken.IsCancellationRequested)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Cancelled", "Upload cancelled.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box.ShowWindowAsync();
                    });
                }
                else if (UploadResult == true)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Completed", "Uploaded successfully !", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box.ShowWindowAsync();
                    });
                }
                else
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Completed", "An error occured while uploading, please check the files!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box.ShowWindowAsync();
                    });
                }
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Completed", $"Error while trying to upload: {ex.Message}", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                });
            }

            // Dispose UploadCTS when done
            UploadCTS?.Dispose();
            UploadCTS = null;
        }
        else
        {
            Close();
        }
    }

    private async Task<bool> UploadFolderAsync(string remoteIP,int remotePort,string LocalDirectoryPath,string RemoteDestinationPath, CancellationToken uploadCTSToken)
    {
        try
        {
            // Calculate sizes & setup progress
            var files = Directory
                .EnumerateFiles(LocalDirectoryPath, "*", SearchOption.AllDirectories)
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();

            long totalBytes = files.Sum(f => new FileInfo(f).Length);
            if (totalBytes == 0) totalBytes = 1;

            var transferredByIndex = new long[files.Count];
            var transferredLock = new object();

            var UploadProgress = new Progress<FtpProgress>(p =>
            {
                if (uploadCTSToken.IsCancellationRequested) return;

                int idx = p.FileIndex;
                long transferredForThisEvent = 0;
                transferredForThisEvent = p.TransferredBytes;

                if (idx >= 0 && idx < transferredByIndex.Length)
                {
                    lock (transferredLock)
                    {
                        transferredByIndex[idx] = transferredForThisEvent;
                    }
                }

                long overallTransferred;
                lock (transferredLock)
                {
                    overallTransferred = transferredByIndex.Sum();
                }

                double overallPercent = (overallTransferred / (double)totalBytes) * 100.0;
                if (overallPercent > 100) overallPercent = 100;

                // Current file progress
                _ = Dispatcher.UIThread.InvokeAsync(() =>
                {
                    string FileBeingUploaded = files[p.FileIndex];
                    UploadCurrentFileProgressBar.Value = p.Progress;
                    UploadCurrentFileStatusTextBlock.Text = $"Uploading {Path.GetFileName(FileBeingUploaded)} - {p.Progress:F0}%";
                });

                // Total progress
                _ = Dispatcher.UIThread.InvokeAsync(() =>
                {
                    UploadTotalProgressBar.Value = overallPercent;
                    UploadProgressTextBlock.Text = overallPercent >= 100 ? "Uploading finished!" : $"{Title} - {overallPercent:F0}%";
                });
            });

            // Connect and upload
            using var NewFtpClient = new AsyncFtpClient(remoteIP, "anonymous", "anonymous", remotePort, NewFTPConfig);
            await NewFtpClient.Connect(uploadCTSToken);
            uploadCTSToken.ThrowIfCancellationRequested();

            await NewFtpClient.UploadDirectory(LocalDirectoryPath, RemoteDestinationPath, FtpFolderSyncMode.Update, FtpRemoteExists.NoCheck, FtpVerify.None, null, UploadProgress, uploadCTSToken);

            await NewFtpClient.Disconnect(uploadCTSToken);
            return true;
        }
        catch (OperationCanceledException)
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Upload cancelled.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            });

            return false;
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not upload the selected backup.\n" + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            });

            return false;
        }
    }

}