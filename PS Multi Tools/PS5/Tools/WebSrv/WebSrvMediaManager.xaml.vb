Imports System.Windows.Forms
Imports WinSCP

Public Class WebSrvMediaManager

    Public MediaPath As String
    Public PS5IP As String
    Public PS5PORT As Integer

    Private Structure MediaListViewItem
        Private _RemoteFilePath As String

        Public Property RemoteFilePath As String
            Get
                Return _RemoteFilePath
            End Get
            Set
                _RemoteFilePath = Value
            End Set
        End Property
    End Structure

    Private Sub WebSrvMediaManager_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        If Not String.IsNullOrEmpty(MediaPath) Then
            ListMediaFiles()
        End If
    End Sub

    Private Sub ListMediaFiles()
        InstalledMediaListView.Items.Clear()

        Dim NewSessionOptions As New SessionOptions
        With NewSessionOptions
            .Protocol = Protocol.Ftp
            .HostName = PS5IP
            .UserName = "anonymous"
            .Password = "anonymous"
            .PortNumber = PS5PORT
        End With
        Dim NewSession As New Session()

        Try
            'Connect
            NewSession.Open(NewSessionOptions)

            'Enumerate files in the media directory
            For Each DirectoryInFTP As RemoteFileInfo In NewSession.EnumerateRemoteFiles(MediaPath, "", EnumerationOptions.AllDirectories)
                Dim NewMediaLVItem As New MediaListViewItem() With {.RemoteFilePath = DirectoryInFTP.FullName}
                InstalledMediaListView.Items.Add(NewMediaLVItem)
            Next

            'Disconnect
            NewSession.Close()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

        InstalledMediaListView.Items.Refresh()
    End Sub

    Private Sub AddContentButton_Click(sender As Object, e As RoutedEventArgs) Handles AddContentButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select your media files", .Multiselect = True}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then

            Try
                Dim NewSessionOptions As New SessionOptions
                With NewSessionOptions
                    .Protocol = Protocol.Ftp
                    .HostName = PS5IP
                    .UserName = "anonymous"
                    .Password = "anonymous"
                    .PortNumber = PS5PORT
                End With
                Dim NewSession As New Session()

                'Connect
                NewSession.Open(NewSessionOptions)

                MsgBox("Upload is starting. There's currently no progressbar in case large files have been selected.", MsgBoxStyle.Information)

                'Upload the selected file(s)
                If OFD.FileNames.Length > 1 Then
                    For Each SelectedMediaFile In OFD.FileNames
                        NewSession.PutFileToDirectory(SelectedMediaFile, MediaPath, False)
                    Next
                    MsgBox("Files uploaded with success!", MsgBoxStyle.Information)
                Else
                    NewSession.PutFileToDirectory(OFD.FileName, MediaPath, False)
                    MsgBox("File uploaded with success!", MsgBoxStyle.Information)
                End If

                'Close the connection
                NewSession.Close()
            Catch ex As Exception
                MsgBox(ex.ToString)
            End Try

            'Refresh
            ListMediaFiles()
        End If
    End Sub

    Private Sub RemoveContentButton_Click(sender As Object, e As RoutedEventArgs) Handles RemoveContentButton.Click
        If InstalledMediaListView.SelectedItem IsNot Nothing Then
            Dim SelectedMediaFile As MediaListViewItem = CType(InstalledMediaListView.SelectedItem, MediaListViewItem)

            If MsgBox("Do you really want to delete the selected media file from the WebSrv ?", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then

                Try
                    Dim NewSessionOptions As New SessionOptions
                    With NewSessionOptions
                        .Protocol = Protocol.Ftp
                        .HostName = PS5IP
                        .UserName = "anonymous"
                        .Password = "anonymous"
                        .PortNumber = PS5PORT
                    End With
                    Dim NewSession As New Session()

                    'Connect
                    NewSession.Open(NewSessionOptions)

                    'Remove
                    NewSession.RemoveFiles(SelectedMediaFile.RemoteFilePath)

                    'Close the connection
                    NewSession.Close()
                Catch ex As Exception
                    MsgBox(ex.ToString)
                End Try

                MsgBox("File removed from the WebSrv.", MsgBoxStyle.Information)

                'Refresh
                ListMediaFiles()
            End If

        End If
    End Sub

End Class
