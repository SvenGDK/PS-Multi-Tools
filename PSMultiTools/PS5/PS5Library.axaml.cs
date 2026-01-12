using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using FluentFTP;
using IniParser;
using IniParser.Model;
using IronSoftware.Drawing;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using PSMultiTools.Classes;
using PSMultiTools.Dialogs;
using PSMultiTools.PS5.Tools;
using PSMultiTools.PS5.Tools.GamePatches;
using PSMultiTools.PS5.Tools.PKGBuilder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xilium.CefGlue.Avalonia;
using static PSMultiTools.Classes.PS5ParamClass;

namespace PSMultiTools.PS5;

public partial class PS5Library : Window
{

    private readonly AvaloniaCefBrowser ContentWebView = new() { Address = "about:blank" };
    private SyncWindow NewLoadingWindow = new() { Title = "Loading PS5 files", ShowActivated = true };
    public string SelectedPath = "";
    public string CurrentPath = "";

    public string ConsoleIP = "";
    public string ConsoleFTPPort = "";
    public string PayloadPort = "";

    public int PKGCount = 0;
    public List<string> URLs = [];
    public int CurrentURL = 0;
    public long TotalSize = 0;
    private int? ScanThreads;

    public bool IsSoundPlaying = false;
    public bool AutoPlay = true;

    #region Supplemental Library Menu Items
    private readonly MenuItem OpenLocalBackupFolderMenuItem = new() { Header = "Open a folder with backups" };
    private readonly MenuItem LoadPatchPKGFolderMenuItem = new() { Header = "Open a folder with source PKG files" };
    private readonly MenuItem LoadFTPFolderMenuItem = new() { Header = "Load installed games & apps from PS5 over FTP" };
    private readonly MenuItem OpenDownloadsFolderMenuItem = new() { Header = "Open the Downloads folder" };
    private readonly MenuItem ExitMenuItem = new() { Header = "Exit" };
    #endregion

    #region Game Context Menu Items
    // Local context menu options
    private ContextMenu GamesContextMenu = new();
    private readonly MenuItem GameCopyToMenuItem = new() { Header = "Copy game to", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/copy-icon.png"))) } };
    // private readonly MenuItem UploadToPS5MenuItem = new() { Header = "Upload to PS5", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/upload.png"))) } };
    private readonly MenuItem GameOpenLocationMenuItem = new() { Header = "Open game folder", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/open-folder.png"))) } };
    private readonly MenuItem GamePlayMenuItem = new() { Header = "Play Soundtrack", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) } };
    private readonly MenuItem GameCheckForUpdatesMenuItem = new() { Header = "Check for updates", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/refresh.png"))) } };
    private readonly MenuItem GameBrowseAssetsMenuItem = new() { Header = "Browse assets", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/open-folder.png"))) } };
    private readonly MenuItem GamePackAsPKG = new() { Header = "Pack as PS5 PKG", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/PKG.png"))) } };

    // Remote context menu options
    private readonly MenuItem GameLaunchMenuItem = new() { Header = "Launch on PS5", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) } };
    private readonly MenuItem GameChangeTypeMenuItem = new() { Header = "Change game type", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/rename.png"))) } };
    private readonly MenuItem GameChangeToGameMenuItem = new() { Header = "To Game App" };
    private readonly MenuItem GameChangeToNativeMediaMenuItem = new() { Header = "To Native Media App" };
    private readonly MenuItem GameChangeToRNPSMediaMenuItem = new() { Header = "To RNPS Media App" };
    private readonly MenuItem GameChangeToBuiltInMenuItem = new() { Header = "To System Built-in" };
    private readonly MenuItem GameChangeToBigDaemonMenuItem = new() { Header = "To Big Daemon" };
    private readonly MenuItem GameChangeToShellUIMenuItem = new() { Header = "To ShellUI" };
    private readonly MenuItem GameChangeToDaemonMenuItem = new() { Header = "To Daemon" };
    private readonly MenuItem GameChangeToShellAppMenuItem = new() { Header = "To ShellApp" };

    private readonly MenuItem GameRenameMenuItem = new() { Header = "Rename game", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/rename.png"))) } };
    private readonly MenuItem GameChangeIconMenuItem = new() { Header = "Change game icon", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/change.png"))) } };
    private readonly MenuItem GameChangeBackgroundMenuItem = new() { Header = "Change game background", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/change.png"))) } };
    private readonly MenuItem GameChangeSoundtrackMenuItem = new() { Header = "Change game soundtrack", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/change.png"))) } };
    #endregion

    #region App Context Menu Items
    private ContextMenu AppsContextMenu = new();
    private readonly MenuItem AppCopyToMenuItem = new() { Header = "Copy app to", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/copy-icon.png"))) } };
    private readonly MenuItem AppOpenLocationMenuItem = new() { Header = "Open app folder", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/open-folder.png"))) } };
    private readonly MenuItem AppPlayMenuItem = new() { Header = "Play Soundtrack", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) } };
    private readonly MenuItem AppCheckForUpdatesMenuItem = new() { Header = "Check for updates", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/refresh.png"))) } };
    private readonly MenuItem AppPackAsPKG = new() { Header = "Pack as PS5 PKG", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/PKG.png"))) } };

    private readonly MenuItem AppChangeTypeMenuItem = new() { Header = "Change app type", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/rename.png"))) } };
    private readonly MenuItem AppChangeToGameMenuItem = new() { Header = "To Game App" };
    private readonly MenuItem AppChangeToNativeMediaMenuItem = new() { Header = "To Native Media App" };
    private readonly MenuItem AppChangeToRNPSMediaMenuItem = new() { Header = "To RNPS Media App" };
    private readonly MenuItem AppChangeToBuiltInMenuItem = new() { Header = "To System Built-in" };
    private readonly MenuItem AppChangeToBigDaemonMenuItem = new() { Header = "To Big Daemon" };
    private readonly MenuItem AppChangeToShellUIMenuItem = new() { Header = "To ShellUI" };
    private readonly MenuItem AppChangeToDaemonMenuItem = new() { Header = "To Daemon" };
    private readonly MenuItem AppChangeToShellAppMenuItem = new() { Header = "To ShellApp" };

    private readonly MenuItem AppRenameMenuItem = new() { Header = "Rename application", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/rename.png"))) } };
    private readonly MenuItem AppChangeIconMenuItem = new() { Header = "Change app icon", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/change.png"))) } };
    private readonly MenuItem AppChangeBackgroundMenuItem = new() { Header = "Change app background", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/change.png"))) } };
    private readonly MenuItem AppChangeSoundtrackMenuItem = new() { Header = "Change app soundtrack", Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/change.png"))) } };
    #endregion

    public PS5Library()
    {
        InitializeComponent();

        Loaded += PS5Library_Loaded;

        NewPS5Menu.IPChanged += NewPS5Menu_IPChanged;
        NewPS5Menu.FTPPortChanged += NewPS5Menu_FTPPortChanged;
        NewPS5Menu.PayloadPortChanged += NewPS5Menu_PayloadPortChanged;

        GamesListBox.PointerWheelChanged += GamesListBox_PointerWheelChanged;
        GamesListBox.SelectionChanged += GamesListBox_SelectionChanged;

        AppsListBox.SelectionChanged += AppsListBox_SelectionChanged;

        GamesContextMenu.Opening += GamesContextMenu_Opening;
        GamesContextMenu.Closing += GamesContextMenu_Closing;

        GameVersionFileURITextBlock.PointerPressed += GameVersionFileURITextBlock_PointerPressed;

        // ContentWebView setup
        ContentWebView.LoadEnd += ContentWebView_LoadEnd;
        var browserWrapper = this.FindControl<Decorator>("ContentWebViewWrapper");
        browserWrapper!.Child = ContentWebView;

        OpenLocalBackupFolderMenuItem.Click += OpenLocalBackupFolderMenuItem_Click;
        LoadFTPFolderMenuItem.Click += LoadFTPFolderMenuItem_Click;
        LoadPatchPKGFolderMenuItem.Click += LoadPatchPKGFolderMenuItem_Click;
        OpenDownloadsFolderMenuItem.Click += OpenDownloadsFolderMenuItem_Click;
        ExitMenuItem.Click += ExitMenuItem_Click;

        GameCopyToMenuItem.Click += GameCopyToMenuItem_Click;
        GamePlayMenuItem.Click += GamePlayMenuItem_Click;
        GameCheckForUpdatesMenuItem.Click += GameCheckForUpdatesMenuItem_Click;
        GameChangeToGameMenuItem.Click += GameChangeToGameMenuItem_Click;
        GameChangeToNativeMediaMenuItem.Click += GameChangeToNativeMediaMenuItem_Click;
        GameChangeToRNPSMediaMenuItem.Click += GameChangeToRNPSMediaMenuItem_Click;
        GameChangeToShellAppMenuItem.Click += GameChangeToShellAppMenuItem_Click;
        GameChangeToShellUIMenuItem.Click += GameChangeToShellUIMenuItem_Click;
        GameChangeToBigDaemonMenuItem.Click += GameChangeToBigDaemonMenuItem_Click;
        GameChangeToBuiltInMenuItem.Click += GameChangeToBuiltInMenuItem_Click;
        GameChangeToDaemonMenuItem.Click += GameChangeToDaemonMenuItem_Click;
        GameRenameMenuItem.Click += GameRenameMenuItem_Click;
        GameChangeIconMenuItem.Click += GameChangeIconMenuItem_Click;
        GameChangeBackgroundMenuItem.Click += GameChangeBackgroundMenuItem_Click;
        GameChangeSoundtrackMenuItem.Click += GameChangeSoundtrackMenuItem_Click;
        GameOpenLocationMenuItem.Click += GameOpenLocationMenuItem_Click;
        GameBrowseAssetsMenuItem.Click += GameBrowseAssetsMenuItem_Click;
        GamePackAsPKG.Click += GamePackAsPKG_Click;
        GameLaunchMenuItem.Click += GameLaunchMenuItem_Click;

        AppCopyToMenuItem.Click += AppCopyToMenuItem_Click;
        AppPlayMenuItem.Click += AppPlayMenuItem_Click;
        AppCheckForUpdatesMenuItem.Click += AppCheckForUpdatesMenuItem_Click;
        AppChangeToGameMenuItem.Click += AppChangeToGameMenuItem_Click;
        AppChangeToNativeMediaMenuItem.Click += AppChangeToNativeMediaMenuItem_Click;
        AppChangeToRNPSMediaMenuItem.Click += AppChangeToRNPSMediaMenuItem_Click;
        AppChangeToBigDaemonMenuItem.Click += AppChangeToBigDaemonMenuItem_Click;
        AppChangeToBuiltInMenuItem.Click += AppChangeToBuiltInMenuItem_Click;
        AppChangeToDaemonMenuItem.Click += AppChangeToDaemonMenuItem_Click;
        AppChangeToShellAppMenuItem.Click += AppChangeToShellAppMenuItem_Click;
        AppChangeToShellUIMenuItem.Click += AppChangeToShellUIMenuItem_Click;
        AppRenameMenuItem.Click += AppRenameMenuItem_Click;
        AppChangeIconMenuItem.Click += AppChangeIconMenuItem_Click;
        AppChangeBackgroundMenuItem.Click += AppChangeBackgroundMenuItem_Click;
        AppChangeSoundtrackMenuItem.Click += AppChangeSoundtrackMenuItem_Click;
        AppOpenLocationMenuItem.Click += AppOpenLocationMenuItem_Click;
        AppPackAsPKG.Click += AppPackAsPKG_Click;
    }

    private void PS5Library_Loaded(object? sender, RoutedEventArgs e)
    {
        // Add some tooltips
        ToolTip.SetTip(OpenLocalBackupFolderMenuItem, "This option allows you to select & load any folder on your PC that contains all your backups. Do not select the root (main) folder of a drive or it will fail.");
        ToolTip.SetTip(LoadPatchPKGFolderMenuItem, "This option allows you to select & load any folder on your PC that contains '_sc.pkg' named PKG files.");
        ToolTip.SetTip(LoadFTPFolderMenuItem, "This option will load all your installed game & applications from your PS5 when an IP address and FTP port has been set in the 'Settings'.");

        // Add supplemental menu items that will be handled in PS Multi Tools
        MenuItem LibraryMenuItem = (MenuItem)NewPS5Menu.MainMenu.Items[0]!;
        LibraryMenuItem.Items.Add(OpenLocalBackupFolderMenuItem);
        LibraryMenuItem.Items.Add(LoadPatchPKGFolderMenuItem);
        LibraryMenuItem.Items.Add(new Separator());
        LibraryMenuItem.Items.Add(LoadFTPFolderMenuItem);
        LibraryMenuItem.Items.Add(new Separator());
        LibraryMenuItem.Items.Add(ExitMenuItem);

        GameChangeTypeMenuItem.Items.Add(GameChangeToGameMenuItem);
        GameChangeTypeMenuItem.Items.Add(GameChangeToNativeMediaMenuItem);
        GameChangeTypeMenuItem.Items.Add(GameChangeToRNPSMediaMenuItem);
        GameChangeTypeMenuItem.Items.Add(GameChangeToBuiltInMenuItem);
        GameChangeTypeMenuItem.Items.Add(GameChangeToBigDaemonMenuItem);
        GameChangeTypeMenuItem.Items.Add(GameChangeToShellUIMenuItem);
        GameChangeTypeMenuItem.Items.Add(GameChangeToDaemonMenuItem);
        GameChangeTypeMenuItem.Items.Add(GameChangeToShellAppMenuItem);

        // Add context menu for apps
        AppsContextMenu.Items.Add(AppOpenLocationMenuItem);
        AppsContextMenu.Items.Add(AppCopyToMenuItem);
        AppsContextMenu.Items.Add(AppPlayMenuItem);
        AppsContextMenu.Items.Add(AppCheckForUpdatesMenuItem);
        AppsContextMenu.Items.Add(AppPackAsPKG);
        AppsContextMenu.Items.Add(new Separator());

        // Add sub menu for AppChangeType
        AppChangeTypeMenuItem.Items.Add(AppChangeToGameMenuItem);
        AppChangeTypeMenuItem.Items.Add(AppChangeToNativeMediaMenuItem);
        AppChangeTypeMenuItem.Items.Add(AppChangeToRNPSMediaMenuItem);
        AppChangeTypeMenuItem.Items.Add(AppChangeToBuiltInMenuItem);
        AppChangeTypeMenuItem.Items.Add(AppChangeToBigDaemonMenuItem);
        AppChangeTypeMenuItem.Items.Add(AppChangeToShellUIMenuItem);
        AppChangeTypeMenuItem.Items.Add(AppChangeToDaemonMenuItem);
        AppChangeTypeMenuItem.Items.Add(AppChangeToShellAppMenuItem);

        AppsContextMenu.Items.Add(AppChangeTypeMenuItem);
        AppsContextMenu.Items.Add(AppRenameMenuItem);
        AppsContextMenu.Items.Add(AppChangeIconMenuItem);
        AppsContextMenu.Items.Add(AppChangeBackgroundMenuItem);
        AppsContextMenu.Items.Add(AppChangeSoundtrackMenuItem);

        // Set context menu
        GamesListBox.ContextMenu = GamesContextMenu;
        AppsListBox.ContextMenu = AppsContextMenu;

        // Set background fading animation
        BlurringShape.Transitions = [new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(500), Easing = new CubicEaseOut() }];

        // Load config if exists
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini")))
        {
            try
            {
                var PSMTConfigParser = new FileIniDataParser();
                IniData PSMTConfigData = PSMTConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini"));

                ConsoleIP = PSMTConfigData["PS5 Tools"]["IP"];
                ConsoleFTPPort = PSMTConfigData["PS5 Tools"]["FTPPort"];
                PayloadPort = PSMTConfigData["PS5 Tools"]["PayloadPort"];

                if (!string.IsNullOrEmpty(PSMTConfigData["PS5 Library"]["ScanThreads"]))
                {
                    ScanThreads = Convert.ToInt32(PSMTConfigData["PS5 Library"]["ScanThreads"]);
                }
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

    #region Menu TextBox Changed Events

    private void NewPS5Menu_IPChanged(object? sender, RoutedEventArgs e)
    {
        ConsoleIP = NewPS5Menu.SharedIPAddress;

        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini")))
        {
            try
            {
                var PSMTConfigParser = new FileIniDataParser();
                IniData PSMTConfigData = PSMTConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini"));

                PSMTConfigData["PS5 Tools"]["IP"] = ConsoleIP;

                PSMTConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini"), PSMTConfigData);
            }
            catch (Exception)
            {
            }
        }
    }

    private void NewPS5Menu_FTPPortChanged(object? sender, RoutedEventArgs e)
    {
        ConsoleFTPPort = NewPS5Menu.SharedFTPPort;

        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini")))
        {
            try
            {
                var PSMTConfigParser = new FileIniDataParser();
                IniData PSMTConfigData = PSMTConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini"));

                PSMTConfigData["PS5 Tools"]["FTPPort"] = ConsoleFTPPort;

                PSMTConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini"), PSMTConfigData);
            }
            catch (Exception)
            {
            }
        }
    }

    private void NewPS5Menu_PayloadPortChanged(object? sender, RoutedEventArgs e)
    {
        PayloadPort = NewPS5Menu.SharedPayloadPort;

        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini")))
        {
            try
            {
                var PSMTConfigParser = new FileIniDataParser();
                IniData PSMTConfigData = PSMTConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini"));

                PSMTConfigData["PS5 Tools"]["PayloadPort"] = PayloadPort;

                PSMTConfigParser.WriteFile(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini"), PSMTConfigData);
            }
            catch (Exception)
            {
            }
        }
    }

    #endregion

    #region Library Menu Actions

    private async void OpenLocalBackupFolderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.Items.Count > 0)
        {
            var NewDialog = new CustomDialog();
            NewDialog.ButtonsGrid.IsVisible = true;
            NewDialog.Title = "PS5 Library";
            NewDialog.ButtonsTitleTextBlock.Text = "A folder is already loaded. Please select an action :";

            CustomDialog.CustomDialogResult DiagResult = await NewDialog.ShowDialog<CustomDialog.CustomDialogResult>(this);
            switch (DiagResult)
            {
                case CustomDialog.CustomDialogResult.LoadNew:
                    {
                        GamesListBox.Items.Clear();
                        ShowBackupFolderBrowser();
                        break;
                    }
                case CustomDialog.CustomDialogResult.Append:
                    {
                        ShowBackupFolderBrowser();
                        break;
                    }
                case CustomDialog.CustomDialogResult.Cancel:
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Aborted", "Aborted", ButtonEnum.Ok);
                        await box.ShowWindowAsync();
                        break;
                    }
            }
        }
        else
        {
            ShowBackupFolderBrowser();
        }
    }

    private async void ShowBackupFolderBrowser()
    {
        var FBD = new OpenFolderDialog() { Title = "Select your PS5 backups folder" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedPath = FBDResult;
            Trace.WriteLine(FBDResult);

            bool LoadIcons = true;
            bool LoadBackgrounds = true;
            bool SkipFileChecks = false;

            // Show loading window
            NewLoadingWindow = new SyncWindow() { Title = "PS5 Library Loader", ShowActivated = true };
            NewLoadingWindow.Show(this);

            var NewGameLoaderArgs = new Structures.PS5GameLoaderArgs() { FolderPath = FBDResult, LoadIcons = LoadIcons, LoadBackgrounds = LoadBackgrounds, SkipFileChecks = SkipFileChecks };
            ProcessBackups(NewGameLoaderArgs);
        }
    }

    private async void ProcessBackups(Structures.PS5GameLoaderArgs WorkerArgs)
    {
        await Task.Run(async () =>
        {
            // Search for files
            try
            {

                Dispatcher.UIThread.Invoke(() =>
                {
                    NewLoadingWindow.LoadStatusTextBlock.Text = "Counting backups, please wait";
                });

                var NewCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                List<string> ValidBackups = await FindAllFilesAsync(WorkerArgs.FolderPath, CTS: NewCancellationTokenSource.Token, ParallelTasks: ScanThreads ?? 8);

                Dispatcher.UIThread.Invoke(() =>
                {
                    NewLoadingWindow.LoadProgressBar.Maximum = ValidBackups.Count;
                    NewLoadingWindow.LoadStatusTextBlock.Text = "Loading backup 1 of " + ValidBackups.Count.ToString();
                });

                foreach (string ValidBackup in ValidBackups)
                {
                    TotalSize = 0;

                    var NewPS5Game = new PS5Game() { GameBackupType = "Folder", GameLocation = PS5Game.Location.Local };
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(File.ReadAllText(ValidBackup));
                    var ParamFileInfo = new FileInfo(ValidBackup);

                    if (ParamData is not null)
                    {

                        string MainGamePath = Directory.GetParent(ParamFileInfo.FullName)!.Parent!.FullName;
                        string SCESYSFolder = Path.GetDirectoryName(ParamFileInfo.FullName)!;

                        if (ParamData.TitleId is not null)
                        {
                            NewPS5Game.GameID = "Title ID: " + ParamData.TitleId;
                            NewPS5Game.GameRegion = "Region: " + PS5Game.GetGameRegion(ParamData.TitleId);
                        }

                        bool NonEUUSTitle = false;
                        if (ParamData.LocalizedParameters!.EnUS is not null)
                        {
                            NewPS5Game.GameTitle = ParamData.LocalizedParameters.EnUS.TitleName;
                        }
                        else
                        {
                            NonEUUSTitle = true;
                        }
                        if (ParamData.LocalizedParameters.DeDE is not null)
                        {
                            NewPS5Game.DEGameTitle = ParamData.LocalizedParameters.DeDE.TitleName;
                        }
                        if (ParamData.LocalizedParameters.FrFR is not null)
                        {
                            NewPS5Game.FRGameTitle = ParamData.LocalizedParameters.FrFR.TitleName;
                        }
                        if (ParamData.LocalizedParameters.ItIT is not null)
                        {
                            NewPS5Game.ITGameTitle = ParamData.LocalizedParameters.ItIT.TitleName;
                        }
                        if (ParamData.LocalizedParameters.EsES is not null)
                        {
                            NewPS5Game.ESGameTitle = ParamData.LocalizedParameters.EsES.TitleName;
                        }
                        if (ParamData.LocalizedParameters.JaJP is not null)
                        {
                            NewPS5Game.JPGameTitle = ParamData.LocalizedParameters.JaJP.TitleName;
                        }
                        if (NonEUUSTitle)
                        {
                            NewPS5Game.GameTitle = GetAlternativeGameTitle(ParamData);
                        }

                        if (ParamData.ContentId is not null)
                        {
                            NewPS5Game.GameContentID = "Content ID: " + ParamData.ContentId;
                        }

                        if (ParamData.ApplicationCategoryType == 0)
                        {
                            NewPS5Game.GameCategory = "Type: Game";
                        }
                        else if (ParamData.ApplicationCategoryType == 65536)
                        {
                            NewPS5Game.GameCategory = "Type: Native Media App";
                        }
                        else if (ParamData.ApplicationCategoryType == 65792)
                        {
                            NewPS5Game.GameCategory = "Type: RNPS Media App";
                        }
                        else if (ParamData.ApplicationCategoryType == 131328)
                        {
                            NewPS5Game.GameCategory = "Type: System Built-in App";
                        }
                        else if (ParamData.ApplicationCategoryType == 131584)
                        {
                            NewPS5Game.GameCategory = "Type: Big Daemon";
                        }
                        else if (ParamData.ApplicationCategoryType == 16777216)
                        {
                            NewPS5Game.GameCategory = "Type: ShellUI";
                        }
                        else if (ParamData.ApplicationCategoryType == 33554432)
                        {
                            NewPS5Game.GameCategory = "Type: Daemon";
                        }
                        else if (ParamData.ApplicationCategoryType == 67108864)
                        {
                            NewPS5Game.GameCategory = "Type: ShellApp";
                        }
                        else
                        {
                            NewPS5Game.GameCategory = "Type: Unknown";
                        }

                        NewPS5Game.GameFileOrFolderPath = MainGamePath;
                        NewPS5Game.GameSize = "Size: " + Utils.HumanReadableBytes(Utils.GetDirectorySize(MainGamePath));

                        if (ParamData.ContentVersion is not null)
                        {
                            NewPS5Game.GameVersion = "Version: " + ParamData.ContentVersion;
                        }
                        if (ParamData.RequiredSystemSoftwareVersion is not null)
                        {
                            NewPS5Game.GameRequiredFirmware = "Required Firmware: " + ParamData.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, ".").Insert(5, ".").Insert(8, ".").Remove(11, 8);

                            if (string.Compare(ParamData.RequiredSystemSoftwareVersion, "0x1001000000000000", false) > 0)
                            {
                                NewPS5Game.IsCompatibleFW = "The required firmware for this game is too high and might not be supported.";
                            }
                            else
                            {
                                NewPS5Game.IsCompatibleFW = "The required firmware for this game is compatible.";
                            }
                        }
                        if (ParamData.MasterVersion is not null)
                        {
                            NewPS5Game.GameMasterVersion = "Master Version: " + ParamData.MasterVersion;
                        }
                        if (ParamData.SdkVersion is not null)
                        {
                            NewPS5Game.GameSDKVersion = "SDK Version: " + ParamData.SdkVersion;
                        }
                        if (ParamData.Pubtools is not null)
                        {
                            NewPS5Game.GamePubToolVersion = "PubTools Version: " + ParamData.Pubtools.ToolVersion;
                        }
                        if (ParamData.VersionFileUri is not null)
                        {
                            NewPS5Game.GameVersionFileURI = ParamData.VersionFileUri;
                        }

                        // Check for game icon
                        if (WorkerArgs.LoadIcons)
                        {
                            if (File.Exists(Path.Combine(SCESYSFolder, "icon0.png")))
                            {
                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(SCESYSFolder, "icon0.png"));
                                    NewPS5Game.GameCoverSource = TempBitmapImage;
                                });
                            }
                            else if (ParamData.ApplicationCategoryType == 0 & ParamData.TitleId!.StartsWith("PP"))
                            {
                                if (Utils.IsURLValid("https://prosperopatches.com/" + ParamData.TitleId.Trim()).Result == true)
                                {
                                    URLs.Add("https://prosperopatches.com/" + ParamData.TitleId.Trim()); // Get the image from prosperopatches
                                }
                            }
                        }

                        // Check for game background
                        if (WorkerArgs.LoadBackgrounds)
                        {
                            if (File.Exists(Path.Combine(SCESYSFolder, "pic0.png")))
                            {
                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(SCESYSFolder, "pic0.png"));
                                    NewPS5Game.GameBGSource = TempBitmapImage;
                                });
                            }
                        }

                        // Check for game soundtrack
                        if (File.Exists(Path.Combine(SCESYSFolder, "snd0.at9")))
                        {
                            NewPS5Game.GameSoundFile = Path.Combine(SCESYSFolder, "snd0.at9");
                        }

                        // Add other available content ids as tooltip
                        string GameContentIDs = "";
                        if (File.Exists(Path.Combine(MainGamePath, "contentids.json")))
                        {
                            foreach (var Line in File.ReadAllLines(Path.Combine(MainGamePath, "contentids.json")))
                            {
                                if (!string.IsNullOrWhiteSpace(Line) && Line.StartsWith('\t'))
                                {
                                    GameContentIDs += Line.Split('"')[1] + "\r\n";
                                }
                            }
                            if (!string.IsNullOrEmpty(GameContentIDs))
                            {
                                GameContentIDs = GameContentIDs.TrimEnd();
                                NewPS5Game.GameContentIDs = GameContentIDs;
                            }
                        }

                        string ToolTipString = "This game includes: ";
                        if (WorkerArgs.SkipFileChecks == false)
                        {
                            // Check if prx is encrypted
                            if (File.Exists(Path.Combine(MainGamePath + "sce_module", "libc.prx")))
                            {
                                string FirstStr = "";
                                using (var PRXReader = new FileStream(Path.Combine(MainGamePath, "sce_module", "libc.prx"), FileMode.Open, FileAccess.Read))
                                {
                                    var BinReader = new BinaryReader(PRXReader);
                                    FirstStr = BinReader.ReadString();
                                    PRXReader.Close();
                                }
                                if (!string.IsNullOrEmpty(FirstStr))
                                {
                                    if (FirstStr.Contains("ELF"))
                                    {
                                        ToolTipString += Environment.NewLine + "Decrypted .prx files";
                                    }
                                    else
                                    {
                                        ToolTipString += Environment.NewLine + "Encrypted .prx files";
                                    }
                                }
                            }
                            // Check if eboot is encrypted and signed
                            if (File.Exists(Path.Combine(MainGamePath, "eboot.bin")))
                            {
                                string FirstStr = "";
                                string SecondStr = "";
                                using (var EBOOTReader = new FileStream(Path.Combine(MainGamePath, "eboot.bin"), FileMode.Open, FileAccess.Read))
                                {
                                    var BinReader = new BinaryReader(EBOOTReader);

                                    FirstStr = BinReader.ReadString();
                                    BinReader.BaseStream.Seek(416L, SeekOrigin.Begin);
                                    SecondStr = BinReader.ReadString();

                                    BinReader.Close();
                                    EBOOTReader.Close();
                                }
                                if (!string.IsNullOrEmpty(FirstStr))
                                {
                                    if (FirstStr.Contains("ELF"))
                                    {
                                        ToolTipString += Environment.NewLine + "EBOOT: Decrypted";
                                    }
                                    else
                                    {
                                        ToolTipString += Environment.NewLine + "EBOOT: Encrypted";
                                    }
                                }
                                if (!string.IsNullOrEmpty(SecondStr))
                                {
                                    if (SecondStr.Contains("ELF"))
                                    {
                                        ToolTipString += Environment.NewLine + "EBOOT: Signed";
                                    }
                                    else
                                    {
                                        ToolTipString += Environment.NewLine + "EBOOT: Decrypted & Unsigned";
                                    }
                                }
                            }
                            // Check for some other encrypted files
                            if (File.Exists(Path.Combine(SCESYSFolder, "trophy2", "trophy00.UCP")))
                            {
                                ToolTipString += Environment.NewLine + "Trophy2: trophy00.UCP";
                            }
                            if (File.Exists(Path.Combine(SCESYSFolder, "uds", "uds00.ucp")))
                            {
                                ToolTipString += Environment.NewLine + "UDS: uds00.ucp";
                            }
                            if (File.Exists(Path.Combine(SCESYSFolder, "keystone")))
                            {
                                ToolTipString += Environment.NewLine + "Keystone: keystone";
                            }
                            if (File.Exists(Path.Combine(SCESYSFolder, "nptitle.dat")))
                            {
                                ToolTipString += Environment.NewLine + "NPTitle: nptitle.dat";
                            }
                            if (File.Exists(Path.Combine(MainGamePath, "disc_info.dat")))
                            {
                                ToolTipString += Environment.NewLine + "Disc Info: disc_info.dat";
                            }
                            if (File.Exists(Path.Combine(MainGamePath, "ext_info.dat")))
                            {
                                ToolTipString += Environment.NewLine + "Ext Info: ext_info.dat";
                            }
                            if (File.Exists(Path.Combine(SCESYSFolder, "about", "right.sprx")))
                            {
                                ToolTipString += Environment.NewLine + "Right: right.sprx";
                            }
                            if (File.Exists(Path.Combine(SCESYSFolder, "about", "right.sprx.auth_info")))
                            {
                                ToolTipString += Environment.NewLine + "Right Auth info: right.sprx.auth_info";
                            }
                        }
                        else
                        {
                            ToolTipString = "File checks skipped";
                        }

                        NewPS5Game.DecFilesIncluded = ToolTipString;

                        // Update progress
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            NewLoadingWindow.LoadProgressBar.Value += 1;
                            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading backup " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + ValidBackups.Count.ToString();
                        });

                        // Add to the ListView
                        if (ParamData.ApplicationCategoryType == 0 & ParamData.TitleId!.StartsWith("PP")) // Games
                        {
                            Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(NewPS5Game));
                        }
                        else if (ParamData.ApplicationCategoryType == 65536 & ParamData.TitleId.StartsWith("PP")) // Media apps
                        {
                            Dispatcher.UIThread.Invoke(() => AppsListBox.Items.Add(NewPS5Game));
                        }
                        else
                        {
                            Dispatcher.UIThread.Invoke(() => AppsListBox.Items.Add(NewPS5Game));
                        }
                    }
                }

                if (URLs.Count > 0)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        NewLoadingWindow.LoadStatusTextBlock.Text = "Getting " + URLs.Count.ToString() + " available covers";
                        NewLoadingWindow.LoadProgressBar.Value = 0;
                        NewLoadingWindow.LoadProgressBar.Maximum = URLs.Count;
                    });

                    ContentWebView.Address = URLs[0];
                }
            }
            catch (Exception)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error accessing files. Please retry while running as Administrator.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        });

        NewLoadingWindow.Close();
    }

    private async void LoadFTPFolderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ConsoleIP))
        {
            await Task.Run(async () =>
            {

                Dispatcher.UIThread.Invoke(() =>
                {
                    // Clear
                    GamesListBox.Items.Clear();
                    AppsListBox.Items.Clear();

                    // Show the loading progress window
                    NewLoadingWindow = new SyncWindow() { Title = "Loading PS5 files", ShowActivated = true };
                    NewLoadingWindow.LoadProgressBar.IsIndeterminate = true;
                    NewLoadingWindow.LoadStatusTextBlock.Text = "Loading files, please wait ...";
                    NewLoadingWindow.Show(this);
                });

                try
                {
                    // Get installed games and apps over FTP
                    int CPort = int.Parse(ConsoleFTPPort);
                    using var conn = new FtpClient(ConsoleIP, "anonymous", "anonymous", CPort);
                    // Configurate the FTP connection
                    conn.Config.EncryptionMode = FtpEncryptionMode.None;
                    conn.Config.SslProtocols = System.Security.Authentication.SslProtocols.None;
                    conn.Config.DataConnectionEncryption = false;

                    // Connect
                    conn.Connect();

                    // List backups on /data/homebrew
                    foreach (var item in conn.GetListing("/data/homebrew"))
                    {
                        if (item.Type == FtpObjectType.Directory)
                        {
                            var PS5GameLVItem = new PS5Game() { GameBackupType = "FTP", GameLocation = PS5Game.Location.Remote, GameRootLocation = PS5Game.RootLocation.Internal };

                            // Check for icon0.png
                            if (conn.GetObjectInfo(item.FullName + "/sce_sys/icon0.png") is not null)
                            {
                                if (conn.DownloadBytes(out byte[] Icon0Bytes, item.FullName + "/sce_sys/icon0.png"))
                                {
                                    PS5GameLVItem.GameCoverSource = AnyBitmap.FromBytes(Icon0Bytes);
                                }
                            }

                            // Check for icon0.png
                            if (conn.GetObjectInfo(item.FullName + "/sce_sys/pic0.png") is not null)
                            {
                                if (conn.DownloadBytes(out byte[] Pic0Bytes, item.FullName + "/sce_sys/pic0.png"))
                                {
                                    PS5GameLVItem.GameBGSource = AnyBitmap.FromBytes(Pic0Bytes);
                                }
                            }

                            // Check for param.json
                            if (conn.GetObjectInfo(item.FullName + "/sce_sys/param.json") is not null)
                            {
                                if (conn.DownloadBytes(out byte[] ParamBytes, item.FullName + "/sce_sys/param.json"))
                                {
                                    string ParamBytesAsString = Encoding.UTF8.GetString(ParamBytes);
                                    var BackupInfos = JsonConvert.DeserializeObject<PS5Param>(ParamBytesAsString);
                                    if (BackupInfos is not null)
                                    {
                                        PS5GameLVItem.GameFileOrFolderPath = item.FullName;
                                        PS5GameLVItem.GameID = "Title ID: " + BackupInfos.TitleId;
                                        PS5GameLVItem.GameTitle = BackupInfos.LocalizedParameters!.EnUS!.TitleName;
                                        PS5GameLVItem.GameContentID = "Content ID: " + BackupInfos.ContentId;
                                        PS5GameLVItem.GameRegion = "Region: " + PS4Game.GetGameRegion(BackupInfos.ContentId!);

                                        if (BackupInfos.LocalizedParameters.EnUS is not null)
                                        {
                                            PS5GameLVItem.GameTitle = BackupInfos.LocalizedParameters.EnUS.TitleName;
                                        }
                                        if (BackupInfos.LocalizedParameters.DeDE is not null)
                                        {
                                            PS5GameLVItem.DEGameTitle = BackupInfos.LocalizedParameters.DeDE.TitleName;
                                        }
                                        if (BackupInfos.LocalizedParameters.FrFR is not null)
                                        {
                                            PS5GameLVItem.FRGameTitle = BackupInfos.LocalizedParameters.FrFR.TitleName;
                                        }
                                        if (BackupInfos.LocalizedParameters.ItIT is not null)
                                        {
                                            PS5GameLVItem.ITGameTitle = BackupInfos.LocalizedParameters.ItIT.TitleName;
                                        }
                                        if (BackupInfos.LocalizedParameters.EsES is not null)
                                        {
                                            PS5GameLVItem.ESGameTitle = BackupInfos.LocalizedParameters.EsES.TitleName;
                                        }
                                        if (BackupInfos.LocalizedParameters.JaJP is not null)
                                        {
                                            PS5GameLVItem.JPGameTitle = BackupInfos.LocalizedParameters.JaJP.TitleName;
                                        }

                                        if (BackupInfos.ApplicationCategoryType == 0)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: Game";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 65536)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: Native Media App";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 65792)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: RNPS Media App";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 131328)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: System Built-in App";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 131584)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: Big Daemon";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 16777216)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: ShellUI";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 33554432)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: Daemon";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 67108864)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: ShellApp";
                                        }
                                        else
                                        {
                                            PS5GameLVItem.GameCategory = "Type: Unknown";
                                        }

                                        long BackupSize = conn.GetObjectInfo(item.FullName).Size;
                                        PS5GameLVItem.GameSize = $"Size: {Utils.HumanReadableBytes(BackupSize)}";

                                        if (BackupInfos.ContentVersion is not null)
                                        {
                                            PS5GameLVItem.GameVersion = "Version: " + BackupInfos.ContentVersion;
                                        }
                                        if (BackupInfos.RequiredSystemSoftwareVersion is not null)
                                        {
                                            PS5GameLVItem.GameRequiredFirmware = "Required Firmware: " + BackupInfos.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, ".").Insert(5, ".").Insert(8, ".").Remove(11, 8);
                                        }
                                        if (BackupInfos.MasterVersion is not null)
                                        {
                                            PS5GameLVItem.GameMasterVersion = "Master Version: " + BackupInfos.MasterVersion;
                                        }
                                        if (BackupInfos.SdkVersion is not null)
                                        {
                                            PS5GameLVItem.GameSDKVersion = "SDK Version: " + BackupInfos.SdkVersion;
                                        }
                                        if (BackupInfos.Pubtools!.ToolVersion is not null)
                                        {
                                            PS5GameLVItem.GamePubToolVersion = "PubTools Version: " + BackupInfos.Pubtools.ToolVersion;
                                        }
                                        if (BackupInfos.VersionFileUri is not null)
                                        {
                                            PS5GameLVItem.GameVersionFileURI = BackupInfos.VersionFileUri;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }

                            // Add to the GamesListBox
                            Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(PS5GameLVItem));
                        }
                    }

                    // List backups on connected USB0
                    foreach (var item in conn.GetListing("/mnt/usb0/homebrew"))
                    {
                        if (item.Type == FtpObjectType.Directory)
                        {
                            var PS5GameLVItem = new PS5Game() { GameBackupType = "FTP", GameLocation = PS5Game.Location.Remote, GameRootLocation = PS5Game.RootLocation.USB };

                            // Check for icon0.png
                            if (conn.GetObjectInfo(item.FullName + "/sce_sys/icon0.png") is not null)
                            {
                                if (conn.DownloadBytes(out byte[] Icon0Bytes, item.FullName + "/sce_sys/icon0.png"))
                                {
                                    PS5GameLVItem.GameCoverSource = AnyBitmap.FromBytes(Icon0Bytes);
                                }
                            }

                            // Check for icon0.png
                            if (conn.GetObjectInfo(item.FullName + "/sce_sys/pic0.png") is not null)
                            {
                                if (conn.DownloadBytes(out byte[] Pic0Bytes, item.FullName + "/sce_sys/pic0.png"))
                                {
                                    PS5GameLVItem.GameBGSource = AnyBitmap.FromBytes(Pic0Bytes);
                                }
                            }

                            // Check for param.json
                            if (conn.GetObjectInfo(item.FullName + "/sce_sys/param.json") is not null)
                            {
                                if (conn.DownloadBytes(out byte[] ParamBytes, item.FullName + "/sce_sys/param.json"))
                                {
                                    string ParamBytesAsString = Encoding.UTF8.GetString(ParamBytes);
                                    var BackupInfos = JsonConvert.DeserializeObject<PS5Param>(ParamBytesAsString);
                                    if (BackupInfos is not null)
                                    {
                                        PS5GameLVItem.GameFileOrFolderPath = item.FullName;
                                        PS5GameLVItem.GameID = "Title ID: " + BackupInfos.TitleId;
                                        PS5GameLVItem.GameTitle = BackupInfos.LocalizedParameters!.EnUS!.TitleName;
                                        PS5GameLVItem.GameContentID = "Content ID: " + BackupInfos.ContentId;
                                        PS5GameLVItem.GameRegion = "Region: " + PS4Game.GetGameRegion(BackupInfos.ContentId!);

                                        if (BackupInfos.LocalizedParameters.EnUS is not null)
                                        {
                                            PS5GameLVItem.GameTitle = BackupInfos.LocalizedParameters.EnUS.TitleName;
                                        }
                                        if (BackupInfos.LocalizedParameters.DeDE is not null)
                                        {
                                            PS5GameLVItem.DEGameTitle = BackupInfos.LocalizedParameters.DeDE.TitleName;
                                        }
                                        if (BackupInfos.LocalizedParameters.FrFR is not null)
                                        {
                                            PS5GameLVItem.FRGameTitle = BackupInfos.LocalizedParameters.FrFR.TitleName;
                                        }
                                        if (BackupInfos.LocalizedParameters.ItIT is not null)
                                        {
                                            PS5GameLVItem.ITGameTitle = BackupInfos.LocalizedParameters.ItIT.TitleName;
                                        }
                                        if (BackupInfos.LocalizedParameters.EsES is not null)
                                        {
                                            PS5GameLVItem.ESGameTitle = BackupInfos.LocalizedParameters.EsES.TitleName;
                                        }
                                        if (BackupInfos.LocalizedParameters.JaJP is not null)
                                        {
                                            PS5GameLVItem.JPGameTitle = BackupInfos.LocalizedParameters.JaJP.TitleName;
                                        }

                                        if (BackupInfos.ApplicationCategoryType == 0)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: Game";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 65536)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: Native Media App";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 65792)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: RNPS Media App";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 131328)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: System Built-in App";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 131584)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: Big Daemon";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 16777216)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: ShellUI";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 33554432)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: Daemon";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 67108864)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: ShellApp";
                                        }
                                        else
                                        {
                                            PS5GameLVItem.GameCategory = "Type: Unknown";
                                        }

                                        long BackupSize = conn.GetObjectInfo(item.FullName).Size;
                                        PS5GameLVItem.GameSize = $"Size: {Utils.HumanReadableBytes(BackupSize)}";

                                        if (BackupInfos.ContentVersion is not null)
                                        {
                                            PS5GameLVItem.GameVersion = "Version: " + BackupInfos.ContentVersion;
                                        }
                                        if (BackupInfos.RequiredSystemSoftwareVersion is not null)
                                        {
                                            PS5GameLVItem.GameRequiredFirmware = "Required Firmware: " + BackupInfos.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, ".").Insert(5, ".").Insert(8, ".").Remove(11, 8);
                                        }
                                        if (BackupInfos.MasterVersion is not null)
                                        {
                                            PS5GameLVItem.GameMasterVersion = "Master Version: " + BackupInfos.MasterVersion;
                                        }
                                        if (BackupInfos.SdkVersion is not null)
                                        {
                                            PS5GameLVItem.GameSDKVersion = "SDK Version: " + BackupInfos.SdkVersion;
                                        }
                                        if (BackupInfos.Pubtools!.ToolVersion is not null)
                                        {
                                            PS5GameLVItem.GamePubToolVersion = "PubTools Version: " + BackupInfos.Pubtools.ToolVersion;
                                        }
                                        if (BackupInfos.VersionFileUri is not null)
                                        {
                                            PS5GameLVItem.GameVersionFileURI = BackupInfos.VersionFileUri;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }

                            // Add to the GamesListBox
                            Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(PS5GameLVItem));
                        }
                    }
                    // List backups on connected USB1
                    foreach (var item in conn.GetListing("/mnt/usb1/homebrew"))
                    {
                        if (item.Type == FtpObjectType.Directory)
                        {
                            var PS5GameLVItem = new PS5Game() { GameBackupType = "FTP", GameLocation = PS5Game.Location.Remote, GameRootLocation = PS5Game.RootLocation.USB };

                            // Check for icon0.png
                            if (conn.GetObjectInfo(item.FullName + "/sce_sys/icon0.png") is not null)
                            {
                                if (conn.DownloadBytes(out byte[] Icon0Bytes, item.FullName + "/sce_sys/icon0.png"))
                                {
                                    PS5GameLVItem.GameCoverSource = AnyBitmap.FromBytes(Icon0Bytes);
                                }
                            }

                            // Check for icon0.png
                            if (conn.GetObjectInfo(item.FullName + "/sce_sys/pic0.png") is not null)
                            {
                                if (conn.DownloadBytes(out byte[] Pic0Bytes, item.FullName + "/sce_sys/pic0.png"))
                                {
                                    PS5GameLVItem.GameBGSource = AnyBitmap.FromBytes(Pic0Bytes);
                                }
                            }

                            // Check for param.json
                            if (conn.GetObjectInfo(item.FullName + "/sce_sys/param.json") is not null)
                            {
                                if (conn.DownloadBytes(out byte[] ParamBytes, item.FullName + "/sce_sys/param.json"))
                                {
                                    string ParamBytesAsString = Encoding.UTF8.GetString(ParamBytes);
                                    var BackupInfos = JsonConvert.DeserializeObject<PS5Param>(ParamBytesAsString);
                                    if (BackupInfos is not null)
                                    {
                                        PS5GameLVItem.GameFileOrFolderPath = item.FullName;
                                        PS5GameLVItem.GameID = "Title ID: " + BackupInfos.TitleId;
                                        PS5GameLVItem.GameTitle = BackupInfos.LocalizedParameters!.EnUS!.TitleName;
                                        PS5GameLVItem.GameContentID = "Content ID: " + BackupInfos.ContentId;
                                        PS5GameLVItem.GameRegion = "Region: " + PS4Game.GetGameRegion(BackupInfos.ContentId!);

                                        if (BackupInfos.LocalizedParameters.EnUS is not null)
                                        {
                                            PS5GameLVItem.GameTitle = BackupInfos.LocalizedParameters.EnUS.TitleName;
                                        }
                                        if (BackupInfos.LocalizedParameters.DeDE is not null)
                                        {
                                            PS5GameLVItem.DEGameTitle = BackupInfos.LocalizedParameters.DeDE.TitleName;
                                        }
                                        if (BackupInfos.LocalizedParameters.FrFR is not null)
                                        {
                                            PS5GameLVItem.FRGameTitle = BackupInfos.LocalizedParameters.FrFR.TitleName;
                                        }
                                        if (BackupInfos.LocalizedParameters.ItIT is not null)
                                        {
                                            PS5GameLVItem.ITGameTitle = BackupInfos.LocalizedParameters.ItIT.TitleName;
                                        }
                                        if (BackupInfos.LocalizedParameters.EsES is not null)
                                        {
                                            PS5GameLVItem.ESGameTitle = BackupInfos.LocalizedParameters.EsES.TitleName;
                                        }
                                        if (BackupInfos.LocalizedParameters.JaJP is not null)
                                        {
                                            PS5GameLVItem.JPGameTitle = BackupInfos.LocalizedParameters.JaJP.TitleName;
                                        }

                                        if (BackupInfos.ApplicationCategoryType == 0)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: Game";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 65536)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: Native Media App";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 65792)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: RNPS Media App";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 131328)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: System Built-in App";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 131584)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: Big Daemon";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 16777216)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: ShellUI";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 33554432)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: Daemon";
                                        }
                                        else if (BackupInfos.ApplicationCategoryType == 67108864)
                                        {
                                            PS5GameLVItem.GameCategory = "Type: ShellApp";
                                        }
                                        else
                                        {
                                            PS5GameLVItem.GameCategory = "Type: Unknown";
                                        }

                                        long BackupSize = conn.GetObjectInfo(item.FullName).Size;
                                        PS5GameLVItem.GameSize = $"Size: {Utils.HumanReadableBytes(BackupSize)}";

                                        if (BackupInfos.ContentVersion is not null)
                                        {
                                            PS5GameLVItem.GameVersion = "Version: " + BackupInfos.ContentVersion;
                                        }
                                        if (BackupInfos.RequiredSystemSoftwareVersion is not null)
                                        {
                                            PS5GameLVItem.GameRequiredFirmware = "Required Firmware: " + BackupInfos.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, ".").Insert(5, ".").Insert(8, ".").Remove(11, 8);
                                        }
                                        if (BackupInfos.MasterVersion is not null)
                                        {
                                            PS5GameLVItem.GameMasterVersion = "Master Version: " + BackupInfos.MasterVersion;
                                        }
                                        if (BackupInfos.SdkVersion is not null)
                                        {
                                            PS5GameLVItem.GameSDKVersion = "SDK Version: " + BackupInfos.SdkVersion;
                                        }
                                        if (BackupInfos.Pubtools!.ToolVersion is not null)
                                        {
                                            PS5GameLVItem.GamePubToolVersion = "PubTools Version: " + BackupInfos.Pubtools.ToolVersion;
                                        }
                                        if (BackupInfos.VersionFileUri is not null)
                                        {
                                            PS5GameLVItem.GameVersionFileURI = BackupInfos.VersionFileUri;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }

                            // Add to the GamesListBox
                            Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(PS5GameLVItem));
                        }
                    }

                    // Disconnect
                    conn.Disconnect();
                }
                catch (Exception ex)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.ToString(), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            });

            NewLoadingWindow.Close();

            if (URLs.Count > 0)
            {
                NewLoadingWindow.LoadStatusTextBlock.Text = "Getting " + URLs.Count.ToString() + " available covers";
                NewLoadingWindow.LoadProgressBar.Value = 0;
                NewLoadingWindow.LoadProgressBar.Maximum = URLs.Count;

                ContentWebView.Address = URLs[0];
            }
            else
            {
                NewLoadingWindow.Close();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please enter your console's FTP IP address in the settings before continuing.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async void LoadPatchPKGFolderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog { Title = "Select your PS5 patches folder" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            PatchesListBox.Items.Clear();

            IEnumerable<string> FoundPKGs = Directory.EnumerateFiles(FBDResult, "*_sc.pkg", SearchOption.AllDirectories);
            PKGCount = FoundPKGs.Count();

            NewLoadingWindow = new SyncWindow() { Title = "Loading PS5 files", ShowActivated = true };
            NewLoadingWindow.LoadProgressBar.Maximum = PKGCount;
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + PKGCount.ToString();
            NewLoadingWindow.Show(this);

            // PS5 Patch Source pkgs
            foreach (string PatchSCPKG in FoundPKGs)
            {

                var NewPS5Game = new PS5Game() { GameBackupType = "Patch" };
                var PKGFileInfo = new FileInfo(PatchSCPKG);

                TotalSize = 0L;

                using var PARAMReader = new Process();
                PARAMReader.StartInfo.FileName = Environment.CurrentDirectory + @"\Tools\ps5_pkg.exe";
                PARAMReader.StartInfo.Arguments = "--psmtparam file:\"" + PatchSCPKG + "\"";
                PARAMReader.StartInfo.RedirectStandardOutput = true;
                PARAMReader.StartInfo.UseShellExecute = false;
                PARAMReader.StartInfo.CreateNoWindow = true;
                PARAMReader.Start();

                var OutputReader = PARAMReader.StandardOutput;
                string ProcessOutput = OutputReader.ReadToEnd();

                if (ProcessOutput.Length > 0)
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(ProcessOutput);

                    if (ParamData is not null)
                    {
                        NewPS5Game.GameID = "Title ID: " + ParamData.TitleId;
                        NewPS5Game.GameTitle = ParamData.LocalizedParameters!.EnUS!.TitleName;
                        NewPS5Game.GameContentID = "Content ID: " + ParamData.ContentId;
                        NewPS5Game.GameRegion = "Region: " + PS5Game.GetGameRegion(ParamData.TitleId!);

                        if (ParamData.ApplicationCategoryType == 0)
                        {
                            NewPS5Game.GameCategory = "Type: Game";
                        }
                        else if (ParamData.ApplicationCategoryType == 65536)
                        {
                            NewPS5Game.GameCategory = "Type: Native Media App";
                        }
                        else if (ParamData.ApplicationCategoryType == 65792)
                        {
                            NewPS5Game.GameCategory = "Type: RNPS Media App";
                        }
                        else if (ParamData.ApplicationCategoryType == 131328)
                        {
                            NewPS5Game.GameCategory = "Type: System Built-in App";
                        }
                        else if (ParamData.ApplicationCategoryType == 131584)
                        {
                            NewPS5Game.GameCategory = "Type: Big Daemon";
                        }
                        else if (ParamData.ApplicationCategoryType == 16777216)
                        {
                            NewPS5Game.GameCategory = "Type: ShellUI";
                        }
                        else if (ParamData.ApplicationCategoryType == 33554432)
                        {
                            NewPS5Game.GameCategory = "Type: Daemon";
                        }
                        else if (ParamData.ApplicationCategoryType == 67108864)
                        {
                            NewPS5Game.GameCategory = "Type: ShellApp";
                        }
                        else
                        {
                            NewPS5Game.GameCategory = "Type: Unknown";
                        }

                        NewPS5Game.GameSize = "Size: " + Utils.HumanReadableBytes(GetDirSize(PKGFileInfo.DirectoryName!));
                        NewPS5Game.GameVersion = "Version: " + ParamData.ContentVersion;
                        NewPS5Game.GameRequiredFirmware = "Req.FW: " + ParamData.RequiredSystemSoftwareVersion!.Replace("0x", "").Insert(2, ".").Insert(5, ".").Insert(8, ".").Remove(11, 8);

                        if (await Utils.IsURLValid("https://prosperopatches.com/" + ParamData.TitleId!.Trim()))
                        {
                            URLs.Add("https://prosperopatches.com/" + ParamData.TitleId.Trim()); // Get the image from prosperopatches
                        }

                        Dispatcher.UIThread.Invoke(() =>
                        {
                            NewLoadingWindow.LoadProgressBar.Value += 1;
                            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading PKG " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + PKGCount.ToString();
                        });

                        // Add to the ListView
                        if (ParamData.ApplicationCategoryType == 0 && ParamData.TitleId!.StartsWith("PP") && PKGFileInfo.Name == "app_sc.pkg") // Installed Games or to be installed
                        {
                            Dispatcher.UIThread.Invoke(() => GamesListBox.Items.Add(NewPS5Game));
                        }
                        else if (ParamData.ApplicationCategoryType == 0 && ParamData.TitleId.StartsWith("PP") && PKGFileInfo.Name.StartsWith("UP")) // Update/Patch source PKGs for games
                        {
                            Dispatcher.UIThread.Invoke(() => PatchesListBox.Items.Add(NewPS5Game));
                        }
                        else if (ParamData.ApplicationCategoryType == 65536 && ParamData.TitleId.StartsWith("PP") && PKGFileInfo.Name == "app_sc.pkg") // Installed Media apps or to be installed
                        {
                            Dispatcher.UIThread.Invoke(() => AppsListBox.Items.Add(NewPS5Game));
                        }
                        else if (ParamData.ApplicationCategoryType == 65536 && ParamData.TitleId.StartsWith("PP") && PKGFileInfo.Name.StartsWith("UP")) // Update/Patch source PKGs for Media apps
                        {
                            Dispatcher.UIThread.Invoke(() => PatchesListBox.Items.Add(NewPS5Game));
                        }
                        else
                        {
                            Dispatcher.UIThread.Invoke(() => AppsListBox.Items.Add(NewPS5Game));
                        }

                    }
                }
            }

            if (URLs.Count > 0)
            {
                NewLoadingWindow.LoadStatusTextBlock.Text = "Getting " + URLs.Count.ToString() + " available covers";
                NewLoadingWindow.LoadProgressBar.Value = 0;
                NewLoadingWindow.LoadProgressBar.Maximum = URLs.Count;

                ContentWebView.Address = URLs[0];
            }
            else
            {
                NewLoadingWindow.Close();
            }

        }
    }

    private void OpenDownloadsFolderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        Utils.OpenDownloadsFolder();
    }

    #endregion

    #region Game Context Menu Actions

    private void GamesContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;

            if (SelectedPS5Game.GameLocation == PS5Game.Location.Local)
            {
                GamesContextMenu.Items.Add(GameOpenLocationMenuItem);
                GamesContextMenu.Items.Add(GameCopyToMenuItem);
                GamesContextMenu.Items.Add(GameBrowseAssetsMenuItem);
                GamesContextMenu.Items.Add(GamePlayMenuItem);
                GamesContextMenu.Items.Add(GameCheckForUpdatesMenuItem);

                if (SelectedPS5Game.GameBackupType == "Folder")
                {
                    GamesContextMenu.Items.Add(GamePackAsPKG);
                }

                GamesContextMenu.Items.Add(new Separator());
                GamesContextMenu.Items.Add(GameChangeTypeMenuItem);
                GamesContextMenu.Items.Add(GameRenameMenuItem);
                GamesContextMenu.Items.Add(GameChangeIconMenuItem);
                GamesContextMenu.Items.Add(GameChangeBackgroundMenuItem);
                GamesContextMenu.Items.Add(GameChangeSoundtrackMenuItem);
            }
            else if (SelectedPS5Game.GameLocation == PS5Game.Location.Remote)
            {
                GamesContextMenu.Items.Add(GameLaunchMenuItem);

                // Add correct move options
                if (SelectedPS5Game.GameRootLocation == PS5Game.RootLocation.Internal)
                {
                }
                // GamesContextMenu.Items.Add(GameMoveToUSB0MenuItem)
                // GamesContextMenu.Items.Add(GameMoveToUSB1MenuItem)
                else
                {
                    // GamesContextMenu.Items.Add(GameMoveToInternalMenuItem)
                }
            }

        }
    }

    private void GamesContextMenu_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        GamesContextMenu.Items.Clear();
    }

    #region Local Context Menu Options

    private async void GameCopyToMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            if (!string.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath))
            {

                var FBD = new OpenFolderDialog() { Title = "Where do you want to copy the selected game ?" };
                var FBDResult = await FBD.ShowAsync(this);

                if (FBDResult != null)
                {
                    var NewCopyWindow = new CopyWindow()
                    {
                        GameIcon = AnyBitmap.FromFile(Path.Combine(SelectedPS5Game.GameFileOrFolderPath, "sce_sys", "icon0.png")),
                        ShowActivated = true,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        BackupDestinationPath = Utils.EnsureTrailingSeparator(FBDResult),
                        Title = "Copying " + SelectedPS5Game.GameTitle + " to " + Utils.EnsureTrailingSeparator(FBDResult),
                        BackupPath = SelectedPS5Game.GameFileOrFolderPath
                    };

                    if (SelectedPS5Game.GameCoverSource is not null)
                    {
                        NewCopyWindow.GameIcon = SelectedPS5Game.GameCoverSource;
                    }

                    if (await NewCopyWindow.ShowDialog<bool>(this) == true)
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Completed", "Game copied with success !", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box.ShowWindowAsync();
                    }
                }

            }
        }
    }

    private async void GamePlayMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            if (!string.IsNullOrEmpty(SelectedPS5Game.GameSoundFile))
            {
                if (IsSoundPlaying)
                {
                    Utils.StopGameSound();
                    IsSoundPlaying = false;
                    GamePlayMenuItem.Header = "Play Soundtrack";
                    GamePlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) };
                }
                else
                {
                    Utils.PlayGameSoundFile(SelectedPS5Game.GameSoundFile);
                    IsSoundPlaying = true;
                    GamePlayMenuItem.Header = "Stop Soundtrack";
                    GamePlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/stop.png"))) };
                }
            }
            else
            {
                if (IsSoundPlaying)
                {
                    Utils.StopGameSound();
                    IsSoundPlaying = false;
                    GamePlayMenuItem.Header = "Play Soundtrack";
                    GamePlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) };
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Info", "No game soundtrack found.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
        }
    }

    private void GameCheckForUpdatesMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            if (!string.IsNullOrEmpty(SelectedPS5Game.GameID))
            {
                var NewPS5GamePatches = new PS5GamePatches() { ShowActivated = true, SearchForGamePatchWithID = SelectedPS5Game.GameID.Split(["Title ID: "], StringSplitOptions.None)[1] };
                NewPS5GamePatches.Show();
            }
        }
    }

    private async void GameChangeToGameMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);
                    ParamData!.ApplicationCategoryType = 0;

                    string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);
                    SelectedPS5Game.GameCategory = "Type: Game";

                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "Game type changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void GameChangeToNativeMediaMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);
                    ParamData!.ApplicationCategoryType = 65536;

                    string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);
                    SelectedPS5Game.GameCategory = "Type: Media";

                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "Game type changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void GameChangeToRNPSMediaMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);
                    ParamData!.ApplicationCategoryType = 65792;

                    string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);
                    SelectedPS5Game.GameCategory = "Type: Media";

                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "Game type changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void GameChangeToShellAppMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);
                    ParamData!.ApplicationCategoryType = 67108864;

                    string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);
                    SelectedPS5Game.GameCategory = "Type: ShellApp";

                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "Game type changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void GameChangeToShellUIMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);
                    ParamData!.ApplicationCategoryType = 16777216;

                    string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);
                    SelectedPS5Game.GameCategory = "Type: ShellUI";

                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "Game type changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void GameChangeToBigDaemonMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);
                    ParamData!.ApplicationCategoryType = 131584;

                    string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);
                    SelectedPS5Game.GameCategory = "Type: BigDaemon";

                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "Game type changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void GameChangeToBuiltInMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);
                    ParamData!.ApplicationCategoryType = 131328;

                    string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);
                    SelectedPS5Game.GameCategory = "Type: System Built-In";

                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "Game type changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void GameChangeToDaemonMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);
                    ParamData!.ApplicationCategoryType = 33554432;

                    string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);
                    SelectedPS5Game.GameCategory = "Type: Daemon";

                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "Game type changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void GameRenameMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);

                    // Set new default language (optional)
                    var box = MessageBoxManager.GetMessageBoxStandard("Default Language", "Do you also want to change the default language for this game?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult = await box.ShowWindowDialogAsync(this);
                    if (boxresult == ButtonResult.Yes)
                    {
                        var NewInputDialog = new InputDialog() { Title = "Change default language" };
                        NewInputDialog.NewValueTextBox.Text = ParamData!.LocalizedParameters!.DefaultLanguage!;
                        NewInputDialog.InputDialogTitleTextBlock.Text = "Enter a new default language identifier (like en-US, de-DE, fr-FR, es-ES...):";
                        NewInputDialog.ConfirmButton.Content = "Confirm";

                        string NewDefaultLanguage = await NewInputDialog.ShowDialog<string>(this);
                        if (!string.IsNullOrEmpty(NewDefaultLanguage))
                        {
                            ParamData.LocalizedParameters.DefaultLanguage = NewDefaultLanguage;
                        }
                    }

                    // Set new title
                    var NewInputDialog2 = new InputDialog() { Title = "Change game title" };
                    NewInputDialog2.NewValueTextBox.Text = ParamData!.LocalizedParameters!.EnUS!.TitleName!;
                    NewInputDialog2.InputDialogTitleTextBlock.Text = "Enter a new title for this game:";
                    NewInputDialog2.ConfirmButton.Content = "Confirm";

                    string NewAppTitle = await NewInputDialog2.ShowDialog<string>(this);
                    if (!string.IsNullOrEmpty(NewAppTitle))
                    {
                        ParamData!.LocalizedParameters!.ArAE!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.CsCZ!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.DaDK!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.DeDE!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.ElGR!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.EnGB!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.EnUS!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.Es419!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.FiFI!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.FrCA!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.FrFR!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.HuHU!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.IdID!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.ItIT!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.JaJP!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.KoKR!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.NoNO!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.NlNL!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.PlPL!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.PtBR!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.PtPT!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.RoRO!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.RuRU!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.SvSE!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.ThTH!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.TrTR!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.ViVN!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.ZhHans!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.ZhHant!.TitleName = NewAppTitle;

                        // Write back to file
                        string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                        File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);

                        // Set new title in GamesListView
                        SelectedPS5Game.GameTitle = NewAppTitle;
                    }

                    box = MessageBoxManager.GetMessageBoxStandard("Info", "Game renamed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void GameChangeIconMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;

            var pngFileFilter = new FileDialogFilter
            {
                Name = "PNG Image File",
                Extensions = ["png"]
            };
            var OFD = new OpenFileDialog() { Title = "Select a new icon0.png image for this game", Filters = { pngFileFilter }, AllowMultiple = false };
            var OFDResult = await OFD.ShowAsync(this);

            if (OFDResult != null && OFDResult.Length > 0)
            {
                if (File.Exists(OFDResult[0]) && !string.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath))
                {
                    // Move new icon to sce_sys
                    File.Copy(OFDResult[0], Path.Combine(SelectedPS5Game.GameFileOrFolderPath, "sce_sys", "icon0.png"), true);

                    // Reload new icon
                    var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(SelectedPS5Game.GameFileOrFolderPath, "sce_sys", "icon0.png"));
                    SelectedPS5Game.GameCoverSource = TempBitmapImage;
                }
            }
        }
    }

    private async void GameChangeBackgroundMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;

            var pngFileFilter = new FileDialogFilter
            {
                Name = "PNG Image File",
                Extensions = ["png"]
            };
            var OFD = new OpenFileDialog() { Title = "Select a new pic0.png image for this game", Filters = { pngFileFilter }, AllowMultiple = false };
            var OFDResult = await OFD.ShowAsync(this);

            if (OFDResult != null && OFDResult.Length > 0)
            {
                if (File.Exists(OFDResult[0]) && !string.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath))
                {
                    File.Copy(OFDResult[0], Path.Combine(SelectedPS5Game.GameFileOrFolderPath, "sce_sys", "pic0.png"), true);

                    // Set new background
                    var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(SelectedPS5Game.GameFileOrFolderPath, "sce_sys", "pic0.png"));
                    SelectedPS5Game.GameBGSource = TempBitmapImage;
                }
            }
        }
    }

    private async void GameChangeSoundtrackMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;

            var at9FileFilter = new FileDialogFilter
            {
                Name = "at9 Sound File",
                Extensions = ["at9"]
            };
            var OFD = new OpenFileDialog() { Title = "Select a new snd0.at9 soundtrack for this game", Filters = { at9FileFilter }, AllowMultiple = false };

            if (IsSoundPlaying)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Info", "A soundtrack is currently playing. Please stop it before changing any soundtrack.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
            // Set new soundtrack
            else
            {
                var OFDResult = await OFD.ShowAsync(this);
                if (OFDResult != null && OFDResult.Length > 0)
                {
                    if (File.Exists(OFDResult[0]) && !string.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath))
                    {
                        File.Copy(OFDResult[0], Path.Combine(SelectedPS5Game.GameFileOrFolderPath, "sce_sys", "snd0.at9"), true);
                    }
                }
            }
        }
    }

    private void GameOpenLocationMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            if (!string.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath))
            {
                Utils.OpenFolder(SelectedPS5Game.GameFileOrFolderPath);
            }
        }
    }

    private void GameBrowseAssetsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            if (!string.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath))
            {
                var NewAssetBrowser = new PS5AssetsBrowser() { SelectedDirectory = SelectedPS5Game.GameFileOrFolderPath };
                NewAssetBrowser.Show();
            }
        }
    }

    private async void GamePackAsPKG_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            if (!string.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath))
            {

                // Create a GP5 project
                string AppProjectPath = SelectedPS5Game.GameFileOrFolderPath;
                var NewGP5Project = new XDocument(
                    new XElement("psproject", new XAttribute("fmt", "gp5"), new XAttribute("version", "1000"),
                    new XElement("volume",
                    new XElement("volume_type", "prospero_app"),
                    new XElement("package", new XAttribute("passcode", "00000000000000000000000000000000")),
                    new XElement("chunk_info", new XAttribute("chunk_count", "1"), new XAttribute("scenario_count", "1"),
                    new XElement("chunks",
                    new XElement("chunk", new XAttribute("id", "0"), new XAttribute("label", "Chunk #0"))),
                    new XElement("scenarios", new XAttribute("default_id", "0"),
                    new XElement("scenario", new XAttribute("id", "0"), new XAttribute("initial_chunk_count", "1"), new XAttribute("label", "Scenario #0"), new XAttribute("type", "playmode"), "0")))),
                    new XElement("global_exclude"),
                    new XElement("rootdir", new XAttribute("dir_exclude", "about"), new XAttribute("file_exclude", "*.esbak;keystone;*.dds;disc_info.dat;pfs-version.dat;ext_info.dat"), new XAttribute("src_path", AppProjectPath)))
                    );
                // Save the GP5 project
                string GP5ProjectPath = "";

                var gp5FileFilter = new FileDialogFilter
                {
                    Name = "GP5 Project File",
                    Extensions = ["gp5"]
                };
                var SFD = new SaveFileDialog() { Title = "Select a save path for the GP5 project", DefaultExtension = ".gp5", Filters = { gp5FileFilter } };
                var SFDResult = await SFD.ShowAsync(this);
                if (SFDResult != null)
                {
                    GP5ProjectPath = SFDResult;
                    NewGP5Project.Save(SFDResult);
                }

                // Show the PKG Builder
                var NewPKGBuilder = new PS5PKGBuilder() { ShowActivated = true };
                NewPKGBuilder.SelectedProjectTextBox.Text = GP5ProjectPath;
                NewPKGBuilder.Show();

                var box = MessageBoxManager.GetMessageBoxStandard("Info", "A GP5 project for " + SelectedPS5Game.GameTitle + " - [" + SelectedPS5Game.GameID + "] has been created." + Environment.NewLine + "Select an output path for the final PKG file and build it.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
        }
    }

    #endregion

    #region Remote Context Menu Options

    private async void GameLaunchMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            if (SelectedPS5Game.GameLocation == PS5Game.Location.Remote)
            {
                string GameTitleID = SelectedPS5Game.GameID!.Replace("Title ID: ", "").Trim();
                string HomebrewArgs;
                string HomebrewLoaderURL;
                var box = MessageBoxManager.GetMessageBoxStandard("Dump Runner Payload", "Is dump_runner.elf located at /data/homebrew ? Click 'No' if located on the USB or 'Cancel' to abort.", ButtonEnum.YesNo);
                var boxresult = await box.ShowWindowDialogAsync(this);

                if (boxresult == ButtonResult.Yes)
                {
                    HomebrewArgs = "/data/homebrew/dump_runner.elf+" + GameTitleID;
                    HomebrewLoaderURL = $"http://{ConsoleIP}:8080/hbldr?pipe=0&daemon=1&path=/data/homebrew/dump_runner.elf&args=" + HomebrewArgs + "&cwd=" + SelectedPS5Game.GameFileOrFolderPath;
                }
                else if (boxresult == ButtonResult.No)
                {
                    HomebrewArgs = "/mnt/usb0/homebrew/dump_runner.elf+" + GameTitleID;
                    HomebrewLoaderURL = $"http://{ConsoleIP}:8080/hbldr?pipe=0&daemon=1&path=/mnt/usb0/homebrew/dump_runner.elf&args=" + HomebrewArgs + "&cwd=" + SelectedPS5Game.GameFileOrFolderPath;
                }
                else
                {
                    box = MessageBoxManager.GetMessageBoxStandard("Aborted", "Aborted", ButtonEnum.Ok);
                    await box.ShowWindowAsync();
                    return;
                }

                // Launch
                NewPS5Menu.NavigateTowebMANWebSrvUrl(HomebrewLoaderURL);
            }
        }
    }

    #endregion

    #endregion

    #region Apps Context Menu Actions

    private async void AppCopyToMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;
            if (!string.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath))
            {

                var FBD = new OpenFolderDialog() { Title = "Where do you want to copy the selected app ?" };
                var FBDResult = await FBD.ShowAsync(this);

                if (FBDResult != null)
                {

                    var NewCopyWindow = new CopyWindow()
                    {
                        ShowActivated = true,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        BackupDestinationPath = Utils.EnsureTrailingSeparator(FBDResult),
                        Title = "Copying " + SelectedPS5Game.GameTitle + " to " + Utils.EnsureTrailingSeparator(FBDResult),
                        BackupPath = SelectedPS5Game.GameFileOrFolderPath
                    };

                    if (SelectedPS5Game.GameCoverSource is not null)
                    {
                        NewCopyWindow.GameIcon = SelectedPS5Game.GameCoverSource;
                    }

                    if (await NewCopyWindow.ShowDialog<bool>(this) == true)
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Completed", "App copied with success !", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box.ShowWindowAsync();
                    }
                }

            }
        }
    }

    private async void AppPlayMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;
            if (!string.IsNullOrEmpty(SelectedPS5Game.GameSoundFile))
            {
                if (IsSoundPlaying)
                {
                    Utils.StopGameSound();
                    IsSoundPlaying = false;
                    GamePlayMenuItem.Header = "Play Soundtrack";
                    GamePlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) };
                }
                else
                {
                    Utils.PlayGameSoundFile(SelectedPS5Game.GameSoundFile);
                    IsSoundPlaying = true;
                    GamePlayMenuItem.Header = "Stop Soundtrack";
                    GamePlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/stop.png"))) };
                }
            }
            else
            {
                if (IsSoundPlaying)
                {
                    Utils.StopGameSound();
                    IsSoundPlaying = false;
                    GamePlayMenuItem.Header = "Play Soundtrack";
                    GamePlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) };
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Info", "No app soundtrack found.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
        }
    }

    private async void AppCheckForUpdatesMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;
            if (!string.IsNullOrEmpty(SelectedPS5Game.GameID))
            {
                if (!SelectedPS5Game.GameID.StartsWith("NPXS"))
                {
                    var NewPS5GamePatches = new PS5GamePatches() { ShowActivated = true, SearchForGamePatchWithID = SelectedPS5Game.GameID.Split(["Title ID: "], StringSplitOptions.None)[1] };
                    NewPS5GamePatches.Show();
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Info", "Updates can only be checked for retail games and apps.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void AppChangeToGameMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);
                    ParamData!.ApplicationCategoryType = 0;

                    string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);
                    SelectedPS5Game.GameCategory = "Type: Game";

                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "App type changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void AppChangeToNativeMediaMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);
                    ParamData!.ApplicationCategoryType = 65536;

                    string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);
                    SelectedPS5Game.GameCategory = "Type: Native Media App";

                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "App type changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void AppChangeToRNPSMediaMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);
                    ParamData!.ApplicationCategoryType = 65792;

                    string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);
                    SelectedPS5Game.GameCategory = "Type: RNPS Media App";

                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "App type changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void AppChangeToBigDaemonMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);
                    ParamData!.ApplicationCategoryType = 131584;

                    string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);
                    SelectedPS5Game.GameCategory = "Type: Big Daemon";

                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "App type changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void AppChangeToBuiltInMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);
                    ParamData!.ApplicationCategoryType = 131328;

                    string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);
                    SelectedPS5Game.GameCategory = "Type: System Built-In";

                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "App type changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void AppChangeToDaemonMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);
                    ParamData!.ApplicationCategoryType = 33554432;

                    string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);
                    SelectedPS5Game.GameCategory = "Type: Daemon";

                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "App type changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void AppChangeToShellAppMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);
                    ParamData!.ApplicationCategoryType = 67108864;

                    string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);
                    SelectedPS5Game.GameCategory = "Type: ShellApp";

                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "App type changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void AppChangeToShellUIMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);
                    ParamData!.ApplicationCategoryType = 16777216;

                    string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);
                    SelectedPS5Game.GameCategory = "Type: ShellUI";

                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "App type changed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void AppRenameMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;
            if (File.Exists(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json")))
            {
                string JSONData = File.ReadAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"));
                try
                {
                    var ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData);

                    var box = MessageBoxManager.GetMessageBoxStandard("Default Language", "Do you also want to change the default language for this app?", ButtonEnum.YesNo);
                    var boxresult = await box.ShowWindowDialogAsync(this);
                    if (boxresult == ButtonResult.Yes)
                    {
                        var NewInputDialog = new InputDialog() { Title = "Change default language" };
                        NewInputDialog.NewValueTextBox.Text = ParamData!.LocalizedParameters!.DefaultLanguage!;
                        NewInputDialog.InputDialogTitleTextBlock.Text = "Enter a new default language identifier (like en-US, de-DE, fr-FR, es-ES...):";
                        NewInputDialog.ConfirmButton.Content = "Confirm";

                        string NewDefaultLanguage = await NewInputDialog.ShowDialog<string>(this);
                        if (!string.IsNullOrEmpty(NewDefaultLanguage))
                        {
                            ParamData.LocalizedParameters.DefaultLanguage = NewDefaultLanguage;
                        }
                    }

                    // Set new title
                    var NewInputDialog2 = new InputDialog() { Title = "Change app title" };
                    NewInputDialog2.NewValueTextBox.Text = ParamData!.LocalizedParameters!.EnUS!.TitleName!;
                    NewInputDialog2.InputDialogTitleTextBlock.Text = "Enter a new title for this app:";
                    NewInputDialog2.ConfirmButton.Content = "Confirm";

                    string NewAppTitle = await NewInputDialog2.ShowDialog<string>(this);
                    if (!string.IsNullOrEmpty(NewAppTitle))
                    {
                        ParamData!.LocalizedParameters!.ArAE!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.CsCZ!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.DaDK!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.DeDE!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.ElGR!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.EnGB!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.EnUS!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.Es419!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.FiFI!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.FrCA!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.FrFR!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.HuHU!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.IdID!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.ItIT!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.JaJP!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.KoKR!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.NoNO!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.NlNL!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.PlPL!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.PtBR!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.PtPT!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.RoRO!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.RuRU!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.SvSE!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.ThTH!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.TrTR!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.ViVN!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.ZhHans!.TitleName = NewAppTitle;
                        ParamData!.LocalizedParameters!.ZhHant!.TitleName = NewAppTitle;

                        // Write back to file
                        string RawDataJSON = JsonConvert.SerializeObject(ParamData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                        File.WriteAllText(Path.Combine(SelectedPS5Game.GameFileOrFolderPath!, "sce_sys", "param.json"), RawDataJSON);

                        // Set new title in GamesListView
                        SelectedPS5Game.GameTitle = NewAppTitle;
                    }

                    box = MessageBoxManager.GetMessageBoxStandard("Info", "App renamed!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                catch (JsonSerializationException)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
    }

    private async void AppChangeIconMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;

            var pngFileFilter = new FileDialogFilter
            {
                Name = "PNG Image File",
                Extensions = ["png"]
            };
            var OFD = new OpenFileDialog() { Title = "Select a new icon0.png image for this app", Filters = { pngFileFilter }, AllowMultiple = false };
            var OFDResult = await OFD.ShowAsync(this);

            if (OFDResult != null && OFDResult.Length > 0)
            {
                if (File.Exists(OFDResult[0]) && !string.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath))
                {
                    // Move new icon to sce_sys
                    File.Copy(OFDResult[0], Path.Combine(SelectedPS5Game.GameFileOrFolderPath, "sce_sys", "icon0.png"), true);

                    // Reload new icon
                    var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(SelectedPS5Game.GameFileOrFolderPath, "sce_sys", "icon0.png"));
                    SelectedPS5Game.GameCoverSource = TempBitmapImage;
                }
            }
        }
    }

    private async void AppChangeBackgroundMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;

            var pngFileFilter = new FileDialogFilter
            {
                Name = "PNG Image File",
                Extensions = ["png"]
            };
            var OFD = new OpenFileDialog() { Title = "Select a new pic0.png background image for this app", Filters = { pngFileFilter }, AllowMultiple = false };
            var OFDResult = await OFD.ShowAsync(this);

            if (OFDResult != null && OFDResult.Length > 0)
            {
                if (File.Exists(OFDResult[0]) && !string.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath))
                {
                    File.Copy(OFDResult[0], Path.Combine(SelectedPS5Game.GameFileOrFolderPath, "sce_sys", "pic0.png"), true);

                    // Set new background
                    var TempBitmapImage = AnyBitmap.FromFile(Path.Combine(SelectedPS5Game.GameFileOrFolderPath, "sce_sys", "pic0.png"));
                    SelectedPS5Game.GameBGSource = TempBitmapImage;
                }
            }
        }
    }

    private async void AppChangeSoundtrackMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;

            var at9FileFilter = new FileDialogFilter
            {
                Name = "at9 Sound File",
                Extensions = ["at9"]
            };
            var OFD = new OpenFileDialog() { Title = "Select a new snd0.at9 soundtrack for this app", Filters = { at9FileFilter }, AllowMultiple = false };

            if (IsSoundPlaying)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Info", "A soundtrack is currently playing. Please stop it before changing any soundtrack.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
            else
            {
                var OFDResult = await OFD.ShowAsync(this);
                if (OFDResult != null && OFDResult.Length > 0)
                {
                    if (File.Exists(OFDResult[0]) && !string.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath))
                    {
                        File.Copy(OFDResult[0], Path.Combine(SelectedPS5Game.GameFileOrFolderPath, "sce_sys", "snd0.at9"), true);
                    }
                }
            }
        }
    }

    private void AppOpenLocationMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;
            if (!string.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath))
            {
                if (Directory.Exists(SelectedPS5Game.GameFileOrFolderPath))
                {
                    Utils.OpenFolder(SelectedPS5Game.GameFileOrFolderPath);
                }
            }
        }
    }

    private async void AppPackAsPKG_Click(object? sender, RoutedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;
            if (!string.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath))
            {

                // Create a GP5 project
                string AppProjectPath = SelectedPS5Game.GameFileOrFolderPath;
                var NewGP5Project = new XDocument(
                    new XElement("psproject", new XAttribute("fmt", "gp5"), new XAttribute("version", "1000"),
                    new XElement("volume",
                    new XElement("volume_type", "prospero_app"),
                    new XElement("package", new XAttribute("passcode", "00000000000000000000000000000000")),
                    new XElement("chunk_info", new XAttribute("chunk_count", "1"), new XAttribute("scenario_count", "1"),
                    new XElement("chunks",
                    new XElement("chunk", new XAttribute("id", "0"), new XAttribute("label", "Chunk #0"))),
                    new XElement("scenarios", new XAttribute("default_id", "0"),
                    new XElement("scenario", new XAttribute("id", "0"), new XAttribute("initial_chunk_count", "1"), new XAttribute("label", "Scenario #0"), new XAttribute("type", "playmode"), "0")))),
                    new XElement("global_exclude"),
                    new XElement("rootdir", new XAttribute("dir_exclude", "about"), new XAttribute("file_exclude", "*.esbak;keystone;*.dds;disc_info.dat;pfs-version.dat;ext_info.dat"), new XAttribute("src_path", AppProjectPath))));

                // Save the GP5 project
                string GP5ProjectPath = "";
                var gp5FileFilter = new FileDialogFilter
                {
                    Name = "GP5 Project File",
                    Extensions = ["gp5"]
                };
                var SFD = new SaveFileDialog() { Title = "Select a save path for the GP5 project", DefaultExtension = ".gp5", Filters = { gp5FileFilter } };
                var SFDResult = await SFD.ShowAsync(this);
                if (SFDResult != null)
                {
                    GP5ProjectPath = SFDResult;
                    NewGP5Project.Save(SFDResult);
                }

                // Show the PKG Builder
                var NewPKGBuilder = new PS5PKGBuilder() { ShowActivated = true };
                NewPKGBuilder.SelectedProjectTextBox.Text = GP5ProjectPath;
                NewPKGBuilder.Show();

                var box = MessageBoxManager.GetMessageBoxStandard("Info", "A GP5 project for " + SelectedPS5Game.GameTitle + " - [" + SelectedPS5Game.GameID + "] has been created." + Environment.NewLine + "Select an output path for the final PKG file and build it.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                await box.ShowWindowAsync();
            }
        }
    }

    #endregion

    private long GetDirSize(string RootFolder)
    {
        var FolderInfo = new DirectoryInfo(RootFolder);
        foreach (var File in FolderInfo.GetFiles())
        {
            if (!File.Name.EndsWith("-merged.pkg")) // skip merged .pkg
            {
                TotalSize += File.Length;
            }
        }
        foreach (var SubFolderInfo in FolderInfo.GetDirectories())
            GetDirSize(SubFolderInfo.FullName);
        return TotalSize;
    }

    private async void GamesListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            // Get values
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            GameTitleTextBlock.Text = SelectedPS5Game.GameTitle;
            GameIDTextBlock.Text = SelectedPS5Game.GameID;
            GameRegionTextBlock.Text = SelectedPS5Game.GameRegion;
            GameContentVersionTextBlock.Text = SelectedPS5Game.GameVersion;
            GameContentIDTextBlock.Text = SelectedPS5Game.GameContentID;
            GameCategoryTextBlock.Text = SelectedPS5Game.GameCategory;
            GameSizeTextBlock.Text = SelectedPS5Game.GameSize;
            GameRequiredFirmwareTextBlock.Text = SelectedPS5Game.GameRequiredFirmware;
            GameMasterVersionTextBlock.Text = SelectedPS5Game.GameMasterVersion;
            GameSDKVersionTextBlock.Text = SelectedPS5Game.GameSDKVersion;
            GamePubToolVersionTextBlock.Text = SelectedPS5Game.GamePubToolVersion;

            // Show values if hidden
            if (GameTitleTextBlock.IsVisible == false)
            {
                GameTitleTextBlock.IsVisible = true;
                GameIDTextBlock.IsVisible = true;
                GameContentIDTextBlock.IsVisible = true;
                GameRegionTextBlock.IsVisible = true;
                GameContentVersionTextBlock.IsVisible = true;
                GameMasterVersionTextBlock.IsVisible = true;
                GameSDKVersionTextBlock.IsVisible = true;
                GamePubToolVersionTextBlock.IsVisible = true;
                GameCategoryTextBlock.IsVisible = true;
                GameSizeTextBlock.IsVisible = true;
                GameRequiredFirmwareTextBlock.IsVisible = true;
                GameBackupFolderNameTextBlock.IsVisible = true;
            }

            if (SelectedPS5Game.GameVersionFileURI != null)
            {
                GameVersionFileURITextBlock.Text = "Update URL: " + Utils.GetFilenameFromUrl(new Uri(SelectedPS5Game.GameVersionFileURI)).Replace("-version.xml", "").Trim();
            }

            // Set backup folder name or path
            if (!string.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath) && SelectedPS5Game.GameLocation == PS5Game.Location.Local)
            {
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " + new DirectoryInfo(Path.GetDirectoryName(SelectedPS5Game.GameFileOrFolderPath)!).Name;
            }
            else
            {
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " + SelectedPS5Game.GameFileOrFolderPath;
            }

            // Show background
            if (SelectedPS5Game.GameBGSource is not null)
            {
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        if (BlurringShape.Fill is ImageBrush RectangleImageBrush)
                        {
                            using MemoryStream memory = new();
                            SelectedPS5Game.GameBGSource.ExportStream(memory);
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
                        SelectedPS5Game.GameBGSource.ExportStream(memory);
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
                GamePlayMenuItem.Header = "Play Soundtrack";
                GamePlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/play.png"))) };
            }
            if (SelectedPS5Game.GameSoundFile is not null)
            {
                if (AutoPlay)
                {
                    Utils.PlayGameSoundFile(SelectedPS5Game.GameSoundFile);
                    IsSoundPlaying = true;
                    GamePlayMenuItem.Header = "Stop Soundtrack";
                    GamePlayMenuItem.Icon = new Image() { Width = 16, Height = 16, Source = new Bitmap(AssetLoader.Open(new Uri("avares://PSMultiTools/Images/stop.png"))) };
                }
            }

            // Show values
            GameTitleTextBlock.IsVisible = true;
            GameIDTextBlock.IsVisible = true;
            GameRegionTextBlock.IsVisible = true;
            GameContentVersionTextBlock.IsVisible = true;
            GameContentIDTextBlock.IsVisible = true;
            GameCategoryTextBlock.IsVisible = true;
            GameSizeTextBlock.IsVisible = true;
            GameRequiredFirmwareTextBlock.IsVisible = true;
            GameBackupFolderNameTextBlock.IsVisible = true;
            GameMasterVersionTextBlock.IsVisible = true;
            GameSDKVersionTextBlock.IsVisible = true;
            GamePubToolVersionTextBlock.IsVisible = true;
            GameVersionFileURITextBlock.IsVisible = true;
        }
        else
        {
            // Hide values
            GameTitleTextBlock.IsVisible = false;
            GameIDTextBlock.IsVisible = false;
            GameRegionTextBlock.IsVisible = false;
            GameContentVersionTextBlock.IsVisible = false;
            GameContentIDTextBlock.IsVisible = false;
            GameCategoryTextBlock.IsVisible = false;
            GameSizeTextBlock.IsVisible = false;
            GameRequiredFirmwareTextBlock.IsVisible = false;
            GameBackupFolderNameTextBlock.IsVisible = false;
            GameMasterVersionTextBlock.IsVisible = false;
            GameSDKVersionTextBlock.IsVisible = false;
            GamePubToolVersionTextBlock.IsVisible = false;
            GameVersionFileURITextBlock.IsVisible = false;
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

    private async void ContentWebView_LoadEnd(object sender, Xilium.CefGlue.Common.Events.LoadEndEventArgs e)
    {
        if (e.HttpStatusCode == 200)
        {
            string GameCoverSource = string.Empty;
            if (ContentWebView.Address.StartsWith("https://prosperopatches.com/"))
            {
                // Game ID
                string GameID = await ContentWebView.EvaluateJavaScript<string>("return document.getElementsByClassName('bd-links-group py-2')[0].innerText");

                // Game Image
                string GameImageURL = await ContentWebView.EvaluateJavaScript<string>("return document.getElementsByClassName('game-icon secondary')[0].outerHTML");
                string[] SplittedGameImageURL = GameImageURL.Split(["(", ")"], StringSplitOptions.None);

                if (SplittedGameImageURL.Length > 0 & !string.IsNullOrEmpty(GameID))
                {
                    GameID = GameID.Split("\n")[1].Trim();
                    GameCoverSource = SplittedGameImageURL[1];
                }

                if (!string.IsNullOrEmpty(GameCoverSource) && !string.IsNullOrEmpty(GameID))
                {
                    foreach (var ItemInListView in GamesListBox.Items)
                    {
                        if (ItemInListView is PS5Game FoundGame)
                        {
                            if (FoundGame.GameID!.Contains(GameID!) | (FoundGame.GameID ?? "") == (GameID ?? ""))
                            {
                                await Dispatcher.UIThread.Invoke(async () => FoundGame.GameCoverSource = await AnyBitmap.FromUriAsync(new Uri(GameCoverSource)));
                                break;
                            }
                        }
                    }

                    foreach (var GamePatch in PatchesListBox.Items)
                    {
                        PS5Game FoundGamePatch = (PS5Game)GamePatch!;

                        if (FoundGamePatch.GameID!.Contains(GameID!) | (FoundGamePatch.GameID ?? "") == (GameID ?? ""))
                        {
                            await Dispatcher.UIThread.Invoke(async () => FoundGamePatch.GameCoverSource = await AnyBitmap.FromUriAsync(new Uri(GameCoverSource)));
                            break;
                        }
                    }
                }

                if (CurrentURL < URLs.Count)
                {
                    ContentWebView.Address = URLs[CurrentURL];
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
    }

    private void AppsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (AppsListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)AppsListBox.SelectedItem;

            if (SelectedPS5Game.GameBackupType == "Folder")
            {
                AppPackAsPKG.IsVisible = true;
            }
        }
        else
        {
            AppPackAsPKG.IsVisible = false;
        }
    }

    private void GameVersionFileURITextBlock_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (GamesListBox.SelectedItem is not null)
        {
            PS5Game SelectedPS5Game = (PS5Game)GamesListBox.SelectedItem;
            var NewProcessStartInfo = new ProcessStartInfo() { FileName = SelectedPS5Game.GameVersionFileURI, UseShellExecute = true };
            Process.Start(NewProcessStartInfo);
            e.Handled = true;
        }
    }

    public static async Task<List<string>> FindAllFilesAsync(string SelectedRootDirectory, bool CollectFilesRecursively = false, int ParallelTasks = 8, CancellationToken CTS = default)
    {
        if (string.IsNullOrWhiteSpace(SelectedRootDirectory)) throw new ArgumentException(null, nameof(SelectedRootDirectory));
        if (!Directory.Exists(SelectedRootDirectory)) return [];

        var FoundParamFiles = new ConcurrentBag<string>();

        IEnumerable<string> RootTopDirectories;
        try
        {
            RootTopDirectories = Directory.EnumerateDirectories(SelectedRootDirectory, "*", SearchOption.TopDirectoryOnly);
        }
        catch
        {
            return [];
        }

        var NewParallelOptions = new ParallelOptions
        {
            CancellationToken = CTS,
            MaxDegreeOfParallelism = Math.Max(1, ParallelTasks)
        };

        await Task.Run(() =>
        {
            Parallel.ForEach(RootTopDirectories, NewParallelOptions, topDir =>
            {
                NewParallelOptions.CancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var SCESYSFolder = Path.Combine(topDir, "sce_sys");
                    if (!Directory.Exists(SCESYSFolder)) return;

                    EnumerateMatchingFiles(SCESYSFolder, "param.json", CollectFilesRecursively, FoundParamFiles, NewParallelOptions.CancellationToken);
                }
                catch (OperationCanceledException) { throw; }
                catch
                {
                }
            });
        }, CTS);

        return [.. FoundParamFiles];
    }

    private static void EnumerateMatchingFiles(string SearchInFolder, string FilePattern, bool SearchRecursive, ConcurrentBag<string> OutConcurrentBag, CancellationToken CTS)
    {
        try
        {
            var EnumOptions = SearchRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var FoundParamFile in Directory.EnumerateFiles(SearchInFolder, FilePattern, EnumOptions))
            {
                CTS.ThrowIfCancellationRequested();
                OutConcurrentBag.Add(FoundParamFile);
            }
        }
        catch (UnauthorizedAccessException) { }
        catch (PathTooLongException) { }
        catch (DirectoryNotFoundException) { }
    }

    private static string? GetAlternativeGameTitle(PS5Param ParamFile)
    {
        var Languages = ParamFile.LocalizedParameters;
        if (Languages == null) return null;

        if (!string.IsNullOrWhiteSpace(Languages.JaJP!.TitleName)) return Languages.JaJP.TitleName;
        if (!string.IsNullOrWhiteSpace(Languages.KoKR!.TitleName)) return Languages.KoKR.TitleName;
        if (!string.IsNullOrWhiteSpace(Languages.ZhHant!.TitleName)) return Languages.ZhHant.TitleName;
        if (!string.IsNullOrWhiteSpace(Languages.ZhHans!.TitleName)) return Languages.ZhHans.TitleName;
        if (!string.IsNullOrWhiteSpace(Languages.ArAE!.TitleName)) return Languages.ArAE.TitleName;

        return null;
    }

    private void ExitMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

}