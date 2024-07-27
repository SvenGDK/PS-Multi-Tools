Public Class PSVMenu

    Public LView As Controls.ListView
    Public GamesLView As Controls.ListView

#Region "Menu Downloads"

#Region "Homebrew"

    Private Sub DownloadAdrenaline_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAdrenaline.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/Adrenaline-7.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadAdrenalineBubbleManager_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAdrenalineBubbleManager.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/AdrenalineBubbleManager-6.19.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadAdrenalineBubbleManagerGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAdrenalineBubbleManagerGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/ONElua/AdrenalineBubbleManager/releases/latest/download/AdrenalineBubbleManager.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadAdrenalineGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAdrenalineGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/TheOfficialFloW/Adrenaline/releases/latest/download/Adrenaline.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadAdrenalineStates_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAdrenalineStates.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/AdrenalineStates.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadApollo_Click(sender As Object, e As RoutedEventArgs) Handles DownloadApollo.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/apollo-vita-1.2.4.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadApolloGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadApolloGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/bucanero/apollo-vita/releases/latest/download/apollo-vita.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadAppDBTool_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAppDBTool.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/appdbtool.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadDownloadEnabler_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDownloadEnabler.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/download_enabler.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadDownloadEnablerGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDownloadEnablerGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/TheOfficialFloW/VitaTweaks/releases/latest/download/download_enabler.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadElevenMPV_Click(sender As Object, e As RoutedEventArgs) Handles DownloadElevenMPV.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/ElevenMPV-A-7.10.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadElevenMPVGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadElevenMPVGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/GrapheneCt/ElevenMPV-A/releases/latest/download/ElevenMPV-A.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadFontInstaller_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFontInstaller.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/fontInstaller-1.0.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadFontInstallerGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFontInstallerGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/cxziaho/fontInstaller/releases/latest/download/fontInstaller.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadFontRedirect_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFontRedirect.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/fontRedirect.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadFTPClient_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFTPClient.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/ftpclient-1.54.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadFTPClientGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFTPClientGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/cy33hc/vita-ftp-client/releases/latest/download/ftpclient.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadiTLSEnso_Click(sender As Object, e As RoutedEventArgs) Handles DownloadiTLSEnso.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/iTLS-Enso-3.2.1.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadiTLSEnsoGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadiTLSEnsoGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/SKGleba/iTLS-Enso/releases/latest/download/iTLS-Enso.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadMediaImporter_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMediaImporter.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/MediaImporter-0.91.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadMediaImporterGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMediaImporterGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/cnsldv/MediaImporter/releases/latest/download/MediaImporter.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadMoonlight_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMoonlight.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/moonlight-0.9.2.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadMoonlightGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMoonlightGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/xyzz/vita-moonlight/releases/latest/download/moonlight.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadMultidownloadVita_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMultidownloadVita.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/Multidownload-Vita-1.0.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadMultidownloadVitaGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMultidownloadVitaGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/DavisDev/Multidownload-Vita/releases/latest/download/Multidownload-Vita.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadMVPLAYER0_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMVPLAYER0.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/MVPLAYER0-1.3.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadMVPLAYER0GitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMVPLAYER0GitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/AntHJ/MVPlayer/releases/latest/download/MVPLAYER0.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadNetcheckBypass_Click(sender As Object, e As RoutedEventArgs) Handles DownloadNetcheckBypass.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/netcheck_bypass.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadNetcheckBypassGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadNetcheckBypassGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/yifanlu/netcheck_bypass/releases/latest/download/netcheck_bypass.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadNetStream_Click(sender As Object, e As RoutedEventArgs) Handles DownloadNetStream.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/NetStream-2.04.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadNetStreamGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadNetStreamGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/GrapheneCt/NetStream/releases/latest/download/NetStream.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadParentalControlBypass_Click(sender As Object, e As RoutedEventArgs) Handles DownloadParentalControlBypass.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/Parental_Control_Bypass.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub Downloadpkgj_Click(sender As Object, e As RoutedEventArgs) Handles Downloadpkgj.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/pkgj-0.57.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadpkgjGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadpkgjGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/blastrock/pkgj/releases/latest/download/pkgj.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPNGShot_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPNGShot.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/pngshot.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPNGShotGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPNGShotGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/xyzz/pngshot/releases/latest/download/pngshot.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadRegistryEditor_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRegistryEditor.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/RegistryEditor-1.0.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadRetroFlow_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroFlow.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/RetroFlow_v6.0.0.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadRetroFlowAdrenalineLauncher_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroFlowAdrenalineLauncher.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/RetroFlow_Adrenaline_Launcher_v3.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadRetroFlowAdrenalineLauncherGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroFlowAdrenalineLauncherGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/jimbob4000/RetroFlow-Launcher/releases/latest/download/RetroFlow_Adrenaline_Launcher_v3.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadRetroFlowGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroFlowGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/jimbob4000/RetroFlow-Launcher/releases/latest/download/RetroFlow_v6.0.0.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadSMBClient_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSMBClient.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/smbclient-1.04.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadSMBClientGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSMBClientGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/cy33hc/vita-smb-client/releases/latest/download/smbclient.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadUSBDisable_Click(sender As Object, e As RoutedEventArgs) Handles DownloadUSBDisable.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/USBDisable.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadUSBDisableGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadUSBDisableGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/Ibrahim778/USBDisable/releases/latest/download/USBDisable.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadUserAgentSpoofer_Click(sender As Object, e As RoutedEventArgs) Handles DownloadUserAgentSpoofer.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/UserAgentSpoofer.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadUserAgentSpooferGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadUserAgentSpooferGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/GrapheneCt/UserAgentSpoofer/releases/latest/download/UserAgentSpoofer.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadVITAlbum_Click(sender As Object, e As RoutedEventArgs) Handles DownloadVITAlbum.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/VITAlbum-1.40.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadVITAlbumGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadVITAlbumGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/joel16/VITAlbum/releases/latest/download/VITAlbum.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadVitaMediaPlayer_Click(sender As Object, e As RoutedEventArgs) Handles DownloadVitaMediaPlayer.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/VitaMediaPlayer-1.01.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadVitaMediaPlayerGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadVitaMediaPlayerGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/SonicMastr/Vita-Media-Player/releases/latest/download/VitaMediaPlayer.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadVitaShell_Click(sender As Object, e As RoutedEventArgs) Handles DownloadVitaShell.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/VitaShell-2.02.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadVitaShellGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadVitaShellGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/TheOfficialFloW/VitaShell/releases/latest/download/VitaShell.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadWebDAVClient_Click(sender As Object, e As RoutedEventArgs) Handles DownloadWebDAVClient.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/webdavclient-1.02.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadWebDAVClientGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadWebDAVClientGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/cy33hc/vita-webdav-client/releases/latest/download/webdavclient.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloaTrophiesFixer_Click(sender As Object, e As RoutedEventArgs) Handles DownloaTrophiesFixer.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/trophies_fixer-1.1.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloaTrophiesFixerGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloaTrophiesFixerGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/Yoti/psv_trophfix/releases/latest/download/trophies_fixer.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadBlasphemousPort_Click(sender As Object, e As RoutedEventArgs) Handles DownloadBlasphemousPort.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/BlasphemousVita.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadCupheadPort_Click(sender As Object, e As RoutedEventArgs) Handles DownloadCupheadPort.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/Cuphead.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadGTACTWPort_Click(sender As Object, e As RoutedEventArgs) Handles DownloadGTACTWPort.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/GTACTW.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadNoNpDrm_Click(sender As Object, e As RoutedEventArgs) Handles DownloadNoNpDrm.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/nonpdrm.skprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadNoNpDrmGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadNoNpDrmGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/TheOfficialFloW/NoNpDrm/releases/latest/download/nonpdrm.skprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadreF00D_Click(sender As Object, e As RoutedEventArgs) Handles DownloadreF00D.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/reF00D.skprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadreF00DGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadreF00DGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/dots-tb/reF00D/releases/latest/download/reF00D.skprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadSonicManiaPort_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSonicManiaPort.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/SonicMania.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#Region "Firmwares"

    Private Sub DownloadOFW365_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOFW365.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/fw/OFW3.65.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#Region "Emulators"

    Private Sub DownloadDaedalusX64_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDaedalusX64.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/DaedalusX64.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadDaedalusX64Github_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDaedalusX64Github.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/Rinnegatamante/DaedalusX64-vitaGL/releases/latest/download/DaedalusX64.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadFlycast_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFlycast.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/Flycast-1.1.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadFlycastGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFlycastGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/Rinnegatamante/flycast-vita/releases/latest/download/Flycast.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadmGBA_Click(sender As Object, e As RoutedEventArgs) Handles DownloadmGBA.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/mGBA-0.10.1-vita.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadRetroArch_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroArch.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/RetroArch.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadRetroArchData_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroArchData.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/RetroArch_data.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadSnes9xVITA_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSnes9xVITA.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/Snes9xVITA.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadSnes9xVITAGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSnes9xVITAGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("https://github.com/theheroGAC/Snes9xVITA/releases/download/latest/Snes9xVITA.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#End Region

#Region "Tools"

    Private Sub OpenPKGExtractor_Click(sender As Object, e As RoutedEventArgs) Handles OpenPKGExtractor.Click
        Dim NewPKGExtractor As New PKGExtractor() With {.ShowActivated = True}
        NewPKGExtractor.Show()
    End Sub

    Private Sub OpenRCOExtractor_Click(sender As Object, e As RoutedEventArgs) Handles OpenRCOExtractor.Click
        Dim NewPS5RcoExtractor As New PS5RcoExtractor() With {.ShowActivated = True}
        NewPS5RcoExtractor.Show()
    End Sub

    Private Sub OpenIMGTools_Click(sender As Object, e As RoutedEventArgs) Handles OpenIMGTools.Click
        'Dim NewPSVIMGTools As New PSVIMGTools() With {.ShowActivated = True}
        'NewPSVIMGTools.Show()
        MsgBox("Not ready yet", MsgBoxStyle.Information)
    End Sub

    Private Sub OpenParamSFOEditorMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenParamSFOEditorMenuItem.Click
        Dim NewSFOEditor As New SFOEditor() With {.ShowActivated = True}
        NewSFOEditor.Show()
    End Sub

    Private Sub OpenPFSToolsMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenPFSToolsMenuItem.Click
        Dim NewPSVPFSTools As New PSVPFSTools() With {.ShowActivated = True}
        NewPSVPFSTools.Show()
    End Sub

#End Region

End Class
