Imports System.ComponentModel
Imports System.IO
Imports System.Windows.Forms

Public Class PSVLibrary

    Dim WithEvents GameLoaderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}
    Dim WithEvents NPSBrowser As New Forms.WebBrowser() With {.ScriptErrorsSuppressed = True}
    Dim WithEvents NewLoadingWindow As New SyncWindow() With {.Title = "Loading PS Vita files", .ShowActivated = True}

    Dim FoldersCount As Integer = 0
    Dim PKGCount As Integer = 0

    Dim URLs As New List(Of String)
    Dim CurrentURL As Integer = 0

    'Selected game context menu
    Dim WithEvents NewContextMenu As New Controls.ContextMenu()
    Dim WithEvents CopyToMenuItem As New Controls.MenuItem() With {.Header = "Copy to", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/copy-icon.png", UriKind.Relative))}}
    Dim WithEvents PKGInfoMenuItem As New Controls.MenuItem() With {.Header = "PKG Details", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/information-button.png", UriKind.Relative))}}
    Dim WithEvents PSNInfoMenuItem As New Controls.MenuItem() With {.Header = "Store Details", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/information-button.png", UriKind.Relative))}}

    'Supplemental library menu items
    Dim WithEvents LoadFolderMenuItem As New Controls.MenuItem() With {.Header = "Load a new folder"}
    Dim WithEvents LoadLibraryMenuItem As New Controls.MenuItem() With {.Header = "Show games library"}
    Dim WithEvents LoadDLFolderMenuItem As New Controls.MenuItem() With {.Header = "Open Downloads folder"}

    Private Sub PSVLibrary_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Set the controls in the shared library
        NewPSVMenu.LView = DLListView
        NewPSVMenu.GamesLView = GamesListView

        'Add supplemental library menu items that will be handled in the app
        Dim LibraryMenuItem As Controls.MenuItem = CType(NewPSVMenu.Items(0), Controls.MenuItem)
        LibraryMenuItem.Items.Add(LoadFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadLibraryMenuItem)
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem)

        'Load available context menu options
        NewPSVMenu.LoadDownloaderContextMenu()

        NewContextMenu.Items.Add(CopyToMenuItem)
        NewContextMenu.Items.Add(PKGInfoMenuItem)
        NewContextMenu.Items.Add(PSNInfoMenuItem)
        GamesListView.ContextMenu = NewContextMenu
    End Sub

#Region "Game Loader"

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

    Private Sub LoadLibraryMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadLibraryMenuItem.Click
        GamesListView.Visibility = Visibility.Visible
        DLListView.Visibility = Visibility.Hidden
    End Sub

    Private Sub GameLoaderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles GameLoaderWorker.DoWork

        'PSV encrypted/decrypted folders
        For Each Game In Directory.GetFiles(e.Argument.ToString, "*.sfo", SearchOption.AllDirectories)

            Dim NewPSVGame As New PSVGame()

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
                            NewPSVGame.GameTitle = Line.Split("="c)(1).Trim(""""c).Trim()
                        ElseIf Line.StartsWith("TITLE_ID=") Then
                            NewPSVGame.GameID = Line.Split("="c)(1).Trim(""""c).Trim()
                        ElseIf Line.StartsWith("CATEGORY=") Then
                            NewPSVGame.GameCategory = PSVGame.GetCategory(Line.Split("="c)(1).Trim(""""c))
                        ElseIf Line.StartsWith("APP_VER=") Then
                            NewPSVGame.GameAppVer = "App Ver.: " + FormatNumber(Line.Split("="c)(1).Trim(""""c), 2)
                        ElseIf Line.StartsWith("PSP2_DISP_VER=") Then
                            NewPSVGame.GameRequiredFW = "Req. FW: " + FormatNumber(Line.Split("="c)(1).Trim(""""c), 2)
                        ElseIf Line.StartsWith("VERSION=") Then
                            NewPSVGame.GameVer = "Version: " + FormatNumber(Line.Split("="c)(1).Trim(""""c), 2)
                        ElseIf Line.StartsWith("CONTENT_ID=") Then
                            NewPSVGame.ContentID = Line.Split("="c)(1).Trim(""""c).Trim()
                        End If
                    Next

                    Dim PSVGAMEFolder As String = Path.GetDirectoryName(Directory.GetParent(Game).FullName)
                    Dim PSVGAMEFolderSize As Long = Utils.DirSize(PSVGAMEFolder, True)
                    NewPSVGame.GameSize = FormatNumber(PSVGAMEFolderSize / 1073741824, 2) + " GB"
                    NewPSVGame.GameFolderPath = PSVGAMEFolder
                    NewPSVGame.GameFileType = PSVGame.GameFileTypes.Backup

                    If Not String.IsNullOrWhiteSpace(NewPSVGame.GameID) Then
                        NewPSVGame.GameRegion = PSVGame.GetGameRegion(NewPSVGame.GameID)
                    End If

                    If NewPSVGame.ContentID.Split("-"c)(2) IsNot Nothing Then
                        Dim NPSURL As String = "https://nopaystation.com/view/PSV/" + NewPSVGame.GameID + "/" + NewPSVGame.ContentID.Split("-"c)(2) + "/1"
                        URLs.Add(NPSURL)
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

            Dim NewPSVGame As New PSVGame()
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
                            NewPSVGame.GameTitle = Line.Split(":"c)(1).Trim(""""c).Trim()
                        ElseIf Line.StartsWith("Title ID:") Then
                            NewPSVGame.GameID = Line.Split(":"c)(1).Trim(""""c).Trim()
                        ElseIf Line.StartsWith("NPS Type:") Then
                            NewPSVGame.GameCategory = Line.Split(":"c)(1).Trim(""""c).Trim()
                        ElseIf Line.StartsWith("App Ver:") Then
                            NewPSVGame.GameAppVer = "App Ver.: " + FormatNumber(Line.Split(":"c)(1).Trim(""""c), 2)
                        ElseIf Line.StartsWith("Min FW:") Then
                            NewPSVGame.GameRequiredFW = "Req. FW: " + FormatNumber(Line.Split(":"c)(1).Trim(""""c), 2)
                        ElseIf Line.StartsWith("Version:") Then
                            NewPSVGame.GameVer = "Version: " + FormatNumber(Line.Split(":"c)(1).Trim(""""c), 2)
                        ElseIf Line.StartsWith("Content ID:") Then
                            NewPSVGame.ContentID = Line.Split(":"c)(1).Trim(""""c).Trim()
                        ElseIf Line.StartsWith("Region:") Then
                            NewPSVGame.GameRegion = Line.Split(":"c)(1).Trim(""""c).Trim()
                        End If
                    Next

                    NewPSVGame.GameSize = FormatNumber(GameInfo.Length / 1073741824, 2) + " GB"
                    NewPSVGame.GameFilePath = GamePKG
                    NewPSVGame.GameFileType = PSVGame.GameFileTypes.PKG

                    If NewPSVGame.GameCategory = "PSX GAME" Then
                        URLs.Add("https://nopaystation.com/view/PSX/" + NewPSVGame.GameID + "/" + NewPSVGame.ContentID.Split("-"c)(2) + "/0")
                    Else
                        URLs.Add("https://nopaystation.com/view/PSV/" + NewPSVGame.GameID + "/" + NewPSVGame.ContentID.Split("-"c)(2) + "/1")
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

        NewLoadingWindow.LoadStatusTextBlock.Text = "Getting " + URLs.Count.ToString() + " available covers"
        NewLoadingWindow.LoadProgressBar.Value = 0
        NewLoadingWindow.LoadProgressBar.Maximum = URLs.Count

        GetGameCovers()
    End Sub

    Private Sub GetGameCovers()
        NPSBrowser.Navigate(URLs.Item(0))
    End Sub

    Private Sub NPSBrowser_DocumentCompleted(sender As Object, e As Forms.WebBrowserDocumentCompletedEventArgs) Handles NPSBrowser.DocumentCompleted

        On Error Resume Next
        RemoveHandler NPSBrowser.DocumentCompleted, AddressOf NPSBrowser_DocumentCompleted

        Dim GameCoverSource As String = ""
        Dim TitleID As String = ""

        'Art
        If NPSBrowser.Document.GetElementById("itemArtwork") IsNot Nothing Then
            If NPSBrowser.Document.GetElementById("itemArtwork").GetAttribute("src") IsNot Nothing Then
                GameCoverSource = NPSBrowser.Document.GetElementById("itemArtwork").GetAttribute("src").Trim()
            End If
        End If

        'Title ID
        If NPSBrowser.Document.GetElementById("titleId") IsNot Nothing Then
            If NPSBrowser.Document.GetElementById("titleId").GetAttribute("value") IsNot Nothing Then
                TitleID = NPSBrowser.Document.GetElementById("titleId").GetAttribute("value").Trim()
            End If
        End If

        If Not GameCoverSource = "" And Not TitleID = "" Then
            For Each Game In GamesListView.Items
                Dim FoundGame As PSVGame = CType(Game, PSVGame)
                If FoundGame.GameID.Contains(TitleID) Or FoundGame.GameID = TitleID Then
                    FoundGame.GameCoverSource = New BitmapImage(New Uri(NPSBrowser.Document.GetElementById("itemArtwork").GetAttribute("src"), UriKind.RelativeOrAbsolute))
                    Exit For
                End If
            Next
        End If

        AddHandler NPSBrowser.DocumentCompleted, AddressOf NPSBrowser_DocumentCompleted

        If CurrentURL < URLs.Count Then
            NPSBrowser.Navigate(URLs.Item(CurrentURL))
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

    Private Sub CopyToMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles CopyToMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPSVGame As PSVGame = CType(GamesListView.SelectedItem, PSVGame)
            Dim FBD As New FolderBrowserDialog() With {.Description = "Where do you want to save the selected game ?"}

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

End Class
