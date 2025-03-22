Public Class PSVMenu

    Public LView As Controls.ListView
    Public GamesLView As Controls.ListView

#Region "Menu Downloads"

#Region "Homebrew"

    Private Async Sub DownloadAdrenaline_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAdrenaline.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/Adrenaline-7.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadAdrenalineBubbleManager_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAdrenalineBubbleManager.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/AdrenalineBubbleManager-6.19.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadAdrenalineBubbleManagerGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAdrenalineBubbleManagerGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/ONElua/AdrenalineBubbleManager/releases/latest/download/AdrenalineBubbleManager.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadAdrenalineGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAdrenalineGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/TheOfficialFloW/Adrenaline/releases/latest/download/Adrenaline.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadAdrenalineStates_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAdrenalineStates.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/AdrenalineStates.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadApollo_Click(sender As Object, e As RoutedEventArgs) Handles DownloadApollo.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/apollo-vita-1.2.4.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadApolloGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadApolloGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/bucanero/apollo-vita/releases/latest/download/apollo-vita.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadAppDBTool_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAppDBTool.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/appdbtool.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadDownloadEnabler_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDownloadEnabler.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/download_enabler.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadDownloadEnablerGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDownloadEnablerGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/TheOfficialFloW/VitaTweaks/releases/latest/download/download_enabler.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadElevenMPV_Click(sender As Object, e As RoutedEventArgs) Handles DownloadElevenMPV.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/ElevenMPV-A-7.10.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadElevenMPVGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadElevenMPVGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/GrapheneCt/ElevenMPV-A/releases/latest/download/ElevenMPV-A.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadFontInstaller_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFontInstaller.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/fontInstaller-1.0.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadFontInstallerGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFontInstallerGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/cxziaho/fontInstaller/releases/latest/download/fontInstaller.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadFontRedirect_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFontRedirect.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/fontRedirect.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadFTPClient_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFTPClient.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/ftpclient-1.54.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadFTPClientGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFTPClientGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/cy33hc/vita-ftp-client/releases/latest/download/ftpclient.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadiTLSEnso_Click(sender As Object, e As RoutedEventArgs) Handles DownloadiTLSEnso.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/iTLS-Enso-3.2.1.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadiTLSEnsoGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadiTLSEnsoGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/SKGleba/iTLS-Enso/releases/latest/download/iTLS-Enso.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadMediaImporter_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMediaImporter.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/MediaImporter-0.91.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadMediaImporterGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMediaImporterGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/cnsldv/MediaImporter/releases/latest/download/MediaImporter.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadMoonlight_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMoonlight.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/moonlight-0.9.2.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadMoonlightGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMoonlightGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/xyzz/vita-moonlight/releases/latest/download/moonlight.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadMultidownloadVita_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMultidownloadVita.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/Multidownload-Vita-1.0.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadMultidownloadVitaGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMultidownloadVitaGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/DavisDev/Multidownload-Vita/releases/latest/download/Multidownload-Vita.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadMVPLAYER0_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMVPLAYER0.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/MVPLAYER0-1.3.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadMVPLAYER0GitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMVPLAYER0GitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/AntHJ/MVPlayer/releases/latest/download/MVPLAYER0.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadNetcheckBypass_Click(sender As Object, e As RoutedEventArgs) Handles DownloadNetcheckBypass.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/netcheck_bypass.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadNetcheckBypassGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadNetcheckBypassGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/yifanlu/netcheck_bypass/releases/latest/download/netcheck_bypass.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadNetStream_Click(sender As Object, e As RoutedEventArgs) Handles DownloadNetStream.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/NetStream-2.04.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadNetStreamGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadNetStreamGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/GrapheneCt/NetStream/releases/latest/download/NetStream.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadParentalControlBypass_Click(sender As Object, e As RoutedEventArgs) Handles DownloadParentalControlBypass.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/Parental_Control_Bypass.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub Downloadpkgj_Click(sender As Object, e As RoutedEventArgs) Handles Downloadpkgj.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/pkgj-0.57.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadpkgjGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadpkgjGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/blastrock/pkgj/releases/latest/download/pkgj.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadPNGShot_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPNGShot.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/pngshot.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadPNGShotGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPNGShotGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/xyzz/pngshot/releases/latest/download/pngshot.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadRegistryEditor_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRegistryEditor.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/RegistryEditor-1.0.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadRetroFlow_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroFlow.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/RetroFlow_v6.0.0.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadRetroFlowAdrenalineLauncher_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroFlowAdrenalineLauncher.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/RetroFlow_Adrenaline_Launcher_v3.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadRetroFlowAdrenalineLauncherGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroFlowAdrenalineLauncherGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/jimbob4000/RetroFlow-Launcher/releases/latest/download/RetroFlow_Adrenaline_Launcher_v3.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadRetroFlowGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroFlowGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/jimbob4000/RetroFlow-Launcher/releases/latest/download/RetroFlow_v6.0.0.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadSMBClient_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSMBClient.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/smbclient-1.04.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadSMBClientGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSMBClientGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/cy33hc/vita-smb-client/releases/latest/download/smbclient.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadUSBDisable_Click(sender As Object, e As RoutedEventArgs) Handles DownloadUSBDisable.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/USBDisable.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadUSBDisableGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadUSBDisableGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/Ibrahim778/USBDisable/releases/latest/download/USBDisable.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadUserAgentSpoofer_Click(sender As Object, e As RoutedEventArgs) Handles DownloadUserAgentSpoofer.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/UserAgentSpoofer.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadUserAgentSpooferGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadUserAgentSpooferGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/GrapheneCt/UserAgentSpoofer/releases/latest/download/UserAgentSpoofer.suprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadVITAlbum_Click(sender As Object, e As RoutedEventArgs) Handles DownloadVITAlbum.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/VITAlbum-1.40.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadVITAlbumGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadVITAlbumGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/joel16/VITAlbum/releases/latest/download/VITAlbum.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadVitaMediaPlayer_Click(sender As Object, e As RoutedEventArgs) Handles DownloadVitaMediaPlayer.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/VitaMediaPlayer-1.01.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadVitaMediaPlayerGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadVitaMediaPlayerGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/SonicMastr/Vita-Media-Player/releases/latest/download/VitaMediaPlayer.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadVitaShell_Click(sender As Object, e As RoutedEventArgs) Handles DownloadVitaShell.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/VitaShell-2.02.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadVitaShellGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadVitaShellGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/TheOfficialFloW/VitaShell/releases/latest/download/VitaShell.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadWebDAVClient_Click(sender As Object, e As RoutedEventArgs) Handles DownloadWebDAVClient.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/webdavclient-1.02.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadWebDAVClientGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadWebDAVClientGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/cy33hc/vita-webdav-client/releases/latest/download/webdavclient.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadTrophiesFixer_Click(sender As Object, e As RoutedEventArgs) Handles DownloadTrophiesFixer.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/trophies_fixer-1.1.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloaTrophiesFixerGitHub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadTrophiesFixerGitHub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/Yoti/psv_trophfix/releases/latest/download/trophies_fixer.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadBlasphemousPort_Click(sender As Object, e As RoutedEventArgs) Handles DownloadBlasphemousPort.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/BlasphemousVita.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadCupheadPort_Click(sender As Object, e As RoutedEventArgs) Handles DownloadCupheadPort.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/Cuphead.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadGTACTWPort_Click(sender As Object, e As RoutedEventArgs) Handles DownloadGTACTWPort.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/GTACTW.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadNoNpDrm_Click(sender As Object, e As RoutedEventArgs) Handles DownloadNoNpDrm.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/nonpdrm.skprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadNoNpDrmGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadNoNpDrmGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/TheOfficialFloW/NoNpDrm/releases/latest/download/nonpdrm.skprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadreF00D_Click(sender As Object, e As RoutedEventArgs) Handles DownloadreF00D.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/reF00D.skprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadreF00DGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadreF00DGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/dots-tb/reF00D/releases/latest/download/reF00D.skprx") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadSonicManiaPort_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSonicManiaPort.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/hb/SonicMania.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#Region "Firmwares"

    Private Async Sub DownloadOFW365_Click(sender As Object, e As RoutedEventArgs) Handles DownloadOFW365.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/fw/OFW3.65.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#Region "Emulators"

    Private Async Sub DownloadDaedalusX64_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDaedalusX64.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/DaedalusX64.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadDaedalusX64Github_Click(sender As Object, e As RoutedEventArgs) Handles DownloadDaedalusX64Github.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/Rinnegatamante/DaedalusX64-vitaGL/releases/latest/download/DaedalusX64.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadFlycast_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFlycast.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/Flycast-1.1.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadFlycastGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFlycastGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/Rinnegatamante/flycast-vita/releases/latest/download/Flycast.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadmGBA_Click(sender As Object, e As RoutedEventArgs) Handles DownloadmGBA.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/mGBA-0.10.1-vita.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadRetroArch_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroArch.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/RetroArch.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadRetroArchData_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroArchData.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/RetroArch_data.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadSnes9xVITA_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSnes9xVITA.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("http://X.X.X.X/vita/emu/Snes9xVITA.vpk") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Async Sub DownloadSnes9xVITAGithub_Click(sender As Object, e As RoutedEventArgs) Handles DownloadSnes9xVITAGithub.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If Await NewDownloader.CreateNewDownload("https://github.com/theheroGAC/Snes9xVITA/releases/download/latest/Snes9xVITA.vpk") = False Then
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
