Imports System.ComponentModel
Imports System.IO
Imports System.Windows.Media.Animation
Imports PS4_Tools

Public Class PS4Library

    Dim WithEvents GameLoaderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}
    Dim WithEvents NewLoadingWindow As New SyncWindow() With {.Title = "Loading PS4 pkg files", .ShowActivated = True}

    'Selected game context menu
    Dim WithEvents NewContextMenu As New ContextMenu()
    Dim WithEvents CopyToMenuItem As New Controls.MenuItem() With {.Header = "Copy to", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/copy-icon.png", UriKind.Relative))}}
    Dim WithEvents ExtractPKGMenuItem As New Controls.MenuItem() With {.Header = "Extract .pkg", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/extract.png", UriKind.Relative))}}
    Dim WithEvents PKGInfoMenuItem As New Controls.MenuItem() With {.Header = "PKG Details", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/information-button.png", UriKind.Relative))}}
    Dim WithEvents PSNInfoMenuItem As New Controls.MenuItem() With {.Header = "Store Details", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/information-button.png", UriKind.Relative))}}
    Dim WithEvents PlayMenuItem As New Controls.MenuItem() With {.Header = "Play Soundtrack", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Play-icon.png", UriKind.Relative))}}
    Dim WithEvents PlayGameMenuItem As New Controls.MenuItem() With {.Header = "Play with psOff", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/controller.png", UriKind.Relative))}}

    Dim PKGCount As Integer = 0
    Dim FoldersCount As Integer = 0
    Dim IsSoundPlaying As Boolean = False

    'Supplemental library menu items
    Dim WithEvents LoadFolderMenuItem As New Controls.MenuItem() With {.Header = "Load a new folder"}
    Dim WithEvents LoadDLFolderMenuItem As New Controls.MenuItem() With {.Header = "Open Downloads folder"}

    'Supplemental emulator menu item
    Dim WithEvents EMU_Settings As New Controls.MenuItem() With {.Header = "psOff Settings"}

    Private Sub PS4Library_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Add supplemental library menu items that will be handled in the app
        Dim LibraryMenuItem As MenuItem = CType(NewPS4Menu.Items(0), MenuItem)
        LibraryMenuItem.Items.Add(LoadFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem)

        'Add the games context menu
        GamesListView.ContextMenu = NewContextMenu

        'Add supplemental emulator menu item
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\psOff\psoff.exe") Then
            NewPS4Menu.Items.Add(EMU_Settings)
        End If
    End Sub

#Region "Game Loader"

    Private Sub GameLoaderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles GameLoaderWorker.DoWork

        'PS4 PKGs
        For Each Game In Directory.GetFiles(e.Argument.ToString, "*.pkg", SearchOption.AllDirectories)
            Dim NewPS4Game As New PS4Game()
            Dim GamePKG As PKG.SceneRelated.Unprotected_PKG = PKG.SceneRelated.Read_PKG(Game)

            'Set game infos
            NewPS4Game.GameTitle = GamePKG.PS4_Title
            NewPS4Game.GameContentID = GamePKG.Content_ID
            NewPS4Game.GameFilePath = Game
            NewPS4Game.GameRegion = GamePKG.Region.Replace("(", "").Replace(")", "").Trim()
            NewPS4Game.GameRequiredFW = GamePKG.Firmware_Version
            NewPS4Game.GameSize = GamePKG.Size
            NewPS4Game.GameAppVer = GamePKG.Param.APP_VER
            NewPS4Game.GameFileType = PS4Game.GameFileTypes.PKG

            If GamePKG.Param IsNot Nothing Then
                If Not String.IsNullOrEmpty(GamePKG.Param.Category) Then
                    NewPS4Game.GameCategory = PS4Game.GetCategory(GamePKG.Param.Category)
                End If
                If Not String.IsNullOrEmpty(GamePKG.Param.APP_VER) Then
                    NewPS4Game.GameAppVer = GamePKG.Param.APP_VER
                End If

                For Each TableRow In GamePKG.Param.Tables
                    If TableRow.Name = "TITLE_ID" Then
                        NewPS4Game.GameID = TableRow.Value
                    End If
                    If TableRow.Name = "VERSION" Then
                        NewPS4Game.GameVer = TableRow.Value
                    End If
                Next

            End If

            If GamePKG.Icon IsNot Nothing Then
                Dispatcher.BeginInvoke(Sub() NewPS4Game.GameCoverSource = Utils.BitmapSourceFromByteArray(GamePKG.Icon))
            End If
            If GamePKG.Image IsNot Nothing Then
                Dispatcher.BeginInvoke(Sub() NewPS4Game.GameBackgroundSource = Utils.BitmapSourceFromByteArray(GamePKG.Image))
            End If
            If GamePKG.Sound IsNot Nothing Then
                NewPS4Game.GameSoundtrackBytes = Media.Atrac9.LoadAt9(GamePKG.Sound)
            End If

            Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
            Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading PKG " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + PKGCount.ToString())

            'Add to the ListView
            Select Case GamePKG.Param.Category
                Case "ac"
                    If DLCsListView.Dispatcher.CheckAccess() = False Then
                        DLCsListView.Dispatcher.BeginInvoke(Sub() DLCsListView.Items.Add(NewPS4Game))
                    Else
                        DLCsListView.Items.Add(NewPS4Game)
                    End If
                Case "gd"
                    If GamesListView.Dispatcher.CheckAccess() = False Then
                        GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPS4Game))
                    Else
                        GamesListView.Items.Add(NewPS4Game)
                    End If
                Case "gp"
                    If UpdatesListView.Dispatcher.CheckAccess() = False Then
                        UpdatesListView.Dispatcher.BeginInvoke(Sub() UpdatesListView.Items.Add(NewPS4Game))
                    Else
                        UpdatesListView.Items.Add(NewPS4Game)
                    End If
                Case Else
                    If OthersListView.Dispatcher.CheckAccess() = False Then
                        OthersListView.Dispatcher.BeginInvoke(Sub() OthersListView.Items.Add(NewPS4Game))
                    Else
                        OthersListView.Items.Add(NewPS4Game)
                    End If
            End Select
        Next

        'PS4 Backup folders
        For Each Game In Directory.GetFiles(e.Argument.ToString, "*.sfo", SearchOption.AllDirectories)
            Dim NewPS4Game As New PS4Game()

            Using SFOReader As New Process()
                SFOReader.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\sfo.exe"
                SFOReader.StartInfo.Arguments = """" + Game + """"
                SFOReader.StartInfo.RedirectStandardOutput = True
                SFOReader.StartInfo.UseShellExecute = False
                SFOReader.StartInfo.CreateNoWindow = True
                SFOReader.Start()

                Dim OutputReader As StreamReader = SFOReader.StandardOutput
                Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
                If ProcessOutput.Count > 0 Then

                    For Each Line In ProcessOutput
                        If Line.StartsWith("TITLE=") Then
                            NewPS4Game.GameTitle = Utils.CleanTitle(Line.Split("="c)(1).Trim(""""c).Trim())
                        ElseIf Line.StartsWith("TITLE_ID=") Then
                            NewPS4Game.GameID = Line.Split("="c)(1).Trim(""""c).Trim()
                        ElseIf Line.StartsWith("CATEGORY=") Then
                            NewPS4Game.GameCategory = Line.Split("="c)(1).Trim(""""c)
                        ElseIf Line.StartsWith("APP_VER=") Then
                            NewPS4Game.GameAppVer = Line.Split("="c)(1).Trim(""""c).Trim()
                        ElseIf Line.StartsWith("SYSTEM_VER=") Then
                            NewPS4Game.GameRequiredFW = Line.Split("="c)(1).Trim(""""c).Trim().Replace("0x0", "").Insert(1, ".").Insert(5, ".")
                        ElseIf Line.StartsWith("VERSION=") Then
                            NewPS4Game.GameVer = Line.Split("="c)(1).Trim(""""c).Trim()
                        ElseIf Line.StartsWith("CONTENT_ID=") Then
                            NewPS4Game.GameContentID = Line.Split("="c)(1).Trim(""""c).Trim()
                        End If
                    Next

                    Dim PSVGAMEFolder As String = Path.GetDirectoryName(Directory.GetParent(Game).FullName)
                    Dim PSVGAMEFolderSize As Long = Utils.DirSize(PSVGAMEFolder, True)

                    NewPS4Game.GameSize = FormatNumber(PSVGAMEFolderSize / 1073741824, 2) + " GB"
                    NewPS4Game.GameFolderPath = PSVGAMEFolder
                    NewPS4Game.GameFileType = PS4Game.GameFileTypes.Backup

                    'Load icon, background & sound if available
                    If File.Exists(PSVGAMEFolder + "\sce_sys\icon0.png") Then
                        If Dispatcher.CheckAccess() = False Then
                            Dispatcher.BeginInvoke(Sub()
                                                       Dim TempBitmapImage = New BitmapImage()
                                                       TempBitmapImage.BeginInit()
                                                       TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                       TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                       TempBitmapImage.UriSource = New Uri(PSVGAMEFolder + "\sce_sys\icon0.png", UriKind.RelativeOrAbsolute)
                                                       TempBitmapImage.EndInit()
                                                       NewPS4Game.GameCoverSource = TempBitmapImage
                                                   End Sub)
                        Else
                            Dim TempBitmapImage = New BitmapImage()
                            TempBitmapImage.BeginInit()
                            TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                            TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                            TempBitmapImage.UriSource = New Uri(PSVGAMEFolder + "\sce_sys\icon0.png", UriKind.RelativeOrAbsolute)
                            TempBitmapImage.EndInit()
                            NewPS4Game.GameCoverSource = TempBitmapImage
                        End If
                    End If
                    If File.Exists(PSVGAMEFolder + "\sce_sys\pic1.png") Then
                        If Dispatcher.CheckAccess() = False Then
                            Dispatcher.BeginInvoke(Sub()
                                                       Dim TempBitmapImage = New BitmapImage()
                                                       TempBitmapImage.BeginInit()
                                                       TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                       TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                       TempBitmapImage.UriSource = New Uri(PSVGAMEFolder + "\sce_sys\pic1.png", UriKind.RelativeOrAbsolute)
                                                       TempBitmapImage.EndInit()
                                                       NewPS4Game.GameBackgroundSource = TempBitmapImage
                                                   End Sub)
                        Else
                            Dim TempBitmapImage = New BitmapImage()
                            TempBitmapImage.BeginInit()
                            TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                            TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                            TempBitmapImage.UriSource = New Uri(PSVGAMEFolder + "\sce_sys\pic1.png", UriKind.RelativeOrAbsolute)
                            TempBitmapImage.EndInit()
                            NewPS4Game.GameBackgroundSource = TempBitmapImage
                        End If
                    End If
                    If File.Exists(PSVGAMEFolder + "\sce_sys\snd0.at9") Then
                        NewPS4Game.GameSoundtrackBytes = Media.Atrac9.LoadAt9(PSVGAMEFolder + "\sce_sys\snd0.at9")
                    End If

                    Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                    Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading backup folder " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FoldersCount.ToString())

                    'Add to the ListView
                    Select Case NewPS4Game.GameCategory
                        Case "ac"
                            If DLCsListView.Dispatcher.CheckAccess() = False Then
                                DLCsListView.Dispatcher.BeginInvoke(Sub() DLCsListView.Items.Add(NewPS4Game))
                            Else
                                DLCsListView.Items.Add(NewPS4Game)
                            End If
                        Case "gd"
                            If GamesListView.Dispatcher.CheckAccess() = False Then
                                GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPS4Game))
                            Else
                                GamesListView.Items.Add(NewPS4Game)
                            End If
                        Case "gp"
                            If UpdatesListView.Dispatcher.CheckAccess() = False Then
                                UpdatesListView.Dispatcher.BeginInvoke(Sub() UpdatesListView.Items.Add(NewPS4Game))
                            Else
                                UpdatesListView.Items.Add(NewPS4Game)
                            End If
                        Case Else
                            If OthersListView.Dispatcher.CheckAccess() = False Then
                                OthersListView.Dispatcher.BeginInvoke(Sub() OthersListView.Items.Add(NewPS4Game))
                            Else
                                OthersListView.Items.Add(NewPS4Game)
                            End If
                    End Select

                End If

            End Using

        Next

    End Sub

    Private Sub GameLoaderWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles GameLoaderWorker.RunWorkerCompleted
        NewLoadingWindow.Close()
    End Sub

#End Region

    Private Sub GamesListView_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs) Handles GamesListView.ContextMenuOpening
        NewContextMenu.Items.Clear()

        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS3Game As PS4Game = CType(GamesListView.SelectedItem, PS4Game)

            NewContextMenu.Items.Add(CopyToMenuItem)

            Select Case SelectedPS3Game.GameFileType
                Case PS4Game.GameFileTypes.Backup
                    NewContextMenu.Items.Add(PlayMenuItem)
                    NewContextMenu.Items.Add(PlayGameMenuItem)
                Case PS4Game.GameFileTypes.PKG
                    NewContextMenu.Items.Add(ExtractPKGMenuItem)
                    NewContextMenu.Items.Add(PKGInfoMenuItem)
                    NewContextMenu.Items.Add(PlayMenuItem)
                    NewContextMenu.Items.Add(PSNInfoMenuItem)
            End Select

        End If
    End Sub

    Private Sub GamesListView_ContextMenuClosing(sender As Object, e As ContextMenuEventArgs) Handles GamesListView.ContextMenuClosing
        NewContextMenu.Items.Clear()
    End Sub

#Region "Contextmenu Actions"

    Private Sub CopyToMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles CopyToMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS4Game As PS4Game = CType(GamesListView.SelectedItem, PS4Game)
            Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Where do you want to copy the selected game ?"}

            If FBD.ShowDialog() = Forms.DialogResult.OK Then
                Dim NewCopyWindow As New CopyWindow() With {.ShowActivated = True,
                    .WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    .BackupDestinationPath = FBD.SelectedPath + "\",
                    .Title = "Copying " + SelectedPS4Game.GameTitle + " to " + FBD.SelectedPath + "\" + Path.GetFileName(SelectedPS4Game.GameFilePath)}

                If SelectedPS4Game.GameFileType = PS4Game.GameFileTypes.Backup Then
                    NewCopyWindow.BackupPath = SelectedPS4Game.GameFolderPath
                ElseIf SelectedPS4Game.GameFileType = PS4Game.GameFileTypes.PKG Then
                    NewCopyWindow.BackupPath = SelectedPS4Game.GameFilePath
                End If

                If SelectedPS4Game.GameCoverSource IsNot Nothing Then
                    NewCopyWindow.GameIcon = SelectedPS4Game.GameCoverSource
                End If

                If NewCopyWindow.ShowDialog() = True Then
                    MsgBox("Game copied with success !", MsgBoxStyle.Information, "Completed")
                End If
            End If

        End If
    End Sub

    Private Sub PlayMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PlayMenuItem.Click
        If IsSoundPlaying = True Then
            Utils.StopSND()
            IsSoundPlaying = False

            PlayMenuItem.Header = "Play Soundtrack"
        Else
            If GamesListView.SelectedItem IsNot Nothing Then
                Dim SelectedPS4Game As PS4Game = CType(GamesListView.SelectedItem, PS4Game)
                If SelectedPS4Game.GameSoundtrackBytes IsNot Nothing Then
                    Utils.PlaySND(SelectedPS4Game.GameSoundtrackBytes)
                    IsSoundPlaying = True

                    PlayMenuItem.Header = "Stop Soundtrack"
                Else
                    MsgBox("Selected game does not have a soundtrack.", MsgBoxStyle.Information, "Soundtrack not found")
                End If
            End If
        End If
    End Sub

    Private Sub PSNInfoMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PSNInfoMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS4Game As PS4Game = CType(GamesListView.SelectedItem, PS4Game)
            If Not String.IsNullOrEmpty(SelectedPS4Game.GameContentID) Then
                Dim NewPSNInfo As New PSNInfo() With {.ShowActivated = True, .CurrentGameContentID = SelectedPS4Game.GameContentID}
                NewPSNInfo.Show()
            Else
                MsgBox("Selected game requires a Content ID to display infos from PSN.", MsgBoxStyle.Information)
            End If
        End If
    End Sub

    Private Sub PKGInfoMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PKGInfoMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS4Game As PS4Game = CType(GamesListView.SelectedItem, PS4Game)
            Dim NewPKGInfo As New PKGInfo() With {.SelectedPKG = SelectedPS4Game.GameFilePath, .Console = "PS4"}
            NewPKGInfo.Show()
        End If
    End Sub

    Private Sub PlayGameMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PlayGameMenuItem.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\psOff\psoff.exe") Then
            If GamesListView.SelectedItem IsNot Nothing Then
                Dim SelectedPS4Game As PS4Game = CType(GamesListView.SelectedItem, PS4Game)
                If SelectedPS4Game.GameFileType = PS4Game.GameFileTypes.Backup Then

                    If MsgBox("Start " + SelectedPS4Game.GameTitle + " using psOff ?", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then
                        Dim EmulatorLauncherStartInfo As New ProcessStartInfo()
                        Dim EmulatorLauncher As New Process() With {.StartInfo = EmulatorLauncherStartInfo}
                        EmulatorLauncherStartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Emulators\psOff\psoff.exe"
                        EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(My.Computer.FileSystem.CurrentDirectory + "\Emulators\psOff\psoff.exe")
                        EmulatorLauncherStartInfo.Arguments = "--file """ + SelectedPS4Game.GameFolderPath + "\eboot.bin"""

                        EmulatorLauncher.Start()
                    End If

                Else
                    MsgBox("PKG files are not supported yet." + vbCrLf + "Please extract the .pkg file before running with psOff.", MsgBoxStyle.Information, "Cannot launch PKG")
                End If
            End If
        Else
            MsgBox("Cannot start psoff." + vbCrLf + "Emulator pack is not installed.", MsgBoxStyle.Critical, "Error")
        End If
    End Sub

    Private Sub ExtractPKGMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles ExtractPKGMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS4Game As PS4Game = CType(GamesListView.SelectedItem, PS4Game)
            Dim NewPKGExtractor As New PS4PKGExtractor() With {.ShowActivated = True, .PKGToExtract = SelectedPS4Game.GameFilePath}
            NewPKGExtractor.Show()
        End If
    End Sub

#End Region

#Region "Menu Actions"

    Private Sub LoadFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadFolderMenuItem.Click
        Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Select your PS4 backup folder"}

        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            'Set the count of pkg files & backup folders
            PKGCount = Directory.GetFiles(FBD.SelectedPath, "*.pkg", SearchOption.AllDirectories).Count
            FoldersCount = Directory.GetFiles(FBD.SelectedPath, "*.sfo", SearchOption.AllDirectories).Count

            'Show the loading progress window
            NewLoadingWindow = New SyncWindow() With {.Title = "Loading PS4 files", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.Maximum = PKGCount + FoldersCount
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + (PKGCount + FoldersCount).ToString()
            NewLoadingWindow.Show()

            'Load the pkg files
            GameLoaderWorker.RunWorkerAsync(FBD.SelectedPath)
        End If
    End Sub

    Private Sub LoadDLFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadDLFolderMenuItem.Click
        If Directory.Exists(My.Computer.FileSystem.CurrentDirectory + "\Downloads") Then
            Process.Start(My.Computer.FileSystem.CurrentDirectory + "\Downloads")
        End If
    End Sub

#End Region

    Private Sub GamesListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles GamesListView.SelectionChanged
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS4Game As PS4Game = CType(GamesListView.SelectedItem, PS4Game)

            GameTitleTextBlock.Text = SelectedPS4Game.GameTitle
            GameIDTextBlock.Text = "Title ID: " & SelectedPS4Game.GameID
            GameContentIDTextBlock.Text = "Content ID: " & SelectedPS4Game.GameContentID
            GameRegionTextBlock.Text = "Region: " & SelectedPS4Game.GameRegion
            GameVersionTextBlock.Text = "Game Version: " & SelectedPS4Game.GameVer
            GameAppVersionTextBlock.Text = "Application Version: " & SelectedPS4Game.GameAppVer
            GameCategoryTextBlock.Text = "Category: " & SelectedPS4Game.GameCategory
            GameSizeTextBlock.Text = "Size: " & SelectedPS4Game.GameSize
            GameRequiredFirmwareTextBlock.Text = "Required Firmware: " & SelectedPS4Game.GameRequiredFW

            If Not String.IsNullOrEmpty(SelectedPS4Game.GameFilePath) Then
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " & New DirectoryInfo(Path.GetDirectoryName(SelectedPS4Game.GameFilePath)).Name
            Else
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " & New DirectoryInfo(SelectedPS4Game.GameFolderPath).Name
            End If

            If SelectedPS4Game.GameBackgroundSource IsNot Nothing Then
                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub()
                                               RectangleImageBrush.ImageSource = SelectedPS4Game.GameBackgroundSource
                                               BlurringShape.BeginAnimation(OpacityProperty, New DoubleAnimation With {.From = 0, .To = 1, .Duration = New Duration(TimeSpan.FromMilliseconds(500))})
                                           End Sub)
                Else
                    RectangleImageBrush.ImageSource = SelectedPS4Game.GameBackgroundSource
                    BlurringShape.BeginAnimation(OpacityProperty, New DoubleAnimation With {.From = 0, .To = 1, .Duration = New Duration(TimeSpan.FromMilliseconds(500))})
                End If
            Else
                RectangleImageBrush.ImageSource = Nothing
            End If

            If SelectedPS4Game.GameSoundtrackBytes IsNot Nothing Then
                If IsSoundPlaying Then
                    Utils.StopSND()
                    IsSoundPlaying = False
                Else
                    Utils.PlaySND(SelectedPS4Game.GameSoundtrackBytes)
                    IsSoundPlaying = True
                End If
            Else
                If IsSoundPlaying Then
                    Utils.StopGameSound()
                    IsSoundPlaying = False
                End If
            End If
        End If
    End Sub

    Private Sub GamesListView_PreviewMouseWheel(sender As Object, e As MouseWheelEventArgs) Handles GamesListView.PreviewMouseWheel
        Dim OpenWindowsListViewScrollViewer As ScrollViewer = Utils.FindScrollViewer(GamesListView)
        Dim HorizontalOffset As Double = OpenWindowsListViewScrollViewer.HorizontalOffset
        OpenWindowsListViewScrollViewer.ScrollToHorizontalOffset(HorizontalOffset - (e.Delta / 100))
        e.Handled = True
    End Sub

    Private Sub EMU_Settings_Click(sender As Object, e As RoutedEventArgs) Handles EMU_Settings.Click
        Dim NewPS4EmulatorSettingsWindow As New PS4EmulatorSettings() With {.ShowActivated = True}
        NewPS4EmulatorSettingsWindow.Show()
    End Sub

End Class
