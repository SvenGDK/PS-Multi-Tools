Public Class PSPMenu


#Region "Menu Downloads"

#Region "Homebrew"

    Private Sub DownloadAlphabase_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAlphabase.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/alphabase-3.6.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadBeatBox_Click(sender As Object, e As RoutedEventArgs) Handles DownloadBeatBox.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/beatbox-1.6.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadBookr_Click(sender As Object, e As RoutedEventArgs) Handles DownloadBookr.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/bookr-8.2.0.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadCex2DexConverter_Click(sender As Object, e As RoutedEventArgs) Handles DownloadCex2DexConverter.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/Cex2DexConverter-1.1.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadCMFileManager_Click(sender As Object, e As RoutedEventArgs) Handles DownloadCMFileManager.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/CMFileManager-PSP.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadCMFileManagerGo_Click(sender As Object, e As RoutedEventArgs) Handles DownloadCMFileManagerGo.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/CMFileManager-PSP-GO-M2.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadCyanogenPSP_Click(sender As Object, e As RoutedEventArgs) Handles DownloadCyanogenPSP.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/cyanogenpsp-6.1Final.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadFlashlight_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFlashlight.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/flashlightpsp.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadIDPSDumper_Click(sender As Object, e As RoutedEventArgs) Handles DownloadIDPSDumper.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/PSP_IDPS_Dumper_v0.9.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPaintLite_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPaintLite.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/paintlitepsp-0.3.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadprxDecrypter_Click(sender As Object, e As RoutedEventArgs) Handles DownloadprxDecrypter.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/prxdecrypter-2.7a.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPSPIdent_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPSPIdent.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/pspident-0.75.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadQwikMove_Click(sender As Object, e As RoutedEventArgs) Handles DownloadQwikMove.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/qwikmovepsp-v2.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadRebootPSP_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRebootPSP.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/rebootpsp.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadShutdownPSP_Click(sender As Object, e As RoutedEventArgs) Handles DownloadShutdownPSP.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/shutdownpsp.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadWifiHack_Click(sender As Object, e As RoutedEventArgs) Handles DownloadWifiHack.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/hb/wifihack-2.1.rar") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#Region "Emulators"

    Private Sub DownloadRetroArch_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRetroArch.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/emu/RetroArch-1.15.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#Region "Firmwares"

    Private Sub Download100PBP_Click(sender As Object, e As RoutedEventArgs) Handles Download100PBP.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/fw/100.PBP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub Download360Archive_Click(sender As Object, e As RoutedEventArgs) Handles Download360Archive.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/fw/360.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub Download360PSAR_Click(sender As Object, e As RoutedEventArgs) Handles Download360PSAR.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/fw/360.PSAR") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub Download570PSARGo_Click(sender As Object, e As RoutedEventArgs) Handles Download570PSARGo.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/fw/570go.PSAR") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub Download661PBP_Click(sender As Object, e As RoutedEventArgs) Handles Download661PBP.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/fw/661.PBP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub Download661PBPGo_Click(sender As Object, e As RoutedEventArgs) Handles Download661PBPGo.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/fw/661go.PBP") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#Region "Tools"

    Private Sub DownloadISOPBPConverter_Click(sender As Object, e As RoutedEventArgs) Handles DownloadISOPBPConverter.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/tools/isopbpconverter0.1.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPBPUnpacker_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPBPUnpacker.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/tools/pbpunpackerrea-1.2.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPBPViewer_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPBPViewer.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/tools/pbpviewerlma-0.1.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPSPDisp_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPSPDisp.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/tools/pspdisp0.6.1.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadRCOMage_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRCOMage.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/tools/rcomage-1.1.1.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadRemoteJoyLite_Click(sender As Object, e As RoutedEventArgs) Handles DownloadRemoteJoyLite.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/psp/tools/remotejoylite-20a.7z") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#End Region

#Region "Tools"

    Private Sub OpenISOCISOConverter_Click(sender As Object, e As RoutedEventArgs) Handles OpenISOCISOConverter.Click
        Dim NewCISOConverter As New CISOConverter() With {.ShowActivated = True}
        NewCISOConverter.Show()
    End Sub

    Private Sub OpenPBPISOConverter_Click(sender As Object, e As RoutedEventArgs) Handles OpenPBPISOConverter.Click
        Dim NewPBPISOConverter As New PBPISOConverter() With {.ShowActivated = True}
        NewPBPISOConverter.Show()
    End Sub

    Private Sub OpenPBPPacker_Click(sender As Object, e As RoutedEventArgs) Handles OpenPBPPacker.Click
        Dim NewPBPPacker As New PBPPacker() With {.ShowActivated = True}
        NewPBPPacker.Show()
    End Sub

    Private Sub OpenSFOEditor_Click(sender As Object, e As RoutedEventArgs) Handles OpenSFOEditor.Click
        Dim NewSFOEditor As New SFOEditor() With {.ShowActivated = True}
        NewSFOEditor.Show()
    End Sub

#End Region


End Class
