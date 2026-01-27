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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xilium.CefGlue.Avalonia;

namespace PSMultiTools.PS1;

public partial class PS1Library : Window
{

    private readonly AvaloniaCefBrowser PSXDatacenterBrowser = new() { Address = "about:blank" };
    private SyncWindow NewLoadingWindow = new() { Title = "Loading PS1 files", ShowActivated = true };

    public ObservableCollection<PS1Game> GamesList { get; } = [];
    public List<string> URLs = [];
    public int CurrentURL = 0;

    public int BINCount = 0;
    public int VCDCount = 0;

    // Selected game context menu
    private ContextMenu NewContextMenu = new();
    private MenuItem CopyToMenuItem = new() { Header = "Copy to", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/copy-icon.png"))) } };
    private MenuItem PlayGameMenuItem = new() { Header = "Play with ePSXe", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/controller.png"))) } };
    private MenuItem CreateProjectMenuItem = new() { Header = "Create a game project for the PSX", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/copy-icon.png"))) } };

    // Supplemental library menu items
    private MenuItem LoadFolderMenuItem = new() { Header = "Load a new folder" };
    private MenuItem LoadDLFolderMenuItem = new() { Header = "Open Downloads folder" };

    // Supplemental emulator menu item
    private MenuItem EMU_Settings = new() { Header = "ePSXe Settings" };

    public PS1Library()
    {
        InitializeComponent();

        Loaded += PS1Library_Loaded;

        GamesListBox.PointerWheelChanged += GamesListBox_PointerWheelChanged;
        GamesListBox.SelectionChanged += GamesListBox_SelectionChanged;

        LoadFolderMenuItem.Click += LoadFolderMenuItem_Click;
        LoadDLFolderMenuItem.Click += LoadDLFolderMenuItem_Click;
        CopyToMenuItem.Click += CopyToMenuItem_Click;
        PlayGameMenuItem.Click += PlayGameMenuItem_Click;
        EMU_Settings.Click += EMU_Settings_Click;
        CreateProjectMenuItem.Click += CreateProjectMenuItem_Click;

        PSXDatacenterBrowser.LoadEnd += PSXDatacenterBrowser_LoadEnd;
        var browserWrapper = this.FindControl<Decorator>("PSXDatacenterBrowserWrapper");
        browserWrapper!.Child = PSXDatacenterBrowser;
    }

    private void PS1Library_Loaded(object? sender, RoutedEventArgs e)
    {
        // Set items source
        GamesListBox.ItemsSource = GamesList;

        // Set the controls in the shared library
        NewPS1Menu.GamesListBox = GamesListBox;

        // Add supplemental library menu items that will be handled in the app
        MenuItem LibraryMenuItem = (MenuItem)NewPS1Menu.MainMenu.Items[0]!;
        LibraryMenuItem.Items.Add(LoadFolderMenuItem);
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem);

        NewContextMenu.Items.Add(CopyToMenuItem);
        NewContextMenu.Items.Add(PlayGameMenuItem);
        NewContextMenu.Items.Add(CreateProjectMenuItem);

        GamesListBox.ContextMenu = NewContextMenu;

        // Add supplemental emulator menu item
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Emulators", "ePSXe", "ePSXe.exe")))
        {
            NewPS1Menu.MainMenu.Items.Add(EMU_Settings);
        }
    }

    #region Menu Actions

    private async void LoadFolderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select your PS1 backup folder" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {

            Cursor = new Cursor(StandardCursorType.Wait);

            // Reset
            GamesList.Clear();
            URLs.Clear();

            string[] BINFiles = Directory.GetFiles(FBDResult, "*.bin", SearchOption.AllDirectories);
            List<string> FilteredBINFiles = [];
            string[] VCDFiles = Directory.GetFiles(FBDResult, "*.vcd", SearchOption.AllDirectories);
            List<string> FailList = [];

            // Skip multiple "Track" files and only add the first one
            foreach (var GameBIN in BINFiles)
            {
                var GameInfo = new FileInfo(GameBIN);
                if (GameInfo.Name.Contains("track", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (GameBIN.Contains("(track 1)", StringComparison.CurrentCultureIgnoreCase))
                    {
                        FilteredBINFiles.Add(GameBIN);
                    }
                    else if (GameInfo.Name.Contains("(track 01)", StringComparison.CurrentCultureIgnoreCase))
                    {
                        FilteredBINFiles.Add(GameBIN);
                    }
                    else
                    {
                        // Skip
                        continue;
                    }
                }
                else
                {
                    FilteredBINFiles.Add(GameBIN);
                }
            }

            // Merge the list of VCDFiles & FilteredBINFiles
            IEnumerable<string> AllFoundFiles = FilteredBINFiles.Concat(VCDFiles);

            NewLoadingWindow = new SyncWindow() { Title = "Loading PS1 files", ShowActivated = true };
            NewLoadingWindow.LoadProgressBar.Maximum = AllFoundFiles.Count();
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + AllFoundFiles.Count();
            NewLoadingWindow.Show(this);

            foreach (var Game in AllFoundFiles)
            {
                var GameInfo = new FileInfo(Game);
                string GameStartLetter = GameInfo.Name[..1]; // Take the first letter of the file name (required to browse PSXDatacenter)
                var NewPS1Game = new PS1Game() { GameFilePath = Game, GameSize = Utils.HumanReadableBytes(GameInfo.Length) };

                var RetrievedBOOTValue = FindBootValue(Game);
                bool GameIDFound = false;

                if (RetrievedBOOTValue != null)
                {
                    // Game ID found
                    GameIDFound = true;

                    string GameID = RetrievedBOOTValue.Replace(@"BOOT = cdrom:\", "").Replace(@"BOOT=cdrom:\", "").Replace("BOOT = cdrom:", "").Replace(";1", "").Replace("_", "-").Replace(".", "").Replace(@"MGS\", "").Replace("cdrom:\\", "").Replace("cdrom:", "").Trim();
                    string RegionCharacter = PS1Game.GetRegionChar(GameID);

                    if (GameID != null && RegionCharacter != null)
                    {
                        // Set known values
                        NewPS1Game.GameID = GameID.ToUpper();

                        // Check game id length & if the generated url is valid
                        if (GameID.Length == 10 && GameID.Contains('-'))
                        {
                            if (await Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS1/" + GameID + ".jpg"))
                            {
                                if (Dispatcher.UIThread.CheckAccess() == false)
                                {
                                    await Dispatcher.UIThread.InvokeAsync(async () =>
                                    {
                                        AnyBitmap TempBitmapImage = await AnyBitmap.FromUriAsync(new Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS1/" + GameID + ".jpg"), true);
                                        NewPS1Game.GameCoverSource = TempBitmapImage;
                                    });
                                }
                                else
                                {
                                    AnyBitmap TempBitmapImage = await AnyBitmap.FromUriAsync(new Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS1/" + GameID + ".jpg", UriKind.Absolute), true);
                                    NewPS1Game.GameCoverSource = TempBitmapImage;
                                }
                            }

                            if (await Utils.IsURLValid("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html"))
                            {
                                URLs.Add("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html");
                                NewPS1Game.GameTitle = PS1Game.GetPS1GameTitleFromDatabaseList(GameID.ToUpper().Trim());
                            }
                            else
                            {
                                NewPS1Game.GameTitle = PS1Game.GetPS1GameTitleFromDatabaseList(GameID.ToUpper().Trim());
                            }
                        }
                    }
                    else
                    {
                        NewPS1Game.GameTitle = GameInfo.Name;
                    }
                }
                else
                {
                    NewPS1Game.GameTitle = GameInfo.Name;
                }

                // Update progress
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        NewLoadingWindow.LoadProgressBar.Value += 1;
                        NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + AllFoundFiles.Count();
                    });
                }
                else
                {
                    NewLoadingWindow.LoadProgressBar.Value += 1;
                    NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + AllFoundFiles.Count();
                }

                if (GameIDFound == false)
                {
                    FailList.Add(Game);
                }
                else
                {
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            GamesList.Add(NewPS1Game);
                        });
                    }
                    else
                    {
                        GamesList.Add(NewPS1Game);
                    }
                }
            }

            if (FailList.Count > 0) // Ask for an extended search
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Not all Game IDs could be found", "Some Game IDs could not be found quickly, do you want to extend the search ? This requires about 1-5min for each game (depending on your hardware).", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                var boxresult = await box.ShowWindowDialogAsync(this);
                if (boxresult == ButtonResult.Yes)
                {

                    // Update progress
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        NewLoadingWindow.LoadProgressBar.Value = 0;
                        NewLoadingWindow.LoadProgressBar.Maximum = FailList.Count;
                        NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + FailList.Count.ToString();
                    });

                    foreach (var Game in FailList)
                    {

                        var GameInfo = new FileInfo(Game);

                        // Skip multiple "Track" files and only read the first one
                        if (GameInfo.Name.Contains("track", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (!Game.Contains("(track 1).bin", StringComparison.CurrentCultureIgnoreCase) || !GameInfo.Name.Contains("(track 01).bin", StringComparison.CurrentCultureIgnoreCase))
                            {
                                // Skip
                                continue;
                            }
                        }

                        string GameStartLetter = GameInfo.Name[..1]; // Take the first letter of the file name (required to browse PSXDatacenter)
                        var NewPS1Game = new PS1Game() { GameFilePath = Game, GameSize = Utils.HumanReadableBytes(GameInfo.Length) };
                        string GameFileSizeAsString = GameInfo.Length.ToString();

                        var RetrievedBOOTValue = ExtendedFindBootValue(Game);

                        if (RetrievedBOOTValue != null) // Game ID found
                        {
                            if (RetrievedBOOTValue.Contains("BOOT =") | RetrievedBOOTValue.Contains("BOOT="))
                            {
                                string GameID = RetrievedBOOTValue.Replace(@"BOOT = cdrom:\", "").Replace(@"BOOT=cdrom:\", "").Replace("BOOT = cdrom:", "").Replace(";1", "").Replace("_", "-").Replace(".", "").Replace(@"MGS\", "").Trim();
                                string RegionCharacter = PS1Game.GetRegionChar(GameID);

                                // Set known values
                                NewPS1Game.GameID = GameID.ToUpper();

                                // Check game id length & if the generated url is valid
                                if (GameID.Length == 10 && GameID.Contains('-'))
                                {
                                    if (await Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS1/" + GameID + ".jpg"))
                                    {
                                        if (Dispatcher.UIThread.CheckAccess() == false)
                                        {
                                            await Dispatcher.UIThread.InvokeAsync(async () =>
                                            {
                                                AnyBitmap TempBitmapImage = await AnyBitmap.FromUriAsync(new Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS1/" + GameID + ".jpg"), true);
                                                NewPS1Game.GameCoverSource = TempBitmapImage;
                                            });
                                        }
                                        else
                                        {
                                            AnyBitmap TempBitmapImage = await AnyBitmap.FromUriAsync(new Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS1/" + GameID + ".jpg", UriKind.Absolute), true);
                                            NewPS1Game.GameCoverSource = TempBitmapImage;
                                        }
                                    }

                                    if (await Utils.IsURLValid("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html"))
                                    {
                                        URLs.Add("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html");
                                    }
                                    else
                                    {
                                        NewPS1Game.GameTitle = PS1Game.GetPS1GameTitleFromDatabaseList(GameID.ToUpper().Trim());
                                    }
                                }
                                break;
                            }
                            else
                            {
                                NewPS1Game.GameTitle = GameInfo.Name;
                            }
                        }
                        else
                        {
                            NewPS1Game.GameTitle = GameInfo.Name;
                        }

                        // Update progress
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            NewLoadingWindow.LoadProgressBar.Value += 1;
                            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + FailList.Count.ToString();
                        });

                        // Add to the ListView
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                GamesList.Add(NewPS1Game);
                            });
                        }
                        else
                        {
                            GamesList.Add(NewPS1Game);
                        }
                    }

                }
            }

            if (URLs.Count > 0)
            {
                Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadStatusTextBlock.Text = "Getting " + URLs.Count.ToString() + " available game infos and missing covers.");
                Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadProgressBar.Value = 0);
                Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadProgressBar.Maximum = URLs.Count);

                PSXDatacenterBrowser.Address = URLs[0];
            }
            else
            {
                Dispatcher.UIThread.Invoke(() => NewLoadingWindow.Close());
                Dispatcher.UIThread.Invoke(() => Cursor = new Cursor(StandardCursorType.Arrow));
            }
        }
    }

    private void LoadDLFolderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        Utils.OpenDownloadsFolder();
    }

    #endregion

    #region Contextmenu Actions

    private async void CopyToMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS1Game SelectedPS1Game = (PS1Game)GamesListBox.SelectedItem;
            var FBD = new OpenFolderDialog() { Title = "Where do you want to save the selected game ?" };
            var FBDResult = await FBD.ShowAsync(this);

            if (FBDResult != null)
            {
                var NewCopyWindow = new CopyWindow()
                {
                    ShowActivated = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    BackupPath = SelectedPS1Game.GameFilePath!,
                    BackupDestinationPath = Utils.EnsureTrailingSeparator(FBDResult),
                    Title = "Copying " + SelectedPS1Game.GameID + " to " + FBDResult
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
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Emulators", "ePSXe", "ePSXe.exe")))
        {
            if (GamesListBox.SelectedItem is not null)
            {
                PS1Game SelectedPS1Game = (PS1Game)GamesListBox.SelectedItem;

                // Check if any PS1 BIOS file is available
                if (!(Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Emulators", "ePSXe", "bios"), "*.bin", SearchOption.TopDirectoryOnly).Length > 0))
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Cannot launch game", "No PS1 BIOS file available." + Environment.NewLine + "You need at least one BIOS file installed in order to play " + SelectedPS1Game.GameTitle + "." + Environment.NewLine + "Do you want to copy a BIOS file to the Emulators folder of PS Multi Tools ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult = await box.ShowWindowDialogAsync(this);
                    if (boxresult == ButtonResult.Yes)
                    {
                        // Get a BIOS file from OpenFileDialog
                        var binFileFilter = new FileDialogFilter
                        {
                            Name = "BIN File",
                            Extensions = ["bin"]
                        };
                        var OFD = new OpenFileDialog() { Title = "Select a PS1 BIOS file", Filters = { binFileFilter }, AllowMultiple = false };
                        var OFDResult = await OFD.ShowAsync(this);

                        if (OFDResult != null && OFDResult.Length > 0)
                        {
                            string SelectedBIOSFile = OFDResult[0];
                            string SelectedBIOSFileName = Path.GetFileName(SelectedBIOSFile);

                            // Copy to the BIOS folder
                            File.Copy(SelectedBIOSFile, Path.Combine(Environment.CurrentDirectory, "Emulators", "ePSXe", "bios") + SelectedBIOSFileName, true);

                            // Proceed
                            var box2 = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Start " + SelectedPS1Game.GameTitle + " using ePSXe ?" + Environment.NewLine + Environment.NewLine + @"If the game doesn't start then you have to set the BIOS manually using ePSXe.exe in \Emulators\ePSXe (Config -> BIOS).", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                            var boxresult2 = await box2.ShowWindowDialogAsync(this);
                            if (boxresult2 == ButtonResult.Yes)
                            {
                                var EmulatorLauncherStartInfo = new ProcessStartInfo();
                                var EmulatorLauncher = new Process() { StartInfo = EmulatorLauncherStartInfo };
                                EmulatorLauncherStartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Emulators", "ePSXe", "ePSXe.exe");
                                EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(Path.Combine(Environment.CurrentDirectory, "Emulators", "ePSXe", "ePSXe.exe"));
                                EmulatorLauncherStartInfo.Arguments = "-nogui -loadbin \"" + SelectedPS1Game.GameFilePath + "\"";
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
                    var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Start " + SelectedPS1Game.GameTitle + " using ePSXe ?" + Environment.NewLine + Environment.NewLine + @"If the game doesn't start then you have to set the BIOS manually using ePSXe.exe in \Emulators\ePSXe (Config -> BIOS).", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult = await box.ShowWindowDialogAsync(this);
                    if (boxresult == ButtonResult.Yes)
                    {
                        var EmulatorLauncherStartInfo = new ProcessStartInfo();
                        var EmulatorLauncher = new Process() { StartInfo = EmulatorLauncherStartInfo };
                        EmulatorLauncherStartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Emulators", "ePSXe", "ePSXe.exe");
                        EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(Path.Combine(Environment.CurrentDirectory, "Emulators", "ePSXe", "ePSXe.exe"));
                        EmulatorLauncherStartInfo.Arguments = "-nogui -loadbin \"" + SelectedPS1Game.GameFilePath + "\"";
                        EmulatorLauncher.Start();

                    }
                }
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Cannot start ePSXe." + Environment.NewLine + "Emulator pack is not installed.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
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

    private async void GamesListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS1Game SelectedPS1Game = (PS1Game)GamesListBox.SelectedItem;

            GameTitleTextBlock.Text = SelectedPS1Game.GameTitle;
            GameIDTextBlock.Text = "Title ID: " + SelectedPS1Game.GameID;
            GameRegionTextBlock.Text = "Region: " + SelectedPS1Game.GameRegion;
            GameGenreTextBlock.Text = "Genre: " + SelectedPS1Game.GameGenre;
            GameDeveloperTextBlock.Text = "Developer: " + SelectedPS1Game.GameDeveloper;

            GameDescriptionTextBlock.Text = "Hover for Game Description";
            ToolTip.SetTip(GameDescriptionTextBlock, SelectedPS1Game.GameDescription);

            GameSizeTextBlock.Text = "Size: " + SelectedPS1Game.GameSize;
            GamePublisherTextBlock.Text = "Publisher: " + SelectedPS1Game.GamePublisher;
            GameReleaseDateTextBlock.Text = "Release Date: " + SelectedPS1Game.GameReleaseDate;

            if (!string.IsNullOrEmpty(SelectedPS1Game.GameFilePath))
            {
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " + new DirectoryInfo(Path.GetDirectoryName(SelectedPS1Game.GameFilePath)!).Name;
            }
            else
            {
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " + new DirectoryInfo(SelectedPS1Game.GameFolderPath!).Name;
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
                            if (GameInListBox is not PS1Game FoundGame) continue;

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
        //var NewPS1EmulatorSettingsWindow = new PS1EmulatorSettings() { ShowActivated = true };
        //NewPS1EmulatorSettingsWindow.Show();
    }

    private async void CreateProjectMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            //PS1Game SelectedPS1Game = (PS1Game)GamesListBox.SelectedItem;

            //if (Path.GetExtension(SelectedPS1Game.GameFilePath) == ".VCD")
            //{
            //    //string GameProjectDirectory = SelectedPS1Game.GameTitle + " [" + SelectedPS1Game.GameID + "]";
            //    string NewGameProjectDirectory = Environment.CurrentDirectory + @"\Projects\" + SelectedPS1Game.GameTitle + " [" + SelectedPS1Game.GameID + "]";

            //    var NewGameProjectWindow = new PSXNewPS1GameProject() { ShowActivated = true };
            //    var NewGameEditor = new PSXPS1GameEditor() { ProjectDirectory = NewGameProjectDirectory, Title = "Game Ressources Editor - " + NewGameProjectDirectory };

            //    // Set project information
            //    NewGameProjectWindow.ImportFromPSMT(SelectedPS1Game.GameFilePath, SelectedPS1Game.GameTitle, NewGameProjectDirectory, SelectedPS1Game.GameID);

            //    // Create game project directory
            //    if (!Directory.Exists(NewGameProjectDirectory))
            //    {
            //        Directory.CreateDirectory(NewGameProjectDirectory);
            //    }

            //    // Write Project settings to .CFG
            //    using (var ProjectWriter = new StreamWriter(Environment.CurrentDirectory + @"\Projects\" + SelectedPS1Game.GameTitle + ".CFG", false))
            //    {
            //        ProjectWriter.WriteLine("TITLE=" + SelectedPS1Game.GameTitle);
            //        ProjectWriter.WriteLine("ID=" + SelectedPS1Game.GameID);
            //        ProjectWriter.WriteLine("DIR=" + NewGameProjectDirectory);
            //        ProjectWriter.WriteLine("ELForISO=" + SelectedPS1Game.GameFilePath);
            //        ProjectWriter.WriteLine("TYPE=GAME");
            //        ProjectWriter.WriteLine("SIGNED=FALSE");
            //        ProjectWriter.WriteLine("GAMETYPE=PS1");
            //    }

            //    // Write SYSTEM.CNF to project directory
            //    using (var CNFWriter = new StreamWriter(NewGameProjectDirectory + @"\SYSTEM.CNF", false))
            //    {
            //        CNFWriter.WriteLine("BOOT2 = pfs:/EXECUTE.KELF");
            //        CNFWriter.WriteLine("VER = 1.01");
            //        CNFWriter.WriteLine("VMODE = NTSC");
            //        CNFWriter.WriteLine("HDDUNITPOWER = NICHDD");
            //    }

            //    // Write icon.sys to project directory
            //    using (var CNFWriter = new StreamWriter(NewGameProjectDirectory + @"\icon.sys", false))
            //    {
            //        CNFWriter.WriteLine("PS2X");
            //        CNFWriter.WriteLine("title0=" + SelectedPS1Game.GameTitle);
            //        CNFWriter.WriteLine("title1=" + SelectedPS1Game.GameID);
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
            //        SYSWriter.WriteLine("title = " + SelectedPS1Game.GameTitle);
            //        SYSWriter.WriteLine("title_id = " + SelectedPS1Game.GameID);
            //        SYSWriter.WriteLine("title_sub_id = 0");
            //        SYSWriter.WriteLine("release_date = " + SelectedPS1Game.GameReleaseDate);
            //        SYSWriter.WriteLine("developer_id = " + SelectedPS1Game.GameDeveloper);
            //        SYSWriter.WriteLine("publisher_id = " + SelectedPS1Game.GamePublisher);
            //        SYSWriter.WriteLine("note = ");
            //        SYSWriter.WriteLine("content_web = ");
            //        SYSWriter.WriteLine("image_topviewflag = 0");
            //        SYSWriter.WriteLine("image_type = 0");
            //        SYSWriter.WriteLine("image_count = 1");
            //        SYSWriter.WriteLine("image_viewsec = 600");
            //        SYSWriter.WriteLine("copyright_viewflag = 0");
            //        SYSWriter.WriteLine("copyright_imgcount = 1");
            //        SYSWriter.WriteLine("genre = " + SelectedPS1Game.GameGenre);
            //        SYSWriter.WriteLine("parental_lock = 1");
            //        SYSWriter.WriteLine("effective_date = 0");
            //        SYSWriter.WriteLine("expire_date = 0");

            //        switch (SelectedPS1Game.GameRegion ?? "")
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
            //        MANWriter.WriteLine("<TITLE id=\"TOP-TITLE\" label=\"" + SelectedPS1Game.GameTitle + "\" />");
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
            //    string GameStartLetter = SelectedPS1Game.GameTitle[..1];
            //    string RegionCharacter = PS1Game.GetRegionChar(SelectedPS1Game.GameID);

            //    if (await Utils.IsURLValid("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + SelectedPS1Game.GameID + ".html"))
            //    {
            //        NewGameEditor.PSXDatacenterBrowser.Navigate("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + SelectedPS1Game.GameID + ".html");
            //    }
            //    else
            //    {
            //        // Apply cover, title and region only if no data is available on PSXDatacenter
            //        NewGameEditor.ApplyKnownValues(SelectedPS1Game.GameID, SelectedPS1Game.GameTitle);
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Games in BIN format cannot be installed directly on the HDD, please convert it with cue2pops using PS Multi Tools.", "BIN files not supported", MessageBoxButton.OK, MessageBoxImage.Information);
            //}

        }
    }

    //private static async Task<string[]> GetBINOutputAsync(string FilePath)
    //{
    //    using var WindowsCMD = new Process();

    //    if (OperatingSystem.IsWindows())
    //    {
    //        WindowsCMD.StartInfo.FileName = "cmd";
    //        WindowsCMD.StartInfo.Arguments = $"/c strings.exe /accepteula -nobanner -b 7340032 \"{FilePath}\" | findstr BOOT";
    //    }
    //    else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
    //    {
    //        WindowsCMD.StartInfo.FileName = "/bin/bash";
    //        WindowsCMD.StartInfo.Arguments = $"-c dd if=\"{FilePath}\" bs=1048576 count=7 2>/dev/null | strings | LANG=C grep -F \"BOOT\"";
    //    }
    //    else if (OperatingSystem.IsMacOS())
    //    {
    //        WindowsCMD.StartInfo.FileName = "/bin/sh";
    //        WindowsCMD.StartInfo.Arguments = $"-c \"dd if='{FilePath}' bs=1048576 count=7 2>/dev/null | strings | LANG=C grep -F 'BOOT'\"";
    //    }

    //    Console.WriteLine(WindowsCMD.StartInfo.Arguments);

    //    WindowsCMD.StartInfo.RedirectStandardOutput = true;
    //    WindowsCMD.StartInfo.RedirectStandardError = true;
    //    WindowsCMD.StartInfo.UseShellExecute = false;
    //    WindowsCMD.StartInfo.CreateNoWindow = true;
    //    WindowsCMD.Start();

    //    Task<string> stderrTask = WindowsCMD.StandardError.ReadToEndAsync();
    //    Task<string> readTask = WindowsCMD.StandardOutput.ReadToEndAsync();
    //    await WindowsCMD.WaitForExitAsync();

    //    string stdout = await readTask;
    //    string stderr = await stderrTask;

    //    return stdout.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
    //}

    public static string? FindBootValue(string GamePath)
    {
        try
        {
            const int SevenMb = 7 * 1024 * 1024;
            byte[] buffer = new byte[SevenMb];

            using var fs = new FileStream(GamePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096);
            int bytesRead = fs.Read(buffer);
            if (bytesRead == 0) return null;

            string text = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            var m = Regex.Match(text, @"BOOT\s*=\s*(\S+)", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value : null;
        }
        catch { return ""; }
    }

    public static string? ExtendedFindBootValue(string GamePath)
    {
        try
        {
            FileInfo NewFileInfo = new(GamePath);
            byte[] buffer = new byte[NewFileInfo.Length - 1];

            using var fs = new FileStream(GamePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096);
            int bytesRead = fs.Read(buffer);
            if (bytesRead == 0) return null;

            string text = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            var m = Regex.Match(text, @"BOOT\s*=\s*(\S+)", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value : null;
        }
        catch { return ""; }
    }

}