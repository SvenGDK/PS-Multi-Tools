using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using PSMultiTools.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PSMultiTools.PS5.Tools.GamePatches;

public partial class PS5GamePatches : Window
{

    public string SearchForGamePatchWithID = "";

    public ObservableCollection<DownloadQueueItem> DownloadQueueItemCollection { get; set; } = [];

    private ListSortDirection GameIDHeaderSorting = ListSortDirection.Ascending;
    private ListSortDirection FileNameHeaderSorting = ListSortDirection.Ascending;
    private ListSortDirection PKGSizeHeaderSorting = ListSortDirection.Ascending;
    private ListSortDirection DownloadStateHeaderSorting = ListSortDirection.Ascending;
    private ListSortDirection MergeStateHeaderSorting = ListSortDirection.Ascending;

    public ContextMenu DownloadsContextMenu = new();
    private readonly MenuItem MergeNowMenuItem = new() { Header = "Merge selected patch from this source pkg" };
    private readonly MenuItem DeleteMenuItem = new() { Header = "Delete selected pkg" };

    public PS5GamePatches()
    {
        InitializeComponent();

        Loaded += PS5GamePatches_Loaded;

        MergeNowMenuItem.Click += MergeNowMenuItem_Click;
        DeleteMenuItem.Click += DeleteMenuItem_Click;

        DownloadQueueListBox.SelectionChanged += DownloadQueueListBox_SelectionChanged;
    }

    private void DownloadQueueListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DownloadQueueListBox.SelectedItem is not null)
        {
            DownloadQueueItem SelectedItemAsQueueItem = (DownloadQueueItem)DownloadQueueListBox.SelectedItem;
            if (SelectedItemAsQueueItem.DownloadState == "Downloaded")
            {
                DownloadButton.IsEnabled = false;
            }
            else
            {
                DownloadButton.IsEnabled = true;
            }
        }
    }

    private async void PS5GamePatches_Loaded(object? sender, RoutedEventArgs e)
    {
        DownloadsContextMenu.Items.Add(MergeNowMenuItem);
        DownloadsContextMenu.Items.Add(DeleteMenuItem);

        DownloadQueueListBox.ContextMenu = DownloadsContextMenu;
        DownloadQueueListBox.ItemsSource = DownloadQueueItemCollection;

        // Add already downloaded patch pkgs
        if (Directory.Exists(Utils.GetDownloadsFolderPath()))
        {
            foreach (var PKG in Directory.GetFiles(Utils.GetDownloadsFolderPath(), "*.pkg", SearchOption.AllDirectories))
            {
                var PKGFileInfo = new FileInfo(PKG);
                string PKGFileName = PKGFileInfo.Name;

                if (PKGFileName.Split('-').Length > 1)
                {
                    if (PKGFileName.Split('-')[1].Split('_').Length > 1)
                    {
                        string PKGID = PKGFileName.Split('-')[1].Split('_')[0];
                        string PKGFileSize = Utils.HumanReadableBytes(PKGFileInfo.Length);
                        var NewQueueItem = new DownloadQueueItem() { FileName = PKGFileName, GameID = PKGID, DownloadURL = PKG, DownloadState = "Downloaded", PKGSize = PKGFileSize };

                        // Get the basename of this pkg
                        string BaseName = PKGFileName.Split([".pkg"], StringSplitOptions.None)[0];
                        // Check if has been already merged
                        if (BaseName.EndsWith("-merged"))
                        {
                            NewQueueItem.MergeState = "Merged";
                        }
                        else
                        {
                            NewQueueItem.MergeState = "Not merged";
                        }

                        DownloadQueueItemCollection.Add(NewQueueItem);
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(SearchForGamePatchWithID))
        {
            SearchGameIDTextBox.Text = SearchForGamePatchWithID;

            if (await Utils.IsURLValid("https://prosperopatches.com/" + SearchForGamePatchWithID))
            {
                var NewWin = new PS5GamePatchSelector() { CurrentGameID = SearchForGamePatchWithID };
                NewWin.Show();
                NewWin.ContentWebView.Address = "https://prosperopatches.com/" + SearchForGamePatchWithID;
            }
        }
    }

    private async void SearchButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SearchGameIDTextBox.Text))
        {
            if (await Utils.IsURLValid("https://prosperopatches.com/" + SearchGameIDTextBox.Text))
            {
                var NewWin = new PS5GamePatchSelector() { CurrentGameID = SearchGameIDTextBox.Text };
                NewWin.Show();
                NewWin.ContentWebView.Address = "https://prosperopatches.com/" + SearchGameIDTextBox.Text;
            }
        }
    }

    private void VisitButton_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://prosperopatches.com/" + SearchGameIDTextBox.Text) { UseShellExecute = true });
    }

    private async void DownloadButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DownloadQueueListBox.SelectedItem is not null)
        {
            if (DownloadQueueListBox.SelectedItems!.Count > 1)
            {
                // Create a new download window for each selected item
                foreach (var SelectedItem in DownloadQueueListBox.SelectedItems)
                {
                    DownloadQueueItem SelectedItemAsQueueItem = (DownloadQueueItem)SelectedItem;
                    SelectedItemAsQueueItem.DownloadState = "Download started";

                    var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5", DownloadQueueItem = SelectedItemAsQueueItem };
                    NewDownloader.Show();
                    if (await NewDownloader.CreateNewDownload(SelectedItemAsQueueItem.DownloadURL!, FileSize: SelectedItemAsQueueItem.PKGSize!) == false)
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();

                        NewDownloader.Close();
                        SelectedItemAsQueueItem.DownloadState = "Download failed";
                    }
                }
            }
            else
            {
                // Download only selected item
                DownloadQueueItem SelectedItemAsQueueItem = (DownloadQueueItem)DownloadQueueListBox.SelectedItem;
                SelectedItemAsQueueItem.DownloadState = "Download started";

                var NewDownloader = new Downloader() { ShowActivated = true, PackageConsole = "PS5", DownloadQueueItem = SelectedItemAsQueueItem };
                NewDownloader.Show();
                if (await NewDownloader.CreateNewDownload(SelectedItemAsQueueItem.DownloadURL!, FileSize: SelectedItemAsQueueItem.PKGSize!) == false)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not download the selected file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();

                    NewDownloader.Close();
                    SelectedItemAsQueueItem.DownloadState = "Download failed";
                }
            }
        }
    }

    private async void MergeNowMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (DownloadQueueListBox.SelectedItem is not null)
        {
            DownloadQueueItem SelectedItemAsQueueItem = (DownloadQueueItem)DownloadQueueListBox.SelectedItem;

            if (SelectedItemAsQueueItem.FileName!.EndsWith("_sc.pkg"))
            {
                if (!(SelectedItemAsQueueItem.MergeState == "Merged"))
                {
                    SelectedItemAsQueueItem.MergeState = "Merge started";

                    // Create a folder to move the downloaded packages into it for merging
                    string BaseName = SelectedItemAsQueueItem.FileName.Split(["_sc.pkg"], StringSplitOptions.None)[0];
                    string NewMergeFolder = Path.Combine(Utils.GetDownloadsFolderPath(), BaseName);
                    if (!Directory.Exists(NewMergeFolder))
                    {
                        Directory.CreateDirectory(NewMergeFolder);
                    }

                    // Get all downloaded packages of this patch and move to the new folder
                    foreach (var DownloadedPatchPKG in Directory.GetFiles(Utils.GetDownloadsFolderPath(), BaseName + "_*.pkg", SearchOption.TopDirectoryOnly))
                    {
                        string PackageFileName = Path.GetFileName(DownloadedPatchPKG);
                        File.Move(DownloadedPatchPKG, Path.Combine(NewMergeFolder, PackageFileName));
                    }

                    // Rename _sc.pkg
                    int PKGCount = Directory.GetFiles(NewMergeFolder).Length - 1;
                    string SCPKGName = BaseName + "_sc.pkg";
                    string NewPKGName = BaseName + "_" + PKGCount.ToString() + ".pkg";
                    if (File.Exists(Path.Combine(NewMergeFolder, SCPKGName)))
                    {
                        File.Move(Path.Combine(NewMergeFolder, SCPKGName), Path.Combine(NewMergeFolder, NewPKGName));
                    }

                    // Open a new PKGMerger window
                    var NewPKGMerger = new PKGBuilder.PS5PKGMerger() { MergeDownloadSourceFolder = NewMergeFolder, MergeBaseName = BaseName };
                    NewPKGMerger.Show();
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Info", "Selected patch has already been merged.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Info", "Selected package is not a source package." + Environment.NewLine + "Select the _sc.pkg of the downloaded patch.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
        }
    }

    private async void DeleteMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (DownloadQueueListBox.SelectedItem is not null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("", "Do you really want to delete this package ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box.ShowWindowDialogAsync(this);
            if (boxresult == ButtonResult.Yes)
            {
                DownloadQueueItem SelectedItemAsQueueItem = (DownloadQueueItem)DownloadQueueListBox.SelectedItem;
                try
                {
                    File.Delete(Path.Combine(Utils.GetDownloadsFolderPath(), SelectedItemAsQueueItem.FileName!));
                }
                catch { }
                DownloadQueueItemCollection.Remove(SelectedItemAsQueueItem);
            }
        }
    }

    private void GameIDLabel_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (GameIDHeaderSorting == ListSortDirection.Descending)
        {
            //Not safe - sort ascending
            var sorted = DownloadQueueItemCollection.OrderBy(x => x.GameID).ToList();
            DownloadQueueItemCollection.Clear();
            foreach (var item in sorted) DownloadQueueItemCollection.Add(item);
            GameIDHeaderSorting = ListSortDirection.Ascending;
        }
        else
        {
            //Sort descending
            var sorted = DownloadQueueItemCollection.OrderByDescending(x => x.GameID).ToList();
            DownloadQueueItemCollection.Clear();
            foreach (var item in sorted) DownloadQueueItemCollection.Add(item);
            GameIDHeaderSorting = ListSortDirection.Descending;
        }
    }

    private void FileNameLabel_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (FileNameHeaderSorting == ListSortDirection.Descending)
        {
            //Not safe - sort ascending
            var sorted = DownloadQueueItemCollection.OrderBy(x => x.FileName).ToList();
            DownloadQueueItemCollection.Clear();
            foreach (var item in sorted) DownloadQueueItemCollection.Add(item);
            FileNameHeaderSorting = ListSortDirection.Ascending;
        }
        else
        {
            //Sort descending
            var sorted = DownloadQueueItemCollection.OrderByDescending(x => x.FileName).ToList();
            DownloadQueueItemCollection.Clear();
            foreach (var item in sorted) DownloadQueueItemCollection.Add(item);
            FileNameHeaderSorting = ListSortDirection.Descending;
        }
    }

    private void PKGSizeLabel_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (PKGSizeHeaderSorting == ListSortDirection.Descending)
        {
            //Not safe - sort ascending
            var sorted = DownloadQueueItemCollection.OrderBy(x => x.PKGSize).ToList();
            DownloadQueueItemCollection.Clear();
            foreach (var item in sorted) DownloadQueueItemCollection.Add(item);
            PKGSizeHeaderSorting = ListSortDirection.Ascending;
        }
        else
        {
            //Sort descending
            var sorted = DownloadQueueItemCollection.OrderByDescending(x => x.PKGSize).ToList();
            DownloadQueueItemCollection.Clear();
            foreach (var item in sorted) DownloadQueueItemCollection.Add(item);
            PKGSizeHeaderSorting = ListSortDirection.Descending;
        }
    }

    private void DownloadStateLabel_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (DownloadStateHeaderSorting == ListSortDirection.Descending)
        {
            //Not safe - sort ascending
            var sorted = DownloadQueueItemCollection.OrderBy(x => x.DownloadState).ToList();
            DownloadQueueItemCollection.Clear();
            foreach (var item in sorted) DownloadQueueItemCollection.Add(item);
            DownloadStateHeaderSorting = ListSortDirection.Ascending;
        }
        else
        {
            //Sort descending
            var sorted = DownloadQueueItemCollection.OrderByDescending(x => x.DownloadState).ToList();
            DownloadQueueItemCollection.Clear();
            foreach (var item in sorted) DownloadQueueItemCollection.Add(item);
            DownloadStateHeaderSorting = ListSortDirection.Descending;
        }
    }

    private void MergeStateLabel_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (MergeStateHeaderSorting == ListSortDirection.Descending)
        {
            //Not safe - sort ascending
            var sorted = DownloadQueueItemCollection.OrderBy(x => x.MergeState).ToList();
            DownloadQueueItemCollection.Clear();
            foreach (var item in sorted) DownloadQueueItemCollection.Add(item);
            MergeStateHeaderSorting = ListSortDirection.Ascending;
        }
        else
        {
            //Sort descending
            var sorted = DownloadQueueItemCollection.OrderByDescending(x => x.MergeState).ToList();
            DownloadQueueItemCollection.Clear();
            foreach (var item in sorted) DownloadQueueItemCollection.Add(item);
            MergeStateHeaderSorting = ListSortDirection.Descending;
        }
    }

}