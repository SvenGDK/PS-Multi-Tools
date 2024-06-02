Imports System.ComponentModel
Imports System.IO
Imports System.Windows.Forms
Imports System.Windows.Media.Animation

Public Class PSPLibrary

    Dim WithEvents GameLoaderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}
    Dim WithEvents NewLoadingWindow As New SyncWindow() With {.Title = "Loading PSP files", .ShowActivated = True}

    Dim FoldersCount As Integer = 0
    Dim ISOCount As Integer = 0
    Dim IsSoundPlaying As Boolean = False

    'Selected game context menu
    Dim WithEvents NewContextMenu As New Controls.ContextMenu()
    Dim WithEvents CopyToMenuItem As New Controls.MenuItem() With {.Header = "Copy to", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/copy-icon.png", UriKind.Relative))}}
    Dim WithEvents PlayGameMenuItem As New Controls.MenuItem() With {.Header = "Play with PPSSPP", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/controller.png", UriKind.Relative))}}

    'Supplemental library menu items
    Dim WithEvents LoadFolderMenuItem As New Controls.MenuItem() With {.Header = "Load a new folder"}
    Dim WithEvents LoadDLFolderMenuItem As New Controls.MenuItem() With {.Header = "Open Downloads folder"}

    Private Sub PSPLibrary_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Add supplemental library menu items that will be handled in the app
        Dim LibraryMenuItem As Controls.MenuItem = CType(NewPSPMenu.Items(0), Controls.MenuItem)
        LibraryMenuItem.Items.Add(LoadFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem)

        NewContextMenu.Items.Add(CopyToMenuItem)
        NewContextMenu.Items.Add(PlayGameMenuItem)
        GamesListView.ContextMenu = NewContextMenu
    End Sub

#Region "Game Loader"

    Private Sub GameLoaderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles GameLoaderWorker.DoWork

        'PSP backup folders
        For Each Game In Directory.GetFiles(e.Argument.ToString, "*.SFO", SearchOption.AllDirectories)
            Dim NewPSPGame As New PSPGame()

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

                    'Load game infos
                    For Each Line In ProcessOutput
                        If Line.StartsWith("TITLE=") Then
                            NewPSPGame.GameTitle = Utils.CleanTitle(Line.Split("="c)(1).Trim(""""c))
                        ElseIf Line.StartsWith("DISC_ID=") Then
                            NewPSPGame.GameID = Line.Split("="c)(1).Trim(""""c)
                        ElseIf Line.StartsWith("CATEGORY=") Then
                            NewPSPGame.GameCategory = PSPGame.GetCategory(Line.Split("="c)(1).Trim(""""c))
                        ElseIf Line.StartsWith("DISC_VERSION=") Then
                            NewPSPGame.GameAppVer = FormatNumber(Line.Split("="c)(1).Trim(""""c), 0).Insert(1, ".")
                        ElseIf Line.StartsWith("PSP_SYSTEM_VER=") Then
                            NewPSPGame.GameRequiredFW = FormatNumber(Line.Split("="c)(1).Trim(""""c), 0).Insert(1, ".")
                        End If
                    Next

                    'Load game files
                    Dim PSPGAMEFolder As String = Path.GetDirectoryName(Game)
                    If File.Exists(PSPGAMEFolder + "\ICON0.PNG") Then
                        If Dispatcher.CheckAccess() = False Then
                            Dispatcher.BeginInvoke(Sub()
                                                       Dim TempBitmapImage = New BitmapImage()
                                                       TempBitmapImage.BeginInit()
                                                       TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                       TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                       TempBitmapImage.UriSource = New Uri(PSPGAMEFolder + "\ICON0.PNG", UriKind.RelativeOrAbsolute)
                                                       TempBitmapImage.EndInit()
                                                       NewPSPGame.GameCoverSource = TempBitmapImage
                                                   End Sub)
                        Else
                            Dim TempBitmapImage = New BitmapImage()
                            TempBitmapImage.BeginInit()
                            TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                            TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                            TempBitmapImage.UriSource = New Uri(PSPGAMEFolder + "\ICON0.PNG", UriKind.RelativeOrAbsolute)
                            TempBitmapImage.EndInit()
                            NewPSPGame.GameCoverSource = TempBitmapImage
                        End If
                    End If
                    If File.Exists(PSPGAMEFolder + "\PIC1.PNG") Then
                        If Dispatcher.CheckAccess() = False Then
                            Dispatcher.BeginInvoke(Sub()
                                                       Dim TempBitmapImage = New BitmapImage()
                                                       TempBitmapImage.BeginInit()
                                                       TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                       TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                       TempBitmapImage.UriSource = New Uri(PSPGAMEFolder + "\PIC1.PNG", UriKind.RelativeOrAbsolute)
                                                       TempBitmapImage.EndInit()
                                                       NewPSPGame.GameBackgroundSource = TempBitmapImage
                                                   End Sub)
                        Else
                            Dim TempBitmapImage = New BitmapImage()
                            TempBitmapImage.BeginInit()
                            TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                            TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                            TempBitmapImage.UriSource = New Uri(PSPGAMEFolder + "\PIC1.PNG", UriKind.RelativeOrAbsolute)
                            TempBitmapImage.EndInit()
                            NewPSPGame.GameBackgroundSource = TempBitmapImage
                        End If
                    End If
                    If File.Exists(PSPGAMEFolder + "\SND0.AT3") Then
                        NewPSPGame.GameBackgroundSoundFile = PSPGAMEFolder + "\SND0.AT3"
                    End If

                    Dim PSPGAMEFolderSize As Long = Utils.DirSize(PSPGAMEFolder, True)
                    NewPSPGame.GameSize = FormatNumber(PSPGAMEFolderSize / 1073741824, 2) + " GB"
                    NewPSPGame.GameFolderPath = Directory.GetParent(PSPGAMEFolder).FullName
                    NewPSPGame.GameFileType = PSPGame.GameFileTypes.Backup

                    If Not String.IsNullOrWhiteSpace(NewPSPGame.GameID) Then
                        NewPSPGame.GameRegion = PSPGame.GetGameRegion(NewPSPGame.GameID)
                    End If

                    Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                    Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading folder " + (NewLoadingWindow.LoadProgressBar.Value - ISOCount).ToString + " of " + FoldersCount.ToString())

                    'Add to the ListView
                    If GamesListView.Dispatcher.CheckAccess() = False Then
                        GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPSPGame))
                    Else
                        GamesListView.Items.Add(NewPSPGame)
                    End If

                End If

            End Using
        Next

        'PSP ISOs
        For Each GameISO In Directory.GetFiles(e.Argument.ToString, "*.iso", SearchOption.AllDirectories)

            Dim NewPSPGame As New PSPGame()
            Dim ISOFileInfo As New FileInfo(GameISO)
            Dim ISOCacheFolderName As String = Path.GetFileNameWithoutExtension(ISOFileInfo.Name)

            'Create cache dir for PSP games
            If Not Directory.Exists(My.Computer.FileSystem.CurrentDirectory + "\Cache\PSP\" + ISOCacheFolderName) Then
                Directory.CreateDirectory(My.Computer.FileSystem.CurrentDirectory + "\Cache\PSP\" + ISOCacheFolderName)

                'Extract files to display infos
                Using ISOExtractor As New Process()
                    ISOExtractor.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\7z.exe"
                    ISOExtractor.StartInfo.Arguments = "e """ + GameISO + """" +
                        " -o""" + My.Computer.FileSystem.CurrentDirectory + "\Cache\PSP\" + ISOCacheFolderName + """" +
                        " PSP_GAME/PARAM.SFO PARAM.SFO PSP_GAME/ICON0.PNG ICON0.PNG PSP_GAME/PIC1.PNG PIC1.PNG PSP_GAME/SND0.AT3 SND0.AT3"
                    ISOExtractor.StartInfo.RedirectStandardOutput = True
                    ISOExtractor.StartInfo.UseShellExecute = False
                    ISOExtractor.StartInfo.CreateNoWindow = True
                    ISOExtractor.Start()
                    ISOExtractor.WaitForExit()
                End Using
            End If

            'Read PARAM.SFO
            Using SFOReader As New Process()
                SFOReader.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\sfo.exe"
                SFOReader.StartInfo.Arguments = """" + My.Computer.FileSystem.CurrentDirectory + "\Cache\PSP\" + ISOCacheFolderName + "\PARAM.SFO" + """"
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
                            NewPSPGame.GameTitle = Utils.CleanTitle(Line.Split("="c)(1).Trim(""""c))
                        ElseIf Line.StartsWith("DISC_ID=") Then
                            NewPSPGame.GameID = Line.Split("="c)(1).Trim(""""c)
                        ElseIf Line.StartsWith("CATEGORY=") Then
                            NewPSPGame.GameCategory = PSPGame.GetCategory(Line.Split("="c)(1).Trim(""""c))
                        ElseIf Line.StartsWith("DISC_VERSION=") Then
                            NewPSPGame.GameAppVer = FormatNumber(Line.Split("="c)(1).Trim(""""c), 0).Insert(1, ".")
                        ElseIf Line.StartsWith("PSP_SYSTEM_VER=") Then
                            NewPSPGame.GameRequiredFW = FormatNumber(Line.Split("="c)(1).Trim(""""c), 0).Insert(1, ".")
                        End If
                    Next

                    'Load game files
                    Dim PSPGAMEFolder As String = My.Computer.FileSystem.CurrentDirectory + "\Cache\PSP\" + ISOCacheFolderName
                    If File.Exists(PSPGAMEFolder + "\ICON0.PNG") Then
                        If Dispatcher.CheckAccess() = False Then
                            Dispatcher.BeginInvoke(Sub()
                                                       Dim TempBitmapImage = New BitmapImage()
                                                       TempBitmapImage.BeginInit()
                                                       TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                       TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                       TempBitmapImage.UriSource = New Uri(PSPGAMEFolder + "\ICON0.PNG", UriKind.RelativeOrAbsolute)
                                                       TempBitmapImage.EndInit()
                                                       NewPSPGame.GameCoverSource = TempBitmapImage
                                                   End Sub)
                        Else
                            Dim TempBitmapImage = New BitmapImage()
                            TempBitmapImage.BeginInit()
                            TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                            TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                            TempBitmapImage.UriSource = New Uri(PSPGAMEFolder + "\ICON0.PNG", UriKind.RelativeOrAbsolute)
                            TempBitmapImage.EndInit()
                            NewPSPGame.GameCoverSource = TempBitmapImage
                        End If
                    End If
                    If File.Exists(PSPGAMEFolder + "\PIC1.PNG") Then
                        If Dispatcher.CheckAccess() = False Then
                            Dispatcher.BeginInvoke(Sub()
                                                       Dim TempBitmapImage = New BitmapImage()
                                                       TempBitmapImage.BeginInit()
                                                       TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                       TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                       TempBitmapImage.UriSource = New Uri(PSPGAMEFolder + "\PIC1.PNG", UriKind.RelativeOrAbsolute)
                                                       TempBitmapImage.EndInit()
                                                       NewPSPGame.GameBackgroundSource = TempBitmapImage
                                                   End Sub)
                        Else
                            Dim TempBitmapImage = New BitmapImage()
                            TempBitmapImage.BeginInit()
                            TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                            TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                            TempBitmapImage.UriSource = New Uri(PSPGAMEFolder + "\PIC1.PNG", UriKind.RelativeOrAbsolute)
                            TempBitmapImage.EndInit()
                            NewPSPGame.GameBackgroundSource = TempBitmapImage
                        End If
                    End If
                    If File.Exists(PSPGAMEFolder + "\SND0.AT3") Then
                        NewPSPGame.GameBackgroundSoundFile = PSPGAMEFolder + "\SND0.AT3"
                    End If

                    NewPSPGame.GameSize = FormatNumber(ISOFileInfo.Length / 1073741824, 2) + " GB"

                    If Not String.IsNullOrWhiteSpace(NewPSPGame.GameID) Then
                        NewPSPGame.GameRegion = PSPGame.GetGameRegion(NewPSPGame.GameID)
                    End If

                    NewPSPGame.GameFilePath = GameISO
                    NewPSPGame.GameFileType = PSPGame.GameFileTypes.ISO

                    Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                    Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading ISO " + (NewLoadingWindow.LoadProgressBar.Value - FoldersCount).ToString + " of " + ISOCount.ToString())

                    'Add to the ListView
                    If GamesListView.Dispatcher.CheckAccess() = False Then
                        GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPSPGame))
                    Else
                        GamesListView.Items.Add(NewPSPGame)
                    End If

                End If

            End Using

        Next

    End Sub

    Private Sub GameLoaderWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles GameLoaderWorker.RunWorkerCompleted
        NewLoadingWindow.Close()
    End Sub

#End Region

#Region "Menu Actions"

    Private Sub LoadFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadFolderMenuItem.Click
        Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Select your PSP backup folder"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            FoldersCount = Directory.GetFiles(FBD.SelectedPath, "*.SFO", SearchOption.AllDirectories).Count
            ISOCount = Directory.GetFiles(FBD.SelectedPath, "*.iso", SearchOption.AllDirectories).Count

            NewLoadingWindow = New SyncWindow() With {.Title = "Loading PSP files", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.Maximum = FoldersCount + ISOCount
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + (FoldersCount + ISOCount).ToString()
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

    Private Sub CopyToMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles CopyToMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPSPGame As PSPGame = CType(GamesListView.SelectedItem, PSPGame)
            Dim FBD As New FolderBrowserDialog() With {.Description = "Where do you want to save the selected game ?"}

            If FBD.ShowDialog() = Forms.DialogResult.OK Then
                Dim NewCopyWindow As New CopyWindow() With {.ShowActivated = True,
                    .WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    .BackupDestinationPath = FBD.SelectedPath + "\",
                    .Title = "Copying " + SelectedPSPGame.GameTitle + " to " + FBD.SelectedPath + "\" + Path.GetFileName(SelectedPSPGame.GameFilePath)}

                If SelectedPSPGame.GameFileType = PSPGame.GameFileTypes.Backup Then
                    NewCopyWindow.BackupPath = SelectedPSPGame.GameFolderPath
                ElseIf SelectedPSPGame.GameFileType = PSPGame.GameFileTypes.ISO Then
                    NewCopyWindow.BackupPath = SelectedPSPGame.GameFilePath
                End If

                If NewCopyWindow.ShowDialog() = True Then
                    MsgBox("Game copied with success !", MsgBoxStyle.Information, "Completed")
                End If
            End If

        End If
    End Sub

    Private Sub PlayGameMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PlayGameMenuItem.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\ppsspp\PPSSPPWindows64.exe") Then
            If GamesListView.SelectedItem IsNot Nothing Then
                Dim SelectedPSPGame As PSPGame = CType(GamesListView.SelectedItem, PSPGame)

                If MsgBox("Start " + SelectedPSPGame.GameTitle + " using PPSSPP ?", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then
                    Dim EmulatorLauncherStartInfo As New ProcessStartInfo()
                    Dim EmulatorLauncher As New Process() With {.StartInfo = EmulatorLauncherStartInfo}
                    EmulatorLauncherStartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Emulators\ppsspp\PPSSPPWindows64.exe"
                    EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(My.Computer.FileSystem.CurrentDirectory + "\Emulators\ppsspp\PPSSPPWindows64.exe")

                    Select Case SelectedPSPGame.GameFileType
                        Case PSPGame.GameFileTypes.Backup
                            EmulatorLauncherStartInfo.Arguments = """" + SelectedPSPGame.GameFolderPath + """"
                        Case PSPGame.GameFileTypes.ISO
                            EmulatorLauncherStartInfo.Arguments = """" + SelectedPSPGame.GameFilePath + """"
                    End Select

                    EmulatorLauncher.Start()
                End If

            End If
        Else
            MsgBox("Cannot start PPSSPP." + vbCrLf + "Emulator pack is not installed.", MsgBoxStyle.Critical, "Error")
        End If
    End Sub

#End Region

    Private Sub GamesListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles GamesListView.SelectionChanged
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPSPGame As PSPGame = CType(GamesListView.SelectedItem, PSPGame)

            GameTitleTextBlock.Text = SelectedPSPGame.GameTitle
            GameIDTextBlock.Text = "Title ID: " & SelectedPSPGame.GameID
            GameRegionTextBlock.Text = "Region: " & SelectedPSPGame.GameRegion
            GameAppVersionTextBlock.Text = "Application Version: " & SelectedPSPGame.GameAppVer
            GameCategoryTextBlock.Text = "Category: " & SelectedPSPGame.GameCategory
            GameSizeTextBlock.Text = "Size: " & SelectedPSPGame.GameSize
            GameRequiredFirmwareTextBlock.Text = "Required Firmware: " & SelectedPSPGame.GameRequiredFW

            GameBackupTypeTextBlock.Text = "Backup Type: " & SelectedPSPGame.GameFileType.ToString()

            If Not String.IsNullOrEmpty(SelectedPSPGame.GameFilePath) Then
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " & New DirectoryInfo(Path.GetDirectoryName(SelectedPSPGame.GameFilePath)).Name
            Else
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " & New DirectoryInfo(SelectedPSPGame.GameFolderPath).Name
            End If

            If SelectedPSPGame.GameBackgroundSource IsNot Nothing Then
                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub()
                                               RectangleImageBrush.ImageSource = SelectedPSPGame.GameBackgroundSource
                                               BlurringShape.BeginAnimation(OpacityProperty, New DoubleAnimation With {.From = 0, .To = 1, .Duration = New Duration(TimeSpan.FromMilliseconds(500))})
                                           End Sub)
                Else
                    RectangleImageBrush.ImageSource = SelectedPSPGame.GameBackgroundSource
                    BlurringShape.BeginAnimation(OpacityProperty, New DoubleAnimation With {.From = 0, .To = 1, .Duration = New Duration(TimeSpan.FromMilliseconds(500))})
                End If
            Else
                RectangleImageBrush.ImageSource = Nothing
            End If

            If SelectedPSPGame.GameBackgroundSoundFile IsNot Nothing Then
                If IsSoundPlaying Then
                    Utils.StopGameSound()
                    IsSoundPlaying = False
                Else
                    Utils.PlayGameSound(SelectedPSPGame.GameBackgroundSoundFile)
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

End Class
