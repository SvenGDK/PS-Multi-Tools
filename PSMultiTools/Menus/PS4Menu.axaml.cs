using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using PSMultiTools.Dialogs;
using PSMultiTools.MultiPlatformTools;
using PSMultiTools.PS4.Tools;
using PSMultiTools.PS5.Tools;
using PSMultiTools.PS5.Tools.PKGBuilder;
using System.Diagnostics;

namespace PSMultiTools.Menus;

public partial class PS4Menu : UserControl
{

    public PS4Menu()
    {
        InitializeComponent();
    }

    #region Tools

    private void OpenPUPExtractorMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPUPExtractor = new PUPExtractor() { ShowActivated = true };
        NewPUPExtractor.Show();
    }

    private void OpenUSBWriterMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewUSBWriter = new USBWriter() { ShowActivated = true };
        NewUSBWriter.Show();
    }

    private void OpenPayloadSenderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPKGSender = new PS5Sender() { ShowActivated = true };
        NewPKGSender.Show();
    }

    private void OpenFTPBrowserMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewFTPBrowser = new FTPBrowser() { ShowActivated = true };
        NewFTPBrowser.Show();
    }

    private void OpenParamSFOEditorMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewSFOEditor = new SFOEditor() { ShowActivated = true };
        NewSFOEditor.Show();
    }

    private void PayloadDispatcherMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPayloadDispatcher = new PayloadDispatcher() { ShowActivated = true };
        NewPayloadDispatcher.Show();
    }

    private void OpenPKGMergerMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPS4PKGMerger = new PS5PKGMerger() { ShowActivated = true };
        NewPS4PKGMerger.Show();
    }

    private void OpenPPPwnerMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPPPwner = new PPPwner() { ShowActivated = true };
        NewPPPwner.Show();
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

    private void OpenPKGExtractorMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPKGExtractor = new PS4PKGExtractor() { ShowActivated = true };
        NewPKGExtractor.Show();
    }

    private void OpenPSClassicsfPKGBuilderMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewPSClassicsfPKGBuilder = new PSClassicsfPKGBuilder() { ShowActivated = true };
        NewPSClassicsfPKGBuilder.Show();
    }

    #endregion

    #region Menu Downloads

    #region Hosts & Exploits

    private async void Download405Host_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/ex/PS4-4.05-Kernel-Exploit-master.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void Download405HostGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/Cryptogenic/PS4-4.05-Kernel-Exploit/archive/refs/heads/master.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void Download505Host_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/ex/PS4-5.05-Kernel-Exploit-master.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void Download505HostGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/Cryptogenic/PS4-5.05-Kernel-Exploit/archive/refs/heads/master.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void Download672Host_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/ex/ps4jb-6.72-master.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void Download672HostGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/sleirsgoevy/ps4jb/archive/refs/heads/master.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void Download750Host_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/ex/ps4jb-7.55-2.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadMira_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/ex/mira-7.55.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void Download900Exfathax_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/ex/exfathax.img") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void Download900ExfathaxGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/ChendoChap/pOOBs4/raw/main/exfathax.img") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void Download900Github_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/ChendoChap/pOOBs4/archive/refs/heads/main.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void Download900Host_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/ex/pOOBs4-main.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private void OpenPPPwnGitHub_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/TheOfficialFloW/PPPwn") { UseShellExecute = true });
    }

    private void OpenPPPwnTool_Click(object? sender, RoutedEventArgs e)
    {
        var NewPPPwner = new PPPwner() { ShowActivated = true };
        NewPPPwner.Show();
    }

    private async void DownloadGoldHENAutoloader_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/ex/pOOBs4-main.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private void OpenLapsePoosGitHub_Click(object? sender, RoutedEventArgs e)
    {
        var NewPPPwner = new PPPwner() { ShowActivated = true };
        NewPPPwner.Show();
    }

    #endregion

    #region Homebrew & Payloads

    private async void DownloadApolloST_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/bucanero/apollo-ps4/releases/download/v2.0.0/IV0000-APOL00004_00-APOLLO0000000PS4.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadBrew_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/Brew_V1.00.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadezRemoteClient_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/ezRemote_Client_v1.37.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadFPKGi_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/FPKGi_v0.87.7.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadFTPClient_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/FTPC00001_V1.0.8.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadGoldHEN_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/ex/goldhen_v2.4b18.7.bin") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadGoldHENCM_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/GoldHENCheatsManager_V1.1.4.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadHamachi_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/LogMeInHamachi_V0.2.3.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadHomebrewStore_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/Store-R2.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadIconMask_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/IconMask_V1.16.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadItemzflowGM_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/ItemzflowGameManager_v1.06.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadNoBDToolkit_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/NoBD_Toolkit_v2.04.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPayloadGuest_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/PayloadGuest_V0.98.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPayloadGuestGithub_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/Al-Azif/ps4-payload-guest/releases/latest/download/IV0000-AZIF00003_00-PAYLOADGUEST0000.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPKGi_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/PKGi_V1.00.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPS4CheatsManager_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/Cheats_Manager_v1.2.2.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPS4Player_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/PS4Player_V1.08.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPS4Player3D_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/PS4Player3D_V1.01.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPS4ToolSet_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/PS4TOOLSET_V2.20.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPS4Xplorer_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/PS4-Xplorer2.0_V2.04.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRemotePKGInstaller_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/RemotePKGInstaller_V1.02.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSMBClient_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/SMBC00001_V1.10.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadWebDAVClient_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/WDVC00001_V1.04.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #region Firmwares

    private async void DownloadRecFW505_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/recovery/5.05/PS4UPDATE.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRecFW672_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/recovery/6.72/PS4UPDATE.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRecFW900_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/recovery/9.00/PS4UPDATE.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRecFW1100_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/recovery/11.00/PS4UPDATE.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSysFW505_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/system/5.05/PS4UPDATE.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSysFW672_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/system/6.72/PS4UPDATE.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSysFW900_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/system/9.00/PS4UPDATE.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSysFW1100_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/system/11.00/PS4UPDATE.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSysFW1250_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/system/12.50/PS4UPDATE.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRecFW1250_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/recovery/12.50/PS4UPDATE.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSysFW1252_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/system/12.52/PS4UPDATE.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRecFW1252_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/recovery/12.52/PS4UPDATE.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSysFW1300_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/system/13.00/PS4UPDATE.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRecFW1300_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/recovery/13.00/PS4UPDATE.PUP") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #region Utilities

    private async void DownloadDiscDumperVTX405_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/other/ps4-dumper-4.05.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadDiscDumperVTX455_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/other/ps4-dumper-4.55.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadDiscDumperVTX505_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/other/ps4-dumper-5.05.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSaturnFPKG_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/other/SATURN-FPKG_v1.1.7z") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #region Emulators

    private async void DownloadMednafen_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/emu/MEDNAFEN_V1.00.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadmGBA_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/emu/mGBA_V1.00.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPCSXR_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/emu/PCSX00002_V1.00.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRetroArchApp_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/emu/RetroArch_PS4_r4.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadRetroArchCores_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/emu/Cores_Installer_r4.1.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadSnesStation_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/emu/SNESSTATION_V1.00.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadYabause_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/emu/Yabause_V1.00.pkg") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #endregion

    private void SettingsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var NewSettings = new PSSettings() { ShowActivated = true };
        NewSettings.Show();
    }

}