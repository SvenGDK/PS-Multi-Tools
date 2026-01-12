using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using DiscUtils.Iso9660;
using FluentFTP;
using IniParser;
using IniParser.Model;
using IronSoftware.Drawing;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using PSMultiTools.Dialogs;
using PSMultiTools.MultiPlatformTools;
using PSMultiTools.PS3.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace PSMultiTools.PS3;

public partial class PS3Library : Window
{

    public SyncWindow NewLoadingWindow = new() { Title = "Loading PS3 files", ShowActivated = true };

    public List<PS3Game> GamesList = [];
    public string ConsoleIP = "";
    public int FoldersCount = 0;
    public int PKGCount = 0;
    public int ISOCount = 0;

    public bool IsSoundPlaying = false;
    public bool AutoPlay = true;

    // Games context menu items
    public ContextMenu NewContextMenu = new();
    private readonly MenuItem CopyToMenuItem = new() { Header = "Copy to", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/copy-icon.png"))) } };
    private readonly MenuItem UploadToPS3MenuItem = new() { Header = "Upload to PS3", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/upload.png"))) } };
    private readonly MenuItem ExtractPKGMenuItem = new() { Header = "Extract .pkg", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/extract.png"))) } };
    private readonly MenuItem PlayMenuItem = new() { Header = "Play Soundtrack", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) } };
    private readonly MenuItem PKGInfoMenuItem = new() { Header = "PKG Details", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/information.png"))) } };
    private readonly MenuItem PlayGameMenuItem = new() { Header = "Play with rpcs3", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/controller.png"))) } };

    // ISO tools context menu items
    private readonly MenuItem ISOToolsMenuItem = new() { Header = "ISO Tools", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/isodisc.png"))) } };
    private readonly MenuItem ExtractISOMenuItem = new() { Header = "Extract ISO", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/extract.png"))) } };
    private readonly MenuItem CreateISOMenuItem = new() { Header = "Create ISO", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/create.png"))) } };
    private readonly MenuItem PatchISOMenuItem = new() { Header = "Patch ISO", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/patch.png"))) } };
    private readonly MenuItem SplitISOMenuItem = new() { Header = "Split ISO", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/split.png"))) } };
    private readonly MenuItem DecryptISOMenuItem = new() { Header = "Decrypt ISO", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/decrypt.png"))) } };

    // webMAN MOD ISO utilities context menu items
    private readonly MenuItem MountISOMenuItem = new() { Header = "Mount selected game", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/isodisc.png"))) } };
    private readonly MenuItem MountAndPlayISOMenuItem = new() { Header = "Mount & Play selected game", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/isodisc.png"))) } };

    // Supplemental library menu items
    private readonly MenuItem LoadFolderMenuItem = new() { Header = "Open a folder with backups" };
    private readonly MenuItem LoadRemoteFolderMenuItem = new() { Header = "Load installed games & apps from PS3 over FTP" };
    private readonly MenuItem LoadDLFolderMenuItem = new() { Header = "Open Downloads folder" };

    // Supplemental emulator menu item
    public MenuItem EMU_Settings = new() { Header = "RPCS3 Settings" };

    public PS3Library()
    {
        InitializeComponent();

        Loaded += PS3Library_Loaded;

        NewPS3Menu.IPChanged += NewPS3Menu_IPTextChanged;

        GamesListBox.PointerWheelChanged += GamesListBox_PointerWheelChanged;
        GamesListBox.SelectionChanged += GamesListBox_SelectionChanged;

        NewContextMenu.Opening += NewContextMenu_Opening;
        NewContextMenu.Closing += NewContextMenu_Closing;

        ExtractISOMenuItem.Click += ExtractISOMenuItem_Click;
        CreateISOMenuItem.Click += CreateISOMenuItem_Click;
        PatchISOMenuItem.Click += PatchISOMenuItem_Click;
        SplitISOMenuItem.Click += SplitISOMenuItem_Click;
        DecryptISOMenuItem.Click += DecryptISOMenuItem_Click;
        MountISOMenuItem.Click += MountISOMenuItem_Click;
        MountAndPlayISOMenuItem.Click += MountAndPlayISOMenuItem_Click;
        ExtractPKGMenuItem.Click += ExtractPKGMenuItem_Click;
        PlayMenuItem.Click += PlayMenuItem_Click;
        PKGInfoMenuItem.Click += PKGInfoMenuItem_Click;
        CopyToMenuItem.Click += CopyToMenuItem_Click;
        PlayGameMenuItem.Click += PlayGameMenuItem_Click;
        LoadFolderMenuItem.Click += LoadFolderMenuItem_Click;
        LoadRemoteFolderMenuItem.Click += LoadRemoteFolderMenuItem_Click;
        LoadDLFolderMenuItem.Click += LoadDLFolderMenuItem_Click;
        EMU_Settings.Click += EMU_Settings_Click;
    }

    private void PS3Library_Loaded(object? sender, RoutedEventArgs e)
    {
        // Add some tooltips
        ToolTip.SetTip(LoadFolderMenuItem, "This option allows you to select & load any folder on your PC that contains all your backups. Do not select the root (main) folder of a drive or it will fail.");
        ToolTip.SetTip(LoadRemoteFolderMenuItem, "This option will load all your installed game & applications from your PS3 when an IP address has been set in the 'Settings'.");

        // Add supplemental library menu items that will be handled in the app
        MenuItem LibraryMenuItem = (MenuItem)NewPS3Menu.MainMenu.Items[0]!;
        LibraryMenuItem.Items.Add(LoadFolderMenuItem);
        LibraryMenuItem.Items.Add(LoadRemoteFolderMenuItem);
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem);

        // Add the new PKG Browser
        var PKGDownloaderMenuItem = new MenuItem() { Header = "PKG Browser & Downloader" };
        PKGDownloaderMenuItem.Click += OpenPKGBrowser;
        NewPS3Menu.MainMenu.Items.Insert(5, PKGDownloaderMenuItem);

        // Add supplemental emulator menu item
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Emulators", "rpcs3", "rpcs3.exe")))
        {
            NewPS3Menu.MainMenu.Items.Add(EMU_Settings);
        }

        // Load available context menu options
        GamesListBox.ContextMenu = NewContextMenu;

        // Set background fading animation
        BlurringShape.Transitions = [new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(500), Easing = new CubicEaseOut() }];

        // Load config if exists
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini")))
        {
            try
            {
                var PSMTConfigParser = new FileIniDataParser();
                IniData PSMTConfigData = PSMTConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini"));
                ConsoleIP = PSMTConfigData["PS3 Tools"]["IP"];

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
    }

    private void NewPS3Menu_IPTextChanged(object? sender, RoutedEventArgs e)
    {
        ConsoleIP = NewPS3Menu.SharedConsoleAddress;

        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini")))
        {
            try
            {
                var PSMTConfigParser = new FileIniDataParser();
                IniData PSMTConfigData = PSMTConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini"));
                PSMTConfigData["PS3 Tools"]["IP"] = NewPS3Menu.SharedConsoleAddress;
                PSMTConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini"), PSMTConfigData);
            }
            catch (Exception)
            {
            }
        }
    }

    #region Game Loader

    public enum LoadType
    {
        BackupFolder,
        FTP
    }

    public struct GameLoaderArgs
    {
        public LoadType Type { get; set; }

        public string FolderPath { get; set; }

        public string ConsoleIP { get; set; }
    }

    private async void ProcessBackups(GameLoaderArgs WorkerArgs)
    {
        await Task.Run(async () =>
        {
            if (WorkerArgs.Type == LoadType.FTP)
            {
                try
                {
                    using var conn = new FtpClient(WorkerArgs.ConsoleIP, "anonymous", "anonymous", 21);
                    // Configurate the FTP connection
                    conn.Config.ValidateAnyCertificate = true;
                    conn.Config.SslProtocols = SslProtocols.None;
                    conn.Config.DataConnectionEncryption = false;
                    conn.Config.DataConnectionType = FtpDataConnectionType.PASV;

                    // Connect
                    conn.Connect();

                    // Get /dev_hdd0/game
                    if (conn.DirectoryExists("/dev_hdd0/game"))
                    {
                        //foreach (var item in conn.GetListing("/dev_hdd0/game"))
                        //var NewPS3Game = new PS3Game();
                    }

                    // Get /dev_hdd0/GAMES
                    if (conn.DirectoryExists("/dev_hdd0/GAMES"))
                    {
                        foreach (var item in conn.GetListing("/dev_hdd0/GAMES"))
                        {
                            var NewPS3Game = new PS3Game();
                            if (item.Type == FtpObjectType.Directory)
                            {
                                if (conn.DirectoryExists(item.FullName + "/PS3_GAME"))
                                {
                                    if (conn.FileExists(item.FullName + "/PS3_GAME/PARAM.SFO"))
                                    {

                                    }
                                }
                            }
                        }
                    }

                    // Get PS3ISO games
                    if (conn.DirectoryExists("/dev_hdd0/PS3ISO"))
                    {
                        foreach (var item in conn.GetListing("/dev_hdd0/PS3ISO"))
                        {
                            if (item.Type == FtpObjectType.File)
                            {
                                if (item.Name.EndsWith(".iso"))
                                {

                                    // Dim ISOCacheFolderName As String = Path.GetFileNameWithoutExtension(item.FullName)
                                    // Dim FullFTPPath As String = "ftp://" + WorkerArgs.ConsoleIP + item.FullName

                                    var NewPS3Game = new PS3Game()
                                    {
                                        GridWidth = 210d,
                                        GridHeight = 210d,
                                        ImageWidth = 200d,
                                        ImageHeight = 200d,
                                        GameSize = Utils.HumanReadableBytes(item.Size),
                                        GameFilePath = item.FullName,
                                        GameFileType = PS3Game.GameFileTypes.PS3ISO,
                                        GameRootLocation = PS3Game.GameLocation.WebMANMOD,
                                        GameTitle = item.Name
                                    };
                                    GamesList.Add(NewPS3Game);
                                    await Dispatcher.UIThread.Invoke(async () => NewPS3Game.GameCoverSource = AnyBitmap.FromStream(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/PS3Disc.png"))));

                                    // Add to the ListView
                                    Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(NewPS3Game));
                                }
                            }
                        }
                    }

                    // Get PS2ISO games
                    if (conn.DirectoryExists("/dev_hdd0/PS2ISO"))
                    {
                        foreach (var item in conn.GetListing("/dev_hdd0/PS2ISO"))
                        {
                            if (item.Type == FtpObjectType.File)
                            {
                                if (item.Name.EndsWith(".bin.enc"))
                                {
                                    var NewPS3Game = new PS3Game()
                                    {
                                        GridWidth = 210d,
                                        GridHeight = 210d,
                                        ImageWidth = 200d,
                                        ImageHeight = 200d,
                                        GameSize = Utils.HumanReadableBytes(item.Size),
                                        GameFilePath = item.FullName,
                                        GameFileType = PS3Game.GameFileTypes.PS2ISO,
                                        GameRootLocation = PS3Game.GameLocation.WebMANMOD,
                                        GameTitle = item.Name
                                    };
                                    GamesList.Add(NewPS3Game);
                                    Dispatcher.UIThread.Invoke(() => NewPS3Game.GameCoverSource = AnyBitmap.FromStream(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/PS2Disc.png"))));

                                    // Add to the ListView
                                    Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(NewPS3Game));
                                }
                            }
                        }
                    }

                    // Get PSXISO games
                    if (conn.DirectoryExists("/dev_hdd0/PSXISO"))
                    {
                        foreach (var item in conn.GetListing("/dev_hdd0/PSXISO"))
                        {
                            if (item.Type == FtpObjectType.File)
                            {
                                if (item.Name.EndsWith(".bin"))
                                {
                                    var NewPS3Game = new PS3Game()
                                    {
                                        GridWidth = 210d,
                                        GridHeight = 210d,
                                        ImageWidth = 200d,
                                        ImageHeight = 200d,
                                        GameSize = Utils.HumanReadableBytes(item.Size),
                                        GameFilePath = item.FullName,
                                        GameFileType = PS3Game.GameFileTypes.PSXISO,
                                        GameRootLocation = PS3Game.GameLocation.WebMANMOD,
                                        GameTitle = item.Name
                                    };
                                    GamesList.Add(NewPS3Game);

                                    Dispatcher.UIThread.Invoke(() => NewPS3Game.GameCoverSource = AnyBitmap.FromStream(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/PS1Disc.png"))));

                                    // Add to the ListView
                                    Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(NewPS3Game));
                                }
                            }
                        }
                    }

                    // Get PSPISO games
                    if (conn.DirectoryExists("/dev_hdd0/PSPISO"))
                    {
                        foreach (var item in conn.GetListing("/dev_hdd0/PSPISO"))
                        {
                            if (item.Type == FtpObjectType.File)
                            {
                                if (item.Name.EndsWith(".iso"))
                                {
                                    var NewPS3Game = new PS3Game()
                                    {
                                        GridWidth = 210d,
                                        GridHeight = 210d,
                                        ImageWidth = 200d,
                                        ImageHeight = 200d,
                                        GameSize = Utils.HumanReadableBytes(item.Size),
                                        GameFilePath = item.FullName,
                                        GameFileType = PS3Game.GameFileTypes.PSPISO,
                                        GameRootLocation = PS3Game.GameLocation.WebMANMOD,
                                        GameTitle = item.Name
                                    };
                                    GamesList.Add(NewPS3Game);
                                    Dispatcher.UIThread.Invoke(() => NewPS3Game.GameCoverSource = AnyBitmap.FromUri(new Uri("/Images/UMD.png", UriKind.Relative)));

                                    // Add to the ListView
                                    Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(NewPS3Game));
                                }
                            }
                        }
                    }

                    // Disconnect
                    conn.Disconnect();
                }
                catch (Exception ex)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowAsPopupAsync(this);
                    });
                }
            }
            else if (WorkerArgs.Type == LoadType.BackupFolder)
            {
                try
                {
                    var FolderBackups = Directory.EnumerateFiles(WorkerArgs.FolderPath, "*.SFO", SearchOption.AllDirectories);
                    var PKGBackups = Directory.EnumerateFiles(WorkerArgs.FolderPath, "*.pkg", SearchOption.AllDirectories);
                    var ISOBackups = Directory.EnumerateFiles(WorkerArgs.FolderPath, "*.iso", SearchOption.AllDirectories);

                    FoldersCount = FolderBackups.Count();
                    PKGCount = PKGBackups.Count();
                    ISOCount = ISOBackups.Count();

                    Dispatcher.UIThread.Invoke(() =>
                    {
                        NewLoadingWindow.LoadProgressBar.Maximum = FoldersCount + ISOCount + PKGCount;
                        NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + (FoldersCount + ISOCount + PKGCount).ToString();
                    });

                    // PS3 classic backup folders
                    foreach (string FolderBackup in FolderBackups)
                    {
                        try
                        {
                            var NewPS3Game = new PS3Game() { GridWidth = 325, GridHeight = 180, ImageWidth = 320, ImageHeight = 176 };

                            string[] ProcessOutput;
                            using (var SFOReader = new Process())
                            {
                                SFOReader.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "sfo.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "sfo");
                                SFOReader.StartInfo.Arguments = "\"" + FolderBackup + "\" --decimal";
                                SFOReader.StartInfo.RedirectStandardOutput = true;
                                SFOReader.StartInfo.UseShellExecute = false;
                                SFOReader.StartInfo.CreateNoWindow = true;
                                SFOReader.Start();

                                var OutputReader = SFOReader.StandardOutput;
                                ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
                            }

                            if (ProcessOutput.Length > 0)
                            {
                                // Load game infos
                                foreach (var Line in ProcessOutput)
                                {
                                    if (Line.StartsWith("TITLE="))
                                    {
                                        NewPS3Game.GameTitle = Utils.CleanTitle(Line.Split('=')[1].Trim('"'));
                                    }
                                    else if (Line.StartsWith("TITLE_ID="))
                                    {
                                        NewPS3Game.GameID = Line.Split('=')[1].Trim('"');
                                    }
                                    else if (Line.StartsWith("CATEGORY="))
                                    {
                                        NewPS3Game.GameCategory = PS3Game.GetCategory(Line.Split('=')[1].Trim('"'));
                                    }
                                    else if (Line.StartsWith("APP_VER="))
                                    {
                                        NewPS3Game.GameAppVer = $"{Line.Split('=')[1].Trim('"'):N2}";
                                    }
                                    else if (Line.StartsWith("PS3_SYSTEM_VER="))
                                    {
                                        NewPS3Game.GameRequiredFW = $"{Line.Split('=')[1].Trim('"'):N2}";
                                    }
                                    else if (Line.StartsWith("VERSION="))
                                    {
                                        NewPS3Game.GameVer = "Version: " + $"{Line.Split('=')[1].Trim('"'):N2}";
                                    }
                                    else if (Line.StartsWith("RESOLUTION="))
                                    {
                                        NewPS3Game.GameResolution = PS3Game.GetGameResolution(Line.Split('=')[1].Trim('"'));
                                    }
                                    else if (Line.StartsWith("SOUND_FORMAT="))
                                    {
                                        NewPS3Game.GameSoundFormat = PS3Game.GetGameSoundFormat(Line.Split('=')[1].Trim('"'));
                                    }
                                }

                                // Load game files
                                string PS3GAMEFolder = Path.GetDirectoryName(FolderBackup)!;
                                if (File.Exists(Path.Combine(PS3GAMEFolder, "ICON0.PNG")))
                                {
                                    Dispatcher.UIThread.Invoke(() =>
                                    {
                                        var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(PS3GAMEFolder, "ICON0.PNG"));
                                        NewPS3Game.GameCoverSource = TempBitmapImage;
                                    });
                                }
                                if (File.Exists(Path.Combine(PS3GAMEFolder, "PIC1.PNG")))
                                {
                                    Dispatcher.UIThread.Invoke(() =>
                                    {
                                        var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(PS3GAMEFolder, "PIC1.PNG"));
                                        NewPS3Game.GameBackgroundSource = TempBitmapImage;
                                    });
                                }
                                if (File.Exists(Path.Combine(PS3GAMEFolder, "SND0.AT3")))
                                {
                                    NewPS3Game.GameBackgroundSoundFile = Path.Combine(PS3GAMEFolder, "SND0.AT3");
                                }

                                long PS3GAMEFolderSize = Utils.GetDirectorySize(PS3GAMEFolder);
                                NewPS3Game.GameSize = Utils.HumanReadableBytes(PS3GAMEFolderSize);
                                NewPS3Game.GameFolderPath = Directory.GetParent(PS3GAMEFolder)!.FullName;

                                NewPS3Game.GameFileType = PS3Game.GameFileTypes.Backup;
                                NewPS3Game.GameRootLocation = PS3Game.GameLocation.Local;

                                if (!string.IsNullOrWhiteSpace(NewPS3Game.GameID))
                                {
                                    NewPS3Game.GameRegion = PS3Game.GetGameRegion(NewPS3Game.GameID);
                                }

                                // Update progress
                                Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadProgressBar.Value += 1);
                                Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadStatusTextBlock.Text = "Loading folders " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " / " + NewLoadingWindow.LoadProgressBar.Maximum.ToString());

                                GamesList.Add(NewPS3Game);

                                // Add to the ListView
                                Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(NewPS3Game));
                            }
                            else
                            {
                                continue;
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            continue;
                        }
                        catch (IOException)
                        {
                            continue;
                        }
                        catch (Exception ex)
                        {
                            await Dispatcher.UIThread.Invoke(async () =>
                            {
                                var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                await box.ShowAsPopupAsync(this);
                            });
                            continue;
                        }
                    }

                    // PS3 PKGs
                    foreach (string GamePKG in PKGBackups)
                    {

                        var NewPS3Game = new PS3Game() { GridWidth = 325, GridHeight = 180, ImageWidth = 320, ImageHeight = 176 };
                        var PKGFileInfo = new FileInfo(GamePKG);
                        var NewPKGDecryptor = new PKGDecryptor();

                        try
                        {
                            // Decrypt pkg file
                            NewPKGDecryptor.ProcessPKGFile(GamePKG);

                            // Load game infos
                            if (NewPKGDecryptor.GetPARAMSFO is not null)
                            {
                                var SFOKeys = SFONew.ReadSfo(NewPKGDecryptor.GetPARAMSFO);
                                if (SFOKeys.TryGetValue("TITLE", out var TITLEValue))
                                {
                                    NewPS3Game.GameTitle = Utils.CleanTitle(TITLEValue.ToString()!);
                                }
                                if (SFOKeys.TryGetValue("TITLE_ID", out var TITLEIDValue))
                                {
                                    NewPS3Game.GameID = TITLEIDValue.ToString();
                                }
                                if (SFOKeys.TryGetValue("CATEGORY", out var CATEGORYValue))
                                {
                                    NewPS3Game.GameCategory = PS3Game.GetCategory(CATEGORYValue.ToString()!);
                                }
                                if (SFOKeys.TryGetValue("CONTENT_ID", out var CONTENTIDValue))
                                {
                                    NewPS3Game.ContentID = CONTENTIDValue.ToString();
                                }
                                if (SFOKeys.TryGetValue("APP_VER", out var APPVERValue))
                                {
                                    string AppVer = APPVERValue.ToString()![..5];
                                    NewPS3Game.GameAppVer = AppVer;
                                }
                                if (SFOKeys.TryGetValue("PS3_SYSTEM_VER", out var PS3SYSTEMVERValue))
                                {
                                    string SystemVer = PS3SYSTEMVERValue.ToString()![..5];
                                    NewPS3Game.GameRequiredFW = SystemVer;
                                }
                                if (SFOKeys.TryGetValue("VERSION", out var VERSIONValue))
                                {
                                    string Ver = VERSIONValue.ToString()![..5];
                                    NewPS3Game.GameVer = Ver;
                                }
                                if (SFOKeys.TryGetValue("RESOLUTION", out var RESOLUTIONValue))
                                {
                                    NewPS3Game.GameResolution = PS3Game.GetGameResolution(RESOLUTIONValue.ToString()!);
                                }
                                if (SFOKeys.TryGetValue("SOUND_FORMAT", out var SOUNDFORMATValue))
                                {
                                    NewPS3Game.GameSoundFormat = PS3Game.GetGameSoundFormat(SOUNDFORMATValue.ToString()!);
                                }
                            }
                            else
                            {
                                continue;
                            }

                            NewPS3Game.GameSize = Utils.HumanReadableBytes(PKGFileInfo.Length);
                            NewPS3Game.GameFileType = PS3Game.GameFileTypes.PKG;
                            NewPS3Game.GameRootLocation = PS3Game.GameLocation.Local;

                            if (!string.IsNullOrWhiteSpace(NewPS3Game.GameID))
                            {
                                NewPS3Game.GameRegion = PS3Game.GetGameRegion(NewPS3Game.GameID);
                            }

                            NewPS3Game.GameFilePath = GamePKG;

                            // Check for additional content
                            if (NewPKGDecryptor.ICON0 is not null)
                            {
                                Dispatcher.UIThread.Invoke(() => NewPS3Game.GameCoverSource = NewPKGDecryptor.ICON0);
                            }
                            if (NewPKGDecryptor.PIC1 is not null)
                            {
                                Dispatcher.UIThread.Invoke(() => NewPS3Game.GameBackgroundSource = NewPKGDecryptor.PIC1);
                            }
                            if (NewPKGDecryptor.SND0 is not null)
                            {
                                Dispatcher.UIThread.Invoke(() => NewPS3Game.GameBackgroundSoundBytes = NewPKGDecryptor.SND0);
                            }

                            // Update progress
                            Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadProgressBar.Value += 1);
                            Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadStatusTextBlock.Text = "Loading PKGs " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " / " + NewLoadingWindow.LoadProgressBar.Maximum.ToString());

                            if (NewPS3Game.GameTitle is not null)
                            {
                                // Add
                                GamesList.Add(NewPS3Game);
                                Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(NewPS3Game));
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            continue;
                        }
                        catch (IOException)
                        {
                            continue;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    // PS3 ISOs
                    foreach (string GameISO in ISOBackups)
                    {
                        try
                        {
                            var NewPS3Game = new PS3Game() { GridWidth = 325, GridHeight = 180, ImageWidth = 320, ImageHeight = 176 };
                            var ISOFileInfo = new FileInfo(GameISO);
                            string ISOFileNameWithoutExt = Path.GetFileNameWithoutExtension(ISOFileInfo.Name);
                            string CachePath = Path.Combine(Environment.CurrentDirectory, "Cache");
                            bool Extracted = false;

                            // Create cache dir for PS3 games
                            if (!Directory.Exists(CachePath))
                            {
                                Directory.CreateDirectory(CachePath);
                            }

                            // Identify ISO
                            string IdentifiedISO = "";

                            try
                            {
                                using var NewISOStream = File.Open(GameISO, FileMode.Open, FileAccess.Read, FileShare.Read);
                                var NewCDReader = new CDReader(NewISOStream, true);
                                try
                                {
                                    if (NewCDReader.DirectoryExists("PS3_GAME"))
                                    {
                                        IdentifiedISO = "PS3";
                                    }
                                    else if (NewCDReader.DirectoryExists("PSP_GAME"))
                                    {
                                        IdentifiedISO = "PSP";
                                    }

                                    if (NewCDReader.FileExists("SYSTEM.CNF"))
                                    {
                                        IdentifiedISO = "PS2";
                                    }
                                }
                                catch (Exception)
                                {
                                    // No valid ISO
                                    continue;
                                }
                            }
                            catch (Exception)
                            {
                                // Unreadable ISO
                                continue;
                            }

                            if (IdentifiedISO == "PS3")
                            {

                                NewPS3Game.GameFileType = PS3Game.GameFileTypes.PS3ISO;

                                // Extract files to display infos
                                if (!File.Exists(Path.Combine(CachePath, ISOFileNameWithoutExt, "PARAM.SFO")))
                                {
                                    try
                                    {
                                        Extracted = await Utils.ExtractISOFiles(GameISO, CachePath, true);
                                    }
                                    catch (Exception)
                                    {
                                        Console.WriteLine($"Error extracting files from ISO {GameISO}");
                                        Trace.WriteLine($"Error extracting files from ISO {GameISO}");
                                    }
                                }

                                try
                                {
                                    // Check if ISO is encrypted/decrypted
                                    using var NewISOStream = File.Open(GameISO, FileMode.Open, FileAccess.Read, FileShare.Read);
                                    var NewCDReader = new CDReader(NewISOStream, true);
                                    using Stream NewFileStream = NewCDReader.OpenFile(@"PS3_GAME\USRDIR\EBOOT.BIN", FileMode.Open);
                                    var TempBuffer = new byte[3];
                                    NewFileStream.ReadExactly(TempBuffer);
                                    string Output = System.Text.Encoding.ASCII.GetString(TempBuffer);
                                    if (!string.IsNullOrEmpty(Output))
                                    {
                                        if (Output == "SCE")
                                        {
                                            NewPS3Game.ISOEncryption = "Decrypted";
                                        }
                                        else
                                        {
                                            NewPS3Game.ISOEncryption = "Encrypted";
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    // No valid PS3 ISO
                                    Console.WriteLine($"Could not check encryption status {GameISO}");
                                    Trace.WriteLine($"Could not check encryption status {GameISO}");
                                    continue;
                                }

                                if (Extracted)
                                {
                                    // Read PARAM.SFO
                                    if (File.Exists(Path.Combine(CachePath, ISOFileNameWithoutExt, "PS3_GAME", "PARAM.SFO")))
                                    {
                                        try
                                        {
                                            using var ParamFileStream = new FileStream(Path.Combine(CachePath, ISOFileNameWithoutExt, "PS3_GAME", "PARAM.SFO"), FileMode.Open, FileAccess.Read);
                                            var SFOKeys = SFONew.ReadSfo(ParamFileStream);
                                            if (SFOKeys is not null && SFOKeys.Count > 0)
                                            {
                                                if (SFOKeys.TryGetValue("TITLE", out var TITLEValue))
                                                {
                                                    NewPS3Game.GameTitle = Utils.CleanTitle(TITLEValue.ToString()!);
                                                }
                                                if (SFOKeys.TryGetValue("TITLE_ID", out var TITLEIDValue))
                                                {
                                                    NewPS3Game.GameID = TITLEIDValue.ToString();
                                                }
                                                if (SFOKeys.TryGetValue("CATEGORY", out var CATEGORYValue))
                                                {
                                                    NewPS3Game.GameCategory = PS3Game.GetCategory(CATEGORYValue.ToString()!);
                                                }
                                                if (SFOKeys.TryGetValue("CONTENT_ID", out var CONTENTIDValue))
                                                {
                                                    NewPS3Game.ContentID = CONTENTIDValue.ToString();
                                                }
                                                if (SFOKeys.TryGetValue("APP_VER", out var APPVERValue))
                                                {
                                                    string AppVer = APPVERValue.ToString()![..5];
                                                    NewPS3Game.GameAppVer = AppVer;
                                                }
                                                if (SFOKeys.TryGetValue("PS3_SYSTEM_VER", out var PS3SYSTEMVERValue))
                                                {
                                                    string SystemVer = PS3SYSTEMVERValue.ToString()![..5];
                                                    NewPS3Game.GameRequiredFW = SystemVer;
                                                }
                                                if (SFOKeys.TryGetValue("VERSION", out var VERSIONValue))
                                                {
                                                    string Ver = VERSIONValue.ToString()![..5];
                                                    NewPS3Game.GameVer = Ver;
                                                }
                                                if (SFOKeys.TryGetValue("RESOLUTION", out var RESOLUTIONValue))
                                                {
                                                    NewPS3Game.GameResolution = PS3Game.GetGameResolution(RESOLUTIONValue.ToString()!);
                                                }
                                                if (SFOKeys.TryGetValue("SOUND_FORMAT", out var SOUNDFORMATValue))
                                                {
                                                    NewPS3Game.GameSoundFormat = PS3Game.GetGameSoundFormat(SOUNDFORMATValue.ToString()!);
                                                }
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            // No valid PS3 ISO
                                            Console.WriteLine($"Could not parse PARAM.SFO {GameISO}");
                                            Trace.WriteLine($"Could not parse PARAM.SFO {GameISO}");
                                            continue;
                                        }
                                    }

                                    // Load game files
                                    if (File.Exists(Path.Combine(CachePath, ISOFileNameWithoutExt, "PS3_GAME", "ICON0.PNG")))
                                    {
                                        Dispatcher.UIThread.Invoke(() => NewPS3Game.GameCoverSource = AnyBitmap.FromFile(Path.Combine(CachePath, ISOFileNameWithoutExt, "PS3_GAME", "ICON0.PNG")));
                                    }
                                    if (File.Exists(Path.Combine(CachePath, ISOFileNameWithoutExt, "PS3_GAME", "PIC1.PNG")))
                                    {
                                        Dispatcher.UIThread.Invoke(() => NewPS3Game.GameBackgroundPath = Path.Combine(CachePath, ISOFileNameWithoutExt, "PS3_GAME", "PIC1.PNG"));
                                    }
                                    if (File.Exists(Path.Combine(CachePath, ISOFileNameWithoutExt, "PS3_GAME", "SND0.AT3")))
                                    {
                                        NewPS3Game.GameBackgroundSoundFile = Path.Combine(CachePath, ISOFileNameWithoutExt, "PS3_GAME", "SND0.AT3");
                                    }
                                }
                            }
                            else if (IdentifiedISO == "PSP")
                            {
                                NewPS3Game.GameFileType = PS3Game.GameFileTypes.PSPISO;

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
                                    if (File.Exists(Path.Combine(CachePath, ISOFileNameWithoutExt, "PSP_GAME", "PARAM.SFO")))
                                    {
                                        try
                                        {
                                            using var ParamFileStream = new FileStream(Path.Combine(CachePath, ISOFileNameWithoutExt, "PSP_GAME", "PARAM.SFO"), FileMode.Open, FileAccess.Read);
                                            var SFOKeys = SFONew.ReadSfo(ParamFileStream);
                                            if (SFOKeys is not null && SFOKeys.Count > 0)
                                            {
                                                if (SFOKeys.TryGetValue("TITLE", out var TITLEValue))
                                                {
                                                    NewPS3Game.GameTitle = Utils.CleanTitle(TITLEValue.ToString()!);
                                                }
                                                if (SFOKeys.TryGetValue("DISC_ID", out var TITLEIDValue))
                                                {
                                                    NewPS3Game.GameID = TITLEIDValue.ToString();
                                                }
                                                if (SFOKeys.TryGetValue("CATEGORY", out var CATEGORYValue))
                                                {
                                                    NewPS3Game.GameCategory = PSPGame.GetCategory(CATEGORYValue.ToString()!);
                                                }
                                                if (SFOKeys.TryGetValue("PSP_SYSTEM_VER", out var PSPSYSTEMVERValue))
                                                {
                                                    string SystemVer = PSPSYSTEMVERValue.ToString()!;
                                                    NewPS3Game.GameRequiredFW = SystemVer;
                                                }
                                                if (SFOKeys.TryGetValue("DISC_VERSION", out var VERSIONValue))
                                                {
                                                    string Ver = VERSIONValue.ToString()!;
                                                    NewPS3Game.GameVer = Ver;
                                                }
                                            }
                                        }
                                        catch { }
                                    }

                                    // Load game files
                                    if (File.Exists(Path.Combine(CachePath, ISOFileNameWithoutExt, "PSP_GAME", "ICON0.PNG")))
                                    {
                                        Dispatcher.UIThread.Invoke(() => NewPS3Game.GameCoverSource = AnyBitmap.FromFile(Path.Combine(CachePath, ISOFileNameWithoutExt, "PSP_GAME", "ICON0.PNG")));
                                    }
                                    if (File.Exists(Path.Combine(CachePath, ISOFileNameWithoutExt, "PSP_GAME", "PIC1.PNG")))
                                    {
                                        Dispatcher.UIThread.Invoke(() => NewPS3Game.GameBackgroundPath = Path.Combine(CachePath, ISOFileNameWithoutExt, "PSP_GAME", "PIC1.PNG"));
                                    }
                                    if (File.Exists(Path.Combine(CachePath, ISOFileNameWithoutExt, "PSP_GAME", "SND0.AT3")))
                                    {
                                        NewPS3Game.GameBackgroundSoundFile = Path.Combine(CachePath, ISOFileNameWithoutExt, "PSP_GAME", "SND0.AT3");
                                    }
                                }
                            }
                            else if (IdentifiedISO == "PS2")
                            {
                                NewPS3Game.GameFileType = PS3Game.GameFileTypes.PS2ISO;

                                string? GameID = PS2Game.GetPS2GameID(GameISO).Replace(".", "").Replace("_", "-").Trim();
                                if (!string.IsNullOrEmpty(GameID))
                                {
                                    NewPS3Game.GameID = GameID;

                                    if (await Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameID + ".jpg"))
                                    {
                                        if (Dispatcher.UIThread.CheckAccess() == false)
                                        {
                                            await Dispatcher.UIThread.InvokeAsync(async () =>
                                            {
                                                var TempBitmapImage = await AnyBitmap.FromUriAsync(new Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameID + ".jpg"));
                                                NewPS3Game.GameCoverSource = TempBitmapImage;
                                            });
                                        }
                                        else
                                        {
                                            var TempBitmapImage = await AnyBitmap.FromUriAsync(new Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameID + ".jpg"));
                                            NewPS3Game.GameCoverSource = TempBitmapImage;
                                        }
                                    }
                                }
                            }

                            NewPS3Game.GameSize = Utils.HumanReadableBytes(ISOFileInfo.Length);

                            if (!string.IsNullOrWhiteSpace(NewPS3Game.GameID))
                            {
                                NewPS3Game.GameRegion = PSPGame.GetGameRegion(NewPS3Game.GameID);
                            }

                            NewPS3Game.GameFilePath = GameISO;
                            NewPS3Game.GameFileType = PS3Game.GameFileTypes.PSXISO;
                            NewPS3Game.GameRootLocation = PS3Game.GameLocation.Local;

                            // Update progress
                            Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadProgressBar.Value += 1);
                            Dispatcher.UIThread.Invoke(() => NewLoadingWindow.LoadStatusTextBlock.Text = "Loading ISOs " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " / " + NewLoadingWindow.LoadProgressBar.Maximum.ToString());

                            GamesList.Add(NewPS3Game);

                            // Add to the ListView
                            Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(NewPS3Game));
                        }
                        catch (UnauthorizedAccessException)
                        {
                            continue;
                        }
                        catch (IOException)
                        {
                            continue;
                        }
                        catch (Exception ex)
                        {
                            await Dispatcher.UIThread.Invoke(async () =>
                            {
                                var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                await box.ShowAsPopupAsync(this);
                            });
                            continue;
                        }
                    }
                }
                catch (Exception)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error accessing files. Please retry while running as Administrator/Root.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowAsPopupAsync(this);
                    });
                }
            }
        });

        NewLoadingWindow.Close();
    }

    #endregion

    #region Contextmenu General ISO Tools

    private async void ExtractISOMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS3Game SelectedGame = (PS3Game)GamesListBox.SelectedItem;
            if (File.Exists(SelectedGame.GameFilePath))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Info", "Please continue with PS3 ISO Tools and specify an output folder.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();

                var NewISOTools = new PS3ISOTools() { ShowActivated = true, ISOToExtract = SelectedGame.GameFilePath };
                NewISOTools.Show();
            }
        }
    }

    private async void CreateISOMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS3Game SelectedGame = (PS3Game)GamesListBox.SelectedItem;
            if (Directory.Exists(SelectedGame.GameFilePath))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Info", "Please continue with PS3 ISO Tools and specify an output folder.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();

                var NewISOTools = new PS3ISOTools() { ShowActivated = true, ISOToCreate = SelectedGame.GameFilePath };
                NewISOTools.Show();
            }
        }
    }

    private async void PatchISOMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS3Game SelectedGame = (PS3Game)GamesListBox.SelectedItem;
            if (File.Exists(SelectedGame.GameFilePath))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Info", "Please continue with PS3 ISO Tools.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();

                var NewISOTools = new PS3ISOTools() { ShowActivated = true, ISOToPatch = SelectedGame.GameFilePath };
                NewISOTools.Show();
            }
        }
    }

    private async void SplitISOMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS3Game SelectedGame = (PS3Game)GamesListBox.SelectedItem;
            if (File.Exists(SelectedGame.GameFilePath))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Info", "Please continue with PS3 ISO Tools.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();

                var NewISOTools = new PS3ISOTools() { ShowActivated = true, ISOToSplit = SelectedGame.GameFilePath };
                NewISOTools.Show();
            }
        }
    }

    private async void DecryptISOMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS3Game SelectedGame = (PS3Game)GamesListBox.SelectedItem;
            if (File.Exists(SelectedGame.GameFilePath))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Info", "Please continue with PS3 ISO Tools.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();

                var NewISOTools = new PS3ISOTools() { ShowActivated = true, ISOToDecrypt = SelectedGame.GameFilePath };
                NewISOTools.Show();
            }
        }
    }

    #endregion

    #region Contextmenu webMAN MOD ISO Tools

    private async void MountISOMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS3Game SelectedGame = (PS3Game)GamesListBox.SelectedItem;
            if (!string.IsNullOrEmpty(ConsoleIP))
            {
                if (!string.IsNullOrEmpty(SelectedGame.GameFilePath) && SelectedGame.GameRootLocation == PS3Game.GameLocation.WebMANMOD)
                {
                    NewPS3Menu.NavigateTowebMANMODUrl("http://" + ConsoleIP + "/mount.ps3" + SelectedGame.GameFilePath);
                }
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("No IP Address", "Please set your PS3 IP address in the Settings.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
    }

    private async void MountAndPlayISOMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS3Game SelectedGame = (PS3Game)GamesListBox.SelectedItem;
            if (!string.IsNullOrEmpty(ConsoleIP))
            {
                if (!string.IsNullOrEmpty(SelectedGame.GameFilePath) && SelectedGame.GameRootLocation == PS3Game.GameLocation.WebMANMOD)
                {
                    NewPS3Menu.NavigateTowebMANMODUrl("http://" + ConsoleIP + "/play.ps3" + SelectedGame.GameFilePath);
                }
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("No IP Address", "Please set your PS3 IP address in the Settings.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
    }

    #endregion

    #region Games Library Contextmenu Actions

    private void ExtractPKGMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS3Game SelectedPS3Game = (PS3Game)GamesListBox.SelectedItem;
            var NewPKGExtractor = new PS3PKGExtractor() { SelectedPKG = SelectedPS3Game.GameFilePath! };
            NewPKGExtractor.Show();
        }
    }

    private void PlayMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS3Game SelectedPS3Game = (PS3Game)GamesListBox.SelectedItem;

            if (SelectedPS3Game.GameBackgroundSoundFile is not null)
            {
                if (IsSoundPlaying)
                {
                    Utils.StopGameSound();
                    IsSoundPlaying = false;
                    PlayMenuItem.Header = "Play Soundtrack";
                    PlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) };
                }
                else
                {
                    Utils.PlayGameSoundFile(SelectedPS3Game.GameBackgroundSoundFile);
                    IsSoundPlaying = true;
                    PlayMenuItem.Header = "Stop Soundtrack";
                    PlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/stop.png"))) };
                }
            }
            else if (SelectedPS3Game.GameBackgroundSoundBytes is not null)
            {
                if (IsSoundPlaying)
                {
                    Utils.StopGameSoundBytes();
                    IsSoundPlaying = false;
                    PlayMenuItem.Header = "Play Soundtrack";
                    PlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) };
                }
                else
                {
                    Utils.StartAndStreamSoundBytes(SelectedPS3Game.GameBackgroundSoundBytes);
                    IsSoundPlaying = true;
                    PlayMenuItem.Header = "Stop Soundtrack";
                    PlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/stop.png"))) };
                }
            }
            else if (IsSoundPlaying)
            {
                Utils.StopGameSound();
                IsSoundPlaying = false;
                PlayMenuItem.Header = "Play Soundtrack";
                PlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) };
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Info", "No game soundtrack found.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                box.ShowWindowAsync();
            }
        }
    }

    private void PKGInfoMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS3Game SelectedPS3Game = (PS3Game)GamesListBox.SelectedItem;
            var NewPKGInfo = new PKGInfo() { SelectedPKG = SelectedPS3Game.GameFilePath!, Console = "PS3" };
            NewPKGInfo.Show();
        }
    }

    private async void CopyToMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS3Game SelectedPS3Game = (PS3Game)GamesListBox.SelectedItem;
            var FBD = new OpenFolderDialog() { Title = "Where do you want to copy the selected game ?" };
            var FBDResult = await FBD.ShowAsync(this);

            if (FBDResult != null)
            {
                var NewCopyWindow = new CopyWindow()
                {
                    ShowActivated = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    BackupDestinationPath = FBDResult + @"\",
                    Title = "Copying " + SelectedPS3Game.GameTitle + " to " + FBDResult + @"\" + Path.GetFileName(SelectedPS3Game.GameFilePath),
                    BackupPath = SelectedPS3Game.GameFileType == PS3Game.GameFileTypes.Backup ? SelectedPS3Game.GameFolderPath! : SelectedPS3Game.GameFilePath!
                };

                if (SelectedPS3Game.GameCoverSource is not null)
                {
                    NewCopyWindow.GameIcon = SelectedPS3Game.GameCoverSource;
                }

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
        if (File.Exists(Environment.CurrentDirectory + @"\Emulators\rpcs3\rpcs3.exe"))
        {
            // Check if PS3 firmware is installed
            if (!Directory.Exists(Environment.CurrentDirectory + @"\Emulators\rpcs3\dev_flash\sys\external") || !Directory.Exists(Environment.CurrentDirectory + @"\Emulators\rpcs3\dev_flash\sys\internal"))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Cannot launch game", "Playing games using rpcs3 requires the PS3 firmware to be installed first." + Environment.NewLine + "Do you want to install a firmware now using an PS3UPDAT.PUP file ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                var boxresult = await box.ShowWindowDialogAsync(this);
                if (boxresult == ButtonResult.Yes)
                {
                    var pupFileFilter = new FileDialogFilter
                    {
                        Name = "PUP File",
                        Extensions = ["PUP"]
                    };
                    var OFD = new OpenFileDialog() { Title = "Select the PS3UPDAT.PUP file to install.", Filters = { pupFileFilter }, AllowMultiple = false };
                    var OFDResult = await OFD.ShowAsync(this);
                    if (OFDResult != null && OFDResult.Length > 0)
                    {
                        // Set up rpcs3 to install the selected PS3 firmware
                        var EmulatorLauncherStartInfo = new ProcessStartInfo();
                        var EmulatorLauncher = new Process() { StartInfo = EmulatorLauncherStartInfo };
                        EmulatorLauncherStartInfo.FileName = Environment.CurrentDirectory + @"\Emulators\rpcs3\rpcs3.exe";
                        EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(Environment.CurrentDirectory + @"\Emulators\rpcs3\rpcs3.exe");
                        EmulatorLauncherStartInfo.Arguments = "--installfw \"" + OFDResult[0] + "\"";
                        EmulatorLauncher.Start();
                        EmulatorLauncher.WaitForExit();
                        EmulatorLauncher.Dispose();
                    }

                    else
                    {
                        var box2 = MessageBoxManager.GetMessageBoxStandard("Error", "No PS3UPDAT.PUP file specified.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box2.ShowWindowAsync();
                        return;
                    }
                }

                // Do not skip for PKGs, installation is possible without firmware
                else if (GamesListBox.SelectedItem is not null)
                {
                    PS3Game SelectedPS3Game = (PS3Game)GamesListBox.SelectedItem;
                    if (!(SelectedPS3Game.GameFileType == PS3Game.GameFileTypes.PKG))
                    {
                        var box3 = MessageBoxManager.GetMessageBoxStandard("Error", "Aborting game start.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box3.ShowWindowAsync();
                        return;
                    }
                }
                else
                {
                    var box4 = MessageBoxManager.GetMessageBoxStandard("Error", "Aborting game start.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box4.ShowWindowAsync();
                    return;
                }
            }

            if (GamesListBox.SelectedItem is not null)
            {
                PS3Game SelectedPS3Game = (PS3Game)GamesListBox.SelectedItem;

                var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Start " + SelectedPS3Game.GameTitle + " using RPCS3 ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                var boxresult = await box.ShowWindowDialogAsync(this);
                if (boxresult == ButtonResult.Yes)
                {

                    var EmulatorLauncherStartInfo = new ProcessStartInfo();
                    var EmulatorLauncher = new Process() { StartInfo = EmulatorLauncherStartInfo };
                    EmulatorLauncherStartInfo.FileName = Environment.CurrentDirectory + @"\Emulators\rpcs3\rpcs3.exe";
                    EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(Environment.CurrentDirectory + @"\Emulators\rpcs3\rpcs3.exe");

                    switch (SelectedPS3Game.GameFileType)
                    {
                        case PS3Game.GameFileTypes.Backup:
                            {

                                EmulatorLauncherStartInfo.FileName = Environment.CurrentDirectory + @"\Emulators\rpcs3\rpcs3.exe";
                                EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(Environment.CurrentDirectory + @"\Emulators\rpcs3\rpcs3.exe");
                                EmulatorLauncherStartInfo.Arguments = "\"" + SelectedPS3Game.GameFolderPath + "\" --no-gui";
                                EmulatorLauncher.Start();
                                break;
                            }

                        case PS3Game.GameFileTypes.PKG:
                            {

                                // Installation of game required
                                string RPCS3GameInstallationPath = Environment.CurrentDirectory + @"\Emulators\rpcs3\dev_hdd0\game\" + SelectedPS3Game.GameID;
                                if (!Directory.Exists(RPCS3GameInstallationPath))
                                {
                                    var box2 = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Playing games in PKG format will require you to install them first including their RAP file." + Environment.NewLine + Environment.NewLine +
                                        "Please close rpcs3 when the game PKG has been installed & after the RAP installation or PS Multi Tools will stop responding." + Environment.NewLine + Environment.NewLine +
                                        "The game will start automatically after the RAP file installation." + Environment.NewLine + Environment.NewLine +
                                        "Do you want to continue ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);

                                    var boxresult2 = await box2.ShowWindowDialogAsync(this);
                                    if (boxresult2 == ButtonResult.Yes)
                                    {
                                        var rapFileFilter = new FileDialogFilter
                                        {
                                            Name = "RAP File",
                                            Extensions = ["rap"]
                                        };
                                        var OFD = new OpenFileDialog() { Title = "Select the .rap file for the selected game.", Filters = { rapFileFilter } };

                                        var OFDResult = await OFD.ShowAsync(this);
                                        if (OFDResult != null && OFDResult.Length > 0)
                                        {
                                            string SelectedRAPFile = OFDResult[0];

                                            // Set up rpcs3 to install the .pkg file
                                            EmulatorLauncherStartInfo.Arguments = "--installpkg \"" + SelectedPS3Game.GameFilePath + "\"";
                                            EmulatorLauncher.Start();
                                            EmulatorLauncher.WaitForExit();
                                            EmulatorLauncher.Dispose();

                                            // Set up rpcs3 to install the .rap file
                                            EmulatorLauncher = new Process() { StartInfo = EmulatorLauncherStartInfo };
                                            EmulatorLauncherStartInfo.Arguments = "--installpkg \"" + SelectedRAPFile + "\"";
                                            EmulatorLauncher.Start();
                                            EmulatorLauncher.WaitForExit();
                                            EmulatorLauncher.Dispose();

                                            // Start the game after installation
                                            EmulatorLauncher = new Process() { StartInfo = EmulatorLauncherStartInfo };
                                            EmulatorLauncherStartInfo.Arguments = "--no-gui \"%RPCS3_GAMEID%:" + SelectedPS3Game.GameID + "\"";
                                            EmulatorLauncher.Start();
                                        }
                                        else
                                        {
                                            var box3 = MessageBoxManager.GetMessageBoxStandard("Please confirm", "No .rap file specified. The game will probably not run." + Environment.NewLine + "Do you want to continue ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                            var boxresult3 = await box3.ShowWindowDialogAsync(this);
                                            if (boxresult3 == ButtonResult.Yes)
                                            {
                                                // Set up rpcs3 to install the .pkg file
                                                EmulatorLauncherStartInfo.Arguments = "--installpkg \"" + SelectedPS3Game.GameFilePath + "\"";
                                                EmulatorLauncher.Start();
                                                EmulatorLauncher.WaitForExit();
                                                EmulatorLauncher.Dispose();

                                                // Try to start the game after installation
                                                EmulatorLauncher = new Process() { StartInfo = EmulatorLauncherStartInfo };
                                                EmulatorLauncherStartInfo.Arguments = "--no-gui \"%RPCS3_GAMEID%:" + SelectedPS3Game.GameID + "\"";
                                                EmulatorLauncher.Start();
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    // Game is already installed
                                    // Start the game
                                    EmulatorLauncher = new Process() { StartInfo = EmulatorLauncherStartInfo };
                                    EmulatorLauncherStartInfo.Arguments = "--no-gui \"%RPCS3_GAMEID%:" + SelectedPS3Game.GameID + "\"";
                                    EmulatorLauncher.Start();
                                }

                                break;
                            }

                        case PS3Game.GameFileTypes.PS3ISO:
                            {

                                // Save current list of drives
                                var CurrentDrives = new List<string>();
                                foreach (DriveInfo Drive in DriveInfo.GetDrives())
                                    CurrentDrives.Add(Drive.Name);

                                // Mount the ISO file using explorer & wait 3 sec.
                                Utils.OpenFolder(SelectedPS3Game.GameFilePath!);
                                Thread.Sleep(3000);

                                // Get new list of drives
                                var NewDrivesList = new List<string>();
                                foreach (DriveInfo Drive in DriveInfo.GetDrives())
                                    NewDrivesList.Add(Drive.Name);

                                // Get the new drive name
                                var NewDriveNames = NewDrivesList.Except(CurrentDrives);
                                if (NewDriveNames.Any())
                                {
                                    string NewDriveName = NewDriveNames.ElementAtOrDefault(0)!;

                                    // Set up rpcs3
                                    EmulatorLauncherStartInfo.Arguments = NewDriveName + " --no-gui";
                                    EmulatorLauncher.Start();
                                }

                                else
                                {
                                    var box2 = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find the mounted ISO.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                    await box2.ShowWindowAsync();
                                }

                                break;
                            }

                    }

                }
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "No game selected.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Cannot start rpcs3." + Environment.NewLine + "Emulator pack is not installed.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    #endregion

    #region Library Menu Actions

    private async void LoadFolderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select your PS3 backup folder" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {

            GamesListBox.Items.Clear();

            // Show the loading progress window
            NewLoadingWindow = new SyncWindow() { Title = "Loading PS3 files", ShowActivated = true };
            NewLoadingWindow.Show();

            var NewGameLoaderArgs = new GameLoaderArgs() { Type = LoadType.BackupFolder, FolderPath = FBDResult };
            ProcessBackups(NewGameLoaderArgs);
        }
    }

    private async void LoadRemoteFolderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.Items.Count > 0)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Clear previous games ?", "Games library already contains games." + Environment.NewLine + "Do you want to clear before proceeding ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box.ShowWindowDialogAsync(this);
            if (boxresult == ButtonResult.Yes)
            {
                GamesListBox.Items.Clear();
            }
            else if (boxresult == ButtonResult.No)
            {
                if (!string.IsNullOrEmpty(ConsoleIP))
                {
                    // Show the loading progress window
                    NewLoadingWindow = new SyncWindow() { Title = "Loading PS3 files", ShowActivated = true };
                    NewLoadingWindow.LoadProgressBar.IsIndeterminate = true;
                    NewLoadingWindow.LoadStatusTextBlock.Text = "Loading files, please wait ...";
                    NewLoadingWindow.Show();

                    // Load the files
                    var NewGameLoaderArgs = new GameLoaderArgs() { Type = LoadType.FTP, ConsoleIP = ConsoleIP, FolderPath = string.Empty };
                    ProcessBackups(NewGameLoaderArgs);
                }
                else
                {
                    var NewInputDialog = new InputDialog() { Title = "Enter PS3 IP Address" };
                    NewInputDialog.NewValueTextBox.Text = "0.0.0.0";
                    NewInputDialog.InputDialogTitleTextBlock.Text = "Please enter your PS3 IP address :";
                    NewInputDialog.ConfirmButton.Content = "Confirm";

                    string PS3IPAddress = await NewInputDialog.ShowDialog<string>(this);
                    if (PS3IPAddress != null)
                    {
                        // Show the loading progress window
                        NewLoadingWindow = new SyncWindow() { Title = "Loading PS3 files", ShowActivated = true };
                        NewLoadingWindow.LoadProgressBar.IsIndeterminate = true;
                        NewLoadingWindow.LoadStatusTextBlock.Text = "Loading files, please wait ...";
                        NewLoadingWindow.Show();

                        // Load the files
                        var NewGameLoaderArgs = new GameLoaderArgs() { Type = LoadType.FTP, ConsoleIP = PS3IPAddress, FolderPath = string.Empty };
                        ProcessBackups(NewGameLoaderArgs);
                    }
                }
            }
        }
        else if (!string.IsNullOrEmpty(ConsoleIP))
        {
            // Show the loading progress window
            NewLoadingWindow = new SyncWindow() { Title = "Loading PS3 files", ShowActivated = true };
            NewLoadingWindow.LoadProgressBar.IsIndeterminate = true;
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading files, please wait ...";
            NewLoadingWindow.Show();

            // Load the files
            var NewGameLoaderArgs = new GameLoaderArgs() { Type = LoadType.FTP, ConsoleIP = ConsoleIP, FolderPath = string.Empty };
            ProcessBackups(NewGameLoaderArgs);
        }
        else
        {
            var NewInputDialog = new InputDialog() { Title = "Enter PS3 IP Address" };
            NewInputDialog.NewValueTextBox.Text = "0.0.0.0";
            NewInputDialog.InputDialogTitleTextBlock.Text = "Please enter your PS3 IP address :";
            NewInputDialog.ConfirmButton.Content = "Confirm";

            string PS3IPAddress = await NewInputDialog.ShowDialog<string>(this);
            if (PS3IPAddress != null)
            {
                // Show the loading progress window
                NewLoadingWindow = new SyncWindow() { Title = "Loading PS3 files", ShowActivated = true };
                NewLoadingWindow.LoadProgressBar.IsIndeterminate = true;
                NewLoadingWindow.LoadStatusTextBlock.Text = "Loading files, please wait ...";
                NewLoadingWindow.Show();

                // Load the files
                var NewGameLoaderArgs = new GameLoaderArgs() { Type = LoadType.FTP, ConsoleIP = PS3IPAddress, FolderPath = string.Empty };
                ProcessBackups(NewGameLoaderArgs);
            }
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
            PS3Game SelectedPS3Game = (PS3Game)GamesListBox.SelectedItem;

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
                GameBackupTypeTextBlock.IsVisible = true;
                GameBackupFolderNameTextBlock.IsVisible = true;
                ISOEncryptionStatusTextBlock.IsVisible = true;
                SupportedResolutionsTextBlock.IsVisible = true;
                SupportedSoundFormatsTextBlock.IsVisible = true;
                ResolutionsImage.IsVisible = true;
                SoundFormatsImage.IsVisible = true;
            }

            GameTitleTextBlock.Text = SelectedPS3Game.GameTitle;
            GameIDTextBlock.Text = "Title ID: " + SelectedPS3Game.GameID;
            GameContentIDTextBlock.Text = "Content ID: " + SelectedPS3Game.ContentID;
            GameRegionTextBlock.Text = "Region: " + SelectedPS3Game.GameRegion;
            GameVersionTextBlock.Text = "Game Version: " + SelectedPS3Game.GameVer;
            GameAppVersionTextBlock.Text = "Application Version: " + SelectedPS3Game.GameAppVer;
            GameCategoryTextBlock.Text = "Category: " + SelectedPS3Game.GameCategory;
            GameSizeTextBlock.Text = "Size: " + SelectedPS3Game.GameSize;
            GameRequiredFirmwareTextBlock.Text = "Required Firmware: " + SelectedPS3Game.GameRequiredFW;
            GameBackupTypeTextBlock.Text = "Backup Type: " + SelectedPS3Game.GameFileType.ToString();

            ToolTip.SetTip(ResolutionsImage, SelectedPS3Game.GameResolution);
            ToolTip.SetTip(SoundFormatsImage, SelectedPS3Game.GameSoundFormat);

            if (!string.IsNullOrEmpty(SelectedPS3Game.GameFilePath))
            {
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " + new DirectoryInfo(Path.GetDirectoryName(SelectedPS3Game.GameFilePath)!).Name;
            }
            else
            {
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " + new DirectoryInfo(SelectedPS3Game.GameFolderPath!).Name;
            }

            if (!string.IsNullOrEmpty(SelectedPS3Game.GameBackgroundPath))
            {
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        if (BlurringShape.Fill is ImageBrush RectangleImageBrush)
                        {
                            using MemoryStream memory = new();
                            AnyBitmap SelectedPS3GameBG = new(SelectedPS3Game.GameBackgroundPath);
                            SelectedPS3GameBG.ExportStream(memory);
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
                        AnyBitmap SelectedPS3GameBG = new(SelectedPS3Game.GameBackgroundPath);
                        SelectedPS3GameBG.ExportStream(memory);
                        memory.Position = 0;
                        Bitmap avaloniaBitmap = new(memory);
                        RectangleImageBrush.Source = avaloniaBitmap;
                    }
                    BlurringShape.Opacity = 0;
                    BlurringShape.Opacity = 1;
                }
            }
            else if (SelectedPS3Game.GameBackgroundSource is not null)
            {
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        if (BlurringShape.Fill is ImageBrush RectangleImageBrush)
                        {
                            using MemoryStream memory = new();
                            SelectedPS3Game.GameBackgroundSource.ExportStream(memory);
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
                        SelectedPS3Game.GameBackgroundSource.ExportStream(memory);
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
                Utils.StopGameSoundBytes();
                IsSoundPlaying = false;
                PlayMenuItem.Header = "Play Soundtrack";
                PlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) };
            }
            if (SelectedPS3Game.GameBackgroundSoundFile is not null)
            {
                if (AutoPlay)
                {
                    Utils.PlayGameSoundFile(SelectedPS3Game.GameBackgroundSoundFile);
                    IsSoundPlaying = true;
                    PlayMenuItem.Header = "Stop Soundtrack";
                    PlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/stop.png"))) };
                }
            }
            else if (SelectedPS3Game.GameBackgroundSoundBytes is not null)
            {
                if (AutoPlay)
                {
                    Utils.StartAndStreamSoundBytes(SelectedPS3Game.GameBackgroundSoundBytes);
                    IsSoundPlaying = true;
                    PlayMenuItem.Header = "Stop Soundtrack";
                    PlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/stop.png"))) };
                }
            }

            if (!string.IsNullOrEmpty(SelectedPS3Game.ISOEncryption))
            {
                ISOEncryptionStatusTextBlock.IsVisible = true;
                ISOEncryptionStatusTextBlock.Text = "ISO Status: " + SelectedPS3Game.ISOEncryption;

                if (SelectedPS3Game.ISOEncryption == "Encrypted")
                {
                    ISOEncryptionStatusTextBlock.Foreground = Brushes.Red;
                }
                else
                {
                    ISOEncryptionStatusTextBlock.Foreground = Brushes.Green;
                }
            }
            else
            {
                ISOEncryptionStatusTextBlock.IsVisible = false;
                ISOEncryptionStatusTextBlock.Text = "";
            }
        }
    }

    private void NewContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        NewContextMenu.Items.Clear();
        ISOToolsMenuItem.Items.Clear();

        if (GamesListBox.SelectedItem is not null)
        {
            PS3Game SelectedPS3Game = (PS3Game)GamesListBox.SelectedItem;

            NewContextMenu.Items.Add(CopyToMenuItem);

            switch (SelectedPS3Game.GameFileType)
            {
                case PS3Game.GameFileTypes.Backup:
                    {
                        NewContextMenu.Items.Add(PlayMenuItem);
                        NewContextMenu.Items.Add(ISOToolsMenuItem);
                        NewContextMenu.Items.Add(PlayGameMenuItem);

                        ISOToolsMenuItem.Items.Add(CreateISOMenuItem);
                        break;
                    }
                case PS3Game.GameFileTypes.PKG:
                    {
                        NewContextMenu.Items.Add(PKGInfoMenuItem);
                        NewContextMenu.Items.Add(ExtractPKGMenuItem);
                        NewContextMenu.Items.Add(PlayGameMenuItem);
                        break;
                    }
                case PS3Game.GameFileTypes.PS3ISO:
                    {
                        if (SelectedPS3Game.GameRootLocation == PS3Game.GameLocation.WebMANMOD)
                        {
                            NewContextMenu.Items.Add(ISOToolsMenuItem);
                            ISOToolsMenuItem.Items.Add(MountAndPlayISOMenuItem);
                            ISOToolsMenuItem.Items.Add(MountISOMenuItem);
                        }
                        else
                        {
                            NewContextMenu.Items.Add(PlayMenuItem);
                            NewContextMenu.Items.Add(ISOToolsMenuItem);
                            NewContextMenu.Items.Add(PlayGameMenuItem);
                            ISOToolsMenuItem.Items.Add(ExtractISOMenuItem);
                            ISOToolsMenuItem.Items.Add(PatchISOMenuItem);
                            ISOToolsMenuItem.Items.Add(SplitISOMenuItem);

                            if (SelectedPS3Game.ISOEncryption == "Encrypted")
                            {
                                ISOToolsMenuItem.Items.Add(DecryptISOMenuItem);
                            }
                        }

                        break;
                    }
                case PS3Game.GameFileTypes.PS2ISO:
                case PS3Game.GameFileTypes.PSXISO:
                case PS3Game.GameFileTypes.PSPISO:
                    {
                        NewContextMenu.Items.Add(PlayMenuItem);
                        NewContextMenu.Items.Add(ISOToolsMenuItem);
                        ISOToolsMenuItem.Items.Add(MountAndPlayISOMenuItem);
                        ISOToolsMenuItem.Items.Add(MountISOMenuItem);
                        break;
                    }
            }

        }
    }

    private void NewContextMenu_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        NewContextMenu.Items.Clear();
    }

    private void GamesListBox_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        var GamesListBoxScrollViewer = Utils.FindScrollViewer(GamesListBox)!;
        var delta = e.Delta.Y * 20;
        var newX = Math.Clamp(GamesListBoxScrollViewer.Offset.X + delta, 0, Math.Max(0, GamesListBoxScrollViewer.Extent.Width - GamesListBoxScrollViewer.Viewport.Width));
        GamesListBoxScrollViewer.Offset = new Avalonia.Vector(newX, GamesListBoxScrollViewer.Offset.Y);
        e.Handled = true;
    }

    #region Filtering

    private void FilterByBackupFoldersButton_Click(object? sender, RoutedEventArgs e)
    {
        GamesListBox.Items.Clear();

        foreach (PS3Game PS3GameInList in GamesList.Where(lvi => lvi.GameFileType.Equals(PS3Game.GameFileTypes.Backup)))
            GamesListBox.Items.Add(PS3GameInList);
    }

    private void FilterByPS3ISOButton_Click(object? sender, RoutedEventArgs e)
    {
        GamesListBox.Items.Clear();

        foreach (PS3Game PS3GameInList in GamesList.Where(lvi => lvi.GameFileType.Equals(PS3Game.GameFileTypes.PS3ISO)))
            GamesListBox.Items.Add(PS3GameInList);
    }

    private void FilterByPS2ISOButton_Click(object? sender, RoutedEventArgs e)
    {
        GamesListBox.Items.Clear();

        foreach (PS3Game PS3GameInList in GamesList.Where(lvi => lvi.GameFileType.Equals(PS3Game.GameFileTypes.PS2ISO)))
            GamesListBox.Items.Add(PS3GameInList);
    }

    private void FilterByPSXISOButton_Click(object? sender, RoutedEventArgs e)
    {
        GamesListBox.Items.Clear();

        foreach (PS3Game PS3GameInList in GamesList.Where(lvi => lvi.GameFileType.Equals(PS3Game.GameFileTypes.PSXISO)))
            GamesListBox.Items.Add(PS3GameInList);
    }

    private void FilterByPSPISOButton_Click(object? sender, RoutedEventArgs e)
    {
        GamesListBox.Items.Clear();

        foreach (PS3Game PS3GameInList in GamesList.Where(lvi => lvi.GameFileType.Equals(PS3Game.GameFileTypes.PSPISO)))
            GamesListBox.Items.Add(PS3GameInList);
    }

    private void FilterByPKGButton_Click(object? sender, RoutedEventArgs e)
    {
        GamesListBox.Items.Clear();

        foreach (PS3Game PS3GameInList in GamesList.Where(lvi => lvi.GameFileType.Equals(PS3Game.GameFileTypes.PKG)))
            GamesListBox.Items.Add(PS3GameInList);
    }

    private void FilterByLocalGamesButton_Click(object? sender, RoutedEventArgs e)
    {
        GamesListBox.Items.Clear();

        foreach (PS3Game PS3GameInList in GamesList.Where(lvi => lvi.GameRootLocation.Equals(PS3Game.GameLocation.Local)))
            GamesListBox.Items.Add(PS3GameInList);
    }

    private void FilterByRemoteGamesButton_Click(object? sender, RoutedEventArgs e)
    {
        GamesListBox.Items.Clear();

        foreach (PS3Game PS3GameInList in GamesList.Where(lvi => lvi.GameRootLocation.Equals(PS3Game.GameLocation.WebMANMOD)))
            GamesListBox.Items.Add(PS3GameInList);
    }

    private void ShowAllButton_Click(object? sender, RoutedEventArgs e)
    {
        GamesListBox.Items.Clear();

        foreach (PS3Game PS3GameInList in GamesList)
            GamesListBox.Items.Add(PS3GameInList);
    }

    #endregion

    private void OpenPKGBrowser(object? sender, RoutedEventArgs e)
    {
        var NewPKGBrowser = new PKGBrowser() { Console = "PS3", ShowActivated = true };
        NewPKGBrowser.Show();
    }

    private void EMU_Settings_Click(object? sender, RoutedEventArgs e)
    {
        //var NewPS3EmulatorSettingsWindow = new PS3EmulatorSettings() { ShowActivated = true };
        //NewPS3EmulatorSettingsWindow.Show();
    }

}