using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using IniParser;
using IniParser.Model;
using IronSoftware.Drawing;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PS4_Tools;
using PSMultiTools.Classes;
using PSMultiTools.Dialogs;
using PSMultiTools.MultiPlatformTools;
using PSMultiTools.PS4.Tools;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace PSMultiTools.PS4;

public partial class PS4Library : Window
{
    public BackgroundWorker GameLoaderWorker = new() { WorkerReportsProgress = true };
    public SyncWindow NewLoadingWindow = new() { Title = "Loading PS4 pkg files", ShowActivated = true };

    // Selected game context menu
    public ContextMenu NewContextMenu = new();
    public MenuItem CopyToMenuItem = new() { Header = "Copy to", Icon = new Avalonia.Controls.Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/copy-icon.png"))) } };
    public MenuItem ExtractPKGMenuItem = new() { Header = "Extract .pkg", Icon = new Avalonia.Controls.Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/extract.png"))) } };
    public MenuItem PKGInfoMenuItem = new() { Header = "PKG Details", Icon = new Avalonia.Controls.Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/information.png"))) } };
    public MenuItem PSNInfoMenuItem = new() { Header = "Store Details", Icon = new Avalonia.Controls.Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/information.png"))) } };
    public MenuItem PlayMenuItem = new() { Header = "Play Soundtrack", Icon = new Avalonia.Controls.Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) } };
    public MenuItem PlayGameMenuItem = new() { Header = "Play with psOff", Icon = new Avalonia.Controls.Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/controller.png"))) } };

    public int PKGCount = 0;
    public int FoldersCount = 0;

    public bool IsSoundPlaying = false;
    public bool AutoPlay = true;

    // Supplemental library menu items
    public MenuItem LoadFolderMenuItem = new() { Header = "Load a new folder" };
    public MenuItem LoadDLFolderMenuItem = new() { Header = "Open Downloads folder" };

    // Supplemental emulator menu item
    public MenuItem EMU_Settings = new() { Header = "psOff Settings" };

    public PS4Library()
    {
        InitializeComponent();

        Loaded += PS4Library_Loaded;

        GamesListBox.PointerWheelChanged += GamesListBox_PointerWheelChanged;
        GamesListBox.SelectionChanged += GamesListBox_SelectionChanged;

        NewContextMenu.Opening += NewContextMenu_Opening;
        NewContextMenu.Closing += NewContextMenu_Closing;

        GameLoaderWorker.DoWork += GameLoaderWorker_DoWork;
        GameLoaderWorker.RunWorkerCompleted += GameLoaderWorker_RunWorkerCompleted;

        CopyToMenuItem.Click += CopyToMenuItem_Click;
        PlayMenuItem.Click += PlayMenuItem_Click;
        PSNInfoMenuItem.Click += PSNInfoMenuItem_Click;
        PKGInfoMenuItem.Click += PKGInfoMenuItem_Click;
        PlayGameMenuItem.Click += PlayGameMenuItem_Click;
        ExtractPKGMenuItem.Click += ExtractPKGMenuItem_Click;
        LoadFolderMenuItem.Click += LoadFolderMenuItem_Click;
        LoadDLFolderMenuItem.Click += LoadDLFolderMenuItem_Click;
        EMU_Settings.Click += EMU_Settings_Click;
    }

    private void PS4Library_Loaded(object? sender, RoutedEventArgs e)
    {
        // Add supplemental library menu items that will be handled in the app
        MenuItem LibraryMenuItem = (MenuItem)NewPS4Menu.MainMenu.Items[0]!;
        LibraryMenuItem.Items.Add(LoadFolderMenuItem);
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem);

        // Add the games context menu
        GamesListBox.ContextMenu = NewContextMenu;

        // Add supplemental emulator menu item
        if (File.Exists(Environment.CurrentDirectory + @"\Emulators\psOff\psoff.exe"))
        {
            NewPS4Menu.MainMenu.Items.Add(EMU_Settings);
        }

        // Load config if exists
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini")))
        {
            try
            {
                var PSMTConfigParser = new FileIniDataParser();
                IniData PSMTConfigData = PSMTConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini"));

                if (!string.IsNullOrEmpty(PSMTConfigData["General"]["AutoLibraryMusic"]))
                {
                    if (PSMTConfigData["General"]["AutoLibraryMusic"] == "False")
                    {
                        AutoPlay = false;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        // Set background fading animation
        BlurringShape.Transitions = [new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(500), Easing = new CubicEaseOut() }];
    }

    #region Game Loader

    private void GameLoaderWorker_DoWork(object? sender, DoWorkEventArgs e)
    {

        // PS4 PKGs
        foreach (var Game in Directory.GetFiles(e.Argument!.ToString()!, "*.pkg", SearchOption.AllDirectories))
        {
            var NewPS4Game = new PS4Game();
            var GamePKG = PKG.SceneRelated.Read_PKG(Game);

            // Set game infos
            NewPS4Game.GameTitle = GamePKG.PS4_Title;
            NewPS4Game.GameContentID = GamePKG.Content_ID;
            NewPS4Game.GameFilePath = Game;
            NewPS4Game.GameRegion = GamePKG.Region.Replace("(", "").Replace(")", "").Trim();
            NewPS4Game.GameRequiredFW = GamePKG.Firmware_Version;
            NewPS4Game.GameSize = GamePKG.Size;
            NewPS4Game.GameAppVer = GamePKG.Param.APP_VER;
            NewPS4Game.GameFileType = PS4Game.GameFileTypes.PKG;

            if (GamePKG.Param is not null)
            {
                if (!string.IsNullOrEmpty(GamePKG.Param.Category))
                {
                    NewPS4Game.GameCategory = PS4Game.GetCategory(GamePKG.Param.Category);
                }
                if (!string.IsNullOrEmpty(GamePKG.Param.APP_VER))
                {
                    NewPS4Game.GameAppVer = GamePKG.Param.APP_VER;
                }

                foreach (var TableRow in GamePKG.Param.Tables)
                {
                    if (TableRow.Name == "TITLE_ID")
                    {
                        NewPS4Game.GameID = TableRow.Value;
                    }
                    if (TableRow.Name == "VERSION")
                    {
                        NewPS4Game.GameVer = TableRow.Value;
                    }
                }

            }

            if (GamePKG.Icon is not null)
            {
                Dispatcher.UIThread.Invoke(() => NewPS4Game.GameCoverSource = AnyBitmap.FromBytes(GamePKG.Icon));
            }
            if (GamePKG.Image is not null)
            {
                Dispatcher.UIThread.Invoke(() => NewPS4Game.GameBackgroundSource = AnyBitmap.FromBytes(GamePKG.Image));
            }
            if (GamePKG.Sound is not null)
            {
                NewPS4Game.GameSoundtrackBytes = Media.Atrac9.LoadAt9(GamePKG.Sound);
            }

            Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadProgressBar.Value += 1);
            Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadStatusTextBlock.Text = "Loading PKG " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + PKGCount.ToString());

            // Add to the ListView
            switch (GamePKG.Param!.Category ?? "")
            {
                case "ac":
                    {
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() => DLCsListBox.Items.Add(NewPS4Game));
                        }
                        else
                        {
                            DLCsListBox.Items.Add(NewPS4Game);
                        }

                        break;
                    }
                case "gd":
                    {
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(NewPS4Game));
                        }
                        else
                        {
                            GamesListBox.Items.Add(NewPS4Game);
                        }

                        break;
                    }
                case "gp":
                    {
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() => UpdatesListBox.Items.Add(NewPS4Game));
                        }
                        else
                        {
                            UpdatesListBox.Items.Add(NewPS4Game);
                        }

                        break;
                    }

                default:
                    {
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() => OthersListBox.Items.Add(NewPS4Game));
                        }
                        else
                        {
                            OthersListBox.Items.Add(NewPS4Game);
                        }

                        break;
                    }
            }
        }

        // PS4 Backup folders
        foreach (var Game in Directory.GetFiles(e.Argument.ToString()!, "*.sfo", SearchOption.AllDirectories))
        {
            var NewPS4Game = new PS4Game();

            using var SFOReader = new Process();
            SFOReader.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "sfo.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "sfo");
            SFOReader.StartInfo.Arguments = "\"" + Game + "\"";
            SFOReader.StartInfo.RedirectStandardOutput = true;
            SFOReader.StartInfo.UseShellExecute = false;
            SFOReader.StartInfo.CreateNoWindow = true;
            SFOReader.Start();

            var OutputReader = SFOReader.StandardOutput;
            string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
            if (ProcessOutput.Length > 0)
            {

                foreach (var Line in ProcessOutput)
                {
                    if (Line.StartsWith("TITLE="))
                    {
                        NewPS4Game.GameTitle = Utils.CleanTitle(Line.Split('=')[1].Trim('"').Trim());
                    }
                    else if (Line.StartsWith("TITLE_ID="))
                    {
                        NewPS4Game.GameID = Line.Split('=')[1].Trim('"').Trim();
                    }
                    else if (Line.StartsWith("CATEGORY="))
                    {
                        NewPS4Game.GameCategory = Line.Split('=')[1].Trim('"');
                    }
                    else if (Line.StartsWith("APP_VER="))
                    {
                        NewPS4Game.GameAppVer = Line.Split('=')[1].Trim('"').Trim();
                    }
                    else if (Line.StartsWith("SYSTEM_VER="))
                    {
                        NewPS4Game.GameRequiredFW = Line.Split('=')[1].Trim('"').Trim().Replace("0x0", "").Insert(1, ".").Insert(5, ".");
                    }
                    else if (Line.StartsWith("VERSION="))
                    {
                        NewPS4Game.GameVer = Line.Split('=')[1].Trim('"').Trim();
                    }
                    else if (Line.StartsWith("CONTENT_ID="))
                    {
                        NewPS4Game.GameContentID = Line.Split('=')[1].Trim('"').Trim();
                    }
                }

                string PSVGAMEFolder = Path.GetDirectoryName(Directory.GetParent(Game)!.FullName)!;
                long PSVGAMEFolderSize = Utils.GetDirectorySize(PSVGAMEFolder);

                NewPS4Game.GameSize = Utils.HumanReadableBytes(PSVGAMEFolderSize);
                NewPS4Game.GameFolderPath = PSVGAMEFolder;
                NewPS4Game.GameFileType = PS4Game.GameFileTypes.Backup;

                // Load icon, background & sound if available
                if (File.Exists(Path.Combine(PSVGAMEFolder, "sce_sys", "icon0.png")))
                {
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(PSVGAMEFolder, "sce_sys", "icon0.png"));
                            NewPS4Game.GameCoverSource = TempBitmapImage;
                        });
                    }
                    else
                    {
                        var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(PSVGAMEFolder, "sce_sys", "icon0.png"));
                        NewPS4Game.GameCoverSource = TempBitmapImage;
                    }
                }
                if (File.Exists(Path.Combine(PSVGAMEFolder, "sce_sys", "pic1.png")))
                {
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(PSVGAMEFolder, "sce_sys", "pic1.png"));
                            NewPS4Game.GameBackgroundSource = TempBitmapImage;
                        });
                    }
                    else
                    {
                        var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(PSVGAMEFolder, "sce_sys", "pic1.png"));
                        NewPS4Game.GameBackgroundSource = TempBitmapImage;
                    }
                }
                if (File.Exists(Path.Combine(PSVGAMEFolder, "sce_sys", "snd0.at9")))
                {
                    NewPS4Game.GameSoundtrackBytes = Media.Atrac9.LoadAt9(Path.Combine(PSVGAMEFolder, "sce_sys", "snd0.at9"));
                }

                Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadProgressBar.Value += 1d);
                Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadStatusTextBlock.Text = "Loading backup folder " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + FoldersCount.ToString());

                // Add to the ListView
                switch (NewPS4Game.GameCategory ?? "")
                {
                    case "ac":
                        {
                            if (Dispatcher.UIThread.CheckAccess() == false)
                            {
                                Dispatcher.UIThread.Invoke(() => DLCsListBox.Items.Add(NewPS4Game));
                            }
                            else
                            {
                                DLCsListBox.Items.Add(NewPS4Game);
                            }

                            break;
                        }
                    case "gd":
                        {
                            if (Dispatcher.UIThread.CheckAccess() == false)
                            {
                                Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(NewPS4Game));
                            }
                            else
                            {
                                GamesListBox.Items.Add(NewPS4Game);
                            }

                            break;
                        }
                    case "gp":
                        {
                            if (Dispatcher.UIThread.CheckAccess() == false)
                            {
                                Dispatcher.UIThread.Invoke(() => UpdatesListBox.Items.Add(NewPS4Game));
                            }
                            else
                            {
                                UpdatesListBox.Items.Add(NewPS4Game);
                            }

                            break;
                        }

                    default:
                        {
                            if (Dispatcher.UIThread.CheckAccess() == false)
                            {
                                Dispatcher.UIThread.Invoke(() => OthersListBox.Items.Add(NewPS4Game));
                            }
                            else
                            {
                                OthersListBox.Items.Add(NewPS4Game);
                            }

                            break;
                        }
                }

            }

        }

    }

    private void GameLoaderWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        NewLoadingWindow.Close();
    }

    #endregion

    #region Contextmenu Actions

    private async void CopyToMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS4Game SelectedPS4Game = (PS4Game)GamesListBox.SelectedItem;
            var FBD = new OpenFolderDialog() { Title = "Where do you want to copy the selected game ?" };
            var FBDResult = await FBD.ShowAsync(this);

            if (FBDResult != null)
            {
                var NewCopyWindow = new CopyWindow()
                {
                    ShowActivated = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    BackupDestinationPath = FBDResult + @"\",
                    Title = "Copying " + SelectedPS4Game.GameTitle + " to " + FBDResult + @"\" + Path.GetFileName(SelectedPS4Game.GameFilePath),
                    BackupPath = SelectedPS4Game.GameFileType == PS4Game.GameFileTypes.Backup ? SelectedPS4Game.GameFolderPath! : SelectedPS4Game.GameFilePath!
                };

                if (SelectedPS4Game.GameCoverSource is not null)
                {
                    NewCopyWindow.GameIcon = SelectedPS4Game.GameCoverSource;
                }

                if (await NewCopyWindow.ShowDialog<bool>(this) == true)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Completed", "Game copied with success !", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
            }

        }
    }

    private async void PlayMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (IsSoundPlaying == true)
        {
            Utils.StopGameSoundBytes();
            IsSoundPlaying = false;

            PlayMenuItem.Header = "Play Soundtrack";
            PlayMenuItem.Icon = new Avalonia.Controls.Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) };
        }
        else if (GamesListBox.SelectedItem is not null)
        {
            PS4Game SelectedPS4Game = (PS4Game)GamesListBox.SelectedItem;
            if (SelectedPS4Game.GameSoundtrackBytes is not null)
            {
                Utils.StartAndStreamSoundBytes(SelectedPS4Game.GameSoundtrackBytes);
                IsSoundPlaying = true;

                PlayMenuItem.Header = "Stop Soundtrack";
                PlayMenuItem.Icon = new Avalonia.Controls.Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/stop.png"))) };
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Info", "No game soundtrack found.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
        }
    }

    private async void PSNInfoMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS4Game SelectedPS4Game = (PS4Game)GamesListBox.SelectedItem;
            if (!string.IsNullOrEmpty(SelectedPS4Game.GameContentID))
            {
                var NewPSNInfo = new PSNInfo() { ShowActivated = true, CurrentGameContentID = SelectedPS4Game.GameContentID };
                NewPSNInfo.Show();
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Info", "Selected game requires a Content ID to display infos from PSN.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
        }
    }

    private void PKGInfoMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS4Game SelectedPS4Game = (PS4Game)GamesListBox.SelectedItem;
            var NewPKGInfo = new PKGInfo() { SelectedPKG = SelectedPS4Game.GameFilePath!, Console = "PS4" };
            NewPKGInfo.Show();
        }
    }

    private async void PlayGameMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Environment.CurrentDirectory + @"\Emulators\psOff\psoff.exe"))
        {
            if (GamesListBox.SelectedItem is not null)
            {
                PS4Game SelectedPS4Game = (PS4Game)GamesListBox.SelectedItem;
                if (SelectedPS4Game.GameFileType == PS4Game.GameFileTypes.Backup)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Start " + SelectedPS4Game.GameTitle + " using psOff ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult = await box.ShowWindowDialogAsync(this);
                    if (boxresult == ButtonResult.Yes)
                    {
                        var EmulatorLauncherStartInfo = new ProcessStartInfo();
                        var EmulatorLauncher = new Process() { StartInfo = EmulatorLauncherStartInfo };
                        EmulatorLauncherStartInfo.FileName = Environment.CurrentDirectory + @"\Emulators\psOff\psoff.exe";
                        EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(Environment.CurrentDirectory + @"\Emulators\psOff\psoff.exe");
                        EmulatorLauncherStartInfo.Arguments = "--file \"" + SelectedPS4Game.GameFolderPath + @"\eboot.bin""";

                        EmulatorLauncher.Start();
                    }
                }

                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Cannot launch PKG", "PKG files are not supported yet." + Environment.NewLine + "Please extract the .pkg file before running with psOff.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Cannot start psoff." + Environment.NewLine + "Emulator pack is not installed.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private void ExtractPKGMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS4Game SelectedPS4Game = (PS4Game)GamesListBox.SelectedItem;
            var NewPKGExtractor = new PS4PKGExtractor() { ShowActivated = true, PKGToExtract = SelectedPS4Game.GameFilePath! };
            NewPKGExtractor.Show();
        }
    }

    #endregion

    #region Menu Actions

    private async void LoadFolderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select your PS4 backup folder" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            // Set the count of pkg files & backup folders
            PKGCount = Directory.GetFiles(FBDResult, "*.pkg", SearchOption.AllDirectories).Length;
            FoldersCount = Directory.GetFiles(FBDResult, "*.sfo", SearchOption.AllDirectories).Length;

            // Show the loading progress window
            NewLoadingWindow = new SyncWindow() { Title = "Loading PS4 files", ShowActivated = true };
            NewLoadingWindow.LoadProgressBar.Maximum = (double)(PKGCount + FoldersCount);
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + (PKGCount + FoldersCount).ToString();
            NewLoadingWindow.Show();

            // Load the pkg files
            GameLoaderWorker.RunWorkerAsync(FBDResult);
        }
    }

    private void LoadDLFolderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        Utils.OpenDownloadsFolder();
    }

    #endregion

    private async void GamesListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS4Game SelectedPS4Game = (PS4Game)GamesListBox.SelectedItem;

            if (GameTitleTextBlock.IsVisible == false)
            {
                GameTitleTextBlock.IsVisible = true;
                GameIDTextBlock.IsVisible = true;
                GameContentIDTextBlock.IsVisible = true;
                GameRegionTextBlock.IsVisible = true;
                GameVersionTextBlock.IsVisible = true;
                GameAppVersionTextBlock.IsVisible = true;
                GameCategoryTextBlock.IsVisible = true;
                GameSizeTextBlock.IsVisible = true;
                GameRequiredFirmwareTextBlock.IsVisible = true;
                GameBackupFolderNameTextBlock.IsVisible = true;
            }

            GameTitleTextBlock.Text = SelectedPS4Game.GameTitle;
            GameIDTextBlock.Text = "Title ID: " + SelectedPS4Game.GameID;
            GameContentIDTextBlock.Text = "Content ID: " + SelectedPS4Game.GameContentID;
            GameRegionTextBlock.Text = "Region: " + SelectedPS4Game.GameRegion;
            GameVersionTextBlock.Text = "Game Version: " + SelectedPS4Game.GameVer;
            GameAppVersionTextBlock.Text = "Application Version: " + SelectedPS4Game.GameAppVer;
            GameCategoryTextBlock.Text = "Category: " + SelectedPS4Game.GameCategory;
            GameSizeTextBlock.Text = "Size: " + SelectedPS4Game.GameSize;
            GameRequiredFirmwareTextBlock.Text = "Required Firmware: " + SelectedPS4Game.GameRequiredFW;

            if (!string.IsNullOrEmpty(SelectedPS4Game.GameFilePath))
            {
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " + new DirectoryInfo(Path.GetDirectoryName(SelectedPS4Game.GameFilePath)!).Name;
            }
            else
            {
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " + new DirectoryInfo(SelectedPS4Game.GameFolderPath!).Name;
            }

            if (SelectedPS4Game.GameBackgroundSource is not null)
            {
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        if (BlurringShape.Fill is ImageBrush RectangleImageBrush)
                        {
                            using MemoryStream memory = new();
                            SelectedPS4Game.GameBackgroundSource.ExportStream(memory);
                            memory.Position = 0;
                            Avalonia.Media.Imaging.Bitmap avaloniaBitmap = new(memory);
                            RectangleImageBrush.Source = avaloniaBitmap;
                        }
                        BlurringShape.Opacity = 0;
                        BlurringShape.Opacity = 1;
                    });
                }
                else
                {
                    if (BlurringShape.Fill is ImageBrush RectangleImageBrush)
                    {
                        using MemoryStream memory = new();
                        SelectedPS4Game.GameBackgroundSource.ExportStream(memory);
                        memory.Position = 0;
                        Avalonia.Media.Imaging.Bitmap avaloniaBitmap = new(memory);
                        RectangleImageBrush.Source = avaloniaBitmap;
                    }
                    BlurringShape.Opacity = 0;
                    BlurringShape.Opacity = 1;
                }
            }
            else
            {
                if (BlurringShape.Fill is ImageBrush RectangleImageBrush)
                {
                    RectangleImageBrush.Source = null;
                }
            }

            if (IsSoundPlaying)
            {
                Utils.StopGameSoundBytes();
                IsSoundPlaying = false;
                PlayMenuItem.Header = "Play Soundtrack";
                PlayMenuItem.Icon = new Avalonia.Controls.Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) };
            }
            if (SelectedPS4Game.GameSoundtrackBytes is not null)
            {
                if (AutoPlay)
                {
                    Utils.StartAndStreamSoundBytes(SelectedPS4Game.GameSoundtrackBytes);
                    IsSoundPlaying = true;
                    PlayMenuItem.Header = "Stop Soundtrack";
                    PlayMenuItem.Icon = new Avalonia.Controls.Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/stop.png"))) };
                }
            }
        }
    }

    private void GamesListBox_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        var GamesListBoxScrollViewer = Utils.FindScrollViewer(GamesListBox)!;
        var delta = e.Delta.Y * 20;
        var newX = Math.Clamp(GamesListBoxScrollViewer.Offset.X + delta, 0, Math.Max(0, GamesListBoxScrollViewer.Extent.Width - GamesListBoxScrollViewer.Viewport.Width));
        GamesListBoxScrollViewer.Offset = new Avalonia.Vector(newX, GamesListBoxScrollViewer.Offset.Y);
        e.Handled = true;
    }

    private void NewContextMenu_Opening(object? sender, CancelEventArgs e)
    {
        NewContextMenu.Items.Clear();

        if (GamesListBox.SelectedItem is not null)
        {
            PS4Game SelectedPS3Game = (PS4Game)GamesListBox.SelectedItem;

            NewContextMenu.Items.Add(CopyToMenuItem);

            switch (SelectedPS3Game.GameFileType)
            {
                case PS4Game.GameFileTypes.Backup:
                    {
                        NewContextMenu.Items.Add(PlayMenuItem);
                        NewContextMenu.Items.Add(PlayGameMenuItem);
                        break;
                    }
                case PS4Game.GameFileTypes.PKG:
                    {
                        NewContextMenu.Items.Add(ExtractPKGMenuItem);
                        NewContextMenu.Items.Add(PKGInfoMenuItem);
                        NewContextMenu.Items.Add(PlayMenuItem);
                        NewContextMenu.Items.Add(PSNInfoMenuItem);
                        break;
                    }
            }
        }
    }

    private void NewContextMenu_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        NewContextMenu.Items.Clear();
    }

    private void EMU_Settings_Click(object? sender, RoutedEventArgs e)
    {
        //var NewPS4EmulatorSettingsWindow = new PS4EmulatorSettings() { ShowActivated = true };
        //NewPS4EmulatorSettingsWindow.Show();
    }

}