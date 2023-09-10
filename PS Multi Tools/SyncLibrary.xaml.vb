Imports System.ComponentModel
Imports System.IO
Imports System.Net

Public Class SyncLibrary

    Dim WithEvents DownloadClient As New WebClient()
    Dim NewVersion As String = String.Empty
    Dim Changelog As String = String.Empty

    Private Sub SyncLibrary_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        LoadStatusTextBlock.Text = "Updating PS Multi Tools Library"
        CheckLibraryUpdate()
    End Sub

    Public Sub CheckLibraryUpdate()
        Dim LibraryInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(My.Computer.FileSystem.CurrentDirectory + "\psmt-lib.dll")
        Dim CurrentLibraryVersion As String = LibraryInfo.FileVersion

        Dim VerCheckClient As New WebClient()
        NewVersion = VerCheckClient.DownloadString("http://X.X.X.X/psmt-lib.txt")
        Changelog = VerCheckClient.DownloadString("http://X.X.X.X/changelog.txt")

        Try
            File.Delete(My.Computer.FileSystem.CurrentDirectory + "\psmt-lib.dll")
            DownloadClient.DownloadFileAsync(New Uri("http://X.X.X.X/psmt-lib.dll"), My.Computer.FileSystem.CurrentDirectory + "\psmt-lib.dll")
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub DownloadClient_DownloadProgressChanged(sender As Object, e As DownloadProgressChangedEventArgs) Handles DownloadClient.DownloadProgressChanged
        LoadProgressBar.Value = e.ProgressPercentage
    End Sub

    Private Sub DownloadClient_DownloadFileCompleted(sender As Object, e As AsyncCompletedEventArgs) Handles DownloadClient.DownloadFileCompleted
        If MsgBox("Library update " + NewVersion + " installed." + vbCrLf + "PS Multi Tools will restart." + vbCrLf + vbCrLf + Changelog, MsgBoxStyle.OkOnly, "Update") = MsgBoxResult.Ok Then
            Process.Start(Windows.Application.ResourceAssembly.Location)
            Windows.Application.Current.Shutdown()
        End If
    End Sub

End Class
