Public Class PS1Menu

    Public GamesLView As ListView

#Region "Menu Downloads"

#Region "Exploits"

    Private Sub DownloadFreePSXBoot_Click(sender As Object, e As RoutedEventArgs) Handles DownloadFreePSXBoot.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps1/freepsxboot-2.1.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadTonyHax_Click(sender As Object, e As RoutedEventArgs) Handles DownloadTonyHax.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps1/tonyhax-v1.4.5.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadTonyHaxInternational_Click(sender As Object, e As RoutedEventArgs) Handles DownloadTonyHaxInternational.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps1/tonyhax-international-v1.5.1.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#Region "Tools"


    Private Sub DownloadAprip_Click(sender As Object, e As RoutedEventArgs) Handles DownloadAprip.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps1/aprip-v1.0.9-windows-x86_64-static.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadEDCRE_Click(sender As Object, e As RoutedEventArgs) Handles DownloadEDCRE.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps1/edcre-v1.0.6-windows-x86_64.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadLibCryptPatcher_Click(sender As Object, e As RoutedEventArgs) Handles DownloadLibCryptPatcher.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps1/libcrypt-patcher-v1.0.8-windows-x86_64-static.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPSEXE2ROM_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPSEXE2ROM.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps1/psexe2rom_1.0.2_windows_x86_64.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Private Sub DownloadPSX80MP_Click(sender As Object, e As RoutedEventArgs) Handles DownloadPSX80MP.Click
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("http://X.X.X.X/ps1/psx80mp-v2.0-windows-x86_64.zip") = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

#End Region

#End Region

#Region "Tools"

    Private Sub OpenMergeBinTool_Click(sender As Object, e As RoutedEventArgs) Handles OpenMergeBinTool.Click
        Dim NewMergeBINTool As New MergeBinTool() With {.ShowActivated = True}
        NewMergeBINTool.Show()
    End Sub

    Private Sub OpenBINCUEConverter_Click(sender As Object, e As RoutedEventArgs) Handles OpenBINCUEConverter.Click
        Dim NewBINCUEConverter As New BINCUEConverter() With {.ShowActivated = True, .ConvertForPS1 = True}
        NewBINCUEConverter.Show()
    End Sub

#End Region

End Class
