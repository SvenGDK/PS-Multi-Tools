Imports System.IO
Imports System.Net
Imports System.Security.Authentication
Imports System.Text
Imports System.Windows.Forms
Imports System.Windows.Media.Animation
Imports FluentFTP
Imports Microsoft.Web.WebView2.Core
Imports Newtonsoft.Json
Imports PS_Multi_Tools.INI
Imports PS_Multi_Tools.PS5ParamClass

Public Class PS5Library

    Dim MainConfig As New IniFile(Environment.CurrentDirectory + "\psmt-config.ini")
    Dim WithEvents NewLoadingWindow As New SyncWindow() With {.Title = "Loading PS5 files", .ShowActivated = True}

    Dim SelectedPath As String = ""
    Public CurrentPath As String = ""

    Dim ConsoleIP As String = ""
    Dim ConsoleFTPPort As String = ""
    Dim PayloadPort As String = ""

    Dim PKGCount As Integer = 0
    Dim FilesCount As Integer = 0
    Dim CurrentFileCount As Integer = 0
    Dim URLs As New List(Of String)()
    Dim CurrentURL As Integer = 0
    Dim TotalSize As Long = 0

    Dim IsSoundPlaying As Boolean = False

#Region "Supplemental Library Menu Items"
    Dim WithEvents OpenLocalBackupFolderMenuItem As New Controls.MenuItem() With {.Header = "Load a local backup folder"}
    Dim WithEvents LoadFTPFolderMenuItem As New Controls.MenuItem() With {.Header = "Load installed games & apps over FTP"}
    Dim WithEvents LoadPatchPKGFolderMenuItem As New Controls.MenuItem() With {.Header = "Load a local folder with Patch PKGs"}
    Dim WithEvents OpenDownloadsFolderMenuItem As New Controls.MenuItem() With {.Header = "Open the Downloads folder"}

    Dim WithEvents NewSettingsMenuItem As New Controls.MenuItem() With {.Header = "More Settings"}
#End Region

#Region "Game Context Menu Items"
    'Local context menu options
    Dim WithEvents GamesContextMenu As New Controls.ContextMenu()
    Dim WithEvents GameCopyToMenuItem As New Controls.MenuItem() With {.Header = "Copy game to", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/copy-icon.png", UriKind.Relative))}}
    Dim WithEvents GameOpenLocationMenuItem As New Controls.MenuItem() With {.Header = "Open game folder", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/OpenFolder-icon.png", UriKind.Relative))}}
    Dim WithEvents GamePlayMenuItem As New Controls.MenuItem() With {.Header = "Play Soundtrack", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Play-icon.png", UriKind.Relative))}}
    Dim WithEvents GameCheckForUpdatesMenuItem As New Controls.MenuItem() With {.Header = "Check for updates", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Refresh-icon.png", UriKind.Relative))}}
    Dim WithEvents GameBrowseAssetsMenuItem As New Controls.MenuItem() With {.Header = "Browse assets", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/OpenFolder-icon.png", UriKind.Relative))}}
    Dim WithEvents GamePackAsPKG As New Controls.MenuItem() With {.Header = "Pack as PS5 PKG", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/PKG.png", UriKind.Relative))}}

    'Remote context menu options
    Dim WithEvents GameLaunchMenuItem As New Controls.MenuItem() With {.Header = "Launch on PS5", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Play-icon.png", UriKind.Relative))}}
    'Dim WithEvents GameMoveToInternalMenuItem As New Controls.MenuItem() With {.Header = "Move to internal storage of PS5", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/move.png", UriKind.Relative))}}
    'Dim WithEvents GameMoveToUSB0MenuItem As New Controls.MenuItem() With {.Header = "Move to first attached USB drive", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/move.png", UriKind.Relative))}}
    'Dim WithEvents GameMoveToUSB1MenuItem As New Controls.MenuItem() With {.Header = "Move to second attached USB drive", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/move.png", UriKind.Relative))}}

    Dim WithEvents GameChangeTypeMenuItem As New Controls.MenuItem() With {.Header = "Change game type", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/rename.png", UriKind.Relative))}}
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
#End Region

#Region "App Context Menu Items"
    Dim WithEvents AppsContextMenu As New Controls.ContextMenu()
    Dim WithEvents AppCopyToMenuItem As New Controls.MenuItem() With {.Header = "Copy app to", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/copy-icon.png", UriKind.Relative))}}
    Dim WithEvents AppOpenLocationMenuItem As New Controls.MenuItem() With {.Header = "Open app folder", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/OpenFolder-icon.png", UriKind.Relative))}}
    Dim WithEvents AppPlayMenuItem As New Controls.MenuItem() With {.Header = "Play Soundtrack", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Play-icon.png", UriKind.Relative))}}
    Dim WithEvents AppCheckForUpdatesMenuItem As New Controls.MenuItem() With {.Header = "Check for updates", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Refresh-icon.png", UriKind.Relative))}}
    Dim WithEvents AppPackAsPKG As New Controls.MenuItem() With {.Header = "Pack as PS5 PKG", .Visibility = Visibility.Collapsed, .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/PKG.png", UriKind.Relative))}}

    Dim WithEvents AppChangeTypeMenuItem As New Controls.MenuItem() With {.Header = "Change app type", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/rename.png", UriKind.Relative))}}
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
#End Region

    Private Sub PS5Library_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Add supplemental menu items that will be handled in PS Multi Tools
        Dim LibraryMenuItem As Controls.MenuItem = CType(NewPS5Menu.Items(0), Controls.MenuItem)
        Dim SettingsMenuItem As Controls.MenuItem = CType(NewPS5Menu.Items(6), Controls.MenuItem)
        LibraryMenuItem.Items.Add(OpenLocalBackupFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadPatchPKGFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadFTPFolderMenuItem)
        LibraryMenuItem.Items.Add(OpenDownloadsFolderMenuItem)

        SettingsMenuItem.Items.Add(NewSettingsMenuItem)

        GameChangeTypeMenuItem.Items.Add(GameChangeToGameMenuItem)
        GameChangeTypeMenuItem.Items.Add(GameChangeToNativeMediaMenuItem)
        GameChangeTypeMenuItem.Items.Add(GameChangeToRNPSMediaMenuItem)
        GameChangeTypeMenuItem.Items.Add(GameChangeToBuiltInMenuItem)
        GameChangeTypeMenuItem.Items.Add(GameChangeToBigDaemonMenuItem)
        GameChangeTypeMenuItem.Items.Add(GameChangeToShellUIMenuItem)
        GameChangeTypeMenuItem.Items.Add(GameChangeToDaemonMenuItem)
        GameChangeTypeMenuItem.Items.Add(GameChangeToShellAppMenuItem)

        'Add context menu for apps
        AppsContextMenu.Items.Add(AppOpenLocationMenuItem)
        AppsContextMenu.Items.Add(AppCopyToMenuItem)
        AppsContextMenu.Items.Add(AppPlayMenuItem)
        AppsContextMenu.Items.Add(AppCheckForUpdatesMenuItem)
        AppsContextMenu.Items.Add(AppPackAsPKG)
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
        NewGamesListView.ContextMenu = GamesContextMenu
        AppsListView.ContextMenu = AppsContextMenu
    End Sub

    Private Sub PS5Library_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        'Load config
        Try
            ConsoleIP = MainConfig.IniReadValue("PS5 Tools", "IP")
            ConsoleFTPPort = MainConfig.IniReadValue("PS5 Tools", "FTPPort")
            PayloadPort = MainConfig.IniReadValue("PS5 Tools", "PayloadPort")
        Catch ex As Exception
            MsgBox("Could not find a valid config file.", MsgBoxStyle.Exclamation)
        End Try
    End Sub

#Region "Menu TextBox Changed Events"

    Private Sub NewPS5Menu_IPChanged(sender As Object, e As RoutedEventArgs) Handles NewPS5Menu.IPTextChanged
        ConsoleIP = NewPS5Menu.SharedIPAddress

        'Save config
        Try
            MainConfig.IniWriteValue("PS5 Tools", "IP", NewPS5Menu.SharedIPAddress)
        Catch ex As Exception
            MsgBox("Could not find a valid config file.", MsgBoxStyle.Exclamation)
        End Try
    End Sub

    Private Sub NewPS5Menu_FTPPortChanged(sender As Object, e As RoutedEventArgs) Handles NewPS5Menu.FTPPortChanged
        ConsoleFTPPort = NewPS5Menu.SharedFTPPort

        'Save config
        Try
            MainConfig.IniWriteValue("PS5 Tools", "FTPPort", NewPS5Menu.SharedFTPPort)
        Catch ex As Exception
            MsgBox("Could not find a valid config file.", MsgBoxStyle.Exclamation)
        End Try
    End Sub

    Private Sub NewPS5Menu_PayloadPortChanged(sender As Object, e As RoutedEventArgs) Handles NewPS5Menu.PayloadPortChanged
        PayloadPort = NewPS5Menu.SharedPayloadPort

        'Save config
        Try
            MainConfig.IniWriteValue("PS5 Tools", "PayloadPort", PayloadPort)
        Catch ex As Exception
            MsgBox("Could not find a valid config file.", MsgBoxStyle.Exclamation)
        End Try
    End Sub

#End Region

#Region "Library Menu Actions"

    Private Sub OpenLocalBackupFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenLocalBackupFolderMenuItem.Click
        If NewGamesListView.Items.Count > 0 Then
            Dim NewDialog As New CustomDialog()
            NewDialog.ButtonsGrid.Visibility = Visibility.Visible
            NewDialog.Title = "PS5 Library"
            NewDialog.ButtonsTitleTextBlock.Text = "A folder is already loaded. Please select an action :"

            If NewDialog.ShowDialog() = True Then
                Select Case NewDialog.CustomDialogResultValue
                    Case CustomDialog.CustomDialogResult.LoadNew
                        NewGamesListView.Items.Clear()
                        ShowBackupFolderBrowser()
                    Case CustomDialog.CustomDialogResult.Append
                        ShowBackupFolderBrowser()
                    Case CustomDialog.CustomDialogResult.Cancel
                        MsgBox("Aborted", MsgBoxStyle.Information)
                End Select
            End If
        Else
            ShowBackupFolderBrowser()
        End If
    End Sub

    Private Sub ShowBackupFolderBrowser()
        Dim FBD As New FolderBrowserDialog() With {.Description = "Select your PS5 backups folder"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPath = FBD.SelectedPath

            Dim LoadIcons As Boolean = True
            Dim LoadBackgrounds As Boolean = True
            Dim SkipFileChecks As Boolean = False

            'Load library config before processing
            If File.Exists(Environment.CurrentDirectory + "\psmt-config.ini") Then
                If Not String.IsNullOrEmpty(MainConfig.IniReadValue("PS5 Library", "LoadIcons")) Then
                    If MainConfig.IniReadValue("PS5 Library", "LoadIcons") = "False" Then
                        LoadIcons = False
                    End If
                End If
                If Not String.IsNullOrEmpty(MainConfig.IniReadValue("PS5 Library", "LoadBackgrounds")) Then
                    If MainConfig.IniReadValue("PS5 Library", "LoadBackgrounds") = "False" Then
                        LoadBackgrounds = False
                    End If
                End If
                If Not String.IsNullOrEmpty(MainConfig.IniReadValue("PS5 Library", "SkipFileChecks")) Then
                    If MainConfig.IniReadValue("PS5 Library", "SkipFileChecks") = "True" Then
                        SkipFileChecks = True
                    End If
                End If
            End If

            'Show loading window
            NewLoadingWindow = New SyncWindow() With {.Title = "PS5 Library Loader", .ShowActivated = True}
            NewLoadingWindow.Show()

            Dim NewGameLoaderArgs As New Structures.PS5GameLoaderArgs() With {.FolderPath = FBD.SelectedPath, .LoadIcons = LoadIcons, .LoadBackgrounds = LoadBackgrounds, .SkipFileChecks = SkipFileChecks}
            ProcessBackups(NewGameLoaderArgs)
        End If
    End Sub

    Private Async Sub ProcessBackups(WorkerArgs As Structures.PS5GameLoaderArgs)
        Await Task.Run(Sub()
                           'Search for files
                           Try
                               Dim ValidBackups As New List(Of String)()
                               Dim FoundDirectories As String() = Directory.GetDirectories(WorkerArgs.FolderPath)
                               For Each FoundDirectory In FoundDirectories
                                   Dim ParamFilePath As String = FoundDirectory + "\sce_sys\param.json"
                                   If File.Exists(ParamFilePath) Then
                                       ValidBackups.Add(ParamFilePath)
                                   End If
                               Next

                               Dispatcher.BeginInvoke(Sub()
                                                          NewLoadingWindow.LoadProgressBar.Maximum = ValidBackups.Count
                                                          NewLoadingWindow.LoadStatusTextBlock.Text = "Loading backup 1 of " + ValidBackups.Count.ToString()
                                                      End Sub)

                               For Each ValidBackup As String In ValidBackups
                                   TotalSize = 0

                                   Dim NewPS5Game As New PS5Game() With {.GameBackupType = "Folder", .GameLocation = PS5Game.Location.Local}
                                   Dim ParamData = JsonConvert.DeserializeObject(Of PS5Param)(File.ReadAllText(ValidBackup))
                                   Dim ParamFileInfo As New FileInfo(ValidBackup)

                                   If ParamData IsNot Nothing Then

                                       Dim MainGamePath As String = Directory.GetParent(ParamFileInfo.FullName).Parent.FullName
                                       Dim SCESYSFolder As String = Path.GetDirectoryName(ParamFileInfo.FullName)

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

                                       NewPS5Game.GameFileOrFolderPath = MainGamePath
                                       NewPS5Game.GameSize = "Size: " + FormatNumber(GetDirSize(MainGamePath) / 1073741824, 2) + " GB" 'Will only display correct if all files are present.

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
                                       If ParamData.MasterVersion IsNot Nothing Then
                                           NewPS5Game.GameMasterVersion = "Master Version: " + ParamData.MasterVersion
                                       End If
                                       If ParamData.SdkVersion IsNot Nothing Then
                                           NewPS5Game.GameSDKVersion = "SDK Version: " + ParamData.SdkVersion
                                       End If
                                       If ParamData.Pubtools.ToolVersion IsNot Nothing Then
                                           NewPS5Game.GamePubToolVersion = "PubTools Version: " + ParamData.Pubtools.ToolVersion
                                       End If
                                       If ParamData.VersionFileUri IsNot Nothing Then
                                           NewPS5Game.GameVersionFileURI = ParamData.VersionFileUri
                                       End If

                                       'Check for game icon
                                       If WorkerArgs.LoadIcons Then
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
                                                   If Utils.IsURLValid("https://prosperopatches.com/" + ParamData.TitleId.Trim()).Result = True Then
                                                       URLs.Add("https://prosperopatches.com/" + ParamData.TitleId.Trim()) 'Get the image from prosperopatches
                                                   End If
                                               End If
                                           End If
                                       End If

                                       'Check for game background
                                       If WorkerArgs.LoadBackgrounds Then
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
                                       End If

                                       'Check for game soundtrack
                                       If File.Exists(SCESYSFolder + "\snd0.at9") Then
                                           NewPS5Game.GameSoundFile = SCESYSFolder + "\snd0.at9"
                                       End If

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
                                       If WorkerArgs.SkipFileChecks = False Then
                                           'Check if prx is encrypted
                                           If File.Exists(MainGamePath + "\sce_module\libc.prx") Then
                                               Dim FirstStr As String = ""
                                               Using PRXReader As New FileStream(MainGamePath + "\sce_module\libc.prx", FileMode.Open, FileAccess.Read)
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
                                               Using EBOOTReader As New FileStream(MainGamePath + "\eboot.bin", FileMode.Open, FileAccess.Read)
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
                                       Else
                                           ToolTipString = "File checks skipped"
                                       End If

                                       NewPS5Game.DecFilesIncluded = ToolTipString

                                       'Update progress
                                       Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                                       Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading backup " + NewLoadingWindow.LoadProgressBar.Value.ToString() + " of " + ValidBackups.Count.ToString())

                                       'Add to the ListView
                                       If ParamData.ApplicationCategoryType = 0 And ParamData.TitleId.StartsWith("PP") Then 'Games
                                           Dispatcher.BeginInvoke(Sub() NewGamesListView.Items.Add(NewPS5Game))
                                       ElseIf ParamData.ApplicationCategoryType = 65536 And ParamData.TitleId.StartsWith("PP") Then 'Media apps
                                           Dispatcher.BeginInvoke(Sub() AppsListView.Items.Add(NewPS5Game))
                                       Else
                                           Dispatcher.BeginInvoke(Sub() AppsListView.Items.Add(NewPS5Game))
                                       End If
                                   End If
                               Next

                               If URLs.Count > 0 Then
                                   Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Getting " + URLs.Count.ToString() + " available covers")
                                   Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value = 0)
                                   Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Maximum = URLs.Count)

                                   ContentWebView.Source = New Uri(URLs.Item(0))
                               End If
                           Catch ex As Exception
                               MsgBox("Error accessing files. Please retry while running as Administrator.", MsgBoxStyle.Critical, "Error")
                           End Try
                       End Sub)

        NewLoadingWindow.Close()
    End Sub

    Private Async Sub LoadFTPFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadFTPFolderMenuItem.Click
        If Not String.IsNullOrEmpty(ConsoleIP) Then
            Await Task.Run(Sub()

                               Dispatcher.BeginInvoke(Sub()
                                                          'Clear
                                                          NewGamesListView.Items.Clear()
                                                          AppsListView.Items.Clear()

                                                          'Show the loading progress window
                                                          NewLoadingWindow = New SyncWindow() With {.Title = "Loading PS5 files", .ShowActivated = True}
                                                          NewLoadingWindow.LoadProgressBar.IsIndeterminate = True
                                                          NewLoadingWindow.LoadStatusTextBlock.Text = "Loading files, please wait ..."
                                                          NewLoadingWindow.Show()
                                                      End Sub)
                               Try
                                   'Get installed games and apps over FTP
                                   Dim CPort As Integer = Integer.Parse(ConsoleFTPPort)
                                   Using conn As New FtpClient(ConsoleIP, "anonymous", "anonymous", CPort)
                                       'Configurate the FTP connection
                                       conn.Config.EncryptionMode = FtpEncryptionMode.None
                                       conn.Config.SslProtocols = SslProtocols.None
                                       conn.Config.DataConnectionEncryption = False

                                       'Connect
                                       conn.Connect()

                                       'List backups on /data/homebrew
                                       For Each item In conn.GetListing("/data/homebrew")
                                           If item.Type = FtpObjectType.Directory Then
                                               Dim PS5GameLVItem As New PS5Game() With {.GameBackupType = "FTP", .GameLocation = PS5Game.Location.Remote, .GameRootLocation = PS5Game.RootLocation.Internal}

                                               'Check for icon0.png
                                               If conn.GetObjectInfo(item.FullName + "/sce_sys/icon0.png") IsNot Nothing Then
                                                   Dim Icon0Bytes As Byte() = Nothing
                                                   If conn.DownloadBytes(Icon0Bytes, item.FullName + "/sce_sys/icon0.png") Then
                                                       PS5GameLVItem.GameCoverSource = Utils.BitmapSourceFromByteArray(Icon0Bytes)
                                                   End If
                                               End If

                                               'Check for icon0.png
                                               If conn.GetObjectInfo(item.FullName + "/sce_sys/pic0.png") IsNot Nothing Then
                                                   Dim Pic0Bytes As Byte() = Nothing
                                                   If conn.DownloadBytes(Pic0Bytes, item.FullName + "/sce_sys/pic0.png") Then
                                                       PS5GameLVItem.GameBGSource = Utils.BitmapSourceFromByteArray(Pic0Bytes)
                                                   End If
                                               End If

                                               'Check for param.json
                                               If conn.GetObjectInfo(item.FullName + "/sce_sys/param.json") IsNot Nothing Then
                                                   Dim ParamBytes As Byte() = Nothing
                                                   If conn.DownloadBytes(ParamBytes, item.FullName + "/sce_sys/param.json") Then
                                                       Dim ParamBytesAsString = Encoding.UTF8.GetString(ParamBytes)
                                                       Dim BackupInfos As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(ParamBytesAsString)
                                                       If BackupInfos IsNot Nothing Then
                                                           PS5GameLVItem.GameFileOrFolderPath = item.FullName
                                                           PS5GameLVItem.GameID = "Title ID: " + BackupInfos.TitleId
                                                           PS5GameLVItem.GameTitle = BackupInfos.LocalizedParameters.EnUS.TitleName
                                                           PS5GameLVItem.GameContentID = "Content ID: " + BackupInfos.ContentId
                                                           PS5GameLVItem.GameRegion = "Region: " + PS4Game.GetGameRegion(BackupInfos.ContentId)

                                                           If BackupInfos.LocalizedParameters.EnUS IsNot Nothing Then
                                                               PS5GameLVItem.GameTitle = BackupInfos.LocalizedParameters.EnUS.TitleName
                                                           End If
                                                           If BackupInfos.LocalizedParameters.DeDE IsNot Nothing Then
                                                               PS5GameLVItem.DEGameTitle = BackupInfos.LocalizedParameters.DeDE.TitleName
                                                           End If
                                                           If BackupInfos.LocalizedParameters.FrFR IsNot Nothing Then
                                                               PS5GameLVItem.FRGameTitle = BackupInfos.LocalizedParameters.FrFR.TitleName
                                                           End If
                                                           If BackupInfos.LocalizedParameters.ItIT IsNot Nothing Then
                                                               PS5GameLVItem.ITGameTitle = BackupInfos.LocalizedParameters.ItIT.TitleName
                                                           End If
                                                           If BackupInfos.LocalizedParameters.EsES IsNot Nothing Then
                                                               PS5GameLVItem.ESGameTitle = BackupInfos.LocalizedParameters.EsES.TitleName
                                                           End If
                                                           If BackupInfos.LocalizedParameters.JaJP IsNot Nothing Then
                                                               PS5GameLVItem.JPGameTitle = BackupInfos.LocalizedParameters.JaJP.TitleName
                                                           End If

                                                           If BackupInfos.ApplicationCategoryType = 0 Then
                                                               PS5GameLVItem.GameCategory = "Type: Game"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 65536 Then
                                                               PS5GameLVItem.GameCategory = "Type: Native Media App"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 65792 Then
                                                               PS5GameLVItem.GameCategory = "Type: RNPS Media App"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 131328 Then
                                                               PS5GameLVItem.GameCategory = "Type: System Built-in App"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 131584 Then
                                                               PS5GameLVItem.GameCategory = "Type: Big Daemon"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 16777216 Then
                                                               PS5GameLVItem.GameCategory = "Type: ShellUI"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 33554432 Then
                                                               PS5GameLVItem.GameCategory = "Type: Daemon"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 67108864 Then
                                                               PS5GameLVItem.GameCategory = "Type: ShellApp"
                                                           Else
                                                               PS5GameLVItem.GameCategory = "Type: Unknown"
                                                           End If

                                                           Dim BackupSize As Long = conn.GetObjectInfo(item.FullName).Size
                                                           PS5GameLVItem.GameSize = "Size: " + FormatNumber(BackupSize / 1073741824, 2) + " GB"

                                                           If BackupInfos.ContentVersion IsNot Nothing Then
                                                               PS5GameLVItem.GameVersion = "Version: " + BackupInfos.ContentVersion
                                                           End If
                                                           If BackupInfos.RequiredSystemSoftwareVersion IsNot Nothing Then
                                                               PS5GameLVItem.GameRequiredFirmware = "Required Firmware: " + BackupInfos.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, "."c).Insert(5, "."c).Insert(8, "."c).Remove(11, 8)
                                                           End If
                                                           If BackupInfos.MasterVersion IsNot Nothing Then
                                                               PS5GameLVItem.GameMasterVersion = "Master Version: " + BackupInfos.MasterVersion
                                                           End If
                                                           If BackupInfos.SdkVersion IsNot Nothing Then
                                                               PS5GameLVItem.GameSDKVersion = "SDK Version: " + BackupInfos.SdkVersion
                                                           End If
                                                           If BackupInfos.Pubtools.ToolVersion IsNot Nothing Then
                                                               PS5GameLVItem.GamePubToolVersion = "PubTools Version: " + BackupInfos.Pubtools.ToolVersion
                                                           End If
                                                           If BackupInfos.VersionFileUri IsNot Nothing Then
                                                               PS5GameLVItem.GameVersionFileURI = BackupInfos.VersionFileUri
                                                           End If
                                                       Else
                                                           Continue For
                                                       End If
                                                   Else
                                                       Continue For
                                                   End If
                                               Else
                                                   Continue For
                                               End If

                                               'Add to the NewGamesListView
                                               Dispatcher.BeginInvoke(Sub() NewGamesListView.Items.Add(PS5GameLVItem))
                                           End If
                                       Next

                                       'List backups on connected USB0
                                       For Each item In conn.GetListing("/mnt/usb0/homebrew")
                                           If item.Type = FtpObjectType.Directory Then
                                               Dim PS5GameLVItem As New PS5Game() With {.GameBackupType = "FTP", .GameLocation = PS5Game.Location.Remote, .GameRootLocation = PS5Game.RootLocation.USB}

                                               'Check for icon0.png
                                               If conn.GetObjectInfo(item.FullName + "/sce_sys/icon0.png") IsNot Nothing Then
                                                   Dim Icon0Bytes As Byte() = Nothing
                                                   If conn.DownloadBytes(Icon0Bytes, item.FullName + "/sce_sys/icon0.png") Then
                                                       PS5GameLVItem.GameCoverSource = Utils.BitmapSourceFromByteArray(Icon0Bytes)
                                                   End If
                                               End If

                                               'Check for icon0.png
                                               If conn.GetObjectInfo(item.FullName + "/sce_sys/pic0.png") IsNot Nothing Then
                                                   Dim Pic0Bytes As Byte() = Nothing
                                                   If conn.DownloadBytes(Pic0Bytes, item.FullName + "/sce_sys/pic0.png") Then
                                                       PS5GameLVItem.GameBGSource = Utils.BitmapSourceFromByteArray(Pic0Bytes)
                                                   End If
                                               End If

                                               'Check for param.json
                                               If conn.GetObjectInfo(item.FullName + "/sce_sys/param.json") IsNot Nothing Then
                                                   Dim ParamBytes As Byte() = Nothing
                                                   If conn.DownloadBytes(ParamBytes, item.FullName + "/sce_sys/param.json") Then
                                                       Dim ParamBytesAsString = Encoding.UTF8.GetString(ParamBytes)
                                                       Dim BackupInfos As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(ParamBytesAsString)
                                                       If BackupInfos IsNot Nothing Then
                                                           PS5GameLVItem.GameFileOrFolderPath = item.FullName
                                                           PS5GameLVItem.GameID = "Title ID: " + BackupInfos.TitleId
                                                           PS5GameLVItem.GameTitle = BackupInfos.LocalizedParameters.EnUS.TitleName
                                                           PS5GameLVItem.GameContentID = "Content ID: " + BackupInfos.ContentId
                                                           PS5GameLVItem.GameRegion = "Region: " + PS4Game.GetGameRegion(BackupInfos.ContentId)

                                                           If BackupInfos.LocalizedParameters.EnUS IsNot Nothing Then
                                                               PS5GameLVItem.GameTitle = BackupInfos.LocalizedParameters.EnUS.TitleName
                                                           End If
                                                           If BackupInfos.LocalizedParameters.DeDE IsNot Nothing Then
                                                               PS5GameLVItem.DEGameTitle = BackupInfos.LocalizedParameters.DeDE.TitleName
                                                           End If
                                                           If BackupInfos.LocalizedParameters.FrFR IsNot Nothing Then
                                                               PS5GameLVItem.FRGameTitle = BackupInfos.LocalizedParameters.FrFR.TitleName
                                                           End If
                                                           If BackupInfos.LocalizedParameters.ItIT IsNot Nothing Then
                                                               PS5GameLVItem.ITGameTitle = BackupInfos.LocalizedParameters.ItIT.TitleName
                                                           End If
                                                           If BackupInfos.LocalizedParameters.EsES IsNot Nothing Then
                                                               PS5GameLVItem.ESGameTitle = BackupInfos.LocalizedParameters.EsES.TitleName
                                                           End If
                                                           If BackupInfos.LocalizedParameters.JaJP IsNot Nothing Then
                                                               PS5GameLVItem.JPGameTitle = BackupInfos.LocalizedParameters.JaJP.TitleName
                                                           End If

                                                           If BackupInfos.ApplicationCategoryType = 0 Then
                                                               PS5GameLVItem.GameCategory = "Type: Game"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 65536 Then
                                                               PS5GameLVItem.GameCategory = "Type: Native Media App"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 65792 Then
                                                               PS5GameLVItem.GameCategory = "Type: RNPS Media App"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 131328 Then
                                                               PS5GameLVItem.GameCategory = "Type: System Built-in App"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 131584 Then
                                                               PS5GameLVItem.GameCategory = "Type: Big Daemon"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 16777216 Then
                                                               PS5GameLVItem.GameCategory = "Type: ShellUI"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 33554432 Then
                                                               PS5GameLVItem.GameCategory = "Type: Daemon"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 67108864 Then
                                                               PS5GameLVItem.GameCategory = "Type: ShellApp"
                                                           Else
                                                               PS5GameLVItem.GameCategory = "Type: Unknown"
                                                           End If

                                                           Dim BackupSize As Long = conn.GetObjectInfo(item.FullName).Size
                                                           PS5GameLVItem.GameSize = "Size: " + FormatNumber(BackupSize / 1073741824, 2) + " GB"

                                                           If BackupInfos.ContentVersion IsNot Nothing Then
                                                               PS5GameLVItem.GameVersion = "Version: " + BackupInfos.ContentVersion
                                                           End If
                                                           If BackupInfos.RequiredSystemSoftwareVersion IsNot Nothing Then
                                                               PS5GameLVItem.GameRequiredFirmware = "Required Firmware: " + BackupInfos.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, "."c).Insert(5, "."c).Insert(8, "."c).Remove(11, 8)
                                                           End If
                                                           If BackupInfos.MasterVersion IsNot Nothing Then
                                                               PS5GameLVItem.GameMasterVersion = "Master Version: " + BackupInfos.MasterVersion
                                                           End If
                                                           If BackupInfos.SdkVersion IsNot Nothing Then
                                                               PS5GameLVItem.GameSDKVersion = "SDK Version: " + BackupInfos.SdkVersion
                                                           End If
                                                           If BackupInfos.Pubtools.ToolVersion IsNot Nothing Then
                                                               PS5GameLVItem.GamePubToolVersion = "PubTools Version: " + BackupInfos.Pubtools.ToolVersion
                                                           End If
                                                           If BackupInfos.VersionFileUri IsNot Nothing Then
                                                               PS5GameLVItem.GameVersionFileURI = BackupInfos.VersionFileUri
                                                           End If
                                                       Else
                                                           Continue For
                                                       End If
                                                   Else
                                                       Continue For
                                                   End If
                                               Else
                                                   Continue For
                                               End If

                                               'Add to the NewGamesListView
                                               Dispatcher.BeginInvoke(Sub() NewGamesListView.Items.Add(PS5GameLVItem))
                                           End If
                                       Next
                                       'List backups on connected USB1
                                       For Each item In conn.GetListing("/mnt/usb1/homebrew")
                                           If item.Type = FtpObjectType.Directory Then
                                               Dim PS5GameLVItem As New PS5Game() With {.GameBackupType = "FTP", .GameLocation = PS5Game.Location.Remote, .GameRootLocation = PS5Game.RootLocation.USB}

                                               'Check for icon0.png
                                               If conn.GetObjectInfo(item.FullName + "/sce_sys/icon0.png") IsNot Nothing Then
                                                   Dim Icon0Bytes As Byte() = Nothing
                                                   If conn.DownloadBytes(Icon0Bytes, item.FullName + "/sce_sys/icon0.png") Then
                                                       PS5GameLVItem.GameCoverSource = Utils.BitmapSourceFromByteArray(Icon0Bytes)
                                                   End If
                                               End If

                                               'Check for icon0.png
                                               If conn.GetObjectInfo(item.FullName + "/sce_sys/pic0.png") IsNot Nothing Then
                                                   Dim Pic0Bytes As Byte() = Nothing
                                                   If conn.DownloadBytes(Pic0Bytes, item.FullName + "/sce_sys/pic0.png") Then
                                                       PS5GameLVItem.GameBGSource = Utils.BitmapSourceFromByteArray(Pic0Bytes)
                                                   End If
                                               End If

                                               'Check for param.json
                                               If conn.GetObjectInfo(item.FullName + "/sce_sys/param.json") IsNot Nothing Then
                                                   Dim ParamBytes As Byte() = Nothing
                                                   If conn.DownloadBytes(ParamBytes, item.FullName + "/sce_sys/param.json") Then
                                                       Dim ParamBytesAsString = Encoding.UTF8.GetString(ParamBytes)
                                                       Dim BackupInfos As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(ParamBytesAsString)
                                                       If BackupInfos IsNot Nothing Then
                                                           PS5GameLVItem.GameFileOrFolderPath = item.FullName
                                                           PS5GameLVItem.GameID = "Title ID: " + BackupInfos.TitleId
                                                           PS5GameLVItem.GameTitle = BackupInfos.LocalizedParameters.EnUS.TitleName
                                                           PS5GameLVItem.GameContentID = "Content ID: " + BackupInfos.ContentId
                                                           PS5GameLVItem.GameRegion = "Region: " + PS4Game.GetGameRegion(BackupInfos.ContentId)

                                                           If BackupInfos.LocalizedParameters.EnUS IsNot Nothing Then
                                                               PS5GameLVItem.GameTitle = BackupInfos.LocalizedParameters.EnUS.TitleName
                                                           End If
                                                           If BackupInfos.LocalizedParameters.DeDE IsNot Nothing Then
                                                               PS5GameLVItem.DEGameTitle = BackupInfos.LocalizedParameters.DeDE.TitleName
                                                           End If
                                                           If BackupInfos.LocalizedParameters.FrFR IsNot Nothing Then
                                                               PS5GameLVItem.FRGameTitle = BackupInfos.LocalizedParameters.FrFR.TitleName
                                                           End If
                                                           If BackupInfos.LocalizedParameters.ItIT IsNot Nothing Then
                                                               PS5GameLVItem.ITGameTitle = BackupInfos.LocalizedParameters.ItIT.TitleName
                                                           End If
                                                           If BackupInfos.LocalizedParameters.EsES IsNot Nothing Then
                                                               PS5GameLVItem.ESGameTitle = BackupInfos.LocalizedParameters.EsES.TitleName
                                                           End If
                                                           If BackupInfos.LocalizedParameters.JaJP IsNot Nothing Then
                                                               PS5GameLVItem.JPGameTitle = BackupInfos.LocalizedParameters.JaJP.TitleName
                                                           End If

                                                           If BackupInfos.ApplicationCategoryType = 0 Then
                                                               PS5GameLVItem.GameCategory = "Type: Game"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 65536 Then
                                                               PS5GameLVItem.GameCategory = "Type: Native Media App"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 65792 Then
                                                               PS5GameLVItem.GameCategory = "Type: RNPS Media App"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 131328 Then
                                                               PS5GameLVItem.GameCategory = "Type: System Built-in App"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 131584 Then
                                                               PS5GameLVItem.GameCategory = "Type: Big Daemon"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 16777216 Then
                                                               PS5GameLVItem.GameCategory = "Type: ShellUI"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 33554432 Then
                                                               PS5GameLVItem.GameCategory = "Type: Daemon"
                                                           ElseIf BackupInfos.ApplicationCategoryType = 67108864 Then
                                                               PS5GameLVItem.GameCategory = "Type: ShellApp"
                                                           Else
                                                               PS5GameLVItem.GameCategory = "Type: Unknown"
                                                           End If

                                                           Dim BackupSize As Long = conn.GetObjectInfo(item.FullName).Size
                                                           PS5GameLVItem.GameSize = "Size: " + FormatNumber(BackupSize / 1073741824, 2) + " GB"

                                                           If BackupInfos.ContentVersion IsNot Nothing Then
                                                               PS5GameLVItem.GameVersion = "Version: " + BackupInfos.ContentVersion
                                                           End If
                                                           If BackupInfos.RequiredSystemSoftwareVersion IsNot Nothing Then
                                                               PS5GameLVItem.GameRequiredFirmware = "Required Firmware: " + BackupInfos.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, "."c).Insert(5, "."c).Insert(8, "."c).Remove(11, 8)
                                                           End If
                                                           If BackupInfos.MasterVersion IsNot Nothing Then
                                                               PS5GameLVItem.GameMasterVersion = "Master Version: " + BackupInfos.MasterVersion
                                                           End If
                                                           If BackupInfos.SdkVersion IsNot Nothing Then
                                                               PS5GameLVItem.GameSDKVersion = "SDK Version: " + BackupInfos.SdkVersion
                                                           End If
                                                           If BackupInfos.Pubtools.ToolVersion IsNot Nothing Then
                                                               PS5GameLVItem.GamePubToolVersion = "PubTools Version: " + BackupInfos.Pubtools.ToolVersion
                                                           End If
                                                           If BackupInfos.VersionFileUri IsNot Nothing Then
                                                               PS5GameLVItem.GameVersionFileURI = BackupInfos.VersionFileUri
                                                           End If
                                                       Else
                                                           Continue For
                                                       End If
                                                   Else
                                                       Continue For
                                                   End If
                                               Else
                                                   Continue For
                                               End If

                                               'Add to the NewGamesListView
                                               Dispatcher.BeginInvoke(Sub() NewGamesListView.Items.Add(PS5GameLVItem))
                                           End If
                                       Next

                                       'Disconnect
                                       conn.Disconnect()
                                   End Using
                               Catch ex As Exception
                                   MsgBox(ex.ToString())
                               End Try
                           End Sub)

            NewLoadingWindow.Close()

            If URLs.Count > 0 Then
                NewLoadingWindow.LoadStatusTextBlock.Text = "Getting " + URLs.Count.ToString() + " available covers"
                NewLoadingWindow.LoadProgressBar.Value = 0
                NewLoadingWindow.LoadProgressBar.Maximum = URLs.Count

                ContentWebView.Source = New Uri(URLs.Item(0))
            Else
                NewLoadingWindow.Close()
            End If
        Else
            MsgBox("Please enter your console's FTP IP address in the settings before continuing.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Async Sub LoadPatchPKGFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadPatchPKGFolderMenuItem.Click
        Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Select your PS5 patches folder"}

        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            PatchesListView.Items.Clear()

            Dim FoundPKGs As String() = Directory.GetFiles(FBD.SelectedPath, "*_sc.pkg", SearchOption.AllDirectories)
            PKGCount = FoundPKGs.Length

            NewLoadingWindow = New SyncWindow() With {.Title = "Loading PS5 patches", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.Maximum = PKGCount
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + PKGCount.ToString()
            NewLoadingWindow.Show()

            Dim LoadIcons As Boolean = True
            Dim LoadBackgrounds As Boolean = True
            Dim SkipFileChecks As Boolean = False

            'Load library config
            If File.Exists(Environment.CurrentDirectory + "\psmt-config.ini") Then
                If Not String.IsNullOrEmpty(MainConfig.IniReadValue("PS5 Library", "LoadIcons")) Then
                    If MainConfig.IniReadValue("PS5 Library", "LoadIcons") = "False" Then
                        LoadIcons = False
                    End If
                End If
                If Not String.IsNullOrEmpty(MainConfig.IniReadValue("PS5 Library", "LoadBackgrounds")) Then
                    If MainConfig.IniReadValue("PS5 Library", "LoadBackgrounds") = "False" Then
                        LoadBackgrounds = False
                    End If
                End If
                If Not String.IsNullOrEmpty(MainConfig.IniReadValue("PS5 Library", "SkipFileChecks")) Then
                    If MainConfig.IniReadValue("PS5 Library", "SkipFileChecks") = "True" Then
                        SkipFileChecks = True
                    End If
                End If
            End If

            'PS5 Patch Source pkgs
            For Each PatchSCPKG In FoundPKGs

                Dim NewPS5Game As New PS5Game() With {.GameBackupType = "Patch"}
                Dim PKGFileInfo As New FileInfo(PatchSCPKG)

                TotalSize = 0

                Using PARAMReader As New Process()
                    PARAMReader.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\ps5_pkg.exe"
                    PARAMReader.StartInfo.Arguments = "--psmtparam file:""" + PatchSCPKG + """"
                    PARAMReader.StartInfo.RedirectStandardOutput = True
                    PARAMReader.StartInfo.UseShellExecute = False
                    PARAMReader.StartInfo.CreateNoWindow = True
                    PARAMReader.Start()

                    Dim OutputReader As StreamReader = PARAMReader.StandardOutput
                    Dim ProcessOutput As String = OutputReader.ReadToEnd()

                    If ProcessOutput.Length > 0 Then
                        Dim ParamData = JsonConvert.DeserializeObject(Of PS5Param)(ProcessOutput)

                        If ParamData IsNot Nothing Then
                            NewPS5Game.GameID = ParamData.TitleId
                            NewPS5Game.GameTitle = ParamData.LocalizedParameters.EnUS.TitleName
                            NewPS5Game.GameContentID = ParamData.ContentId
                            NewPS5Game.GameRegion = PS5Game.GetGameRegion(ParamData.TitleId)
                            NewPS5Game.GameCategory = "Game Patch"
                            NewPS5Game.GameSize = FormatNumber(GetDirSize(PKGFileInfo.DirectoryName) / 1073741824, 2) + " GB" 'Will only display correct if all PKG parts are present.
                            NewPS5Game.GameVersion = "Patch: " + ParamData.ContentVersion
                            NewPS5Game.GameRequiredFirmware = "Req.FW: " + ParamData.RequiredSystemSoftwareVersion.Replace("0x", "").Insert(2, "."c).Insert(5, "."c).Insert(8, "."c).Remove(11, 8)

                            If LoadIcons Then
                                If Await Utils.IsURLValid("https://prosperopatches.com/" + ParamData.TitleId.Trim()) Then
                                    URLs.Add("https://prosperopatches.com/" + ParamData.TitleId.Trim()) 'Get the image from prosperopatches
                                End If
                            End If

                        End If

                        Await Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                        Await Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading PKG " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + PKGCount.ToString())

                        'Add to the ListView
                        If PatchesListView.Dispatcher.CheckAccess() = False Then
                            Await PatchesListView.Dispatcher.BeginInvoke(Sub() PatchesListView.Items.Add(NewPS5Game))
                        Else
                            PatchesListView.Items.Add(NewPS5Game)
                        End If

                    End If

                End Using

            Next

            If URLs.Count > 0 Then
                NewLoadingWindow.LoadStatusTextBlock.Text = "Getting " + URLs.Count.ToString() + " available covers"
                NewLoadingWindow.LoadProgressBar.Value = 0
                NewLoadingWindow.LoadProgressBar.Maximum = URLs.Count

                ContentWebView.Source = New Uri(URLs.Item(0))
            Else
                NewLoadingWindow.Close()
            End If

        End If
    End Sub

    Private Sub OpenDownloadsFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenDownloadsFolderMenuItem.Click
        If Directory.Exists(Environment.CurrentDirectory + "\Downloads") Then
            Process.Start("explorer", Environment.CurrentDirectory + "\Downloads")
        End If
    End Sub

#End Region

#Region "Game Context Menu Actions"

#Region "Local Context Menu Options"

    Private Sub GameCopyToMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameCopyToMenuItem.Click
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
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
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            If Not String.IsNullOrEmpty(SelectedPS5Game.GameSoundFile) Then
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
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            If Not String.IsNullOrEmpty(SelectedPS5Game.GameID) Then
                Dim NewPS5GamePatches As New PS5GamePatches With {.ShowActivated = True, .SearchForGamePatchWithID = SelectedPS5Game.GameID.Split(New String() {"Title ID: "}, StringSplitOptions.None)(1)}
                NewPS5GamePatches.Show()
            End If
        End If
    End Sub

    Private Sub GameChangeToGameMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeToGameMenuItem.Click
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 0

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: Game"

                    MsgBox("Game type changed!", MsgBoxStyle.Information)
                    NewGamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameChangeToNativeMediaMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeToNativeMediaMenuItem.Click
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 65536

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: Media"

                    MsgBox("Game type changed!", MsgBoxStyle.Information)
                    NewGamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameChangeToRNPSMediaMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeToRNPSMediaMenuItem.Click
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 65792

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: Media"

                    MsgBox("Game type changed!", MsgBoxStyle.Information)
                    NewGamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameChangeToShellAppMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeToShellAppMenuItem.Click
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 67108864

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: ShellApp"

                    MsgBox("Game type changed!", MsgBoxStyle.Information)
                    NewGamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameChangeToShellUIMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeToShellUIMenuItem.Click
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 16777216

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: ShellUI"

                    MsgBox("Game type changed!", MsgBoxStyle.Information)
                    NewGamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameChangeToBigDaemonMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeToBigDaemonMenuItem.Click
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 131584

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: BigDaemon"

                    MsgBox("Game type changed!", MsgBoxStyle.Information)
                    NewGamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameChangeToBuiltInMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeToBuiltInMenuItem.Click
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 131328

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: System Built-In"

                    MsgBox("Game type changed!", MsgBoxStyle.Information)
                    NewGamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameChangeToDaemonMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeToDaemonMenuItem.Click
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            If File.Exists(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json") Then
                Dim JSONData As String = File.ReadAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json")
                Try
                    Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(JSONData)
                    ParamData.ApplicationCategoryType = 33554432

                    Dim RawDataJSON As String = JsonConvert.SerializeObject(ParamData, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\param.json", RawDataJSON)
                    SelectedPS5Game.GameCategory = "Type: Daemon"

                    MsgBox("Game type changed!", MsgBoxStyle.Information)
                    NewGamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameRenameMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameRenameMenuItem.Click
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
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
                    NewGamesListView.Items.Refresh()
                Catch ex As JsonSerializationException
                    MsgBox("Could not parse the selected param.json file.", MsgBoxStyle.Critical, "Error")
                End Try
            End If
        End If
    End Sub

    Private Sub GameChangeIconMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeIconMenuItem.Click
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            Dim OFD As New OpenFileDialog() With {.Title = "Select a new icon0.png image for this game", .Filter = "PNG images (*.png)|*.png", .Multiselect = False}

            If OFD.ShowDialog() = Forms.DialogResult.OK Then
                If File.Exists(OFD.FileName) AndAlso Not String.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath) Then
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

                    NewGamesListView.Items.Refresh()
                End If
            End If
        End If
    End Sub

    Private Sub GameChangeBackgroundMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeBackgroundMenuItem.Click
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            Dim OFD As New OpenFileDialog() With {.Title = "Select a new pic0.png image for this game", .Filter = "PNG images (*.png)|*.png", .Multiselect = False}

            If OFD.ShowDialog() = Forms.DialogResult.OK Then
                If File.Exists(OFD.FileName) AndAlso Not String.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath) Then
                    File.Copy(OFD.FileName, SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\pic0.png", True)

                    'Set new background
                    Dim TempBitmapImage = New BitmapImage()
                    TempBitmapImage.BeginInit()
                    TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                    TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                    TempBitmapImage.UriSource = New Uri(SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\icon0.png", UriKind.RelativeOrAbsolute)
                    TempBitmapImage.EndInit()
                    SelectedPS5Game.GameBGSource = TempBitmapImage

                    NewGamesListView.Items.Refresh()
                End If
            End If
        End If
    End Sub

    Private Sub GameChangeSoundtrackMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameChangeSoundtrackMenuItem.Click
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            Dim OFD As New OpenFileDialog() With {.Title = "Select a new snd0.at9 soundtrack for this game", .Filter = "AT9 Audio Files (*.at9)|*.at9", .Multiselect = False}

            If IsSoundPlaying Then
                MsgBox("A soundtrack is currently playing. Please stop it before changing any soundtrack.", MsgBoxStyle.Information)
            Else
                'Set new soundtrack
                If OFD.ShowDialog() = Forms.DialogResult.OK Then
                    If File.Exists(OFD.FileName) AndAlso Not String.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath) Then
                        File.Copy(OFD.FileName, SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\snd0.at9", True)
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub GameOpenLocationMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameOpenLocationMenuItem.Click
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            If Not String.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath) Then
                Process.Start("explorer", SelectedPS5Game.GameFileOrFolderPath)
            End If
        End If
    End Sub

    Private Sub GameBrowseAssetsMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameBrowseAssetsMenuItem.Click
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            If Not String.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath) Then
                Dim NewAssetBrowser As New PS5AssetsBrowser With {.SelectedDirectory = SelectedPS5Game.GameFileOrFolderPath}
                NewAssetBrowser.Show()
            End If
        End If
    End Sub

    Private Sub GamePackAsPKG_Click(sender As Object, e As RoutedEventArgs) Handles GamePackAsPKG.Click
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            If Not String.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath) Then

                'Create a GP5 project
                Dim AppProjectPath As String = SelectedPS5Game.GameFileOrFolderPath
                Dim NewGP5Project As XDocument = <?xml version="1.0" encoding="utf-8" standalone="no"?>
                                                 <psproject fmt="gp5" version="1000">
                                                     <volume>
                                                         <volume_type>prospero_app</volume_type>
                                                         <package passcode="00000000000000000000000000000000"/>
                                                         <chunk_info chunk_count="1" scenario_count="1">
                                                             <chunks>
                                                                 <chunk id="0" label="Chunk #0"/>
                                                             </chunks>
                                                             <scenarios default_id="0">
                                                                 <scenario id="0" initial_chunk_count="1" label="Scenario #0" type="playmode">0</scenario>
                                                             </scenarios>
                                                         </chunk_info>
                                                     </volume>
                                                     <global_exclude/>
                                                     <rootdir dir_exclude="about" file_exclude="*.esbak;keystone;*.dds;disc_info.dat;pfs-version.dat;ext_info.dat" src_path=<%= AppProjectPath %>/>
                                                 </psproject>

                'Save the GP5 project
                Dim GP5ProjectPath As String = ""
                Dim SFD As New SaveFileDialog() With {.Title = "Select a save path for the GP5 project", .DefaultExt = ".gp5", .AddExtension = True, .Filter = "GP5 Project (*.gp5)|*.gp5"}
                If SFD.ShowDialog() = Forms.DialogResult.OK Then
                    GP5ProjectPath = SFD.FileName
                    NewGP5Project.Save(SFD.FileName)
                End If

                'Show the PKG Builder
                Dim NewPKGBuilder As New PS5PKGBuilder() With {.ShowActivated = True}
                NewPKGBuilder.SelectedProjectTextBox.Text = GP5ProjectPath
                NewPKGBuilder.Show()

                MsgBox("A GP5 project for " + SelectedPS5Game.GameTitle + " - [" + SelectedPS5Game.GameID + "] has been created." + vbCrLf + "Select an output path for the final PKG file and build it.", MsgBoxStyle.Information, "PS5 Game Library")
            End If
        End If
    End Sub

#End Region

#Region "Remote Context Menu Options"

    Private Sub GameLaunchMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles GameLaunchMenuItem.Click
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            If SelectedPS5Game.GameLocation = PS5Game.Location.Remote Then
                Dim GameTitleID As String = SelectedPS5Game.GameID.Replace("Title ID: ", "").Trim
                Dim HomebrewArgs As String
                Dim HomebrewLoaderURL As String
                Dim NewMsgBoxResult As MsgBoxResult = MsgBox("Is dump_runner.elf located at /data/homebrew ? Click 'No' if located on the USB or 'Cancel' to abort.", MsgBoxStyle.YesNoCancel)

                If NewMsgBoxResult = MsgBoxResult.Yes Then
                    HomebrewArgs = "/data/homebrew/dump_runner.elf+" + GameTitleID
                    HomebrewLoaderURL = $"http://{ConsoleIP}:8080/hbldr?pipe=0&daemon=1&path=/data/homebrew/dump_runner.elf&args=" + HomebrewArgs + "&cwd=" + SelectedPS5Game.GameFileOrFolderPath
                ElseIf NewMsgBoxResult = MsgBoxResult.No Then
                    HomebrewArgs = "/mnt/usb0/homebrew/dump_runner.elf+" + GameTitleID
                    HomebrewLoaderURL = $"http://{ConsoleIP}:8080/hbldr?pipe=0&daemon=1&path=/mnt/usb0/homebrew/dump_runner.elf&args=" + HomebrewArgs + "&cwd=" + SelectedPS5Game.GameFileOrFolderPath
                Else
                    MsgBox("Cancelled", MsgBoxStyle.Information)
                    Exit Sub
                End If

                'Launch
                NewPS5Menu.NavigateTowebMANWebSrvUrl(HomebrewLoaderURL)
            End If
        End If
    End Sub

#End Region

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
            If Not String.IsNullOrEmpty(SelectedPS5Game.GameSoundFile) Then
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
            If Not String.IsNullOrEmpty(SelectedPS5Game.GameID) Then
                If Not SelectedPS5Game.GameID.StartsWith("NPXS") Then
                    Dim NewPS5GamePatches As New PS5GamePatches With {.ShowActivated = True, .SearchForGamePatchWithID = SelectedPS5Game.GameID.Split(New String() {"Title ID: "}, StringSplitOptions.None)(1)}
                    NewPS5GamePatches.Show()
                Else
                    MsgBox("Updates can only be checked for retail games and apps.", MsgBoxStyle.Information)
                End If
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
                If File.Exists(OFD.FileName) AndAlso Not String.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath) Then
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
        End If
    End Sub

    Private Sub AppChangeBackgroundMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppChangeBackgroundMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            Dim OFD As New OpenFileDialog() With {.Title = "Select a new pic0.png image for this app", .Filter = "PNG images (*.png)|*.png", .Multiselect = False}

            If OFD.ShowDialog() = Forms.DialogResult.OK Then
                If File.Exists(OFD.FileName) AndAlso Not String.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath) Then
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
                    If File.Exists(OFD.FileName) AndAlso Not String.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath) Then
                        File.Copy(OFD.FileName, SelectedPS5Game.GameFileOrFolderPath + "\sce_sys\snd0.at9", True)
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub AppOpenLocationMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles AppOpenLocationMenuItem.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            If Not String.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath) Then
                If Directory.Exists(SelectedPS5Game.GameFileOrFolderPath) Then
                    Process.Start("explorer", SelectedPS5Game.GameFileOrFolderPath)
                End If
            End If
        End If
    End Sub

    Private Sub AppPackAsPKG_Click(sender As Object, e As RoutedEventArgs) Handles AppPackAsPKG.Click
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)
            If Not String.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath) Then

                'Create a GP5 project
                Dim AppProjectPath As String = SelectedPS5Game.GameFileOrFolderPath
                Dim NewGP5Project As XDocument = <?xml version="1.0" encoding="utf-8" standalone="no"?>
                                                 <psproject fmt="gp5" version="1000">
                                                     <volume>
                                                         <volume_type>prospero_app</volume_type>
                                                         <package passcode="00000000000000000000000000000000"/>
                                                         <chunk_info chunk_count="1" scenario_count="1">
                                                             <chunks>
                                                                 <chunk id="0" label="Chunk #0"/>
                                                             </chunks>
                                                             <scenarios default_id="0">
                                                                 <scenario id="0" initial_chunk_count="1" label="Scenario #0" type="playmode">0</scenario>
                                                             </scenarios>
                                                         </chunk_info>
                                                     </volume>
                                                     <global_exclude/>
                                                     <rootdir dir_exclude="about" file_exclude="*.esbak;keystone;*.dds;disc_info.dat;pfs-version.dat;ext_info.dat" src_path=<%= AppProjectPath %>/>
                                                 </psproject>

                'Save the GP5 project
                Dim GP5ProjectPath As String = ""
                Dim SFD As New SaveFileDialog() With {.Title = "Select a save path for the GP5 project", .DefaultExt = ".gp5", .AddExtension = True, .Filter = "GP5 Project (*.gp5)|*.gp5"}
                If SFD.ShowDialog() = Forms.DialogResult.OK Then
                    GP5ProjectPath = SFD.FileName
                    NewGP5Project.Save(SFD.FileName)
                End If

                'Show the PKG Builder
                Dim NewPKGBuilder As New PS5PKGBuilder() With {.ShowActivated = True}
                NewPKGBuilder.SelectedProjectTextBox.Text = GP5ProjectPath
                NewPKGBuilder.Show()

                MsgBox("A GP5 project for " + SelectedPS5Game.GameTitle + " - [" + SelectedPS5Game.GameID + "] has been created." + vbCrLf + "Select an output path for the final PKG file and build it.", MsgBoxStyle.Information, "PS5 Apps Library")
            End If
        End If
    End Sub

#End Region

    Private Async Sub ContentWebView_NavigationCompleted(sender As Object, e As CoreWebView2NavigationCompletedEventArgs) Handles ContentWebView.NavigationCompleted
        Dim GameCoverSource As String = String.Empty
        If e.IsSuccess And ContentWebView.Source.ToString.StartsWith("https://prosperopatches.com/") Then
            'Game ID
            Dim GameID As String = Await ContentWebView.ExecuteScriptAsync("document.getElementsByClassName('bd-links-group py-2')[0].innerText;")

            'Game Image
            Dim GameImageURL As String = Await ContentWebView.ExecuteScriptAsync("document.getElementsByClassName('game-icon secondary')[0].outerHTML;")
            Dim SplittedGameImageURL As String() = GameImageURL.Split(New String() {"(", ")"}, StringSplitOptions.None)

            If SplittedGameImageURL.Length > 0 And Not String.IsNullOrEmpty(GameID) Then
                GameID = GameID.Split(New String() {"\n"}, StringSplitOptions.None)(1).Replace("""", "")
                GameCoverSource = SplittedGameImageURL(1)
            End If

            If Not String.IsNullOrEmpty(GameCoverSource) And Not String.IsNullOrEmpty(GameID) Then
                For Each ItemInListView In NewGamesListView.Items
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
                NewGamesListView.Items.Refresh()
                PatchesListView.Items.Refresh()
                Cursor = Input.Cursors.Arrow
            End If
        End If
    End Sub

    Private Function GetDirSize(RootFolder As String) As Long
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

    Private Sub NewGamesListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles NewGamesListView.SelectionChanged
        If NewGamesListView.SelectedItem IsNot Nothing Then

            'Get values
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            GameTitleTextBlock.Text = SelectedPS5Game.GameTitle
            GameIDTextBlock.Text = SelectedPS5Game.GameID
            GameRegionTextBlock.Text = SelectedPS5Game.GameRegion
            GameContentVersionTextBlock.Text = SelectedPS5Game.GameVersion
            GameContentIDTextBlock.Text = SelectedPS5Game.GameContentID
            GameCategoryTextBlock.Text = SelectedPS5Game.GameCategory
            GameSizeTextBlock.Text = SelectedPS5Game.GameSize
            GameRequiredFirmwareTextBlock.Text = SelectedPS5Game.GameRequiredFirmware
            GameMasterVersionTextBlock.Text = SelectedPS5Game.GameMasterVersion
            GameSDKVersionTextBlock.Text = SelectedPS5Game.GameSDKVersion
            GamePubToolVersionTextBlock.Text = SelectedPS5Game.GamePubToolVersion
            GameVersionFileURITextBlock.Text = "Update URL: " + Utils.GetFilenameFromUrl(New Uri(SelectedPS5Game.GameVersionFileURI)).Replace("-version.xml", "").Trim()

            'Set backup folder name or path
            If Not String.IsNullOrEmpty(SelectedPS5Game.GameFileOrFolderPath) AndAlso SelectedPS5Game.GameLocation = PS5Game.Location.Local Then
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " & New DirectoryInfo(Path.GetDirectoryName(SelectedPS5Game.GameFileOrFolderPath)).Name
            Else
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " & SelectedPS5Game.GameFileOrFolderPath
            End If

            'Show background
            If SelectedPS5Game.GameBGSource IsNot Nothing Then
                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub()
                                               RectangleImageBrush.ImageSource = SelectedPS5Game.GameBGSource
                                               BlurringShape.BeginAnimation(OpacityProperty, New DoubleAnimation With {.From = 0, .To = 1, .Duration = New Duration(TimeSpan.FromMilliseconds(500))})
                                           End Sub)
                Else
                    RectangleImageBrush.ImageSource = SelectedPS5Game.GameBGSource
                    BlurringShape.BeginAnimation(OpacityProperty, New DoubleAnimation With {.From = 0, .To = 1, .Duration = New Duration(TimeSpan.FromMilliseconds(500))})
                End If
            Else
                RectangleImageBrush.ImageSource = Nothing
            End If

            'Play background music
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
                End If
            End If

            'Set up context menu for selected backup
            GamesContextMenu.Items.Clear()
            If SelectedPS5Game.GameLocation = PS5Game.Location.Local Then
                GamesContextMenu.Items.Add(GameOpenLocationMenuItem)
                GamesContextMenu.Items.Add(GameCopyToMenuItem)
                GamesContextMenu.Items.Add(GameBrowseAssetsMenuItem)
                GamesContextMenu.Items.Add(GamePlayMenuItem)
                GamesContextMenu.Items.Add(GameCheckForUpdatesMenuItem)

                If SelectedPS5Game.GameBackupType = "Folder" Then
                    GamesContextMenu.Items.Add(GamePackAsPKG)
                End If

                GamesContextMenu.Items.Add(New Separator())
                GamesContextMenu.Items.Add(GameChangeTypeMenuItem)
                GamesContextMenu.Items.Add(GameRenameMenuItem)
                GamesContextMenu.Items.Add(GameChangeIconMenuItem)
                GamesContextMenu.Items.Add(GameChangeBackgroundMenuItem)
                GamesContextMenu.Items.Add(GameChangeSoundtrackMenuItem)
            ElseIf SelectedPS5Game.GameLocation = PS5Game.Location.Remote Then
                GamesContextMenu.Items.Add(GameLaunchMenuItem)

                'Add correct move options
                If SelectedPS5Game.GameRootLocation = PS5Game.RootLocation.Internal Then
                    'GamesContextMenu.Items.Add(GameMoveToUSB0MenuItem)
                    'GamesContextMenu.Items.Add(GameMoveToUSB1MenuItem)
                Else
                    'GamesContextMenu.Items.Add(GameMoveToInternalMenuItem)
                End If
            End If

            'Show values
            GameTitleTextBlock.Visibility = Visibility.Visible
            GameIDTextBlock.Visibility = Visibility.Visible
            GameRegionTextBlock.Visibility = Visibility.Visible
            GameContentVersionTextBlock.Visibility = Visibility.Visible
            GameContentIDTextBlock.Visibility = Visibility.Visible
            GameCategoryTextBlock.Visibility = Visibility.Visible
            GameSizeTextBlock.Visibility = Visibility.Visible
            GameRequiredFirmwareTextBlock.Visibility = Visibility.Visible
            GameBackupFolderNameTextBlock.Visibility = Visibility.Visible
            GameMasterVersionTextBlock.Visibility = Visibility.Visible
            GameSDKVersionTextBlock.Visibility = Visibility.Visible
            GamePubToolVersionTextBlock.Visibility = Visibility.Visible
            GameVersionFileURITextBlock.Visibility = Visibility.Visible
        Else
            'Hide values
            GameTitleTextBlock.Visibility = Visibility.Hidden
            GameIDTextBlock.Visibility = Visibility.Hidden
            GameRegionTextBlock.Visibility = Visibility.Hidden
            GameContentVersionTextBlock.Visibility = Visibility.Hidden
            GameContentIDTextBlock.Visibility = Visibility.Hidden
            GameCategoryTextBlock.Visibility = Visibility.Hidden
            GameSizeTextBlock.Visibility = Visibility.Hidden
            GameRequiredFirmwareTextBlock.Visibility = Visibility.Hidden
            GameBackupFolderNameTextBlock.Visibility = Visibility.Hidden
            GameMasterVersionTextBlock.Visibility = Visibility.Hidden
            GameSDKVersionTextBlock.Visibility = Visibility.Hidden
            GamePubToolVersionTextBlock.Visibility = Visibility.Hidden
            GameVersionFileURITextBlock.Visibility = Visibility.Hidden
        End If
    End Sub

    Private Sub NewGamesListView_PreviewMouseWheel(sender As Object, e As MouseWheelEventArgs) Handles NewGamesListView.PreviewMouseWheel
        Dim OpenWindowsListViewScrollViewer As ScrollViewer = Utils.FindScrollViewer(NewGamesListView)
        Dim HorizontalOffset As Double = OpenWindowsListViewScrollViewer.HorizontalOffset
        OpenWindowsListViewScrollViewer.ScrollToHorizontalOffset(HorizontalOffset - (e.Delta / 100))
        e.Handled = True
    End Sub

    Private Sub NewSettingsMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles NewSettingsMenuItem.Click
        Dim NewSettings As New PSSettings() With {.ShowActivated = True}
        NewSettings.Show()
    End Sub

    Private Sub AppsListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles AppsListView.SelectionChanged
        If AppsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(AppsListView.SelectedItem, PS5Game)

            If SelectedPS5Game.GameBackupType = "Folder" Then
                AppPackAsPKG.Visibility = Visibility.Visible
            End If
        Else
            AppPackAsPKG.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Sub GameVersionFileURITextBlock_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles GameVersionFileURITextBlock.MouseLeftButtonDown
        If NewGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS5Game As PS5Game = CType(NewGamesListView.SelectedItem, PS5Game)
            Dim NewProcessStartInfo As New ProcessStartInfo() With {.FileName = SelectedPS5Game.GameVersionFileURI, .UseShellExecute = True}
            Process.Start(NewProcessStartInfo)
            e.Handled = True
        End If
    End Sub

End Class
