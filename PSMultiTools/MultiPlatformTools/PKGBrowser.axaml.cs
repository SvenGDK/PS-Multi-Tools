using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using IronSoftware.Drawing;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xilium.CefGlue.Avalonia;

namespace PSMultiTools.MultiPlatformTools;

public partial class PKGBrowser : Window
{

    public string Console = "";
    public List<string> URLs = [];
    public int CurrentURL = 0;

    private ListBox? CurrentListView = null;
    private readonly AvaloniaCefBrowser ContentWebView = new() { Address = "about:blank" };

    public ObservableCollection<NPSPKG> PKGList { get; } = [];
    public List<NPSPKG> DownloadsList = [];
    public List<NPSPKG> TempDownloadsList = [];

    public MenuItem DownloadMenuItem = new() { Header = "Download PKG", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/download.png"))) } };
    public MenuItem CreateRAPMenuItem = new() { Header = "Create .rap file", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/create.png"))) } };
    public MenuItem ExtractPKGMenuItem = new() { Header = "Extract .pkg file", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/extract.png"))) } };

    public MenuItem ShowDownloadMenuItem = new() { Header = "Show download in folder" };
    public MenuItem CancelDownloadMenuItem = new() { Header = "Cancel download" };

    public PKGBrowser()
    {
        InitializeComponent();

        Loaded += PKGBrowser_Loaded;

        DownloadMenuItem.Click += DownloadMenuItem_Click;
        CreateRAPMenuItem.Click += CreateRAPMenuItem_Click;
        ShowDownloadMenuItem.Click += ShowDownloadMenuItem_Click;
        CancelDownloadMenuItem.Click += CancelDownloadMenuItem_Click;
        ExtractPKGMenuItem.Click += ExtractPKGMenuItem_Click;

        MainTabControl.PointerPressed += MainTabControl_PointerPressed;

        NameSearchTextBox.TextChanged += NameSearchTextBox_TextChanged;
        TitleIDSearchTextBox.TextChanged += TitleIDSearchTextBox_TextChanged;
        ContentIDSearchTextBox.TextChanged += ContentIDSearchTextBox_TextChanged;

        GamesListBox.SelectionChanged += GamesListBox_SelectionChanged;

        // ContentWebView setup
        ContentWebView.LoadEnd += ContentWebView_LoadEnd;

        var browserWrapper = this.FindControl<Decorator>("ContentWebViewWrapper");
        browserWrapper!.Child = ContentWebView;
    }

    private void PKGBrowser_Loaded(object? sender, RoutedEventArgs e)
    {
        LoadListViewContextMenu();
        LoadDownloadsContextMenu();
    }

    private void MainTabControl_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        switch (MainTabControl.SelectedIndex)
        {
            case 0:
                {
                    LoadGames();
                    break;
                }
            case 1:
                {
                    LoadDemos();
                    break;
                }
            case 2:
                {
                    LoadDLCs();
                    break;
                }
            case 3:
                {
                    LoadThemes();
                    break;
                }
            case 4:
                {
                    LoadAvatars();
                    break;
                }
        }
    }

    private async void LoadGames()
    {
        PKGList.Clear();

        var box = MessageBoxManager.GetMessageBoxStandard("Loading game database", "Do you want to load the latest database ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
        var boxresult = await box.ShowWindowDialogAsync(this);
        if (boxresult == ButtonResult.Yes)
        {
            LoadDLList(Console + "_GAMES.tsv", true);
        }
        else if (boxresult == ButtonResult.No)
        {
            LoadDLList(Console + "_GAMES.tsv", false);
        }

        CurrentListView = GamesListBox;
        GamesListBox.ItemsSource = PKGList;
    }

    private async void LoadDemos()
    {
        PKGList.Clear();

        var box = MessageBoxManager.GetMessageBoxStandard("Loading game database", "Do you want to load the latest database ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
        var boxresult = await box.ShowWindowDialogAsync(this);
        if (boxresult == ButtonResult.Yes)
        {
            LoadDLList(Console + "_DEMOS.tsv", true);
        }
        else if (boxresult == ButtonResult.No)
        {
            LoadDLList(Console + "_DEMOS.tsv", false);
        }

        CurrentListView = DemosListBox;
        DemosListBox.ItemsSource = PKGList;
    }

    private async void LoadDLCs()
    {
        PKGList.Clear();

        var box = MessageBoxManager.GetMessageBoxStandard("Loading game database", "Do you want to load the latest database ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
        var boxresult = await box.ShowWindowDialogAsync(this);
        if (boxresult == ButtonResult.Yes)
        {
            LoadDLList(Console + "_DLCS.tsv", true);
        }
        else if (boxresult == ButtonResult.No)
        {
            LoadDLList(Console + "_DLCS.tsv", false);
        }

        CurrentListView = DLCsListBox;
        DLCsListBox.ItemsSource = PKGList;
    }

    private async void LoadThemes()
    {
        PKGList.Clear();

        var box = MessageBoxManager.GetMessageBoxStandard("Loading game database", "Do you want to load the latest database ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
        var boxresult = await box.ShowWindowDialogAsync(this);
        if (boxresult == ButtonResult.Yes)
        {
            LoadDLList(Console + "_THEMES.tsv", true);
        }
        else if (boxresult == ButtonResult.No)
        {
            LoadDLList(Console + "_THEMES.tsv", false);
        }

        CurrentListView = ThemesListBox;
        ThemesListBox.ItemsSource = PKGList;
    }

    private async void LoadAvatars()
    {
        PKGList.Clear();

        var box = MessageBoxManager.GetMessageBoxStandard("Loading game database", "Do you want to load the latest database ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
        var boxresult = await box.ShowWindowDialogAsync(this);
        if (boxresult == ButtonResult.Yes)
        {
            LoadDLList(Console + "_AVATARS.tsv", true);
        }
        else if (boxresult == ButtonResult.No)
        {
            LoadDLList(Console + "_AVATARS.tsv", false);
        }

        CurrentListView = AvatarsListBox;
        AvatarsListBox.ItemsSource = PKGList;
    }

    #region NPS Downloads

    private async void LoadDLList(string RequestedList, bool LoadLatest)
    {
        // Get the latest database from NPS
        if (LoadLatest == true)
        {
            using var client = new HttpClient();
            string ListURL = "https://nopaystation.com/tsv/" + RequestedList;
            string GamesList = await client.GetStringAsync(ListURL);
            string[] GamesListLines = GamesList.Split([Environment.NewLine], StringSplitOptions.None);

            foreach (string GameLine in GamesListLines.Skip(1))
            {
                string[] SplittedValues = GameLine.Split("\t");
                var AdditionalInfo = Utils.GetFileSizeAndDate(SplittedValues[8].Trim(), SplittedValues[6].Trim());
                var NewPackage = new NPSPKG()
                {
                    PackageName = SplittedValues[2].Trim(),
                    PackageURL = SplittedValues[3].Trim(),
                    PackageTitleID = SplittedValues[0].Trim(),
                    PackageContentID = SplittedValues[5].Trim(),
                    PackageRAP = SplittedValues[4].Trim(),
                    PackageDate = AdditionalInfo.FileDate,
                    PackageSize = AdditionalInfo.FileSize,
                    PackageRegion = SplittedValues[1].Trim()
                };

                if (!SplittedValues[3].Trim().Equals("MISSING", StringComparison.OrdinalIgnoreCase))
                {
                    switch (Console ?? "")
                    {
                        case "PS3":
                            {
                                if (!string.IsNullOrEmpty(NewPackage.PackageContentID))
                                {
                                    string TitleID = NewPackage.PackageTitleID;
                                    NewPackage.PackageCoverSource = "https://www.gametdb.com/PS3/" + TitleID;
                                }
                                break;
                            }
                        case "PSV":
                            {
                                if (!string.IsNullOrEmpty(NewPackage.PackageTitleID))
                                {
                                    string TitleID = NewPackage.PackageTitleID;
                                    NewPackage.PackageCoverSource = "https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + TitleID + ".png";
                                }
                                break;
                            }
                    }

                    DownloadsList.Add(NewPackage);
                }
            }
        }
        else if (File.Exists(Environment.CurrentDirectory + @"\Databases\" + RequestedList)) // Use local .tsv file
        {
            string[] FileReader = File.ReadAllLines(Environment.CurrentDirectory + @"\Databases\" + RequestedList, System.Text.Encoding.UTF8);
            foreach (string GameLine in FileReader.Skip(1)) // Skip 1st line in TSV
            {
                string[] SplittedValues = GameLine.Split(Convert.ToChar("\t"));
                var AdditionalInfo = Utils.GetFileSizeAndDate(SplittedValues[8], SplittedValues[6]);
                var NewPackage = new NPSPKG()
                {
                    PackageName = SplittedValues[2],
                    PackageURL = SplittedValues[3],
                    PackageTitleID = SplittedValues[0],
                    PackageContentID = SplittedValues[5],
                    PackageRAP = SplittedValues[4],
                    PackageDate = AdditionalInfo.FileDate,
                    PackageSize = AdditionalInfo.FileSize,
                    PackageRegion = SplittedValues[1]
                };

                if (!(SplittedValues[3] == "MISSING"))
                {

                    switch (Console ?? "")
                    {
                        case "PS3":
                            {
                                if (!string.IsNullOrEmpty(NewPackage.PackageContentID))
                                {
                                    string TitleID = NewPackage.PackageTitleID;
                                    //string ContentID = NewPackage.PackageContentID.Split('-')[2];

                                    NewPackage.PackageCoverSource = "https://www.gametdb.com/PS3/" + TitleID;
                                }

                                break;
                            }
                        case "PSV":
                            {
                                if (!string.IsNullOrEmpty(NewPackage.PackageTitleID))
                                {
                                    string TitleID = NewPackage.PackageTitleID;
                                    //string ContentID = NewPackage.PackageContentID.Split('-')[2];

                                    NewPackage.PackageCoverSource = "https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + TitleID + ".png";
                                }

                                break;
                            }
                    }

                    DownloadsList.Add(NewPackage);
                }
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Could not load list", "No data available. Please add TSV files to the 'Databases' directory.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }

        // Add to the list
        foreach (var AvailablePKG in DownloadsList)
        {
            switch (RequestedList.Split('.')[0] ?? "")
            {
                case "PS3_GAMES":
                case "PSV_GAMES":
                    {
                        PKGList.Add(AvailablePKG);
                        break;
                    }
                case "PS3_DEMOS":
                    {
                        PKGList.Add(AvailablePKG);
                        break;
                    }
                case "PS3_DLCS":
                case "PSV_DLCS":
                    {
                        PKGList.Add(AvailablePKG);
                        break;
                    }
                case "PS3_THEMES":
                case "PSV_THEMES":
                    {
                        PKGList.Add(AvailablePKG);
                        break;
                    }
                case "PS3_AVATARS":
                    {
                        PKGList.Add(AvailablePKG);
                        break;
                    }
            }
        }
    }

    private void TextSearch(TextBox TxtBox)
    {
        if (CurrentListView is not null)
        {
            PKGList.Clear();

            if ((TxtBox.Name ?? "") == (NameSearchTextBox.Name ?? ""))
            {
                if (!string.IsNullOrEmpty(TxtBox.Text))
                {
                    foreach (NPSPKG item in DownloadsList.Where(lvi => lvi.PackageName!.Contains(TxtBox.Text.ToLower().Trim(), StringComparison.CurrentCultureIgnoreCase)))
                        PKGList.Add(item);
                }
            }
            else if ((TxtBox.Name ?? "") == (TitleIDSearchTextBox.Name ?? ""))
            {
                if (!string.IsNullOrEmpty(TxtBox.Text))
                {
                    foreach (NPSPKG item in DownloadsList.Where(lvi => lvi.PackageTitleID!.Contains(TxtBox.Text.ToLower().Trim(), StringComparison.CurrentCultureIgnoreCase)))
                        PKGList.Add(item);
                }
            }
            else if ((TxtBox.Name ?? "") == (ContentIDSearchTextBox.Name ?? ""))
            {
                if (!string.IsNullOrEmpty(TxtBox.Text))
                {
                    foreach (NPSPKG item in DownloadsList.Where(lvi => lvi.PackageContentID!.Contains(TxtBox.Text.ToLower().Trim(), StringComparison.CurrentCultureIgnoreCase)))
                        PKGList.Add(item);
                }
            }
        }
    }

    private async void NameSearchTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (CurrentListView is not null)
        {
            TextSearch(NameSearchTextBox);
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Search unavailable", "Please load the database first by clicking on the middle of the list or the left/right arrow.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    private async void TitleIDSearchTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (CurrentListView is not null)
        {
            TextSearch(TitleIDSearchTextBox);
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Search unavailable", "Please load the database first by clicking on the middle of the list or the left/right arrow.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    private async void ContentIDSearchTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (CurrentListView is not null)
        {
            TextSearch(ContentIDSearchTextBox);
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Search unavailable", "Please load the database first by clicking on the middle of the list or the left/right arrow.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    private void DownloadMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (CurrentListView is not null && CurrentListView.SelectedItem is not null)
        {
            NPSPKG SelectedPackage = (NPSPKG)CurrentListView.SelectedItem;
            CreateNewDownload(SelectedPackage);
        }
    }

    private void CreateNewDownload(NPSPKG NPSPKG)
    {

        if (!Directory.Exists(Path.Combine(Utils.GetDownloadsFolderPath(), Console)))
        {
            Directory.CreateDirectory(Path.Combine(Utils.GetDownloadsFolderPath(), Console, "packages"));
            Directory.CreateDirectory(Path.Combine(Utils.GetDownloadsFolderPath(), Console, "exdata"));
        }

        var NewWebClient = new WebClient();

        var NewPKGDL = new PKGDownloadListViewItem()
        {
            AssociatedWebClient = NewWebClient,
            PackageName = NPSPKG.PackageName,
            PackageSize = NPSPKG.PackageSize,
            PackageContentID = NPSPKG.PackageContentID,
            PackageTitleID = NPSPKG.PackageTitleID,
            PackageDownloadDestination = Path.Combine(Utils.GetDownloadsFolderPath(), Console, "packages", NPSPKG.PackageName + ".pkg"),
            PackageDownloadState = "Downloading"
        };

        DownloadsListBox.Items.Add(NewPKGDL);

        if (!string.IsNullOrEmpty(NPSPKG.PackageURL))
        {
            NewWebClient.DownloadFileAsync(new Uri(NPSPKG.PackageURL), Path.Combine(Utils.GetDownloadsFolderPath(), Console, "packages", NPSPKG.PackageName + ".pkg"), Stopwatch.StartNew());

            NewWebClient.DownloadProgressChanged += (sender, e) =>
            {
                // Update values
                WebClient ClientSender = (WebClient)sender;

                foreach (var DownloadItem in DownloadsListBox.Items)
                {
                    PKGDownloadListViewItem DLListViewItem = (PKGDownloadListViewItem)DownloadItem!;
                    if (ReferenceEquals(DLListViewItem.AssociatedWebClient, ClientSender))
                    {
                        DLListViewItem.PackageDownloadState = e.ProgressPercentage.ToString() + "% - " + (e.BytesReceived / (double)(1024 * 1024)).ToString("0.000 MB") + "/" + (e.TotalBytesToReceive / (double)(1024 * 1024)).ToString("0.000 MB");
                    }
                }

            };

            NewWebClient.DownloadFileCompleted += (sender, e) =>
            {
                WebClient ClientSender = (WebClient)sender!;

                // Update values
                foreach (var DownloadItem in DownloadsListBox.Items)
                {
                    PKGDownloadListViewItem DLListViewItem = (PKGDownloadListViewItem)DownloadItem!;
                    if (ReferenceEquals(DLListViewItem.AssociatedWebClient, ClientSender))
                    {
                        DLListViewItem.PackageDownloadState = "Download complete";
                    }
                }
            };
        }
    }

    private void CreateRAPMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (CurrentListView is not null && CurrentListView.SelectedItem is not null)
        {
            NPSPKG SelectedPackage = (NPSPKG)CurrentListView.SelectedItem;
            CreateRAP(SelectedPackage.PackageContentID!, SelectedPackage.PackageRAP!);
        }
    }

    public void LoadListViewContextMenu()
    {
        var ListViewContextMenu = new ContextMenu();
        ListViewContextMenu.Items.Add(DownloadMenuItem);

        GamesListBox.ContextMenu = ListViewContextMenu;
        DemosListBox.ContextMenu = ListViewContextMenu;
        DLCsListBox.ContextMenu = ListViewContextMenu;
        ThemesListBox.ContextMenu = ListViewContextMenu;
        AvatarsListBox.ContextMenu = ListViewContextMenu;
    }

    private void LoadDownloadsContextMenu()
    {
        DownloadsListBox.ContextMenu = null;

        var DownloadsContextMenu = new ContextMenu();
        DownloadsContextMenu.Items.Add(ShowDownloadMenuItem);
        DownloadsContextMenu.Items.Add(CancelDownloadMenuItem);
        DownloadsContextMenu.Items.Add(new Separator());

        if (Console == "PS3")
        {
            DownloadsContextMenu.Items.Add(CreateRAPMenuItem);
        }
        else if (Console == "PSV")
        {
            DownloadsContextMenu.Items.Add(ExtractPKGMenuItem);
        }

        DownloadsListBox.ContextMenu = DownloadsContextMenu;
    }

    private static async void CreateRAP(string ContentID, string RAP)
    {
        try
        {
            if (!string.IsNullOrEmpty(ContentID) && RAP.Length % 2 == 0)
            {

                // Create exdata folder if not exists
                if (!Directory.Exists(Path.Combine(Utils.GetDownloadsFolderPath(), "PS3", "exdata")))
                    Directory.CreateDirectory(Path.Combine(Utils.GetDownloadsFolderPath(), "PS3", "exdata"));

                byte[] bytes = new byte[(int)Math.Round(RAP.Length / 2d - 1d) + 1];
                for (int index = 0, loopTo = (int)Math.Round(RAP.Length / 2d - 1d); index <= loopTo; index++)
                    bytes[index] = Convert.ToByte(RAP.Substring(index * 2, 2), 16);
                File.WriteAllBytes(Path.Combine(Utils.GetDownloadsFolderPath(), "PS3", "exdata", ContentID + ".rap"), bytes);

                var box = MessageBoxManager.GetMessageBoxStandard("Info", ContentID + ".rap file created!" + Environment.NewLine + @"You can find it in the 'Downloads\PS3\exdata' folder.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Info", "This package requires no .rap file. Simply activate it with ReactPSN.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error creating RAP file: " + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    #endregion

    private async void ContentWebView_LoadEnd(object sender, Xilium.CefGlue.Common.Events.LoadEndEventArgs e)
    {
        if (e.HttpStatusCode == 200)
        {
            // Game ID
            string GameID = await ContentWebView.EvaluateJavaScript<string>("return document.getElementsByClassName('GameData')[0].getElementsByTagName('td')[1].innerText");
            // Game Image
            string GameImageURL = await ContentWebView.EvaluateJavaScript<string>("return document.getElementsByClassName('frame lfloat')[0].getElementsByTagName('img')[0].src");

            if (!(GameImageURL == "null") && !string.IsNullOrEmpty(GameID))
            {

                GameID = GameID.Replace("\"", "");
                GameImageURL = GameImageURL.Replace("\"", "");

                // Update values on library
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    foreach (var PKGinList in PKGList)
                    {
                        if (PKGinList is not NPSPKG FoundPKG) continue;

                        if (!string.IsNullOrEmpty(GameID) && GameImageURL != null)
                        {
                            if (FoundPKG.PackageTitleID!.Contains(GameID, StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (FoundPKG.GameCoverSource is null && !string.IsNullOrEmpty(GameImageURL))
                                {
                                    try
                                    {
                                        FoundPKG.GameCoverSource = await AnyBitmap.FromUriAsync(new Uri(GameImageURL), true);
                                    }
                                    catch
                                    {
                                        Trace.WriteLine("Could not set cover");
                                    }
                                }

                                break;
                            }
                        }
                    }
                });
            }
        }
        else
        {
            Trace.WriteLine(e.HttpStatusCode);
        }
    }

    private async void GamesListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            NPSPKG SelectedGame = (NPSPKG)GamesListBox.SelectedItem;
            if (!string.IsNullOrEmpty(SelectedGame.PackageCoverSource))
            {
                if (await Utils.IsURLValid(SelectedGame.PackageCoverSource))
                {
                    switch (Console ?? "")
                    {
                        case "PS3":
                            {
                                Trace.WriteLine(SelectedGame.PackageCoverSource);
                                ContentWebView.Address = SelectedGame.PackageCoverSource;
                                break;
                            }
                        case "PSV":
                            {
                                if (Dispatcher.UIThread.CheckAccess() == false)
                                {
                                    await Dispatcher.UIThread.Invoke(async () =>
                                    {
                                        foreach (var PKGinList in PKGList)
                                        {
                                            if (PKGinList is not NPSPKG FoundPKG) continue;

                                            if (FoundPKG == SelectedGame)
                                            {
                                                if (FoundPKG.GameCoverSource is null)
                                                {
                                                    try
                                                    {
                                                        FoundPKG.GameCoverSource = await AnyBitmap.FromUriAsync(new Uri(SelectedGame.PackageCoverSource), true);
                                                    }
                                                    catch
                                                    {
                                                        Trace.WriteLine("Could not set cover");
                                                    }
                                                }

                                                break;
                                            }
                                        }
                                    });
                                }
                                else
                                {
                                    foreach (var PKGinList in PKGList)
                                    {
                                        if (PKGinList is not NPSPKG FoundPKG) continue;

                                        if (FoundPKG == SelectedGame)
                                        {
                                            if (FoundPKG.GameCoverSource is null)
                                            {
                                                try
                                                {
                                                    FoundPKG.GameCoverSource = await AnyBitmap.FromUriAsync(new Uri(SelectedGame.PackageCoverSource), true);
                                                }
                                                catch
                                                {
                                                    Trace.WriteLine("Could not set cover");
                                                }
                                            }

                                            break;
                                        }
                                    }
                                }

                                break;
                            }
                    }
                }
            }

            PKGTitleTextBlock.Text = SelectedGame.PackageName;
            TitleIDTextBlock.Text = "Title ID: " + SelectedGame.PackageTitleID;
            ContentIDTextBlock.Text = "Content ID: " + SelectedGame.PackageContentID;
            RegionTextBlock.Text = "Region: " + SelectedGame.PackageRegion;
            SizeTextBlock.Text = "PKG Size: " + SelectedGame.PackageSize;
        }
    }

    private void ShowDownloadMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (DownloadsListBox.SelectedItem is not null)
        {
            Utils.OpenFolder(Utils.GetDownloadsFolderPath());
        }
    }

    private void CancelDownloadMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (DownloadsListBox.SelectedItem is not null)
        {
            PKGDownloadListViewItem SelectedDownload = (PKGDownloadListViewItem)DownloadsListBox.SelectedItem;

            if (SelectedDownload.AssociatedWebClient is not null)
            {
                if (SelectedDownload.AssociatedWebClient.IsBusy)
                {
                    SelectedDownload.AssociatedWebClient.CancelAsync();
                }
            }
        }
    }

    private async void ExtractPKGMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (DownloadsListBox.SelectedItem is not null)
        {
            PKGDownloadListViewItem SelectedDownload = (PKGDownloadListViewItem)DownloadsListBox.SelectedItem;
            string GameContentID = SelectedDownload.PackageContentID!;
            string GamezRIF = "";

            if (!string.IsNullOrEmpty(SelectedDownload.PackageDownloadDestination))
            {
                if (!File.Exists(Path.Combine(Path.GetDirectoryName(SelectedDownload.PackageDownloadDestination)!, "pkg2zip.exe")) || !File.Exists(Path.Combine(Path.GetDirectoryName(SelectedDownload.PackageDownloadDestination)!, "pkg2zip")))
                {
                    File.Copy(OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "pkg2zip.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "pkg2zip"),
                        Path.Combine(Path.GetDirectoryName(SelectedDownload.PackageDownloadDestination)!, OperatingSystem.IsWindows() ? "pkg2zip.exe" : "pkg2zip"), true);
                }
            }

            string DatabaseToLoad = "";

            if (CurrentListView is not null)
            {
                switch (CurrentListView.Name ?? "")
                {
                    case "GamesListBox":
                        {
                            DatabaseToLoad = "PSV_GAMES.tsv";
                            break;
                        }
                    case "DLCsListBox":
                        {
                            DatabaseToLoad = "PSV_DLCS.tsv";
                            break;
                        }
                    case "ThemesListBox":
                        {
                            DatabaseToLoad = "PSV_THEMES.tsv";
                            break;
                        }
                }
            }

            var box = MessageBoxManager.GetMessageBoxStandard("", "Load zRIF key from the latest database ?" + Environment.NewLine + "Selecting 'No' will use the local database file.", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box.ShowWindowDialogAsync(this);
            if (boxresult == ButtonResult.Yes)
            {
                using var NewWebClient = new HttpClient();
                string ListURL = "https://nopaystation.com/tsv/" + DatabaseToLoad;
                string GamesList = await NewWebClient.GetStringAsync(ListURL);
                string[] GamesListLines = GamesList.Split([Environment.NewLine], StringSplitOptions.None);
                foreach (string GameLine in GamesListLines.Skip(1))
                {
                    string[] SplittedValues = GameLine.Split(Convert.ToChar("\t"));
                    var AdditionalInfo = Utils.GetFileSizeAndDate(SplittedValues[8].Trim(), SplittedValues[6].Trim());
                    var NewPackage = new NPSPKG()
                    {
                        PackageName = SplittedValues[2].Trim(),
                        PackageURL = SplittedValues[3].Trim(),
                        PackageTitleID = SplittedValues[0].Trim(),
                        PackageContentID = SplittedValues[5].Trim(),
                        PackageRAP = SplittedValues[4].Trim(),
                        PackageDate = AdditionalInfo.FileDate,
                        PackageSize = AdditionalInfo.FileSize,
                        PackageRegion = SplittedValues[1].Trim()
                    };
                    if (!SplittedValues[3].Trim().Equals("MISSING", StringComparison.OrdinalIgnoreCase)) // Only add available PKGs
                    {
                        TempDownloadsList.Add(NewPackage);
                    }
                }
            }
            else if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Databases", DatabaseToLoad))) // Use local .tsv file
            {
                string[] FileReader = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "Databases", DatabaseToLoad), System.Text.Encoding.UTF8);
                foreach (string GameLine in FileReader.Skip(1)) // Skip 1st line in TSV
                {
                    string[] SplittedValues = GameLine.Split(Convert.ToChar("\t"));
                    var AdditionalInfo = Utils.GetFileSizeAndDate(SplittedValues[8], SplittedValues[6]);
                    var NewPackage = new NPSPKG()
                    {
                        PackageName = SplittedValues[2],
                        PackageURL = SplittedValues[3],
                        PackageTitleID = SplittedValues[0],
                        PackageContentID = SplittedValues[5],
                        PackageRAP = SplittedValues[4],
                        PackageDate = AdditionalInfo.FileDate,
                        PackageSize = AdditionalInfo.FileSize,
                        PackageRegion = SplittedValues[1]
                    };
                    if (!SplittedValues[3].Trim().Equals("MISSING", StringComparison.OrdinalIgnoreCase)) // Only add available PKGs
                    {
                        TempDownloadsList.Add(NewPackage);
                    }
                }
            }
            else
            {
                var box2 = MessageBoxManager.GetMessageBoxStandard("Could not load list", "No database available. Please add PS Vita TSV files to the 'Databases' directory.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box2.ShowWindowAsync();
            }

            foreach (NPSPKG AvailablePKG in TempDownloadsList)
            {
                if ((AvailablePKG.PackageContentID ?? "") == (GameContentID ?? ""))
                {
                    if (AvailablePKG.PackageRAP is not null)
                    {
                        GamezRIF = AvailablePKG.PackageRAP;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(GamezRIF))
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(SelectedDownload.PackageDownloadDestination)!); // Required to extract at pkg location

                if (!string.IsNullOrEmpty(SelectedDownload.PackageDownloadDestination) && File.Exists(SelectedDownload.PackageDownloadDestination))
                {
                    Process PKG2ZIP = new();
                    PKG2ZIP.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Path.GetDirectoryName(SelectedDownload.PackageDownloadDestination)!, "pkg2zip.exe") : Path.Combine(Path.GetDirectoryName(SelectedDownload.PackageDownloadDestination)!, "pkg2zip");
                    PKG2ZIP.StartInfo.Arguments = "-x \"" + SelectedDownload.PackageDownloadDestination + "\" \"" + GamezRIF + "\"";
                    PKG2ZIP.StartInfo.RedirectStandardOutput = true;
                    PKG2ZIP.StartInfo.RedirectStandardError = true;
                    PKG2ZIP.StartInfo.UseShellExecute = false;
                    PKG2ZIP.StartInfo.CreateNoWindow = true;
                    PKG2ZIP.Start();

                    var OutputReader = PKG2ZIP.StandardOutput;
                    string ProcessOutput = OutputReader.ReadToEnd();

                    await PKG2ZIP.WaitForExitAsync();
                    PKG2ZIP.Close();

                    if (ProcessOutput.Contains("done!"))
                    {
                        var box2 = MessageBoxManager.GetMessageBoxStandard("", "PKG extracted! Do you want to open the folder containing the extracted folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                        var boxresult2 = await box2.ShowWindowDialogAsync(this);
                        if (boxresult2 == ButtonResult.Yes)
                        {
                            Utils.OpenFolder(Path.GetDirectoryName(SelectedDownload.PackageDownloadDestination)!);
                        }
                    }
                    else
                    {
                        var box2 = MessageBoxManager.GetMessageBoxStandard("Error", "Could not extract the selected .pkg file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box2.ShowWindowAsync();
                    }

                    if (File.Exists(Path.Combine(Path.GetDirectoryName(SelectedDownload.PackageDownloadDestination)!, "pkg2zip.exe")))
                    {
                        File.Delete(Path.Combine(Path.GetDirectoryName(SelectedDownload.PackageDownloadDestination)!, "pkg2zip.exe"));
                    }
                }
                else
                {

                }

                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            }
            else
            {
                var box2 = MessageBoxManager.GetMessageBoxStandard("Error", "No zRIF found.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box2.ShowWindowAsync();
            }

        }
    }

}