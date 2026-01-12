using Avalonia.Controls;
using Avalonia.Interactivity;
using PSMultiTools.Classes;
using PSMultiTools.Dialogs;
using PSMultiTools.MultiPlatformTools;
using PSMultiTools.PSP.Tools;

namespace PSMultiTools.Menus;

public partial class PSPMenu : UserControl
{
    public PSPMenu()
    {
        InitializeComponent();
    }

    #region Menu Downloads

    #region Homebrew

    private async void DownloadAlphabase_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/alphabase-3.6.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadBeatBox_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/beatbox-1.6.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadBookr_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/bookr-8.2.0.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadCex2DexConverter_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/Cex2DexConverter-1.1.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadCMFileManager_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/CMFileManager-PSP.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadCMFileManagerGo_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/CMFileManager-PSP-GO-M2.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadCyanogenPSP_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/cyanogenpsp-6.1Final.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadFlashlight_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/flashlightpsp.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadIDPSDumper_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/PSP_IDPS_Dumper_v0.9.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPaintLite_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/paintlitepsp-0.3.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadprxDecrypter_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/prxdecrypter-2.7a.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPSPIdent_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/pspident-0.75.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadQwikMove_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/qwikmovepsp-v2.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRebootPSP_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/rebootpsp.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadShutdownPSP_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/shutdownpsp.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadWifiHack_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/wifihack-2.1.rar") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #region Emulators

    private async void DownloadRetroArch_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/emu/RetroArch-1.15.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #region Firmwares

    private async void Download100PBP_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/fw/100.PBP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void Download360Archive_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/fw/360.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void Download360PSAR_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/fw/360.PSAR") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void Download570PSARGo_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/fw/570go.PSAR") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void Download661PBP_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/fw/661.PBP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void Download661PBPGo_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/fw/661go.PBP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #region Tools

    private async void DownloadISOPBPConverter_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/tools/isopbpconverter0.1.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPBPUnpacker_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/tools/pbpunpackerrea-1.2.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPBPViewer_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/tools/pbpviewerlma-0.1.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPSPDisp_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/tools/pspdisp0.6.1.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRCOMage_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/tools/rcomage-1.1.1.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRemoteJoyLite_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/psp/tools/remotejoylite-20a.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #endregion

    #region Tools

    private void OpenISOCISOConverter_Click(object sender, RoutedEventArgs e)
    {
        var NewCISOConverter = new CISOConverter() { ShowActivated = true };
        NewCISOConverter.Show();
    }

    private void OpenPBPISOConverter_Click(object sender, RoutedEventArgs e)
    {
        var NewPBPISOConverter = new PBPISOConverter() { ShowActivated = true };
        NewPBPISOConverter.Show();
    }

    private void OpenPBPPacker_Click(object sender, RoutedEventArgs e)
    {
        var NewPBPPacker = new PBPPacker() { ShowActivated = true };
        NewPBPPacker.Show();
    }

    private void OpenSFOEditor_Click(object sender, RoutedEventArgs e)
    {
        var NewSFOEditor = new SFOEditor() { ShowActivated = true };
        NewSFOEditor.Show();
    }

    #endregion

    private void SettingsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewSettings = new PSSettings() { ShowActivated = true };
        NewSettings.Show();
    }

}