using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using IronSoftware.Drawing;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using PSMultiTools.Dialogs;
using PSMultiTools.PS5.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Xilium.CefGlue.Avalonia;

namespace PSMultiTools.PS2;

public partial class PS2Library : Window
{

    private readonly AvaloniaCefBrowser PSXDatacenterBrowser = new() { Address = "about:blank" };
    public SyncWindow NewLoadingWindow = new() { Title = "Loading PS2 files", ShowActivated = true };

    public int ISOCount = 0;
    public int CSOCount = 0;

    public ObservableCollection<PS2Game> GamesList { get; } = [];
    public List<string> URLs = [];
    public int CurrentURL = 0;

    // Selected game context menu
    public ContextMenu NewContextMenu = new();
    public MenuItem CopyToMenuItem = new() { Header = "Copy to", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/copy-icon.png"))) } };
    public MenuItem SendToMenuItem = new() { Header = "Send to PS4/5", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/send.png"))) } };
    public MenuItem PlayGameMenuItem = new() { Header = "Play with PCSX2", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/controller.png"))) } };
    public MenuItem CreateProjectMenuItem = new() { Header = "Create a game project for the PSX", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/copy-icon.png"))) } };

    // Supplemental library menu items
    public MenuItem LoadFolderMenuItem = new() { Header = "Load a new folder" };
    public MenuItem LoadDLFolderMenuItem = new() { Header = "Open Downloads folder" };

    // Supplemental emulator menu item
    public MenuItem EMU_Settings = new() { Header = "PCSX2 Settings" };

    public PS2Library()
    {
        InitializeComponent();

        Loaded += PS2Library_Loaded;

        GamesListBox.PointerWheelChanged += GamesListBox_PointerWheelChanged;
        GamesListBox.SelectionChanged += GamesListBox_SelectionChanged;

        LoadFolderMenuItem.Click += LoadFolderMenuItem_Click;
        LoadDLFolderMenuItem.Click += LoadDLFolderMenuItem_Click;
        SendToMenuItem.Click += SendToMenuItem_Click;
        CopyToMenuItem.Click += CopyToMenuItem_Click;
        PlayGameMenuItem.Click += PlayGameMenuItem_Click;
        EMU_Settings.Click += EMU_Settings_Click;
        CreateProjectMenuItem.Click += CreateProjectMenuItem_Click;

        PSXDatacenterBrowser.LoadEnd += PSXDatacenterBrowser_LoadEnd;
        var browserWrapper = this.FindControl<Decorator>("PSXDatacenterBrowserWrapper");
        browserWrapper!.Child = PSXDatacenterBrowser;
    }

    private void PS2Library_Loaded(object? sender, RoutedEventArgs e)
    {
        // Set items source
        GamesListBox.ItemsSource = GamesList;

        // Set the controls in the shared library
        NewPS2Menu.GamesListBox = GamesListBox;

        // Add supplemental library menu items that will be handled in the app
        MenuItem LibraryMenuItem = (MenuItem)NewPS2Menu.MainMenu.Items[0]!;
        LibraryMenuItem.Items.Add(LoadFolderMenuItem);
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem);

        NewContextMenu.Items.Add(CopyToMenuItem);
        NewContextMenu.Items.Add(SendToMenuItem);
        NewContextMenu.Items.Add(PlayGameMenuItem);
        NewContextMenu.Items.Add(CreateProjectMenuItem);
        GamesListBox.ContextMenu = NewContextMenu;

        // Add supplemental emulator menu item
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Emulators", "PCSX2", "pcsx2.exe")))
        {
            NewPS2Menu.MainMenu.Items.Add(EMU_Settings);
        }
    }

    #region Menu Actions

    private async void LoadFolderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select your PS2 backup folder" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            ISOCount = Directory.GetFiles(FBDResult, "*.iso", SearchOption.AllDirectories).Length;
            CSOCount = Directory.GetFiles(FBDResult, "*.cso", SearchOption.AllDirectories).Length;

            NewLoadingWindow = new SyncWindow() { Title = "Loading PS2 files", ShowActivated = true };
            NewLoadingWindow.LoadProgressBar.Maximum = ISOCount;
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading ISO 1 of " + ISOCount.ToString();
            NewLoadingWindow.Show(this);

            // PS2 ISOs
            foreach (var GameISO in Directory.GetFiles(FBDResult, "*.iso", SearchOption.AllDirectories))
            {
                var NewPS2Game = new PS2Game();
                string GameID = await PS2Game.GetPS2GameIDAsync(GameISO);

                if (string.IsNullOrEmpty(GameID))
                {
                    NewPS2Game.GameFilePath = GameISO;
                    var PS2ISOFileInfo = new FileInfo(GameISO);
                    NewPS2Game.GameSize = Utils.HumanReadableBytes(PS2ISOFileInfo.Length);
                    NewPS2Game.GameID = "Unknown";
                    NewPS2Game.GameBackupType = PS2Game.GameFileType.ISO;

                    // Update progress
                    Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadProgressBar.Value += 1);
                    Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadStatusTextBlock.Text = "Loading ISO " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + ISOCount.ToString());

                    // Add to the ListView
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => GamesList.Add(NewPS2Game));
                    }
                    else
                    {
                        GamesList.Add(NewPS2Game);
                    }
                }

                else
                {
                    GameID = GameID.Replace(".", "").Replace("_", "-").Trim();

                    var PS2ISOFileInfo = new FileInfo(GameISO);
                    NewPS2Game.GameSize = Utils.HumanReadableBytes(PS2ISOFileInfo.Length);
                    NewPS2Game.GameID = GameID;
                    NewPS2Game.GameFilePath = GameISO;
                    NewPS2Game.GameBackupType = PS2Game.GameFileType.ISO;

                    if (await Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameID + ".jpg"))
                    {
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            await Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                var TempBitmapImage = await AnyBitmap.FromUriAsync(new Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameID + ".jpg", UriKind.Absolute));
                                NewPS2Game.GameCoverSource = TempBitmapImage;
                            });
                        }
                        else
                        {
                            var TempBitmapImage = await AnyBitmap.FromUriAsync(new Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameID + ".jpg", UriKind.Absolute));
                            NewPS2Game.GameCoverSource = TempBitmapImage;
                        }
                    }

                    if (await Utils.IsURLValid("https://psxdatacenter.com/psx2/games2/" + GameID + ".html"))
                    {
                        URLs.Add("https://psxdatacenter.com/psx2/games2/" + GameID + ".html");
                        NewPS2Game.GameTitle = PS2Game.GetPS2GameTitleFromDatabaseList(GameID.Replace("-", ""));
                    }
                    else
                    {
                        NewPS2Game.GameTitle = PS2Game.GetPS2GameTitleFromDatabaseList(GameID.Replace("-", ""));
                    }

                    // Update progress
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        NewLoadingWindow.LoadProgressBar.Value += 1d;
                        NewLoadingWindow.LoadStatusTextBlock.Text = "Loading ISO " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + ISOCount.ToString();
                    });

                    // Add to the ListView
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => GamesList.Add(NewPS2Game));
                    }
                    else
                    {
                        GamesList.Add(NewPS2Game);
                    }

                }
            }

            // Reset
            Dispatcher.UIThread.Invoke(() =>
            {
                NewLoadingWindow.LoadProgressBar.Value = 0;
                NewLoadingWindow.LoadProgressBar.Maximum = CSOCount;
                NewLoadingWindow.LoadStatusTextBlock.Text = "Loading CSO 1 of " + CSOCount.ToString();
            });

            // PS2 CSOs
            foreach (var GameCSO in Directory.GetFiles(FBDResult, "*.cso", SearchOption.AllDirectories))
            {
                var NewPS2Game = new PS2Game();
                string GameID = await PS2Game.GetPS2GameIDAsync(GameCSO);

                if (string.IsNullOrEmpty(GameID))
                {
                    // Add to the GamesListBox anyway
                    NewPS2Game.GameFilePath = GameCSO;
                    var PS2CSOFileInfo = new FileInfo(GameCSO);
                    NewPS2Game.GameSize = Utils.HumanReadableBytes(PS2CSOFileInfo.Length);
                    NewPS2Game.GameBackupType = PS2Game.GameFileType.CSO;

                    // Update progress
                    Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadProgressBar.Value += 1);
                    Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadStatusTextBlock.Text = "Loading CSO " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + CSOCount.ToString());

                    // Add to the ListView
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => GamesList.Add(NewPS2Game));
                    }
                    else
                    {
                        GamesList.Add(NewPS2Game);
                    }
                }
                else
                {
                    GameID = GameID.Replace(".", "").Replace("_", "-").Trim();

                    var PS2CSOFileInfo = new FileInfo(GameCSO);
                    NewPS2Game.GameSize = Utils.HumanReadableBytes(PS2CSOFileInfo.Length);
                    NewPS2Game.GameID = GameID;
                    NewPS2Game.GameFilePath = GameCSO;
                    NewPS2Game.GameBackupType = PS2Game.GameFileType.CSO;

                    if (await Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameID + ".jpg"))
                    {
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            await Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                var TempBitmapImage = await AnyBitmap.FromUriAsync(new Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameID + ".jpg", UriKind.Absolute));
                                NewPS2Game.GameCoverSource = TempBitmapImage;
                            });
                        }
                        else
                        {
                            var TempBitmapImage = await AnyBitmap.FromUriAsync(new Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameID + ".jpg", UriKind.Absolute));
                            NewPS2Game.GameCoverSource = TempBitmapImage;
                        }
                    }

                    if (await Utils.IsURLValid("https://psxdatacenter.com/psx2/games2/" + GameID + ".html"))
                    {
                        URLs.Add("https://psxdatacenter.com/psx2/games2/" + GameID + ".html");
                        NewPS2Game.GameTitle = PS2Game.GetPS2GameTitleFromDatabaseList(GameID.Replace("-", ""));
                    }
                    else
                    {
                        NewPS2Game.GameTitle = PS2Game.GetPS2GameTitleFromDatabaseList(GameID.Replace("-", ""));
                    }

                    // Update progress
                    Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadProgressBar.Value += 1d);
                    Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadStatusTextBlock.Text = "Loading CSO " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + CSOCount.ToString());

                    // Add to the ListView
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => GamesList.Add(NewPS2Game));
                    }
                    else
                    {
                        GamesList.Add(NewPS2Game);
                    }

                }
            }

            if (URLs.Count > 0)
            {
                Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadStatusTextBlock.Text = "Getting " + URLs.Count.ToString() + " available game infos.");
                Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadProgressBar.Value = 0);
                Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadProgressBar.Maximum = URLs.Count);

                PSXDatacenterBrowser.Address = URLs[0];
            }
            else
            {
                NewLoadingWindow.Close();
                Cursor = new Cursor(StandardCursorType.Arrow);
            }
        }
    }

    private void LoadDLFolderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        Utils.OpenDownloadsFolder();
    }

    #endregion

    #region Contextmenu Actions

    private async void SendToMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS2Game SelectedPS2Game = (PS2Game)GamesListBox.SelectedItem;
            var NewPS5Sender = new PS5Sender() { SelectedISO = SelectedPS2Game.GameFilePath! };
            NewPS5Sender.Show();

            var box = MessageBoxManager.GetMessageBoxStandard("Info", "Please continue with the PS5 Mast1c0re Sender and send the Network GAME Loader for your PS4/PS5 first.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    private async void CopyToMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS2Game SelectedPS2Game = (PS2Game)GamesListBox.SelectedItem;
            var FBD = new OpenFolderDialog() { Title = "Where do you want to save the selected game ?" };
            var FBDResult = await FBD.ShowAsync(this);

            if (FBDResult != null)
            {
                var NewCopyWindow = new CopyWindow()
                {
                    ShowActivated = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    BackupPath = SelectedPS2Game.GameFilePath!,
                    BackupDestinationPath = Utils.EnsureTrailingSeparator(FBDResult),
                    Title = "Copying " + SelectedPS2Game.GameTitle + " to " + FBDResult
                };

                if (await NewCopyWindow.ShowDialog<bool>(this) == true)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Completed", "Game copied with success !", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
            }

        }
    }

    private async void PlayGameMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Emulators", "PCSX2", "pcsx2.exe")))
        {
            if (GamesListBox.SelectedItem is not null)
            {
                PS2Game SelectedPS2Game = (PS2Game)GamesListBox.SelectedItem;

                // Check if any PS2 BIOS file is available
                if (!(Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Emulators", "PCSX2", "bios"), "*.bin", SearchOption.TopDirectoryOnly).Length > 0))
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Cannot launch game", "No PS2 BIOS file available." + Environment.NewLine + "You need at least one BIOS file installed in order to play " + SelectedPS2Game.GameTitle + "." + Environment.NewLine + "Do you want to copy a BIOS file to the Emulators folder of PS Multi Tools ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult = await box.ShowWindowDialogAsync(this);
                    if (boxresult == ButtonResult.Yes)
                    {

                        // Get a BIOS file from OpenFileDialog
                        var binFileFilter = new FileDialogFilter
                        {
                            Name = "BIN File",
                            Extensions = ["bin"]
                        };
                        var OFD = new OpenFileDialog() { Title = "Select a PS2 BIOS file", Filters = { binFileFilter }, AllowMultiple = false };
                        var OFDResult = await OFD.ShowAsync(this);

                        if (OFDResult != null && OFDResult.Length > 0)
                        {
                            string SelectedBIOSFile = OFDResult[0];
                            string SelectedBIOSFileName = Path.GetFileName(SelectedBIOSFile);

                            // Copy to the BIOS folder
                            File.Copy(SelectedBIOSFile, Path.Combine(Environment.CurrentDirectory, "Emulators", "PCSX2", "bios", SelectedBIOSFileName), true);

                            // Proceed
                            var box2 = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Start " + SelectedPS2Game.GameTitle + " using PCSX2 ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                            var boxresult2 = await box2.ShowWindowDialogAsync(this);
                            if (boxresult2 == ButtonResult.Yes)
                            {
                                var EmulatorLauncherStartInfo = new ProcessStartInfo();
                                var EmulatorLauncher = new Process() { StartInfo = EmulatorLauncherStartInfo };
                                EmulatorLauncherStartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Emulators", "PCSX2", "pcsx2.exe");
                                EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(Path.Combine(Environment.CurrentDirectory, "Emulators", "PCSX2", "pcsx2.exe"));
                                EmulatorLauncherStartInfo.Arguments = "\"" + SelectedPS2Game.GameFilePath + "\" --nogui --fullboot --portable";
                                EmulatorLauncher.Start();
                            }
                        }

                        else
                        {
                            var box2 = MessageBoxManager.GetMessageBoxStandard("Error", "No BIOS file specied, aborting.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box2.ShowWindowAsync();
                            return;
                        }
                    }
                    else
                    {
                        var box3 = MessageBoxManager.GetMessageBoxStandard("Error", "No BIOS file available, aborting.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box3.ShowWindowAsync();
                        return;
                    }
                }

                // Proceed
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Start " + SelectedPS2Game.GameTitle + " using PCSX2 ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult = await box.ShowWindowDialogAsync(this);
                    if (boxresult == ButtonResult.Yes)
                    {
                        var EmulatorLauncherStartInfo = new ProcessStartInfo();
                        var EmulatorLauncher = new Process() { StartInfo = EmulatorLauncherStartInfo };
                        EmulatorLauncherStartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Emulators", "PCSX2", "pcsx2.exe");
                        EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(Path.Combine(Environment.CurrentDirectory, "Emulators", "PCSX2", "pcsx2.exe"));
                        EmulatorLauncherStartInfo.Arguments = "\"" + SelectedPS2Game.GameFilePath + "\" --nogui --fullboot --portable";
                        EmulatorLauncher.Start();

                    }
                }
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Cannot start pcsx2." + Environment.NewLine + "Emulator pack is not installed.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    #endregion

    private void GamesListBox_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        var GamesListBoxScrollViewer = Utils.FindScrollViewer(GamesListBox)!;
        var delta = e.Delta.Y * 20;
        var newX = Math.Clamp(GamesListBoxScrollViewer.Offset.X + delta, 0, Math.Max(0, GamesListBoxScrollViewer.Extent.Width - GamesListBoxScrollViewer.Viewport.Width));
        GamesListBoxScrollViewer.Offset = new Avalonia.Vector(newX, GamesListBoxScrollViewer.Offset.Y);
        e.Handled = true;
    }

    private void GamesListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS2Game SelectedPS2Game = (PS2Game)GamesListBox.SelectedItem;

            GameTitleTextBlock.Text = SelectedPS2Game.GameTitle;
            GameIDTextBlock.Text = "Title ID: " + SelectedPS2Game.GameID;
            GameRegionTextBlock.Text = "Region: " + SelectedPS2Game.GameRegion;
            GameGenreTextBlock.Text = "Genre: " + SelectedPS2Game.GameGenre;
            GameDeveloperTextBlock.Text = "Developer: " + SelectedPS2Game.GameDeveloper;

            GameDescriptionTextBlock.Text = "Hover for Game Description";
            ToolTip.SetTip(GameDescriptionTextBlock, SelectedPS2Game.GameDescription);

            GameSizeTextBlock.Text = "Size: " + SelectedPS2Game.GameSize;
            GamePublisherTextBlock.Text = "Publisher: " + SelectedPS2Game.GamePublisher;
            GameReleaseDateTextBlock.Text = "Release Date: " + SelectedPS2Game.GameReleaseDate;
            GameBackupTypeTextBlock.Text = "Backup Type: " + SelectedPS2Game.GameBackupType.ToString();

            if (!string.IsNullOrEmpty(SelectedPS2Game.GameFilePath))
            {
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " + new DirectoryInfo(Path.GetDirectoryName(SelectedPS2Game.GameFilePath)!).Name;
            }
            else
            {
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " + new DirectoryInfo(SelectedPS2Game.GameFolderPath!).Name;
            }

        }
    }

    private async void PSXDatacenterBrowser_LoadEnd(object sender, Xilium.CefGlue.Common.Events.LoadEndEventArgs e)
    {
        if (e.HttpStatusCode == 200)
        {
            try
            {
                string CoverJSReturnValue = await PSXDatacenterBrowser.EvaluateJavaScript<string>("return document.getElementById('table2').getElementsByClassName('sectional')[1].querySelector('img').src");
                string TitleJSReturnValue = await PSXDatacenterBrowser.EvaluateJavaScript<string>("return document.getElementById('table4').rows[0].cells[1].textContent");
                string TitleIDJSReturnValue = await PSXDatacenterBrowser.EvaluateJavaScript<string>("return document.getElementById('table4').rows[2].cells[1].textContent");

                string RegionJSReturnValue = await PSXDatacenterBrowser.EvaluateJavaScript<string>("return document.getElementById('table4').rows[3].cells[1].textContent");
                string GenreJSReturnValue = await PSXDatacenterBrowser.EvaluateJavaScript<string>("return document.getElementById('table4').rows[4].cells[1].textContent");
                string DeveloperJSReturnValue = await PSXDatacenterBrowser.EvaluateJavaScript<string>("return document.getElementById('table4').rows[5].cells[1].textContent");
                string PublisherJSReturnValue = await PSXDatacenterBrowser.EvaluateJavaScript<string>("return document.getElementById('table4').rows[6].cells[1].textContent");
                string ReleaseDateJSReturnValue = await PSXDatacenterBrowser.EvaluateJavaScript<string>("return document.getElementById('table4').rows[7].cells[1].textContent");

                string DescriptionJSReturnValue = await PSXDatacenterBrowser.EvaluateJavaScript<string>("return document.getElementById('table16').rows[0].cells[0].textContent");

                if (TitleJSReturnValue != null && TitleIDJSReturnValue != null)
                {

                    // Trim received values
                    CoverJSReturnValue = CoverJSReturnValue.Trim();
                    TitleJSReturnValue = TitleJSReturnValue.Trim();
                    TitleIDJSReturnValue = TitleIDJSReturnValue.Trim();
                    RegionJSReturnValue = RegionJSReturnValue.Trim();
                    GenreJSReturnValue = GenreJSReturnValue.Trim();
                    DeveloperJSReturnValue = DeveloperJSReturnValue.Trim();
                    PublisherJSReturnValue = PublisherJSReturnValue.Trim();
                    ReleaseDateJSReturnValue = ReleaseDateJSReturnValue.Trim();
                    DescriptionJSReturnValue = DescriptionJSReturnValue.Trim();

                    // Some IDs are returned with additional language/disc IDs, we only want the main game ID
                    if (TitleIDJSReturnValue.Length > 10)
                    {
                        TitleIDJSReturnValue = TitleIDJSReturnValue[..10];
                    }

                    // Update values on library
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        foreach (var GameInListBox in GamesList)
                        {
                            if (GameInListBox is not PS2Game FoundGame) continue;

                            if (!string.IsNullOrEmpty(FoundGame.GameTitle) && TitleJSReturnValue != null)
                            {
                                if (FoundGame.GameTitle.Contains(TitleJSReturnValue, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    FoundGame.GameTitle = string.IsNullOrEmpty(FoundGame.GameTitle) ? TitleJSReturnValue : FoundGame.GameTitle;
                                    FoundGame.GameID = string.IsNullOrEmpty(FoundGame.GameID) ? TitleIDJSReturnValue : FoundGame.GameID;
                                    FoundGame.GameRegion = string.IsNullOrEmpty(FoundGame.GameReleaseDate) ? RegionJSReturnValue : FoundGame.GameRegion;
                                    FoundGame.GameGenre = string.IsNullOrEmpty(FoundGame.GameGenre) ? GenreJSReturnValue : FoundGame.GameGenre;
                                    FoundGame.GameDeveloper = string.IsNullOrEmpty(FoundGame.GameDeveloper) ? DeveloperJSReturnValue : FoundGame.GameDeveloper;
                                    FoundGame.GamePublisher = string.IsNullOrEmpty(FoundGame.GamePublisher) ? PublisherJSReturnValue : FoundGame.GamePublisher;
                                    FoundGame.GameReleaseDate = string.IsNullOrEmpty(FoundGame.GameReleaseDate) ? ReleaseDateJSReturnValue : FoundGame.GameReleaseDate;
                                    FoundGame.GameDescription = string.IsNullOrEmpty(FoundGame.GameDescription) ? DescriptionJSReturnValue : FoundGame.GameDescription;

                                    if (FoundGame.GameCoverSource is null && !string.IsNullOrEmpty(CoverJSReturnValue))
                                    {
                                        try
                                        {
                                            FoundGame.GameCoverSource = await AnyBitmap.FromUriAsync(new Uri(CoverJSReturnValue), true);
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
            catch (Exception ex)
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Info", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                });
            }

            if (CurrentURL < URLs.Count)
            {
                PSXDatacenterBrowser.Address = URLs[CurrentURL];
                CurrentURL += 1;
                Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadProgressBar.Value = CurrentURL);
            }
            else
            {
                CurrentURL = 0;
                URLs.Clear();
                Dispatcher.UIThread.Invoke(() => NewLoadingWindow.Close());
                Dispatcher.UIThread.Invoke(() => Cursor = new Cursor(StandardCursorType.Arrow));
            }
        }
    }

    private void EMU_Settings_Click(object? sender, RoutedEventArgs e)
    {
        //var NewPS2EmulatorSettingsWindow = new PS2EmulatorSettings() { ShowActivated = true };
        //NewPS2EmulatorSettingsWindow.Show();
    }

    private async void CreateProjectMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        //if (GamesListBox.SelectedItem is not null)
        //{
        //    PS2Game SelectedPS2Game = (PS2Game)GamesListBox.SelectedItem;
        //    //string GameProjectDirectory = SelectedPS2Game.GameTitle + " [" + SelectedPS2Game.GameID + "]";
        //    string NewGameProjectDirectory = Environment.CurrentDirectory + @"\Projects\" + SelectedPS2Game.GameTitle + " [" + SelectedPS2Game.GameID + "]";

        //    var NewGameProjectWindow = new PSXNewPS2GameProject() { ShowActivated = true };
        //    var NewGameEditor = new PSXPS2GameEditor() { ProjectDirectory = NewGameProjectDirectory, Title = "Game Ressources Editor - " + NewGameProjectDirectory };

        //    // Set project information
        //    NewGameProjectWindow.ImportFromPSMT(SelectedPS2Game.GameFilePath, SelectedPS2Game.GameTitle, NewGameProjectDirectory, SelectedPS2Game.GameID.Replace("-", "_").Insert(8, "."));

        //    // Create game project directory
        //    if (!Directory.Exists(NewGameProjectDirectory))
        //    {
        //        Directory.CreateDirectory(NewGameProjectDirectory);
        //    }

        //    // Write Project settings to .CFG
        //    using (var ProjectWriter = new StreamWriter(Environment.CurrentDirectory + @"\Projects\" + SelectedPS2Game.GameTitle + ".CFG", false))
        //    {
        //        ProjectWriter.WriteLine("TITLE=" + SelectedPS2Game.GameTitle);
        //        ProjectWriter.WriteLine("ID=" + SelectedPS2Game.GameID.Replace("-", "_").Insert(8, "."));
        //        ProjectWriter.WriteLine("DIR=" + NewGameProjectDirectory);
        //        ProjectWriter.WriteLine("ELForISO=" + SelectedPS2Game.GameFilePath);
        //        ProjectWriter.WriteLine("TYPE=GAME");
        //        ProjectWriter.WriteLine("SIGNED=FALSE");
        //        ProjectWriter.WriteLine("GAMETYPE=PS2");
        //    }

        //    // Write SYSTEM.CNF to project directory
        //    using (var CNFWriter = new StreamWriter(NewGameProjectDirectory + @"\SYSTEM.CNF", false))
        //    {
        //        CNFWriter.WriteLine("BOOT2 = pfs:/EXECUTE.KELF"); // Loads EXECUTE.KELF
        //        CNFWriter.WriteLine("VER = 1.01");
        //        CNFWriter.WriteLine("VMODE = NTSC");
        //        CNFWriter.WriteLine("HDDUNITPOWER = NICHDD");
        //    }

        //    // Write icon.sys to project directory
        //    using (var CNFWriter = new StreamWriter(NewGameProjectDirectory + @"\icon.sys", false))
        //    {
        //        CNFWriter.WriteLine("PS2X");
        //        CNFWriter.WriteLine("title0=" + SelectedPS2Game.GameTitle);
        //        CNFWriter.WriteLine("title1=" + SelectedPS2Game.GameID);
        //        CNFWriter.WriteLine("bgcola=0");
        //        CNFWriter.WriteLine("bgcol0=0,0,0");
        //        CNFWriter.WriteLine("bgcol1=0,0,0");
        //        CNFWriter.WriteLine("bgcol2=0,0,0");
        //        CNFWriter.WriteLine("bgcol3=0,0,0");
        //        CNFWriter.WriteLine("lightdir0=1.0,-1.0,1.0");
        //        CNFWriter.WriteLine("lightdir1=-1.0,1.0,-1.0");
        //        CNFWriter.WriteLine("lightdir2=0.0,0.0,0.0");
        //        CNFWriter.WriteLine("lightcolamb=64,64,64");
        //        CNFWriter.WriteLine("lightcol0=64,64,64");
        //        CNFWriter.WriteLine("lightcol1=16,16,16");
        //        CNFWriter.WriteLine("lightcol2=0,0,0");
        //        CNFWriter.WriteLine("uninstallmes0=Do you want to uninstall this game ?");
        //        CNFWriter.WriteLine("uninstallmes1=");
        //        CNFWriter.WriteLine("uninstallmes2=");
        //    }

        //    // Create game project res & image directory
        //    if (!Directory.Exists(NewGameProjectDirectory + @"\res"))
        //    {
        //        Directory.CreateDirectory(NewGameProjectDirectory + @"\res");
        //    }
        //    if (!Directory.Exists(NewGameProjectDirectory + @"\res\image"))
        //    {
        //        Directory.CreateDirectory(NewGameProjectDirectory + @"\res\image");
        //    }

        //    // Write info.sys to res directory
        //    using (var SYSWriter = new StreamWriter(NewGameProjectDirectory + @"\res\info.sys", false))
        //    {
        //        SYSWriter.WriteLine("title = " + SelectedPS2Game.GameTitle);
        //        SYSWriter.WriteLine("title_id = " + SelectedPS2Game.GameID);
        //        SYSWriter.WriteLine("title_sub_id = 0");
        //        SYSWriter.WriteLine("release_date = " + SelectedPS2Game.GameReleaseDate);
        //        SYSWriter.WriteLine("developer_id = " + SelectedPS2Game.GameDeveloper);
        //        SYSWriter.WriteLine("publisher_id = " + SelectedPS2Game.GamePublisher);
        //        SYSWriter.WriteLine("note = ");
        //        SYSWriter.WriteLine("content_web = " + SelectedPS2Game.GameWebsite);
        //        SYSWriter.WriteLine("image_topviewflag = 0");
        //        SYSWriter.WriteLine("image_type = 0");
        //        SYSWriter.WriteLine("image_count = 1");
        //        SYSWriter.WriteLine("image_viewsec = 600");
        //        SYSWriter.WriteLine("copyright_viewflag = 0");
        //        SYSWriter.WriteLine("copyright_imgcount = 1");
        //        SYSWriter.WriteLine("genre = " + SelectedPS2Game.GameGenre);
        //        SYSWriter.WriteLine("parental_lock = 1");
        //        SYSWriter.WriteLine("effective_date = 0");
        //        SYSWriter.WriteLine("expire_date = 0");

        //        switch (SelectedPS2Game.GameRegion ?? "")
        //        {
        //            case "Europe":
        //                {
        //                    SYSWriter.WriteLine("area = E");
        //                    break;
        //                }
        //            case "US":
        //                {
        //                    SYSWriter.WriteLine("area = U");
        //                    break;
        //                }
        //            case "Japan":
        //                {
        //                    SYSWriter.WriteLine("area = J");
        //                    break;
        //                }

        //            default:
        //                {
        //                    SYSWriter.WriteLine("area = J");
        //                    break;
        //                }
        //        }

        //        SYSWriter.WriteLine("violence_flag = 0");
        //        SYSWriter.WriteLine("content_type = 255");
        //        SYSWriter.WriteLine("content_subtype = 0");
        //    }

        //    // Create man.xml
        //    using (var MANWriter = new StreamWriter(NewGameProjectDirectory + @"\res\man.xml", false))
        //    {
        //        MANWriter.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        //        MANWriter.WriteLine("");
        //        MANWriter.WriteLine("<MANUAL version=\"1.0\">");
        //        MANWriter.WriteLine("");
        //        MANWriter.WriteLine("<IMG id=\"bg\" src=\"./image/0.png\" />");
        //        MANWriter.WriteLine("");
        //        MANWriter.WriteLine("<MENUGROUP id=\"TOP\">");
        //        MANWriter.WriteLine("<TITLE id=\"TOP-TITLE\" label=\"" + SelectedPS2Game.GameTitle + "\" />");
        //        MANWriter.WriteLine("<ITEM id=\"M00\" label=\"Screenshots\"	page=\"PIC0000\" />");
        //        MANWriter.WriteLine("</MENUGROUP>");
        //        MANWriter.WriteLine("");
        //        MANWriter.WriteLine("<PAGEGROUP>");
        //        MANWriter.WriteLine("<PAGE id=\"PIC0000\" src=\"./image/1.png\" retitem=\"M00\" retgroup=\"TOP\" />");
        //        MANWriter.WriteLine("<PAGE id=\"PIC0000\" src=\"./image/2.png\" retitem=\"M00\" retgroup=\"TOP\" />");
        //        MANWriter.WriteLine("</PAGEGROUP>");
        //        MANWriter.WriteLine("</MANUAL>");
        //        MANWriter.WriteLine("");
        //    }

        //    // Open project settings window
        //    NewGameProjectWindow.Show();

        //    // Open the Game Editor (in case of additional changes)
        //    NewGameEditor.Show();
        //    NewGameEditor.AutoSave = true;

        //    // Open the Game Editor and try to load values from PSXDatacenter
        //    if (await Utils.IsURLValid("https://psxdatacenter.com/psx2/games2/" + SelectedPS2Game.GameID + ".html"))
        //    {
        //        NewGameEditor.PSXDatacenterBrowser.Navigate("https://psxdatacenter.com/psx2/games2/" + SelectedPS2Game.GameID + ".html");
        //    }
        //    else
        //    {
        //        // Apply cover, title and region only if no data is available on PSXDatacenter
        //        NewGameEditor.ApplyKnownValues(SelectedPS2Game.GameID, SelectedPS2Game.GameTitle);
        //    }

        //}
    }

}