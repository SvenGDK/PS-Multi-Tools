Imports FluentFTP
Imports Microsoft.Web.WebView2.Core
Imports MS.Internal
Imports Newtonsoft.Json
Imports PS_Multi_Tools.INI
Imports psmt_lib
Imports psmt_lib.PS5ParamClass
Imports System.ComponentModel
Imports System.IO
Imports System.Security.Authentication
Imports System.Text
Imports System.Windows.Forms

Public Class PS5Library

    Dim WithEvents GameLoaderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}
    Dim WithEvents NewLoadingWindow As New SyncWindow() With {.Title = "Loading PS5 files", .ShowActivated = True}

    Public CurrentPath As String = ""
    Dim ConsoleIP As String = ""
    Dim ConsolePort As String = ""

    Dim PKGCount As Integer = 0
    Dim FilesCount As Integer = 0
    Dim URLs As New List(Of String)
    Dim CurrentURL As Integer = 0
    Dim TotalSize As Long = 0

    Dim IsSoundPlaying As Boolean = False

    'Supplemental library menu items
    Dim WithEvents OpenFolderMenuItem As New Controls.MenuItem() With {.Header = "Load folder with games and apps"}
    Dim WithEvents LoadFolderMenuItem As New Controls.MenuItem() With {.Header = "Load installed games and apps over FTP"}
    Dim WithEvents LoadPKGFolderMenuItem As New Controls.MenuItem() With {.Header = "Load patches PKG folder"}
    Dim WithEvents LoadDLFolderMenuItem As New Controls.MenuItem() With {.Header = "Open Downloads folder"}

    'Games context menu items
    Dim GamesContextMenu As New Controls.ContextMenu()
    Dim WithEvents GameCopyToMenuItem As New Controls.MenuItem() With {.Header = "Copy game to", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/copy-icon.png", UriKind.Relative))}}
    Dim WithEvents GameOpenLocationMenuItem As New Controls.MenuItem() With {.Header = "Open game folder", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/OpenFolder-icon.png", UriKind.Relative))}}
    Dim WithEvents GamePlayMenuItem As New Controls.MenuItem() With {.Header = "Play Soundtrack", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Play-icon.png", UriKind.Relative))}}
    Dim WithEvents GameCheckForUpdatesMenuItem As New Controls.MenuItem() With {.Header = "Check for updates", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Refresh-icon.png", UriKind.Relative))}}
    Dim WithEvents GameBrowseAssetsMenuItem As New Controls.MenuItem() With {.Header = "Browse assets", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/OpenFolder-icon.png", UriKind.Relative))}}

    Dim GameChangeTypeMenuItem As New Controls.MenuItem() With {.Header = "Change game type", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/rename.png", UriKind.Relative))}}
    Dim WithEvents GameChangeToGameMenuItem As New Controls.MenuItem() With {.Header = "To Game App"}
    Dim WithEvents GameChangeToNativeMediaMenuItem As New Controls.MenuItem() With {.Header = "To Native Media App"}
    Dim WithEvents GameChangeToRNPSMediaMenuItem As New Controls.MenuItem() With {.Header = "To RNPS Media App"}
    Dim WithEvents GameChangeToBuiltInMenuItem As New Controls.MenuItem() With {.Header = "To System Built-in"}
    Dim WithEvents GameChangeToBigDaemonMenuItem As New Controls.MenuItem() With {.Header = "To Big Daemon"}
    Dim WithEvents GameChangeToShellUIMenuItem As New Controls.MenuItem() With {.Header = "To ShellUI"}
    Dim WithEvents GameChangeToDaemonMenuItem As New Controls.MenuItem() With {.Header = "To Daemon"}
    Dim WithEvents GameChangeToShellAppMenuItem As New Controls.MenuItem() With {.Header = "To ShellApp"}

    Dim WithEvents GameRenameMenuItem As New Controls.MenuItem() With {.Header = "Rename game", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/rename.png", UriKind.Relative))}}
    Dim WithEvents GameChangeIconMenuItem As New Controls.MenuItem() With {.Header = "Change game icon", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Replace-icon.png", UriKind.Relative))}}
    Dim WithEvents GameChangeBackgroundMenuItem As New Controls.MenuItem() With {.Header = "Change game background", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Replace-icon.png", UriKind.Relative))}}
    Dim WithEvents GameChangeSoundtrackMenuItem As New Controls.MenuItem() With {.Header = "Change game soundtrack", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Replace-icon.png", UriKind.Relative))}}

    'Apps context menu items
    Dim AppsContextMenu As New Controls.ContextMenu()
    Dim WithEvents AppCopyToMenuItem As New Controls.MenuItem() With {.Header = "Copy app to", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/copy-icon.png", UriKind.Relative))}}
    Dim WithEvents AppOpenLocationMenuItem As New Controls.MenuItem() With {.Header = "Open app folder", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/OpenFolder-icon.png", UriKind.Relative))}}
    Dim WithEvents AppPlayMenuItem As New Controls.MenuItem() With {.Header = "Play Soundtrack", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Play-icon.png", UriKind.Relative))}}
    Dim WithEvents AppCheckForUpdatesMenuItem As New Controls.MenuItem() With {.Header = "Check for updates", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Refresh-icon.png", UriKind.Relative))}}

    Dim AppChangeTypeMenuItem As New Controls.MenuItem() With {.Header = "Change app type", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/rename.png", UriKind.Relative))}}
    Dim WithEvents AppChangeToGameMenuItem As New Controls.MenuItem() With {.Header = "To Game App"}
    Dim WithEvents AppChangeToNativeMediaMenuItem As New Controls.MenuItem() With {.Header = "To Native Media App"}
    Dim WithEvents AppChangeToRNPSMediaMenuItem As New Controls.MenuItem() With {.Header = "To RNPS Media App"}
    Dim WithEvents AppChangeToBuiltInMenuItem As New Controls.MenuItem() With {.Header = "To System Built-in"}
    Dim WithEvents AppChangeToBigDaemonMenuItem As New Controls.MenuItem() With {.Header = "To Big Daemon"}
    Dim WithEvents AppChangeToShellUIMenuItem As New Controls.MenuItem() With {.Header = "To ShellUI"}
    Dim WithEvents AppChangeToDaemonMenuItem As New Controls.MenuItem() With {.Header = "To Daemon"}
    Dim WithEvents AppChangeToShellAppMenuItem As New Controls.MenuItem() With {.Header = "To ShellApp"}

    Dim WithEvents AppRenameMenuItem As New Controls.MenuItem() With {.Header = "Rename application", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/rename.png", UriKind.Relative))}}
    Dim WithEvents AppChangeIconMenuItem As New Controls.MenuItem() With {.Header = "Change app icon", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Replace-icon.png", UriKind.Relative))}}
    Dim WithEvents AppChangeBackgroundMenuItem As New Controls.MenuItem() With {.Header = "Change app background", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Replace-icon.png", UriKind.Relative))}}
    Dim WithEvents AppChangeSoundtrackMenuItem As New Controls.MenuItem() With {.Header = "Change app soundtrack", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Replace-icon.png", UriKind.Relative))}}

    Private Sub PS5Library_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Will set the console IP and port when changing the console address in the settings
        AddHandler NewPS5Menu.IPTextChanged, AddressOf PS5MenuIPTextChanged

        'Add supplemental library menu items that will be handled in the app
        Dim LibraryMenuItem As Controls.MenuItem = CType(NewPS5Menu.Items(0), Controls.MenuItem)
        LibraryMenuItem.Items.Add(OpenFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadPKGFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem)

        'Add context menu for games
        GamesContextMenu.Items.Add(GameOpenLocationMenuItem)
        GamesContextMenu.Items.Add(GameCopyToMenuItem)
        GamesContextMenu.Items.Add(GameBrowseAssetsMenuItem)
        GamesContextMenu.Items.Add(GamePlayMenuItem)
        GamesContextMenu.Items.Add(GameCheckForUpdatesMenuItem)
        GamesContextMenu.Items.Add(New Separator())

        'Add sub menu for GameChangeType
        GameChangeTypeMenuItem.Items.Add(GameChangeToGameMenuItem)
        GameChangeTypeMenuItem.Items.Add(GameChangeToNativeMediaMenuItem)
        GameChangeTypeMenuItem.Items.Add(GameChangeToRNPSMediaMenuItem)
        GameChangeTypeMenuItem.Items.Add(GameChangeToBuiltInMenuItem)
        GameChangeTypeMenuItem.Items.Add(GameChangeToBigDaemonMenuItem)
        GameChangeTypeMenuItem.Items.Add(GameChangeToShellUIMenuItem)
        GameChangeTypeMenuItem.Items.Add(GameChangeToDaemonMenuItem)
        GameChangeTypeMenuItem.Items.Add(GameChangeToShellAppMenuItem)

        GamesContextMenu.Items.Add(GameChangeTypeMenuItem)
        GamesContextMenu.Items.Add(GameRenameMenuItem)
        GamesContextMenu.Items.Add(GameChangeIconMenuItem)
        GamesContextMenu.Items.Add(GameChangeBackgroundMenuItem)
        GamesContextMenu.Items.Add(GameChangeSoundtrackMenuItem)

        'Add context menu for apps
        AppsContextMenu.Items.Add(AppOpenLocationMenuItem)
        AppsContextMenu.Items.Add(AppCopyToMenuItem)
        AppsContextMenu.Items.Add(AppPlayMenuItem)
        AppsContextMenu.Items.Add(AppCheckForUpdatesMenuItem)
        AppsContextMenu.Items.Add(New Separator())

        'Add sub menu for AppChangeType
        AppChangeTypeMenuItem.Items.Add(AppChangeToGameMenuItem)
        AppChangeTypeMenuItem.Items.Add(AppChangeToNativeMediaMenuItem)
        AppChangeTypeMenuItem.Items.Add(AppChangeToRNPSMediaMenuItem)
        AppChangeTypeMenuItem.Items.Add(AppChangeToBuiltInMenuItem)
        AppChangeTypeMenuItem.Items.Add(AppChangeToBigDaemonMenuItem)
        AppChangeTypeMenuItem.Items.Add(AppChangeToShellUIMenuItem)
        AppChangeTypeMenuItem.Items.Add(AppChangeToDaemonMenuItem)
        AppChangeTypeMenuItem.Items.Add(AppChangeToShellAppMenuItem)

        AppsContextMenu.Items.Add(AppChangeTypeMenuItem)
        AppsContextMenu.Items.Add(AppRenameMenuItem)
        AppsContextMenu.Items.Add(AppChangeIconMenuItem)
        AppsContextMenu.Items.Add(AppChangeBackgroundMenuItem)
        AppsContextMenu.Items.Add(AppChangeSoundtrackMenuItem)

        'Set context menu
        GamesListView.ContextMenu = GamesContextMenu
        AppsListView.ContextMenu = AppsContextMenu
    End Sub

#Region "Game Loader"

    Public Enum LoadType
        LocalFolder
        FTP
        PKGs
    End Enum

    Public Structure GameLoaderArgs
        Private _Type As LoadType
        Private _FolderPath As String

        Public Property Type As LoadType
            Get
                Return _Type
            End Get
            Set
                _Type = Value
            End Set
        End Property

        Public Property FolderPath As String
            Get
                Return _FolderPath
            End Get
            Set
                _FolderPath = Value
            End Set
        End Property
    End Structure

    Private Sub GameLoaderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles GameLoaderWorker.DoWork

        Dim WorkerArgs As GameLoaderArgs = CType(e.Argument, GameLoaderArgs)

        If WorkerArgs.Type = LoadType.FTP Then

            'Get installed games and apps over FTP
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

        ElseIf WorkerArgs.Type = LoadType.PKGs Then

            'PS5 Patch Source pkgs
            For Each PatchSCPKG In Directory.GetFiles(WorkerArgs.FolderPath, "*_sc.pkg", SearchOption.AllDirectories)

                Dim NewPS5Game As New PS5Game()
                Dim PKGFileInfo As New FileInfo(PatchSCPKG)

                TotalSize = 0

                Using PARAMReader As New Process()
                    PARAMReader.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\ps5_pkg.exe"
                    PARAMReader.StartInfo.Arguments = "--psmtparam file:""" + PatchSCPKG + """"
                    PARAMReader.StartInfo.RedirectStandardOutput = True
                    PARAMReader.StartInfo.UseShellExecute = False
                    PARAMReader.StartInfo.CreateNoWindow = True
                    PARAMReader.Start()

                    Dim OutputReader As StreamReader = PARAMReader.StandardOutput
                    Dim ProcessOutput As String = OutputReader.ReadToEnd()

                    If ProcessOutput.Count > 0 Then
                        Dim ParamData = JsonConvert.DeserializeObject(Of PS5ParamClass.PS5Param)(ProcessOutput)

                        If ParamData IsNot Nothing Then
                            NewPS5Game.GameID = ParamData.TitleId
                            NewPS5Game.GameTitle = ParamData.LocalizedParameters.EnUS.TitleName
                            NewPS5Game.GameContentID = ParamData.ContentId
                            NewPS5Game.GameRegion = PS5Game.GetGameRegion(ParamData.TitleId)
                            NewPS5Game.GameCategory = "Game Patch"
                            NewPS5Game.GameSize = FormatNumber(GetDirSize(PKGFileInfo.DirectoryName) / 1073741824, 2) + " GB" 'Will only display correct if all PKG parts are present.
                            NewPS5Game.GameVersion = "Patch: " + ParamData.ContentVersion
                            NewPS5Game.GameRequiredFirmware = "Req.FW: " + ParamData.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, "."c).Insert(5, "."c).Insert(8, "."c).Remove(11, 8)

                            If File.Exists(Path.GetDirectoryName(PatchSCPKG) + "\icon0.png") Then
                                Dispatcher.BeginInvoke(Sub() NewPS5Game.GameCoverSource = New BitmapImage(New Uri(Path.GetDirectoryName(PatchSCPKG) + "\icon0.png", UriKind.RelativeOrAbsolute)))
                            Else
                                If Utils.IsURLValid("https://prosperopatches.com/" + ParamData.TitleId.Trim()) Then
                                    URLs.Add("https://prosperopatches.com/" + ParamData.TitleId.Trim()) 'Get the image from prosperopatches
                                End If
                            End If

                        End If

                        Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                        Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading PKG " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + PKGCount.ToString())

                        'Add to the ListView
                        If PatchesListView.Dispatcher.CheckAccess() = False Then
                            PatchesListView.Dispatcher.BeginInvoke(Sub() PatchesListView.Items.Add(NewPS5Game))
                        Else
                            PatchesListView.Items.Add(NewPS5Game)
                        End If

                    End If

                End Using

            Next

        ElseIf WorkerArgs.Type = LoadType.LocalFolder Then

            'PS5 Source pkgs
            For Each PatchSCPKG In Directory.GetFiles(WorkerArgs.FolderPath, "*_sc.pkg", SearchOption.AllDirectories)

                Dim PKGFileInfo As New FileInfo(PatchSCPKG)
                Dim NewPS5Game As New PS5Game()

                TotalSize = 0

                Using PARAMReader As New Process()
                    PARAMReader.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\ps5_pkg.exe"
                    PARAMReader.StartInfo.Arguments = "--psmtparam file:""" + PatchSCPKG + """"
                    PARAMReader.StartInfo.RedirectStandardOutput = True
                    PARAMReader.StartInfo.UseShellExecute = False
                    PARAMReader.StartInfo.CreateNoWindow = True
                    PARAMReader.Start()

                    Dim OutputReader As StreamReader = PARAMReader.StandardOutput
                    Dim ProcessOutput As String = OutputReader.ReadToEnd()

                    If ProcessOutput.Count > 0 Then
                        Dim ParamData = JsonConvert.DeserializeObject(Of PS5ParamClass.PS5Param)(ProcessOutput)

                        If ParamData IsNot Nothing Then
                            If ParamData.TitleId IsNot Nothing Then
                                NewPS5Game.GameID = "Title ID: " + ParamData.TitleId
                                NewPS5Game.GameRegion = "Region: " + PS5Game.GetGameRegion(ParamData.TitleId)
                            End If

                            If ParamData.LocalizedParameters.EnUS IsNot Nothing Then
                                NewPS5Game.GameTitle = ParamData.LocalizedParameters.EnUS.TitleName
                            End If
                            If ParamData.LocalizedParameters.DeDE IsNot Nothing Then
                                NewPS5Game.DEGameTitle = ParamData.LocalizedParameters.DeDE.TitleName
                            End If
                            If ParamData.LocalizedParameters.FrFR IsNot Nothing Then
                                NewPS5Game.FRGameTitle = ParamData.LocalizedParameters.FrFR.TitleName
                            End If
                            If ParamData.LocalizedParameters.ItIT IsNot Nothing Then
                                NewPS5Game.ITGameTitle = ParamData.LocalizedParameters.ItIT.TitleName
                            End If
                            If ParamData.LocalizedParameters.EsES IsNot Nothing Then
                                NewPS5Game.ESGameTitle = ParamData.LocalizedParameters.EsES.TitleName
                            End If
                            If ParamData.LocalizedParameters.JaJP IsNot Nothing Then
                                NewPS5Game.JPGameTitle = ParamData.LocalizedParameters.JaJP.TitleName
                            End If

                            If ParamData.ContentId IsNot Nothing Then
                                NewPS5Game.GameContentID = "Content ID: " + ParamData.ContentId
                            End If

                            If ParamData.ApplicationCategoryType = 0 Then
                                NewPS5Game.GameCategory = "Type: PS5 Game"
                            ElseIf ParamData.ApplicationCategoryType = 65792 Then
                                NewPS5Game.GameCategory = "Type: RNPS Media App"
                            ElseIf ParamData.ApplicationCategoryType = 131328 Then
                                NewPS5Game.GameCategory = "Type: System Built-in App"
                            ElseIf ParamData.ApplicationCategoryType = 131584 Then
                                NewPS5Game.GameCategory = "Type: Big Daemon"
                            ElseIf ParamData.ApplicationCategoryType = 16777216 Then
                                NewPS5Game.GameCategory = "Type: ShellUI"
                            ElseIf ParamData.ApplicationCategoryType = 33554432 Then
                                NewPS5Game.GameCategory = "Type: Daemon"
                            ElseIf ParamData.ApplicationCategoryType = 67108864 Then
                                NewPS5Game.GameCategory = "Type: ShellApp"
                            End If

                            NewPS5Game.GameSize = "Size: " + FormatNumber(GetDirSize(PKGFileInfo.DirectoryName) / 1073741824, 2) + " GB" 'Will only display correct if all PKG files are present.

                            If ParamData.ContentVersion IsNot Nothing Then
                                NewPS5Game.GameVersion = "Version: " + ParamData.ContentVersion
                            End If
                            If ParamData.RequiredSystemSoftwareVersion IsNot Nothing Then
                                NewPS5Game.GameRequiredFirmware = "Required Firmware: " + ParamData.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, "."c).Insert(5, "."c).Insert(8, "."c).Remove(11, 8)
                            End If

                            If Utils.IsURLValid("https://prosperopatches.com/" + ParamData.TitleId.Trim()) Then
                                URLs.Add("https://prosperopatches.com/" + ParamData.TitleId.Trim()) 'Get the image from prosperopatches
                            End If
                        End If

                        Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                        Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading PKG " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString())

                        'Add to the ListView
                        If ParamData.ApplicationCategoryType = 0 And ParamData.TitleId.StartsWith("PP") Then
                            If GamesListView.Dispatcher.CheckAccess() = False Then
                                GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPS5Game))
                            Else
                                GamesListView.Items.Add(NewPS5Game)
                            End If
                        Else
                            If AppsListView.Dispatcher.CheckAccess() = False Then
                                AppsListView.Dispatcher.BeginInvoke(Sub() AppsListView.Items.Add(NewPS5Game))
                            Else
                                AppsListView.Items.Add(NewPS5Game)
                            End If
                        End If

                    End If

                End Using

            Next

            'PS5 backup folders
            For Each ParamJSON In Directory.GetFiles(WorkerArgs.FolderPath, "param.json", SearchOption.AllDirectories)

                TotalSize = 0

                Dim NewPS5Game As New PS5Game()
                Dim ParamData = JsonConvert.DeserializeObject(Of PS5Param)(File.ReadAllText(ParamJSON))
                Dim ParamFileInfo As New FileInfo(ParamJSON)

                If ParamData IsNot Nothing Then
                    If ParamData.TitleId IsNot Nothing Then
                        NewPS5Game.GameID = "Title ID: " + ParamData.TitleId
                        NewPS5Game.GameRegion = "Region: " + PS5Game.GetGameRegion(ParamData.TitleId)
                    End If

                    If ParamData.LocalizedParameters.EnUS IsNot Nothing Then
                        NewPS5Game.GameTitle = ParamData.LocalizedParameters.EnUS.TitleName
                    End If
                    If ParamData.LocalizedParameters.DeDE IsNot Nothing Then
                        NewPS5Game.DEGameTitle = ParamData.LocalizedParameters.DeDE.TitleName
                    End If
                    If ParamData.LocalizedParameters.FrFR IsNot Nothing Then
                        NewPS5Game.FRGameTitle = ParamData.LocalizedParameters.FrFR.TitleName
                    End If
                    If ParamData.LocalizedParameters.ItIT IsNot Nothing Then
                        NewPS5Game.ITGameTitle = ParamData.LocalizedParameters.ItIT.TitleName
                    End If
                    If ParamData.LocalizedParameters.EsES IsNot Nothing Then
                        NewPS5Game.ESGameTitle = ParamData.LocalizedParameters.EsES.TitleName
                    End If
                    If ParamData.LocalizedParameters.JaJP IsNot Nothing Then
                        NewPS5Game.JPGameTitle = ParamData.LocalizedParameters.JaJP.TitleName
                    End If

                    If ParamData.ContentId IsNot Nothing Then
                        NewPS5Game.GameContentID = "Content ID: " + ParamData.ContentId
                    End If

                    If ParamData.ApplicationCategoryType = 0 Then
                        NewPS5Game.GameCategory = "Type: Game"
                    ElseIf ParamData.ApplicationCategoryType = 65536 Then
                        NewPS5Game.GameCategory = "Type: Native Media App"
                    ElseIf ParamData.ApplicationCategoryType = 65792 Then
                        NewPS5Game.GameCategory = "Type: RNPS Media App"
                    ElseIf ParamData.ApplicationCategoryType = 131328 Then
                        NewPS5Game.GameCategory = "Type: System Built-in App"
                    ElseIf ParamData.ApplicationCategoryType = 131584 Then
                        NewPS5Game.GameCategory = "Type: Big Daemon"
                    ElseIf ParamData.ApplicationCategoryType = 16777216 Then
                        NewPS5Game.GameCategory = "Type: ShellUI"
                    ElseIf ParamData.ApplicationCategoryType = 33554432 Then
                        NewPS5Game.GameCategory = "Type: Daemon"
                    ElseIf ParamData.ApplicationCategoryType = 67108864 Then
                        NewPS5Game.GameCategory = "Type: ShellApp"
                    Else
                        NewPS5Game.GameCategory = "Type: Unknown"
                    End If

                    NewPS5Game.GameFileOrFolderPath = Directory.GetParent(ParamFileInfo.FullName).Parent.FullName
                    NewPS5Game.GameSize = "Size: " + FormatNumber(GetDirSize(Directory.GetParent(ParamFileInfo.FullName).Parent.FullName) / 1073741824, 2) + " GB" 'Will only display correct if all files are present.

                    If ParamData.ContentVersion IsNot Nothing Then
                        NewPS5Game.GameVersion = "Version: " + ParamData.ContentVersion
                    End If
                    If ParamData.RequiredSystemSoftwareVersion IsNot Nothing Then
                        NewPS5Game.GameRequiredFirmware = "Required Firmware: " + ParamData.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, "."c).Insert(5, "."c).Insert(8, "."c).Remove(11, 8)

                        If ParamData.RequiredSystemSoftwareVersion > "0x0451000000000000" Then
                            NewPS5Game.IsCompatibleFW = "The required firmware for this game is too high and might not be supported."
                        Else
                            NewPS5Game.IsCompatibleFW = "The required firmware for this game is compatible."
                        End If
                    End If

                    Dim SCESYSFolder As String = Path.GetDirectoryName(ParamFileInfo.FullName)

                    'Check for game icon
                    If File.Exists(SCESYSFolder + "\icon0.png") Then
                        Dispatcher.BeginInvoke(Sub()
                                                   Dim TempBitmapImage = New BitmapImage()
                                                   TempBitmapImage.BeginInit()
                                                   TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                   TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                   TempBitmapImage.UriSource = New Uri(SCESYSFolder + "\icon0.png", UriKind.RelativeOrAbsolute)
                                                   TempBitmapImage.EndInit()
                                                   NewPS5Game.GameCoverSource = TempBitmapImage
                                               End Sub)
                    Else
                        If ParamData.ApplicationCategoryType = 0 And ParamData.TitleId.StartsWith("PP") Then
                            If Utils.IsURLValid("https://prosperopatches.com/" + ParamData.TitleId.Trim()) Then
                                URLs.Add("https://prosperopatches.com/" + ParamData.TitleId.Trim()) 'Get the image from prosperopatches
                            End If
                        End If
                    End If

                    'Check for game background
                    If File.Exists(SCESYSFolder + "\pic0.png") Then
                        Dispatcher.BeginInvoke(Sub()
                                                   Dim TempBitmapImage = New BitmapImage()
                                                   TempBitmapImage.BeginInit()
                                                   TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                   TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                   TempBitmapImage.UriSource = New Uri(SCESYSFolder + "\pic0.png", UriKind.RelativeOrAbsolute)
                                                   TempBitmapImage.EndInit()
                                                   NewPS5Game.GameBGSource = TempBitmapImage
                                               End Sub)
                    End If

                    'Check for game soundtrack
                    If File.Exists(SCESYSFolder + "\snd0.at9") Then
                        NewPS5Game.GameSoundFile = SCESYSFolder + "\snd0.at9"
                    End If

                    Dim MainGamePath As String = Directory.GetParent(ParamFileInfo.FullName).Parent.FullName

                    'Add other available content ids as tooltip
                    Dim GameContentIDs As String = ""
                    If File.Exists(MainGamePath + "\contentids.json") Then
                        For Each Line In File.ReadAllLines(MainGamePath + "\contentids.json")
                            If Not String.IsNullOrWhiteSpace(Line) AndAlso Line.StartsWith(vbTab) Then
                                GameContentIDs += Line.Split(""""c)(1) + vbCrLf
                            End If
                        Next
                        If Not String.IsNullOrEmpty(GameContentIDs) Then
                            GameContentIDs = GameContentIDs.TrimEnd()
                            NewPS5Game.GameContentIDs = GameContentIDs
                        End If
                    End If

                    Dim ToolTipString As String = "This game includes: "
                    'Check if prx is encrypted
                    If File.Exists(MainGamePath + "\sce_module\libc.prx") Then
                        Dim FirstStr As String = ""
                        Using PRXReader As New FileStream(MainGamePath + "\sce_module\libc.prx", FileMode.Open)
                            Dim BinReader As New BinaryReader(PRXReader)
                            FirstStr = BinReader.ReadString()
                            PRXReader.Close()
                        End Using
                        If Not String.IsNullOrEmpty(FirstStr) Then
                            If FirstStr.Contains("ELF") Then
                                ToolTipString += vbCrLf + "Decrypted .prx files"
                            Else
                                ToolTipString += vbCrLf + "Encrypted .prx files"
                            End If
                        End If
                    End If
                    'Check if eboot is encrypted and signed
                    If File.Exists(MainGamePath + "\eboot.bin") Then
                        Dim FirstStr As String = ""
                        Dim SecondStr As String = ""
                        Using EBOOTReader As New FileStream(MainGamePath + "\eboot.bin", FileMode.Open)
                            Dim BinReader As New BinaryReader(EBOOTReader)

                            FirstStr = BinReader.ReadString()
                            BinReader.BaseStream.Seek(416, SeekOrigin.Begin)
                            SecondStr = BinReader.ReadString()

                            BinReader.Close()
                            EBOOTReader.Close()
                        End Using
                        If Not String.IsNullOrEmpty(FirstStr) Then
                            If FirstStr.Contains("ELF") Then
                                ToolTipString += vbCrLf + "EBOOT: Decrypted"
                            Else
                                ToolTipString += vbCrLf + "EBOOT: Encrypted"
                            End If
                        End If
                        If Not String.IsNullOrEmpty(SecondStr) Then
                            If SecondStr.Contains("ELF") Then
                                ToolTipString += vbCrLf + "EBOOT: Signed"
                            Else
                                ToolTipString += vbCrLf + "EBOOT: Decrypted & Unsigned"
                            End If
                        End If
                    End If
                    'Check for some other encrypted files
                    If File.Exists(SCESYSFolder + "\trophy2\trophy00.UCP") Then
                        ToolTipString += vbCrLf + "Trophy2: trophy00.UCP"
                    End If
                    If File.Exists(SCESYSFolder + "\uds\uds00.ucp") Then
                        ToolTipString += vbCrLf + "UDS: uds00.ucp"
                    End If
                    If File.Exists(SCESYSFolder + "\keystone") Then
                        ToolTipString += vbCrLf + "Keystone: keystone"
                    End If
                    If File.Exists(SCESYSFolder + "\nptitle.dat") Then
                        ToolTipString += vbCrLf + "NPTitle: nptitle.dat"
                    End If
                    If File.Exists(MainGamePath + "\disc_info.dat") Then
                        ToolTipString += vbCrLf + "Disc Info: disc_info.dat"
                    End If
                    If File.Exists(MainGamePath + "\ext_info.dat") Then
                        ToolTipString += vbCrLf + "Ext Info: ext_info.dat"
                    End If
                    If File.Exists(SCESYSFolder + "\about\right.sprx") Then
                        ToolTipString += vbCrLf + "Right: right.sprx"
                    End If
                    If File.Exists(SCESYSFolder + "\about\right.sprx.auth_info") Then
                        ToolTipString += vbCrLf + "Right Auth info: right.sprx.auth_info"
                    End If

                    NewPS5Game.DecFilesIncluded = ToolTipString
                End If

                'Update progress
                Dispatcher.BeginInvoke(Sub()
                                           NewLoadingWindow.LoadProgressBar.Value += 1
                                           NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                                       End Sub)

                'Add to the ListView
                If ParamData.ApplicationCategoryType = 0 And ParamData.TitleId.StartsWith("PP") Then 'Games
                    If GamesListView.Dispatcher.CheckAccess() = False Then
                        GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPS5Game))
                    Else
                        GamesListView.Items.Add(NewPS5Game)
                    End If
                ElseIf ParamData.ApplicationCategoryType = 65536 And ParamData.TitleId.StartsWith("PP") Then 'Media apps
                    If AppsListView.Dispatcher.CheckAccess() = False Then
                        AppsListView.Dispatcher.BeginInvoke(Sub() AppsListView.Items.Add(NewPS5Game))
                    Else
                        AppsListView.Items.Add(NewPS5Game)
                    End If
                Else
                    If AppsListView.Dispatcher.CheckAccess() = False Then 'NPXS
                        AppsListView.Dispatcher.BeginInvoke(Sub() AppsListView.Items.Add(NewPS5Game))
                    Else
                        AppsListView.Items.Add(NewPS5Game)
                    End If
                End If

            Next

        End If
    End Sub

    Private Sub GameLoaderWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles GameLoaderWorker.RunWorkerCompleted
        If URLs.Count > 0 Then
            NewLoadingWindow.LoadStatusTextBlock.Text = "Getting " + URLs.Count.ToString() + " available covers"
            NewLoadingWindow.LoadProgressBar.Value = 0
            NewLoadingWindow.LoadProgressBar.Maximum = URLs.Count

            ContentWebView.Source = New Uri(URLs.Item(0))
        Else
            NewLoadingWindow.Close()
        End If
    End Sub

#End Region

#Region "Game Context Menu Actions"

    Private Sub GameCopyToMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameCopyToMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(GamesListView.SelectedItem, PS5Game)
            If Not String.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath) Then

                Dim FBD As New FolderBrowserDialog() With {.Description = "Where do you want to copy the selected game ?"}
                If FBD.ShowDialog() = Forms.DialogResult.OK Then

                    Dim NewCopyWindow As New CopyWindow With {
                        .ShowActivated = True,
                        .WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        .BackupDestinationPath = FBD.SelectedPath + "\",
                        .Title = "Copying " + SelectedPS5Game.GameTitle + " to " + FBD.SelectedPath + Path.GetFileName(SelectedPS5Game.GameFileOrFolderPath),
                        .BackupPath = SelectedPS5Game.GameFileOrFolderPath
                    }

                    If SelectedPS5Game.GameCoverSource IsNot Nothing Then
                        NewCopyWindow.GameIcon = SelectedPS5Game.GameCoverSource
                    End If

                    If NewCopyWindow.ShowDialog() = True Then
                        MsgBox("Game copied with success !", MsgBoxStyle.Information, "Completed")
                    End If
                End If

            End If
        End If
    End Sub

    Private Sub GamePlayMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GamePlayMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(GamesListView.SelectedItem, PS5Game)
            If SelectedPS5Game.GameSoundFile IsNot Nothing Then
                If IsSoundPlaying Then
                    Utils.StopGameSound()
                    IsSoundPlaying = False
                    GamePlayMenuItem.Header = "Play Soundtrack"
                    GamePlayMenuItem.Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Play-icon.png", UriKind.Relative))}
                Else
                    Utils.PlayGameSound(SelectedPS5Game.GameSoundFile)
                    IsSoundPlaying = True
                    GamePlayMenuItem.Header = "Stop Soundtrack"
                    GamePlayMenuItem.Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Stop-icon.png", UriKind.Relative))}
                End If
            Else
                If IsSoundPlaying Then
                    Utils.StopGameSound()
                    IsSoundPlaying = False
                    GamePlayMenuItem.Header = "Play Soundtrack"
                    GamePlayMenuItem.Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Play-icon.png", UriKind.Relative))}
                Else
                    MsgBox("No game soundtrack found.", MsgBoxStyle.Information)
                End If
            End If
        End If
    End Sub

    Private Sub GameCheckForUpdatesMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameCheckForUpdatesMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(GamesListView.SelectedItem, PS5Game)
            Dim NewPS5GamePatches As New PS5GamePatches With {.ShowActivated = True, .SearchForGamePatchWithID = SelectedPS5Game.GameID.Split(New String() {"Title ID: "}, StringSplitOptions.None)(1)}
            NewPS5GamePatches.Show()
        End If
    End Sub

    Private Sub GameChangeToGameMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeToGameMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(GamesListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 0

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: Game"

                    MsgBox("Game type changed!", MsgBoxStyle.Information)
                    GamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameChangeToNativeMediaMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeToNativeMediaMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(GamesListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 65536

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: Game"

                    MsgBox("Game type changed!", MsgBoxStyle.Information)
                    GamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameChangeToRNPSMediaMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeToRNPSMediaMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(GamesListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 65792

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: Game"

                    MsgBox("Game type changed!", MsgBoxStyle.Information)
                    GamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameChangeToShellAppMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeToShellAppMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(GamesListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 67108864

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: Game"

                    MsgBox("Game type changed!", MsgBoxStyle.Information)
                    GamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameChangeToShellUIMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeToShellUIMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(GamesListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 16777216

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: Game"

                    MsgBox("Game type changed!", MsgBoxStyle.Information)
                    GamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameChangeToBigDaemonMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeToBigDaemonMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(GamesListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 131584

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: Game"

                    MsgBox("Game type changed!", MsgBoxStyle.Information)
                    GamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameChangeToBuiltInMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeToBuiltInMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(GamesListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 131328

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: Game"

                    MsgBox("Game type changed!", MsgBoxStyle.Information)
                    GamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameChangeToDaemonMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeToDaemonMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(GamesListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 33554432

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: Game"

                    MsgBox("Game type changed!", MsgBoxStyle.Information)
                    GamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameRenameMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameRenameMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(GamesListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)

                    'Set new default language (optional)
                    If MsgBox("Do you want to change the default language for this game?", MsgBoxStyle.YesNo, "Also change default language?") = MsgBoxResult.Yes Then
                        Dim NewDefaultLanguage As String = InputBox("Enter a new default language identifier (like en-US, de-DE, fr-FR, es-ES...):", "Change default language", ParamData.LocalizedParameters.DefaultLanguage)
                        ParamData.LocalizedParameters.DefaultLanguage = NewDefaultLanguage
                    End If

                    'Set new title
                    Dim NewAppTitle As String = InputBox("Enter a new title for this game:", "Change game title", ParamData.LocalizedParameters.EnUS.TitleName)
                    If ParamData.LocalizedParameters.ArAE IsNot Nothing Then
                        ParamData.LocalizedParameters.ArAE.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.CsCZ IsNot Nothing Then
                        ParamData.LocalizedParameters.CsCZ.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.DaDK IsNot Nothing Then
                        ParamData.LocalizedParameters.DaDK.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.DaDK IsNot Nothing Then
                    End If
                    If ParamData.LocalizedParameters.DeDE IsNot Nothing Then
                        ParamData.LocalizedParameters.DeDE.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.ElGR IsNot Nothing Then
                        ParamData.LocalizedParameters.ElGR.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.EnGB IsNot Nothing Then
                        ParamData.LocalizedParameters.EnGB.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.EnUS IsNot Nothing Then
                        ParamData.LocalizedParameters.EnUS.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.Es419 IsNot Nothing Then
                        ParamData.LocalizedParameters.Es419.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.FiFI IsNot Nothing Then
                        ParamData.LocalizedParameters.FiFI.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.FrCA IsNot Nothing Then
                        ParamData.LocalizedParameters.FrCA.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.FrFR IsNot Nothing Then
                        ParamData.LocalizedParameters.FrFR.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.HuHU IsNot Nothing Then
                        ParamData.LocalizedParameters.HuHU.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.IdID IsNot Nothing Then
                        ParamData.LocalizedParameters.IdID.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.ItIT IsNot Nothing Then
                        ParamData.LocalizedParameters.ItIT.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.JaJP IsNot Nothing Then
                        ParamData.LocalizedParameters.JaJP.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.KoKR IsNot Nothing Then
                        ParamData.LocalizedParameters.KoKR.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.NoNO IsNot Nothing Then
                        ParamData.LocalizedParameters.NoNO.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.NlNL IsNot Nothing Then
                        ParamData.LocalizedParameters.NlNL.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.PlPL IsNot Nothing Then
                        ParamData.LocalizedParameters.PlPL.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.PtBR IsNot Nothing Then
                        ParamData.LocalizedParameters.PtBR.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.PtPT IsNot Nothing Then
                        ParamData.LocalizedParameters.PtPT.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.RoRO IsNot Nothing Then
                        ParamData.LocalizedParameters.RoRO.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.RuRU IsNot Nothing Then
                        ParamData.LocalizedParameters.RuRU.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.SvSE IsNot Nothing Then
                        ParamData.LocalizedParameters.SvSE.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.ThTH IsNot Nothing Then
                        ParamData.LocalizedParameters.ThTH.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.TrTR IsNot Nothing Then
                        ParamData.LocalizedParameters.TrTR.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.ViVN IsNot Nothing Then
                        ParamData.LocalizedParameters.ViVN.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.ZhHans IsNot Nothing Then
                        ParamData.LocalizedParameters.ZhHans.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.ZhHant IsNot Nothing Then
                        ParamData.LocalizedParameters.ZhHant.TitleName = NewAppTitle
                    End If

                    'Write back to file
                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)

                    'Set new title in GamesListView
                    SelectedPS5Game.GameTitle = NewAppTitle

                    MsgBox("Game renamed!", MsgBoxStyle.Information)
                    GamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameChangeIconMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeIconMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(GamesListView.SelectedItem, PS5Game)
            Dim OFD As New OpenFileDialog() With {.Title = "Select a new icon0.png image for this game", .Filter = "PNG images (*.png)|*.png", .Multiselect = False}

            If OFD.ShowDialog() = Forms.DialogResult.OK Then
                'Move new icon to sce_sys
                File.Copy(OFD.FileName, SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\icon0.png", True)

                'Reload new icon
                Dim TempBitmapImage = New BitmapImage()
                TempBitmapImage.BeginInit()
                TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                TempBitmapImage.UriSource = New Uri(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\icon0.png", UriKind.RelativeOrAbsolute)
                TempBitmapImage.EndInit()
                SelectedPS5Game.GameCoverSource = TempBitmapImage

                GamesListView.Items.Refresh()
            End If
        End If
    End Sub

    Private Sub GameChangeBackgroundMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeBackgroundMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(GamesListView.SelectedItem, PS5Game)
            Dim OFD As New OpenFileDialog() With {.Title = "Select a new pic0.png image for this game", .Filter = "PNG images (*.png)|*.png", .Multiselect = False}

            If OFD.ShowDialog() = Forms.DialogResult.OK Then
                File.Copy(OFD.FileName, SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\pic0.png", True)

                'Set new background
                Dim TempBitmapImage = New BitmapImage()
                TempBitmapImage.BeginInit()
                TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                TempBitmapImage.UriSource = New Uri(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\icon0.png", UriKind.RelativeOrAbsolute)
                TempBitmapImage.EndInit()
                SelectedPS5Game.GameBGSource = TempBitmapImage

                GamesListView.Items.Refresh()
            End If
        End If
    End Sub

    Private Sub GameChangeSoundtrackMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeSoundtrackMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(GamesListView.SelectedItem, PS5Game)
            Dim OFD As New OpenFileDialog() With {.Title = "Select a new snd0.at9 soundtrack for this game", .Filter = "AT9 Audio Files (*.at9)|*.at9", .Multiselect = False}

            If IsSoundPlaying Then
                MsgBox("A soundtrack is currently playing. Please stop it before changing any soundtrack.", MsgBoxStyle.Information)
            Else
                'Set new soundtrack
                If OFD.ShowDialog() = Forms.DialogResult.OK Then
                    File.Copy(OFD.FileName, SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\snd0.at9", True)
                End If
            End If
        End If
    End Sub

    Private Sub GameOpenLocationMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameOpenLocationMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(GamesListView.SelectedItem, PS5Game)
            Process.Start(SelectedPS5Game.GameFileOrFolderPath)
        End If
    End Sub

    Private Sub GameBrowseAssetsMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameBrowseAssetsMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(GamesListView.SelectedItem, PS5Game)
            Dim NewAssetBrowser As New PS5AssetsBrowser With {.SelectedDirectory = SelectedPS5Game.GameFileOrFolderPath}
            NewAssetBrowser.Show()
        End If
    End Sub

#End Region

#Region "Apps Context Menu Actions"

    Private Sub AppCopyToMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppCopyToMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            If Not String.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath) Then

                Dim FBD As New FolderBrowserDialog() With {.Description = "Where do you want to copy the selected app ?"}
                If FBD.ShowDialog() = Forms.DialogResult.OK Then

                    Dim NewCopyWindow As New CopyWindow With {
                        .ShowActivated = True,
                        .WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        .BackupDestinationPath = FBD.SelectedPath + "\",
                        .Title = "Copying " + SelectedPS5Game.GameTitle + " to " + FBD.SelectedPath + Path.GetFileName(SelectedPS5Game.GameFileOrFolderPath),
                        .BackupPath = SelectedPS5Game.GameFileOrFolderPath
                    }

                    If SelectedPS5Game.GameCoverSource IsNot Nothing Then
                        NewCopyWindow.GameIcon = SelectedPS5Game.GameCoverSource
                    End If

                    If NewCopyWindow.ShowDialog() = True Then
                        MsgBox("Game copied with success !", MsgBoxStyle.Information, "Completed")
                    End If
                End If

            End If
        End If
    End Sub

    Private Sub AppPlayMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppPlayMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            If SelectedPS5Game.GameSoundFile IsNot Nothing Then
                If IsSoundPlaying Then
                    Utils.StopGameSound()
                    IsSoundPlaying = False
                    GamePlayMenuItem.Header = "Play Soundtrack"
                    GamePlayMenuItem.Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Play-icon.png", UriKind.Relative))}
                Else
                    Utils.PlayGameSound(SelectedPS5Game.GameSoundFile)
                    IsSoundPlaying = True
                    GamePlayMenuItem.Header = "Stop Soundtrack"
                    GamePlayMenuItem.Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Stop-icon.png", UriKind.Relative))}
                End If
            Else
                If IsSoundPlaying Then
                    Utils.StopGameSound()
                    IsSoundPlaying = False
                    GamePlayMenuItem.Header = "Play Soundtrack"
                    GamePlayMenuItem.Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Play-icon.png", UriKind.Relative))}
                Else
                    MsgBox("No app soundtrack found.", MsgBoxStyle.Information)
                End If
            End If
        End If
    End Sub

    Private Sub AppCheckForUpdatesMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppCheckForUpdatesMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            If Not SelectedPS5Game.GameID.StartsWith("NPXS") Then
                Dim NewPS5GamePatches As New PS5GamePatches With {.ShowActivated = True, .SearchForGamePatchWithID = SelectedPS5Game.GameID.Split(New String() {"Title ID: "}, StringSplitOptions.None)(1)}
                NewPS5GamePatches.Show()
            Else
                MsgBox("Updates can only be checked for retail games and apps.", MsgBoxStyle.Information)
            End If
        End If
    End Sub

    Private Sub AppChangeToGameMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppChangeToGameMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 0

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: Game"

                    MsgBox("App type changed!", MsgBoxStyle.Information)
                    AppsListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub AppChangeToNativeMediaMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppChangeToNativeMediaMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 65536

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: Native Media App"

                    MsgBox("App type changed!", MsgBoxStyle.Information)
                    AppsListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub AppChangeToRNPSMediaMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppChangeToRNPSMediaMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 65792

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: RNPS Media App"

                    MsgBox("App type changed!", MsgBoxStyle.Information)
                    AppsListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub AppChangeToBigDaemonMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppChangeToBigDaemonMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 131584

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: Big Daemon"

                    MsgBox("App type changed!", MsgBoxStyle.Information)
                    AppsListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub AppChangeToBuiltInMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppChangeToBuiltInMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 131328

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: System Built-In"

                    MsgBox("App type changed!", MsgBoxStyle.Information)
                    AppsListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub AppChangeToDaemonMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppChangeToDaemonMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 33554432

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: Daemon"

                    MsgBox("App type changed!", MsgBoxStyle.Information)
                    AppsListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub AppChangeToShellAppMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppChangeToShellAppMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 67108864

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: ShellApp"

                    MsgBox("App type changed!", MsgBoxStyle.Information)
                    AppsListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub AppChangeToShellUIMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppChangeToShellUIMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 16777216

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: ShellUI"

                    MsgBox("App type changed!", MsgBoxStyle.Information)
                    AppsListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub AppRenameMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppRenameMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)

                    If MsgBox("Do you want to change the default language for this app?", MsgBoxStyle.YesNo, "Also change default language?") = MsgBoxResult.Yes Then
                        Dim NewDefaultLanguage As String = InputBox("Enter a new default language identifier (like en-US, de-DE, fr-FR, es-ES...):", "Change default language", ParamData.LocalizedParameters.DefaultLanguage)
                        ParamData.LocalizedParameters.DefaultLanguage = NewDefaultLanguage
                    End If

                    Dim NewAppTitle As String = InputBox("Enter a new title for this app:", "Change app title", ParamData.LocalizedParameters.EnUS.TitleName)
                    If ParamData.LocalizedParameters.ArAE IsNot Nothing Then
                        ParamData.LocalizedParameters.ArAE.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.CsCZ IsNot Nothing Then
                        ParamData.LocalizedParameters.CsCZ.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.DaDK IsNot Nothing Then
                        ParamData.LocalizedParameters.DaDK.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.DaDK IsNot Nothing Then
                    End If
                    If ParamData.LocalizedParameters.DeDE IsNot Nothing Then
                        ParamData.LocalizedParameters.DeDE.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.ElGR IsNot Nothing Then
                        ParamData.LocalizedParameters.ElGR.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.EnGB IsNot Nothing Then
                        ParamData.LocalizedParameters.EnGB.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.EnUS IsNot Nothing Then
                        ParamData.LocalizedParameters.EnUS.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.Es419 IsNot Nothing Then
                        ParamData.LocalizedParameters.Es419.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.FiFI IsNot Nothing Then
                        ParamData.LocalizedParameters.FiFI.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.FrCA IsNot Nothing Then
                        ParamData.LocalizedParameters.FrCA.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.FrFR IsNot Nothing Then
                        ParamData.LocalizedParameters.FrFR.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.HuHU IsNot Nothing Then
                        ParamData.LocalizedParameters.HuHU.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.IdID IsNot Nothing Then
                        ParamData.LocalizedParameters.IdID.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.ItIT IsNot Nothing Then
                        ParamData.LocalizedParameters.ItIT.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.JaJP IsNot Nothing Then
                        ParamData.LocalizedParameters.JaJP.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.KoKR IsNot Nothing Then
                        ParamData.LocalizedParameters.KoKR.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.NoNO IsNot Nothing Then
                        ParamData.LocalizedParameters.NoNO.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.NlNL IsNot Nothing Then
                        ParamData.LocalizedParameters.NlNL.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.PlPL IsNot Nothing Then
                        ParamData.LocalizedParameters.PlPL.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.PtBR IsNot Nothing Then
                        ParamData.LocalizedParameters.PtBR.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.PtPT IsNot Nothing Then
                        ParamData.LocalizedParameters.PtPT.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.RoRO IsNot Nothing Then
                        ParamData.LocalizedParameters.RoRO.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.RuRU IsNot Nothing Then
                        ParamData.LocalizedParameters.RuRU.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.SvSE IsNot Nothing Then
                        ParamData.LocalizedParameters.SvSE.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.ThTH IsNot Nothing Then
                        ParamData.LocalizedParameters.ThTH.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.TrTR IsNot Nothing Then
                        ParamData.LocalizedParameters.TrTR.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.ViVN IsNot Nothing Then
                        ParamData.LocalizedParameters.ViVN.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.ZhHans IsNot Nothing Then
                        ParamData.LocalizedParameters.ZhHans.TitleName = NewAppTitle
                    End If
                    If ParamData.LocalizedParameters.ZhHant IsNot Nothing Then
                        ParamData.LocalizedParameters.ZhHant.TitleName = NewAppTitle
                    End If

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameTitle = NewAppTitle

                    MsgBox("App renamed!", MsgBoxStyle.Information)
                    AppsListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub AppChangeIconMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppChangeIconMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            Dim OFD As New OpenFileDialog() With {.Title = "Select a new icon0.png image for this app", .Filter = "PNG images (*.png)|*.png", .Multiselect = False}

            If OFD.ShowDialog() = Forms.DialogResult.OK Then
                'Move new icon to sce_sys
                File.Copy(OFD.FileName, SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\icon0.png", True)

                'Reload new icon
                Dim TempBitmapImage = New BitmapImage()
                TempBitmapImage.BeginInit()
                TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                TempBitmapImage.UriSource = New Uri(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\icon0.png", UriKind.RelativeOrAbsolute)
                TempBitmapImage.EndInit()
                SelectedPS5Game.GameCoverSource = TempBitmapImage

                AppsListView.Items.Refresh()
            End If
        End If
    End Sub

    Private Sub AppChangeBackgroundMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppChangeBackgroundMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            Dim OFD As New OpenFileDialog() With {.Title = "Select a new pic0.png image for this app", .Filter = "PNG images (*.png)|*.png", .Multiselect = False}

            If OFD.ShowDialog() = Forms.DialogResult.OK Then
                File.Copy(OFD.FileName, SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\pic0.png", True)

                'Set new background
                Dim TempBitmapImage = New BitmapImage()
                TempBitmapImage.BeginInit()
                TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                TempBitmapImage.UriSource = New Uri(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\icon0.png", UriKind.RelativeOrAbsolute)
                TempBitmapImage.EndInit()
                SelectedPS5Game.GameBGSource = TempBitmapImage
            End If
        End If
    End Sub

    Private Sub AppChangeSoundtrackMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppChangeSoundtrackMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            Dim OFD As New OpenFileDialog() With {.Title = "Select a new snd0.at9 soundtrack for this app", .Filter = "AT9 Audio Files (*.at9)|*.at9", .Multiselect = False}

            If IsSoundPlaying Then
                MsgBox("A soundtrack is currently playing. Please stop it before changing any soundtrack.", MsgBoxStyle.Information)
            Else
                If OFD.ShowDialog() = Forms.DialogResult.OK Then
                    File.Copy(OFD.FileName, SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\snd0.at9", True)
                End If
            End If
        End If
    End Sub

    Private Sub AppOpenLocationMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppOpenLocationMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            Process.Start(SelectedPS5Game.GameFileOrFolderPath)
        End If
    End Sub

#End Region

    Private Sub LoadFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadFolderMenuItem.Click
        If Not String.IsNullOrEmpty(ConsoleIP) Then
            GamesListView.Items.Clear()
            AppsListView.Items.Clear()

            'Show the loading progress window
            NewLoadingWindow = New SyncWindow() With {.Title = "Loading PS5 files", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.IsIndeterminate = True
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading files, please wait ..."
            NewLoadingWindow.Show()

            'Load the files
            GameLoaderWorker.RunWorkerAsync(New GameLoaderArgs() With {.Type = LoadType.FTP, .FolderPath = String.Empty})
        Else
            MsgBox("Please enter your console's FTP IP address in the settings before continuing.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub LoadPKGFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadPKGFolderMenuItem.Click
        Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Select your PS5 patches folder"}

        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            PatchesListView.Items.Clear()

            PKGCount = Directory.GetFiles(FBD.SelectedPath, "*_sc.pkg", SearchOption.AllDirectories).Count

            NewLoadingWindow = New SyncWindow() With {.Title = "Loading PS5 patches", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.Maximum = PKGCount
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + PKGCount.ToString()
            NewLoadingWindow.Show()

            GameLoaderWorker.RunWorkerAsync(New GameLoaderArgs() With {.Type = LoadType.PKGs, .FolderPath = FBD.SelectedPath})
        End If
    End Sub

    Private Sub OpenFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenFolderMenuItem.Click
        Dim FBD As New FolderBrowserDialog() With {.Description = "Select your PS5 games & apps folder"}

        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            GamesListView.Items.Clear()

            FilesCount = 0
            FilesCount += Directory.GetFiles(FBD.SelectedPath, "*_sc.pkg", SearchOption.AllDirectories).Count
            FilesCount += Directory.GetFiles(FBD.SelectedPath, "param.json", SearchOption.AllDirectories).Count

            NewLoadingWindow = New SyncWindow() With {.Title = "Loading PS5 games & apps", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.Maximum = FilesCount
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + FilesCount.ToString()
            NewLoadingWindow.Show()

            GameLoaderWorker.RunWorkerAsync(New GameLoaderArgs() With {.Type = LoadType.LocalFolder, .FolderPath = FBD.SelectedPath})
        End If
    End Sub

    Private Sub PS5MenuIPTextChanged(sender As Object, e As RoutedEventArgs)
        ConsoleIP = NewPS5Menu.SharedConsoleAddress.Split(":"c)(0)
        ConsolePort = NewPS5Menu.SharedConsoleAddress.Split(":"c)(1)

        'Save config
        Try
            Dim MainConfig As New IniFile(My.Computer.FileSystem.CurrentDirectory + "\psmt-config.ini")
            MainConfig.IniWriteValue("PS5 Tools", "IP", NewPS5Menu.SharedConsoleAddress.Split(":"c)(0))
            MainConfig.IniWriteValue("PS5 Tools", "Port", NewPS5Menu.SharedConsoleAddress.Split(":"c)(1))
        Catch ex As FileNotFoundException
            MsgBox("Could not find a valid config file.", MsgBoxStyle.Exclamation)
        End Try
    End Sub

    Private Async Sub ContentWebView_NavigationCompleted(sender As Object, e As CoreWebView2NavigationCompletedEventArgs) Handles ContentWebView.NavigationCompleted

        Dim GameCoverSource As String = String.Empty

        If e.IsSuccess And ContentWebView.Source.ToString.StartsWith("https://prosperopatches.com/") Then
            'Game ID
            Dim GameID As String = Await ContentWebView.ExecuteScriptAsync("document.getElementsByClassName('bd-links-group py-2')[0].innerText;")

            'Game Image
            Dim GameImageURL As String = Await ContentWebView.ExecuteScriptAsync("document.getElementsByClassName('game-icon secondary')[0].outerHTML;")
            Dim SplittedGameImageURL As String() = GameImageURL.Split(New String() {"(", ")"}, StringSplitOptions.None)

            If SplittedGameImageURL.Count > 0 And Not String.IsNullOrEmpty(GameID) Then
                GameID = GameID.Split(New String() {"\n"}, StringSplitOptions.None)(1).Replace("""", "")
                GameCoverSource = SplittedGameImageURL(1)
            End If

            If Not String.IsNullOrEmpty(GameCoverSource) And Not String.IsNullOrEmpty(GameID) Then
                For Each ItemInListView In GamesListView.Items
                    Dim FoundGame As PS5Game = CType(ItemInListView, PS5Game)

                    If FoundGame.GameID.Contains(GameID) Or FoundGame.GameID = GameID Then
                        FoundGame.GameCoverSource = New BitmapImage(New Uri(GameCoverSource))
                        Exit For
                    End If
                Next

                For Each GamePatch In PatchesListView.Items
                    Dim FoundGamePatch As PS5Game = CType(GamePatch, PS5Game)

                    If FoundGamePatch.GameID.Contains(GameID) Or FoundGamePatch.GameID = GameID Then
                        FoundGamePatch.GameCoverSource = New BitmapImage(New Uri(GameCoverSource))
                        Exit For
                    End If
                Next
            End If

            If CurrentURL < URLs.Count Then
                ContentWebView.CoreWebView2.Navigate(URLs.Item(CurrentURL))
                CurrentURL += 1
                NewLoadingWindow.LoadProgressBar.Value = CurrentURL
            Else
                CurrentURL = 0
                URLs.Clear()
                NewLoadingWindow.Close()
                GamesListView.Items.Refresh()
                PatchesListView.Items.Refresh()
            End If
        End If

    End Sub

    Public Function GetDirSize(RootFolder As String) As Long
        Dim FolderInfo = New DirectoryInfo(RootFolder)
        For Each File In FolderInfo.GetFiles
            If Not File.Name.EndsWith("-merged.pkg") Then 'skip merged .pkg
                TotalSize += File.Length
            End If
        Next
        For Each SubFolderInfo In FolderInfo.GetDirectories : GetDirSize(SubFolderInfo.FullName)
        Next
        Return TotalSize
    End Function

    Private Sub PS5Library_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        'Load config if exists
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\psmt-config.ini") Then
            Try
                Dim MainConfig As New IniFile(My.Computer.FileSystem.CurrentDirectory + "\psmt-config.ini")
                ConsoleIP = MainConfig.IniReadValue("PS5 Tools", "IP")
                ConsolePort = MainConfig.IniReadValue("PS5 Tools", "Port")
            Catch ex As FileNotFoundException
                MsgBox("Could not find a valid config file.", MsgBoxStyle.Exclamation)
            End Try
        End If
    End Sub

End Class
