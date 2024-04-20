Imports System.ComponentModel
Imports System.IO
Imports System.Windows.Media.Animation
Imports psmt_lib

Public Class PSVLibrary

    Dim WithEvents GameLoaderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}
    Dim WithEvents CoverBrowser As New Forms.WebBrowser() With {.ScriptErrorsSuppressed = True}
    Dim WithEvents NewLoadingWindow As New SyncWindow() With {.Title = "Loading PS Vita files", .ShowActivated = True}

    Dim FoldersCount As Integer = 0
    Dim PKGCount As Integer = 0

    Dim URLs As New List(Of String)()
    Dim CurrentURL As Integer = 0

    'Selected game context menu
    Dim WithEvents NewContextMenu As New Controls.ContextMenu()
    Dim WithEvents CopyToMenuItem As New Controls.MenuItem() With {.Header = "Copy to", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/copy-icon.png", UriKind.Relative))}}
    Dim WithEvents PKGInfoMenuItem As New Controls.MenuItem() With {.Header = "PKG Details", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/information-button.png", UriKind.Relative))}}
    Dim WithEvents PSNInfoMenuItem As New Controls.MenuItem() With {.Header = "Load infos from NPS", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/information-button.png", UriKind.Relative))}}

    'Supplemental library menu items
    Dim WithEvents LoadFolderMenuItem As New Controls.MenuItem() With {.Header = "Load a new folder"}
    Dim WithEvents LoadLibraryMenuItem As New Controls.MenuItem() With {.Header = "Show games library"}
    Dim WithEvents LoadDLFolderMenuItem As New Controls.MenuItem() With {.Header = "Open Downloads folder"}

    Private Sub PSVLibrary_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Set the controls in the shared library
        NewPSVMenu.GamesLView = GamesListView

        'Add supplemental library menu items that will be handled in the app
        Dim LibraryMenuItem As Controls.MenuItem = CType(NewPSVMenu.Items(0), Controls.MenuItem)
        LibraryMenuItem.Items.Add(LoadFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadLibraryMenuItem)
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem)

        'Add the new PKG Browser
        Dim PKGDownloaderMenuItem As New Controls.MenuItem() With {.Header = "PKG Browser & Downloader"}
        AddHandler PKGDownloaderMenuItem.Click, AddressOf OpenPKGBrowser
        NewPSVMenu.Items.Add(PKGDownloaderMenuItem)

        NewContextMenu.Items.Add(CopyToMenuItem)
        NewContextMenu.Items.Add(PKGInfoMenuItem)
        NewContextMenu.Items.Add(PSNInfoMenuItem)
        GamesListView.ContextMenu = NewContextMenu
    End Sub

#Region "Game Loader"

    Private Sub GameLoaderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles GameLoaderWorker.DoWork

        'PSV encrypted/decrypted folders
        For Each Game In Directory.GetFiles(e.Argument.ToString, "*.sfo", SearchOption.AllDirectories)

            Dim NewPSVGame As New PSVGame() With {.GridWidth = 125, .GridHeight = 175, .ImageWidth = 100, .ImageHeight = 128}

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
                            NewPSVGame.GameTitle = Utils.CleanTitle(Line.Split("="c)(1).Trim(""""c).Trim())
                        ElseIf Line.StartsWith("TITLE_ID=") Then
                            NewPSVGame.GameID = Line.Split("="c)(1).Trim(""""c).Trim()
                        ElseIf Line.StartsWith("CATEGORY=") Then
                            NewPSVGame.GameCategory = PSVGame.GetCategory(Line.Split("="c)(1).Trim(""""c))
                        ElseIf Line.StartsWith("APP_VER=") Then
                            NewPSVGame.GameAppVer = FormatNumber(Line.Split("="c)(1).Trim(""""c), 0).Insert(1, ".")
                        ElseIf Line.StartsWith("PSP2_DISP_VER=") Then
                            NewPSVGame.GameRequiredFW = FormatNumber(Line.Split("="c)(1).Trim(""""c), 0).Replace("."c, "").Insert(2, ".")
                        ElseIf Line.StartsWith("VERSION=") Then
                            NewPSVGame.GameVer = FormatNumber(Line.Split("="c)(1).Trim(""""c), 0).Insert(1, ".")
                        ElseIf Line.StartsWith("CONTENT_ID=") Then
                            NewPSVGame.ContentID = Line.Split("="c)(1).Trim(""""c).Trim()
                        End If
                    Next

                    Dim PSVGAMEFolder As String = Path.GetDirectoryName(Directory.GetParent(Game).FullName)
                    Dim PSVGAMEFolderSize As Long = Utils.DirSize(PSVGAMEFolder, True)
                    NewPSVGame.GameSize = FormatNumber(PSVGAMEFolderSize / 1073741824, 2) + " GB"
                    NewPSVGame.GameFolderPath = PSVGAMEFolder
                    NewPSVGame.GameFileType = PSVGame.GameFileTypes.Backup

                    If Not String.IsNullOrEmpty(NewPSVGame.GameID) Then
                        NewPSVGame.GameRegion = PSVGame.GetGameRegion(NewPSVGame.GameID)

                        If Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png") Then
                            If Dispatcher.CheckAccess() = False Then
                                Dispatcher.BeginInvoke(Sub()
                                                           Dim TempBitmapImage = New BitmapImage()
                                                           TempBitmapImage.BeginInit()
                                                           TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                           TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                           TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png", UriKind.RelativeOrAbsolute)
                                                           TempBitmapImage.EndInit()
                                                           NewPSVGame.GameCoverSource = TempBitmapImage
                                                       End Sub)
                            Else
                                Dim TempBitmapImage = New BitmapImage()
                                TempBitmapImage.BeginInit()
                                TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png", UriKind.RelativeOrAbsolute)
                                TempBitmapImage.EndInit()
                                NewPSVGame.GameCoverSource = TempBitmapImage
                            End If
                        End If
                    End If

                    Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                    Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading folder " + (NewLoadingWindow.LoadProgressBar.Value - PKGCount).ToString + " of " + FoldersCount.ToString())

                    'Add to the ListView
                    If GamesListView.Dispatcher.CheckAccess() = False Then
                        GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPSVGame))
                    Else
                        GamesListView.Items.Add(NewPSVGame)
                    End If

                End If

            End Using

        Next

        'PSV PSN pkgs
        For Each GamePKG In Directory.GetFiles(e.Argument.ToString, "*.pkg", SearchOption.AllDirectories)

            Dim NewPSVGame As New PSVGame() With {.GridWidth = 125, .GridHeight = 175, .ImageWidth = 100, .ImageHeight = 128}
            Dim GameInfo As New FileInfo(GamePKG)

            Using SFOReader As New Process()
                SFOReader.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PSN_get_pkg_info.exe"
                SFOReader.StartInfo.Arguments = """" + GamePKG + """"
                SFOReader.StartInfo.RedirectStandardOutput = True
                SFOReader.StartInfo.UseShellExecute = False
                SFOReader.StartInfo.CreateNoWindow = True
                SFOReader.Start()

                Dim OutputReader As StreamReader = SFOReader.StandardOutput
                Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

                If ProcessOutput.Count > 0 Then

                    'Load game infos
                    For Each Line In ProcessOutput
                        If Line.StartsWith("Title:") Then
                            NewPSVGame.GameTitle = Utils.CleanTitle(Line.Split(":"c)(1).Trim(""""c).Trim())
                        ElseIf Line.StartsWith("Title ID:") Then
                            NewPSVGame.GameID = Line.Split(":"c)(1).Trim(""""c).Trim()
                        ElseIf Line.StartsWith("NPS Type:") Then
                            NewPSVGame.GameCategory = Line.Split(":"c)(1).Trim(""""c).Trim()
                        ElseIf Line.StartsWith("App Ver:") Then
                            NewPSVGame.GameAppVer = FormatNumber(Line.Split(":"c)(1).Trim(""""c), 2).Replace(","c, "").Insert(1, ".")
                        ElseIf Line.StartsWith("Min FW:") Then
                            NewPSVGame.GameRequiredFW = FormatNumber(Line.Split(":"c)(1).Trim(""""c), 2).Replace(","c, "").Replace("."c, "").Insert(2, ".")
                        ElseIf Line.StartsWith("Version:") Then
                            NewPSVGame.GameVer = FormatNumber(Line.Split(":"c)(1).Trim(""""c), 2).Replace(","c, "").Insert(1, ".")
                        ElseIf Line.StartsWith("Content ID:") Then
                            NewPSVGame.ContentID = Line.Split(":"c)(1).Trim(""""c).Trim()
                        ElseIf Line.StartsWith("Region:") Then
                            NewPSVGame.GameRegion = Line.Split(":"c)(1).Trim(""""c).Trim()
                        End If
                    Next

                    NewPSVGame.GameSize = FormatNumber(GameInfo.Length / 1073741824, 2) + " GB"
                    NewPSVGame.GameFilePath = GamePKG
                    NewPSVGame.GameFileType = PSVGame.GameFileTypes.PKG

                    If Not String.IsNullOrEmpty(NewPSVGame.GameID) Then
                        If Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png") Then
                            If Dispatcher.CheckAccess() = False Then
                                Dispatcher.BeginInvoke(Sub()
                                                           Dim TempBitmapImage = New BitmapImage()
                                                           TempBitmapImage.BeginInit()
                                                           TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                           TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                           TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png", UriKind.RelativeOrAbsolute)
                                                           TempBitmapImage.EndInit()
                                                           NewPSVGame.GameCoverSource = TempBitmapImage
                                                       End Sub)
                            Else
                                Dim TempBitmapImage = New BitmapImage()
                                TempBitmapImage.BeginInit()
                                TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PSVita/" + NewPSVGame.GameID + ".png", UriKind.RelativeOrAbsolute)
                                TempBitmapImage.EndInit()
                                NewPSVGame.GameCoverSource = TempBitmapImage
                            End If
                        End If
                    End If

                    Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                    Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading PKG " + (NewLoadingWindow.LoadProgressBar.Value - FoldersCount).ToString + " of " + PKGCount.ToString())

                    'Add to the ListView
                    If GamesListView.Dispatcher.CheckAccess() = False Then
                        GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPSVGame))
                    Else
                        GamesListView.Items.Add(NewPSVGame)
                    End If

                End If

            End Using

        Next

    End Sub

    Private Sub GameLoaderWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles GameLoaderWorker.RunWorkerCompleted
        NewLoadingWindow.Close()
        GamesListView.Items.Refresh()
    End Sub

#End Region

#Region "Contextmenu Actions"

    Private Sub CopyToMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles CopyToMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPSVGame As PSVGame = CType(GamesListView.SelectedItem, PSVGame)
            Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Where do you want to save the selected game ?"}

            If FBD.ShowDialog() = Forms.DialogResult.OK Then
                Dim NewCopyWindow As New CopyWindow() With {.ShowActivated = True,
                    .WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    .BackupDestinationPath = FBD.SelectedPath + "\",
                    .Title = "Copying " + SelectedPSVGame.GameTitle + " to " + FBD.SelectedPath + "\" + Path.GetFileName(SelectedPSVGame.GameFilePath)}

                If SelectedPSVGame.GameFileType = PSVGame.GameFileTypes.Backup Then
                    NewCopyWindow.BackupPath = SelectedPSVGame.GameFolderPath
                ElseIf SelectedPSVGame.GameFileType = PSVGame.GameFileTypes.PKG Then
                    NewCopyWindow.BackupPath = SelectedPSVGame.GameFilePath
                End If

                If NewCopyWindow.ShowDialog() = True Then
                    MsgBox("Game copied with success !", MsgBoxStyle.Information, "Completed")
                End If
            End If

        End If
    End Sub

    Private Sub PSNInfoMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PSNInfoMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPSVGame As PSVGame = CType(GamesListView.SelectedItem, PSVGame)
            If Not String.IsNullOrEmpty(SelectedPSVGame.ContentID) Then
                If MsgBox("Load from NPS?", MsgBoxStyle.YesNo, "") = MsgBoxResult.Yes Then
                    Dim SelectedPackage As New psmt_lib.Structures.Package() With {.PackageContentID = SelectedPSVGame.ContentID, .PackageTitleID = SelectedPSVGame.GameID}
                    Dim NewPackageInfoWindow As New DownloadPackageInfoWindow() With {.ShowActivated = True, .Title = SelectedPSVGame.GameTitle, .CurrentPackage = SelectedPackage, .PackageConsole = "PSV"}
                    NewPackageInfoWindow.Show()
                Else
                    Dim NewPKGInfo As New PKGInfo() With {.SelectedPKG = SelectedPSVGame.GameFilePath}
                    NewPKGInfo.Show()
                End If
            End If
        End If
    End Sub

    Private Sub PKGInfoMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PKGInfoMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPSVGame As PSVGame = CType(GamesListView.SelectedItem, PSVGame)
            Dim NewPKGInfo As New PKGInfo() With {.SelectedPKG = SelectedPSVGame.GameFilePath, .Console = "PSV"}
            NewPKGInfo.Show()
        End If
    End Sub

    Private Sub GamesListView_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs) Handles GamesListView.ContextMenuOpening
        NewContextMenu.Items.Clear()

        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPSVGame As PSVGame = CType(GamesListView.SelectedItem, PSVGame)

            NewContextMenu.Items.Add(CopyToMenuItem)

            If SelectedPSVGame.GameFileType = PS3Game.GameFileTypes.Backup Then
                NewContextMenu.Items.Add(PSNInfoMenuItem)
            ElseIf SelectedPSVGame.GameFileType = PS3Game.GameFileTypes.PKG Then
                NewContextMenu.Items.Add(PSNInfoMenuItem)
                NewContextMenu.Items.Add(PKGInfoMenuItem)
            End If
        End If
    End Sub

    Private Sub GamesListView_ContextMenuClosing(sender As Object, e As ContextMenuEventArgs) Handles GamesListView.ContextMenuClosing
        NewContextMenu.Items.Clear()
    End Sub

#End Region

#Region "Menu Actions"

    Private Sub LoadFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadFolderMenuItem.Click
        Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Select your PSV backup folder"}

        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            PKGCount = Directory.GetFiles(FBD.SelectedPath, "*.pkg", SearchOption.AllDirectories).Count
            FoldersCount = Directory.GetFiles(FBD.SelectedPath, "*.sfo", SearchOption.AllDirectories).Count

            NewLoadingWindow = New SyncWindow() With {.Title = "Loading PS Vita files", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.Maximum = PKGCount + FoldersCount
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + (PKGCount + FoldersCount).ToString()
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

    Private Sub OpenPKGBrowser(sender As Object, e As RoutedEventArgs)
        Dim NewPKGBrowser As New PKGBrowser() With {.Console = "PSV", .ShowActivated = True}
        NewPKGBrowser.Show()
    End Sub

    Private Sub GamesListView_PreviewMouseWheel(sender As Object, e As MouseWheelEventArgs) Handles GamesListView.PreviewMouseWheel
        Dim OpenWindowsListViewScrollViewer As ScrollViewer = Utils.FindScrollViewer(GamesListView)
        Dim HorizontalOffset As Double = OpenWindowsListViewScrollViewer.HorizontalOffset
        OpenWindowsListViewScrollViewer.ScrollToHorizontalOffset(HorizontalOffset - (e.Delta / 100))
        e.Handled = True
    End Sub

    Private Sub GamesListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles GamesListView.SelectionChanged
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPSVGame As PSVGame = CType(GamesListView.SelectedItem, PSVGame)

            GameTitleTextBlock.Text = SelectedPSVGame.GameTitle
            GameIDTextBlock.Text = "Title ID: " & SelectedPSVGame.GameID
            GameContentIDTextBlock.Text = "Content ID: " & SelectedPSVGame.ContentID
            GameRegionTextBlock.Text = "Region: " & SelectedPSVGame.GameRegion
            GameVersionTextBlock.Text = "Game Version: " & SelectedPSVGame.GameVer
            GameAppVersionTextBlock.Text = "Application Version: " & SelectedPSVGame.GameAppVer
            GameCategoryTextBlock.Text = "Category: " & SelectedPSVGame.GameCategory
            GameSizeTextBlock.Text = "Size: " & SelectedPSVGame.GameSize
            GameRequiredFirmwareTextBlock.Text = "Required Firmware: " & SelectedPSVGame.GameRequiredFW

            GameBackupTypeTextBlock.Text = "Backup Type: " & SelectedPSVGame.GameFileType.ToString()

            If Not String.IsNullOrEmpty(SelectedPSVGame.GameFilePath) Then
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " & New DirectoryInfo(Path.GetDirectoryName(SelectedPSVGame.GameFilePath)).Name
            Else
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " & New DirectoryInfo(SelectedPSVGame.GameFolderPath).Name
            End If

        End If
    End Sub

End Class
