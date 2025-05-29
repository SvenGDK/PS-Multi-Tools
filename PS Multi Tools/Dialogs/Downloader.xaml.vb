Imports System.ComponentModel
Imports System.IO
Imports System.Net.Http
Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading

Public Class Downloader

    Public PackageConsole As String
    Public PackageTitleID As String
    Public PackageContentID As String

    Dim DownloadClient As HttpClient
    Public IsDownloadClientBusy As Boolean = False
    Private DownloadClientCTS As CancellationTokenSource

    Public DownloadIcon As ImageSource = Nothing
    Public DownloadQueueItem As DownloadQueueItem = Nothing
    Public DownloadCompleted As Boolean = False
    Public DownloadFileName As String = ""

    Public Sub New()
        InitializeComponent()
        DownloadProgressBar.Value = 0
    End Sub

    Private Function SetHttpClientServerCertificateCustomValidationCallback() As HttpClient
        Dim NewHttpClientHandler As New HttpClientHandler With {.ServerCertificateCustomValidationCallback = Function(httpRequestMessage, cert, cetChain, policyErrors)
                                                                                                                 Return True
                                                                                                             End Function}
        Return New HttpClient(NewHttpClientHandler)
    End Function

    Private Sub Downloader_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub() DownloadImage.Source = New BitmapImage(New Uri("/Images/PKG.png", UriKind.Relative)))
        Else
            DownloadImage.Source = New BitmapImage(New Uri("/Images/PKG.png", UriKind.Relative))
        End If

        'Allows downloading with certificate errors
        DownloadClient = SetHttpClientServerCertificateCustomValidationCallback()
    End Sub

    Public Async Function CreateNewDownload(Source As String, Optional ModifyName As Boolean = False, Optional NewName As String = "", Optional FileSize As String = "") As Task(Of Boolean)
        'Create Downloads directory if not exists
        If Not Directory.Exists(Environment.CurrentDirectory + "\Downloads") Then Directory.CreateDirectory(Environment.CurrentDirectory + "\Downloads")

        If Dispatcher.CheckAccess() = False Then
            Await Dispatcher.BeginInvoke(Sub() If DownloadIcon IsNot Nothing Then DownloadImage.Source = DownloadIcon)
        Else
            If DownloadIcon IsNot Nothing Then DownloadImage.Source = DownloadIcon
        End If

        'Get file name of requested download
        Dim FileName As String = Utils.GetFilenameFromUrl(New Uri(Source))
        If Not String.IsNullOrEmpty(FileName) Then

            DownloadFileName = FileName

            'Change the file name for .pkgs
            If ModifyName = True And Not String.IsNullOrEmpty(NewName) Then
                FileName = NewName
            End If

            'Get size of requested download
            Dim URLFileSize As Double = Await Utils.WebFileSize(Source)
            If URLFileSize > 0 Then
                If Dispatcher.CheckAccess() = False Then
                    Await Dispatcher.BeginInvoke(Sub()
                                                     DownloadFileSizeTB.Text = "File Size: " + URLFileSize.ToString + " MB"
                                                     FileToDownloadTB.Text = "Downloading " + FileName + " ..."
                                                 End Sub)
                Else
                    DownloadFileSizeTB.Text = "File Size: " + URLFileSize.ToString + " MB"
                    FileToDownloadTB.Text = "Downloading " + FileName + " ..."
                End If

                'Prompt for new file name if download already exists in the Downloads folder
                If File.Exists(Environment.CurrentDirectory + "\Downloads\" + FileName) Then
                    Dim NewSaveFileName As String = InputBox("The file " + FileName + " already exists in the Downloads folder. Please enter a new name for the file or leave it to overwrite the existing file.", "File already exists", FileName)
                    If Not String.IsNullOrEmpty(NewSaveFileName) Then
                        DownloadFileName = NewSaveFileName
                        Await DownloadFileWithProgressAsync(Source, Environment.CurrentDirectory + "\Downloads\" + NewSaveFileName)
                    Else
                        Await DownloadFileWithProgressAsync(Source, Environment.CurrentDirectory + "\Downloads\" + FileName)
                    End If
                Else
                    Await DownloadFileWithProgressAsync(Source, Environment.CurrentDirectory + "\Downloads\" + FileName)
                End If

                Return True
            Else
                'Set file size from 
                If Not String.IsNullOrEmpty(FileSize) Then

                    If Dispatcher.CheckAccess() = False Then
                        Await Dispatcher.BeginInvoke(Sub()
                                                         DownloadFileSizeTB.Text = "File Size: " + FileSize
                                                         FileToDownloadTB.Text = "Downloading " + FileName + " ..."
                                                     End Sub)
                    Else
                        DownloadFileSizeTB.Text = "File Size: " + FileSize
                        FileToDownloadTB.Text = "Downloading " + FileName + " ..."
                    End If

                    'Prompt for new file name if download already exists in the Downloads folder
                    If File.Exists(Environment.CurrentDirectory + "\Downloads\" + FileName) Then
                        Dim NewSaveFileName As String = InputBox("The file " + FileName + " already exists in the Downloads folder. Please enter a new name for the file or leave it to overwrite the existing file.", "File already exists", FileName)
                        If Not String.IsNullOrEmpty(NewSaveFileName) Then
                            DownloadFileName = NewSaveFileName
                            Await DownloadFileWithProgressAsync(Source, Environment.CurrentDirectory + "\Downloads\" + NewSaveFileName)
                        Else
                            Await DownloadFileWithProgressAsync(Source, Environment.CurrentDirectory + "\Downloads\" + FileName)
                        End If
                    Else
                        Await DownloadFileWithProgressAsync(Source, Environment.CurrentDirectory + "\Downloads\" + FileName)
                    End If

                    Return True
                Else
                    Return False
                End If
            End If
        Else
            Return False
        End If
    End Function

    Public Shared Function ValidateRemoteCertificate(sender As Object, certificate As X509Certificate, chain As X509Chain, sslPolicyErrors As SslPolicyErrors) As Boolean
        Return True
    End Function

    Public Async Function DownloadFileWithProgressAsync(FileUrl As String, SavePath As String) As Task
        Try
            Using DownloadClient
                Using NewHttpResponseMessage As HttpResponseMessage = Await DownloadClient.GetAsync(FileUrl, HttpCompletionOption.ResponseHeadersRead)
                    If NewHttpResponseMessage.IsSuccessStatusCode Then

                        DownloadClientCTS = New CancellationTokenSource()
                        Dim DownloadCancellationToken As CancellationToken = DownloadClientCTS.Token
                        IsDownloadClientBusy = True

                        Dim TotalBytes As Long = NewHttpResponseMessage.Content.Headers.ContentLength.GetValueOrDefault(0)
                        Using ResponseStream As Stream = Await NewHttpResponseMessage.Content.ReadAsStreamAsync(DownloadCancellationToken)

                            Using NewFileStream As New FileStream(SavePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, True)
                                Dim Buffer(8191) As Byte
                                Dim TotalBytesRead As Long = 0
                                Dim BytesRead As Integer

                                Dim NewStopwatch As New Stopwatch()
                                NewStopwatch.Start()

                                Do
                                    BytesRead = Await ResponseStream.ReadAsync(Buffer, DownloadCancellationToken)
                                    If BytesRead = 0 Then Exit Do

                                    Await NewFileStream.WriteAsync(Buffer.AsMemory(0, BytesRead))

                                    TotalBytesRead += BytesRead

                                    Dim ElapsedSeconds As Double = NewStopwatch.Elapsed.TotalSeconds
                                    Dim DownloadSpeed As Double = TotalBytesRead / ElapsedSeconds
                                    Dim SpeedInKbps As Double = DownloadSpeed / 1024
                                    Dim ETAInSeconds As Double = If(TotalBytes > 0, (TotalBytes - TotalBytesRead) / DownloadSpeed, 0)

                                    'Display progress
                                    If TotalBytes > 0 Then
                                        Dim DLProgress As Double = TotalBytesRead * 100 / TotalBytes

                                        If Dispatcher.CheckAccess() = False Then
                                            Await Dispatcher.BeginInvoke(Sub()
                                                                             DownloadETA.Text = $"ETA: {ETAInSeconds:F0} seconds left"
                                                                             DownloadSpeedTB.Text = $"Speed: {SpeedInKbps:F2} KB/s"
                                                                             DownloadProgressBar.Value = DLProgress
                                                                         End Sub)

                                        Else
                                            DownloadETA.Text = $"ETA: {ETAInSeconds:F0} seconds left"
                                            DownloadSpeedTB.Text = $"Speed: {SpeedInKbps:F2} KB/s"
                                            DownloadProgressBar.Value = DLProgress
                                        End If

                                        'Console.WriteLine($"Progress: {progress:F2}% | Speed: {speedInKbps:F2} KB/s | ETA: {ETAInSeconds:F0} seconds")
                                    End If
                                Loop

                                NewStopwatch.Stop()

                                If FileToDownloadTB.Dispatcher.CheckAccess() = False Then
                                    Await FileToDownloadTB.Dispatcher.BeginInvoke(Sub() FileToDownloadTB.Text = "Download finished")
                                Else
                                    FileToDownloadTB.Text = "Download finished"
                                End If

                                'For PS5 game patches
                                If DownloadQueueItem IsNot Nothing Then
                                    If Not String.IsNullOrEmpty(DownloadQueueItem.FileName) Then
                                        'Update progress in PS5GamePatches (if open)
                                        Dim OpenGamePatchesWindow As PS5GamePatches
                                        For Each OpenWin In System.Windows.Application.Current.Windows()
                                            If OpenWin.ToString = "PS_Multi_Tools.PS5GamePatches" Then
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

                                DownloadCompleted = True
                            End Using
                        End Using
                    End If
                End Using
            End Using

            If DownloadCompleted Then
                'Prompt for extracting a downloaded archive and opening the downloads folder
                If DownloadFileName.EndsWith(".zip") Or DownloadFileName.EndsWith(".7z") Or DownloadFileName.EndsWith(".rar") Then
                    If MsgBox("Download completed!" + vbCrLf + "The downloaded file is an archive that can be extracted, do you want to extract it now ?", MsgBoxStyle.YesNo, "Extract download ?") = MsgBoxResult.Yes Then

                        'Check and extract the downloaded archive into the downloads folder
                        If File.Exists(Environment.CurrentDirectory + "\Downloads\" + DownloadFileName) Then

                            Using ArchiveExtractor As New Process()
                                ArchiveExtractor.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\7z.exe"
                                ArchiveExtractor.StartInfo.Arguments = "x """ + Environment.CurrentDirectory + "\Downloads\" + DownloadFileName + """" +
                                    " -o""" + Environment.CurrentDirectory + "\Downloads\" + """ -y"
                                ArchiveExtractor.StartInfo.UseShellExecute = False
                                ArchiveExtractor.StartInfo.CreateNoWindow = True
                                ArchiveExtractor.Start()
                                ArchiveExtractor.WaitForExit()
                            End Using

                            If MsgBox("Extraction done!" + vbCrLf + "Do you want to open the Downloads folder ?", MsgBoxStyle.YesNo, "Completed") = MsgBoxResult.Yes Then
                                Process.Start("explorer", Environment.CurrentDirectory + "\Downloads")
                            End If
                        Else
                            MsgBox("Could not find the downloaded archive.", MsgBoxStyle.Critical, "Error while extracting")
                            If MsgBox("Do you want to open the Downloads folder ?", MsgBoxStyle.YesNo, "Completed") = MsgBoxResult.Yes Then
                                Process.Start("explorer", Environment.CurrentDirectory + "\Downloads")
                            End If
                        End If

                    Else
                        If MsgBox("Do you want to open the Downloads folder ?", MsgBoxStyle.YesNo, "Completed") = MsgBoxResult.Yes Then
                            Process.Start("explorer", Environment.CurrentDirectory + "\Downloads")
                        End If
                    End If
                Else
                    If MsgBox("Download completed!" + vbCrLf + "Do you want to open the Downloads folder ?", MsgBoxStyle.YesNo, "Completed") = MsgBoxResult.Yes Then
                        Process.Start("explorer", Environment.CurrentDirectory + "\Downloads")
                    End If
                End If
            Else
                MsgBox("Download canceled.", MsgBoxStyle.Exclamation, "Info")
            End If
        Catch ex As OperationCanceledException
            MsgBox("Download canceled.", MsgBoxStyle.Exclamation, "Info")
        Catch ex As Exception
            MsgBox(ex.ToString())
        Finally
            If DownloadClientCTS IsNot Nothing Then
                DownloadClientCTS.Dispose()
                DownloadClientCTS = Nothing
            End If
        End Try
    End Function

    Private Sub Downloader_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If IsDownloadClientBusy AndAlso DownloadClientCTS IsNot Nothing Then
            DownloadClientCTS.Cancel()
            DownloadClientCTS.Dispose()
            DownloadClientCTS = Nothing
        End If
    End Sub

End Class
