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
using PSMultiTools.MultiPlatformTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PSMultiTools.PSV;

public partial class PSVLibrary : Window
{
    public SyncWindow NewLoadingWindow = new() { Title = "Loading PS Vita files", ShowActivated = true };

    public int FoldersCount = 0;
    public int PKGCount = 0;

    public List<string> URLs = [];
    public int CurrentURL = 0;

    // Selected game context menu
    public ContextMenu NewContextMenu = new();
    public MenuItem CopyToMenuItem = new() { Header = "Copy to", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/copy-icon.png"))) } };
    public MenuItem PKGInfoMenuItem = new() { Header = "PKG Details", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/information.png"))) } };
    public MenuItem PlayGameMenuItem = new() { Header = "Play with vita3k", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/controller.png"))) } };

    // Supplemental library menu items
    public MenuItem LoadFolderMenuItem = new() { Header = "Load a backup folder" };
    public MenuItem LoadFTPMenuItem = new() { Header = "Load installed backups over FTP" };
    public MenuItem LoadLibraryMenuItem = new() { Header = "Show games library" };
    public MenuItem LoadDLFolderMenuItem = new() { Header = "Open Downloads folder" };

    // Supplemental emulator menu item
    public MenuItem EMU_Settings = new() { Header = "Vita3k Settings" };

    public PSVLibrary()
    {
        InitializeComponent();

        Loaded += PSVLibrary_Loaded;

        GamesListBox.PointerWheelChanged += GamesListBox_PointerWheelChanged;
        GamesListBox.SelectionChanged += GamesListBox_SelectionChanged;

        NewContextMenu.Opening += NewContextMenu_Opening;
        NewContextMenu.Closing += NewContextMenu_Closing;

        CopyToMenuItem.Click += CopyToMenuItem_Click;
        PKGInfoMenuItem.Click += PKGInfoMenuItem_Click;
        PlayGameMenuItem.Click += PlayGameMenuItem_Click;
        LoadFolderMenuItem.Click += LoadFolderMenuItem_Click;
        LoadDLFolderMenuItem.Click += LoadDLFolderMenuItem_Click;
        EMU_Settings.Click += EMU_Settings_Click;
    }

    private void PSVLibrary_Loaded(object? sender, RoutedEventArgs e)
    {
        // Set the controls in the shared library
        NewPSVMenu.GamesLView = GamesListBox;

        // Add supplemental library menu items that will be handled in the app
        MenuItem LibraryMenuItem = (MenuItem)NewPSVMenu.MainMenu.Items[0]!;
        LibraryMenuItem.Items.Add(LoadFolderMenuItem);
        LibraryMenuItem.Items.Add(LoadLibraryMenuItem);
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem);

        // Add the new PKG Browser
        var PKGDownloaderMenuItem = new MenuItem() { Header = "PKG Browser & Downloader" };
        PKGDownloaderMenuItem.Click += OpenPKGBrowser;
        NewPSVMenu.MainMenu.Items.Add(PKGDownloaderMenuItem);

        NewContextMenu.Items.Add(CopyToMenuItem);
        NewContextMenu.Items.Add(PKGInfoMenuItem);
        NewContextMenu.Items.Add(PlayGameMenuItem);
        GamesListBox.ContextMenu = NewContextMenu;

        // Add supplemental emulator menu item
        if (File.Exists(Environment.CurrentDirectory + @"\Emulators\vita3k\Vita3K.exe"))
        {
            NewPSVMenu.MainMenu.Items.Add(EMU_Settings);
        }
    }

    #region Contextmenu Actions

    private async void CopyToMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PSVGame SelectedPSVGame = (PSVGame)GamesListBox.SelectedItem;
            var FBD = new OpenFolderDialog() { Title = "Where do you want to save the selected game ?" };
            var FBDResult = await FBD.ShowAsync(this);

            if (FBDResult != null)
            {
                var NewCopyWindow = new CopyWindow()
                {
                    ShowActivated = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    BackupDestinationPath = FBDResult + @"\",
                    Title = "Copying " + SelectedPSVGame.GameTitle + " to " + FBDResult + @"\" + Path.GetFileName(SelectedPSVGame.GameFilePath),
                    BackupPath = SelectedPSVGame.GameFileType == PSVGame.GameFileTypes.Backup ? SelectedPSVGame.GameFolderPath! : SelectedPSVGame.GameFilePath!
                };

                if (await NewCopyWindow.ShowDialog<bool>(this) == true)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Completed", "Game copied with success !", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
            }

        }
    }

    private void PKGInfoMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PSVGame SelectedPSVGame = (PSVGame)GamesListBox.SelectedItem;
            var NewPKGInfo = new PKGInfo() { SelectedPKG = SelectedPSVGame.GameFilePath!, Console = "PSV" };
            NewPKGInfo.Show();
        }
    }

    private async void PlayGameMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Environment.CurrentDirectory + @"\Emulators\vita3k\Vita3K.exe"))
        {
            if (GamesListBox.SelectedItem is not null)
            {
                PSVGame SelectedPSVGame = (PSVGame)GamesListBox.SelectedItem;

                // Check if vita3k has been configured before
                if (!File.Exists(Environment.CurrentDirectory + @"\Emulators\vita3k\config.yml"))
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "In order to play PS Vita games using vita3k you need to finish the initial setup." + Environment.NewLine + "Do you want to start the initial setup now ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult = await box.ShowWindowDialogAsync(this);
                    if (boxresult == ButtonResult.Yes)
                    {
                        if (File.Exists(Environment.CurrentDirectory + @"\Emulators\vita3k\Vita3K.exe"))
                        {
                            Process.Start(Environment.CurrentDirectory + @"\Emulators\vita3k\Vita3K.exe");
                        }
                        else
                        {
                            var box2 = MessageBoxManager.GetMessageBoxStandard("Error", "vita3k not found at " + Environment.CurrentDirectory + @"\Emulators\vita3k", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box2.ShowWindowAsync();
                            return;
                        }
                    }
                    else
                    {
                        var box2 = MessageBoxManager.GetMessageBoxStandard("Error", "Aborting", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box2.ShowWindowAsync();
                        return;
                    }
                }
                else
                {
                    // Read vita3k config & check if initial setup done
                    string[] ConfigLines = File.ReadAllLines(Environment.CurrentDirectory + @"\Emulators\vita3k\config.yml", System.Text.Encoding.UTF8);
                    if (!(ConfigLines[1] == "initial-setup: true"))
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "In order to play PS Vita games using vita3k you need to finish the initial setup." + Environment.NewLine + "Do you want to start the initial setup now ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                        var boxresult = await box.ShowWindowDialogAsync(this);
                        if (boxresult == ButtonResult.Yes)
                        {
                            if (File.Exists(Environment.CurrentDirectory + @"\Emulators\vita3k\Vita3K.exe"))
                            {
                                Process.Start(Environment.CurrentDirectory + @"\Emulators\vita3k\Vita3K.exe");
                            }
                            else
                            {
                                var box2 = MessageBoxManager.GetMessageBoxStandard("Error", "vita3k not found at " + Environment.CurrentDirectory + @"\Emulators\vita3k", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                await box2.ShowWindowAsync();
                                return;
                            }
                        }
                        else
                        {
                            var box2 = MessageBoxManager.GetMessageBoxStandard("Error", "Aborting", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box2.ShowWindowAsync();
                            return;
                        }
                    }
                    else
                    {
                        // Proceed

                        // Get the vita3k pref path (emulator path)
                        string Vita3kPrefPath = "";
                        foreach (string ConfigLine in ConfigLines)
                        {
                            if (ConfigLine.Contains("pref-path:"))
                            {
                                Vita3kPrefPath = ConfigLine.Replace("pref-path: ", "");
                                break;
                            }
                        }

                        if (string.IsNullOrEmpty(Vita3kPrefPath))
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error reading vita3k config.yaml", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                            return;
                        }
                        else
                        {

                            //bool FontPackageIntalled = false;
                            bool FirmwareIntalled;

                            // Check if firmware fonts package is installed
                            if (!File.Exists(Vita3kPrefPath + @"sa0\data\font\pvf\ltn0.pvf"))
                            {
                                var box = MessageBoxManager.GetMessageBoxStandard("Firmware Fonts Package not installed", "PS Vita Firmware Fonts Package is not installed but recommended." + Environment.NewLine + "Do you want to intall a PSP2UPDAT.PUP file now ?" + Environment.NewLine + "If you select YES then close vita3k after the firmware installation if still open.", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                var boxresult = await box.ShowWindowDialogAsync(this);
                                if (boxresult == ButtonResult.Yes)
                                {

                                    var pupFileFilter = new FileDialogFilter
                                    {
                                        Name = "PUP File",
                                        Extensions = ["PUP"]
                                    };
                                    var OFD = new OpenFileDialog() { Title = "Select the PSP2UPDAT.PUP file to install.", Filters = { pupFileFilter }, AllowMultiple = false };
                                    var OFDResult = await OFD.ShowAsync(this);

                                    if (OFDResult != null && OFDResult.Length > 0)
                                    {

                                        // Set up rpcs3 to install the selected PS3 firmware
                                        var EmulatorLauncherStartInfo = new ProcessStartInfo();
                                        var EmulatorLauncher = new Process() { StartInfo = EmulatorLauncherStartInfo };
                                        EmulatorLauncherStartInfo.FileName = Environment.CurrentDirectory + @"\Emulators\vita3k\Vita3K.exe";
                                        EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(Environment.CurrentDirectory + @"\Emulators\vita3k\Vita3K.exe");
                                        EmulatorLauncherStartInfo.Arguments = "--firmware \"" + OFDResult[0] + "\"";
                                        EmulatorLauncher.Start();
                                        EmulatorLauncher.WaitForExit();
                                        EmulatorLauncher.Dispose();

                                        //FontPackageIntalled = true;
                                    }
                                    else
                                    {
                                        var box2 = MessageBoxManager.GetMessageBoxStandard("Error", "No PSP2UPDAT.PUP file specified.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                        await box2.ShowWindowAsync();
                                        return;
                                    }

                                }
                            }
                            else
                            {
                                //FontPackageIntalled = true;
                            }

                            // Check if PS Vita firmware is installed
                            if (!File.Exists(Vita3kPrefPath + @"os0\kd\registry.db0"))
                            {
                                var box = MessageBoxManager.GetMessageBoxStandard("PS Vita Firmware not intalled", "PS Vita Firmware is not installed but required to launch games." + Environment.NewLine + "Do you want to intall a PSVUPDAT.PUP file now ?" + Environment.NewLine + "If you select YES then close vita3k after the firmware installation if still open.", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                var boxresult = await box.ShowWindowDialogAsync(this);
                                if (boxresult == ButtonResult.Yes)
                                {

                                    var pupFileFilter = new FileDialogFilter
                                    {
                                        Name = "PUP File",
                                        Extensions = ["PUP"]
                                    };
                                    var OFD = new OpenFileDialog() { Title = "Select the PSVUPDAT.PUP file to install.", Filters = { pupFileFilter }, AllowMultiple = false };
                                    var OFDResult = await OFD.ShowAsync(this);

                                    if (OFDResult != null && OFDResult.Length > 0)
                                    {

                                        // Set up rpcs3 to install the selected PS3 firmware
                                        var EmulatorLauncherStartInfo = new ProcessStartInfo();
                                        var EmulatorLauncher = new Process() { StartInfo = EmulatorLauncherStartInfo };
                                        EmulatorLauncherStartInfo.FileName = Environment.CurrentDirectory + @"\Emulators\vita3k\Vita3K.exe";
                                        EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(Environment.CurrentDirectory + @"\Emulators\vita3k\Vita3K.exe");
                                        EmulatorLauncherStartInfo.Arguments = "--firmware \"" + OFDResult[0] + "\"";
                                        EmulatorLauncher.Start();
                                        EmulatorLauncher.WaitForExit();
                                        EmulatorLauncher.Dispose();

                                        FirmwareIntalled = true;
                                    }
                                    else
                                    {
                                        var box2 = MessageBoxManager.GetMessageBoxStandard("Error", "No PSVUPDAT.PUP file specified.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                        await box2.ShowWindowAsync();
                                        return;
                                    }
                                }
                                else
                                {
                                    return;
                                }
                            }
                            else
                            {
                                FirmwareIntalled = true;
                            }

                            // Proceed if PS Vita Firmware is installed
                            if (FirmwareIntalled == true)
                            {
                                var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Start " + SelectedPSVGame.GameTitle + " using vita3k ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                var boxresult = await box.ShowWindowDialogAsync(this);
                                if (boxresult == ButtonResult.Yes)
                                {
                                    var EmulatorLauncherStartInfo = new ProcessStartInfo();
                                    var EmulatorLauncher = new Process() { StartInfo = EmulatorLauncherStartInfo };
                                    EmulatorLauncherStartInfo.FileName = Environment.CurrentDirectory + @"\Emulators\vita3k\Vita3K.exe";
                                    EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(Environment.CurrentDirectory + @"\Emulators\vita3k\Vita3K.exe");

                                    switch (SelectedPSVGame.GameFileType)
                                    {
                                        case PSVGame.GameFileTypes.PKG:
                                            {

                                                // Check if game is already installed
                                                if (!Directory.Exists(Vita3kPrefPath + @"ux0\app\" + SelectedPSVGame.GameID))
                                                {
                                                    var box2 = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Playing games in PKG format will require you to install them first with the zRIF key." + Environment.NewLine + Environment.NewLine + "The game will start automatically after the installation." + Environment.NewLine + Environment.NewLine + "Do you want to continue ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                                    var boxresult2 = await box2.ShowWindowDialogAsync(this);
                                                    if (boxresult2 == ButtonResult.Yes)
                                                    {

                                                        string RequiredzRIF = "";

                                                        if (SelectedPSVGame.ContentID != null)
                                                        {
                                                            // Get zRIF using the online or offline database or user input
                                                            RequiredzRIF = await GetzRIF(SelectedPSVGame.ContentID);
                                                            if (string.IsNullOrEmpty(RequiredzRIF))
                                                            {
                                                                var NewInputDialog = new InputDialog() { Title = "zRIF required" };
                                                                NewInputDialog.NewValueTextBox.Text = "";
                                                                NewInputDialog.InputDialogTitleTextBlock.Text = "Enter the zRIF Key string for the selected PKG:";
                                                                NewInputDialog.ConfirmButton.Content = "Confirm";

                                                                string UserzRIF = await NewInputDialog.ShowDialog<string>(this);
                                                                if (!string.IsNullOrEmpty(UserzRIF))
                                                                {
                                                                    RequiredzRIF = UserzRIF;
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            var NewInputDialog = new InputDialog() { Title = "zRIF required" };
                                                            NewInputDialog.NewValueTextBox.Text = "";
                                                            NewInputDialog.InputDialogTitleTextBlock.Text = "Enter the zRIF Key string for the selected PKG:";
                                                            NewInputDialog.ConfirmButton.Content = "Confirm";

                                                            string UserzRIF = await NewInputDialog.ShowDialog<string>(this);
                                                            if (!string.IsNullOrEmpty(UserzRIF))
                                                            {
                                                                RequiredzRIF = UserzRIF;
                                                            }
                                                        }

                                                        if (RequiredzRIF != null)
                                                        {
                                                            // Install PKG with zRIF key
                                                            EmulatorLauncherStartInfo.Arguments = "--pkg \"" + SelectedPSVGame.GameFilePath + "\" --zrif " + RequiredzRIF;
                                                            EmulatorLauncher.Start();
                                                            EmulatorLauncher.WaitForExit();
                                                            EmulatorLauncher.Dispose();

                                                            // Start after installation
                                                            EmulatorLauncher = new Process() { StartInfo = EmulatorLauncherStartInfo };
                                                            EmulatorLauncherStartInfo.Arguments = "-r " + SelectedPSVGame.GameID;
                                                            EmulatorLauncher.Start();
                                                        }
                                                        else
                                                        {
                                                            var box3 = MessageBoxManager.GetMessageBoxStandard("Error", "Cannot install the selected PKG without zRIF key.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                                            await box3.ShowWindowAsync();
                                                        }

                                                    }
                                                }
                                                else
                                                {
                                                    // Start an already installed game
                                                    EmulatorLauncherStartInfo.Arguments = "-r " + SelectedPSVGame.GameID;
                                                    EmulatorLauncher.Start();
                                                }

                                                break;
                                            }

                                        case PSVGame.GameFileTypes.Backup:
                                            {
                                                // Game will be decrypted automatically by vita3k if the selected game folder is still encrypted
                                                EmulatorLauncherStartInfo.Arguments = "\"" + SelectedPSVGame.GameFolderPath + "\"";
                                                EmulatorLauncher.Start();
                                                break;
                                            }
                                    }

                                }

                            }

                        }

                    }
                }

            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Cannot start vita3k." + Environment.NewLine + "Emulator pack is not installed.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private void NewContextMenu_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        NewContextMenu.Items.Clear();
    }

    private void NewContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PSVGame SelectedPSVGame = (PSVGame)GamesListBox.SelectedItem;

            NewContextMenu.Items.Add(CopyToMenuItem);
            NewContextMenu.Items.Add(PlayGameMenuItem);

            if ((int)SelectedPSVGame.GameFileType == (int)PS3Game.GameFileTypes.PKG)
            {
                NewContextMenu.Items.Add(PKGInfoMenuItem);
            }
        }
    }

    #endregion

    #region Menu Actions

    private async void LoadFolderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select your PSV backup folder" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            PKGCount = Directory.GetFiles(FBDResult, "*.pkg", SearchOption.AllDirectories).Length;
            FoldersCount = Directory.GetFiles(FBDResult, "*.sfo", SearchOption.AllDirectories).Length;

            NewLoadingWindow = new SyncWindow() { Title = "Loading PS Vita files", ShowActivated = true };
            NewLoadingWindow.LoadProgressBar.Maximum = FoldersCount;
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading folder 1 of " + FoldersCount.ToString();
            NewLoadingWindow.Show();

            try
            {
                // PSV encrypted/decrypted folders
                foreach (var Game in Directory.GetFiles(FBDResult, "*.sfo", SearchOption.AllDirectories))
                {
                    var NewPSVGame = new PSVGame() { GridWidth = 125, GridHeight = 175, ImageWidth = 100, ImageHeight = 128 };
                    Process SFOReader = new();
                    SFOReader.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "sfo.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "sfo");
                    SFOReader.StartInfo.Arguments = "\"" + Game + "\"";
                    SFOReader.StartInfo.RedirectStandardOutput = true;
                    SFOReader.StartInfo.UseShellExecute = false;
                    SFOReader.StartInfo.CreateNoWindow = true;
                    SFOReader.Start();

                    var OutputReader = SFOReader.StandardOutput;
                    string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

                    await SFOReader.WaitForExitAsync();
                    SFOReader.Close();

                    if (ProcessOutput.Length > 0)
                    {

                        // Load game infos
                        foreach (var Line in ProcessOutput)
                        {
                            if (Line.StartsWith("TITLE="))
                            {
                                NewPSVGame.GameTitle = Utils.CleanTitle(Line.Split('=')[1].Trim('"').Trim());
                            }
                            else if (Line.StartsWith("TITLE_ID="))
                            {
                                NewPSVGame.GameID = Line.Split('=')[1].Trim('"').Trim();
                            }
                            else if (Line.StartsWith("CATEGORY="))
                            {
                                NewPSVGame.GameCategory = PSVGame.GetCategory(Line.Split('=')[1].Trim('"'));
                            }
                            else if (Line.StartsWith("APP_VER="))
                            {
                                NewPSVGame.GameAppVer = Line.Split('=')[1].Trim('"').Trim();
                            }
                            else if (Line.StartsWith("PSP2_DISP_VER="))
                            {
                                NewPSVGame.GameRequiredFW = Line.Split('=')[1].Trim('"').Trim();
                            }
                            else if (Line.StartsWith("VERSION="))
                            {
                                NewPSVGame.GameVer = Line.Split('=')[1].Trim('"').Trim();
                            }
                            else if (Line.StartsWith("CONTENT_ID="))
                            {
                                NewPSVGame.ContentID = Line.Split('=')[1].Trim('"').Trim();
                            }
                        }

                        string PSVGAMEFolder = Path.GetDirectoryName(Directory.GetParent(Game)!.FullName)!;
                        long PSVGAMEFolderSize = Utils.GetDirectorySize(PSVGAMEFolder);

                        NewPSVGame.GameSize = Utils.HumanReadableBytes(PSVGAMEFolderSize);
                        NewPSVGame.GameFolderPath = PSVGAMEFolder;
                        NewPSVGame.GameFileType = PSVGame.GameFileTypes.Backup;

                        if (!string.IsNullOrEmpty(NewPSVGame.GameID))
                        {
                            NewPSVGame.GameRegion = PSVGame.GetGameRegion(NewPSVGame.GameID);
                            if (await Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png"))
                            {
                                if (Dispatcher.UIThread.CheckAccess() == false)
                                {
                                    await Dispatcher.UIThread.Invoke(async () =>
                                    {
                                        var TempBitmapImage = await AnyBitmap.FromUriAsync(new Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png", UriKind.Absolute));
                                        NewPSVGame.GameCoverSource = TempBitmapImage;
                                    });
                                }
                                else
                                {
                                    var TempBitmapImage = await AnyBitmap.FromUriAsync(new Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png", UriKind.Absolute));
                                    NewPSVGame.GameCoverSource = TempBitmapImage;
                                }
                            }
                        }

                        Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadProgressBar.Value += 1d);
                        Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadStatusTextBlock.Text = "Loading folder " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + FoldersCount.ToString());

                        // Add to the ListView
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(NewPSVGame));
                        }
                        else
                        {
                            GamesListBox.Items.Add(NewPSVGame);
                        }
                    }
                }

                // Reset
                Dispatcher.UIThread.Invoke(() =>
                {
                    NewLoadingWindow.LoadProgressBar.Value = 0;
                    NewLoadingWindow.LoadProgressBar.Maximum = PKGCount;
                    NewLoadingWindow.LoadStatusTextBlock.Text = "Loading PKG 1 of " + PKGCount.ToString();
                });

                // PSV PSN pkgs
                foreach (var GamePKG in Directory.GetFiles(FBDResult, "*.pkg", SearchOption.AllDirectories))
                {
                    var NewPSVGame = new PSVGame() { GridWidth = 125, GridHeight = 175, ImageWidth = 100, ImageHeight = 128 };
                    var GameInfo = new FileInfo(GamePKG);

                    Process SFOReader = new();
                    SFOReader.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "PSN_get_pkg_info.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "PSN_get_pkg_info");
                    SFOReader.StartInfo.Arguments = "\"" + GamePKG + "\"";
                    SFOReader.StartInfo.RedirectStandardOutput = true;
                    SFOReader.StartInfo.UseShellExecute = false;
                    SFOReader.StartInfo.CreateNoWindow = true;
                    SFOReader.Start();

                    var OutputReader = SFOReader.StandardOutput;
                    string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

                    await SFOReader.WaitForExitAsync();
                    SFOReader.Close();

                    if (ProcessOutput.Length > 0)
                    {

                        // Load game infos
                        foreach (var Line in ProcessOutput)
                        {
                            if (Line.StartsWith("Title:"))
                            {
                                NewPSVGame.GameTitle = Utils.CleanTitle(Line.Split(':')[1].Trim('"').Trim());
                            }
                            else if (Line.StartsWith("Title ID:"))
                            {
                                NewPSVGame.GameID = Line.Split(':')[1].Trim('"').Trim();
                            }
                            else if (Line.StartsWith("NPS Type:"))
                            {
                                NewPSVGame.GameCategory = Line.Split(':')[1].Trim('"').Trim();
                            }
                            else if (Line.StartsWith("App Ver:"))
                            {
                                NewPSVGame.GameAppVer = Line.Split(':')[1].Trim('"').Replace(",", "").Insert(1, ".");
                            }
                            else if (Line.StartsWith("Min FW:"))
                            {
                                NewPSVGame.GameRequiredFW = Line.Split(':')[1].Trim('"').Replace(",", "").Replace(".", "").Insert(2, ".");
                            }
                            else if (Line.StartsWith("Version:"))
                            {
                                NewPSVGame.GameVer = Line.Split(':')[1].Trim('"').Replace(",", "").Insert(1, ".");
                            }
                            else if (Line.StartsWith("Content ID:"))
                            {
                                NewPSVGame.ContentID = Line.Split(':')[1].Trim('"').Trim();
                            }
                            else if (Line.StartsWith("Region:"))
                            {
                                NewPSVGame.GameRegion = Line.Split(':')[1].Trim('"').Trim();
                            }
                        }
                        NewPSVGame.GameSize = Utils.HumanReadableBytes(GameInfo.Length);
                        NewPSVGame.GameFilePath = GamePKG;
                        NewPSVGame.GameFileType = PSVGame.GameFileTypes.PKG;
                        if (!string.IsNullOrEmpty(NewPSVGame.GameID))
                        {
                            if (await Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png"))
                            {
                                if (Dispatcher.UIThread.CheckAccess() == false)
                                {
                                    await Dispatcher.UIThread.Invoke(async () =>
                                    {
                                        var TempBitmapImage = await AnyBitmap.FromUriAsync(new Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png", UriKind.RelativeOrAbsolute));
                                        NewPSVGame.GameCoverSource = TempBitmapImage;
                                    });
                                }
                                else
                                {
                                    var TempBitmapImage = await AnyBitmap.FromUriAsync(new Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png", UriKind.RelativeOrAbsolute));
                                    NewPSVGame.GameCoverSource = TempBitmapImage;
                                }
                            }
                        }

                        Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadProgressBar.Value += 1);
                        Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadStatusTextBlock.Text = "Loading PKG " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + PKGCount.ToString());

                        // Add to the ListView
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(NewPSVGame));
                        }
                        else
                        {
                            GamesListBox.Items.Add(NewPSVGame);
                        }
                    }
                }

                NewLoadingWindow.Close();
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
    }

    private void LoadDLFolderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        Utils.OpenDownloadsFolder();
    }

    #endregion

    private void OpenPKGBrowser(object? sender, RoutedEventArgs e)
    {
        var NewPKGBrowser = new PKGBrowser() { Console = "PSV", ShowActivated = true };
        NewPKGBrowser.Show();
    }

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
            PSVGame SelectedPSVGame = (PSVGame)GamesListBox.SelectedItem;

            GameTitleTextBlock.Text = SelectedPSVGame.GameTitle;
            GameIDTextBlock.Text = "Title ID: " + SelectedPSVGame.GameID;
            GameContentIDTextBlock.Text = "Content ID: " + SelectedPSVGame.ContentID;
            GameRegionTextBlock.Text = "Region: " + SelectedPSVGame.GameRegion;
            GameVersionTextBlock.Text = "Game Version: " + SelectedPSVGame.GameVer;
            GameAppVersionTextBlock.Text = "Application Version: " + SelectedPSVGame.GameAppVer;
            GameCategoryTextBlock.Text = "Category: " + SelectedPSVGame.GameCategory;
            GameSizeTextBlock.Text = "Size: " + SelectedPSVGame.GameSize;
            GameRequiredFirmwareTextBlock.Text = "Required Firmware: " + SelectedPSVGame.GameRequiredFW;

            GameBackupTypeTextBlock.Text = "Backup Type: " + SelectedPSVGame.GameFileType.ToString();

            if (!string.IsNullOrEmpty(SelectedPSVGame.GameFilePath))
            {
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " + new DirectoryInfo(Path.GetDirectoryName(SelectedPSVGame.GameFilePath)!).Name;
            }
            else
            {
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " + new DirectoryInfo(SelectedPSVGame.GameFolderPath!).Name;
            }

        }
    }

    private void EMU_Settings_Click(object? sender, RoutedEventArgs e)
    {
        //var NewPSVEmulatorSettingsWindow = new PSVEmulatorSettings() { ShowActivated = true };
        //NewPSVEmulatorSettingsWindow.Show();
    }

    public async Task<string> GetzRIF(string PKGContentID)
    {
        var DownloadsList = new List<Structures.Package>();
        var box = MessageBoxManager.GetMessageBoxStandard("Update", "Load zRIF from the latest database ?", ButtonEnum.YesNo);
        var boxresult = await box.ShowWindowDialogAsync(this);
        if (boxresult == ButtonResult.Yes)
        {
            using var NewWebClient = new HttpClient();
            string GamesList = await NewWebClient.GetStringAsync("https://nopaystation.com/tsv/PSV_GAMES.tsv");
            string[] GamesListLines = GamesList.Split(Convert.ToChar("\r\n"));
            foreach (string GameLine in GamesListLines.Skip(1))
            {
                string[] SplittedValues = GameLine.Split(Convert.ToChar("\t"));
                var AdditionalInfo = Utils.GetFileSizeAndDate(SplittedValues[8].Trim(), SplittedValues[6].Trim());
                var NewPackage = new Structures.Package()
                {
                    PackageName = SplittedValues[2].Trim(),
                    PackageURL = SplittedValues[3].Trim(),
                    PackageTitleID = SplittedValues[0].Trim(),
                    PackageContentID = SplittedValues[5].Trim(),
                    PackagezRIF = SplittedValues[4].Trim(),
                    PackageDate = AdditionalInfo.FileDate,
                    PackageSize = AdditionalInfo.FileSize,
                    PackageRegion = SplittedValues[1].Trim()
                };
                if (!(SplittedValues[3].Trim() == "MISSING")) // Only add available PKGs
                {
                    DownloadsList.Add(NewPackage);
                }
            }
        }
        else if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Databases", "PSV_GAMES.tsv"))) // Use local .tsv file
        {
            string[] FileReader = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "Databases", "PSV_GAMES.tsv"), Encoding.UTF8);
            foreach (string GameLine in FileReader.Skip(1)) // Skip 1st line in TSV
            {
                string[] SplittedValues = GameLine.Split(Convert.ToChar("\t"));
                var AdditionalInfo = Utils.GetFileSizeAndDate(SplittedValues[8], SplittedValues[6]);
                var NewPackage = new Structures.Package()
                {
                    PackageName = SplittedValues[2],
                    PackageURL = SplittedValues[3],
                    PackageTitleID = SplittedValues[0],
                    PackageContentID = SplittedValues[5],
                    PackagezRIF = SplittedValues[4],
                    PackageDate = AdditionalInfo.FileDate,
                    PackageSize = AdditionalInfo.FileSize,
                    PackageRegion = SplittedValues[1]
                };
                if (!(SplittedValues[3] == "MISSING")) // Only add available PKGs
                {
                    DownloadsList.Add(NewPackage);
                }
            }
        }
        else
        {
            var newbox = MessageBoxManager.GetMessageBoxStandard("Could not load list", "Nothing available. Please add TSV files to the 'Databases' directory.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await newbox.ShowWindowAsync();
        }

        string zRIFStr = "";
        // Check if we have a zRIF for the selected .pkg
        foreach (Structures.Package AvailablePKG in DownloadsList)
        {
            if ((AvailablePKG.PackageContentID ?? "") == (PKGContentID ?? ""))
            {
                if (AvailablePKG.PackagezRIF is not null)
                {
                    zRIFStr = AvailablePKG.PackagezRIF;
                    break;
                }
            }
        }

        return zRIFStr;
    }

}