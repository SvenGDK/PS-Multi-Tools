Public Class PS4Menu

#Region "Tools"

    Private Sub OpenPUPExtractorMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenPUPExtractorMenuItem.Click
        Dim NewPUPExtractor As New PUPExtractor() With {.ShowActivated = True}
        NewPUPExtractor.Show()
    End Sub

    Private Sub OpenUSBWriterMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenUSBWriterMenuItem.Click
        Dim NewUSBWriter As New USBWriter() With {.ShowActivated = True}
        NewUSBWriter.Show()
    End Sub

    Private Sub OpenPayloadSenderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenPayloadSenderMenuItem.Click
        Dim NewPKGSender As New PS5Sender() With {.ShowActivated = True}
        NewPKGSender.Show()
    End Sub

    Private Sub OpenFTPBrowserMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenFTPBrowserMenuItem.Click
        Dim NewFTPBrowser As New FTPBrowser() With {.ShowActivated = True}
        NewFTPBrowser.Show()
    End Sub

    Private Sub OpenParamSFOEditorMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenParamSFOEditorMenuItem.Click
        Dim NewSFOEditor As New SFOEditor() With {.ShowActivated = True}
        NewSFOEditor.Show()
    End Sub

    Private Sub OpenPKGMergerMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenPKGMergerMenuItem.Click
        Dim NewPS4PKGMerger As New PS5PKGMerger() With {.ShowActivated = True}
        NewPS4PKGMerger.Show()
    End Sub

    Private Sub OpenPPPwnerMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenPPPwnerMenuItem.Click
        Dim NewPPPwner As New PPPwner() With {.ShowActivated = True}
        NewPPPwner.Show()
    End Sub

    Private Sub CheckForUpdatesMenuItems_Click(sender As Object, e As RoutedEventArgs) Handles CheckForUpdatesMenuItems.Click
        If Utils.IsPSMultiToolsUpdateAvailable() Then
            If MsgBox("An update is available, do you want to download it now ?", MsgBoxStyle.YesNo, "PS Multi Tools Update found") = MsgBoxResult.Yes Then
                Utils.DownloadAndExecuteUpdater()
            End If
        Else
            MsgBox("PS Multi Tools is up to date!", MsgBoxStyle.Information, "No update found")
        End If
    End Sub

    Private Sub OpenPKGExtractorMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenPKGExtractorMenuItem.Click
        Dim NewPKGExtractor As New PS4PKGExtractor() With {.ShowActivated = True}
        NewPKGExtractor.Show()
    End Sub

    Private Sub OpenPSClassicsfPKGBuilderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenPSClassicsfPKGBuilderMenuItem.Click
        Dim NewPSClassicsfPKGBuilder As New PSClassicsfPKGBuilder() With {.ShowActivated = True}
        NewPSClassicsfPKGBuilder.Show()
    End Sub

#End Region

#Region "Menu Downloads"

#Region "Hosts & Exploits"

    Private Sub Download405Host_Click(sender As Object, e As RoutedEventArgs) Handles Download405Host.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/ex/PS4-4.05-Kernel-Exploit-master.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub Download405HostGithub_Click(sender As Object, e As RoutedEventArgs) Handles Download405HostGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/Cryptogenic/PS4-4.05-Kernel-Exploit/archive/refs/heads/master.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub Download505Host_Click(sender As Object, e As RoutedEventArgs) Handles Download505Host.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/ex/PS4-5.05-Kernel-Exploit-master.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub Download505HostGithub_Click(sender As Object, e As RoutedEventArgs) Handles Download505HostGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/Cryptogenic/PS4-5.05-Kernel-Exploit/archive/refs/heads/master.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub Download672Host_Click(sender As Object, e As RoutedEventArgs) Handles Download672Host.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/ex/ps4jb-6.72-master.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub Download672HostGithub_Click(sender As Object, e As RoutedEventArgs) Handles Download672HostGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/sleirsgoevy/ps4jb/archive/refs/heads/master.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub Download750Host_Click(sender As Object, e As RoutedEventArgs) Handles Download750Host.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/ex/ps4jb-7.55-2.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadMira_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMira.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/ex/mira-7.55.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub Download900Exfathax_Click(sender As Object, e As RoutedEventArgs) Handles Download900Exfathax.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/ex/exfathax.img") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub Download900ExfathaxGithub_Click(sender As Object, e As RoutedEventArgs) Handles Download900ExfathaxGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/ChendoChap/pOOBs4/raw/main/exfathax.img") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub Download900Github_Click(sender As Object, e As RoutedEventArgs) Handles Download900Github.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/ChendoChap/pOOBs4/archive/refs/heads/main.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub Download900Host_Click(sender As Object, e As RoutedEventArgs) Handles Download900Host.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/ex/pOOBs4-main.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub OpenPPPwnGitHub_Click(sender As Object, e As RoutedEventArgs) Handles OpenPPPwnGitHub.Click
        Process.Start("https://github.com/TheOfficialFloW/PPPwn")
    End Sub

    Private Sub OpenPPPwnTool_Click(sender As Object, e As RoutedEventArgs) Handles OpenPPPwnTool.Click
        Dim NewPPPwner As New PPPwner() With {.ShowActivated = True}
        NewPPPwner.Show()
    End Sub

#End Region

#Region "Homebrew & Payloads"

    Private Sub DownloadApolloST_Click(sender As Object, e As RoutedEventArgs) Handles DownloadApolloST.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/bucanero/apollo-ps4/releases/download/v1.6.0/IV0000-APOL00004_00-APOLLO0000000PS4.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadBrew_Click(sender As Object, e As RoutedEventArgs) Handles DownloadBrew.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/Brew_V1.00.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadFTPClient_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFTPClient.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/FTPC00001_V1.0.8.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadGoldHEN_Click(sender As Object, e As RoutedEventArgs) Handles DownloadGoldHEN.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/GoldHEN/GoldHEN/blob/beta/goldhen.bin") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadGoldHENCM_Click(sender As Object, e As RoutedEventArgs) Handles DownloadGoldHENCM.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/GoldHENCheatsManager_V1.0.3.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadGoldHENCMGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadGoldHENCMGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/GoldHEN/GoldHEN_Cheat_Manager/releases/latest/download/IV0000-GOLD00777_00-GOLDCHEATS000PS4.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadHamachi_Click(sender As Object, e As RoutedEventArgs) Handles DownloadHamachi.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/LogMeInHamachi_V0.2.3.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadHomebrewStore_Click(sender As Object, e As RoutedEventArgs) Handles DownloadHomebrewStore.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/Store-R2.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadIconMask_Click(sender As Object, e As RoutedEventArgs) Handles DownloadIconMask.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/IconMask_V1.10.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadItemzflowGM_Click(sender As Object, e As RoutedEventArgs) Handles DownloadItemzflowGM.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/ItemzflowGameManager_V1.03.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadNoBDToolkit_Click(sender As Object, e As RoutedEventArgs) Handles DownloadNoBDToolkit.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/NoBDToolkit_V2.00.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPayloadGuest_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPayloadGuest.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/PayloadGuest_V0.98.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPayloadGuestGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPayloadGuestGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/Al-Azif/ps4-payload-guest/releases/latest/download/IV0000-AZIF00003_00-PAYLOADGUEST0000.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPKGi_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPKGi.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/PKGi_V1.00.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPS4Player_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPS4Player.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/PS4Player_V1.07.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPS4ToolSet_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPS4ToolSet.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/PS4TOOLSET_V2.0.0.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPS4Xplorer_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPS4Xplorer.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/PS4-Xplorer2.0_V2.01.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadRemotePKGInstaller_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRemotePKGInstaller.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/RemotePKGInstaller_V1.02.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadSMBClient_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSMBClient.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/SMBC00001_V1.10.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadWebDAVClient_Click(sender As Object, e As RoutedEventArgs) Handles DownloadWebDAVClient.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/hb/WDVC00001_V1.04.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#Region "Firmwares"

    Private Sub DownloadRecFW505_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRecFW505.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/recovery/5.05/PS4UPDATE.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadRecFW672_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRecFW672.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/recovery/6.72/PS4UPDATE.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadRecFW900_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRecFW900.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/recovery/9.00/PS4UPDATE.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadRecFW1100_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRecFW1100.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/recovery/11.00/PS4UPDATE.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadSysFW505_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSysFW505.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/system/5.05/PS4UPDATE.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadSysFW672_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSysFW672.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/system/6.72/PS4UPDATE.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadSysFW900_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSysFW900.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/system/9.00/PS4UPDATE.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadSysFW1100_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSysFW1100.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/fw/system/11.00/PS4UPDATE.PUP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#Region "Utilities"

    Private Sub DownloadDiscDumperVTX405_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDiscDumperVTX405.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/other/ps4-dumper-4.05.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadDiscDumperVTX455_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDiscDumperVTX455.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/other/ps4-dumper-4.55.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadDiscDumperVTX505_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDiscDumperVTX505.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/other/ps4-dumper-5.05.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadSaturnFPKG_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSaturnFPKG.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/other/SATURN-FPKG_v1.1.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#Region "Emulators"

    Private Sub DownloadMednafen_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMednafen.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/emu/MEDNAFEN_V1.00.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadmGBA_Click(sender As Object, e As RoutedEventArgs) Handles DownloadmGBA.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/emu/mGBA_V1.00.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPCSXR_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPCSXR.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/emu/PCSX00002_V1.00.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadRetroArchApp_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroArchApp.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/emu/RetroArch_PS4_r4.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadRetroArchCores_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroArchCores.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/emu/Cores_Installer_r4.1.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadSnesStation_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSnesStation.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/emu/SNESSTATION_V1.00.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadYabause_Click(sender As Object, e As RoutedEventArgs) Handles DownloadYabause.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps4/emu/Yabause_V1.00.pkg") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#End Region

End Class
