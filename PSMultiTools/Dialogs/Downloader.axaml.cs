using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;
using IronSoftware.Drawing;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using PSMultiTools.PS5.Tools.GamePatches;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace PSMultiTools.Dialogs;

public partial class Downloader : Window
{
    public string PackageConsole = "";
    public string PackageTitleID = "";
    public string PackageContentID = "";

    private HttpClient DownloadClient = new();
    public bool IsDownloadClientBusy = false;
    private CancellationTokenSource DownloadClientCTS = new();

    public AnyBitmap? DownloadIcon = null;
    public DownloadQueueItem? DownloadQueueItem = null;
    public bool DownloadCompleted = false;
    public string DownloadFileName = "";

    public Downloader()
    {
        InitializeComponent();

        DownloadProgressBar.Value = 0;
        Loaded += Downloader_Loaded;
        Closing += Downloader_Closing;
    }

    private static HttpClient SetHttpClientServerCertificateCustomValidationCallback()
    {
        var NewHttpClientHandler = new HttpClientHandler() { ServerCertificateCustomValidationCallback = new Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, bool>(static (httpRequestMessage, cert, cetChain, policyErrors) => true) };
        return new HttpClient(NewHttpClientHandler);
    }

    private void Downloader_Loaded(object? sender, RoutedEventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess() == false)
        {
            Dispatcher.UIThread.Invoke(() => DownloadImage.Source = new Avalonia.Media.Imaging.Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/PKG.png", UriKind.RelativeOrAbsolute))));
        }
        else
        {
            DownloadImage.Source = new Avalonia.Media.Imaging.Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/PKG.png", UriKind.RelativeOrAbsolute)));
        }

        // Allows downloading with certificate errors
        DownloadClient = SetHttpClientServerCertificateCustomValidationCallback();
    }

    public async Task<bool> CreateNewDownload(string Source, bool ModifyName = false, string NewName = "", string FileSize = "")
    {
        if (Dispatcher.UIThread.CheckAccess() == false)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (DownloadIcon is not null) DownloadImage.Source = new Avalonia.Media.Imaging.Bitmap(DownloadIcon.GetStream());
            });
        }
        else if (DownloadIcon is not null)
            DownloadImage.Source = new Avalonia.Media.Imaging.Bitmap(DownloadIcon.GetStream());

        // Get file name of requested download
        string FileName = Utils.GetFilenameFromUrl(new Uri(Source));
        if (!string.IsNullOrEmpty(FileName))
        {

            DownloadFileName = FileName;

            // Change the file name for .pkgs
            if (ModifyName == true & !string.IsNullOrEmpty(NewName))
            {
                FileName = NewName;
            }

            // Get size of requested download
            double URLFileSize = await Utils.WebFileSize(Source);
            if (URLFileSize > 0)
            {
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        DownloadFileSizeTB.Text = "File Size: " + URLFileSize.ToString() + " MB";
                        FileToDownloadTB.Text = "Downloading " + FileName + " ...";
                    });
                }
                else
                {
                    DownloadFileSizeTB.Text = "File Size: " + URLFileSize.ToString() + " MB";
                    FileToDownloadTB.Text = "Downloading " + FileName + " ...";
                }

                // Prompt for new file name if download already exists in the Downloads folder
                if (File.Exists(Path.Combine(Utils.GetDownloadsFolderPath(), FileName)))
                {
                    var NewInputDialog = new InputDialog() { Title = FileName + " already exists in the Downloads folder" };
                    NewInputDialog.NewValueTextBox.Text = FileName;
                    NewInputDialog.InputDialogTitleTextBlock.Text = "Please enter a new name for the file or leave it to overwrite the existing file:";
                    NewInputDialog.ConfirmButton.Content = "Confirm";

                    string NewSaveFileName = await NewInputDialog.ShowDialog<string>(this);
                    if (!string.IsNullOrEmpty(NewSaveFileName))
                    {
                        DownloadFileName = NewSaveFileName;
                        await DownloadFileWithProgressAsync(Source, Path.Combine(Utils.GetDownloadsFolderPath(), NewSaveFileName));
                    }
                    else
                    {
                        await DownloadFileWithProgressAsync(Source, Path.Combine(Utils.GetDownloadsFolderPath(), FileName));
                    }
                }
                else
                {
                    await DownloadFileWithProgressAsync(Source, Path.Combine(Utils.GetDownloadsFolderPath(), FileName));
                }

                return true;
            }
            // Set file size from 
            else if (!string.IsNullOrEmpty(FileSize))
            {

                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        DownloadFileSizeTB.Text = "File Size: " + FileSize;
                        FileToDownloadTB.Text = "Downloading " + FileName + " ...";
                    });
                }
                else
                {
                    DownloadFileSizeTB.Text = "File Size: " + FileSize;
                    FileToDownloadTB.Text = "Downloading " + FileName + " ...";
                }

                // Prompt for new file name if download already exists in the Downloads folder
                if (File.Exists(Path.Combine(Utils.GetDownloadsFolderPath(), FileName)))
                {
                    var NewInputDialog = new InputDialog() { Title = FileName + " already exists in the Downloads folder" };
                    NewInputDialog.NewValueTextBox.Text = FileName;
                    NewInputDialog.InputDialogTitleTextBlock.Text = "Please enter a new name for the file or leave it to overwrite the existing file:";
                    NewInputDialog.ConfirmButton.Content = "Confirm";

                    string NewSaveFileName = await NewInputDialog.ShowDialog<string>(this);
                    if (!string.IsNullOrEmpty(NewSaveFileName))
                    {
                        DownloadFileName = NewSaveFileName;
                        await DownloadFileWithProgressAsync(Source, Path.Combine(Utils.GetDownloadsFolderPath(), NewSaveFileName));
                    }
                    else
                    {
                        await DownloadFileWithProgressAsync(Source, Path.Combine(Utils.GetDownloadsFolderPath(), FileName));
                    }
                }
                else
                {
                    await DownloadFileWithProgressAsync(Source, Path.Combine(Utils.GetDownloadsFolderPath(), FileName));
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public async Task DownloadFileWithProgressAsync(string FileUrl, string SavePath)
    {
        try
        {
            using (DownloadClient)
            using (var NewHttpResponseMessage = await DownloadClient.GetAsync(FileUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                if (NewHttpResponseMessage.IsSuccessStatusCode)
                {

                    DownloadClientCTS = new CancellationTokenSource();
                    var DownloadCancellationToken = DownloadClientCTS.Token;
                    IsDownloadClientBusy = true;

                    long TotalBytes = NewHttpResponseMessage.Content.Headers.ContentLength.GetValueOrDefault(0L);
                    using var ResponseStream = await NewHttpResponseMessage.Content.ReadAsStreamAsync(DownloadCancellationToken);

                    using var NewFileStream = new FileStream(SavePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
                    var Buffer = new byte[8192];
                    long TotalBytesRead = 0;
                    int BytesRead;

                    var NewStopwatch = new Stopwatch();
                    NewStopwatch.Start();

                    do
                    {
                        BytesRead = await ResponseStream.ReadAsync(Buffer, DownloadCancellationToken);
                        if (BytesRead == 0)
                            break;

                        await NewFileStream.WriteAsync(Buffer.AsMemory(0, BytesRead));

                        TotalBytesRead += BytesRead;

                        double ElapsedSeconds = NewStopwatch.Elapsed.TotalSeconds;
                        double DownloadSpeed = TotalBytesRead / ElapsedSeconds;
                        double SpeedInKbps = DownloadSpeed / 1024;
                        double ETAInSeconds = TotalBytes > 0 ? (TotalBytes - TotalBytesRead) / DownloadSpeed : 0;

                        // Display progress
                        if (TotalBytes > 0)
                        {
                            double DLProgress = TotalBytesRead * 100 / (double)TotalBytes;

                            if (Dispatcher.UIThread.CheckAccess() == false)
                            {
                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    DownloadETA.Text = $"ETA: {ETAInSeconds:F0} seconds left";
                                    DownloadSpeedTB.Text = $"Speed: {SpeedInKbps:F2} KB/s";
                                    DownloadProgressBar.Value = DLProgress;
                                });
                            }

                            else
                            {
                                DownloadETA.Text = $"ETA: {ETAInSeconds:F0} seconds left";
                                DownloadSpeedTB.Text = $"Speed: {SpeedInKbps:F2} KB/s";
                                DownloadProgressBar.Value = DLProgress;
                            }
                        }
                    }
                    while (true);

                    NewStopwatch.Stop();

                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => FileToDownloadTB.Text = "Download finished");
                    }
                    else
                    {
                        FileToDownloadTB.Text = "Download finished";
                    }

                    // For PS5 game patches
                    if (DownloadQueueItem is not null)
                    {
                        if (!string.IsNullOrEmpty(DownloadQueueItem.FileName))
                        {
                            // Update progress in PS5GamePatches (if open)
                            var AppLifetime = (Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current!.ApplicationLifetime!;
                            foreach (Window OpenWin in AppLifetime.Windows)
                            {
                                if (OpenWin is PS5GamePatches PS5GamePatchesWindow)
                                {
                                    PS5GamePatches OpenGamePatchesWindow = PS5GamePatchesWindow;
                                    foreach (var DownloadItem in OpenGamePatchesWindow.DownloadQueueListBox.Items)
                                    {
                                        DownloadQueueItem DownloadItemAsDownloadQueueItem = (DownloadQueueItem)DownloadItem!;
                                        if ((DownloadItemAsDownloadQueueItem.FileName ?? "") == (DownloadQueueItem.FileName ?? ""))
                                        {
                                            DownloadItemAsDownloadQueueItem.DownloadState = "Downloaded";
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    DownloadCompleted = true;
                }
            }

            if (DownloadCompleted)
            {
                IsDownloadClientBusy = false;

                // Prompt for extracting a downloaded archive and opening the downloads folder
                if (DownloadFileName.EndsWith(".zip") || DownloadFileName.EndsWith(".7z") || DownloadFileName.EndsWith(".rar"))
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Extract download ?", "Download completed!" + Environment.NewLine + "The downloaded file is an archive that can be extracted, do you want to extract it now ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult = await box.ShowWindowDialogAsync(this);
                    if (boxresult == ButtonResult.Yes)
                    {

                        // Check and extract the downloaded archive into the downloads folder
                        string DownloadedFile = Path.Combine(Utils.GetDownloadsFolderPath(), DownloadFileName);
                        if (File.Exists(DownloadedFile))
                        {

                            Process ArchiveExtractor = new();
                            ArchiveExtractor.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "7z.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "7zz");
                            ArchiveExtractor.StartInfo.Arguments = $"x \"{DownloadedFile}\" -o\"{Utils.EnsureTrailingSeparator(Utils.GetDownloadsFolderPath())}\" -y";
                            ArchiveExtractor.StartInfo.UseShellExecute = false;
                            ArchiveExtractor.StartInfo.CreateNoWindow = true;
                            ArchiveExtractor.Start();
                            await ArchiveExtractor.WaitForExitAsync();
                            ArchiveExtractor.Close();

                            var box2 = MessageBoxManager.GetMessageBoxStandard("Completed", "Extraction done!" + Environment.NewLine + "Do you want to open the Downloads folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                            var boxresult2 = await box2.ShowWindowDialogAsync(this);
                            if (boxresult2 == ButtonResult.Yes)
                            {
                                Utils.OpenDownloadsFolder();
                            }
                        }
                        else
                        {
                            var box3 = MessageBoxManager.GetMessageBoxStandard("Completed", "Could not find the downloaded archive.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            var box4 = MessageBoxManager.GetMessageBoxStandard("Completed", "Do you want to open the Downloads folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                            await box3.ShowWindowAsync();
                            var boxresult4 = await box4.ShowWindowDialogAsync(this);
                            if (boxresult4 == ButtonResult.Yes)
                            {
                                Utils.OpenDownloadsFolder();
                            }
                        }
                    }
                    else
                    {
                        var box5 = MessageBoxManager.GetMessageBoxStandard("Completed", "Do you want to open the Downloads folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                        var boxresult5 = await box5.ShowWindowDialogAsync(this);
                        if (boxresult5 == ButtonResult.Yes)
                        {
                            Utils.OpenDownloadsFolder();
                        }
                    }
                }
                else
                {
                    var box6 = MessageBoxManager.GetMessageBoxStandard("Completed", "Download completed!" + Environment.NewLine + "Do you want to open the Downloads folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult6 = await box6.ShowWindowDialogAsync(this);
                    if (boxresult6 == ButtonResult.Yes)
                    {
                        Utils.OpenDownloadsFolder();
                    }
                }
            }
            else
            {
                var box7 = MessageBoxManager.GetMessageBoxStandard("Info", "Download canceled.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);
                await box7.ShowWindowAsync();
            }
        }
        catch (OperationCanceledException)
        {
            var box8 = MessageBoxManager.GetMessageBoxStandard("Info", "Download canceled.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);
            await box8.ShowWindowAsync();
        }
        catch (Exception ex)
        {
            var box9 = MessageBoxManager.GetMessageBoxStandard("", ex.ToString(), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);
            await box9.ShowWindowAsync();
        }
        finally
        {
            DownloadClientCTS?.Dispose();
        }
    }

    private void Downloader_Closing(object? sender, CancelEventArgs e)
    {
        if (IsDownloadClientBusy && DownloadClientCTS is not null)
        {
            DownloadClientCTS.Cancel();
            DownloadClientCTS.Dispose();
        }
    }

}