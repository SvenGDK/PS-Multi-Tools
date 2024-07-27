Imports System.ComponentModel
Imports WinSCP

Public Class FTPBDRip

    Public ConsoleIP As String = ""
    Dim SelectedPath As String = ""

    Dim TotalFiles As Integer = 0
    Dim CopiedFiles As Integer = 0

    Dim WithEvents CopyWorker As New BackgroundWorker With {.WorkerReportsProgress = True}
    Dim DumpResult As SynchronizationResult

    Private Sub ConnectButton_Click(sender As Object, e As RoutedEventArgs) Handles ConnectButton.Click
        If Not String.IsNullOrEmpty(IPTextBox.Text) And Not String.IsNullOrEmpty(PortTextBox.Text) Then
            If FilesAvailable(IPTextBox.Text, PortTextBox.Text) = False Then
                MsgBox("No disc inserted !", MsgBoxStyle.Critical, "Error reading data")
            Else
                ConsoleIP = IPTextBox.Text
                StartButton.IsEnabled = True
                MsgBox("Disc dumping is ready." + vbCrLf + "Please stop the installation if it started.", MsgBoxStyle.Information, "Disc available")
            End If
        End If
    End Sub

    Public Function FilesAvailable(ConsoleIP As String, ConsolePort As String) As Boolean
        Dim sessionOptions As New SessionOptions
        With sessionOptions
            .Protocol = Protocol.Ftp
            .HostName = ConsoleIP
            .UserName = "anonymous"
            .Password = "anonymous"
            .PortNumber = 1337
        End With

        Dim NewSession As New Session()

        ' Connect
        NewSession.Open(sessionOptions)

        ' Reset and get total files count
        TotalFiles = 0
        CopiedFiles = 0
        For Each FileInFTP In NewSession.EnumerateRemoteFiles("/mnt/disc", "", EnumerationOptions.AllDirectories)
            TotalFiles += 1
        Next

        If TotalFiles > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub FileTransferred(sender As Object, e As TransferEventArgs)

        CopiedFiles += 1

        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub()
                                       ReceiveStatusTextBlock.Text = e.FileName + " copied. " + CopiedFiles.ToString() + "/" + TotalFiles.ToString()
                                       ReceiveProgressBar.Value += 1
                                   End Sub)
        Else
            ReceiveStatusTextBlock.Text = e.FileName + " copied. " + CopiedFiles.ToString() + "/" + TotalFiles.ToString()
            ReceiveProgressBar.Value += 1
        End If

        If e.Error Is Nothing Then
            Console.WriteLine("Upload of {0} succeeded", e.FileName)
        Else
            Console.WriteLine("Upload of {0} failed: {1}", e.FileName, e.Error)
        End If

    End Sub

    Private Sub StartButton_Click(sender As Object, e As RoutedEventArgs) Handles StartButton.Click
        If Not String.IsNullOrEmpty(IPTextBox.Text) AndAlso Not String.IsNullOrEmpty(PortTextBox.Text) AndAlso Not String.IsNullOrEmpty(SelectedDirectoryTextBox.Text) Then
            Cursor = Cursors.Wait

            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           ReceiveStatusTextBlock.Text = "Starting, please wait ... 0/" + TotalFiles.ToString()
                                           ReceiveProgressBar.Maximum = TotalFiles
                                       End Sub)
            Else
                ReceiveStatusTextBlock.Text = "Starting, please wait ... 0/" + TotalFiles.ToString()
                ReceiveProgressBar.Maximum = TotalFiles
            End If

            CopyWorker.RunWorkerAsync()
        End If
    End Sub

    Private Sub BrowseFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseFolderButton.Click
        Dim FBD As New Windows.Forms.FolderBrowserDialog()

        If FBD.ShowDialog() = Windows.Forms.DialogResult.OK Then
            SelectedDirectoryTextBox.Text = FBD.SelectedPath
            SelectedPath = FBD.SelectedPath
        End If
    End Sub

    Private Sub CopyWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles CopyWorker.DoWork
        Try
            'Setup session options
            Dim NewSessionOptions As New SessionOptions
            With NewSessionOptions
                .Protocol = Protocol.Ftp
                .HostName = "192.168.178.43"
                .UserName = "anonymous"
                .Password = "anonymous"
                .PortNumber = 1337
            End With

            Dim NewSession As New Session()

            'Report progress of synchronization
            AddHandler NewSession.FileTransferred, AddressOf FileTransferred

            'Connect
            NewSession.Open(NewSessionOptions)

            'Synchronize files and folders
            DumpResult = NewSession.SynchronizeDirectories(SynchronizationMode.Local, SelectedPath, "/mnt/disc", False)

        Catch ex As Exception
            MsgBox("Error reading data.", MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub CopyWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles CopyWorker.RunWorkerCompleted

        Cursor = Cursors.Arrow

        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub() ReceiveProgressBar.Value = 0)
        Else
            ReceiveProgressBar.Value = 0
        End If

        ' Throw on any error
        DumpResult.Check()

        ' Check result
        If DumpResult.IsSuccess Then
            MsgBox("Disc dump completed.", MsgBoxStyle.Information, "Success")
        Else
            MsgBox("Could not dump all files." + vbCrLf, MsgBoxStyle.Exclamation)
        End If

    End Sub

End Class
