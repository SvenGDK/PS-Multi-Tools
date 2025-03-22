Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates
Imports Microsoft.Win32

Public Class Downloader

    Dim TimerID As IntPtr = Nothing
    Dim DownloadSpeed As Integer = 0
    Dim MaximumSpeed As Integer = 0
    Dim AverageSpeed As Integer = 0
    Dim LoopCount As Integer = 0
    Dim ByteCount As Integer = 0
    Dim CurrentBytes As Long = 0
    Dim PreviousBytes As Long = 0
    Dim DownloadSize As Long = 0
    Dim StartTime As Long = 0
    Dim ElapsedTime As TimeSpan = Nothing
    Dim TimeLeft As TimeSpan = Nothing
    Dim TimeLeftAverage As Double = 0
    Dim DownloadIcon As ImageSource = Nothing

    Dim WithEvents DownloadClient As New WebClient()
    Public PackageConsole As String
    Public PackageTitleID As String
    Public PackageContentID As String

    Public DownloadQueueItem As DownloadQueueItem = Nothing

    Public Sub New()
        InitializeComponent()

        TimerID = Nothing
        CurrentBytes = 0
        PreviousBytes = 0
        DownloadSpeed = 0
        MaximumSpeed = 0
        AverageSpeed = 0
        LoopCount = 0
        ByteCount = 0
        DownloadProgressBar.Value = 0

        AddHandler SystemEvents.TimerElapsed, AddressOf DownloadUpdating
    End Sub

    Public Async Function CreateNewDownload(Source As String, Optional ModifyName As Boolean = False, Optional NewName As String = "") As Task(Of Boolean)
        'Create Downloads directory if not exists
        If Not Directory.Exists(Environment.CurrentDirectory + "\Downloads") Then Directory.CreateDirectory(Environment.CurrentDirectory + "\Downloads")

        If Dispatcher.CheckAccess() = False Then
            Await Dispatcher.BeginInvoke(Sub() If DownloadIcon IsNot Nothing Then DownloadImage.Source = DownloadIcon)
        Else
            If DownloadIcon IsNot Nothing Then DownloadImage.Source = DownloadIcon
        End If

        Dim FileName As String = Utils.GetFilenameFromUrl(New Uri(Source))
        If Not String.IsNullOrEmpty(FileName) Then
            TimerID = SystemEvents.CreateTimer(1000)
            StartTime = Now.Ticks

            'Change the file name for .pkgs
            If ModifyName = True And Not String.IsNullOrEmpty(NewName) Then
                FileName = NewName
            End If

            Dim URLFileSize As Double = Await Utils.WebFileSize(Source)

            If Dispatcher.CheckAccess() = False Then
                Await Dispatcher.BeginInvoke(Sub()
                                                 DownloadFileSizeTB.Text = "File Size: " + URLFileSize.ToString + " MB"
                                                 FileToDownloadTB.Text = "Downloading " + FileName + " ..."
                                             End Sub)
            Else
                DownloadFileSizeTB.Text = "File Size: " + URLFileSize.ToString + " MB"
                FileToDownloadTB.Text = "Downloading " + FileName + " ..."
            End If

            Await DownloadClient.DownloadFileTaskAsync(New Uri(Source), Environment.CurrentDirectory + "\Downloads\" + FileName)
            Return True
        Else
            Return False
        End If
    End Function

    Public Async Sub DownloadUpdating(sender As Object, e As TimerElapsedEventArgs)
        DownloadSpeed = CInt(CurrentBytes - PreviousBytes)
        ElapsedTime = TimeSpan.FromTicks(Now.Ticks - StartTime)

        If Dispatcher.CheckAccess() = False Then
            Await Dispatcher.BeginInvoke(Sub()
                                             DownloadETATB.Text = "Time elapsed: " + String.Format("{0:00}h {1:00}m {2:00}s", ElapsedTime.TotalHours, ElapsedTime.Minutes, ElapsedTime.Seconds)

                                             If DownloadSpeed < 1 Then
                                                 DownloadSpeedTB.Text = "< 1 KB/s"
                                             Else
                                                 DownloadSpeedTB.Text = FormatNumber(DownloadSpeed / 1024 / 1024, 2).ToString & " MB/s"
                                             End If

                                             If Not DownloadSpeed < 1 Then
                                                 LoopCount += 1
                                                 ByteCount += DownloadSpeed

                                                 TimeLeftAverage = ElapsedTime.TotalSeconds / CurrentBytes
                                                 TimeLeft = TimeSpan.FromSeconds(TimeLeftAverage * (DownloadSize - CurrentBytes))

                                                 DownloadETALeftTB.Text = "Time left: " + String.Format("{0:00}h {1:00}m {2:00}s", TimeLeft.TotalHours, TimeLeft.Minutes, TimeLeft.Seconds)
                                             End If

                                         End Sub)
        Else
            DownloadETATB.Text = "Time elapsed: " + String.Format("{0:00}h {1:00}m {2:00}s", ElapsedTime.TotalHours, ElapsedTime.Minutes, ElapsedTime.Seconds)

            If DownloadSpeed < 1 Then
                DownloadSpeedTB.Text = "< 1 KB/s"
            Else
                DownloadSpeedTB.Text = FormatNumber(DownloadSpeed / 1024 / 1024, 2).ToString & " MB/s"
            End If

            If Not DownloadSpeed < 1 Then
                LoopCount += 1
                ByteCount += DownloadSpeed

                TimeLeftAverage = ElapsedTime.TotalSeconds / CurrentBytes
                TimeLeft = TimeSpan.FromSeconds(TimeLeftAverage * (DownloadSize - CurrentBytes))

                DownloadETALeftTB.Text = "Time left: " + String.Format("{0:00}h {1:00}m {2:00}s", TimeLeft.TotalHours, TimeLeft.Minutes, TimeLeft.Seconds)
            End If
        End If

        PreviousBytes = CurrentBytes
    End Sub

    Private Sub DownloadClient_DownloadProgressChanged(sender As Object, e As DownloadProgressChangedEventArgs) Handles DownloadClient.DownloadProgressChanged
        DownloadProgressBar.Value = e.ProgressPercentage
        DownloadSize = e.TotalBytesToReceive
        CurrentBytes = e.BytesReceived
    End Sub

    Private Sub DownloadClient_DownloadFileCompleted(sender As Object, e As AsyncCompletedEventArgs) Handles DownloadClient.DownloadFileCompleted
        If e.Cancelled Then
            Try
                SystemEvents.KillTimer(TimerID)
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        Else
            SystemEvents.KillTimer(TimerID)

            If FileToDownloadTB.Dispatcher.CheckAccess() = False Then
                FileToDownloadTB.Dispatcher.BeginInvoke(Sub() FileToDownloadTB.Text = "Download finished")
            Else
                FileToDownloadTB.Text = "Download finished"
            End If

            'For PS5 game patches
            If DownloadQueueItem IsNot Nothing Then
                If Not String.IsNullOrEmpty(DownloadQueueItem.FileName) Then
                    'Update progress in PS5GamePatches (if open)
                    Dim OpenGamePatchesWindow As PS5GamePatches
                    For Each OpenWin In System.Windows.Application.Current.Windows()
                        If OpenWin.ToString = "psmt_lib.PS5GamePatches" Then
                            OpenGamePatchesWindow = CType(OpenWin, PS5GamePatches)

                            For Each DownloadItem In OpenGamePatchesWindow.DownloadQueueListView.Items
                                Dim DownloadItemAsDownloadQueueItem As DownloadQueueItem = CType(DownloadItem, DownloadQueueItem)
                                If DownloadItemAsDownloadQueueItem.FileName = DownloadQueueItem.FileName Then
                                    DownloadItemAsDownloadQueueItem.DownloadState = "Downloaded"
                                    OpenGamePatchesWindow.DownloadQueueListView.Items.Refresh()
                                    Exit For
                                End If
                            Next

                            Exit For
                        End If
                    Next
                End If
            End If

            If MsgBox("Download completed. Open the Downloads folder ?", MsgBoxStyle.YesNo, "Completed") = MsgBoxResult.Yes Then
                Process.Start("explorer", Environment.CurrentDirectory + "\Downloads")
            End If
        End If
    End Sub

    Private Sub Downloader_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub() DownloadImage.Source = New BitmapImage(New Uri("/Images/PKG.png", UriKind.Relative)))
        Else
            DownloadImage.Source = New BitmapImage(New Uri("/Images/PKG.png", UriKind.Relative))
        End If

        ServicePointManager.ServerCertificateValidationCallback = AddressOf ValidateRemoteCertificate 'Allows downloading the sc package that causes an certificate error
    End Sub

    Public Shared Function ValidateRemoteCertificate(sender As Object, certificate As X509Certificate, chain As X509Chain, sslPolicyErrors As SslPolicyErrors) As Boolean
        Return True
    End Function

End Class
