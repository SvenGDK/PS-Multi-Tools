Imports System.ComponentModel

Public Class HomeWindow

    Private Sub HomeWindow_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        Title = String.Format("Multi Tools - v{0}.{1}.{2} - {3}", My.Application.Info.Version.Major, My.Application.Info.Version.Minor, My.Application.Info.Version.Build, Text.Encoding.ASCII.GetString(Utils.ByBytes))
    End Sub

    Private Sub HomeWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Windows.Application.Current.Shutdown()
    End Sub

    Private Sub PS1Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS1Image.MouseLeftButtonDown
        Dim NewPS1Library As New PS1Library() With {.ShowActivated = True}
        NewPS1Library.Show()
    End Sub

    Private Sub PS1TextBlock_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS1TextBlock.MouseLeftButtonDown
        Dim NewPS1Library As New PS1Library() With {.ShowActivated = True}
        NewPS1Library.Show()
    End Sub

    Private Sub PS2Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS2Image.MouseLeftButtonDown
        Dim NewPS2Library As New PS2Library() With {.ShowActivated = True}
        NewPS2Library.Show()
    End Sub

    Private Sub PS2TextBlock_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS2TextBlock.MouseLeftButtonDown
        Dim NewPS2Library As New PS2Library() With {.ShowActivated = True}
        NewPS2Library.Show()
    End Sub

    Private Sub PS3Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS3Image.MouseLeftButtonDown
        Dim NewPS3Library As New PS3Library() With {.ShowActivated = True}
        NewPS3Library.Show()
    End Sub

    Private Sub PS3TextBlock_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS3TextBlock.MouseLeftButtonDown
        Dim NewPS3Library As New PS3Library() With {.ShowActivated = True}
        NewPS3Library.Show()
    End Sub

    Private Sub PS4Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS4Image.MouseLeftButtonDown
        Dim NewPS4Library As New PS4Library() With {.ShowActivated = True}
        NewPS4Library.Show()
    End Sub

    Private Sub PS4TextBlock_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS4TextBlock.MouseLeftButtonDown
        Dim NewPS4Library As New PS4Library() With {.ShowActivated = True}
        NewPS4Library.Show()
    End Sub

    Private Sub PS5Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS5Image.MouseLeftButtonDown
        Dim NewPS5Library As New PS5Library() With {.ShowActivated = True}
        NewPS5Library.Show()
    End Sub

    Private Sub PS5TextBlock_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS5TextBlock.MouseLeftButtonDown
        Dim NewPS5Library As New PS5Library() With {.ShowActivated = True}
        NewPS5Library.Show()
    End Sub

    Private Sub PSPImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PSPImage.MouseLeftButtonDown
        Dim NewPSPLibrary As New PSPLibrary() With {.ShowActivated = True}
        NewPSPLibrary.Show()
    End Sub

    Private Sub PSPTextBlock_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PSPTextBlock.MouseLeftButtonDown
        Dim NewPSPLibrary As New PSPLibrary() With {.ShowActivated = True}
        NewPSPLibrary.Show()
    End Sub

    Private Sub PSVImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PSVImage.MouseLeftButtonDown
        Dim NewPSVLibrary As New PSVLibrary() With {.ShowActivated = True}
        NewPSVLibrary.Show()
    End Sub

    Private Sub PSVTextBlock_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PSVTextBlock.MouseLeftButtonDown
        Dim NewPSVLibrary As New PSVLibrary() With {.ShowActivated = True}
        NewPSVLibrary.Show()
    End Sub

    Private Sub PSXImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PSXImage.MouseLeftButtonDown
        If Utils.IsRunningAsAdministrator() Then
            Dim NewPSXProjectManager As New PSXMainWindow() With {.ShowActivated = True}
            NewPSXProjectManager.Show()
        Else
            If MsgBox("This feature requires Administrator rights." + vbCrLf + "Do you want to restart PS Multi Tools as Administrator ?", MsgBoxStyle.YesNo, "Elevation required") = MsgBoxResult.Yes Then
                Utils.RunAsAdministrator()
            End If
        End If
    End Sub

    Private Sub PSXTextBlock_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PSXTextBlock.MouseLeftButtonDown
        If Utils.IsRunningAsAdministrator() Then
            Dim NewPSXProjectManager As New PSXMainWindow() With {.ShowActivated = True}
            NewPSXProjectManager.Show()
        Else
            If MsgBox("This feature requires Administrator rights." + vbCrLf + "Do you want to restart PS Multi Tools as Administrator ?", MsgBoxStyle.YesNo, "Elevation required") = MsgBoxResult.Yes Then
                Utils.RunAsAdministrator()
            End If
        End If
    End Sub

    Private Sub UpdateImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles UpdateImage.MouseLeftButtonDown
        If Utils.IsPSMultiToolsUpdateAvailable() Then
            If MsgBox("An update is available, do you want to download it now ?", MsgBoxStyle.YesNo, "PS Multi Tools Update found") = MsgBoxResult.Yes Then
                Utils.DownloadAndExecuteUpdater()
            End If
        Else
            MsgBox("PS Multi Tools is up to date!", MsgBoxStyle.Information, "No update found")
        End If
    End Sub

    Private Sub UpdateTextBlock_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles UpdateTextBlock.MouseLeftButtonDown
        If Utils.IsPSMultiToolsUpdateAvailable() Then
            If MsgBox("An update is available, do you want to download it now ?", MsgBoxStyle.YesNo, "PS Multi Tools Update found") = MsgBoxResult.Yes Then
                Utils.DownloadAndExecuteUpdater()
            End If
        Else
            MsgBox("PS Multi Tools is up to date!", MsgBoxStyle.Information, "No update found")
        End If
    End Sub

End Class
