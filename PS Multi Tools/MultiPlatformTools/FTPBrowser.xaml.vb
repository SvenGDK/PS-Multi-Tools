Imports System.IO
Imports System.Security.Authentication
Imports FluentFTP
Imports PS_Multi_Tools.Structures

Public Class FTPBrowser

    Public ConsoleIP As String = ""
    Public ConsoleFTPPort As String = ""
    Public CurrentPath As String = ""

    Dim IsConnected As Boolean = False
    Dim NewFtpConfig As FtpConfig

    Dim WithEvents DownloadMenuItem As New MenuItem() With {.Header = "Download selected file or folder", .Icon = New Image() With {.Source = New BitmapImage(New Uri("/Images/download.png", UriKind.RelativeOrAbsolute))}}
    Dim WithEvents UploadFileMenuItem As New MenuItem() With {.Header = "Upload a file", .Icon = New Image() With {.Source = New BitmapImage(New Uri("/Images/upload.png", UriKind.RelativeOrAbsolute))}}
    Dim WithEvents UploadFolderMenuItem As New MenuItem() With {.Header = "Upload a folder", .Icon = New Image() With {.Source = New BitmapImage(New Uri("/Images/upload.png", UriKind.RelativeOrAbsolute))}}
    Dim WithEvents DeleteMenuItem As New MenuItem() With {.Header = "Delete selected file or folder", .Icon = New Image() With {.Source = New BitmapImage(New Uri("/Images/delete.png", UriKind.RelativeOrAbsolute))}}
    Dim WithEvents RenameMenuItem As New MenuItem() With {.Header = "Rename selected file or folder", .Icon = New Image() With {.Source = New BitmapImage(New Uri("/Images/rename.png", UriKind.RelativeOrAbsolute))}}
    Dim WithEvents NewDirectoryMenuItem As New MenuItem() With {.Header = "Create a new directory", .Icon = New Image() With {.Source = New BitmapImage(New Uri("/Images/NewFolder.png", UriKind.RelativeOrAbsolute))}}

    Private Sub FTPBrowser_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim FTPContentListViewContextMenu As New ContextMenu()
        FTPContentListViewContextMenu.Items.Add(DownloadMenuItem)
        FTPContentListViewContextMenu.Items.Add(UploadFileMenuItem)
        FTPContentListViewContextMenu.Items.Add(UploadFolderMenuItem)
        FTPContentListViewContextMenu.Items.Add(DeleteMenuItem)
        FTPContentListViewContextMenu.Items.Add(RenameMenuItem)
        FTPContentListViewContextMenu.Items.Add(NewDirectoryMenuItem)
        FTPItemsListView.ContextMenu = FTPContentListViewContextMenu

        If Not String.IsNullOrEmpty(ConsoleIP) Then
            ConsoleIPTextBox.Text = ConsoleIP
        End If
        If Not String.IsNullOrEmpty(ConsoleFTPPort) Then
            PortTextBox.Text = ConsoleFTPPort
        End If

        NewFtpConfig = New FtpConfig() With {.EncryptionMode = FtpEncryptionMode.None, .SslProtocols = SslProtocols.None, .DataConnectionEncryption = False}
    End Sub

    Private Async Sub ConnectButton_Click(sender As Object, e As RoutedEventArgs) Handles ConnectButton.Click
        If Not String.IsNullOrEmpty(ConsoleIPTextBox.Text) And Not String.IsNullOrEmpty(PortTextBox.Text) Then
            If ConnectButton.Content.ToString() = "Connect and list content" Then
                If Await ListDirectoryContent("/") Then
                    IsConnected = True
                    ConnectButton.Content = "Disconnect"
                    CurrentPath = "/"
                    CurrentDirTextBlock.Text = "Current directory : " & CurrentPath
                End If
            Else
                IsConnected = False
                FTPItemsListView.Items.Clear()
                ConnectButton.Content = "Connect and list content"
                FTPStatusTextBlock.Text = ""
                CurrentPath = "/"
                CurrentDirTextBlock.Text = "Current directory : " & CurrentPath
            End If
        End If
    End Sub

#Region "FTP Uploader"

    Private Async Function UploadFileAsync(LocalFilePath As String, RemoteDestinationPath As String) As Task(Of Boolean)
        Try
            Using NewFtpClient As New AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", CInt(ConsoleFTPPort), NewFtpConfig)
                'Connect
                Await NewFtpClient.Connect()

                'Upload progress
                Dim UPProgress As New Progress(Of FtpProgress)(Sub(p)
                                                                   If p.Progress = 100 Then
                                                                       Dispatcher.BeginInvoke(Sub()
                                                                                                  FTPTransferProgressBar.Value = 0
                                                                                                  FTPStatusTextBlock.Text = "Uploading finished"
                                                                                              End Sub)
                                                                   Else
                                                                       Dispatcher.BeginInvoke(Sub()
                                                                                                  FTPTransferProgressBar.Value = p.Progress
                                                                                                  FTPStatusTextBlock.Text = "Uploading - " & p.Progress.ToString("F0") & "%"
                                                                                              End Sub)
                                                                   End If
                                                               End Sub)

                Await NewFtpClient.UploadFile(LocalFilePath, RemoteDestinationPath, FtpRemoteExists.OverwriteInPlace, False, FtpVerify.None, UPProgress)

                'Disconnect
                Await NewFtpClient.Disconnect()
                Return True
            End Using
        Catch ex As Exception
            MsgBox("Could not upload the selected file." & vbCrLf & ex.Message, MsgBoxStyle.Critical)
            Return False
        End Try
    End Function

    Private Async Function UploadFolderAsync(LocalDirectoryPath As String, RemoteDestinationPath As String) As Task(Of Boolean)
        Try
            Using NewFtpClient As New AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", CInt(ConsoleFTPPort), NewFtpConfig)
                'Connect
                Await NewFtpClient.Connect()

                'Upload progress
                Dim UPProgress As New Progress(Of FtpProgress)(Sub(p)
                                                                   If p.Progress = 100 Then
                                                                       Dispatcher.BeginInvoke(Sub()
                                                                                                  FTPTransferProgressBar.Value = 0
                                                                                                  FTPStatusTextBlock.Text = "Uploading finished"
                                                                                              End Sub)
                                                                   Else
                                                                       Dispatcher.BeginInvoke(Sub()
                                                                                                  FTPTransferProgressBar.Value = p.Progress
                                                                                                  FTPStatusTextBlock.Text = "Uploading - " & p.Progress.ToString("F0") & "%"
                                                                                              End Sub)
                                                                   End If
                                                               End Sub)

                Await NewFtpClient.UploadDirectory(LocalDirectoryPath, RemoteDestinationPath, FtpFolderSyncMode.Update, FtpRemoteExists.OverwriteInPlace, FtpVerify.None, Nothing, UPProgress)

                'Disconnect
                Await NewFtpClient.Disconnect()
                Return True
            End Using
        Catch ex As Exception
            MsgBox("Could not upload the selected folder." & vbCrLf & ex.Message, MsgBoxStyle.Critical)
            Return False
        End Try
    End Function

#End Region

#Region "FTP Downloader"

    Private Async Function DownloadFileAsync(LocalFilePath As String, RemoteFilePath As String) As Task(Of Boolean)
        Try
            Using NewFtpClient As New AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", CInt(ConsoleFTPPort), NewFtpConfig)
                'Connect
                Await NewFtpClient.Connect()

                'Download progress
                Dim DLProgress As New Progress(Of FtpProgress)(Sub(p)
                                                                   If p.Progress = 1 Then
                                                                       Dispatcher.BeginInvoke(Sub()
                                                                                                  FTPTransferProgressBar.Value = 0
                                                                                                  FTPStatusTextBlock.Text = "Download finished"
                                                                                              End Sub)
                                                                   Else Dispatcher.BeginInvoke(Sub()
                                                                                                   FTPTransferProgressBar.Value = p.Progress * 100
                                                                                                   FTPStatusTextBlock.Text = "Downloading - " & (p.Progress * 100).ToString("F0") & "%"
                                                                                               End Sub)
                                                                   End If
                                                               End Sub)

                Await NewFtpClient.DownloadFile(LocalFilePath, RemoteFilePath, FtpLocalExists.Overwrite, FtpVerify.None, DLProgress)

                'Disconnect
                Await NewFtpClient.Disconnect()
                Return True
            End Using
        Catch ex As Exception
            MsgBox("Could not download the selected file." & vbCrLf & ex.Message, MsgBoxStyle.Critical)
            Return False
        End Try
    End Function

    Private Async Function DownloadFolderAsync(LocalDirectoryPath As String, RemoteDirectoryPath As String) As Task(Of Boolean)
        Try
            Using NewFtpClient As New AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", CInt(ConsoleFTPPort), NewFtpConfig)
                'Connect
                Await NewFtpClient.Connect()

                'Download progress
                Dim DLProgress As New Progress(Of FtpProgress)(Sub(p)
                                                                   If p.Progress = 100 Then
                                                                       Dispatcher.BeginInvoke(Sub()
                                                                                                  FTPTransferProgressBar.Value = 0
                                                                                                  FTPStatusTextBlock.Text = "Download finished"
                                                                                              End Sub)
                                                                   Else
                                                                       Dispatcher.BeginInvoke(Sub()
                                                                                                  FTPTransferProgressBar.Value = p.Progress
                                                                                                  FTPStatusTextBlock.Text = "Downloading - " & p.Progress.ToString("F0") & "%"
                                                                                              End Sub)
                                                                   End If
                                                               End Sub)

                Await NewFtpClient.DownloadDirectory(LocalDirectoryPath, RemoteDirectoryPath, FtpFolderSyncMode.Update, FtpLocalExists.Overwrite, FtpVerify.None, Nothing, DLProgress)

                'Disconnect
                Await NewFtpClient.Disconnect()
                Return True
            End Using
        Catch ex As Exception
            MsgBox("Could not download the selected folder." & vbCrLf & ex.Message, MsgBoxStyle.Critical)
            Return False
        End Try
    End Function

#End Region

#Region "Context Menu Actions"

    Private Async Sub RenameMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles RenameMenuItem.Click
        If FTPItemsListView.SelectedItem IsNot Nothing Then
            Dim SelectedFTPLVItem As FTPListViewItem = CType(FTPItemsListView.SelectedItem, FTPListViewItem)

            If Not SelectedFTPLVItem.FileOrDirName = ".." Then
                Dim NewInputDialog As New InputDialog() With {.Title = "FTP Browser"}
                NewInputDialog.InputDialogTitleTextBlock.Text = "Enter a new name:"
                NewInputDialog.NewValueTextBox.Text = SelectedFTPLVItem.FileOrDirName

                If NewInputDialog.ShowDialog() = True Then
                    Dim InputDialogResult As String
                    InputDialogResult = NewInputDialog.NewValueTextBox.Text

                    If SelectedFTPLVItem.FileOrDirType = "Folder" Then
                        If CurrentPath = "/" Then
                            Await RenameContent(CurrentPath & SelectedFTPLVItem.FileOrDirName, CurrentPath & InputDialogResult, False)
                        Else
                            Await RenameContent(CurrentPath & "/" & SelectedFTPLVItem.FileOrDirName, CurrentPath & "/" & InputDialogResult, False)
                        End If
                    Else
                        If CurrentPath = "/" Then
                            Await RenameContent(CurrentPath & SelectedFTPLVItem.FileOrDirName, CurrentPath & InputDialogResult, True)
                        Else
                            Await RenameContent(CurrentPath & "/" & SelectedFTPLVItem.FileOrDirName, CurrentPath & "/" & InputDialogResult, True)
                        End If
                    End If

                    Await ListDirectoryContent(CurrentPath & "/")
                End If
            End If
        End If
    End Sub

    Private Async Sub DownloadMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles DownloadMenuItem.Click
        If FTPItemsListView.SelectedItem IsNot Nothing Then
            Dim SelectedFTPLVItem As FTPListViewItem = CType(FTPItemsListView.SelectedItem, FTPListViewItem)
            Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Select a folder where the file/folder should be downloaded.", .ShowNewFolderButton = True}

            If FBD.ShowDialog() = Forms.DialogResult.OK Then
                Dim DestinationPath As String = FBD.SelectedPath & "\" & SelectedFTPLVItem.FileOrDirName
                If CurrentPath = "/" Then
                    If SelectedFTPLVItem.FileOrDirType = "Folder" Then
                        Await DownloadFolderAsync(DestinationPath, CurrentPath & SelectedFTPLVItem.FileOrDirName)
                    Else
                        Await DownloadFileAsync(DestinationPath, CurrentPath & SelectedFTPLVItem.FileOrDirName)
                    End If
                Else
                    If SelectedFTPLVItem.FileOrDirType = "Folder" Then
                        Await DownloadFolderAsync(DestinationPath, CurrentPath & "/" & SelectedFTPLVItem.FileOrDirName)
                    Else
                        Await DownloadFileAsync(DestinationPath, CurrentPath & "/" & SelectedFTPLVItem.FileOrDirName)
                    End If
                End If
            End If
        End If
    End Sub

    Private Async Sub UploadFileMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles UploadFileMenuItem.Click
        If IsConnected Then
            Dim OFD As New Forms.OpenFileDialog() With {.Multiselect = True}
            If OFD.ShowDialog() = Forms.DialogResult.OK Then

                LockUI()

                Dim DestinationPath As String
                If OFD.FileNames.Length > 1 Then
                    For Each SelectedItem In OFD.FileNames
                        Dim FileName As String = Path.GetFileName(SelectedItem)

                        If CurrentPath.EndsWith("/"c) Then
                            DestinationPath = CurrentPath & FileName
                        Else
                            DestinationPath = CurrentPath & "/" & FileName
                        End If

                        Await UploadFileAsync(SelectedItem, DestinationPath)
                    Next
                Else
                    If CurrentPath.EndsWith("/"c) Then
                        DestinationPath = CurrentPath & OFD.SafeFileName
                    Else
                        DestinationPath = CurrentPath & "/" & OFD.SafeFileName
                    End If

                    Await UploadFileAsync(OFD.FileName, DestinationPath)
                End If

                Await ListDirectoryContent(CurrentPath & "/")

                LockUI()
            End If
        End If
    End Sub

    Private Async Sub UploadFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles UploadFolderMenuItem.Click
        If IsConnected Then
            Dim FBD As New Forms.FolderBrowserDialog() With {.Multiselect = True}
            If FBD.ShowDialog() = Forms.DialogResult.OK Then

                LockUI()

                Dim DestinationPath As String
                If FBD.SelectedPaths.Length > 1 Then
                    For Each SelectedItem In FBD.SelectedPaths
                        Dim FolderName As String = Path.GetFileName(SelectedItem)

                        If CurrentPath.EndsWith("/"c) Then
                            DestinationPath = CurrentPath & FolderName
                        Else
                            DestinationPath = CurrentPath & "/" & FolderName
                        End If

                        Await UploadFolderAsync(SelectedItem, DestinationPath)
                    Next
                Else
                    Dim FolderName As String = Path.GetFileName(FBD.SelectedPath)

                    If CurrentPath.EndsWith("/"c) Then
                        DestinationPath = CurrentPath & FolderName
                    Else
                        DestinationPath = CurrentPath & "/" & FolderName
                    End If

                    Await UploadFolderAsync(FBD.SelectedPath, DestinationPath)
                End If

                Await ListDirectoryContent(CurrentPath & "/")

                LockUI()
            End If
        End If
    End Sub

    Private Async Sub NewDirectoryMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles NewDirectoryMenuItem.Click
        If IsConnected Then
            Dim SelectedFTPLVItem As FTPListViewItem = CType(FTPItemsListView.SelectedItem, FTPListViewItem)

            If Not SelectedFTPLVItem.FileOrDirName = ".." Then
                Dim NewInputDialog As New InputDialog() With {.Title = "FTP Browser"}
                NewInputDialog.InputDialogTitleTextBlock.Text = "New folder name:"

                If NewInputDialog.ShowDialog() = True Then
                    Dim NewFolderName As String = NewInputDialog.NewValueTextBox.Text

                    If CurrentPath = "/" Then
                        Await CreateDirectory(CurrentPath & NewFolderName)
                    Else
                        Await CreateDirectory(CurrentPath & "/" & NewFolderName)
                    End If

                    Await ListDirectoryContent(CurrentPath & "/")
                End If
            End If

        End If
    End Sub

    Private Async Sub DeleteMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles DeleteMenuItem.Click
        If FTPItemsListView.SelectedItem IsNot Nothing Then
            Dim SelectedFTPLVItem As FTPListViewItem = CType(FTPItemsListView.SelectedItem, FTPListViewItem)

            If SelectedFTPLVItem.FileOrDirType = "Folder" Then
                If CurrentPath = "/" Then
                    Await DeleteContent(CurrentPath & SelectedFTPLVItem.FileOrDirName, False)
                Else
                    Await DeleteContent(CurrentPath & "/" & SelectedFTPLVItem.FileOrDirName, False)
                End If

                Await ListDirectoryContent(CurrentPath & "/")
            Else
                If CurrentPath = "/" Then
                    Await DeleteContent(CurrentPath & SelectedFTPLVItem.FileOrDirName, True)
                Else
                    Await DeleteContent(CurrentPath & "/" & SelectedFTPLVItem.FileOrDirName, True)
                End If

                Await ListDirectoryContent(CurrentPath & "/")
            End If

        End If
    End Sub

#End Region

#Region "General FTP Functions"

    Private Async Function ListDirectoryContent(DirectoryPath As String) As Task(Of Boolean)
        'Clear list
        FTPItemsListView.Items.Clear()

        Try
            Using NewFtpClient As New AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", CInt(ConsoleFTPPort), NewFtpConfig)
                'Connect
                Await NewFtpClient.Connect()
                Dim DirectoryListing As FtpListItem() = Await NewFtpClient.GetListing(DirectoryPath)

                'Add a return option
                Dim ReturnFTPLVItem As New FTPListViewItem() With {.FileOrDirName = "..", .FileOrDirType = "Folder"}
                FTPItemsListView.Items.Add(ReturnFTPLVItem)

                'List directory
                For Each FTPItem In DirectoryListing
                    Dim NewFTPLVItem As New FTPListViewItem()

                    Select Case FTPItem.Type
                        Case FtpObjectType.Directory
                            NewFTPLVItem.FileOrDirType = "Folder"
                        Case FtpObjectType.File
                            NewFTPLVItem.FileOrDirType = "File"
                        Case FtpObjectType.Link
                            NewFTPLVItem.FileOrDirType = "Link"
                    End Select

                    NewFTPLVItem.FileOrDirName = FTPItem.Name
                    NewFTPLVItem.FileOrDirSize = FormatNumber(FTPItem.Size / 1048576, 2) & " MB"
                    NewFTPLVItem.FileOrDirPermissions = FTPItem.RawPermissions
                    NewFTPLVItem.FileOrDirOwner = FTPItem.RawOwner

                    If Not NewFTPLVItem.FileOrDirName = "." Then
                        FTPItemsListView.Items.Add(NewFTPLVItem)
                    End If
                Next

                'Disonnect
                Await NewFtpClient.Disconnect()
                Return True
            End Using
        Catch ex As Exception
            MsgBox("Could not list the remote content." & vbCrLf & ex.Message, MsgBoxStyle.Critical)
            Return False
        End Try
    End Function

    Private Async Function RenameContent(RemotePath As String, RenameTo As String, IsFile As Boolean) As Task(Of Boolean)
        Try
            Using NewFtpClient As New AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", CInt(ConsoleFTPPort), NewFtpConfig)
                'Connect
                Await NewFtpClient.Connect()

                If IsFile = True Then
                    'Rename the file
                    Await NewFtpClient.MoveFile(RemotePath, RenameTo, FtpRemoteExists.NoCheck)
                Else
                    'Rename the directory
                    Await NewFtpClient.MoveDirectory(RemotePath, RenameTo, FtpRemoteExists.NoCheck)
                End If

                'Disconnect
                Await NewFtpClient.Disconnect()
                Return True
            End Using
        Catch ex As Exception
            MsgBox("Could not rename the selected file or folder." & vbCrLf & ex.Message, MsgBoxStyle.Critical)
            Return False
        End Try
    End Function

    Private Async Function CreateDirectory(DirectoryName As String) As Task(Of Boolean)
        Try
            Using NewFtpClient As New AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", CInt(ConsoleFTPPort), NewFtpConfig)
                'Connect
                Await NewFtpClient.Connect()

                'Create the directory
                If Await NewFtpClient.CreateDirectory(DirectoryName, True) = True Then
                    FTPStatusTextBlock.Text = DirectoryName & " created."
                Else
                    FTPStatusTextBlock.Text = "Could not create the directory " & DirectoryName
                End If

                'Disconnect
                Await NewFtpClient.Disconnect()
                Return True
            End Using
        Catch ex As Exception
            MsgBox("Could not create the directory " & DirectoryName & vbCrLf & ex.Message, MsgBoxStyle.Critical)
            Return False
        End Try
    End Function

    Private Async Function DeleteContent(FileOrDirectoryName As String, IsFile As Boolean) As Task(Of Boolean)
        Try
            Using NewFtpClient As New AsyncFtpClient(ConsoleIP, "anonymous", "anonymous", CInt(ConsoleFTPPort), NewFtpConfig)

                'Connect
                Await NewFtpClient.Connect()

                If IsFile = True Then
                    'Delete the file
                    Await NewFtpClient.DeleteFile(FileOrDirectoryName)
                Else
                    'Delete the directory
                    Await NewFtpClient.DeleteDirectory(FileOrDirectoryName)
                End If

                'Disconnect
                Await NewFtpClient.Disconnect()
                Return True
            End Using
        Catch ex As Exception
            MsgBox("Could not delete the selected file or folder." & vbCrLf & ex.Message, MsgBoxStyle.Critical)
            Return False
        End Try
    End Function

#End Region

#Region "Drag & Drop"

    Private Sub FTPItemsListView_DragEnter(sender As Object, e As DragEventArgs) Handles FTPItemsListView.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effects = DragDropEffects.Copy
        Else
            e.Effects = DragDropEffects.None
        End If
    End Sub

    Private Async Sub FTPItemsListView_Drop(sender As Object, e As DragEventArgs) Handles FTPItemsListView.Drop
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim DroppedFilesOrFolders() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())

            LockUI()

            If DroppedFilesOrFolders.Length > 1 Then
                'Mulitple files/folders
                For Each DroppedFileOrFolder As String In DroppedFilesOrFolders
                    Dim DroppedAttr As FileAttributes = File.GetAttributes(DroppedFileOrFolder)
                    If (DroppedAttr And FileAttributes.Directory) = FileAttributes.Directory Then
                        Dim FolderName As String = Path.GetFileName(DroppedFileOrFolder)
                        Dim DestinationPath As String

                        If CurrentPath.EndsWith("/"c) Then
                            DestinationPath = CurrentPath & FolderName
                        Else
                            DestinationPath = CurrentPath & "/" & FolderName
                        End If

                        Await UploadFolderAsync(DroppedFileOrFolder, DestinationPath)
                    Else
                        Dim FileName As String = Path.GetFileName(DroppedFileOrFolder)
                        Dim DestinationPath As String

                        If CurrentPath.EndsWith("/"c) Then
                            DestinationPath = CurrentPath & FileName
                        Else
                            DestinationPath = CurrentPath & "/" & FileName
                        End If

                        Await UploadFileAsync(DroppedFileOrFolder, DestinationPath)
                    End If
                Next
            Else
                'Single file/folder
                Dim DroppedAttr As FileAttributes = File.GetAttributes(DroppedFilesOrFolders(0))
                If (DroppedAttr And FileAttributes.Directory) = FileAttributes.Directory Then
                    Dim FolderName As String = Path.GetFileName(DroppedFilesOrFolders(0))
                    Dim DestinationPath As String

                    If CurrentPath.EndsWith("/"c) Then
                        DestinationPath = CurrentPath & FolderName
                    Else
                        DestinationPath = CurrentPath & "/" & FolderName
                    End If

                    Await UploadFolderAsync(DroppedFilesOrFolders(0), DestinationPath)
                Else
                    Dim FileName As String = Path.GetFileName(DroppedFilesOrFolders(0))
                    Dim DestinationPath As String

                    If CurrentPath.EndsWith("/"c) Then
                        DestinationPath = CurrentPath & FileName
                    Else
                        DestinationPath = CurrentPath & "/" & FileName
                    End If

                    Await UploadFileAsync(DroppedFilesOrFolders(0), DestinationPath)
                End If
            End If

            Await ListDirectoryContent(CurrentPath & "/")

            LockUI()

        End If
    End Sub

#End Region

    Private Sub LockUI()
        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub()
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
                                   End Sub)
        Else
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
        End If
    End Sub

    Private Async Sub FTPItemsListView_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles FTPItemsListView.MouseDoubleClick
        If FTPItemsListView.SelectedItem IsNot Nothing Then
            Dim SelectedFTPLVItem As FTPListViewItem = CType(FTPItemsListView.SelectedItem, FTPListViewItem)

            If SelectedFTPLVItem.FileOrDirType = "Folder" Then
                If SelectedFTPLVItem.FileOrDirName = ".." Then
                    If Not CurrentPath = "/" Then
                        CurrentPath = CurrentPath.Remove(CurrentPath.LastIndexOf("/"c))
                        Await ListDirectoryContent(CurrentPath)
                    End If
                Else
                    If CurrentPath = "/" Then
                        CurrentPath = "/" & SelectedFTPLVItem.FileOrDirName
                    Else
                        CurrentPath = CurrentPath & "/" & SelectedFTPLVItem.FileOrDirName
                    End If
                    Await ListDirectoryContent(CurrentPath)
                End If
            Else
                If MsgBox($"Download {SelectedFTPLVItem.FileOrDirName} ?", MsgBoxStyle.YesNo, "Download Request") = MsgBoxResult.Yes Then
                    Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Select a folder where the file/folder should be downloaded.", .ShowNewFolderButton = True}
                    If FBD.ShowDialog() = Forms.DialogResult.OK Then
                        Dim DestinationPath As String = FBD.SelectedPath & "\" & SelectedFTPLVItem.FileOrDirName
                        If CurrentPath = "/" Then
                            Await DownloadFileAsync(DestinationPath, CurrentPath & SelectedFTPLVItem.FileOrDirName)
                        Else
                            Await DownloadFileAsync(DestinationPath, CurrentPath & "/" & SelectedFTPLVItem.FileOrDirName)
                        End If
                    End If
                End If
            End If

            CurrentDirTextBlock.Text = "Current directory : " & CurrentPath
        End If
    End Sub

End Class
