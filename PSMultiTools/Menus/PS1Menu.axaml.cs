using Avalonia.Controls;
using Avalonia.Interactivity;
using PSMultiTools.Classes;
using PSMultiTools.Dialogs;
using PSMultiTools.PS1.Tools;

namespace PSMultiTools.Menus;

public partial class PS1Menu : UserControl
{

    public ListBox? GamesListBox;

    public PS1Menu()
    {
        InitializeComponent();
    }

    #region Menu Downloads

    #region Exploits

    private async void DownloadFreePSXBoot_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps1/freepsxboot-2.1.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadTonyHax_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps1/tonyhax-v1.4.5.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadTonyHaxInternational_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps1/tonyhax-international-v1.5.1.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #region Tools

    private async void DownloadAprip_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps1/aprip-v1.0.9-windows-x86_64-static.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadEDCRE_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/alex-free/edcre/releases/download/v1.1.0/edcre-v1.1.0-windows-x86_64-static.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadEDCRELinux_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/alex-free/edcre/releases/download/v1.1.0/edcre-v1.1.0-linux-x86_64-static.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadEDCREmacOS_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/alex-free/edcre/releases/download/v1.1.0/edcre-v1.1.0-mac-os-x86_64.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadLibCryptPatcher_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/alex-free/libcrypt-patcher/releases/download/v1.0.9/libcrypt-patcher-v1.0.9-windows-x86_64-static.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadLibCryptPatcherLinux_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/alex-free/libcrypt-patcher/releases/download/v1.0.9/libcrypt-patcher-v1.0.9-linux-x86_64-static.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadLibCryptPatchermacOS_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("https://github.com/alex-free/libcrypt-patcher/releases/download/v1.0.9/libcrypt-patcher-v1.0.9-mac-os-x86_64.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPSEXE2ROM_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps1/psexe2rom_1.0.2_windows_x86_64.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    private async void DownloadPSX80MP_Click(object? sender, RoutedEventArgs e)
    {
        var NewDownloader = new Downloader() { ShowActivated = true };
        NewDownloader.Show();
        if (await NewDownloader.CreateNewDownload("http://X.X.X.X/ps1/psx80mp-v2.0-windows-x86_64.zip") == false)
        {
            Utils.ShowDownloadErrorMessage();
            NewDownloader.Close();
        }
    }

    #endregion

    #endregion

    #region Tools

    private void OpenMergeBinTool_Click(object? sender, RoutedEventArgs e)
    {
        var NewMergeBINTool = new MergeBinTool() { ShowActivated = true };
        NewMergeBINTool.Show();
    }

    private void OpenBINCUEConverter_Click(object? sender, RoutedEventArgs e)
    {
        var NewBINCUEConverter = new BINCUEConverter() { ShowActivated = true, ConvertForPS1 = true };
        NewBINCUEConverter.Show();
    }

    #endregion

}