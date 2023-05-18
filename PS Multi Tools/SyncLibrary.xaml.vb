Imports System.ComponentModel
Imports System.IO
Imports System.Net

Public Class SyncLibrary

    Public WithEvents DownloadClient As New WebClient()
    Dim NewVersion As String = String.Empty
    Dim Changelog As String = String.Empty

    Private Sub SyncLibrary_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If File.Exists("psmt-lib.dll") Then
            If MsgBox("Check for updates ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                CheckLibraryUpdate()
            Else
                Dim NewMainWindow As New MainWindow() With {.ShowActivated = True}
                NewMainWindow.Show()
                Close()
            End If
        Else
            If MsgBox("PS Multi Tools Library not found, do you want to re-download it?") = MsgBoxResult.Yes Then
                DownloadClient.DownloadFileAsync(New Uri("http://X.X.X.X/psmt-lib.dll"), My.Computer.FileSystem.CurrentDirectory + "\psmt-lib.dll")
            Else
                Dim NewMainWindow As New MainWindow() With {.ShowActivated = True}
                NewMainWindow.Show()
                Close()
            End If
        End If
    End Sub

    Public Sub CheckLibraryUpdate()
        Dim FI As FileVersionInfo = FileVersionInfo.GetVersionInfo(My.Computer.FileSystem.CurrentDirectory + "\psmt-lib.dll")
        Dim Ver1 As String = FI.FileVersion

        Dim VerCheckClient As New WebClient()
        Dim Ver2 As String = VerCheckClient.DownloadString("http://X.X.X.X/psmt-lib.txt")
        Changelog = VerCheckClient.DownloadString("http://X.X.X.X/changelog.txt")

        If Ver1 < Ver2 Then
            'Update found
            NewVersion = Ver2
            LoadProgressBar.IsIndeterminate = False
            LoadStatusTextBlock.Text = "Updating PS Multi Tools Lib"

            Try
                File.Delete(My.Computer.FileSystem.CurrentDirectory + "\psmt-lib.dll")
                DownloadClient.DownloadFileAsync(New Uri("http://X.X.X.X/psmt-lib.dll"), My.Computer.FileSystem.CurrentDirectory + "\psmt-lib.dll")
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        Else
            Dim NewMainWindow As New MainWindow() With {.ShowActivated = True}
            NewMainWindow.Show()
            Close()
        End If
    End Sub

    Private Sub DownloadClient_DownloadProgressChanged(sender As Object, e As DownloadProgressChangedEventArgs) Handles DownloadClient.DownloadProgressChanged
        LoadProgressBar.Value = e.ProgressPercentage
    End Sub

    Private Sub DownloadClient_DownloadFileCompleted(sender As Object, e As AsyncCompletedEventArgs) Handles DownloadClient.DownloadFileCompleted
        If MsgBox("Library Update " + NewVersion + " installed." + vbCrLf + vbCrLf + Changelog, MsgBoxStyle.OkOnly, "PS Multi Tools Library Update") = MsgBoxResult.Ok Then
            Process.Start(Application.ResourceAssembly.Location)
            Application.Current.Shutdown()
        End If
    End Sub

End Class
