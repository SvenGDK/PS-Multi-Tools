Imports System.ComponentModel
Imports System.IO

Public Class PS2Library

    Dim WithEvents GameLoaderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}
    Dim WithEvents PSXDatacenterBrowser As New Forms.WebBrowser() With {.ScriptErrorsSuppressed = True}
    Dim WithEvents NewLoadingWindow As New SyncWindow() With {.Title = "Loading PS2 files", .ShowActivated = True}

    Dim ISOCount As Integer = 0
    Dim CSOCount As Integer = 0

    'Used for game infos and covers
    Dim URLs As New List(Of String)()
    Dim CurrentURL As Integer = 0

    'Selected game context menu
    Dim WithEvents NewContextMenu As New Controls.ContextMenu()
    Dim WithEvents CopyToMenuItem As New Controls.MenuItem() With {.Header = "Copy to", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/copy-icon.png", UriKind.Relative))}}
    Dim WithEvents SendToMenuItem As New Controls.MenuItem() With {.Header = "Send to PS4/5", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/send-icon.png", UriKind.Relative))}}
    Dim WithEvents PlayGameMenuItem As New Controls.MenuItem() With {.Header = "Play with PCSX2", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/controller.png", UriKind.Relative))}}
    Dim WithEvents CreateProjectMenuItem As New Controls.MenuItem() With {.Header = "Create a game project for the PSX", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/copy-icon.png", UriKind.Relative))}}

    'Supplemental library menu items
    Dim WithEvents LoadFolderMenuItem As New Controls.MenuItem() With {.Header = "Load a new folder"}
    Dim WithEvents LoadDLFolderMenuItem As New Controls.MenuItem() With {.Header = "Open Downloads folder"}

    'Supplemental emulator menu item
    Dim WithEvents EMU_Settings As New Controls.MenuItem() With {.Header = "PCSX2 Settings"}

    Private Sub PS2Library_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Set the controls in the shared library
        NewPS2Menu.GamesLView = GamesListView

        'Add supplemental library menu items that will be handled in the app
        Dim LibraryMenuItem As Controls.MenuItem = CType(NewPS2Menu.Items(0), Controls.MenuItem)
        LibraryMenuItem.Items.Add(LoadFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem)

        NewContextMenu.Items.Add(CopyToMenuItem)
        NewContextMenu.Items.Add(SendToMenuItem)
        NewContextMenu.Items.Add(PlayGameMenuItem)
        NewContextMenu.Items.Add(CreateProjectMenuItem)
        GamesListView.ContextMenu = NewContextMenu

        'Add supplemental emulator menu item
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\PCSX2\pcsx2.exe") Then
            NewPS2Menu.Items.Add(EMU_Settings)
        End If
    End Sub

#Region "Game Loader"

    Private Sub GameLoaderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles GameLoaderWorker.DoWork

        'PS2 ISOs
        For Each GameISO In Directory.GetFiles(e.Argument.ToString, "*.iso", SearchOption.AllDirectories)

            Dim NewPS2Game As New PS2Game()
            Dim GameID As String = PS2Game.GetPS2GameID(GameISO)

            If GameID = "" Then
                NewPS2Game.GameFilePath = GameISO
                Dim PS2ISOFileInfo As New FileInfo(GameISO)
                NewPS2Game.GameSize = FormatNumber(PS2ISOFileInfo.Length / 1073741824, 2) + " GB"
                NewPS2Game.GameID = "Unknown"
                NewPS2Game.GameBackupType = PS2Game.GameFileType.ISO

                'Update progress
                Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading ISO " + (NewLoadingWindow.LoadProgressBar.Value - CSOCount).ToString() + " of " + ISOCount.ToString())

                'Add to the ListView
                If GamesListView.Dispatcher.CheckAccess() = False Then
                    GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPS2Game))
                Else
                    GamesListView.Items.Add(NewPS2Game)
                End If

            Else
                GameID = GameID.Replace(".", "").Replace("_", "-").Trim()

                Dim PS2ISOFileInfo As New FileInfo(GameISO)
                NewPS2Game.GameSize = FormatNumber(PS2ISOFileInfo.Length / 1073741824, 2) + " GB"
                NewPS2Game.GameID = GameID
                NewPS2Game.GameFilePath = GameISO
                NewPS2Game.GameBackupType = PS2Game.GameFileType.ISO

                If Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameID + ".jpg") Then
                    If Dispatcher.CheckAccess() = False Then
                        Dispatcher.BeginInvoke(Sub()
                                                   Dim TempBitmapImage = New BitmapImage()
                                                   TempBitmapImage.BeginInit()
                                                   TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                   TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                   TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameID + ".jpg", UriKind.RelativeOrAbsolute)
                                                   TempBitmapImage.EndInit()
                                                   NewPS2Game.GameCoverSource = TempBitmapImage
                                               End Sub)
                    Else
                        Dim TempBitmapImage = New BitmapImage()
                        TempBitmapImage.BeginInit()
                        TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                        TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                        TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameID + ".jpg", UriKind.RelativeOrAbsolute)
                        TempBitmapImage.EndInit()
                        NewPS2Game.GameCoverSource = TempBitmapImage
                    End If
                End If

                'Update progress
                Dispatcher.BeginInvoke(Sub()
                                           NewLoadingWindow.LoadProgressBar.Value += 1
                                           NewLoadingWindow.LoadStatusTextBlock.Text = "Loading ISO " + (NewLoadingWindow.LoadProgressBar.Value - CSOCount).ToString() + " of " + ISOCount.ToString()
                                       End Sub)

                'Add to the ListView
                If GamesListView.Dispatcher.CheckAccess() = False Then
                    GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPS2Game))
                Else
                    GamesListView.Items.Add(NewPS2Game)
                End If

                If Utils.IsURLValid("https://psxdatacenter.com/psx2/games2/" + GameID + ".html") Then
                    URLs.Add("https://psxdatacenter.com/psx2/games2/" + GameID + ".html")
                Else
                    NewPS2Game.GameTitle = PS2Game.GetPS2GameTitleFromDatabaseList(GameID.Replace("-", ""))
                End If
            End If
        Next

        'PS2 CSOs
        For Each GameCSO In Directory.GetFiles(e.Argument.ToString, "*.cso", SearchOption.AllDirectories)

            Dim NewPS2Game As New PS2Game()
            Dim GameID As String = PS2Game.GetPS2GameID(GameCSO)

            If GameID = "" Then

                'Add to the GamesListView
                NewPS2Game.GameFilePath = GameCSO
                Dim PS2CSOFileInfo As New FileInfo(GameCSO)
                NewPS2Game.GameSize = FormatNumber(PS2CSOFileInfo.Length / 1073741824, 2) + " GB"
                NewPS2Game.GameBackupType = PS2Game.GameFileType.CSO

                'Update progress
                Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading CSO " + (NewLoadingWindow.LoadProgressBar.Value - ISOCount).ToString() + " of " + CSOCount.ToString())

                'Add to the ListView
                If GamesListView.Dispatcher.CheckAccess() = False Then
                    GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPS2Game))
                Else
                    GamesListView.Items.Add(NewPS2Game)
                End If
            Else
                GameID = GameID.Replace(".", "").Replace("_", "-").Trim()

                Dim PS2CSOFileInfo As New FileInfo(GameCSO)
                NewPS2Game.GameSize = FormatNumber(PS2CSOFileInfo.Length / 1073741824, 2) + " GB"
                NewPS2Game.GameID = GameID
                NewPS2Game.GameFilePath = GameCSO
                NewPS2Game.GameBackupType = PS2Game.GameFileType.CSO

                If Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameID + ".jpg") Then
                    If Dispatcher.CheckAccess() = False Then
                        Dispatcher.BeginInvoke(Sub()
                                                   Dim TempBitmapImage = New BitmapImage()
                                                   TempBitmapImage.BeginInit()
                                                   TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                   TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                   TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameID + ".jpg", UriKind.RelativeOrAbsolute)
                                                   TempBitmapImage.EndInit()
                                                   NewPS2Game.GameCoverSource = TempBitmapImage
                                               End Sub)
                    Else
                        Dim TempBitmapImage = New BitmapImage()
                        TempBitmapImage.BeginInit()
                        TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                        TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                        TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS2/" + GameID + ".jpg", UriKind.RelativeOrAbsolute)
                        TempBitmapImage.EndInit()
                        NewPS2Game.GameCoverSource = TempBitmapImage
                    End If
                End If

                'Update progress
                Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading CSO " + (NewLoadingWindow.LoadProgressBar.Value - ISOCount).ToString() + " of " + CSOCount.ToString())

                'Add to the ListView
                If GamesListView.Dispatcher.CheckAccess() = False Then
                    GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPS2Game))
                Else
                    GamesListView.Items.Add(NewPS2Game)
                End If

                If Utils.IsURLValid("https://psxdatacenter.com/psx2/games2/" + GameID + ".html") Then
                    URLs.Add("https://psxdatacenter.com/psx2/games2/" + GameID + ".html")
                Else
                    NewPS2Game.GameTitle = PS2Game.GetPS2GameTitleFromDatabaseList(GameID.Replace("-", ""))
                End If
            End If
        Next

    End Sub

    Private Sub GameLoaderWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles GameLoaderWorker.RunWorkerCompleted
        NewLoadingWindow.LoadStatusTextBlock.Text = "Getting " + URLs.Count.ToString() + " available game infos."
        NewLoadingWindow.LoadProgressBar.Value = 0
        NewLoadingWindow.LoadProgressBar.Maximum = URLs.Count

        GetGameInfos()
    End Sub

    Private Sub GetGameInfos()
        PSXDatacenterBrowser.Navigate(URLs.Item(0))
    End Sub

    Private Sub PSXDatacenterBrowser_DocumentCompleted(sender As Object, e As Forms.WebBrowserDocumentCompletedEventArgs) Handles PSXDatacenterBrowser.DocumentCompleted
        RemoveHandler PSXDatacenterBrowser.DocumentCompleted, AddressOf PSXDatacenterBrowser_DocumentCompleted

        Dim GameTitle As String = ""
        Dim GameID As String = ""
        Dim GameRegion As String = ""
        Dim GameGenre As String = ""
        Dim GameDeveloper As String = ""
        Dim GamePublisher As String = ""
        Dim GameReleaseDate As String = ""
        Dim GameDescription As String = ""

        'Get game infos
        Dim infoRows As Forms.HtmlElementCollection = Nothing
        If PSXDatacenterBrowser.Document.GetElementsByTagName("tr") IsNot Nothing Then
            infoRows = PSXDatacenterBrowser.Document.GetElementsByTagName("tr")
        End If

        If infoRows IsNot Nothing AndAlso infoRows.Count > 0 Then
            'Game Title
            If infoRows.Item(4).InnerText IsNot Nothing Then
                If infoRows.Item(4).InnerText.Split(New String() {"OFFICIAL TITLE "}, StringSplitOptions.RemoveEmptyEntries).Length > 0 Then
                    GameTitle = infoRows.Item(4).InnerText.Split(New String() {"OFFICIAL TITLE "}, StringSplitOptions.RemoveEmptyEntries)(0)
                End If
            End If

            'Game ID
            If infoRows.Item(6).InnerText IsNot Nothing Then
                If infoRows.Item(6).InnerText.Split(New String() {"SERIAL NUMBER(S) "}, StringSplitOptions.RemoveEmptyEntries).Length > 0 Then
                    GameID = infoRows.Item(6).InnerText.Split(New String() {"SERIAL NUMBER(S) "}, StringSplitOptions.RemoveEmptyEntries)(0)
                End If
            End If

            'Region
            If infoRows.Item(7).InnerText IsNot Nothing Then
                If infoRows.Item(7).InnerText.Split(New String() {"REGION "}, StringSplitOptions.RemoveEmptyEntries).Length > 0 Then
                    Dim Region As String = infoRows.Item(7).InnerText.Split(New String() {"REGION "}, StringSplitOptions.RemoveEmptyEntries)(0)
                    Select Case Region
                        Case "PAL"
                            GameRegion = "Europe"
                        Case "NTSC-U"
                            GameRegion = "US"
                        Case "NTSC-J"
                            GameRegion = "Japan"
                    End Select
                End If
            End If

            'Genre
            If infoRows.Item(8).InnerText IsNot Nothing Then
                If infoRows.Item(8).InnerText.Split(New String() {"GENRE / STYLE "}, StringSplitOptions.RemoveEmptyEntries).Length > 0 Then
                    GameGenre = infoRows.Item(8).InnerText.Split(New String() {"GENRE / STYLE "}, StringSplitOptions.RemoveEmptyEntries)(0)
                End If
            End If

            'Developer
            If infoRows.Item(9).InnerText IsNot Nothing Then
                If infoRows.Item(9).InnerText.Split(New String() {"DEVELOPER "}, StringSplitOptions.RemoveEmptyEntries).Length > 0 Then
                    GameDeveloper = infoRows.Item(9).InnerText.Split(New String() {"DEVELOPER "}, StringSplitOptions.RemoveEmptyEntries)(0)
                End If
            End If

            'Publisher
            If infoRows.Item(10).InnerText IsNot Nothing Then
                If infoRows.Item(10).InnerText.Split(New String() {"PUBLISHER "}, StringSplitOptions.RemoveEmptyEntries).Length > 0 Then
                    GamePublisher = infoRows.Item(10).InnerText.Split(New String() {"PUBLISHER "}, StringSplitOptions.RemoveEmptyEntries)(0)
                End If
            End If

            'Release Date
            If infoRows.Item(11).InnerText IsNot Nothing Then
                If infoRows.Item(11).InnerText.Split(New String() {"DATE RELEASED "}, StringSplitOptions.RemoveEmptyEntries).Length > 0 Then
                    GameReleaseDate = infoRows.Item(11).InnerText.Split(New String() {"DATE RELEASED "}, StringSplitOptions.RemoveEmptyEntries)(0)
                End If
            End If
        End If

        'Get the game description
        If PSXDatacenterBrowser.Document.GetElementById("table16") IsNot Nothing AndAlso PSXDatacenterBrowser.Document.GetElementById("table16").GetElementsByTagName("tr").Count > 0 Then
            GameDescription = PSXDatacenterBrowser.Document.GetElementById("table16").GetElementsByTagName("tr")(0).InnerText
        End If

        'Add the infos to the game
        If Not String.IsNullOrEmpty(GameID) Then
            For Each Game In GamesListView.Items
                Dim FoundGame As PS2Game = CType(Game, PS2Game)

                If FoundGame.GameID.Contains(GameID) Or FoundGame.GameID = GameID Then
                    FoundGame.GameTitle = GameTitle
                    FoundGame.GameRegion = GameRegion
                    FoundGame.GameDescription = GameDescription
                    FoundGame.GameGenre = GameGenre
                    FoundGame.GameDeveloper = GameDeveloper
                    FoundGame.GamePublisher = GamePublisher
                    FoundGame.GameReleaseDate = GameReleaseDate
                    Exit For
                End If
            Next
        End If

        'Continue
        AddHandler PSXDatacenterBrowser.DocumentCompleted, AddressOf PSXDatacenterBrowser_DocumentCompleted

        If CurrentURL < URLs.Count Then
            PSXDatacenterBrowser.Navigate(URLs.Item(CurrentURL))
            CurrentURL += 1
            NewLoadingWindow.LoadProgressBar.Value = CurrentURL
        Else
            CurrentURL = 0
            URLs.Clear()
            NewLoadingWindow.Close()
            GamesListView.Items.Refresh()
        End If
    End Sub

#End Region

#Region "Menu Actions"

    Private Sub LoadFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadFolderMenuItem.Click
        Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Select your PS2 backup folder"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            ISOCount = Directory.GetFiles(FBD.SelectedPath, "*.iso", SearchOption.AllDirectories).Count
            CSOCount = Directory.GetFiles(FBD.SelectedPath, "*.cso", SearchOption.AllDirectories).Count

            NewLoadingWindow = New SyncWindow() With {.Title = "Loading PS2 files", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.Maximum = ISOCount + CSOCount
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + (ISOCount + CSOCount).ToString()
            NewLoadingWindow.Show()

            GameLoaderWorker.RunWorkerAsync(FBD.SelectedPath)
        End If
    End Sub

    Private Sub LoadDLFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadDLFolderMenuItem.Click
        If Directory.Exists(My.Computer.FileSystem.CurrentDirectory + "\Downloads") Then
            Process.Start(My.Computer.FileSystem.CurrentDirectory + "\Downloads")
        End If
    End Sub

#End Region

#Region "Contextmenu Actions"

    Private Sub SendToMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles SendToMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS2Game As PS2Game = CType(GamesListView.SelectedItem, PS2Game)
            Dim NewPS5Sender As New PS5Sender With {.SelectedISO = SelectedPS2Game.GameFilePath}
            NewPS5Sender.Show()
            MsgBox("Please continue with the PS5 Mast1c0re Sender and send the Network GAME Loader for your PS4/PS5 first.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub CopyToMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles CopyToMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS2Game As PS2Game = CType(GamesListView.SelectedItem, PS2Game)
            Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Where do you want to save the selected game ?"}

            If FBD.ShowDialog() = Forms.DialogResult.OK Then
                Dim NewCopyWindow As New CopyWindow() With {.ShowActivated = True,
                    .WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    .BackupPath = SelectedPS2Game.GameFilePath,
                    .BackupDestinationPath = FBD.SelectedPath + "\",
                    .Title = "Copying " + SelectedPS2Game.GameTitle + " to " + FBD.SelectedPath}

                If NewCopyWindow.ShowDialog() = True Then
                    MsgBox("Game copied with success !", MsgBoxStyle.Information, "Completed")
                End If
            End If

        End If
    End Sub

    Private Sub PlayGameMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PlayGameMenuItem.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\PCSX2\pcsx2.exe") Then
            If GamesListView.SelectedItem IsNot Nothing Then
                Dim SelectedPS2Game As PS2Game = CType(GamesListView.SelectedItem, PS2Game)

                'Check if any PS2 BIOS file is available
                If Not Directory.GetFiles(My.Computer.FileSystem.CurrentDirectory + "\Emulators\PCSX2\bios", "*.bin", SearchOption.TopDirectoryOnly).Count > 0 Then
                    If MsgBox("No PS2 BIOS file available." + vbCrLf + "You need at least one BIOS file installed in order to play " + SelectedPS2Game.GameTitle + "." + vbCrLf +
                              "Do you want to copy a BIOS file to the Emulators folder of PS Multi Tools ?", MsgBoxStyle.YesNo, "Cannot launch game") = MsgBoxResult.Yes Then

                        'Get a BIOS file from OpenFileDialog
                        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Select a PS2 BIOS file", .Filter = "PS2 BIOS (*.bin)|*.bin", .Multiselect = False}
                        If OFD.ShowDialog() = Forms.DialogResult.OK Then
                            Dim SelectedBIOSFile As String = OFD.FileName
                            Dim SelectedBIOSFileName As String = Path.GetFileName(SelectedBIOSFile)

                            'Copy to the BIOS folder
                            File.Copy(SelectedBIOSFile, My.Computer.FileSystem.CurrentDirectory + "\Emulators\PCSX2\bios\" + SelectedBIOSFileName, True)

                            'Proceed
                            If MsgBox("Start " + SelectedPS2Game.GameTitle + " using PCSX2 ?", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then
                                Dim EmulatorLauncherStartInfo As New ProcessStartInfo()
                                Dim EmulatorLauncher As New Process() With {.StartInfo = EmulatorLauncherStartInfo}
                                EmulatorLauncherStartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Emulators\PCSX2\pcsx2.exe"
                                EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(My.Computer.FileSystem.CurrentDirectory + "\Emulators\PCSX2\pcsx2.exe")
                                EmulatorLauncherStartInfo.Arguments = """" + SelectedPS2Game.GameFilePath + """ --nogui --fullboot --portable"
                                EmulatorLauncher.Start()
                            End If

                        Else
                            MsgBox("No BIOS file specied, aborting.", MsgBoxStyle.Critical, "Error")
                            Exit Sub
                        End If
                    Else
                        MsgBox("No BIOS file available, aborting.", MsgBoxStyle.Critical, "Error")
                        Exit Sub
                    End If

                Else
                    'Proceed
                    If MsgBox("Start " + SelectedPS2Game.GameTitle + " using PCSX2 ?", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then
                        Dim EmulatorLauncherStartInfo As New ProcessStartInfo()
                        Dim EmulatorLauncher As New Process() With {.StartInfo = EmulatorLauncherStartInfo}
                        EmulatorLauncherStartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Emulators\PCSX2\pcsx2.exe"
                        EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(My.Computer.FileSystem.CurrentDirectory + "\Emulators\PCSX2\pcsx2.exe")
                        EmulatorLauncherStartInfo.Arguments = """" + SelectedPS2Game.GameFilePath + """ --nogui --fullboot --portable"
                        EmulatorLauncher.Start()
                    End If

                End If
            End If
        Else
            MsgBox("Cannot start pcsx2." + vbCrLf + "Emulator pack is not installed.", MsgBoxStyle.Critical, "Error")
        End If
    End Sub

#End Region

    Private Sub GamesListView_PreviewMouseWheel(sender As Object, e As MouseWheelEventArgs) Handles GamesListView.PreviewMouseWheel
        Dim OpenWindowsListViewScrollViewer As ScrollViewer = Utils.FindScrollViewer(GamesListView)
        Dim HorizontalOffset As Double = OpenWindowsListViewScrollViewer.HorizontalOffset
        OpenWindowsListViewScrollViewer.ScrollToHorizontalOffset(HorizontalOffset - (e.Delta / 100))
        e.Handled = True
    End Sub

    Private Sub GamesListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles GamesListView.SelectionChanged
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS2Game As PS2Game = CType(GamesListView.SelectedItem, PS2Game)

            GameTitleTextBlock.Text = SelectedPS2Game.GameTitle
            GameIDTextBlock.Text = "Title ID: " & SelectedPS2Game.GameID
            GameRegionTextBlock.Text = "Region: " & SelectedPS2Game.GameRegion
            GameGenreTextBlock.Text = "Genre: " & SelectedPS2Game.GameGenre
            GameDeveloperTextBlock.Text = "Developer: " & SelectedPS2Game.GameDeveloper

            GameDescriptionTextBlock.Text = "Hover for Game Description"
            GameDescriptionTextBlock.ToolTip = SelectedPS2Game.GameDescription

            GameSizeTextBlock.Text = "Size: " & SelectedPS2Game.GameSize
            GamePublisherTextBlock.Text = "Publisher: " & SelectedPS2Game.GamePublisher
            GameReleaseDateTextBlock.Text = "Release Date: " & SelectedPS2Game.GameReleaseDate
            GameBackupTypeTextBlock.Text = "Backup Type: " & SelectedPS2Game.GameBackupType.ToString()

            If Not String.IsNullOrEmpty(SelectedPS2Game.GameFilePath) Then
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " & New DirectoryInfo(Path.GetDirectoryName(SelectedPS2Game.GameFilePath)).Name
            Else
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " & New DirectoryInfo(SelectedPS2Game.GameFolderPath).Name
            End If

        End If
    End Sub

    Private Sub EMU_Settings_Click(sender As Object, e As RoutedEventArgs) Handles EMU_Settings.Click
        Dim NewPS2EmulatorSettingsWindow As New PS2EmulatorSettings() With {.ShowActivated = True}
        NewPS2EmulatorSettingsWindow.Show()
    End Sub

    Private Sub CreateProjectMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles CreateProjectMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS2Game As PS2Game = CType(GamesListView.SelectedItem, PS2Game)
            Dim GameProjectDirectory As String = SelectedPS2Game.GameTitle + " [" + SelectedPS2Game.GameID + "]"
            Dim NewGameProjectDirectory As String = My.Computer.FileSystem.CurrentDirectory + "\Projects\" + SelectedPS2Game.GameTitle + " [" + SelectedPS2Game.GameID + "]"

            Dim NewGameProjectWindow As New PSXNewPS2GameProject() With {.ShowActivated = True}
            Dim NewGameEditor As New PSXPS2GameEditor() With {.ProjectDirectory = NewGameProjectDirectory, .Title = "Game Ressources Editor - " + NewGameProjectDirectory}

            'Set project information
            NewGameProjectWindow.ImportFromPSMT(SelectedPS2Game.GameFilePath, SelectedPS2Game.GameTitle, NewGameProjectDirectory, SelectedPS2Game.GameID.Replace("-", "_").Insert(8, "."))

            'Create game project directory
            If Not Directory.Exists(NewGameProjectDirectory) Then
                Directory.CreateDirectory(NewGameProjectDirectory)
            End If

            'Write Project settings to .CFG
            Using ProjectWriter As New StreamWriter(My.Computer.FileSystem.CurrentDirectory + "\Projects\" + SelectedPS2Game.GameTitle + ".CFG", False)
                ProjectWriter.WriteLine("TITLE=" + SelectedPS2Game.GameTitle)
                ProjectWriter.WriteLine("ID=" + SelectedPS2Game.GameID.Replace("-", "_").Insert(8, "."))
                ProjectWriter.WriteLine("DIR=" + NewGameProjectDirectory)
                ProjectWriter.WriteLine("ELForISO=" + SelectedPS2Game.GameFilePath)
                ProjectWriter.WriteLine("TYPE=GAME")
                ProjectWriter.WriteLine("SIGNED=FALSE")
                ProjectWriter.WriteLine("GAMETYPE=PS2")
            End Using

            'Write SYSTEM.CNF to project directory
            Using CNFWriter As New StreamWriter(NewGameProjectDirectory + "\SYSTEM.CNF", False)
                CNFWriter.WriteLine("BOOT2 = pfs:/EXECUTE.KELF") 'Loads EXECUTE.KELF
                CNFWriter.WriteLine("VER = 1.01")
                CNFWriter.WriteLine("VMODE = NTSC")
                CNFWriter.WriteLine("HDDUNITPOWER = NICHDD")
            End Using

            'Write icon.sys to project directory
            Using CNFWriter As New StreamWriter(NewGameProjectDirectory + "\icon.sys", False)
                CNFWriter.WriteLine("PS2X")
                CNFWriter.WriteLine("title0=" + SelectedPS2Game.GameTitle)
                CNFWriter.WriteLine("title1=" + SelectedPS2Game.GameID)
                CNFWriter.WriteLine("bgcola=0")
                CNFWriter.WriteLine("bgcol0=0,0,0")
                CNFWriter.WriteLine("bgcol1=0,0,0")
                CNFWriter.WriteLine("bgcol2=0,0,0")
                CNFWriter.WriteLine("bgcol3=0,0,0")
                CNFWriter.WriteLine("lightdir0=1.0,-1.0,1.0")
                CNFWriter.WriteLine("lightdir1=-1.0,1.0,-1.0")
                CNFWriter.WriteLine("lightdir2=0.0,0.0,0.0")
                CNFWriter.WriteLine("lightcolamb=64,64,64")
                CNFWriter.WriteLine("lightcol0=64,64,64")
                CNFWriter.WriteLine("lightcol1=16,16,16")
                CNFWriter.WriteLine("lightcol2=0,0,0")
                CNFWriter.WriteLine("uninstallmes0=Do you want to uninstall this game ?")
                CNFWriter.WriteLine("uninstallmes1=")
                CNFWriter.WriteLine("uninstallmes2=")
            End Using

            'Create game project res & image directory
            If Not Directory.Exists(NewGameProjectDirectory + "\res") Then
                Directory.CreateDirectory(NewGameProjectDirectory + "\res")
            End If
            If Not Directory.Exists(NewGameProjectDirectory + "\res\image") Then
                Directory.CreateDirectory(NewGameProjectDirectory + "\res\image")
            End If

            'Write info.sys to res directory
            Using SYSWriter As New StreamWriter(NewGameProjectDirectory + "\res\info.sys", False)
                SYSWriter.WriteLine("title = " + SelectedPS2Game.GameTitle)
                SYSWriter.WriteLine("title_id = " + SelectedPS2Game.GameID)
                SYSWriter.WriteLine("title_sub_id = 0")
                SYSWriter.WriteLine("release_date = " + SelectedPS2Game.GameReleaseDate)
                SYSWriter.WriteLine("developer_id = " + SelectedPS2Game.GameDeveloper)
                SYSWriter.WriteLine("publisher_id = " + SelectedPS2Game.GamePublisher)
                SYSWriter.WriteLine("note = ")
                SYSWriter.WriteLine("content_web = " + SelectedPS2Game.GameWebsite)
                SYSWriter.WriteLine("image_topviewflag = 0")
                SYSWriter.WriteLine("image_type = 0")
                SYSWriter.WriteLine("image_count = 1")
                SYSWriter.WriteLine("image_viewsec = 600")
                SYSWriter.WriteLine("copyright_viewflag = 0")
                SYSWriter.WriteLine("copyright_imgcount = 1")
                SYSWriter.WriteLine("genre = " + SelectedPS2Game.GameGenre)
                SYSWriter.WriteLine("parental_lock = 1")
                SYSWriter.WriteLine("effective_date = 0")
                SYSWriter.WriteLine("expire_date = 0")

                Select Case SelectedPS2Game.GameRegion
                    Case "Europe"
                        SYSWriter.WriteLine("area = E")
                    Case "US"
                        SYSWriter.WriteLine("area = U")
                    Case "Japan"
                        SYSWriter.WriteLine("area = J")
                    Case Else
                        SYSWriter.WriteLine("area = J")
                End Select

                SYSWriter.WriteLine("violence_flag = 0")
                SYSWriter.WriteLine("content_type = 255")
                SYSWriter.WriteLine("content_subtype = 0")
            End Using

            'Create man.xml
            Using MANWriter As New StreamWriter(NewGameProjectDirectory + "\res\man.xml", False)
                MANWriter.WriteLine("<?xml version=""1.0"" encoding=""UTF-8""?>")
                MANWriter.WriteLine("")
                MANWriter.WriteLine("<MANUAL version=""1.0"">")
                MANWriter.WriteLine("")
                MANWriter.WriteLine("<IMG id=""bg"" src=""./image/0.png"" />")
                MANWriter.WriteLine("")
                MANWriter.WriteLine("<MENUGROUP id=""TOP"">")
                MANWriter.WriteLine("<TITLE id=""TOP-TITLE"" label=""" + SelectedPS2Game.GameTitle + """ />")
                MANWriter.WriteLine("<ITEM id=""M00"" label=""Screenshots""	page=""PIC0000"" />")
                MANWriter.WriteLine("</MENUGROUP>")
                MANWriter.WriteLine("")
                MANWriter.WriteLine("<PAGEGROUP>")
                MANWriter.WriteLine("<PAGE id=""PIC0000"" src=""./image/1.png"" retitem=""M00"" retgroup=""TOP"" />")
                MANWriter.WriteLine("<PAGE id=""PIC0000"" src=""./image/2.png"" retitem=""M00"" retgroup=""TOP"" />")
                MANWriter.WriteLine("</PAGEGROUP>")
                MANWriter.WriteLine("</MANUAL>")
                MANWriter.WriteLine("")
            End Using

            'Open project settings window
            NewGameProjectWindow.Show()

            'Open the Game Editor (in case of additional changes)
            NewGameEditor.Show()
            NewGameEditor.AutoSave = True

            'Open the Game Editor and try to load values from PSXDatacenter
            If Utils.IsURLValid("https://psxdatacenter.com/psx2/games2/" + SelectedPS2Game.GameID + ".html") Then
                NewGameEditor.PSXDatacenterBrowser.Navigate("https://psxdatacenter.com/psx2/games2/" + SelectedPS2Game.GameID + ".html")
            Else
                'Apply cover, title and region only if no data is available on PSXDatacenter
                NewGameEditor.ApplyKnownValues(SelectedPS2Game.GameID, SelectedPS2Game.GameTitle)
            End If

        End If
    End Sub

End Class
