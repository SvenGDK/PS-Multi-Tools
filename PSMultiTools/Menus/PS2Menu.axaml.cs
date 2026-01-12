using Avalonia.Controls;
using Avalonia.Interactivity;
using PSMultiTools.Classes;
using PSMultiTools.Dialogs;
using PSMultiTools.PS1.Tools;
using PSMultiTools.PS2.Tools;
using System.Diagnostics;

namespace PSMultiTools.Menus;

public partial class PS2Menu : UserControl
{
    public PS2Menu()
    {
        InitializeComponent();
    }

    public ListBox? GamesListBox;

    #region Exploit Infos

    private void OpenFortunaGithub_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/ps2homebrew/opentuna-installer") { UseShellExecute = true });
    }

    private void OpenFreeDVDBootGithub_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/CTurt/FreeDVDBoot") { UseShellExecute = true });
    }

    private void OpenFreeHDBootGithub_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://israpps.github.io/FreeMcBoot-Installer/test/FHDB-TUTO.html") { UseShellExecute = true });
    }

    private void OpenFreeMCBootGithub_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://israpps.github.io/FreeMcBoot-Installer/test/1_Introduction.html") { UseShellExecute = true });
    }

    private void OpenFreeMCBootPSXGithub_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://israpps.github.io/FreeMcBoot-Installer/test/1_Introduction.html") { UseShellExecute = true });
    }

    private void OpenIndependenceArchive_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://web.archive.org/web/20050529124415/http://www.0xd6.org/ps2-independence.html") { UseShellExecute = true });
    }

    private void OpenSwapGithub_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://www.psdevwiki.com/ps2/Swap_Magic") { UseShellExecute = true });
    }

    private void OpenYabasicGithub_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/CTurt/PS2-Yabasic-Exploit") { UseShellExecute = true });
    }

    #endregion

    #region Menu Downloads

    #region Homebrew

    private async void DownloadCheatDevice_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/CheatDevicePS2-v1.7.5.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadFSCK_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/FSCK-tool-c7679407.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadGSM_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/gsm037.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadHDDChecker_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/HDDChecker-c7679407.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadHDLGameInstaller_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/HDLGameInstaller-6e8d52aa.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadKELFBinder_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/KELFBinder.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadMCF_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/MFU-Packed.elf") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadMechaPwn_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/MechaPwn_pck_3.0rc4.elf") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadOpenPS2LoaderLangs_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPNPS2LD-LANGUAGES-AND-FONTS-v1.1.0.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadOpenPS2LoaderLatestLangs_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPNPS2LD-LANGS-v1.2.0-Beta-1987-1c5bc79.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadOpenPS2LoaderLatestNormal_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPNPS2LD-v1.2.0-Beta-1987-1c5bc79.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadOpenPS2LoaderLatestVariants_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPNPS2LD-VARIANTS-v1.2.0-Beta-1987-1c5bc79.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadOpenPS2LoaderNormal_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPNPS2LD-v1.1.0.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadOpenPS2LoaderVariants_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPNPS2LD-VARIANTS-v1.1.0.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadOPLLauncher_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPL-Launcher-latest.elf") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadOPLLauncherELF_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPL-Launcher.elf") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadOPLLauncherKELF_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPL-Launcher.kelf") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPS2BBL_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/PS2BBL.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPS2Ident_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/PS2Ident-9032110d-PS2TOOL.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSMS_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/SMS.Version.2.9.Rev.4.elf.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadVTSPS2HBDL_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/VTSPS2-HBDL.elf") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadwLaunchELF_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/wLaunchELF-073dd41.ELF") == false)
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
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/emu/daedalusps2.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadFCEUmm_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/emu/fceu-packed.v0.3.3.elf") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadFCEUmmCDVD_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/emu/fceu-packed.v0.3.3.cdvdsupport.elf") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadGBANTSC_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/emu/GBA_PS2_(v1.45.5_rev3)_NTSC.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadGBAPAL_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/emu/GBA_PS2_(v1.45.5_rev3)_PAL.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRetroArch_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/emu/RetroArch_elf.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSnesStation_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/emu/snes_station_emu-v024S-2016-09-06.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #region Tools

    private async void DownloadBmp2Icon_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/tools/Bmp2Icon_v0.2.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadCodeSeek_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/tools/codeseek.rar") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadDiscPatcher_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/tools/DiscPatcher3.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadELFExtract_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/tools/elf_extract_100.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadESRGUI_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/tools/ESR_Disc_Patcher_GUI_v0.24a.rar") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadFAT32GUI_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/tools/guiformat.exe") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadMastercodeFinder_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/tools/mastercode_finder_211.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #region PSX DVR DESR

    private async void DownloadEnglishXMBTr_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/SvenGDK/DESR-Tools/blob/main/XMB%20Translations/DESR-5500-5700-7500-7700%20FW2.11%20ENGLISH%20v1.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadFrenchXMBTr_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/SvenGDK/DESR-Tools/blob/main/XMB%20Translations/DESR-5500-5700-7500-7700%20FW2.10%20FRENCH.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadGermanXMBTr_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/SvenGDK/DESR-Tools/blob/main/XMB%20Translations/DESR-5500-5700-7500-7700%20FW2.11%20DEUTSCH%20v1.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadUpdate131_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/SvenGDK/DESR-Tools/blob/main/Update%20Discs/DESR-7000-DESR-5000-DESR-7100-DESR-5100%20-%20PSX%20Update%20Disc%20Ver.%201.31.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadUpdate211_Click(object sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/SvenGDK/DESR-Tools/blob/main/Update%20Discs/DESR-7500-DESR-5500-DESR-7700-DESR-5700%20-%20PSX%20Update%20Disc%20Ver.%202.11.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }


    #endregion

    #endregion

    #region Tools

    private void OpenBINCUEConverter_Click(object sender, RoutedEventArgs e)
    {
        var NewBINCUEConverter = new BINCUEConverter() { ShowActivated = true };
        NewBINCUEConverter.Show();
    }

    private void OpenMCManager_Click(object sender, RoutedEventArgs e)
    {
        //var NewPS2MCManager = new MCManager() { ShowActivated = true };
        //NewPS2MCManager.Show();
    }

    private void OpenCUE2POPSConverter_Click(object sender, RoutedEventArgs e)
    {
        var NewCUE2POPSConverter = new CUE2POPSConverter() { ShowActivated = true };
        NewCUE2POPSConverter.Show();
    }

    private void OpenELFtoKELFWrapper_Click(object sender, RoutedEventArgs e)
    {
        var NewELFWrapper = new ELFWrapper() { ShowActivated = true };
        NewELFWrapper.Show();
    }

    private void OpenPAKerUtility_Click(object sender, RoutedEventArgs e)
    {
        var NewPAKerUtility = new PAKerUtility() { ShowActivated = true };
        NewPAKerUtility.Show();
    }

    private void OpenStarExtractor_Click(object sender, RoutedEventArgs e)
    {
        var NewSTARExtractor = new STARExtractor() { ShowActivated = true };
        NewSTARExtractor.Show();
    }

    #endregion

}