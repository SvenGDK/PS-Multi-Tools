Imports System.IO
Imports System.Text
Imports System.Windows.Forms
Imports FluentFTP
Imports FluentFTP.Exceptions
Imports WinSCP

Public Class WebSrvHomebrewManager

    Dim SelectedHomebrewItem As HomebrewListViewItem = Nothing
    Dim PS5Port As Integer = 0

    Private Structure HomebrewListViewItem
        Private _HomebrewPath As String

        Public Property HomebrewPath As String
            Get
                Return _HomebrewPath
            End Get
            Set
                _HomebrewPath = Value
            End Set
        End Property
    End Structure

    Private Sub ConnectButton_Click(sender As Object, e As RoutedEventArgs) Handles ConnectButton.Click
        If Not String.IsNullOrEmpty(PS5IPTextBox.Text) Then

            Dim NewFTPConfig As New FtpConfig() With {.ConnectTimeout = 3000, .RetryAttempts = 1}

            If ConnectButton.Content.ToString() = "Connect" Then
                Try
                    Using conn As New FtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", 1337, NewFTPConfig)
                        'Check connection on port 1337
                        conn.Connect(False)
                        PS5Port = 1337
                        conn.Disconnect()
                    End Using

                    ListHomebrew()
                    ConnectButton.Content = "Disconnect"
                Catch ex As TimeoutException
                    Try
                        Using conn As New FtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", 2121, NewFTPConfig)
                            'Check connection on port 2121
                            conn.Connect(False)
                            PS5Port = 2121
                            conn.Disconnect()
                        End Using

                        ListHomebrew()
                        ConnectButton.Content = "Disconnect"
                    Catch ex2 As TimeoutException
                        MsgBox("Could not connect to the PS5's FTP server. Please verify your IP and if the FTP server is running on the PS5.", MsgBoxStyle.Critical, "Error connecting to the PS5")
                    End Try
                End Try
            Else
                'Disconnect
                Try
                    Using conn As New FtpClient(PS5IPTextBox.Text, "anonymous", "anonymous", PS5Port)
                        If conn.IsConnected Then
                            conn.Disconnect()
                        End If
                    End Using
                    ConnectButton.Content = "Connect"
                Catch ex As FtpException
                    MsgBox("Could not disconnect from the FTP server.", MsgBoxStyle.Critical, "Error disconnecting from the PS5")
                End Try
            End If
        End If
    End Sub

    Private Sub ListHomebrew()

        InstalledHomebrewListView.Items.Clear()

        Dim NewSessionOptions As New SessionOptions
        With NewSessionOptions
            .Protocol = Protocol.Ftp
            .HostName = PS5IPTextBox.Text
            .UserName = "anonymous"
            .Password = "anonymous"
            .PortNumber = PS5Port
        End With
        Dim NewSession As New Session()

        Try
            'Connect
            NewSession.Open(NewSessionOptions)

            'Enumerate directories
            For Each DirectoryInFTP As RemoteFileInfo In NewSession.EnumerateRemoteFiles("/data/homebrew/", "", EnumerationOptions.MatchDirectories)
                If NewSession.FileExists(DirectoryInFTP.FullName + "/homebrew.js") Then
                    Dim NewHomebrewListViewItem As New HomebrewListViewItem() With {.HomebrewPath = DirectoryInFTP.FullName}
                    InstalledHomebrewListView.Items.Add(NewHomebrewListViewItem)
                End If
            Next

            NewSession.Close()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

        InstalledHomebrewListView.Items.Refresh()
    End Sub

    Private Sub GetHomebrewInfo(HomebrewFullPath As String)

        Dim NewSessionOptions As New SessionOptions
        With NewSessionOptions
            .Protocol = Protocol.Ftp
            .HostName = PS5IPTextBox.Text
            .UserName = "anonymous"
            .Password = "anonymous"
            .PortNumber = PS5Port
        End With
        Dim NewSession As New Session()

        'Connect
        NewSession.Open(NewSessionOptions)

        If NewSession.FileExists(HomebrewFullPath + "/homebrew.js") Then

            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js") Then
                File.Delete(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js")
            End If

            'Get the homebrew.js file
            NewSession.GetFileToDirectory(HomebrewFullPath + "/homebrew.js", My.Computer.FileSystem.CurrentDirectory + "\Cache", False, Nothing)

            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js") Then

                Dim PayloadFilePath As String = ""
                Dim PayloadArgs As String = ""
                Dim PayloadEnvVars As String = ""
                Dim PayloadROMDir As String = ""
                Dim PayloadMediaDir As String = ""
                Dim PayloadName As String = ""
                Dim PayloadDescription As String = ""

                'Get homebrew info
                For Each Line As String In File.ReadAllLines(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js")
                    If Line.Contains("const PAYLOAD") Then
                        PayloadFilePath = Line.Split("+"c)(1).Replace("'"c, "").Replace(";"c, "").Trim()
                    End If
                    If Line.Contains("const ARGS") Then
                        PayloadArgs = Line.Split("="c)(1).Replace("["c, "").Replace("]"c, "").Replace(";"c, "").Trim()
                    End If
                    If Line.Contains("const ENVVARS") Then
                        PayloadEnvVars = Line.Split("="c)(1).Replace(";"c, "").Trim()
                    End If
                    If Line.Contains("const ROMDIR") Then
                        PayloadROMDir = Line.Split("+"c)(1).Replace("'"c, "").Replace(";"c, "").Trim()
                    End If
                    If Line.Contains("const MEDIADIR") Then
                        PayloadMediaDir = Line.Split("+"c)(1).Replace("'"c, "").Replace(";"c, "").Trim()
                    End If

                    If Line.Contains("mainText:") Then
                        PayloadName = Line.Split(":"c)(1).Replace(""""c, "").Replace(","c, "").Trim()
                    End If
                    If Line.Contains("secondaryText:") Then
                        PayloadDescription = Line.Split(":"c)(1).Replace("'"c, "").Replace(","c, "").Trim()
                    End If
                Next

                'Show info in TextBoxes
                HomebrewPayloadPathTextBox.Text = PayloadFilePath
                HomebrewPayloadArgumentsTextBox.Text = PayloadArgs
                HomebrewPayloadEnvironmentVariablesTextBox.Text = PayloadEnvVars
                HomebrewPayloadROMDirectoryTextBox.Text = PayloadROMDir
                HomebrewPayloadMediaDirectoryTextBox.Text = PayloadMediaDir
                HomebrewPayloadNameTextBox.Text = PayloadName
                HomebrewPayloadDescriptionTextBox.Text = PayloadDescription

                'Show or hide buttons
                If Not String.IsNullOrEmpty(PayloadROMDir) Then
                    ManageROMsButton.Visibility = Windows.Visibility.Visible
                Else
                    ManageROMsButton.Visibility = Windows.Visibility.Hidden
                End If
                If Not String.IsNullOrEmpty(PayloadMediaDir) Then
                    ManageMediaButton.Visibility = Windows.Visibility.Visible
                Else
                    ManageMediaButton.Visibility = Windows.Visibility.Hidden
                End If

            End If

        End If

        If NewSession.FileExists(HomebrewFullPath + "/sce_sys/icon0.png") Then
            Try
                'Set homebrew icon
                Dim TempBitmapImage = New BitmapImage()
                TempBitmapImage.BeginInit()
                TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                TempBitmapImage.UriSource = New Uri("ftp://" + PS5IPTextBox.Text + ":" + PS5Port.ToString() + HomebrewFullPath + "/sce_sys/icon0.png", UriKind.RelativeOrAbsolute)
                TempBitmapImage.EndInit()
                HomebrewImage.Source = TempBitmapImage
            Catch ex As Exception
            End Try
        Else
            HomebrewImage.Source = New BitmapImage(New Uri("/Images/nothing.png", UriKind.RelativeOrAbsolute))
        End If

        'Close the connection
        NewSession.Close()
    End Sub

    Private Sub InstalledHomebrewListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles InstalledHomebrewListView.SelectionChanged
        If InstalledHomebrewListView.SelectedItem IsNot Nothing AndAlso Not String.IsNullOrEmpty(PS5IPTextBox.Text) Then
            SelectedHomebrewItem = CType(InstalledHomebrewListView.SelectedItem, HomebrewListViewItem)
            GetHomebrewInfo(SelectedHomebrewItem.HomebrewPath)
        End If
    End Sub

    Private Sub SaveChangesButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveChangesButton.Click

        Dim NewJSLines As New List(Of String)()

        'Payload File Path
        If Not String.IsNullOrEmpty(HomebrewPayloadPathTextBox.Text) Then
            For Each Line As String In File.ReadAllLines(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js")
                If Line.Contains("const PAYLOAD") Then
                    NewJSLines.Add("const PAYLOAD = window.workingDir + '" + HomebrewPayloadPathTextBox.Text + "';")
                Else
                    NewJSLines.Add(Line)
                End If
            Next
            'Save
            File.WriteAllLines(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js", NewJSLines.ToArray(), Encoding.UTF8)
            NewJSLines = New List(Of String)()
        End If

        'Payload Title
        If Not String.IsNullOrEmpty(HomebrewPayloadNameTextBox.Text) Then
            For Each Line As String In File.ReadAllLines(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js")
                If Line.Contains("mainText:") Then
                    NewJSLines.Add("mainText: " + """" + HomebrewPayloadNameTextBox.Text + """,")
                Else
                    NewJSLines.Add(Line)
                End If
            Next
            'Save
            File.WriteAllLines(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js", NewJSLines.ToArray(), Encoding.UTF8)
            NewJSLines = New List(Of String)()
        End If

        'Payload Description
        If Not String.IsNullOrEmpty(HomebrewPayloadDescriptionTextBox.Text) Then
            For Each Line As String In File.ReadAllLines(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js")
                If Line.Contains("secondaryText:") Then
                    NewJSLines.Add("secondaryText: " + "'" + HomebrewPayloadDescriptionTextBox.Text + "',")
                Else
                    NewJSLines.Add(Line)
                End If
            Next
            'Save
            File.WriteAllLines(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js", NewJSLines.ToArray(), Encoding.UTF8)
            NewJSLines = New List(Of String)()
        End If

        'Payload Arguments
        If Not String.IsNullOrEmpty(HomebrewPayloadArgumentsTextBox.Text) Then
            For Each Line As String In File.ReadAllLines(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js")
                If Line.Contains("const ARGS") Then
                    NewJSLines.Add("const ARGS = [" + HomebrewPayloadArgumentsTextBox.Text + "]") 'Missing ";" ?
                Else
                    NewJSLines.Add(Line)
                End If
            Next
            'Save
            File.WriteAllLines(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js", NewJSLines.ToArray(), Encoding.UTF8)
            NewJSLines = New List(Of String)()
        End If

        'Payload Enviroment Variables
        If Not String.IsNullOrEmpty(HomebrewPayloadEnvironmentVariablesTextBox.Text) Then
            For Each Line As String In File.ReadAllLines(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js")
                If Line.Contains("const ENVVARS") Then
                    NewJSLines.Add("const ENVVARS = " + HomebrewPayloadEnvironmentVariablesTextBox.Text + ";")
                Else
                    NewJSLines.Add(Line)
                End If
            Next
            'Save
            File.WriteAllLines(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js", NewJSLines.ToArray(), Encoding.UTF8)
            NewJSLines = New List(Of String)()
        End If

        'Payload ROMDIR
        If Not String.IsNullOrEmpty(HomebrewPayloadROMDirectoryTextBox.Text) Then
            For Each Line As String In File.ReadAllLines(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js")
                If Line.Contains("const ROMDIR") Then
                    NewJSLines.Add("const ROMDIR = window.workingDir + '" + HomebrewPayloadROMDirectoryTextBox.Text + "';")
                Else
                    NewJSLines.Add(Line)
                End If
            Next
            'Save
            File.WriteAllLines(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js", NewJSLines.ToArray(), Encoding.UTF8)
            NewJSLines = New List(Of String)()
        End If

        'Payload MEDIADIR
        If Not String.IsNullOrEmpty(HomebrewPayloadMediaDirectoryTextBox.Text) Then
            For Each Line As String In File.ReadAllLines(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js")
                If Line.Contains("const MEDIADIR") Then
                    NewJSLines.Add("const MEDIADIR = window.workingDir + '" + HomebrewPayloadMediaDirectoryTextBox.Text + "';")
                Else
                    NewJSLines.Add(Line)
                End If
            Next
            'Save
            File.WriteAllLines(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js", NewJSLines.ToArray(), Encoding.UTF8)
            NewJSLines = New List(Of String)()
        End If

        'Upload changes
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js") AndAlso Not String.IsNullOrEmpty(SelectedHomebrewItem.HomebrewPath) Then

            Try
                Dim NewSessionOptions As New SessionOptions
                With NewSessionOptions
                    .Protocol = Protocol.Ftp
                    .HostName = PS5IPTextBox.Text
                    .UserName = "anonymous"
                    .Password = "anonymous"
                    .PortNumber = PS5Port
                End With
                Dim NewSession As New Session()

                'Connect & upload changes
                NewSession.Open(NewSessionOptions)
                NewSession.PutFileToDirectory(My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js", SelectedHomebrewItem.HomebrewPath, False, Nothing)

                'Close session when done
                NewSession.Close()
            Catch ex As Exception
                MsgBox("Could not upload changes to the PS5.", MsgBoxStyle.Critical)
                MsgBox(ex.Message)
            End Try

        Else
            MsgBox("Could not find " + My.Computer.FileSystem.CurrentDirectory + "\Cache\homebrew.js", MsgBoxStyle.Critical, "Error uploading changes")
        End If

        MsgBox("Homebrew information updated & saved!", MsgBoxStyle.Information)

    End Sub

    Private Sub ManageROMsButton_Click(sender As Object, e As RoutedEventArgs) Handles ManageROMsButton.Click
        If InstalledHomebrewListView.SelectedItem IsNot Nothing AndAlso Not String.IsNullOrEmpty(HomebrewPayloadROMDirectoryTextBox.Text) AndAlso Not String.IsNullOrEmpty(PS5IPTextBox.Text) Then
            SelectedHomebrewItem = CType(InstalledHomebrewListView.SelectedItem, HomebrewListViewItem)
            Dim ROMFullPath As String = SelectedHomebrewItem.HomebrewPath + HomebrewPayloadROMDirectoryTextBox.Text
            Dim NewROMManager As New WebSrvROMManager() With {.ShowActivated = True, .Title = "PS5 WebSrv Game ROM Manager - " + ROMFullPath, .ROMPath = ROMFullPath, .PS5IP = PS5IPTextBox.Text, .PS5PORT = PS5Port}
            NewROMManager.Show()
        End If
    End Sub

    Private Sub ManageMediaButton_Click(sender As Object, e As RoutedEventArgs) Handles ManageMediaButton.Click
        If InstalledHomebrewListView.SelectedItem IsNot Nothing AndAlso Not String.IsNullOrEmpty(HomebrewPayloadMediaDirectoryTextBox.Text) Then
            SelectedHomebrewItem = CType(InstalledHomebrewListView.SelectedItem, HomebrewListViewItem)
            Dim MediaFullPath As String = SelectedHomebrewItem.HomebrewPath + HomebrewPayloadMediaDirectoryTextBox.Text
            Dim NewROMManager As New WebSrvMediaManager() With {.ShowActivated = True, .Title = "PS5 WebSrv Media Content Manager - " + MediaFullPath, .MediaPath = MediaFullPath, .PS5IP = PS5IPTextBox.Text, .PS5PORT = PS5Port}
            NewROMManager.Show()
        End If
    End Sub

    Private Sub AddHomebrewButton_Click(sender As Object, e As RoutedEventArgs) Handles AddHomebrewButton.Click
        If Not String.IsNullOrEmpty(PS5IPTextBox.Text) Then

            Dim FBD As New FolderBrowserDialog() With {.Description = "Select a compatible homebrew folder", .ShowNewFolderButton = False}
            If FBD.ShowDialog() = Forms.DialogResult.OK Then

                If File.Exists(FBD.SelectedPath + "\homebrew.js") Then

                    Dim DirInfo As New DirectoryInfo(FBD.SelectedPath)

                    Try
                        Dim NewSessionOptions As New SessionOptions
                        With NewSessionOptions
                            .Protocol = Protocol.Ftp
                            .HostName = PS5IPTextBox.Text
                            .UserName = "anonymous"
                            .Password = "anonymous"
                            .PortNumber = PS5Port
                        End With
                        Dim NewSession As New Session()

                        'Connect
                        NewSession.Open(NewSessionOptions)

                        If Not NewSession.FileExists("/data/homebrew/" + DirInfo.Name) Then

                            'Create the directory on the PS5
                            NewSession.CreateDirectory("/data/homebrew/" + DirInfo.Name)
                            'Synchronize the new remote directory with the local content 
                            NewSession.SynchronizeDirectories(SynchronizationMode.Remote, FBD.SelectedPath, "/data/homebrew/" + DirInfo.Name, False)
                            'Close the connection
                            NewSession.Close()

                            MsgBox("Homebrew has been uploaded to the WebSrv.", MsgBoxStyle.Information)
                        Else

                            'Synchronize and update the remote directory
                            NewSession.SynchronizeDirectories(SynchronizationMode.Remote, FBD.SelectedPath, "/data/homebrew/" + DirInfo.Name, False)
                            'Close the connection
                            NewSession.Close()

                            MsgBox("Homebrew updated on the WebSrv.", MsgBoxStyle.Information)
                        End If

                    Catch ex As Exception
                        MsgBox(ex.ToString)
                    End Try

                    'Refresh
                    ListHomebrew()
                Else
                    MsgBox("This folder is missing a homebrew.js file that is required by the WebSrv and cannot be added.", MsgBoxStyle.Exclamation, "Not compatible")
                End If

            End If

        End If
    End Sub

    Private Sub RemoveHomebrewButton_Click(sender As Object, e As RoutedEventArgs) Handles RemoveHomebrewButton.Click
        If InstalledHomebrewListView.SelectedItem IsNot Nothing AndAlso Not String.IsNullOrEmpty(PS5IPTextBox.Text) Then
            SelectedHomebrewItem = CType(InstalledHomebrewListView.SelectedItem, HomebrewListViewItem)

            If MsgBox("Do you really want to delete the selected homebrew from the WebSrv ?", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then

                Try
                    Dim NewSessionOptions As New SessionOptions
                    With NewSessionOptions
                        .Protocol = Protocol.Ftp
                        .HostName = PS5IPTextBox.Text
                        .UserName = "anonymous"
                        .Password = "anonymous"
                        .PortNumber = PS5Port
                    End With
                    Dim NewSession As New Session()

                    'Connect
                    NewSession.Open(NewSessionOptions)

                    'Remove directory with content
                    NewSession.RemoveFiles(SelectedHomebrewItem.HomebrewPath)

                    'Close the connection
                    NewSession.Close()
                Catch ex As Exception
                    MsgBox(ex.ToString)
                End Try

                MsgBox("Homebrew removed from the WebSrv.", MsgBoxStyle.Information)

                'Clear homebrew information
                HomebrewPayloadPathTextBox.Text = ""
                HomebrewPayloadArgumentsTextBox.Text = ""
                HomebrewPayloadEnvironmentVariablesTextBox.Text = ""
                HomebrewPayloadROMDirectoryTextBox.Text = ""
                HomebrewPayloadMediaDirectoryTextBox.Text = ""
                HomebrewPayloadNameTextBox.Text = ""
                HomebrewPayloadDescriptionTextBox.Text = ""
                HomebrewImage.Source = Nothing
                ManageROMsButton.Visibility = Visibility.Hidden
                ManageMediaButton.Visibility = Visibility.Hidden

                'Refresh
                ListHomebrew()
            End If

        End If
    End Sub

    Private Sub HomebrewImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles HomebrewImage.MouseLeftButtonDown
        If InstalledHomebrewListView.SelectedItem IsNot Nothing AndAlso Not String.IsNullOrEmpty(PS5IPTextBox.Text) Then
            If MsgBox("Do you want to replace the icon for the selected homebrew ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then

                Dim OFD As New OpenFileDialog() With {.Title = "Select a new icon0.png file.", .Filter = "PNG files (*.png)|*.png", .Multiselect = False}
                If OFD.ShowDialog() = Forms.DialogResult.OK Then
                    Try
                        Dim NewSessionOptions As New SessionOptions
                        With NewSessionOptions
                            .Protocol = Protocol.Ftp
                            .HostName = PS5IPTextBox.Text
                            .UserName = "anonymous"
                            .Password = "anonymous"
                            .PortNumber = PS5Port
                        End With
                        Dim NewSession As New Session()

                        'Connect
                        NewSession.Open(NewSessionOptions)

                        'Replace the file
                        NewSession.PutFileToDirectory(OFD.FileName, SelectedHomebrewItem.HomebrewPath + "/sce_sys/", False)

                        'Close the connection
                        NewSession.Close()

                        MsgBox("Icon replaced!", MsgBoxStyle.Information)
                    Catch ex As Exception
                        MsgBox(ex.ToString)
                    End Try

                    'Refresh icon
                    GetHomebrewInfo(SelectedHomebrewItem.HomebrewPath)
                End If
            End If
        End If
    End Sub

End Class
