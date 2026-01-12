using Avalonia.Controls;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Dialogs;
using PSMultiTools.PS5.Tools.GamePatches;
using System;
using System.Net.Http;
using Xilium.CefGlue;
using Xilium.CefGlue.Common.Handlers;

namespace PSMultiTools.Classes
{
    internal class GamePatchesDownloadHandler : DownloadHandler
    {

        // There's unfortunately no other way to download the _sc.pkg correctly unless we ignore certificate errors
        private readonly HttpClient NewHttpClient = new(new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator });

        protected override void OnBeforeDownload(CefBrowser browser, CefDownloadItem downloadItem, string suggestedName, CefBeforeDownloadCallback callback)
        {
            if (PossibleFileDownload(downloadItem.OriginalUrl))
            {
                StartDownload(downloadItem.OriginalUrl);
            }
        }

        private async void StartDownload(string DownloadURL)
        {
            PS5GamePatchSelector? PS5GamePatchSelectorWin = null;
            var AppLifetime = (Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current!.ApplicationLifetime!;
            foreach (Window OpenWin in AppLifetime.Windows)
            {
                if (OpenWin is PS5GamePatchSelector PS5GamePatchSelectorWindow)
                {
                    PS5GamePatchSelectorWin = PS5GamePatchSelectorWindow;
                    break;
                }
            }

            if (PS5GamePatchSelectorWin != null)
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Select Download Option", "Do you want to add this download to the queue ?" + Environment.NewLine + "Selecting 'No' will download the file instantly.", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult = await box.ShowWindowDialogAsync(PS5GamePatchSelectorWin);
                    if (boxresult == ButtonResult.Yes)
                    {
                        // Add to download queue
                        string DownloadFileName = Utils.GetFilenameFromUrl(new Uri(DownloadURL));

                        using var resp = await NewHttpClient.GetAsync(DownloadURL, HttpCompletionOption.ResponseHeadersRead);
                        resp.EnsureSuccessStatusCode();
                        long TotalBytes = resp.Content.Headers.ContentLength ?? -1;

                        AddToQueue(DownloadURL, DownloadFileName, TotalBytes);
                    }
                    else if (boxresult == ButtonResult.No)
                    {
                        // Start the download directly
                        using var resp = await NewHttpClient.GetAsync(DownloadURL, HttpCompletionOption.ResponseHeadersRead);
                        resp.EnsureSuccessStatusCode();
                        long TotalBytes = resp.Content.Headers.ContentLength ?? -1;
                        string TotalFileSize = Utils.HumanReadableBytes(TotalBytes);

                        var NewDownloader = new Downloader() { ShowActivated = true };
                        NewDownloader.Show();

                        if (await NewDownloader.CreateNewDownload(DownloadURL, FileSize: TotalFileSize) == false)
                        {
                            var box2 = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box2.ShowWindowAsync();
                            NewDownloader.Close();
                        }
                    }
                });
            }
            else
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box2 = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find the Game Patches window.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box2.ShowWindowAsync();
                });
            }
        }

        private static async void AddToQueue(string URL, string FileName, long TotalSize)
        {
            // Get the GameID from PS5GamePatchSelector
            var RetrievedGameID = "";
            var AppLifetime = (Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current!.ApplicationLifetime!;
            foreach (Window OpenWin in AppLifetime.Windows)
            {
                if (OpenWin is PS5GamePatchSelector PS5GamePatchSelectorWindow)
                {
                    RetrievedGameID = PS5GamePatchSelectorWindow.CurrentGameID;
                    break;
                }
            }

            var NewQueueItem = new DownloadQueueItem() { FileName = FileName, GameID = RetrievedGameID, DownloadURL = URL, DownloadState = "Not started", MergeState = "Not merged" };

            // Set the download file size
            if (TotalSize > 0)
            {
                string FileSize = Utils.HumanReadableBytes(TotalSize);
                NewQueueItem.PKGSize = FileSize;
            }
            else
            {
                // Get size of requested download
                double URLFileSize = await Utils.WebFileSize(URL);
                string NewRetrievedSize = Utils.HumanReadableBytes((long)URLFileSize);
                NewQueueItem.PKGSize = NewRetrievedSize;
            }

            // Add to queue in the PS5GamePatches window
            foreach (Window OpenWin in AppLifetime.Windows)
            {
                if (OpenWin is PS5GamePatches PS5GamePatchesWindow)
                {
                    PS5GamePatchesWindow.DownloadQueueItemCollection.Add(NewQueueItem);
                    break;
                }
            }
        }

        private static bool PossibleFileDownload(string url)
        {
            var lowerurl = url.ToLowerInvariant();
            return lowerurl.EndsWith(".pkg");
        }

    }
}
