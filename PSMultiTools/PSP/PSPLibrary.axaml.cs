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
using PSMultiTools.Classes;
using PSMultiTools.Dialogs;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace PSMultiTools.PSP;

public partial class PSPLibrary : Window
{
    public BackgroundWorker GameLoaderWorker = new() { WorkerReportsProgress = true };
    public SyncWindow NewLoadingWindow = new() { Title = "Loading PSP files", ShowActivated = true };

    public int FoldersCount = 0;
    public int ISOCount = 0;

    public bool IsSoundPlaying = false;
    public bool AutoPlay = true;

    // Selected game context menu
    public ContextMenu NewContextMenu = new();
    public MenuItem CopyToMenuItem = new() { Header = "Copy to", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/copy-icon.png"))) } };
    public MenuItem PlayMenuItem = new() { Header = "Play Soundtrack", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) } };
    public MenuItem PlayGameMenuItem = new() { Header = "Play with PPSSPP", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/controller.png"))) } };

    // Supplemental library menu items
    public MenuItem LoadFolderMenuItem = new() { Header = "Load a new folder" };
    public MenuItem LoadDLFolderMenuItem = new() { Header = "Open Downloads folder" };

    // Supplemental emulator menu item
    public MenuItem EMU_Settings = new() { Header = "PPSSPP Settings" };

    public PSPLibrary()
    {
        InitializeComponent();
        Loaded += PSPLibrary_Loaded;

        GamesListBox.PointerWheelChanged += GamesListBox_PointerWheelChanged;
        GamesListBox.SelectionChanged += GamesListBox_SelectionChanged;

        GameLoaderWorker.DoWork += GameLoaderWorker_DoWork;
        GameLoaderWorker.RunWorkerCompleted += GameLoaderWorker_RunWorkerCompleted;

        LoadFolderMenuItem.Click += LoadFolderMenuItem_Click;
        LoadDLFolderMenuItem.Click += LoadDLFolderMenuItem_Click;
        CopyToMenuItem.Click += CopyToMenuItem_Click;
        PlayMenuItem.Click += PlayMenuItem_Click;
        PlayGameMenuItem.Click += PlayGameMenuItem_Click;
        EMU_Settings.Click += EMU_Settings_Click;
    }

    private void PSPLibrary_Loaded(object? sender, RoutedEventArgs e)
    {
        // Add supplemental library menu items that will be handled in the app
        MenuItem LibraryMenuItem = (MenuItem)NewPSPMenu.MainMenu.Items[0]!;
        LibraryMenuItem.Items.Add(LoadFolderMenuItem);
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem);

        NewContextMenu.Items.Add(CopyToMenuItem);
        NewContextMenu.Items.Add(PlayMenuItem);
        NewContextMenu.Items.Add(PlayGameMenuItem);
        GamesListBox.ContextMenu = NewContextMenu;

        // Add supplemental emulator menu item
        if (File.Exists(Environment.CurrentDirectory + @"\Emulators\ppsspp\PPSSPPWindows64.exe"))
        {
            NewPSPMenu.MainMenu.Items.Add(EMU_Settings);
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

    private async void GameLoaderWorker_DoWork(object? sender, DoWorkEventArgs e)
    {
        // PSP backup folders
        foreach (var Game in Directory.GetFiles(e.Argument!.ToString()!, "*.SFO", SearchOption.AllDirectories))
        {
            var NewPSPGame = new PSPGame();

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

                // Load game infos
                foreach (var Line in ProcessOutput)
                {
                    if (Line.StartsWith("TITLE="))
                    {
                        NewPSPGame.GameTitle = Utils.CleanTitle(Line.Split('=')[1].Trim('"'));
                    }
                    else if (Line.StartsWith("DISC_ID="))
                    {
                        NewPSPGame.GameID = Line.Split('=')[1].Trim('"');
                    }
                    else if (Line.StartsWith("CATEGORY="))
                    {
                        NewPSPGame.GameCategory = PSPGame.GetCategory(Line.Split('=')[1].Trim('"'));
                    }
                    else if (Line.StartsWith("DISC_VERSION="))
                    {
                        NewPSPGame.GameAppVer = Line.Split('=')[1].Trim('"');
                    }
                    else if (Line.StartsWith("PSP_SYSTEM_VER="))
                    {
                        NewPSPGame.GameRequiredFW = Line.Split('=')[1].Trim('"');
                    }
                }

                // Load game files
                string PSPGAMEFolder = Path.GetDirectoryName(Game)!;
                if (File.Exists(Path.Combine(PSPGAMEFolder, "ICON0.PNG")))
                {
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(PSPGAMEFolder, "ICON0.PNG"));
                            NewPSPGame.GameCoverSource = TempBitmapImage;
                        });
                    }
                    else
                    {
                        var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(PSPGAMEFolder, "ICON0.PNG"));
                        NewPSPGame.GameCoverSource = TempBitmapImage;
                    }
                }
                if (File.Exists(Path.Combine(PSPGAMEFolder, "PIC1.PNG")))
                {
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(PSPGAMEFolder, "PIC1.PNG"));
                            NewPSPGame.GameBackgroundSource = TempBitmapImage;
                        });
                    }
                    else
                    {
                        var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(PSPGAMEFolder, "PIC1.PNG"));
                        NewPSPGame.GameBackgroundSource = TempBitmapImage;
                    }
                }
                if (File.Exists(Path.Combine(PSPGAMEFolder, "SND0.AT3")))
                {
                    NewPSPGame.GameBackgroundSoundFile = Path.Combine(PSPGAMEFolder, "SND0.AT3");
                }

                long PSPGAMEFolderSize = Utils.GetDirectorySize(PSPGAMEFolder);

                NewPSPGame.GameSize = Utils.HumanReadableBytes(PSPGAMEFolderSize);
                NewPSPGame.GameFolderPath = Directory.GetParent(PSPGAMEFolder)!.FullName;
                NewPSPGame.GameFileType = PSPGame.GameFileTypes.Backup;

                if (!string.IsNullOrWhiteSpace(NewPSPGame.GameID))
                {
                    NewPSPGame.GameRegion = PSPGame.GetGameRegion(NewPSPGame.GameID);
                }

                Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadProgressBar.Value += 1d);
                Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadStatusTextBlock.Text = "Loading folder " + (NewLoadingWindow.LoadProgressBar.Value - (double)ISOCount).ToString() + " of " + FoldersCount.ToString());

                // Add to the ListView
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(NewPSPGame));
                }
                else
                {
                    GamesListBox.Items.Add(NewPSPGame);
                }

            }
        }

        // PSP ISOs
        foreach (var GameISO in Directory.GetFiles(e.Argument!.ToString()!, "*.iso", SearchOption.AllDirectories))
        {

            var NewPSPGame = new PSPGame();
            var ISOFileInfo = new FileInfo(GameISO);
            string ISOFileNameWithoutExt = Path.GetFileNameWithoutExtension(ISOFileInfo.Name);
            string CachePath = Path.Combine(Environment.CurrentDirectory, "Cache");
            bool Extracted = false;

            // Create cache dir for PSP games
            if (!Directory.Exists(CachePath))
            {
                Directory.CreateDirectory(CachePath);
            }

            // Extract files to display infos
            if (!File.Exists(Path.Combine(CachePath, ISOFileNameWithoutExt, "PSP_GAME", "PARAM.SFO")))
            {
                try
                {
                    Extracted = await Utils.ExtractISOFiles(GameISO, CachePath, false);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Error extracting files from ISO {GameISO}");
                    Trace.WriteLine($"Error extracting files from ISO {GameISO}");
                }
            }

            if (Extracted)
            {
                // Read PARAM.SFO
                using var SFOReader = new Process();
                SFOReader.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "sfo.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "sfo");
                SFOReader.StartInfo.Arguments = $"\"{Path.Combine(CachePath, ISOFileNameWithoutExt, "PSP_GAME", "PARAM.SFO")}\"";
                SFOReader.StartInfo.RedirectStandardOutput = true;
                SFOReader.StartInfo.UseShellExecute = false;
                SFOReader.StartInfo.CreateNoWindow = true;
                SFOReader.Start();

                var OutputReader = SFOReader.StandardOutput;
                string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

                if (ProcessOutput.Length > 0)
                {

                    // Load game infos
                    foreach (var Line in ProcessOutput)
                    {
                        if (Line.StartsWith("TITLE="))
                        {
                            NewPSPGame.GameTitle = Utils.CleanTitle(Line.Split('=')[1].Trim('"'));
                        }
                        else if (Line.StartsWith("DISC_ID="))
                        {
                            NewPSPGame.GameID = Line.Split('=')[1].Trim('"');
                        }
                        else if (Line.StartsWith("CATEGORY="))
                        {
                            NewPSPGame.GameCategory = PSPGame.GetCategory(Line.Split('=')[1].Trim('"'));
                        }
                        else if (Line.StartsWith("DISC_VERSION="))
                        {
                            NewPSPGame.GameAppVer = Line.Split('=')[1].Trim('"');
                        }
                        else if (Line.StartsWith("PSP_SYSTEM_VER="))
                        {
                            NewPSPGame.GameRequiredFW = Line.Split('=')[1].Trim('"');
                        }
                    }

                    // Load game files
                    string PSPGAMEFolder = Path.Combine(CachePath, ISOFileNameWithoutExt, "PSP_GAME");
                    if (File.Exists(Path.Combine(PSPGAMEFolder, "ICON0.PNG")))
                    {
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(PSPGAMEFolder, "ICON0.PNG"));
                                NewPSPGame.GameCoverSource = TempBitmapImage;
                            });
                        }
                        else
                        {
                            var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(PSPGAMEFolder, "ICON0.PNG"));
                            NewPSPGame.GameCoverSource = TempBitmapImage;
                        }
                    }
                    if (File.Exists(Path.Combine(PSPGAMEFolder, "PIC1.PNG")))
                    {
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(PSPGAMEFolder, "PIC1.PNG"));
                                NewPSPGame.GameBackgroundSource = TempBitmapImage;
                            });
                        }
                        else
                        {
                            var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(PSPGAMEFolder, "PIC1.PNG"));
                            NewPSPGame.GameBackgroundSource = TempBitmapImage;
                        }
                    }
                    if (File.Exists(Path.Combine(PSPGAMEFolder, "SND0.AT3")))
                    {
                        NewPSPGame.GameBackgroundSoundFile = Path.Combine(PSPGAMEFolder, "SND0.AT3");
                    }

                    NewPSPGame.GameSize = Utils.HumanReadableBytes(ISOFileInfo.Length);

                    if (!string.IsNullOrWhiteSpace(NewPSPGame.GameID))
                    {
                        NewPSPGame.GameRegion = PSPGame.GetGameRegion(NewPSPGame.GameID);
                    }

                }
            }

            NewPSPGame.GameFilePath = GameISO;
            NewPSPGame.GameFileType = PSPGame.GameFileTypes.ISO;

            Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadProgressBar.Value += 1d);
            Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadStatusTextBlock.Text = "Loading ISO " + (NewLoadingWindow.LoadProgressBar.Value - (double)FoldersCount).ToString() + " of " + ISOCount.ToString());

            // Add to the ListView
            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(NewPSPGame));
            }
            else
            {
                GamesListBox.Items.Add(NewPSPGame);
            }
        }
    }

    private void GameLoaderWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        NewLoadingWindow.Close();
    }

    #endregion

    #region Menu Actions

    private async void LoadFolderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select your PSP backup folder" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            FoldersCount = Directory.GetFiles(FBDResult, "*.SFO", SearchOption.AllDirectories).Length;
            ISOCount = Directory.GetFiles(FBDResult, "*.iso", SearchOption.AllDirectories).Length;

            NewLoadingWindow = new SyncWindow() { Title = "Loading PSP files", ShowActivated = true };
            NewLoadingWindow.LoadProgressBar.Maximum = (double)(FoldersCount + ISOCount);
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + (FoldersCount + ISOCount).ToString();
            NewLoadingWindow.Show();

            GameLoaderWorker.RunWorkerAsync(FBDResult);
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
            PSPGame SelectedPSPGame = (PSPGame)GamesListBox.SelectedItem;
            var FBD = new OpenFolderDialog() { Title = "Where do you want to save the selected game ?" };
            var FBDResult = await FBD.ShowAsync(this);

            if (FBDResult != null)
            {
                var NewCopyWindow = new CopyWindow()
                {
                    ShowActivated = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    BackupDestinationPath = FBDResult + @"\",
                    Title = "Copying " + SelectedPSPGame.GameTitle + " to " + FBDResult + @"\" + Path.GetFileName(SelectedPSPGame.GameFilePath),
                    BackupPath = SelectedPSPGame.GameFileType == PSPGame.GameFileTypes.Backup ? SelectedPSPGame.GameFolderPath! : SelectedPSPGame.GameFilePath!
                };

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
            Utils.StopGameSound();
            IsSoundPlaying = false;

            PlayMenuItem.Header = "Play Soundtrack";
            PlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) };
        }
        else if (GamesListBox.SelectedItem is not null)
        {
            PSPGame SelectedPSPGame = (PSPGame)GamesListBox.SelectedItem;
            if (SelectedPSPGame.GameBackgroundSoundFile is not null)
            {
                Utils.PlayGameSoundFile(SelectedPSPGame.GameBackgroundSoundFile);
                IsSoundPlaying = true;

                PlayMenuItem.Header = "Stop Soundtrack";
                PlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/stop.png"))) };
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Info", "No game soundtrack found.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
        }
    }

    private async void PlayGameMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Environment.CurrentDirectory + @"\Emulators\ppsspp\PPSSPPWindows64.exe"))
        {
            if (GamesListBox.SelectedItem is not null)
            {
                PSPGame SelectedPSPGame = (PSPGame)GamesListBox.SelectedItem;

                var box2 = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Start " + SelectedPSPGame.GameTitle + " using PPSSPP ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                var boxresult2 = await box2.ShowWindowDialogAsync(this);
                if (boxresult2 == ButtonResult.Yes)
                {
                    var EmulatorLauncherStartInfo = new ProcessStartInfo();
                    var EmulatorLauncher = new Process() { StartInfo = EmulatorLauncherStartInfo };
                    EmulatorLauncherStartInfo.FileName = Environment.CurrentDirectory + @"\Emulators\ppsspp\PPSSPPWindows64.exe";
                    EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(Environment.CurrentDirectory + @"\Emulators\ppsspp\PPSSPPWindows64.exe");

                    switch (SelectedPSPGame.GameFileType)
                    {
                        case PSPGame.GameFileTypes.Backup:
                            {
                                EmulatorLauncherStartInfo.Arguments = "\"" + SelectedPSPGame.GameFolderPath + "\"";
                                break;
                            }
                        case PSPGame.GameFileTypes.ISO:
                            {
                                EmulatorLauncherStartInfo.Arguments = "\"" + SelectedPSPGame.GameFilePath + "\"";
                                break;
                            }
                    }

                    EmulatorLauncher.Start();
                }

            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Cannot start PPSSPP." + Environment.NewLine + "Emulator pack is not installed.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    #endregion

    private async void GamesListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PSPGame SelectedPSPGame = (PSPGame)GamesListBox.SelectedItem;

            GameTitleTextBlock.Text = SelectedPSPGame.GameTitle;
            GameIDTextBlock.Text = "Title ID: " + SelectedPSPGame.GameID;
            GameRegionTextBlock.Text = "Region: " + SelectedPSPGame.GameRegion;
            GameAppVersionTextBlock.Text = "Application Version: " + SelectedPSPGame.GameAppVer;
            GameCategoryTextBlock.Text = "Category: " + SelectedPSPGame.GameCategory;
            GameSizeTextBlock.Text = "Size: " + SelectedPSPGame.GameSize;
            GameRequiredFirmwareTextBlock.Text = "Required Firmware: " + SelectedPSPGame.GameRequiredFW;

            GameBackupTypeTextBlock.Text = "Backup Type: " + SelectedPSPGame.GameFileType.ToString();

            if (!string.IsNullOrEmpty(SelectedPSPGame.GameFilePath))
            {
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " + new DirectoryInfo(Path.GetDirectoryName(SelectedPSPGame.GameFilePath)!).Name;
            }
            else
            {
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " + new DirectoryInfo(SelectedPSPGame.GameFolderPath!).Name;
            }

            if (SelectedPSPGame.GameBackgroundSource is not null)
            {
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        if (BlurringShape.Fill is ImageBrush RectangleImageBrush)
                        {
                            using MemoryStream memory = new();
                            SelectedPSPGame.GameBackgroundSource.ExportStream(memory);
                            memory.Position = 0;
                            Bitmap avaloniaBitmap = new(memory);
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
                        SelectedPSPGame.GameBackgroundSource.ExportStream(memory);
                        memory.Position = 0;
                        Bitmap avaloniaBitmap = new(memory);
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
                Utils.StopGameSound();
                IsSoundPlaying = false;
                PlayMenuItem.Header = "Play Soundtrack";
                PlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) };
            }
            if (SelectedPSPGame.GameBackgroundSoundFile is not null)
            {
                if (AutoPlay)
                {
                    Utils.PlayGameSoundFile(SelectedPSPGame.GameBackgroundSoundFile);
                    IsSoundPlaying = true;
                    PlayMenuItem.Header = "Stop Soundtrack";
                    PlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/stop.png"))) };
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

    private void EMU_Settings_Click(object? sender, RoutedEventArgs e)
    {
        //var NewPSPEmulatorSettingsWindow = new PSPEmulatorSettings() { ShowActivated = true };
        //NewPSPEmulatorSettingsWindow.Show();
    }

}