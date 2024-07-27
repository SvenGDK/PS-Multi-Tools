Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports WinSCP
Imports Cursors = System.Windows.Input.Cursors

Public Class PS5FTPGrabber

    Public ConsoleIP As String = String.Empty

    Dim SelectedPath As String = String.Empty
    Dim App0RemoteFolder As String = String.Empty
    Dim App0RemoteFolderName As String = String.Empty
    Dim DiscRemoteFolder As String = String.Empty
    Dim CustomRemoteFolder As String = String.Empty

    Dim AppMetadataRemoteFolder As String = String.Empty
    Dim SystemAppMetadataRemoteFolder As String = String.Empty
    Dim NPBind As String = String.Empty

    Dim TotalFiles As Integer = 0
    Dim CopiedFiles As Integer = 0

    Dim WithEvents AppCopyWorker As New BackgroundWorker With {.WorkerReportsProgress = True}
    Dim WithEvents MetadataWorker As New BackgroundWorker With {.WorkerReportsProgress = True}
    Dim WithEvents ReceiveWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}

    Dim AppDumpResult As SynchronizationResult
    Dim MetadataDumpResult As SynchronizationResult

    Private Sub DownloadButton_Click(sender As Object, e As RoutedEventArgs) Handles DownloadButton.Click

        App0RemoteFolder = String.Empty
        DiscRemoteFolder = String.Empty
        CustomRemoteFolder = String.Empty
        AppMetadataRemoteFolder = String.Empty
        SystemAppMetadataRemoteFolder = String.Empty
        NPBind = String.Empty

        Cursor = Cursors.Wait

        If SelectedFolderComboBox.Text = "/mnt/sandbox/pfsmnt/" Then

            If MetadataDumpCheckBox.IsChecked Then

                If GetAppMetadata() = True Then

                    If Dispatcher.CheckAccess() = False Then
                        Dispatcher.BeginInvoke(Sub()
                                                   ReceiveProgressBar.Value = 0
                                                   ReceiveStatusTextBlock.Text = "Getting metadata, please wait ... 0/" + TotalFiles.ToString()
                                                   ReceiveProgressBar.Maximum = TotalFiles
                                               End Sub)
                    Else
                        ReceiveProgressBar.Value = 0
                        ReceiveStatusTextBlock.Text = "Getting metadata, please wait ... 0/" + TotalFiles.ToString()
                        ReceiveProgressBar.Maximum = TotalFiles
                    End If

                    LockUI()
                    MetadataWorker.RunWorkerAsync()
                Else
                    Cursor = Cursors.Arrow
                    MsgBox("Could not find any metadata.", MsgBoxStyle.Exclamation, "Error reading data")
                End If

            ElseIf SELFDumpCheckBox.IsChecked Then

                If String.IsNullOrEmpty(SelectedDirectoryTextBox.Text) Then

                    'If not path is specified then create a "Dumps" folder
                    If Not Directory.Exists(My.Computer.FileSystem.CurrentDirectory + "\Dumps") Then
                        Directory.CreateDirectory(My.Computer.FileSystem.CurrentDirectory + "\Dumps")
                    End If
                    'Set the new folder
                    SelectedPath = My.Computer.FileSystem.CurrentDirectory + "\Dumps"

                End If

                LockUI()
                ReceiveWorker.RunWorkerAsync()
            Else

                If GetApp0(ConsoleIP) = True Then

                    If Dispatcher.CheckAccess() = False Then
                        Dispatcher.BeginInvoke(Sub()
                                                   ReceiveProgressBar.Value = 0
                                                   ReceiveStatusTextBlock.Text = "Starting, please wait ... 0/" + TotalFiles.ToString()
                                                   ReceiveProgressBar.Maximum = TotalFiles
                                               End Sub)
                    Else
                        ReceiveProgressBar.Value = 0
                        ReceiveStatusTextBlock.Text = "Starting, please wait ... 0/" + TotalFiles.ToString()
                        ReceiveProgressBar.Maximum = TotalFiles
                    End If

                    LockUI()
                    AppCopyWorker.RunWorkerAsync()
                Else
                    Cursor = Cursors.Arrow
                    MsgBox("Could not find any mounted game.", MsgBoxStyle.Exclamation, "Error reading data")
                End If

            End If

        ElseIf SelectedFolderComboBox.Text = "/mnt/disc/" Then

            If FilesAvailable("/mnt/disc") = True Then
                DiscRemoteFolder = "/mnt/disc/"

                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub()
                                               ReceiveStatusTextBlock.Text = "Starting, please wait ... 0/" + TotalFiles.ToString()
                                               ReceiveProgressBar.Maximum = TotalFiles
                                           End Sub)
                Else
                    ReceiveStatusTextBlock.Text = "Starting, please wait ... 0/" + TotalFiles.ToString()
                    ReceiveProgressBar.Maximum = TotalFiles
                End If

                LockUI()
                AppCopyWorker.RunWorkerAsync()
            Else
                Cursor = Cursors.Arrow
                MsgBox("No disc inserted !", MsgBoxStyle.Critical, "Error reading data")
            End If

        Else

            CustomRemoteFolder = SelectedFolderComboBox.Text

            If FilesAvailable(CustomRemoteFolder) = True Then
                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub()
                                               ReceiveStatusTextBlock.Text = "Starting, please wait ... 0/" + TotalFiles.ToString()
                                               ReceiveProgressBar.Maximum = TotalFiles
                                           End Sub)
                Else
                    ReceiveStatusTextBlock.Text = "Starting, please wait ... 0/" + TotalFiles.ToString()
                    ReceiveProgressBar.Maximum = TotalFiles
                End If

                LockUI()
                AppCopyWorker.RunWorkerAsync()
            Else
                Cursor = Cursors.Arrow
                MsgBox("No files available !", MsgBoxStyle.Critical, "Error reading data")
            End If

        End If

    End Sub

    Public Function GetApp0(ConsoleIP As String) As Boolean
        Dim sessionOptions As New SessionOptions
        With sessionOptions
            .Protocol = Protocol.Ftp
            .HostName = ConsoleIP
            .UserName = "anonymous"
            .Password = "anonymous"
            .PortNumber = 1337
        End With

        Dim NewSession As New Session()

        Try
            ' Connect
            NewSession.Open(sessionOptions)

            ' Get the app0 folder
            For Each DirectoryInFTP As RemoteFileInfo In NewSession.EnumerateRemoteFiles("/mnt/sandbox/pfsmnt/", "", EnumerationOptions.MatchDirectories)
                If DirectoryInFTP.Name.EndsWith("app0") Then
                    App0RemoteFolderName = DirectoryInFTP.Name
                    App0RemoteFolder = DirectoryInFTP.FullName
                    Exit For
                End If
            Next

        Catch ex As Exception
            MsgBox(ex.Message)
            MsgBox(ex.InnerException.Message)
        End Try

        If String.IsNullOrEmpty(App0RemoteFolder) Then
            Return False
        Else
            Try
                ' Reset and get total files count
                TotalFiles = 0
                CopiedFiles = 0

                For Each FileInFTP In NewSession.EnumerateRemoteFiles(App0RemoteFolder, "", EnumerationOptions.AllDirectories)
                    TotalFiles += 1
                Next
            Catch ex As Exception
                MsgBox(ex.Message)
                MsgBox(ex.InnerException.Message)
            End Try

            Return True
        End If
    End Function

    Public Function GetAppMetadata() As Boolean
        Dim sessionOptions As New SessionOptions
        With sessionOptions
            .Protocol = Protocol.Ftp
            .HostName = ConsoleIP
            .UserName = "anonymous"
            .Password = "anonymous"
            .PortNumber = 1337
        End With

        Dim NewSession As New Session()
        Dim GameID As String = String.Empty

        Try

            'Connect
            NewSession.Open(sessionOptions)

            If String.IsNullOrEmpty(App0RemoteFolderName) Then
                ' Get the app0 folder
                For Each DirectoryInFTP As RemoteFileInfo In NewSession.EnumerateRemoteFiles("/mnt/sandbox/pfsmnt/", "", EnumerationOptions.MatchDirectories)
                    If DirectoryInFTP.Name.EndsWith("app0") Then
                        App0RemoteFolderName = DirectoryInFTP.Name
                        App0RemoteFolder = DirectoryInFTP.FullName
                        Exit For
                    End If
                Next
            End If

            GameID = App0RemoteFolderName.Split("-"c)(0)

            'Check if the metadata exists
            For Each DirectoryInFTP As RemoteFileInfo In NewSession.EnumerateRemoteFiles("/user/appmeta/", "", EnumerationOptions.MatchDirectories)
                If DirectoryInFTP.Name = GameID Then
                    AppMetadataRemoteFolder = DirectoryInFTP.FullName
                    Exit For
                End If
            Next
            For Each DirectoryInFTP As RemoteFileInfo In NewSession.EnumerateRemoteFiles("/system_data/priv/appmeta/", "", EnumerationOptions.MatchDirectories)
                If DirectoryInFTP.Name = GameID Then
                    SystemAppMetadataRemoteFolder = DirectoryInFTP.FullName
                    Exit For
                End If
            Next

            'Check for npbind.dat
            For Each FileInFTP As RemoteFileInfo In NewSession.EnumerateRemoteFiles("/system_data/priv/appmeta/" + GameID + "/trophy2/", "", EnumerationOptions.AllDirectories)
                If FileInFTP.Name = "npbind.dat" Then
                    NPBind = FileInFTP.FullName
                    Exit For
                End If
            Next

        Catch ex As Exception
            MsgBox(ex.Message)
            MsgBox(ex.InnerException.Message)
        End Try

        If String.IsNullOrEmpty(AppMetadataRemoteFolder) Then
            Return False
        Else
            Try
                'Reset and get total files count
                TotalFiles = 3
                CopiedFiles = 0

                For Each FileInFTP In NewSession.EnumerateRemoteFiles(AppMetadataRemoteFolder, "", EnumerationOptions.AllDirectories)
                    TotalFiles += 1
                Next
                For Each FileInFTP In NewSession.EnumerateRemoteFiles(SystemAppMetadataRemoteFolder, "", EnumerationOptions.AllDirectories)
                    TotalFiles += 1
                Next
            Catch ex As Exception
                MsgBox(ex.Message)
                MsgBox(ex.InnerException.Message)
            End Try

            Return True
        End If
    End Function

    Public Function FilesAvailable(RemotePath As String) As Boolean
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

        For Each FileInFTP In NewSession.EnumerateRemoteFiles(RemotePath, "", EnumerationOptions.AllDirectories)
            TotalFiles += 1
        Next

        If TotalFiles > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub AppCopyWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles AppCopyWorker.DoWork
        Try
            'Setup session options
            Dim NewSessionOptions As New SessionOptions
            With NewSessionOptions
                .Protocol = Protocol.Ftp
                .HostName = ConsoleIP
                .UserName = "anonymous"
                .Password = "anonymous"
                .PortNumber = 1337
                .FtpMode = FtpMode.Passive
                .FtpSecure = FtpSecure.None
                .Secure = False
            End With

            Dim NewSession As New Session()

            'Report progress of synchronization
            AddHandler NewSession.FileTransferred, AddressOf FileTransferred

            'Connect
            NewSession.Open(NewSessionOptions)

            'Synchronize files and folders
            If Not String.IsNullOrEmpty(App0RemoteFolder) Then
                AppDumpResult = NewSession.SynchronizeDirectories(SynchronizationMode.Local, SelectedPath, App0RemoteFolder, False)
            End If
            If Not String.IsNullOrEmpty(DiscRemoteFolder) Then
                AppDumpResult = NewSession.SynchronizeDirectories(SynchronizationMode.Local, SelectedPath, DiscRemoteFolder, False)
            End If
            If Not String.IsNullOrEmpty(CustomRemoteFolder) Then
                AppDumpResult = NewSession.SynchronizeDirectories(SynchronizationMode.Local, SelectedPath, CustomRemoteFolder, False)
            End If

        Catch ex As Exception
            MsgBox("Error reading data.", MsgBoxStyle.Critical)
        End Try
    End Sub

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
    End Sub

    Private Sub AppCopyWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles AppCopyWorker.RunWorkerCompleted

        Cursor = Cursors.Arrow

        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub() ReceiveProgressBar.Value = 0)
        Else
            ReceiveProgressBar.Value = 0
        End If

        'Throw on any error
        AppDumpResult.Check()

        'Check result
        If AppDumpResult.IsSuccess Then

            If FullDumpCheckBox.IsChecked Then

                Cursor = Cursors.Wait

                If GetAppMetadata() = True Then

                    If Dispatcher.CheckAccess() = False Then
                        Dispatcher.BeginInvoke(Sub()
                                                   ReceiveProgressBar.Value = 0
                                                   ReceiveStatusTextBlock.Text = "Getting metadata, please wait ... 0/" + TotalFiles.ToString()
                                                   ReceiveProgressBar.Maximum = TotalFiles
                                               End Sub)
                    Else
                        ReceiveProgressBar.Value = 0
                        ReceiveStatusTextBlock.Text = "Getting metadata, please wait ... 0/" + TotalFiles.ToString()
                        ReceiveProgressBar.Maximum = TotalFiles
                    End If

                    MetadataWorker.RunWorkerAsync()
                Else
                    Cursor = Cursors.Arrow
                    MsgBox("Could not find any metadata.", MsgBoxStyle.Exclamation, "Error reading data")
                End If

            Else
                MsgBox("Dump completed.", MsgBoxStyle.Information, "Success")
                LockUI()
            End If

        Else
            MsgBox("Could not dump all files.", MsgBoxStyle.Exclamation)
        End If

    End Sub

    Private Sub BrowseFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseFolderButton.Click
        Dim FBD As New Windows.Forms.FolderBrowserDialog()

        If FBD.ShowDialog() = Windows.Forms.DialogResult.OK Then
            SelectedDirectoryTextBox.Text = FBD.SelectedPath
            SelectedPath = FBD.SelectedPath
        End If
    End Sub

    Private Sub MetadataWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles MetadataWorker.DoWork
        'Setup session options
        Dim NewSessionOptions As New SessionOptions
        With NewSessionOptions
            .Protocol = Protocol.Ftp
            .HostName = ConsoleIP
            .UserName = "anonymous"
            .Password = "anonymous"
            .PortNumber = 1337
        End With

        Dim NewSession As New Session()

        'Report progress of synchronization
        AddHandler NewSession.FileTransferred, AddressOf FileTransferred

        'Connect
        NewSession.Open(NewSessionOptions)

        'Get npbind.dat
        If Not String.IsNullOrEmpty(NPBind) Then

            If Not Directory.Exists(My.Computer.FileSystem.CurrentDirectory + "/Cache") Then
                Directory.CreateDirectory(My.Computer.FileSystem.CurrentDirectory + "/Cache")
            End If

            NewSession.GetFileToDirectory(NPBind, My.Computer.FileSystem.CurrentDirectory + "\Cache")
        End If

        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Cache\npbind.dat") Then

            'Get NPWR id
            Dim NPWR As String = String.Empty
            Using WindowsCMD As New Process()
                WindowsCMD.StartInfo.FileName = "cmd"
                WindowsCMD.StartInfo.Arguments = "/c strings -nobanner """ + My.Computer.FileSystem.CurrentDirectory + "\Cache\npbind.dat" + """ | findstr NPWR"
                WindowsCMD.StartInfo.RedirectStandardOutput = True
                WindowsCMD.StartInfo.UseShellExecute = False
                WindowsCMD.StartInfo.CreateNoWindow = True
                WindowsCMD.Start()
                WindowsCMD.WaitForExit()

                Dim OutputReader As StreamReader = WindowsCMD.StandardOutput
                Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {""}, StringSplitOptions.RemoveEmptyEntries)

                If ProcessOutput.Count > -1 Then
                    NPWR = ProcessOutput(0).Trim
                End If
            End Using

            'Get uds & trophy ucp files
            If Not String.IsNullOrEmpty(NPWR) Then

                'Check if folder exists for uds and get the file
                For Each DirectoryInFTP As RemoteFileInfo In NewSession.EnumerateRemoteFiles("/user/np_uds/nobackup/conf/", "", EnumerationOptions.MatchDirectories)
                    If DirectoryInFTP.Name = NPWR Then
                        NewSession.GetFileToDirectory(DirectoryInFTP.FullName + "/uds.ucp", SelectedPath + "\sce_sys\uds")
                        Exit For
                    End If
                Next

                If File.Exists(SelectedPath + "\sce_sys\uds\uds.ucp") Then
                    If Not File.Exists(SelectedPath + "\sce_sys\uds\uds00.ucp") Then
                        My.Computer.FileSystem.RenameFile(SelectedPath + "\sce_sys\uds\uds.ucp", "uds00.ucp")
                    End If
                End If

                'Check if folder exists for trophy and get the file
                For Each DirectoryInFTP As RemoteFileInfo In NewSession.EnumerateRemoteFiles("/user/trophy2/nobackup/conf/", "", EnumerationOptions.MatchDirectories)
                    If DirectoryInFTP.Name = NPWR Then
                        NewSession.GetFileToDirectory(DirectoryInFTP.FullName + "/TROPHY.UCP", SelectedPath + "\sce_sys\trophy2")
                        Exit For
                    End If
                Next

                If File.Exists(SelectedPath + "\sce_sys\trophy2\TROPHY.UCP") Then
                    If Not File.Exists(SelectedPath + "\sce_sys\trophy2\trophy00.ucp") Then
                        My.Computer.FileSystem.RenameFile(SelectedPath + "\sce_sys\trophy2\TROPHY.UCP", "trophy00.ucp")
                    End If
                End If

            End If

        End If

        'Get the appmeta
        If Not String.IsNullOrEmpty(AppMetadataRemoteFolder) Then
            MetadataDumpResult = NewSession.SynchronizeDirectories(SynchronizationMode.Local, SelectedPath + "\sce_sys", AppMetadataRemoteFolder, False)
        End If
        If Not String.IsNullOrEmpty(SystemAppMetadataRemoteFolder) Then
            MetadataDumpResult = NewSession.SynchronizeDirectories(SynchronizationMode.Local, SelectedPath + "\sce_sys", SystemAppMetadataRemoteFolder, False)
        End If
    End Sub

    Private Sub MetadataWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles MetadataWorker.RunWorkerCompleted

        Cursor = Cursors.Arrow

        ' Throw on any error
        MetadataDumpResult.Check()

        If MetadataDumpResult.IsSuccess Then
            MsgBox("Dump completed!", MsgBoxStyle.Information, "Success")
            LockUI()
        Else
            MsgBox("Could not dump all files.", MsgBoxStyle.Exclamation, "Error while getting the metadata")
        End If

        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub()
                                       ReceiveProgressBar.Value = 0
                                       ReceiveStatusTextBlock.Text = "Transfer Status: Dump completed!"
                                   End Sub)
        Else
            ReceiveProgressBar.Value = 0
            ReceiveStatusTextBlock.Text = "Transfer Status: Dump completed!"
        End If

    End Sub

    Private Sub SelectedFolderComboBox_SelectionChanged(sender As Object, e As Windows.Controls.SelectionChangedEventArgs) Handles SelectedFolderComboBox.SelectionChanged
        If SelectedFolderComboBox.SelectedItem IsNot Nothing Then
            'Make options available if first entry is selected (should be "/mnt/sandbox/pfsmnt/")
            If SelectedFolderComboBox.SelectedIndex = 0 Then
                FullDumpCheckBox.IsEnabled = True
                MetadataDumpCheckBox.IsEnabled = True
                SELFDumpCheckBox.IsEnabled = True
            Else
                FullDumpCheckBox.IsEnabled = False
                MetadataDumpCheckBox.IsEnabled = False
                SELFDumpCheckBox.IsEnabled = False
            End If
        End If
    End Sub

    Private Sub PS5FTPGrabber_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        Utils.CheckForMissingFiles()
    End Sub

    Public Sub LockUI()
        If SelectedFolderComboBox.IsEnabled Then
            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           SelectedFolderComboBox.IsEnabled = False
                                           FullDumpCheckBox.IsEnabled = False
                                           MetadataDumpCheckBox.IsEnabled = False
                                           SelectedDirectoryTextBox.IsEnabled = False
                                           BrowseFolderButton.IsEnabled = False
                                           DownloadButton.IsEnabled = False
                                       End Sub)
            Else
                SelectedFolderComboBox.IsEnabled = False
                FullDumpCheckBox.IsEnabled = False
                MetadataDumpCheckBox.IsEnabled = False
                SelectedDirectoryTextBox.IsEnabled = False
                BrowseFolderButton.IsEnabled = False
                DownloadButton.IsEnabled = False
            End If
        Else
            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           SelectedFolderComboBox.IsEnabled = True
                                           FullDumpCheckBox.IsEnabled = True
                                           MetadataDumpCheckBox.IsEnabled = True
                                           SelectedDirectoryTextBox.IsEnabled = True
                                           BrowseFolderButton.IsEnabled = True
                                           DownloadButton.IsEnabled = True
                                       End Sub)
            Else
                SelectedFolderComboBox.IsEnabled = True
                FullDumpCheckBox.IsEnabled = True
                MetadataDumpCheckBox.IsEnabled = True
                SelectedDirectoryTextBox.IsEnabled = True
                BrowseFolderButton.IsEnabled = True
                DownloadButton.IsEnabled = True
            End If
        End If
    End Sub

    Private Sub ReceiveWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles ReceiveWorker.DoWork
        Dim Read As Integer
        Dim TotalBytesRead As Integer = 0
        Dim Buffer As Byte() = New Byte(1024 - 1) {}
        Dim EndpointIPAddress As IPAddress = IPAddress.Parse(ConsoleIP)
        Dim NewIPEndPoint As New IPEndPoint(EndpointIPAddress, 9023)

        'Change progress bar
        If ReceiveProgressBar.Dispatcher.CheckAccess() = False Then
            ReceiveProgressBar.Dispatcher.BeginInvoke(Sub() ReceiveProgressBar.IsIndeterminate = True)
        Else
            ReceiveProgressBar.IsIndeterminate = True
        End If

        Using ReceiverSocket As New Socket(SocketType.Stream, ProtocolType.Tcp)

            ReceiverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, True)
            ReceiverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000)

            'Connect to the SELF dumper
            ReceiverSocket.Connect(NewIPEndPoint)

            'Create the dump
            Using NewFileStream As New FileStream(SelectedPath + "\self-dump.tar", FileMode.Create, FileAccess.ReadWrite)

                Do
                    'Receive from socket
                    Read = ReceiverSocket.Receive(Buffer)

                    'Continue if data is present
                    If Read > 0 Then

                        'Write to file & update TotalBytesRead
                        NewFileStream.Write(Buffer, 0, Read)
                        TotalBytesRead += Read

                        'Update the status text
                        If ReceiveStatusTextBlock.Dispatcher.CheckAccess() = False Then
                            ReceiveStatusTextBlock.Dispatcher.BeginInvoke(Sub() ReceiveStatusTextBlock.Text = "Creating self-dump.tar. " + FormatNumber(TotalBytesRead / 1048576, 2) + " MB received.")
                        Else
                            ReceiveStatusTextBlock.Text = "Creating self-dump.tar. " + FormatNumber(TotalBytesRead / 1048576, 2) + " MB received."
                        End If

                    End If

                Loop While Read > 0

            End Using

            'Close the connection
            ReceiverSocket.Close()

        End Using
    End Sub

    Private Sub ReceiveWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles ReceiveWorker.RunWorkerCompleted

        'Change progress bar
        If ReceiveProgressBar.Dispatcher.CheckAccess() = False Then
            ReceiveProgressBar.Dispatcher.BeginInvoke(Sub() ReceiveProgressBar.IsIndeterminate = False)
        Else
            ReceiveProgressBar.IsIndeterminate = False
        End If

        'Update the status text
        If ReceiveStatusTextBlock.Dispatcher.CheckAccess() = False Then
            ReceiveStatusTextBlock.Dispatcher.BeginInvoke(Sub() ReceiveStatusTextBlock.Text = "")
        Else
            ReceiveStatusTextBlock.Text = ""
        End If

        Cursor = Cursors.Arrow

        If SelectedPath = My.Computer.FileSystem.CurrentDirectory + "\Dumps" Then
            If MsgBox("SELF files dumped!" + vbCrLf + "Open the 'Dumps' folder ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                Process.Start("explorer", My.Computer.FileSystem.CurrentDirectory + "\Dumps")
            End If
        Else
            MsgBox("SELF files dumped!", MsgBoxStyle.Information)
        End If

        LockUI()

    End Sub

End Class
