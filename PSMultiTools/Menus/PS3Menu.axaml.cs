using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using IniParser;
using IniParser.Model;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using PSMultiTools.Dialogs;
using PSMultiTools.MultiPlatformTools;
using PSMultiTools.PS3;
using PSMultiTools.PS3.Tools;
using System;
using System.Diagnostics;
using System.IO;
using Xilium.CefGlue.Avalonia;

namespace PSMultiTools.Menus;

public partial class PS3Menu : UserControl
{

    private readonly AvaloniaCefBrowser WebMANWebView = new() { Address = "about:blank" };

    private bool IswebMANMODWebViewReady = false;
    private bool IswebMANMODCommandExecuted = false;

    public string SharedConsoleAddress = "";

    public PS3Menu()
    {
        InitializeComponent();

        Loaded += PS3Menu_Loaded;

        FTPIPTextBox.TextChanged += IPTextBox_TextChanged;

        // WebMANWebView setup
        WebMANWebView.BrowserInitialized += WebMANWebView_BrowserInitialized;
        WebMANWebView.LoadingStateChange += WebMANWebView_LoadingStateChange;
        var browserWrapper = this.FindControl<Decorator>("WebMANWebViewWrapper");
        browserWrapper!.Child = WebMANWebView;
    }

    private void PS3Menu_Loaded(object? sender, RoutedEventArgs e)
    {
        // Load config if exists
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini")))
        {
            try
            {
                var PSMTConfigParser = new FileIniDataParser();
                IniData PSMTConfigData = PSMTConfigParser.ReadFile(Path.Combine(Environment.CurrentDirectory, "psmt-config.ini"));

                SharedConsoleAddress = PSMTConfigData["PS3 Tools"]["IP"];
                FTPIPTextBox.Text = PSMTConfigData["PS3 Tools"]["IP"];
            }
            catch (Exception)
            {
            }
        }
    }

    #region Menu TextBox Changed Events

    public static readonly RoutedEvent<RoutedEventArgs> IPChangedEvent = RoutedEvent.Register<PS3Menu, RoutedEventArgs>(nameof(IPChanged), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs> IPChanged
    {
        add => AddHandler(IPChangedEvent, value);
        remove => RemoveHandler(IPChangedEvent, value);
    }

    private void RaiseIPChanged() => RaiseEvent(new RoutedEventArgs(IPChangedEvent, this));

    private void IPTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrEmpty(FTPIPTextBox.Text))
        {
            SharedConsoleAddress = FTPIPTextBox.Text;
            RaiseIPChanged();
        }
    }

    #endregion

    #region Tools

    private void BatchRenameMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewBatchRename = new BatchRename() { ShowActivated = true };
        NewBatchRename.Show();
    }

    private void OpenSFOEditor_Click(object? sender, RoutedEventArgs e)
    {
        var NewSFOEditor = new SFOEditor() { ShowActivated = true };
        NewSFOEditor.Show();
    }

    private void OpenISOTools_Click(object? sender, RoutedEventArgs e)
    {
        var NewISOTools = new PS3ISOTools() { ShowActivated = true };
        NewISOTools.Show();
    }

    private void OpenCoreOSTools_Click(object? sender, RoutedEventArgs e)
    {
        var NewCoreOSTools = new PS3CoreOSTools() { ShowActivated = true };
        NewCoreOSTools.Show();
    }

    private void OpenFixTar_Click(object? sender, RoutedEventArgs e)
    {
        var NewFixTar = new PS3FixTar() { ShowActivated = true };
        NewFixTar.Show();
    }

    private void OpenPUPUnpacker_Click(object? sender, RoutedEventArgs e)
    {
        var NewPUPUnpacker = new PS3PUPUnpacker() { ShowActivated = true };
        NewPUPUnpacker.Show();
    }

    private void OpenRCODumper_Click(object? sender, RoutedEventArgs e)
    {
        var NewRCODumper = new PS3RCODumper() { ShowActivated = true };
        NewRCODumper.Show();
    }

    private void OpenSELFReader_Click(object? sender, RoutedEventArgs e)
    {
        var NewSELFReader = new PS3ReadSELF() { ShowActivated = true };
        NewSELFReader.Show();
    }

    private void OpenFTPBrowser_Click(object? sender, RoutedEventArgs e)
    {
        var NewFTPBrowser = new FTPBrowser() { ShowActivated = true };
        NewFTPBrowser.Show();
    }

    private void OpenPKGExtractor_Click(object? sender, RoutedEventArgs e)
    {
        var NewPKGExtractor = new PS3PKGExtractor() { ShowActivated = true };
        NewPKGExtractor.Show();
    }

    private async void CheckForUpdatesMenuItems_Click(object? sender, RoutedEventArgs e)
    {
        if (await Utils.IsPSMultiToolsUpdateAvailable())
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Update found", "An update is available, do you want to download it now ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box.ShowWindowAsync();
            if (boxresult == ButtonResult.Yes)
            {
                Utils.DownloadAndExecuteUpdater();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("No update found", "PS Multi Tools is up to date!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    #endregion

    #region Menu Downloads

    #region Homebrew

    private async void DownloadAdvancedPowerOptions_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/Advanced_Power_Options_v1.11.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadAdvancedTools_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/PS3AdvancedTools_v1.0.1.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadApollo_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/bucanero/apollo-ps3/releases/download/v2.2.4/apollo-ps3.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadApolloGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/bucanero/apollo-ps3/releases/latest/download/apollo-ps3.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadArtemis_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/ArtemisPS3-GUI-r6.3..pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadArtemisGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/bucanero/ArtemisPS3/releases/latest/download/ArtemisPS3-GUI.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadAwesomeMPManager_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/Awesome_MountPoint_Manager_1.1a.AllCFW.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadCCAPI_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/CCAPI_v2.80_Rev10.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadComgenieGeohot_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/ComgenieAwesomeFilemanager355.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadComgenieNew_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/ComgenieAwesomeFilemanager421.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadComgenieOld_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/ComgenieAwesomeFilemanager.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadIrisman_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/IRISMAN_4.90.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadIrismanGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/aldostools/IRISMAN/releases/download/4.90/IRISMAN_4.90.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadManagunzBM_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/ManaGunZ_v1.41.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadManagunzFM_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/ManaGunZ_FileManager_v1.41.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadMovian_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/movianM7-7.0.231-playstation3.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadMultiMAN_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/multiMAN_04.85.01_BASE_(20191010).pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPKGi_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/pkgi-ps3.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPKGiGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/bucanero/pkgi-ps3/releases/latest/download/pkgi-ps3.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadReact_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/reActPSN_v3.20+.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRebugToolbox_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/REBUG_TOOLBOX_02.03.06.MULTI.16.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSENEnabler_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/SEN_Enabler_v6.2.7_[CEX-DEX]_[4.87].pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadUltimateToolbox_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/Ultimate_Toolbox_v2.03_FULL_version.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadUnlockHDDSpace_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/hb/Unlock_HDD_Space.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #region webMAN MOD Downloads

    private async void DownloadCoversPackPS3_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/1.0/EP0001-BLES80608_00-COVERS0000000000.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadCoversPackPSXPS2_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/1.0/EP0001-BLES80608_00-COVERS00000RETRO.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadMultiMANMod_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/multiMAN/multiMAN_MOD_based_mmCM_4.85.01.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadNetsrv_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/aldostools/webMAN-MOD/releases/download/1.47.48/ps3netsrv_20250501.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPrepIso_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/aldostools/webMAN-MOD/releases/download/1.47.48/prepISO_1.33.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPS2ClassicsLauncher_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/aldostools/webMAN-MOD/releases/download/1.47.48/PS2_Classics_Launcher.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPS2Config_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/aldostools/webMAN-MOD/releases/download/1.47.48/PS2CONFIG.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPSPMinisLauncher_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/aldostools/webMAN-MOD/releases/download/1.47.48/PSP_Minis_Launcher.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPSPRemastersLauncher_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/aldostools/webMAN-MOD/releases/download/1.47.48/PSP_Remasters_Launcher.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadWebManMod_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/aldostools/webMAN-MOD/releases/download/1.47.48/webMAN_MOD_1.47.48_Installer.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadColorfulWMTheme_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/Themes/wm_theme_colorful.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadFlowerificationWMTheme_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/Themes/wm_theme_flowerification.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadMetalificationWMTheme_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/Themes/wm_theme_metalification.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRebugificationWMTheme_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/Themes/wm_theme_rebugification.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRetroArchWebMANMod_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/RetroArch_CE/RetroArch_CE.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadStandardWMTheme_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/aldostools/Resources/releases/download/Themes/wm_theme_standard.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #endregion

    #region Firmwares

    #region Custom

    #region Classic

    private async void Download355DexDowngrader_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/PS3-CFW-3.55-DEX-DOWNGRADER_PS3UPDAT.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadCobra355_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/Cobra%203.55%20CFW/PS3UPDAT.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadGeoHot_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/GeoHot%203.55%20CFW/PS3UPDAT.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadKmeaw_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/Kmeaw%203.55%20CFW/PS3UPDAT.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadMiralaTijera_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/MiralaTijera%203.55%20CFW/PS3UPDAT.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadOTHEROSColdBoot_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/OTHEROS++%20COLD-BOOT%203.55%20CFW/PS3UPDAT.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadOTHEROSSpecial_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/OTHEROS++%20SPECIAL%203.55%20CFW/PS3UPDAT.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPS3ITA_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/PS3ITA%203.55%20CFW%20v1.1/PS3UPDAT.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPS3ULTIMATE_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/PS3ULTIMATE%203.55%20CFW/PS3UPDAT.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRebugRex_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/REBUG%20REX%20EDITION%203.55.4%20CFW/PS3UPDAT.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRogero_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/Rogero%20v3.7%203.55%20CFW/PS3UPDAT.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadWaninkoko_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/Waninkoko%203.55%20CFW%20v2/PS3UPDAT.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadWutangrza_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/3.55/Wutangrza%203.55%20CFW/PS3UPDAT.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #region Current

    private async void DownloadREBUGDRex484_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.84/REBUG%20D-REX%20EDITION%204.84.2%20CFW.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadREBUGRex484_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.84/REBUG%20REX%20EDITION%204.84.2%20CFW.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadEvilnatCobra490Cex_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.90/CFW%204.90%20Evilnat%20Cobra%208.4%20[CEX].rar") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadEvilnatCobra490Dex_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.90/CFW%204.90%20Evilnat%20Cobra%208.4%20[DEX].rar") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadEvilnatCobra492PEX_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.92/CFW%204.92%20Evilnat%20Cobra%208.5%20%5BPEX%5D.rar") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadEvilnatCobra492PEXNoBD_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.92/CFW%204.92%20Evilnat%20Cobra%208.5%20%5BPEX%5D%20%5BnoBD%5D.rar") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadEvilnatCobra492PEXNoBDNoBT_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.92/CFW%204.92%20Evilnat%20Cobra%208.5%20%5BPEX%5D%20%5BnoBD%2BnoBT%5D.rar") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadEvilnatCobra492PEXNoBT_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.92/CFW%204.92%20Evilnat%20Cobra%208.5%20%5BPEX%5D%20%5BnoBT%5D.rar") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadEvilnatCobra492PEXOC_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/cfw/4.92/CFW%204.92%20Evilnat%20Cobra%208.5%20%5BPEX%5D%20%5BOC%5D.rar") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #endregion

    #region Official

    private async void DownloadOFW102_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/ofw/1.02/PS3UPDAT.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadOFW315_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/ofw/3.15/PS3UPDAT.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadOFW355_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/ofw/3.55/PS3UPDAT.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadOFW492_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/fw/ofw/4.92/PS3UPDAT.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #endregion

    #region Emulators

    private async void DownloadRetroArchCommunity_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/emu/RetroArch_Psx-Place_Community_Edition_unofficial_beta-20220315.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    private async void DownloadDKeyFiles_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/DKEY.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadKeyFiles_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps3/KEY.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #region webMAN MOD Tools

    public void NavigateTowebMANMODUrl(string InputURL)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = InputURL;
            }
        }
    }

    private void EjectDisc_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = "http://" + SharedConsoleAddress + "/eject.ps3";
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void ExitToXMB_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = "http://" + SharedConsoleAddress + "/xmb.ps3$exit";
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private async void DownloadPKGFromURLToPS3_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                var NewInputDialog = new InputDialog() { Title = "Download a PKG to the PS3" };
                NewInputDialog.NewValueTextBox.Text = "";
                NewInputDialog.InputDialogTitleTextBlock.Text = "Enter the PKG URL :";
                NewInputDialog.ConfirmButton.Content = "Download";

                var AppLifetime = (Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current!.ApplicationLifetime!;
                foreach (Window OpenWin in AppLifetime.Windows)
                {
                    if (OpenWin is PS3Library PS3LibraryWindow)
                    {
                        var InputDialogResult = await NewInputDialog.ShowDialog<string>(PS3LibraryWindow);

                        if (InputDialogResult != null)
                        {
                            IswebMANMODCommandExecuted = false;
                            WebMANWebView.Address = "http://" + SharedConsoleAddress + "/xmb.ps3/download.ps3?url=" + InputDialogResult;
                        }
                        break;
                    }
                }
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private async void DownloadFileFromURLToPS3_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                var NewURLInputDialog = new InputDialog() { Title = "Download a file to the PS3" };
                NewURLInputDialog.NewValueTextBox.Text = "";
                NewURLInputDialog.InputDialogTitleTextBlock.Text = "Enter the file URL :";
                NewURLInputDialog.ConfirmButton.Content = "Download";

                var NewDestinationInputDialog = new InputDialog() { Title = "Download Destination" };
                NewDestinationInputDialog.NewValueTextBox.Text = "/dev_hdd0/FOLDER/FILENAME.EXTENSION";
                NewDestinationInputDialog.InputDialogTitleTextBlock.Text = "Enter the destination path :";
                NewDestinationInputDialog.ConfirmButton.Content = "Confirm";

                var AppLifetime = (Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current!.ApplicationLifetime!;
                foreach (Window OpenWin in AppLifetime.Windows)
                {
                    if (OpenWin is PS3Library PS3LibraryWindow)
                    {
                        var NewURLInputDialogResult = await NewURLInputDialog.ShowDialog<string>(PS3LibraryWindow);
                        var NewDestinationInputDialogResult = await NewDestinationInputDialog.ShowDialog<string>(PS3LibraryWindow);

                        if (NewURLInputDialogResult != null && NewDestinationInputDialogResult != null)
                        {
                            IswebMANMODCommandExecuted = false;
                            WebMANWebView.Address = "http://" + SharedConsoleAddress + "/xmb.ps3/download.ps3?to=" + NewDestinationInputDialogResult + "&url=" + NewURLInputDialogResult;
                        }
                        break;
                    }
                }
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private async void DownloadAndInstallPKGFromURL_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                var NewInputDialog = new InputDialog() { Title = "Download & Install a PKG on the PS3" };
                NewInputDialog.NewValueTextBox.Text = "";
                NewInputDialog.InputDialogTitleTextBlock.Text = "Enter the PKG URL :";
                NewInputDialog.ConfirmButton.Content = "Install";

                var AppLifetime = (Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current!.ApplicationLifetime!;
                foreach (Window OpenWin in AppLifetime.Windows)
                {
                    if (OpenWin is PS3Library PS3LibraryWindow)
                    {
                        var InputDialogResult = await NewInputDialog.ShowDialog<string>(PS3LibraryWindow);

                        if (InputDialogResult != null)
                        {
                            IswebMANMODCommandExecuted = false;
                            WebMANWebView.Address = "http://" + SharedConsoleAddress + "/xmb.ps3/install.ps3?url=" + InputDialogResult;
                        }
                        break;
                    }
                }
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void HardRebootPS3_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = "http://" + SharedConsoleAddress + "/reboot.ps3";
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void InsertDisc_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = "http://" + SharedConsoleAddress + "/insert.ps3";
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private async void InstallPKGFromPS3HDD_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                var NewInputDialog = new InputDialog() { Title = "Install a PKG" };
                NewInputDialog.NewValueTextBox.Text = "/dev_hdd0/packages/Homebrew.pkg";
                NewInputDialog.InputDialogTitleTextBlock.Text = "Enter the full path to the .pkg file:";
                NewInputDialog.ConfirmButton.Content = "Install";

                var AppLifetime = (Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current!.ApplicationLifetime!;
                foreach (Window OpenWin in AppLifetime.Windows)
                {
                    if (OpenWin is PS3Library PS3LibraryWindow)
                    {
                        var InputDialogResult = await NewInputDialog.ShowDialog<string>(PS3LibraryWindow);

                        if (InputDialogResult != null)
                        {
                            IswebMANMODCommandExecuted = false;
                            WebMANWebView.Address = "http://" + SharedConsoleAddress + "/install_ps3" + InputDialogResult;
                        }
                        break;
                    }
                }
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private async void InstallThemeFromPS3HDD_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                var NewInputDialog = new InputDialog() { Title = "Install a Theme" };
                NewInputDialog.NewValueTextBox.Text = "/dev_hdd0/Themes/THEME.p3t";
                NewInputDialog.InputDialogTitleTextBlock.Text = "Enter the full path to the .p3t file:";
                NewInputDialog.ConfirmButton.Content = "Install";

                var AppLifetime = (Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current!.ApplicationLifetime!;
                foreach (Window OpenWin in AppLifetime.Windows)
                {
                    if (OpenWin is PS3Library PS3LibraryWindow)
                    {
                        var p3tPath = await NewInputDialog.ShowDialog<string>(PS3LibraryWindow);
                        if (p3tPath != null)
                        {
                            IswebMANMODCommandExecuted = false;
                            WebMANWebView.Address = "http://" + SharedConsoleAddress + "/install.ps3" + p3tPath;
                        }
                        break;
                    }
                }
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private async void OpenPS3WebBrowserURL_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                var NewInputDialog = new InputDialog() { Title = "Open PS3 Web Browser with URL" };
                NewInputDialog.NewValueTextBox.Text = "";
                NewInputDialog.InputDialogTitleTextBlock.Text = "Enter an URL to browse :";
                NewInputDialog.ConfirmButton.Content = "Open";

                var AppLifetime = (Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current!.ApplicationLifetime!;
                foreach (Window OpenWin in AppLifetime.Windows)
                {
                    if (OpenWin is PS3Library PS3LibraryWindow)
                    {
                        var UrlPath = await NewInputDialog.ShowDialog<string>(PS3LibraryWindow);
                        if (UrlPath != null)
                        {
                            IswebMANMODCommandExecuted = false;
                            WebMANWebView.Address = "http://" + SharedConsoleAddress + "/browser.ps3?" + UrlPath;
                        }
                        break;
                    }
                }
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void OpenWebGUICelcius_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            var NewwebMANMODWebGUI = new PS3webMANBrowser() { ShowActivated = true, WebMANAddress = "http://" + SharedConsoleAddress + "/tempc.html" };
            NewwebMANMODWebGUI.Show();
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void OpenWebGUIFahrenheit_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            var NewwebMANMODWebGUI = new PS3webMANBrowser() { ShowActivated = true, WebMANAddress = "http://" + SharedConsoleAddress + "/tempf.html" };
            NewwebMANMODWebGUI.Show();
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void OpenwebMANMODWebGUI_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            var NewwebMANMODWebGUI = new PS3webMANBrowser() { ShowActivated = true, WebMANAddress = "http://" + SharedConsoleAddress };
            NewwebMANMODWebGUI.Show();
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void PlayDiscMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = "http://" + SharedConsoleAddress + "/play.ps3";
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void QuickRebootPS3_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = "http://" + SharedConsoleAddress + "/reboot.ps3?quick";
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void RebootPS3UsingVSH_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = "http://" + SharedConsoleAddress + "/reboot.ps3?vsh";
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void ReloadGame_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = "http://" + SharedConsoleAddress + "/xmb.ps3$reloadgame";
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void RescanGamesRefreshXML_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = "http://" + SharedConsoleAddress + "/refresh.ps3?xmb";
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void RestartPS3_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = "http://" + SharedConsoleAddress + "/restart.ps3";
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void RestartPS3ShowMinVersion_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = "http://" + SharedConsoleAddress + "/restart.ps3?min";
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void RestartPS3WithContentScan_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = "http://" + SharedConsoleAddress + "/restart.ps3?0";
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void ShowSystemInfoOnPS3_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = "http://" + SharedConsoleAddress + "/popup.ps3";
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void ShutdownPS3_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = "http://" + SharedConsoleAddress + "/shutdown.ps3";
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void SoftRebootPS3_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = "http://" + SharedConsoleAddress + "/reboot.ps3?soft";
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void ToggleInGameBGMusicPlayback_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = "http://" + SharedConsoleAddress + "/sysbgm.ps3";
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void ToggleVideoRecording_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SharedConsoleAddress))
        {
            if (IswebMANMODWebViewReady && IswebMANMODCommandExecuted)
            {
                IswebMANMODCommandExecuted = false;
                WebMANWebView.Address = "http://" + SharedConsoleAddress + "/videorec.ps3";
            }
            else
            {
                ShowWebMANReadyMessage();
            }
        }
        else
        {
            ShowMissingIPMessage();
        }
    }

    private void WebMANWebView_BrowserInitialized()
    {
        IswebMANMODWebViewReady = true;
    }

    private async void WebMANWebView_LoadingStateChange(object sender, Xilium.CefGlue.Common.Events.LoadingStateChangeEventArgs e)
    {
        if (!e.IsLoading)
        {
            IswebMANMODCommandExecuted = true;
        }
    }

    private async void CreateFolderStructure_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a destination path" };

        var AppLifetime = (Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current!.ApplicationLifetime!;
        foreach (Window OpenWin in AppLifetime.Windows)
        {
            if (OpenWin is PS3Library PS3LibraryWindow)
            {
                var FBDResult = await FBD.ShowAsync(PS3LibraryWindow);
                if (FBDResult != null)
                {
                    Directory.CreateDirectory(Path.Combine(FBDResult, "GAMES"));
                    Directory.CreateDirectory(Path.Combine(FBDResult, "PS3ISO"));
                    Directory.CreateDirectory(Path.Combine(FBDResult, "PSXISO"));
                    Directory.CreateDirectory(Path.Combine(FBDResult, "PS2ISO"));
                    Directory.CreateDirectory(Path.Combine(FBDResult, "PSPISO"));
                    Directory.CreateDirectory(Path.Combine(FBDResult, "BDISO"));
                    Directory.CreateDirectory(Path.Combine(FBDResult, "DVDISO"));
                    Directory.CreateDirectory(Path.Combine(FBDResult, "ROMS"));
                    Directory.CreateDirectory(Path.Combine(FBDResult, "GAMEI"));
                    Directory.CreateDirectory(Path.Combine(FBDResult, "PKG"));
                    Directory.CreateDirectory(Path.Combine(FBDResult, "MOVIES"));
                    Directory.CreateDirectory(Path.Combine(FBDResult, "MUSIC"));
                    Directory.CreateDirectory(Path.Combine(FBDResult, "PICTURE"));
                    Directory.CreateDirectory(Path.Combine(FBDResult, "REDKEY"));

                    var box = MessageBoxManager.GetMessageBoxStandard("Info", "Directories created!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                break;
            }
        }
    }

    private void ManageVirtualFolders_Click(object? sender, RoutedEventArgs e)
    {
        var NewVirtualFolderManager = new PS3VirtualFolderManager() { ShowActivated = true };
        NewVirtualFolderManager.Show();
    }

    private async void ShareASingleFolder_Click(object? sender, RoutedEventArgs e)
    {
        switch (ShareASingleFolder.Header!.ToString() ?? "")
        {
            case "Share a single folder":
                {
                    if (File.Exists(OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "ps3netsrv.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "ps3netsrv")))
                    {
                        var FBD = new OpenFolderDialog() { Title = "Select the folder you want to share" };
                        var AppLifetime = (Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current!.ApplicationLifetime!;
                        foreach (Window OpenWin in AppLifetime.Windows)
                        {
                            if (OpenWin is PS3Library PS3LibraryWindow)
                            {
                                var FBDResult = await FBD.ShowAsync(PS3LibraryWindow);
                                if (FBDResult != null)
                                {
                                    var box = MessageBoxManager.GetMessageBoxStandard("Please confirm sharing the selected folder", FBDResult + " will be shared using ps3netsrv. Continue ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                    var boxresult = await box.ShowWindowAsync();
                                    if (boxresult == ButtonResult.Yes)
                                    {
                                        string NewArgs = "";
                                        if (FBDResult.Length <= 3)
                                        {
                                            NewArgs = FBDResult + @"\";
                                        }
                                        else
                                        {
                                            NewArgs = "\"" + FBDResult + "\"";
                                        }

                                        Process PS3NetSrvProcess = new()
                                        {
                                            EnableRaisingEvents = true,
                                            StartInfo = new ProcessStartInfo()
                                            {
                                                Arguments = NewArgs,
                                                FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "ps3netsrv.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "ps3netsrv")
                                            }
                                        };

                                        PS3NetSrvProcess.Exited += (s, e) =>
                                        {
                                            PS3NetSrvProcess.Dispose();

                                            if (Dispatcher.UIThread.CheckAccess() == false)
                                            {
                                                Dispatcher.UIThread.Invoke(() =>
                                                {
                                                    ShareASingleFolder.Header = "Share a single folder";
                                                    ShareManagedFolders.Header = "Share configured managed virtual folders";
                                                });
                                            }
                                            else
                                            {
                                                ShareASingleFolder.Header = "Share a single folder";
                                                ShareManagedFolders.Header = "Share configured managed virtual folders";
                                            }
                                        };

                                        PS3NetSrvProcess.Start();

                                        if (Dispatcher.UIThread.CheckAccess() == false)
                                        {
                                            Dispatcher.UIThread.Invoke(() => ShareASingleFolder.Header = "Stop sharing");
                                        }
                                        else
                                        {
                                            ShareASingleFolder.Header = "Stop sharing";
                                        }
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Cannot share without ps3netsrv", "Could not find ps3netsrv", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                    }

                    break;
                }
            case "Stop sharing":
                {
                    // Stop ps3netsrv
                    foreach (var p in Process.GetProcessesByName("ps3netsrv"))
                    {
                        try
                        {
                            if (!p.CloseMainWindow())
                            {
                                p.Kill();
                            }
                            p.WaitForExit(2500);
                        }
                        catch { }
                        finally
                        {
                            p.Dispose();
                        }
                    }

                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => ShareASingleFolder.Header = "Share a single folder");
                    }
                    else
                    {
                        ShareASingleFolder.Header = "Share a single folder";
                    }

                    break;
                }
        }
    }

    private async void ShareManagedFolders_Click(object? sender, RoutedEventArgs e)
    {
        switch (ShareManagedFolders.Header!.ToString() ?? "")
        {
            case "Share configured managed virtual folders":
                {
                    if (File.Exists(OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "ps3netsrv.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "ps3netsrv")))
                    {
                        Directory.SetCurrentDirectory(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv"));

                        Process PS3NetSrvProcess = new() { EnableRaisingEvents = true, StartInfo = new ProcessStartInfo() { Arguments = ".", FileName = OperatingSystem.IsWindows() ? "ps3netsrv.exe" : "ps3netsrv" } };
                        PS3NetSrvProcess.Start();

                        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() => ShareManagedFolders.Header = "Stop sharing");
                        }
                        else
                        {
                            ShareManagedFolders.Header = "Stop sharing";
                        }
                    }
                    else
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Cannot share without ps3netsrv", "Could not find ps3netsrv", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                    }

                    break;
                }
            case "Stop sharing":
                {
                    // Stop ps3netsrv
                    foreach (var p in Process.GetProcessesByName("ps3netsrv"))
                    {
                        try
                        {
                            if (!p.CloseMainWindow())
                            {
                                p.Kill();
                            }
                            p.WaitForExit(2500);
                        }
                        catch { }
                        finally
                        {
                            p.Dispose();
                        }
                    }

                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => ShareASingleFolder.Header = "Share a single folder");
                    }
                    else
                    {
                        ShareASingleFolder.Header = "Share a single folder";
                    }

                    break;
                }
        }
    }

    private static async void ShowWebMANReadyMessage()
    {
        var box = MessageBoxManager.GetMessageBoxStandard("webMAN MOD not ready", "Please wait a couple seconds until WebView2 is ready.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
        await box.ShowWindowAsync();
    }

    private static async void ShowMissingIPMessage()
    {
        var box = MessageBoxManager.GetMessageBoxStandard("No IP address", "Please set your PS3 IP address in the Settings.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
        await box.ShowWindowAsync();
    }

    #endregion

    private void SettingsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewSettings = new PSSettings() { ShowActivated = true };
        NewSettings.Show();
    }

}