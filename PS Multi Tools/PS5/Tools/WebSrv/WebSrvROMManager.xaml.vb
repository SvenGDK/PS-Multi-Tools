Imports System.Windows.Forms
Imports WinSCP

Public Class WebSrvROMManager

    Public PS5IP As String
    Public PS5PORT As Integer
    Public ROMPath As String

    Private Structure ROMListViewItem
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

    Private Sub WebSrvROMManager_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        If Not String.IsNullOrEmpty(ROMPath) Then
            ListROMs()
        End If
    End Sub

    Private Sub ListROMs()
        InstalledROMsListView.Items.Clear()

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

            'Enumerate files in the roms directory
            For Each DirectoryInFTP As RemoteFileInfo In NewSession.EnumerateRemoteFiles(ROMPath, "", EnumerationOptions.AllDirectories)
                If Not DirectoryInFTP.FullName.EndsWith(".jpg") AndAlso Not DirectoryInFTP.FullName.EndsWith(".png") Then
                    Dim NewROMLVItem As New ROMListViewItem() With {.RemoteFilePath = DirectoryInFTP.FullName}
                    InstalledROMsListView.Items.Add(NewROMLVItem)
                End If
            Next

            NewSession.Close()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

        InstalledROMsListView.Items.Refresh()
    End Sub

    Private Sub InstalledROMsListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles InstalledROMsListView.SelectionChanged
        If InstalledROMsListView.SelectedItem IsNot Nothing Then
            Dim SelectedROMLVItem As ROMListViewItem = CType(InstalledROMsListView.SelectedItem, ROMListViewItem)
            GetROMInfo(SelectedROMLVItem.RemoteFilePath)
        End If
    End Sub

    Private Sub GetROMInfo(SelectedROMFile As String)
        Dim LastPos As Integer = SelectedROMFile.LastIndexOf("/") + 1
        Dim FileName As String = SelectedROMFile.Substring(LastPos, SelectedROMFile.Length - LastPos)

        Dim JPGImageFileName As String = FileName.Split("."c)(0) + ".jpg"
        Dim PNGImageFileName As String = FileName.Split("."c)(0) + ".png"
        Dim JPGImageFilePath As String = SelectedROMFile.Replace(FileName, JPGImageFileName)
        Dim PNGImageFilePath As String = SelectedROMFile.Replace(FileName, PNGImageFileName)

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

            ROMFileNameTextBox.Text = FileName

            If NewSession.FileExists(JPGImageFilePath) Then
                Try
                    'Set homebrew icon
                    Dim TempBitmapImage = New BitmapImage()
                    TempBitmapImage.BeginInit()
                    TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                    TempBitmapImage.UriSource = New Uri("ftp://" + PS5IP + ":" + PS5PORT.ToString() + JPGImageFilePath, UriKind.RelativeOrAbsolute)
                    TempBitmapImage.EndInit()
                    ROMImage.Source = TempBitmapImage
                Catch ex As Exception
                End Try
            ElseIf NewSession.FileExists(PNGImageFilePath) Then
                Try
                    'Set homebrew icon
                    Dim TempBitmapImage = New BitmapImage()
                    TempBitmapImage.BeginInit()
                    TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                    TempBitmapImage.UriSource = New Uri("ftp://" + PS5IP + ":" + PS5PORT.ToString() + PNGImageFilePath, UriKind.RelativeOrAbsolute)
                    TempBitmapImage.EndInit()
                    ROMImage.Source = TempBitmapImage
                Catch ex As Exception
                End Try
            Else
                ROMImage.Source = New BitmapImage(New Uri("/Images/nothing.png", UriKind.RelativeOrAbsolute))
            End If

            NewSession.Close()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub AddROMsButton_Click(sender As Object, e As RoutedEventArgs) Handles AddROMsButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select your ROM files", .Multiselect = True}
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

                'Upload the selected file(s)
                If OFD.FileNames.Length > 1 Then
                    For Each SelectedROMFile In OFD.FileNames
                        NewSession.PutFileToDirectory(SelectedROMFile, ROMPath, False)
                    Next
                    MsgBox("Files uploaded with success!", MsgBoxStyle.Information)
                Else
                    NewSession.PutFileToDirectory(OFD.FileName, ROMPath, False)
                    MsgBox("File uploaded with success!", MsgBoxStyle.Information)
                End If

                'Close the connection
                NewSession.Close()
            Catch ex As Exception
                MsgBox(ex.ToString)
            End Try

            'Refresh
            ListROMs()
        End If
    End Sub

    Private Sub RemoveROMButton_Click(sender As Object, e As RoutedEventArgs) Handles RemoveROMButton.Click
        If InstalledROMsListView.SelectedItem IsNot Nothing Then
            Dim SelectedROMFile As ROMListViewItem = CType(InstalledROMsListView.SelectedItem, ROMListViewItem)

            If MsgBox("Do you really want to delete the selected ROM from the WebSrv ?", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then

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
                    NewSession.RemoveFiles(SelectedROMFile.RemoteFilePath)

                    'Close the connection
                    NewSession.Close()
                Catch ex As Exception
                    MsgBox(ex.ToString)
                End Try

                MsgBox("ROM removed from the WebSrv.", MsgBoxStyle.Information)

                'Refresh
                ListROMs()
            End If

        End If
    End Sub

    Private Sub ROMImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles ROMImage.MouseLeftButtonDown
        If InstalledROMsListView.SelectedItem IsNot Nothing AndAlso Not String.IsNullOrEmpty(ROMFileNameTextBox.Text) Then
            Dim SelectedROMLVItem As ROMListViewItem = CType(InstalledROMsListView.SelectedItem, ROMListViewItem)

            If MsgBox("Do you want to replace the icon for the selected ROM file ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then

                Dim ROMFileNameWithoutExtension As String = IO.Path.GetFileNameWithoutExtension(ROMFileNameTextBox.Text)
                Dim OFD As New OpenFileDialog() With {.Title = "Select a new icon in jpg or png format.", .Filter = "PNG files (*.png)|*.png|JPG files (*.jpg)|*.jpg", .Multiselect = False}
                If OFD.ShowDialog() = Forms.DialogResult.OK Then

                    Dim NewIconFileName As String = OFD.SafeFileName
                    Dim NewIconFileNameExtension As String = IO.Path.GetExtension(OFD.FileName)

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

                        'Put the file in the roms directory
                        NewSession.PutFileToDirectory(OFD.FileName, ROMPath, False)

                        'Check if the file name matches the ROM file name
                        If Not NewSession.FileExists(ROMPath + ROMFileNameWithoutExtension + NewIconFileNameExtension) Then
                            'If not then rename the file to the ROM's file name
                            NewSession.MoveFile(ROMPath + NewIconFileName, ROMPath + ROMFileNameWithoutExtension + NewIconFileNameExtension)
                        End If

                        'Close the connection
                        NewSession.Close()

                        MsgBox("Icon replaced!", MsgBoxStyle.Information)
                    Catch ex As Exception
                        MsgBox(ex.ToString)
                    End Try

                    'Refresh icon for selected ROM
                    GetROMInfo(SelectedROMLVItem.RemoteFilePath)
                End If

            End If

        End If
    End Sub

End Class
