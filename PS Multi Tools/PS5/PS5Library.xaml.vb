Imports FluentFTP
Imports Newtonsoft.Json
Imports System.ComponentModel
Imports System.Security.Authentication
Imports System.Text

Public Class PS5Library

    Dim WithEvents GameLoaderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}
    Dim WithEvents NewLoadingWindow As New SyncWindow() With {.Title = "Loading PS5 files", .ShowActivated = True}

    Public CurrentPath As String = ""
    Dim ConsoleIP As String = ""
    Dim ConsolePort As String = ""

    'Supplemental library menu items
    Dim WithEvents LoadFolderMenuItem As New Controls.MenuItem() With {.Header = "Load installed games over FTP"}
    Dim WithEvents LoadDLFolderMenuItem As New Controls.MenuItem() With {.Header = "Open Downloads folder"}

    Private Sub PS5Library_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Will set the console IP and port when changing the console address in the settings
        AddHandler NewPS5Menu.IPTextChanged, AddressOf PS5MenuIPTextChanged

        'Add supplemental library menu items that will be handled in the app
        Dim LibraryMenuItem As Controls.MenuItem = CType(NewPS5Menu.Items(0), Controls.MenuItem)
        LibraryMenuItem.Items.Add(LoadFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem)
    End Sub

    Private Sub LoadFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadFolderMenuItem.Click
        If Not String.IsNullOrEmpty(ConsoleIP) Then
            'Show the loading progress window
            NewLoadingWindow = New SyncWindow() With {.Title = "Loading PS5 files", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.IsIndeterminate = True
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading files, please wait ..."
            NewLoadingWindow.Show()

            'Load the files
            GameLoaderWorker.RunWorkerAsync()
        Else
            MsgBox("Please enter your console's FTP IP address in the settings before continuing.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub GameLoaderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles GameLoaderWorker.DoWork
        Dim CPort As Integer = Integer.Parse(ConsolePort)

        Using conn As New FtpClient(ConsoleIP, "anonymous", "anonymous", CPort)
            'Configurate the FTP connection
            conn.Config.EncryptionMode = FtpEncryptionMode.None
            conn.Config.SslProtocols = SslProtocols.None
            conn.Config.DataConnectionEncryption = False

            'Connect
            conn.Connect()

            'List appmeta directory for the games
            For Each item In conn.GetListing("/user/appmeta")

                Dim PS5GameLVItem As New PS5Game()

                If item.Type = FtpObjectType.Directory Then

                    If item.Name = "push_resource" Or item.Name = "addcont" Or item.Name = "external" Then

                    Else
                        PS5GameLVItem.GameID = item.Name

                        'Check for icon0.png
                        If conn.GetObjectInfo(item.FullName + "/icon0.png") IsNot Nothing Then
                            Dim Icon0Bytes As Byte() = Nothing
                            If conn.DownloadBytes(Icon0Bytes, item.FullName + "/icon0.png") Then
                                PS5GameLVItem.GameCoverSource = Utils.BitmapSourceFromByteArray(Icon0Bytes)
                            End If
                        End If

                        'Check for pic0.png (currently not used - can take also some more time)
                        'If conn.GetObjectInfo(item.FullName + "/pic0.png") IsNot Nothing Then
                        '    Dim Pic0Bytes As Byte() = Nothing
                        '    If conn.DownloadBytes(Pic0Bytes, item.FullName + "/pic0.png") Then
                        '        PS5GameLVItem.GameCoverSource = Utils.BitmapSourceFromByteArray(Pic0Bytes)
                        '    End If
                        'End If

                        'Check for param.json
                        If conn.GetObjectInfo(item.FullName + "/param.json") IsNot Nothing Then
                            Dim ParamBytes As Byte() = Nothing
                            If conn.DownloadBytes(ParamBytes, item.FullName + "/param.json") Then
                                Dim ParamBytesAsString = Encoding.UTF8.GetString(ParamBytes)
                                Dim GameInfos As Structures.PS5GameParamJson = JsonConvert.DeserializeObject(Of Structures.PS5GameParamJson)(ParamBytesAsString)

                                PS5GameLVItem.GameTitle = GameInfos.LocalizedParameters.EnUS.TitleName
                                PS5GameLVItem.GameContentID = GameInfos.ContentId
                                PS5GameLVItem.GameRegion = PS4Game.GetGameRegion(GameInfos.ContentId)
                            End If
                        End If

                        'Check for game pkg info
                        If conn.GetObjectInfo("/user/app/" + item.Name + "/app.pkg") IsNot Nothing Then
                            Dim PKG As FtpListItem = conn.GetObjectInfo("/user/app/" + item.Name + "/app.pkg")
                            PS5GameLVItem.GameSize = FormatNumber(PKG.Size / 1048576, 2) + " MB"
                        End If

                        'Add to the GamesListView
                        If Dispatcher.CheckAccess() = False Then
                            Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(PS5GameLVItem))
                        Else
                            GamesListView.Items.Add(PS5GameLVItem)
                        End If
                    End If

                End If

            Next

            'List installed system apps 
            For Each item In conn.GetListing("/system_ex/app")

                Dim PS5GameLVItem As New PS5Game()

                If item.Type = FtpObjectType.Directory Then

                    PS5GameLVItem.GameID = item.Name

                    'Check for pic0.png (currently not used - can take also some more time)
                    'If conn.GetObjectInfo(item.FullName + "/pic0.png") IsNot Nothing Then
                    '    Dim Pic0Bytes As Byte() = Nothing
                    '    If conn.DownloadBytes(Pic0Bytes, item.FullName + "/pic0.png") Then
                    '        PS5GameLVItem.GameCoverSource = Utils.BitmapSourceFromByteArray(Pic0Bytes)
                    '    End If
                    'End If

                    'Check for param.json
                    If conn.GetObjectInfo(item.FullName + "/sce_sys/param.json") IsNot Nothing Then
                        Dim ParamBytes As Byte() = Nothing
                        If conn.DownloadBytes(ParamBytes, item.FullName + "/sce_sys/param.json") Then
                            Dim ParamBytesAsString = Encoding.UTF8.GetString(ParamBytes)
                            Dim AppInfos As Structures.AppParamJson = JsonConvert.DeserializeObject(Of Structures.AppParamJson)(ParamBytesAsString)

                            PS5GameLVItem.GameTitle = AppInfos.LocalizedParameters.EnUS.TitleName
                            'PS5GameLVItem.GameContentID = AppInfos.ContentId
                            'PS5GameLVItem.GameRegion = PS4Game.GetGameRegion(AppInfos.ContentId)
                        End If
                    End If

                    'Add to the AppsListView
                    If Dispatcher.CheckAccess() = False Then
                        Dispatcher.BeginInvoke(Sub() AppsListView.Items.Add(PS5GameLVItem))
                    Else
                        AppsListView.Items.Add(PS5GameLVItem)
                    End If
                End If

            Next

            'Disconnect
            conn.Disconnect()
        End Using
    End Sub

    Private Sub GameLoaderWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles GameLoaderWorker.RunWorkerCompleted
        NewLoadingWindow.Close()
    End Sub

    Private Sub PS5MenuIPTextChanged(sender As Object, e As RoutedEventArgs)
        ConsoleIP = NewPS5Menu.SharedConsoleAddress.Split(":"c)(0)
        ConsolePort = NewPS5Menu.SharedConsoleAddress.Split(":"c)(1)
    End Sub

End Class
