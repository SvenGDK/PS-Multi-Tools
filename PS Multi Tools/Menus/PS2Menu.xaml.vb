Public Class PS2Menu

    Public GamesLView As ListView

#Region "Exploit Infos"

    Private Sub OpenFortunaGithub_Click(sender As Object, e As RoutedEventArgs) Handles OpenFortunaGithub.Click
        Process.Start("https://github.com/ps2homebrew/opentuna-installer")
    End Sub

    Private Sub OpenFreeDVDBootGithub_Click(sender As Object, e As RoutedEventArgs) Handles OpenFreeDVDBootGithub.Click
        Process.Start("https://github.com/CTurt/FreeDVDBoot")
    End Sub

    Private Sub OpenFreeHDBootGithub_Click(sender As Object, e As RoutedEventArgs) Handles OpenFreeHDBootGithub.Click
        Process.Start("https://israpps.github.io/FreeMcBoot-Installer/test/FHDB-TUTO.html")
    End Sub

    Private Sub OpenFreeMCBootGithub_Click(sender As Object, e As RoutedEventArgs) Handles OpenFreeMCBootGithub.Click
        Process.Start("https://israpps.github.io/FreeMcBoot-Installer/test/1_Introduction.html")
    End Sub

    Private Sub OpenFreeMCBootPSXGithub_Click(sender As Object, e As RoutedEventArgs) Handles OpenFreeMCBootPSXGithub.Click
        Process.Start("https://israpps.github.io/FreeMcBoot-Installer/test/1_Introduction.html")
    End Sub

    Private Sub OpenIndependenceArchive_Click(sender As Object, e As RoutedEventArgs) Handles OpenIndependenceArchive.Click
        Process.Start("https://web.archive.org/web/20050529124415/http://www.0xd6.org/ps2-independence.html")
    End Sub

    Private Sub OpenSwapGithub_Click(sender As Object, e As RoutedEventArgs) Handles OpenSwapGithub.Click
        Process.Start("https://en.wikipedia.org/wiki/Swap_Magic")
    End Sub

    Private Sub OpenYabasicGithub_Click(sender As Object, e As RoutedEventArgs) Handles OpenYabasicGithub.Click
        Process.Start("https://github.com/CTurt/PS2-Yabasic-Exploit")
    End Sub


#End Region

#Region "Menu Downloads"

#Region "Homebrew"

    Private Sub DownloadCheatDevice_Click(sender As Object, e As RoutedEventArgs) Handles DownloadCheatDevice.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/CheatDevicePS2-v1.7.5.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadFSCK_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFSCK.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/FSCK-tool-c7679407.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadGSM_Click(sender As Object, e As RoutedEventArgs) Handles DownloadGSM.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/gsm037.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadHDDChecker_Click(sender As Object, e As RoutedEventArgs) Handles DownloadHDDChecker.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/HDDChecker-c7679407.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadHDLGameInstaller_Click(sender As Object, e As RoutedEventArgs) Handles DownloadHDLGameInstaller.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/HDLGameInstaller-6e8d52aa.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadKELFBinder_Click(sender As Object, e As RoutedEventArgs) Handles DownloadKELFBinder.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/KELFBinder.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadMCF_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMCF.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/MFU-Packed.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadMechaPwn_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMechaPwn.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/MechaPwn_pck_3.0rc4.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadOpenPS2LoaderLangs_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOpenPS2LoaderLangs.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPNPS2LD-LANGUAGES-AND-FONTS-v1.1.0.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadOpenPS2LoaderLatestLangs_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOpenPS2LoaderLatestLangs.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPNPS2LD-LANGS-v1.2.0-Beta-1987-1c5bc79.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadOpenPS2LoaderLatestNormal_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOpenPS2LoaderLatestNormal.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPNPS2LD-v1.2.0-Beta-1987-1c5bc79.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadOpenPS2LoaderLatestVariants_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOpenPS2LoaderLatestVariants.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPNPS2LD-VARIANTS-v1.2.0-Beta-1987-1c5bc79.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadOpenPS2LoaderNormal_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOpenPS2LoaderNormal.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPNPS2LD-v1.1.0.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadOpenPS2LoaderVariants_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOpenPS2LoaderVariants.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPNPS2LD-VARIANTS-v1.1.0.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadOPLLauncher_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOPLLauncher.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPL-Launcher-latest.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadOPLLauncherELF_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOPLLauncherELF.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPL-Launcher.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadOPLLauncherKELF_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOPLLauncherKELF.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/OPL-Launcher.kelf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPS2BBL_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPS2BBL.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/PS2BBL.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPS2Ident_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPS2Ident.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/PS2Ident-9032110d-PS2TOOL.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadSMS_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSMS.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/SMS.Version.2.9.Rev.4.elf.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadVTSPS2HBDL_Click(sender As Object, e As RoutedEventArgs) Handles DownloadVTSPS2HBDL.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/VTSPS2-HBDL.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadwLaunchELF_Click(sender As Object, e As RoutedEventArgs) Handles DownloadwLaunchELF.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/hb/wLaunchELF-073dd41.ELF") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#Region "Emulators"

    Private Sub DownloadDaedalusX64_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDaedalusX64.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/emu/daedalusps2.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadFCEUmm_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFCEUmm.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/emu/fceu-packed.v0.3.3.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadFCEUmmCDVD_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFCEUmmCDVD.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/emu/fceu-packed.v0.3.3.cdvdsupport.elf") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadGBANTSC_Click(sender As Object, e As RoutedEventArgs) Handles DownloadGBANTSC.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/emu/GBA_PS2_(v1.45.5_rev3)_NTSC.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadGBAPAL_Click(sender As Object, e As RoutedEventArgs) Handles DownloadGBAPAL.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/emu/GBA_PS2_(v1.45.5_rev3)_PAL.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadRetroArch_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroArch.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/emu/RetroArch_elf.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadSnesStation_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSnesStation.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/emu/snes_station_emu-v024S-2016-09-06.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#Region "Tools"

    Private Sub DownloadBmp2Icon_Click(sender As Object, e As RoutedEventArgs) Handles DownloadBmp2Icon.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/tools/Bmp2Icon_v0.2.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadCodeSeek_Click(sender As Object, e As RoutedEventArgs) Handles DownloadCodeSeek.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/tools/codeseek.rar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadDiscPatcher_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDiscPatcher.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/tools/DiscPatcher3.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadELFExtract_Click(sender As Object, e As RoutedEventArgs) Handles DownloadELFExtract.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/tools/elf_extract_100.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadESRGUI_Click(sender As Object, e As RoutedEventArgs) Handles DownloadESRGUI.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/tools/ESR_Disc_Patcher_GUI_v0.24a.rar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadFAT32GUI_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFAT32GUI.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/tools/guiformat.exe") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadMastercodeFinder_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMastercodeFinder.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps2/tools/mastercode_finder_211.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#Region "PSX DVR DESR"

    Private Sub DownloadEnglishXMBTr_Click(sender As Object, e As RoutedEventArgs) Handles DownloadEnglishXMBTr.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/SvenGDK/DESR-Tools/blob/main/XMB%20Translations/DESR-5500-5700-7500-7700%20FW2.11%20ENGLISH%20v1.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadFrenchXMBTr_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFrenchXMBTr.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/SvenGDK/DESR-Tools/blob/main/XMB%20Translations/DESR-5500-5700-7500-7700%20FW2.10%20FRENCH.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadGermanXMBTr_Click(sender As Object, e As RoutedEventArgs) Handles DownloadGermanXMBTr.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/SvenGDK/DESR-Tools/blob/main/XMB%20Translations/DESR-5500-5700-7500-7700%20FW2.11%20DEUTSCH%20v1.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadUpdate131_Click(sender As Object, e As RoutedEventArgs) Handles DownloadUpdate131.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/SvenGDK/DESR-Tools/blob/main/Update%20Discs/DESR-7000-DESR-5000-DESR-7100-DESR-5100%20-%20PSX%20Update%20Disc%20Ver.%201.31.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadUpdate211_Click(sender As Object, e As RoutedEventArgs) Handles DownloadUpdate211.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/SvenGDK/DESR-Tools/blob/main/Update%20Discs/DESR-7500-DESR-5500-DESR-7700-DESR-5700%20-%20PSX%20Update%20Disc%20Ver.%202.11.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub


#End Region

#End Region

#Region "Tools"

    Private Sub OpenBINCUEConverter_Click(sender As Object, e As RoutedEventArgs) Handles OpenBINCUEConverter.Click
        Dim NewBINCUEConverter As New BINCUEConverter() With {.ShowActivated = True}
        NewBINCUEConverter.Show()
    End Sub

    Private Sub OpenMCManager_Click(sender As Object, e As RoutedEventArgs) Handles OpenMCManager.Click
        Dim NewPS2MCManager As New MCManager() With {.ShowActivated = True}
        NewPS2MCManager.Show()
    End Sub

    Private Sub OpenCUE2POPSConverter_Click(sender As Object, e As RoutedEventArgs) Handles OpenCUE2POPSConverter.Click
        Dim NewCUE2POPSConverter As New CUE2POPSConverter() With {.ShowActivated = True}
        NewCUE2POPSConverter.Show()
    End Sub

    Private Sub OpenELFtoKELFWrapper_Click(sender As Object, e As RoutedEventArgs) Handles OpenELFtoKELFWrapper.Click
        Dim NewELFWrapper As New ELFWrapper() With {.ShowActivated = True}
        NewELFWrapper.Show()
    End Sub

#End Region

End Class
