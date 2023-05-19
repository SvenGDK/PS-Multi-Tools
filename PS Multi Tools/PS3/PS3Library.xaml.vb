Imports System.ComponentModel
Imports System.IO
Imports System.Windows.Forms
Imports psmt_lib

Public Class PS3Library

    Dim WithEvents GameLoaderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}
    Dim WithEvents NewLoadingWindow As New SyncWindow() With {.Title = "Loading PS3 files", .ShowActivated = True}

    Dim FoldersCount As Integer = 0
    Dim PKGCount As Integer = 0
    Dim ISOCount As Integer = 0
    Dim IsSoundPlaying As Boolean = False

    'Used to change the views
    Dim SelectedListView As Controls.ListView

    'Games context menu items
    Dim WithEvents NewContextMenu As New Controls.ContextMenu()
    Dim WithEvents CopyToMenuItem As New Controls.MenuItem() With {.Header = "Copy to"}
    Dim WithEvents ExtractPKGMenuItem As New Controls.MenuItem() With {.Header = "Extract .pkg", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/extract.png", UriKind.Relative))}}
    Dim WithEvents PlayMenuItem As New Controls.MenuItem() With {.Header = "Play Soundtrack", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Play-icon.png", UriKind.Relative))}}
    Dim WithEvents PKGInfoMenuItem As New Controls.MenuItem() With {.Header = "PKG Details", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/information-button.png", UriKind.Relative))}}

    'ISO tools context menu items
    Dim WithEvents ISOToolsMenuItem As New Controls.MenuItem() With {.Header = "ISO Tools", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/iso.png", UriKind.Relative))}}
    Dim WithEvents ExtractISOMenuItem As New Controls.MenuItem() With {.Header = "Extract ISO", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/extract.png", UriKind.Relative))}}
    Dim WithEvents CreateISOMenuItem As New Controls.MenuItem() With {.Header = "Create ISO", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/create.png", UriKind.Relative))}}
    Dim WithEvents PatchISOMenuItem As New Controls.MenuItem() With {.Header = "Patch ISO", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/patch.png", UriKind.Relative))}}
    Dim WithEvents SplitISOMenuItem As New Controls.MenuItem() With {.Header = "Split ISO", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/split.png", UriKind.Relative))}}

    'Supplemental library menu items
    Dim WithEvents LoadFolderMenuItem As New Controls.MenuItem() With {.Header = "Load a new folder"}
    Dim WithEvents LoadLibraryMenuItem As New Controls.MenuItem() With {.Header = "Show games library"}
    Dim WithEvents LoadDLFolderMenuItem As New Controls.MenuItem() With {.Header = "Open Downloads folder"}

    Private Sub PS3Library_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Set the controls in the shared library
        NewPS3Menu.LView = DLListView
        NewPS3Menu.GamesLView = SimpleGamesListView
        NewPS3Menu.PS3Img = GameIconImage
        NewPS3Menu.PS3Title = SelectedGameTextBlock
        NewPS3Menu.LoadDownloaderContextMenu()

        'Add supplemental library menu items that will be handled in the app
        Dim LibraryMenuItem As Controls.MenuItem = CType(NewPS3Menu.Items(0), Controls.MenuItem)
        LibraryMenuItem.Items.Add(LoadFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadLibraryMenuItem)
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem)

        'Load available context menu options
        SimpleGamesListView.ContextMenu = NewContextMenu
        GamesListView.ContextMenu = NewContextMenu

        'Set the default view
        SelectedListView = SimpleGamesListView
    End Sub

#Region "Game Loader"

    Private Sub GameLoaderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles GameLoaderWorker.DoWork

        'PS3 classic backup folders
        For Each Game In Directory.GetFiles(e.Argument.ToString, "*.SFO", SearchOption.AllDirectories)

            Dim NewPS3Game As New PS3Game()

            Using SFOReader As New Process()
                SFOReader.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\sfo.exe"
                SFOReader.StartInfo.Arguments = """" + Game + """ --decimal"
                SFOReader.StartInfo.RedirectStandardOutput = True
                SFOReader.StartInfo.UseShellExecute = False
                SFOReader.StartInfo.CreateNoWindow = True
                SFOReader.Start()

                Dim OutputReader As StreamReader = SFOReader.StandardOutput
                Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

                If ProcessOutput.Count > 0 Then

                    'Load game infos
                    For Each Line In ProcessOutput
                        If Line.StartsWith("TITLE=") Then
                            NewPS3Game.GameTitle = Utils.CleanTitle(Line.Split("="c)(1).Trim(""""c))
                        ElseIf Line.StartsWith("TITLE_ID=") Then
                            NewPS3Game.GameID = Line.Split("="c)(1).Trim(""""c)
                        ElseIf Line.StartsWith("CATEGORY=") Then
                            NewPS3Game.GameCategory = PS3Game.GetCategory(Line.Split("="c)(1).Trim(""""c))
                        ElseIf Line.StartsWith("APP_VER=") Then
                            NewPS3Game.GameAppVer = FormatNumber(Line.Split("="c)(1).Trim(""""c), 2)
                        ElseIf Line.StartsWith("PS3_SYSTEM_VER=") Then
                            NewPS3Game.GameRequiredFW = FormatNumber(Line.Split("="c)(1).Trim(""""c), 2)
                        ElseIf Line.StartsWith("VERSION=") Then
                            NewPS3Game.GameVer = "Version: " + FormatNumber(Line.Split("="c)(1).Trim(""""c), 2)
                        ElseIf Line.StartsWith("RESOLUTION=") Then
                            NewPS3Game.GameResolution = PS3Game.GetGameResolution(Line.Split("="c)(1).Trim(""""c))
                        ElseIf Line.StartsWith("SOUND_FORMAT=") Then
                            NewPS3Game.GameSoundFormat = PS3Game.GetGameSoundFormat(Line.Split("="c)(1).Trim(""""c))
                        End If
                    Next

                    'Load game files
                    Dim PS3GAMEFolder As String = Path.GetDirectoryName(Game)
                    If File.Exists(PS3GAMEFolder + "\ICON0.PNG") Then
                        Dispatcher.BeginInvoke(Sub() NewPS3Game.GameCoverSource = New BitmapImage(New Uri(PS3GAMEFolder + "\ICON0.PNG", UriKind.RelativeOrAbsolute)))
                    End If
                    If File.Exists(PS3GAMEFolder + "\PIC1.PNG") Then
                        Dispatcher.BeginInvoke(Sub() NewPS3Game.GameBackgroundSource = New BitmapImage(New Uri(PS3GAMEFolder + "\PIC1.PNG", UriKind.RelativeOrAbsolute)))
                    End If
                    If File.Exists(PS3GAMEFolder + "\SND0.AT3") Then
                        NewPS3Game.GameBackgroundSoundFile = PS3GAMEFolder + "\SND0.AT3"
                    End If

                    Dim PS3GAMEFolderSize As Long = Utils.DirSize(PS3GAMEFolder, True)
                    NewPS3Game.GameSize = FormatNumber(PS3GAMEFolderSize / 1073741824, 2) + " GB"
                    NewPS3Game.GameFolderPath = Directory.GetParent(PS3GAMEFolder).FullName
                    NewPS3Game.GameFileType = PS3Game.GameFileTypes.Backup

                    If Not String.IsNullOrWhiteSpace(NewPS3Game.GameID) Then
                        NewPS3Game.GameRegion = PS3Game.GetGameRegion(NewPS3Game.GameID)
                    End If

                    'Update progress
                    Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                    Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading folder " + (NewLoadingWindow.LoadProgressBar.Value - ISOCount - PKGCount).ToString + " of " + FoldersCount.ToString())

                    'Add to the ListView
                    If SimpleGamesListView.Dispatcher.CheckAccess() = False Then
                        SimpleGamesListView.Dispatcher.BeginInvoke(Sub() SimpleGamesListView.Items.Add(NewPS3Game))
                    Else
                        SimpleGamesListView.Items.Add(NewPS3Game)
                    End If

                End If

            End Using

        Next

        'PS3 PKGs
        For Each GamePKG In Directory.GetFiles(e.Argument.ToString, "*.pkg", SearchOption.AllDirectories)

            Dim NewPS3Game As New PS3Game()
            Dim PKGFileInfo As New FileInfo(GamePKG)
            Dim NewPKGDecryptor As New PKGDecryptor()

            Try
                'Decrypt pkg file
                NewPKGDecryptor.ProcessPKGFile(GamePKG)

                'Load game infos
                If NewPKGDecryptor.GetPARAMSFO IsNot Nothing Then
                    Dim SFOKeys As Dictionary(Of String, Object) = SFONew.ReadSfo(NewPKGDecryptor.GetPARAMSFO)
                    If SFOKeys.ContainsKey("TITLE") Then
                        NewPS3Game.GameTitle = Utils.CleanTitle(SFOKeys("TITLE").ToString)
                    End If
                    If SFOKeys.ContainsKey("TITLE_ID") Then
                        NewPS3Game.GameID = SFOKeys("TITLE_ID").ToString
                    End If
                    If SFOKeys.ContainsKey("CATEGORY") Then
                        NewPS3Game.GameCategory = PS3Game.GetCategory(SFOKeys("CATEGORY").ToString)
                    End If
                    If SFOKeys.ContainsKey("CONTENT_ID") Then
                        NewPS3Game.ContentID = SFOKeys("CONTENT_ID").ToString
                    End If
                    If SFOKeys.ContainsKey("APP_VER") Then
                        NewPS3Game.GameAppVer = FormatNumber(SFOKeys("APP_VER").ToString, 2)
                    End If
                    If SFOKeys.ContainsKey("PS3_SYSTEM_VER") Then
                        NewPS3Game.GameRequiredFW = FormatNumber(SFOKeys("PS3_SYSTEM_VER").ToString)
                    End If
                    If SFOKeys.ContainsKey("VERSION") Then
                        NewPS3Game.GameVer = FormatNumber(SFOKeys("VERSION").ToString)
                    End If
                    If SFOKeys.ContainsKey("RESOLUTION") Then
                        NewPS3Game.GameResolution = PS3Game.GetGameResolution(SFOKeys("RESOLUTION").ToString)
                    End If
                    If SFOKeys.ContainsKey("SOUND_FORMAT") Then
                        NewPS3Game.GameSoundFormat = PS3Game.GetGameSoundFormat(SFOKeys("SOUND_FORMAT").ToString)
                    End If
                End If

                NewPS3Game.GameSize = FormatNumber(PKGFileInfo.Length / 1073741824, 2) + " GB"
                NewPS3Game.GameFileType = PS3Game.GameFileTypes.PKG

                If Not String.IsNullOrWhiteSpace(NewPS3Game.GameID) Then
                    NewPS3Game.GameRegion = PS3Game.GetGameRegion(NewPS3Game.GameID)
                End If

                NewPS3Game.GameFilePath = GamePKG

                'Check for additional content
                If NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.ICON0) IsNot Nothing Then
                    Dispatcher.BeginInvoke(Sub() NewPS3Game.GameCoverSource = NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.ICON0))
                End If
                If NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.PIC1) IsNot Nothing Then
                    Dispatcher.BeginInvoke(Sub() NewPS3Game.GameBackgroundSource = NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.PIC1))
                End If
                If NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.SND0) IsNot Nothing Then
                    Dispatcher.BeginInvoke(Sub() NewPS3Game.GameBackgroundSoundBytes = NewPKGDecryptor.GetSND())
                End If

                'Update progress
                Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading PKG " + (NewLoadingWindow.LoadProgressBar.Value - ISOCount - FoldersCount).ToString + " of " + PKGCount.ToString())

                'Add to the ListView
                If SimpleGamesListView.Dispatcher.CheckAccess() = False Then
                    SimpleGamesListView.Dispatcher.BeginInvoke(Sub() SimpleGamesListView.Items.Add(NewPS3Game))
                Else
                    SimpleGamesListView.Items.Add(NewPS3Game)
                End If

            Catch ex As Exception
                If NewPKGDecryptor.ContentID IsNot Nothing Then
                    NewPS3Game.GameTitle = NewPKGDecryptor.ContentID
                    NewPS3Game.GameID = "ID: " + Utils.GetPKGTitleID(GamePKG)
                Else
                    NewPS3Game.GameTitle = "Unsupported PS3 .pkg"
                End If
                Continue For
            End Try

        Next

        'PS3 ISOs
        For Each GameISO In Directory.GetFiles(e.Argument.ToString, "*.iso", SearchOption.AllDirectories)

            Dim NewPS3Game As New PS3Game()
            Dim ISOFileInfo As New FileInfo(GameISO)
            Dim ISOCacheFolderName As String = Path.GetFileNameWithoutExtension(ISOFileInfo.Name)

            'Create cache dir for PS3 games
            If Not Directory.Exists(My.Computer.FileSystem.CurrentDirectory + "\Cache\PS3\" + ISOCacheFolderName) Then
                Directory.CreateDirectory(My.Computer.FileSystem.CurrentDirectory + "\Cache\PS3\" + ISOCacheFolderName)
            End If

            'Extract files to display infos
            If Not File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Cache\PS3\" + ISOCacheFolderName + "\PARAM.SFO") Then
                Using ISOExtractor As New Process()
                    ISOExtractor.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\7z.exe"
                    ISOExtractor.StartInfo.Arguments = "e """ + GameISO + """" +
                        " -o""" + My.Computer.FileSystem.CurrentDirectory + "\Cache\PS3\" + ISOCacheFolderName + """" +
                        " PS3_GAME/PARAM.SFO PARAM.SFO PS3_GAME/ICON0.PNG ICON0.PNG PS3_GAME/PIC1.PNG PIC1.PNG PS3_GAME/SND0.AT3 SND0.AT3"
                    ISOExtractor.StartInfo.RedirectStandardOutput = True
                    ISOExtractor.StartInfo.UseShellExecute = False
                    ISOExtractor.StartInfo.CreateNoWindow = True
                    ISOExtractor.Start()
                    ISOExtractor.WaitForExit()
                End Using
            End If

            Using SFOReader As New Process()
                SFOReader.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\sfo.exe"
                SFOReader.StartInfo.Arguments = """" + My.Computer.FileSystem.CurrentDirectory + "\Cache\PS3\" + ISOCacheFolderName + "\PARAM.SFO" + """  --decimal"
                SFOReader.StartInfo.RedirectStandardOutput = True
                SFOReader.StartInfo.UseShellExecute = False
                SFOReader.StartInfo.CreateNoWindow = True
                SFOReader.Start()

                Dim OutputReader As StreamReader = SFOReader.StandardOutput
                Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

                If ProcessOutput.Count > 0 Then

                    'Load game infos
                    For Each Line In ProcessOutput
                        If Line.StartsWith("TITLE=") Then
                            NewPS3Game.GameTitle = Utils.CleanTitle(Line.Split("="c)(1).Trim(""""c))
                        ElseIf Line.StartsWith("TITLE_ID=") Then
                            NewPS3Game.GameID = Line.Split("="c)(1).Trim(""""c)
                        ElseIf Line.StartsWith("CATEGORY=") Then
                            NewPS3Game.GameCategory = PS3Game.GetCategory(Line.Split("="c)(1).Trim(""""c))
                        ElseIf Line.StartsWith("APP_VER=") Then
                            NewPS3Game.GameAppVer = FormatNumber(Line.Split("="c)(1).Trim(""""c), 2)
                        ElseIf Line.StartsWith("PS3_SYSTEM_VER=") Then
                            NewPS3Game.GameRequiredFW = FormatNumber(Line.Split("="c)(1).Trim(""""c), 2)
                        ElseIf Line.StartsWith("VERSION=") Then
                            NewPS3Game.GameVer = FormatNumber(Line.Split("="c)(1).Trim(""""c), 2)
                        ElseIf Line.StartsWith("RESOLUTION=") Then
                            NewPS3Game.GameResolution = PS3Game.GetGameResolution(Line.Split("="c)(1).Trim(""""c))
                        ElseIf Line.StartsWith("SOUND_FORMAT=") Then
                            NewPS3Game.GameSoundFormat = PS3Game.GetGameSoundFormat(Line.Split("="c)(1).Trim(""""c))
                        End If
                    Next

                    'Load game files
                    Dim PS3GAMEFolder As String = My.Computer.FileSystem.CurrentDirectory + "\Cache\PS3\" + ISOCacheFolderName
                    If File.Exists(PS3GAMEFolder + "\ICON0.PNG") Then
                        Dispatcher.BeginInvoke(Sub() NewPS3Game.GameCoverSource = New BitmapImage(New Uri(PS3GAMEFolder + "\ICON0.PNG", UriKind.RelativeOrAbsolute)))
                    End If
                    If File.Exists(PS3GAMEFolder + "\PIC1.PNG") Then
                        Dispatcher.BeginInvoke(Sub() NewPS3Game.GameBackgroundSource = New BitmapImage(New Uri(PS3GAMEFolder + "\PIC1.PNG", UriKind.RelativeOrAbsolute)))
                    End If
                    If File.Exists(PS3GAMEFolder + "\SND0.AT3") Then
                        NewPS3Game.GameBackgroundSoundFile = PS3GAMEFolder + "\SND0.AT3"
                    End If

                    NewPS3Game.GameSize = FormatNumber(ISOFileInfo.Length / 1073741824, 2) + " GB"

                    If Not String.IsNullOrWhiteSpace(NewPS3Game.GameID) Then
                        NewPS3Game.GameRegion = PS3Game.GetGameRegion(NewPS3Game.GameID)
                    End If

                    NewPS3Game.GameFilePath = GameISO
                    NewPS3Game.GameFileType = PS3Game.GameFileTypes.ISO

                    'Update progress
                    Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                    Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading ISO " + (NewLoadingWindow.LoadProgressBar.Value - FoldersCount - PKGCount).ToString + " of " + ISOCount.ToString())

                    'Add to the ListView
                    If SimpleGamesListView.Dispatcher.CheckAccess() = False Then
                        SimpleGamesListView.Dispatcher.BeginInvoke(Sub() SimpleGamesListView.Items.Add(NewPS3Game))
                    Else
                        SimpleGamesListView.Items.Add(NewPS3Game)
                    End If

                End If

            End Using

        Next

    End Sub

    Private Sub GameLoaderWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles GameLoaderWorker.RunWorkerCompleted
        NewLoadingWindow.Close()
    End Sub

#End Region

#Region "Contextmenu ISO Tools"

    Private Sub ExtractISOMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles ExtractISOMenuItem.Click
        If SelectedListView.SelectedItem IsNot Nothing Then
            Dim SelectedGame As PS3Game = CType(SelectedListView.SelectedItem, PS3Game)
            If File.Exists(SelectedGame.GameFolderPath) Then
                Dim NewISOTools As New PS3ISOTools With {.ShowActivated = True, .ISOToExtract = SelectedGame.GameFolderPath}
                NewISOTools.Show()
                MsgBox("Please continue with PS3 ISO Tools and specify an output folder.", MsgBoxStyle.Information)
            End If
        End If
    End Sub

    Private Sub CreateISOMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles CreateISOMenuItem.Click
        If SelectedListView.SelectedItem IsNot Nothing Then
            Dim SelectedGame As PS3Game = CType(SelectedListView.SelectedItem, PS3Game)
            If Directory.Exists(SelectedGame.GameFolderPath) Then
                Dim NewISOTools As New PS3ISOTools With {.ShowActivated = True, .ISOToCreate = SelectedGame.GameFolderPath}
                NewISOTools.Show()
                MsgBox("Please continue with PS3 ISO Tools and specify an output folder.", MsgBoxStyle.Information)
            End If
        End If
    End Sub

    Private Sub PatchISOMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PatchISOMenuItem.Click
        If SelectedListView.SelectedItem IsNot Nothing Then
            Dim SelectedGame As PS3Game = CType(SelectedListView.SelectedItem, PS3Game)
            If File.Exists(SelectedGame.GameFolderPath) Then
                Dim NewISOTools As New PS3ISOTools With {.ShowActivated = True, .ISOToPatch = SelectedGame.GameFolderPath}
                NewISOTools.Show()
                MsgBox("Please continue with PS3 ISO Tools and specify an output folder.", MsgBoxStyle.Information)
            End If
        End If
    End Sub

    Private Sub SplitISOMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles SplitISOMenuItem.Click
        If SelectedListView.SelectedItem IsNot Nothing Then
            Dim SelectedGame As PS3Game = CType(SelectedListView.SelectedItem, PS3Game)
            If File.Exists(SelectedGame.GameFolderPath) Then
                Dim NewISOTools As New PS3ISOTools With {.ShowActivated = True, .ISOToSplit = SelectedGame.GameFolderPath}
                NewISOTools.Show()
                MsgBox("Please continue with PS3 ISO Tools and specify an output folder.", MsgBoxStyle.Information)
            End If
        End If
    End Sub

#End Region

#Region "Contextmenu Actions"

    Private Sub ExtractPKGMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles ExtractPKGMenuItem.Click
        If SimpleGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS3Game As PS3Game = CType(SimpleGamesListView.SelectedItem, PS3Game)
            Dim NewPKGExtractor As New PS3PKGExtractor() With {.SelectedPKG = SelectedPS3Game.GameFilePath}
            NewPKGExtractor.Show()
        End If
    End Sub

    Private Sub PlayMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PlayMenuItem.Click
        If SimpleGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS3Game As PS3Game = CType(SimpleGamesListView.SelectedItem, PS3Game)

            If SelectedPS3Game.GameBackgroundSoundFile IsNot Nothing Then
                If IsSoundPlaying Then
                    Utils.StopPS3SND()
                    IsSoundPlaying = False
                Else
                    Utils.PlayPS3SND(SelectedPS3Game.GameBackgroundSoundFile)
                    IsSoundPlaying = True
                End If
            Else
                If IsSoundPlaying Then
                    Utils.StopPS3SND()
                    IsSoundPlaying = False
                Else
                    MsgBox("No game soundtrack found.", MsgBoxStyle.Information)
                End If
            End If
        End If
    End Sub

    Private Sub PKGInfoMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PKGInfoMenuItem.Click
        If SimpleGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS3Game As PS3Game = CType(SimpleGamesListView.SelectedItem, PS3Game)
            If Not String.IsNullOrEmpty(SelectedPS3Game.ContentID) Then
                If MsgBox("Load from NPS?", MsgBoxStyle.YesNo, "") = MsgBoxResult.Yes Then
                    Dim SelectedPackage As New psmt_lib.Structures.Package() With {.PackageContentID = SelectedPS3Game.ContentID, .PackageTitleID = SelectedPS3Game.GameID}
                    Dim NewPackageInfoWindow As New DownloadPackageInfoWindow() With {.ShowActivated = True, .Title = SelectedPS3Game.GameTitle, .CurrentPackage = SelectedPackage, .PackageConsole = "PS3"}
                    NewPackageInfoWindow.Show()
                End If
            Else
                Dim NewPKGInfo As New PKGInfo() With {.SelectedPKG = SelectedPS3Game.GameFilePath, .Console = "PS3"}
                NewPKGInfo.Show()
            End If
        End If
    End Sub

    Private Sub CopyToMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles CopyToMenuItem.Click
        If SimpleGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS3Game As PS3Game = CType(SimpleGamesListView.SelectedItem, PS3Game)
            Dim FBD As New FolderBrowserDialog() With {.Description = "Where do you want to save the selected game ?"}

            If FBD.ShowDialog() = Forms.DialogResult.OK Then
                Dim NewCopyWindow As New CopyWindow() With {.ShowActivated = True,
                    .WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    .BackupDestinationPath = FBD.SelectedPath + "\",
                    .Title = "Copying " + SelectedPS3Game.GameTitle + " to " + FBD.SelectedPath + "\" + Path.GetFileName(SelectedPS3Game.GameFilePath)}

                If SelectedPS3Game.GameFileType = PS3Game.GameFileTypes.Backup Then
                    NewCopyWindow.BackupPath = SelectedPS3Game.GameFolderPath
                ElseIf SelectedPS3Game.GameFileType = PS3Game.GameFileTypes.ISO Then
                    NewCopyWindow.BackupPath = SelectedPS3Game.GameFilePath
                ElseIf SelectedPS3Game.GameFileType = PS3Game.GameFileTypes.PKG Then
                    NewCopyWindow.BackupPath = SelectedPS3Game.GameFilePath
                End If

                If NewCopyWindow.ShowDialog() = True Then
                    MsgBox("Game copied with success !", MsgBoxStyle.Information, "Completed")
                End If
            End If

        End If
    End Sub

#End Region

#Region "Menu Actions"

    Private Sub LoadFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadFolderMenuItem.Click
        Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Select your PS3 backup folder"}

        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            FoldersCount = Directory.GetFiles(FBD.SelectedPath, "*.SFO", SearchOption.AllDirectories).Count
            ISOCount = Directory.GetFiles(FBD.SelectedPath, "*.iso", SearchOption.AllDirectories).Count
            PKGCount = Directory.GetFiles(FBD.SelectedPath, "*.pkg", SearchOption.AllDirectories).Count

            'Show the loading progress window
            NewLoadingWindow = New SyncWindow() With {.Title = "Loading PS3 files", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.Maximum = FoldersCount + ISOCount + PKGCount
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + (FoldersCount + ISOCount + PKGCount).ToString()
            NewLoadingWindow.Show()

            GameLoaderWorker.RunWorkerAsync(FBD.SelectedPath)
        End If
    End Sub

    Private Sub LoadLibraryMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadLibraryMenuItem.Click
        SelectedListView.Visibility = Visibility.Visible
        DLListView.Visibility = Visibility.Hidden
    End Sub

    Private Sub LoadDLFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadDLFolderMenuItem.Click
        If Directory.Exists(My.Computer.FileSystem.CurrentDirectory + "\Downloads") Then
            Process.Start(My.Computer.FileSystem.CurrentDirectory + "\Downloads")
        End If
    End Sub

#End Region

    Private Sub SimpleGamesListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles SimpleGamesListView.SelectionChanged
        If SimpleGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS3Game As PS3Game = CType(SimpleGamesListView.SelectedItem, PS3Game)
            If SelectedPS3Game.GameBackgroundSource IsNot Nothing Then
                GameIconImage.Source = SelectedPS3Game.GameCoverSource
                PS3LibraryWindow.Background = New ImageBrush(SelectedPS3Game.GameBackgroundSource)
            End If
            If Not String.IsNullOrEmpty(SelectedPS3Game.GameTitle) Then
                SelectedGameTextBlock.Text = SelectedPS3Game.GameTitle
            End If
        End If
    End Sub

    Private Sub SimpleGamesListView_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs) Handles SimpleGamesListView.ContextMenuOpening
        NewContextMenu.Items.Clear()
        ISOToolsMenuItem.Items.Clear()

        If SimpleGamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS3Game As PS3Game = CType(SimpleGamesListView.SelectedItem, PS3Game)

            NewContextMenu.Items.Add(CopyToMenuItem)

            If SelectedPS3Game.GameFileType = PS3Game.GameFileTypes.Backup Then
                NewContextMenu.Items.Add(PlayMenuItem)
                NewContextMenu.Items.Add(ISOToolsMenuItem)
                ISOToolsMenuItem.Items.Add(CreateISOMenuItem)
            ElseIf SelectedPS3Game.GameFileType = PS3Game.GameFileTypes.PKG Then
                NewContextMenu.Items.Add(PKGInfoMenuItem)
                NewContextMenu.Items.Add(ExtractPKGMenuItem)
            ElseIf SelectedPS3Game.GameFileType = PS3Game.GameFileTypes.ISO Then
                NewContextMenu.Items.Add(ISOToolsMenuItem)
                ISOToolsMenuItem.Items.Add(ExtractISOMenuItem)
                ISOToolsMenuItem.Items.Add(PatchISOMenuItem)
                ISOToolsMenuItem.Items.Add(SplitISOMenuItem)
            End If
        End If
    End Sub

    Private Sub SimpleGamesListView_ContextMenuClosing(sender As Object, e As ContextMenuEventArgs) Handles SimpleGamesListView.ContextMenuClosing
        NewContextMenu.Items.Clear()
    End Sub

End Class
