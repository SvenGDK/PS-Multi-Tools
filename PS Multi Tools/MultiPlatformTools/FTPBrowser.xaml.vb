Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Security.Authentication
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports FluentFTP

Public Class FTPBrowser

    Public FTPS5Mode As Boolean = False
    Public CurrentPath As String = ""
    Dim WithEvents FTPUploadWorker As New BackgroundWorker() With {.WorkerReportsProgress = True, .WorkerSupportsCancellation = True}
    Dim WithEvents FTPUploadWorker2 As New BackgroundWorker() With {.WorkerReportsProgress = True, .WorkerSupportsCancellation = True}

    Dim WithEvents DownloadMenuItem As New Controls.MenuItem() With {.Header = "Download", .Icon = New Image() With {.Source = New BitmapImage(New Uri("/Images/download.png", UriKind.RelativeOrAbsolute))}}
    Dim WithEvents UploadMenuItem As New Controls.MenuItem() With {.Header = "Upload a file or folder", .Icon = New Image() With {.Source = New BitmapImage(New Uri("/Images/upload.png", UriKind.RelativeOrAbsolute))}}
    Dim WithEvents DeleteMenuItem As New Controls.MenuItem() With {.Header = "Delete", .Icon = New Image() With {.Source = New BitmapImage(New Uri("/Images/delete.png", UriKind.RelativeOrAbsolute))}}
    Dim WithEvents RenameMenuItem As New Controls.MenuItem() With {.Header = "Rename", .Icon = New Image() With {.Source = New BitmapImage(New Uri("/Images/rename.png", UriKind.RelativeOrAbsolute))}}
    Dim WithEvents NewDirectoryMenuItem As New Controls.MenuItem() With {.Header = "Create a new directory", .Icon = New Image() With {.Source = New BitmapImage(New Uri("/Images/NewFolder.png", UriKind.RelativeOrAbsolute))}}

    Public Structure FTPListViewItem
        Private _FileOrDirName As String
        Private _FileOrDirType As String
        Private _FileOrDirLastModified As String
        Private _FileOrDirPermissions As String
        Private _FileOrDirOwner As String
        Private _FileOrDirSize As String

        Public Property FileOrDirName As String
            Get
                Return _FileOrDirName
            End Get
            Set
                _FileOrDirName = Value
            End Set
        End Property

        Public Property FileOrDirType As String
            Get
                Return _FileOrDirType
            End Get
            Set
                _FileOrDirType = Value
            End Set
        End Property

        Public Property FileOrDirSize As String
            Get
                Return _FileOrDirSize
            End Get
            Set
                _FileOrDirSize = Value
            End Set
        End Property

        Public Property FileOrDirLastModified As String
            Get
                Return _FileOrDirLastModified
            End Get
            Set
                _FileOrDirLastModified = Value
            End Set
        End Property

        Public Property FileOrDirPermissions As String
            Get
                Return _FileOrDirPermissions
            End Get
            Set
                _FileOrDirPermissions = Value
            End Set
        End Property

        Public Property FileOrDirOwner As String
            Get
                Return _FileOrDirOwner
            End Get
            Set
                _FileOrDirOwner = Value
            End Set
        End Property
    End Structure

    Public Structure FTPUploadWorkerArgs
        Private _FileURL As String
        Private _FileToUpload As String
        Private _DestinationPath As String
        Private _DestinationHost As String

        Public Property FileURL As String
            Get
                Return _FileURL
            End Get
            Set
                _FileURL = Value
            End Set
        End Property

        Public Property FileToUpload As String
            Get
                Return _FileToUpload
            End Get
            Set
                _FileToUpload = Value
            End Set
        End Property

        Public Property DestinationPath As String
            Get
                Return _DestinationPath
            End Get
            Set
                _DestinationPath = Value
            End Set
        End Property

        Public Property DestinationHost As String
            Get
                Return _DestinationHost
            End Get
            Set
                _DestinationHost = Value
            End Set
        End Property

    End Structure

    Public Shared Function GetFilenameFromUrl(FileURL As Uri) As String
        Return FileURL.Segments(FileURL.Segments.Length - 1)
    End Function

    Private Sub FTPS5_MountRW(ConsoleIP As String)
        Using conn As New FtpClient(ConsoleIP, "anonymous", "anonymous", 1337)
            'Configurate the FTP connection
            conn.Config.EncryptionMode = FtpEncryptionMode.None
            conn.Config.SslProtocols = SslProtocols.None
            conn.Config.DataConnectionEncryption = False

            'Connect
            conn.Connect()

            'Mount /system & /system_ex with RW permission
            Dim RequestRWReply As FtpReply = Nothing
            RequestRWReply = conn.Execute("MTRW")

            If RequestRWReply.Success() = False Then
                MsgBox("Could not acquire R/W permissions." + vbCrLf + "Writing to /system and /system_ex will not be possible.", MsgBoxStyle.Information)
            End If

            'Disconnect
            conn.Disconnect()
        End Using
    End Sub

#Region "FTP Uploader"

    Private Sub FTPUploadWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles FTPUploadWorker.DoWork
        Dim FTPUploadWrkArgs As FTPUploadWorkerArgs = CType(e.Argument, FTPUploadWorkerArgs)

        Dim NewFTPWebRequest = CType(WebRequest.Create(FTPUploadWrkArgs.FileURL), FtpWebRequest)
        NewFTPWebRequest.Method = WebRequestMethods.Ftp.UploadFile
        NewFTPWebRequest.Timeout = -1 'Do not timeout
        NewFTPWebRequest.KeepAlive = True
        NewFTPWebRequest.UseBinary = True

        Using inputStream = File.OpenRead(FTPUploadWrkArgs.FileToUpload)
            Using outputStream = NewFTPWebRequest.GetRequestStream()
                Dim buffer = New Byte(8191) {}
                Dim totalReadBytesCount As Long = 0
                Dim readBytesCount As Integer = 0

                Do
                    readBytesCount = inputStream.Read(buffer, 0, buffer.Length)
                    If readBytesCount > 0 Then
                        outputStream.Write(buffer, 0, readBytesCount)
                        totalReadBytesCount += readBytesCount
                        FTPUploadWorker.ReportProgress(CInt(totalReadBytesCount * 100.0 / inputStream.Length))
                    End If
                Loop While readBytesCount > 0
            End Using
        End Using
    End Sub

    Private Sub FTPUploadWorker_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles FTPUploadWorker.ProgressChanged
        If FTPTransferProgressBar.Dispatcher.CheckAccess() = False Then
            FTPTransferProgressBar.Dispatcher.BeginInvoke(Sub() FTPTransferProgressBar.Value = e.ProgressPercentage)
        Else
            FTPTransferProgressBar.Value = e.ProgressPercentage
        End If

        If FTPStatusTextBlock.Dispatcher.CheckAccess() = False Then
            FTPStatusTextBlock.Dispatcher.BeginInvoke(Sub() FTPStatusTextBlock.Text = "Uploading - " + e.ProgressPercentage.ToString() + "%")
        Else
            FTPStatusTextBlock.Text = "Uploading - " + e.ProgressPercentage.ToString() + "%"
        End If
    End Sub

    Private Sub FTPUploadWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles FTPUploadWorker.RunWorkerCompleted
        If FTPTransferProgressBar.Dispatcher.CheckAccess() = False Then
            FTPTransferProgressBar.Dispatcher.BeginInvoke(Sub() FTPTransferProgressBar.Value = 0)
        Else
            FTPTransferProgressBar.Value = 0
        End If
        If FTPStatusTextBlock.Dispatcher.CheckAccess() = False Then
            FTPStatusTextBlock.Dispatcher.BeginInvoke(Sub() FTPStatusTextBlock.Text = "Upload finished")
        Else
            FTPStatusTextBlock.Text = "Upload finished"
        End If

        LockUI()

        ListDirContent(ConsoleIPTextBox.Text,
                                       PortTextBox.Text,
                                       CurrentPath + "/")
    End Sub

    Private Sub FTPUploadWorker2_DoWork(sender As Object, e As DoWorkEventArgs) Handles FTPUploadWorker2.DoWork
        Dim FTPUploadWrkArgs As FTPUploadWorkerArgs = CType(e.Argument, FTPUploadWorkerArgs)

        Using conn As New FtpClient(FTPUploadWrkArgs.DestinationHost, "anonymous", "anonymous", 1337)
            'Configurate the FTP connection
            conn.Config.EncryptionMode = FtpEncryptionMode.None
            conn.Config.SslProtocols = SslProtocols.None
            conn.Config.DataConnectionEncryption = False

            'Connect
            conn.Connect()

            'Upload
            Dim progress As Action(Of FtpProgress) =
                    Sub(p As FtpProgress)
                        If p.Progress = 1 Then

                        Else
                            FTPUploadWorker2.ReportProgress(CInt(p.Progress))
                        End If
                    End Sub

            conn.UploadFile(FTPUploadWrkArgs.FileToUpload, FTPUploadWrkArgs.DestinationPath, FtpRemoteExists.NoCheck, True, FtpVerify.None, progress)

            'Disconnect
            conn.Disconnect()
        End Using

    End Sub

    Private Sub FTPUploadWorker2_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles FTPUploadWorker2.ProgressChanged
        If FTPTransferProgressBar.Dispatcher.CheckAccess() = False Then
            FTPTransferProgressBar.Dispatcher.BeginInvoke(Sub() FTPTransferProgressBar.Value = e.ProgressPercentage)
        Else
            FTPTransferProgressBar.Value = e.ProgressPercentage
        End If

        If FTPStatusTextBlock.Dispatcher.CheckAccess() = False Then
            FTPStatusTextBlock.Dispatcher.BeginInvoke(Sub() FTPStatusTextBlock.Text = "Uploading - " + e.ProgressPercentage.ToString() + "%")
        Else
            FTPStatusTextBlock.Text = "Uploading - " + e.ProgressPercentage.ToString() + "%"
        End If
    End Sub

    Private Sub FTPUploadWorker2_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles FTPUploadWorker2.RunWorkerCompleted
        If FTPTransferProgressBar.Dispatcher.CheckAccess() = False Then
            FTPTransferProgressBar.Dispatcher.BeginInvoke(Sub() FTPTransferProgressBar.Value = 0)
        Else
            FTPTransferProgressBar.Value = 0
        End If
        If FTPStatusTextBlock.Dispatcher.CheckAccess() = False Then
            FTPStatusTextBlock.Dispatcher.BeginInvoke(Sub() FTPStatusTextBlock.Text = "Upload finished")
        Else
            FTPStatusTextBlock.Text = "Upload finished"
        End If

        LockUI()

        ListDirContent2(ConsoleIPTextBox.Text, CurrentPath + "/")
    End Sub

#End Region

    Private Sub LockUI()
        If ConsoleIPTextBox.IsEnabled Then
            ConsoleIPTextBox.IsEnabled = False
            PortTextBox.IsEnabled = False
            ConnectButton.IsEnabled = False
            FTPItemsListView.IsEnabled = False
        Else
            ConsoleIPTextBox.IsEnabled = True
            PortTextBox.IsEnabled = True
            ConnectButton.IsEnabled = True
            FTPItemsListView.IsEnabled = True
        End If
    End Sub

    Public Sub ListDirContent(ConsoleIP As String, ConsolePort As String, Optional Dir As String = "")
        Dim NewRequest As FtpWebRequest
        If String.IsNullOrEmpty(Dir) Then
            NewRequest = CType(WebRequest.Create("ftp://" + ConsoleIP + ":" + ConsolePort), FtpWebRequest)
        Else
            NewRequest = CType(WebRequest.Create("ftp://" + ConsoleIP + ":" + ConsolePort + Dir), FtpWebRequest)
        End If

        NewRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails
        NewRequest.Credentials = New NetworkCredential("anonymous", "")

        Try
            Dim RequestResponse As FtpWebResponse = CType(NewRequest.GetResponse(), FtpWebResponse)
            Dim ResponseStream As Stream = RequestResponse.GetResponseStream()
            Dim NewStreamReader As New StreamReader(ResponseStream)

            FTPItemsListView.Items.Clear()

            For Each Line As String In NewStreamReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

                Dim FileNameRegex As New Regex("^(?:[^ ]+ +){8}(.*)$")
                Dim RegexMatch As Match = FileNameRegex.Match(Line)
                Dim FoundFileName As String = RegexMatch.Groups(1).Value

                Dim NewFTPLVItem As New FTPListViewItem()

                If Line.StartsWith("d") Then
                    Dim SplittedValues As String() = Line.Split(" "c)

                    If String.IsNullOrWhiteSpace(SplittedValues(6)) Then
                        NewFTPLVItem.FileOrDirName = SplittedValues(9)
                        NewFTPLVItem.FileOrDirLastModified = SplittedValues(5) + " " + SplittedValues(7) + " " + SplittedValues(8)
                    Else
                        NewFTPLVItem.FileOrDirName = SplittedValues(8)
                        NewFTPLVItem.FileOrDirLastModified = SplittedValues(5) + " " + SplittedValues(6) + " " + SplittedValues(7)
                    End If

                    NewFTPLVItem.FileOrDirSize = FormatNumber(CLng(SplittedValues(4)) / 1048576, 2) + " MB"
                    NewFTPLVItem.FileOrDirPermissions = SplittedValues(0)
                    NewFTPLVItem.FileOrDirOwner = SplittedValues(2) + " " + SplittedValues(3)
                    NewFTPLVItem.FileOrDirType = "Folder"
                ElseIf Line.StartsWith("-") Then
                    Dim SplittedValues As String() = Line.Split(" "c)

                    If String.IsNullOrWhiteSpace(SplittedValues(6)) Then
                        NewFTPLVItem.FileOrDirLastModified = SplittedValues(5) + " " + SplittedValues(7) + " " + SplittedValues(8)
                    Else
                        NewFTPLVItem.FileOrDirLastModified = SplittedValues(5) + " " + SplittedValues(6) + " " + SplittedValues(7)
                    End If

                    NewFTPLVItem.FileOrDirName = FoundFileName
                    NewFTPLVItem.FileOrDirSize = FormatNumber(CLng(SplittedValues(4)) / 1048576, 2) + " MB"
                    NewFTPLVItem.FileOrDirPermissions = SplittedValues(0)
                    NewFTPLVItem.FileOrDirOwner = SplittedValues(2) + " " + SplittedValues(3)
                    NewFTPLVItem.FileOrDirType = "File"
                End If

                If Not NewFTPLVItem.FileOrDirName = "." Then
                    FTPItemsListView.Items.Add(NewFTPLVItem)
                End If
            Next

            NewStreamReader.Close()
            RequestResponse.Close()
        Catch ex As WebException
            FTPStatusTextBlock.Text = CType(ex.Response, FtpWebResponse).StatusDescription + " - Please reconnect"
        End Try
    End Sub

    Public Sub ListDirContent2(ConsoleIP As String, Optional Dir As String = "")

        FTPItemsListView.Items.Clear()

        Using conn As New FtpClient(ConsoleIP, "anonymous", "anonymous", 1337)

            'Configurate the FTP connection
            conn.Config.EncryptionMode = FtpEncryptionMode.None
            conn.Config.SslProtocols = SslProtocols.None
            conn.Config.DataConnectionEncryption = False

            'Connect
            conn.Connect()

            'List disc directory
            For Each item In conn.GetListing(Dir)

                Dim NewFTPLVItem As New FTPListViewItem()

                Select Case item.Type
                    Case FtpObjectType.Directory
                        NewFTPLVItem.FileOrDirType = "Folder"
                    Case FtpObjectType.File
                        NewFTPLVItem.FileOrDirType = "File"
                    Case FtpObjectType.Link
                        NewFTPLVItem.FileOrDirType = "Link"
                End Select

                NewFTPLVItem.FileOrDirName = item.Name
                NewFTPLVItem.FileOrDirSize = FormatNumber(item.Size / 1048576, 2) + " MB"
                NewFTPLVItem.FileOrDirPermissions = item.RawPermissions
                NewFTPLVItem.FileOrDirOwner = item.RawOwner

                If Not NewFTPLVItem.FileOrDirName = "." Then
                    FTPItemsListView.Items.Add(NewFTPLVItem)
                End If

            Next

            'Disonnect
            conn.Disconnect()
        End Using
    End Sub

    Public Sub RenameContent(ConsoleIP As String, ConsolePort As String, FileOrDir As String, RenameTo As String)
        Dim NewFTPRequest As FtpWebRequest
        NewFTPRequest = CType(WebRequest.Create("ftp://" + ConsoleIP + ":" + ConsolePort + FileOrDir), FtpWebRequest)
        NewFTPRequest.Method = WebRequestMethods.Ftp.Rename
        NewFTPRequest.Credentials = New NetworkCredential("anonymous", "")
        NewFTPRequest.RenameTo = CurrentPath + "/" + RenameTo

        Dim FTPResponse As FtpWebResponse = CType(NewFTPRequest.GetResponse(), FtpWebResponse)
        FTPStatusTextBlock.Text = FTPResponse.StatusDescription
        FTPResponse.Close()
    End Sub

    Public Sub RenameContent2(ConsoleIP As String, RemotePath As String, RenameTo As String, IsFile As Boolean)
        Try
            Using conn As New FtpClient(ConsoleIP, "anonymous", "anonymous", 1337)
                'Configurate the FTP connection
                conn.Config.EncryptionMode = FtpEncryptionMode.None
                conn.Config.SslProtocols = SslProtocols.None
                conn.Config.DataConnectionEncryption = False

                'Connect
                conn.Connect()

                If IsFile = True Then
                    'Rename the file
                    conn.MoveFile(RemotePath, RenameTo, FtpRemoteExists.NoCheck)
                Else
                    'Rename the directory
                    conn.MoveDirectory(RemotePath, RenameTo, FtpRemoteExists.NoCheck)
                End If

                'Disconnect
                conn.Disconnect()
            End Using
        Catch ex As Exception
            MsgBox("Could not delete the selected file or folder", MsgBoxStyle.Critical)
        End Try
    End Sub

    Public Sub DownloadContent(ConsoleIP As String, ConsolePort As String, FileOrDir As String)
        Dim NewDownloader As New Downloader() With {.ShowActivated = True}
        NewDownloader.Show()
        If NewDownloader.CreateNewDownload("ftp://" + ConsoleIP + ":" + ConsolePort + FileOrDir) = False Then
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
            NewDownloader.Close()
        End If
    End Sub

    Public Sub DownloadContent2(ConsoleIP As String, FileOrDir As String)
        If Not Directory.Exists(Environment.CurrentDirectory + "\Downloads") Then Directory.CreateDirectory(Environment.CurrentDirectory + "\Downloads")

        Dim FileName As String = FileOrDir.Split("/"c).Last()
        If Not String.IsNullOrEmpty(FileName) Then
            Using conn As New FtpClient(ConsoleIP, "anonymous", "anonymous", 1337)
                'Configurate the FTP connection
                conn.Config.EncryptionMode = FtpEncryptionMode.None
                conn.Config.SslProtocols = SslProtocols.None
                conn.Config.DataConnectionEncryption = False

                'Connect
                conn.Connect()

                'Get the file
                conn.DownloadFile(Environment.CurrentDirectory + "\Downloads\" + FileName, FileOrDir, FtpLocalExists.Overwrite)

                'Disconnect
                conn.Disconnect()
            End Using

            If MsgBox("Download completed. Open the Downloads folder ?", MsgBoxStyle.YesNo, "Completed") = MsgBoxResult.Yes Then
                Process.Start("explorer", Environment.CurrentDirectory + "\Downloads")
            End If
        Else
            MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
        End If

    End Sub

    Public Sub NewFTPFolder(ConsoleIP As String, ConsolePort As String, DirName As String)
        Dim NewFTPRequest As FtpWebRequest
        NewFTPRequest = CType(WebRequest.Create("ftp://" + ConsoleIP + ":" + ConsolePort + DirName), FtpWebRequest)
        NewFTPRequest.Method = WebRequestMethods.Ftp.MakeDirectory
        NewFTPRequest.Credentials = New NetworkCredential("anonymous", "")

        Dim FTPResponse As FtpWebResponse = CType(NewFTPRequest.GetResponse(), FtpWebResponse)
        FTPStatusTextBlock.Text = FTPResponse.StatusDescription
        FTPResponse.Close()
    End Sub

    Public Sub NewFTPFolder2(ConsoleIP As String, DirName As String)
        Using conn As New FtpClient(ConsoleIP, "anonymous", "anonymous", 1337)
            'Configurate the FTP connection
            conn.Config.EncryptionMode = FtpEncryptionMode.None
            conn.Config.SslProtocols = SslProtocols.None
            conn.Config.DataConnectionEncryption = False

            'Connect
            conn.Connect()

            'Create the directory
            If conn.CreateDirectory(DirName, True) = True Then
                FTPStatusTextBlock.Text = DirName + " created."
            Else
                FTPStatusTextBlock.Text = "Could not create the directory " + DirName
            End If

            'Disconnect
            conn.Disconnect()
        End Using
    End Sub

    Public Sub DeleteContent(ConsoleIP As String, ConsolePort As String, FileOrDirName As String)
        Dim NewFTPRequest As FtpWebRequest
        NewFTPRequest = CType(WebRequest.Create("ftp://" + ConsoleIP + ":" + ConsolePort + FileOrDirName), FtpWebRequest)
        NewFTPRequest.Method = WebRequestMethods.Ftp.DeleteFile
        NewFTPRequest.Credentials = New NetworkCredential("anonymous", "")

        Dim FTPResponse As FtpWebResponse = CType(NewFTPRequest.GetResponse(), FtpWebResponse)
        FTPStatusTextBlock.Text = FTPResponse.StatusDescription
        FTPResponse.Close()
    End Sub

    Public Sub DeleteContent2(ConsoleIP As String, FileOrDirName As String, IsFile As Boolean)
        Try
            Using conn As New FtpClient(ConsoleIP, "anonymous", "anonymous", 1337)
                'Configurate the FTP connection
                conn.Config.EncryptionMode = FtpEncryptionMode.None
                conn.Config.SslProtocols = SslProtocols.None
                conn.Config.DataConnectionEncryption = False

                'Connect
                conn.Connect()

                If IsFile = True Then
                    'Delete the file
                    conn.DeleteFile(FileOrDirName)
                Else
                    'Delete the directory
                    conn.DeleteDirectory(FileOrDirName)
                End If

                'Disconnect
                conn.Disconnect()
            End Using
        Catch ex As Exception
            MsgBox("Could not delete the selected file or folder", MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub FTPBrowser_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim FTPContentListViewContextMenu As New Controls.ContextMenu()
        FTPContentListViewContextMenu.Items.Add(DownloadMenuItem)
        FTPContentListViewContextMenu.Items.Add(UploadMenuItem)
        FTPContentListViewContextMenu.Items.Add(DeleteMenuItem)
        FTPContentListViewContextMenu.Items.Add(RenameMenuItem)
        FTPContentListViewContextMenu.Items.Add(NewDirectoryMenuItem)
        FTPItemsListView.ContextMenu = FTPContentListViewContextMenu
    End Sub

    Private Sub ConnectButton_Click(sender As Object, e As RoutedEventArgs) Handles ConnectButton.Click
        If Not String.IsNullOrEmpty(ConsoleIPTextBox.Text) And Not String.IsNullOrEmpty(PortTextBox.Text) Then

            If FTPS5Mode = False Then
                ListDirContent(ConsoleIPTextBox.Text, PortTextBox.Text)
            Else
                FTPS5_MountRW(ConsoleIPTextBox.Text)
                ListDirContent2(ConsoleIPTextBox.Text, "/")
            End If

            CurrentPath = "/"
            CurrentDirTextBlock.Text = CurrentPath
        End If
    End Sub

    Private Sub FTPItemsListView_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles FTPItemsListView.MouseDoubleClick
        If FTPItemsListView.SelectedItem IsNot Nothing Then
            Dim SelectedFTPLVItem As FTPListViewItem = CType(FTPItemsListView.SelectedItem, FTPListViewItem)

            If FTPS5Mode = False Then
                If SelectedFTPLVItem.FileOrDirType = "Folder" Then
                    If SelectedFTPLVItem.FileOrDirName = ".." Then
                        If Not CurrentPath = "/" Then
                            CurrentPath = CurrentPath.Remove(CurrentPath.LastIndexOf("/"c))
                            ListDirContent(ConsoleIPTextBox.Text, PortTextBox.Text, CurrentPath)
                        End If
                    Else
                        If CurrentPath = "/" Then
                            CurrentPath = "/" + SelectedFTPLVItem.FileOrDirName
                            ListDirContent(ConsoleIPTextBox.Text, PortTextBox.Text, CurrentPath)
                        Else
                            CurrentPath = CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName
                            ListDirContent(ConsoleIPTextBox.Text, PortTextBox.Text, CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName)
                        End If
                    End If
                Else
                    If MsgBox("Do you want to download the selected file ?", MsgBoxStyle.YesNo, "Download file") = MsgBoxResult.Yes Then
                        DownloadContent(ConsoleIPTextBox.Text, PortTextBox.Text, CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName)
                    End If
                End If
            Else
                If SelectedFTPLVItem.FileOrDirType = "Folder" Then
                    If SelectedFTPLVItem.FileOrDirName = ".." Then
                        If Not CurrentPath = "/" Then
                            CurrentPath = CurrentPath.Remove(CurrentPath.LastIndexOf("/"c))
                            ListDirContent2(ConsoleIPTextBox.Text, CurrentPath)
                        End If
                    Else
                        If CurrentPath = "/" Then
                            CurrentPath = "/" + SelectedFTPLVItem.FileOrDirName
                            ListDirContent2(ConsoleIPTextBox.Text, CurrentPath)
                        Else
                            CurrentPath = CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName
                            ListDirContent2(ConsoleIPTextBox.Text, CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName)
                        End If
                    End If
                Else
                    If MsgBox("Do you want to download the selected file ?", MsgBoxStyle.YesNo, "Download file") = MsgBoxResult.Yes Then
                        DownloadContent2(ConsoleIPTextBox.Text, CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName)
                    End If
                End If
            End If

            CurrentDirTextBlock.Text = CurrentPath
        End If
    End Sub

    Private Sub RenameMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles RenameMenuItem.Click
        If FTPItemsListView.SelectedItem IsNot Nothing Then
            Dim SelectedFTPLVItem As FTPListViewItem = CType(FTPItemsListView.SelectedItem, FTPListViewItem)

            If FTPS5Mode = False Then
                If Not SelectedFTPLVItem.FileOrDirName = ".." Then

                    Dim NewInputDialog As New InputDialog() With {.Title = "FTP Browser"}
                    NewInputDialog.InputDialogTitleTextBlock.Text = "Please enter a new name"
                    NewInputDialog.NewValueTextBox.Text = SelectedFTPLVItem.FileOrDirName

                    If NewInputDialog.ShowDialog() = True Then
                        Dim InputDialogResult As String
                        InputDialogResult = NewInputDialog.NewValueTextBox.Text

                        If CurrentPath = "/" Then
                            RenameContent(ConsoleIPTextBox.Text, PortTextBox.Text, CurrentPath + SelectedFTPLVItem.FileOrDirName, InputDialogResult)
                        Else
                            RenameContent(ConsoleIPTextBox.Text, PortTextBox.Text, CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName, InputDialogResult)
                        End If

                        ListDirContent(ConsoleIPTextBox.Text, PortTextBox.Text, CurrentPath + "/")
                    End If
                End If
            Else
                If Not SelectedFTPLVItem.FileOrDirName = ".." Then

                    Dim NewInputDialog As New InputDialog() With {.Title = "FTP Browser"}
                    NewInputDialog.InputDialogTitleTextBlock.Text = "Please enter a new name"
                    NewInputDialog.NewValueTextBox.Text = SelectedFTPLVItem.FileOrDirName

                    If NewInputDialog.ShowDialog() = True Then
                        Dim InputDialogResult As String
                        InputDialogResult = NewInputDialog.NewValueTextBox.Text

                        If SelectedFTPLVItem.FileOrDirType = "Folder" Then
                            If CurrentPath = "/" Then
                                RenameContent2(ConsoleIPTextBox.Text, CurrentPath + SelectedFTPLVItem.FileOrDirName, CurrentPath + InputDialogResult, False)
                            Else
                                RenameContent2(ConsoleIPTextBox.Text, CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName, CurrentPath + "/" + InputDialogResult, False)
                            End If
                        Else
                            If CurrentPath = "/" Then
                                RenameContent2(ConsoleIPTextBox.Text, CurrentPath + SelectedFTPLVItem.FileOrDirName, CurrentPath + InputDialogResult, True)
                            Else
                                RenameContent2(ConsoleIPTextBox.Text, CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName, CurrentPath + "/" + InputDialogResult, True)
                            End If
                        End If

                        ListDirContent2(ConsoleIPTextBox.Text, CurrentPath + "/")
                    End If
                End If
            End If

        End If
    End Sub

    Private Sub DownloadMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMenuItem.Click
        If FTPItemsListView.SelectedItem IsNot Nothing Then
            Dim SelectedFTPLVItem As FTPListViewItem = CType(FTPItemsListView.SelectedItem, FTPListViewItem)

            If FTPS5Mode = False Then
                If CurrentPath = "/" Then
                    DownloadContent(ConsoleIPTextBox.Text, PortTextBox.Text, CurrentPath + SelectedFTPLVItem.FileOrDirName)
                Else
                    DownloadContent(ConsoleIPTextBox.Text, PortTextBox.Text, CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName)
                End If
            Else
                If CurrentPath = "/" Then
                    DownloadContent2(ConsoleIPTextBox.Text, CurrentPath + SelectedFTPLVItem.FileOrDirName)
                Else
                    DownloadContent2(ConsoleIPTextBox.Text, CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName)
                End If
            End If

        End If
    End Sub

    Private Sub UploadMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles UploadMenuItem.Click
        If FTPItemsListView.Items.Count >= 1 Then
            Dim OFD As New OpenFileDialog() With {.Multiselect = False}
            If OFD.ShowDialog() = Forms.DialogResult.OK Then
                Dim SafeUriString As String

                If CurrentPath = "/" Then
                    SafeUriString = Uri.EscapeDataString(Encoding.UTF8.GetString(Encoding.ASCII.GetBytes("ftp://" & ConsoleIPTextBox.Text & ":" + PortTextBox.Text & CurrentPath & OFD.SafeFileName)))
                Else
                    SafeUriString = Uri.EscapeDataString(Encoding.UTF8.GetString(Encoding.ASCII.GetBytes("ftp://" & ConsoleIPTextBox.Text & ":" + PortTextBox.Text & CurrentPath & "/" & OFD.SafeFileName)))
                End If

                LockUI()

                Dim NewFTPUploadWorkerArgs As New FTPUploadWorkerArgs() With {.FileToUpload = OFD.FileName, .FileURL = SafeUriString}
                If FTPS5Mode = False Then
                    FTPUploadWorker.RunWorkerAsync(NewFTPUploadWorkerArgs)
                Else
                    NewFTPUploadWorkerArgs.DestinationHost = ConsoleIPTextBox.Text
                    If CurrentPath.EndsWith("/"c) Then
                        NewFTPUploadWorkerArgs.DestinationPath = CurrentPath + OFD.SafeFileName
                    Else
                        NewFTPUploadWorkerArgs.DestinationPath = CurrentPath + "/" + OFD.SafeFileName
                    End If

                    FTPUploadWorker2.RunWorkerAsync(NewFTPUploadWorkerArgs)
                End If
            End If
        End If
    End Sub

    Private Sub NewDirectoryMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles NewDirectoryMenuItem.Click
        If FTPItemsListView.SelectedItem IsNot Nothing Then
            Dim SelectedFTPLVItem As FTPListViewItem = CType(FTPItemsListView.SelectedItem, FTPListViewItem)

            If FTPS5Mode = False Then
                If Not SelectedFTPLVItem.FileOrDirName = ".." Then
                    Dim NewInputDialog As New InputDialog() With {.Title = "FTP Browser"}
                    NewInputDialog.InputDialogTitleTextBlock.Text = "Please enter a name for the new folder"
                    NewInputDialog.NewValueTextBox.Text = SelectedFTPLVItem.FileOrDirName

                    If NewInputDialog.ShowDialog() = True Then
                        Dim InputDialogResult As String = NewInputDialog.NewValueTextBox.Text

                        If CurrentPath = "/" Then
                            NewFTPFolder(ConsoleIPTextBox.Text, PortTextBox.Text, CurrentPath + InputDialogResult)
                        Else
                            NewFTPFolder(ConsoleIPTextBox.Text, PortTextBox.Text, CurrentPath + "/" + InputDialogResult)
                        End If

                        ListDirContent(ConsoleIPTextBox.Text, PortTextBox.Text, CurrentPath + "/")
                    End If
                End If
            Else
                If Not SelectedFTPLVItem.FileOrDirName = ".." Then
                    Dim NewInputDialog As New InputDialog() With {.Title = "FTP Browser"}
                    NewInputDialog.InputDialogTitleTextBlock.Text = "Please enter a name for the new folder"
                    NewInputDialog.NewValueTextBox.Text = SelectedFTPLVItem.FileOrDirName

                    If NewInputDialog.ShowDialog() = True Then
                        Dim InputDialogResult As String = NewInputDialog.NewValueTextBox.Text

                        If CurrentPath = "/" Then
                            NewFTPFolder2(ConsoleIPTextBox.Text, CurrentPath + InputDialogResult)
                        Else
                            NewFTPFolder2(ConsoleIPTextBox.Text, CurrentPath + "/" + InputDialogResult)
                        End If

                        ListDirContent2(ConsoleIPTextBox.Text, CurrentPath + "/")
                    End If
                End If
            End If

        End If
    End Sub

    Private Sub DeleteMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles DeleteMenuItem.Click
        If FTPItemsListView.SelectedItem IsNot Nothing Then
            Dim SelectedFTPLVItem As FTPListViewItem = CType(FTPItemsListView.SelectedItem, FTPListViewItem)

            If FTPS5Mode = False Then
                If CurrentPath = "/" Then
                    DeleteContent(ConsoleIPTextBox.Text, PortTextBox.Text, CurrentPath + SelectedFTPLVItem.FileOrDirName)
                Else
                    DeleteContent(ConsoleIPTextBox.Text, PortTextBox.Text, CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName)
                End If

                ListDirContent(ConsoleIPTextBox.Text, PortTextBox.Text, CurrentPath + "/")
            Else
                If SelectedFTPLVItem.FileOrDirType = "Folder" Then
                    If CurrentPath = "/" Then
                        DeleteContent2(ConsoleIPTextBox.Text, CurrentPath + SelectedFTPLVItem.FileOrDirName, False)
                    Else
                        DeleteContent2(ConsoleIPTextBox.Text, CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName, False)
                    End If

                    ListDirContent2(ConsoleIPTextBox.Text, CurrentPath + "/")
                Else
                    If CurrentPath = "/" Then
                        DeleteContent2(ConsoleIPTextBox.Text, CurrentPath + SelectedFTPLVItem.FileOrDirName, True)
                    Else
                        DeleteContent2(ConsoleIPTextBox.Text, CurrentPath + "/" + SelectedFTPLVItem.FileOrDirName, True)
                    End If

                    ListDirContent2(ConsoleIPTextBox.Text, CurrentPath + "/")
                End If
            End If

        End If
    End Sub

End Class
