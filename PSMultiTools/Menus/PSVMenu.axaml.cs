using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using PSMultiTools.Dialogs;
using PSMultiTools.MultiPlatformTools;
using PSMultiTools.PS5.Tools;
using PSMultiTools.PSV.Tools;

namespace PSMultiTools.Menus;

public partial class PSVMenu : UserControl
{

    public ListBox? LView;
    public ListBox? GamesLView;

    public PSVMenu()
    {
        InitializeComponent();
    }

    #region Menu Downloads

    #region Homebrew

    private async void DownloadAdrenaline_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/Adrenaline-7.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadAdrenalineBubbleManager_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/AdrenalineBubbleManager-6.19.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadAdrenalineBubbleManagerGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/ONElua/AdrenalineBubbleManager/releases/latest/download/AdrenalineBubbleManager.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadAdrenalineGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/TheOfficialFloW/Adrenaline/releases/latest/download/Adrenaline.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadAdrenalineStates_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/AdrenalineStates.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadApollo_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/apollo-vita-1.2.4.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadApolloGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/bucanero/apollo-vita/releases/latest/download/apollo-vita.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadAppDBTool_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/appdbtool.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadDownloadEnabler_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/download_enabler.suprx") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadDownloadEnablerGithub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/TheOfficialFloW/VitaTweaks/releases/latest/download/download_enabler.suprx") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadElevenMPV_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/ElevenMPV-A-7.10.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadElevenMPVGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/GrapheneCt/ElevenMPV-A/releases/latest/download/ElevenMPV-A.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadFontInstaller_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/fontInstaller-1.0.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadFontInstallerGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/cxziaho/fontInstaller/releases/latest/download/fontInstaller.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadFontRedirect_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/fontRedirect.suprx") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadFTPClient_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/ftpclient-1.54.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadFTPClientGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/cy33hc/vita-ftp-client/releases/latest/download/ftpclient.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadiTLSEnso_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/iTLS-Enso-3.2.1.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadiTLSEnsoGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/SKGleba/iTLS-Enso/releases/latest/download/iTLS-Enso.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadMediaImporter_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/MediaImporter-0.91.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadMediaImporterGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/cnsldv/MediaImporter/releases/latest/download/MediaImporter.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadMoonlight_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/moonlight-0.9.2.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadMoonlightGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/xyzz/vita-moonlight/releases/latest/download/moonlight.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadMultidownloadVita_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/Multidownload-Vita-1.0.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadMultidownloadVitaGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/DavisDev/Multidownload-Vita/releases/latest/download/Multidownload-Vita.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadMVPLAYER0_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/MVPLAYER0-1.3.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadMVPLAYER0GitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/AntHJ/MVPlayer/releases/latest/download/MVPLAYER0.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadNetcheckBypass_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/netcheck_bypass.suprx") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadNetcheckBypassGithub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/yifanlu/netcheck_bypass/releases/latest/download/netcheck_bypass.suprx") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadNetStream_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/NetStream-2.04.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadNetStreamGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/GrapheneCt/NetStream/releases/latest/download/NetStream.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadParentalControlBypass_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/Parental_Control_Bypass.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void Downloadpkgj_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/pkgj-0.57.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadpkgjGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/blastrock/pkgj/releases/latest/download/pkgj.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPNGShot_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/pngshot.suprx") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPNGShotGithub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/xyzz/pngshot/releases/latest/download/pngshot.suprx") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRegistryEditor_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/RegistryEditor-1.0.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRetroFlow_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/RetroFlow_v6.0.0.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRetroFlowAdrenalineLauncher_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/RetroFlow_Adrenaline_Launcher_v3.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRetroFlowAdrenalineLauncherGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/jimbob4000/RetroFlow-Launcher/releases/latest/download/RetroFlow_Adrenaline_Launcher_v3.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRetroFlowGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/jimbob4000/RetroFlow-Launcher/releases/latest/download/RetroFlow_v6.0.0.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSMBClient_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/smbclient-1.04.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSMBClientGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/cy33hc/vita-smb-client/releases/latest/download/smbclient.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadUSBDisable_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/USBDisable.suprx") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadUSBDisableGithub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/Ibrahim778/USBDisable/releases/latest/download/USBDisable.suprx") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadUserAgentSpoofer_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/UserAgentSpoofer.suprx") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadUserAgentSpooferGithub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/GrapheneCt/UserAgentSpoofer/releases/latest/download/UserAgentSpoofer.suprx") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadVITAlbum_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/VITAlbum-1.40.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadVITAlbumGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/joel16/VITAlbum/releases/latest/download/VITAlbum.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadVitaMediaPlayer_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/VitaMediaPlayer-1.01.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadVitaMediaPlayerGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/SonicMastr/Vita-Media-Player/releases/latest/download/VitaMediaPlayer.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadVitaShell_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/VitaShell-2.02.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadVitaShellGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/TheOfficialFloW/VitaShell/releases/latest/download/VitaShell.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadWebDAVClient_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/webdavclient-1.02.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadWebDAVClientGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/cy33hc/vita-webdav-client/releases/latest/download/webdavclient.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadTrophiesFixer_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/trophies_fixer-1.1.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloaTrophiesFixerGitHub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/Yoti/psv_trophfix/releases/latest/download/trophies_fixer.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadBlasphemousPort_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/BlasphemousVita.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadCupheadPort_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/Cuphead.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadGTACTWPort_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/GTACTW.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadNoNpDrm_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/nonpdrm.skprx") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadNoNpDrmGithub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/TheOfficialFloW/NoNpDrm/releases/latest/download/nonpdrm.skprx") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadreF00D_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/reF00D.skprx") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadreF00DGithub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/dots-tb/reF00D/releases/latest/download/reF00D.skprx") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSonicManiaPort_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/SonicMania.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #region Firmwares

    private async void DownloadOFW365_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/fw/OFW3.65.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #region Emulators

    private async void DownloadDaedalusX64_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/DaedalusX64.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadDaedalusX64Github_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/Rinnegatamante/DaedalusX64-vitaGL/releases/latest/download/DaedalusX64.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadFlycast_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/Flycast-1.1.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadFlycastGithub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/Rinnegatamante/flycast-vita/releases/latest/download/Flycast.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadmGBA_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/mGBA-0.10.1-vita.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRetroArch_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/RetroArch.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRetroArchData_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/RetroArch_data.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSnes9xVITA_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/Snes9xVITA.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSnes9xVITAGithub_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/theheroGAC/Snes9xVITA/releases/download/latest/Snes9xVITA.vpk") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #endregion

    #region Tools

    private void OpenPKGExtractor_Click(object sender, RoutedEventArgs e)
    {
        var NewPKGExtractor = new PSVPKGExtractor() { ShowActivated = true };
        NewPKGExtractor.Show();
    }

    private void OpenRCOExtractor_Click(object sender, RoutedEventArgs e)
    {
        var NewPS5RcoExtractor = new PS5RcoExtractor() { ShowActivated = true };
        NewPS5RcoExtractor.Show();
    }

    private async void OpenIMGTools_Click(object sender, RoutedEventArgs e)
    {
        // Dim NewPSVIMGTools As New PSVIMGTools() With {.ShowActivated = True}
        // NewPSVIMGTools.Show()
        var box = MessageBoxManager.GetMessageBoxStandard("Info", "Not ready yet", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
        await box.ShowWindowAsync();
    }

    private void OpenParamSFOEditorMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var NewSFOEditor = new SFOEditor() { ShowActivated = true };
        NewSFOEditor.Show();
    }

    private void OpenPFSToolsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var NewPSVPFSTools = new PSVPFSTools() { ShowActivated = true };
        NewPSVPFSTools.Show();
    }

    #endregion

}