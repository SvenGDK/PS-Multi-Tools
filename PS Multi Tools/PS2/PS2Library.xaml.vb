Imports System.ComponentModel
Imports System.IO
Imports System.Windows.Forms
Imports psmt_lib

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
    Dim WithEvents SendToMenuItem As New Controls.MenuItem() With {.Header = "Send to PS4-5", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/send-icon.png", UriKind.Relative))}}
    Dim WithEvents ISOInfoMenuItem As New Controls.MenuItem() With {.Header = "ISO Infos (not available yet)", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/information-button.png", UriKind.Relative))}}
    Dim WithEvents GameInfoMenuItem As New Controls.MenuItem() With {.Header = "More Game Infos (not available yet)", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/information-button.png", UriKind.Relative))}}

    'Supplemental library menu items
    Dim WithEvents LoadFolderMenuItem As New Controls.MenuItem() With {.Header = "Load a new folder"}
    Dim WithEvents LoadDLFolderMenuItem As New Controls.MenuItem() With {.Header = "Open Downloads folder"}

    Private Sub PS2Library_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Set the controls in the shared library
        NewPS2Menu.GamesLView = GamesListView

        'Add supplemental library menu items that will be handled in the app
        Dim LibraryMenuItem As Controls.MenuItem = CType(NewPS2Menu.Items(0), Controls.MenuItem)
        LibraryMenuItem.Items.Add(LoadFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem)

        NewContextMenu.Items.Add(CopyToMenuItem)
        NewContextMenu.Items.Add(SendToMenuItem)
        NewContextMenu.Items.Add(ISOInfoMenuItem)
        NewContextMenu.Items.Add(GameInfoMenuItem)
        GamesListView.ContextMenu = NewContextMenu
    End Sub

#Region "Game Loader"

    Public Function GetGameID(GameISO As String) As String
        Dim GameID As String = ""

        Using SevenZip As New Process()
            SevenZip.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\7z.exe"
            SevenZip.StartInfo.Arguments = "l -ba """ + GameISO + """"
            SevenZip.StartInfo.RedirectStandardOutput = True
            SevenZip.StartInfo.UseShellExecute = False
            SevenZip.StartInfo.CreateNoWindow = True
            SevenZip.Start()

            'Read the output
            Dim OutputReader As StreamReader = SevenZip.StandardOutput
            Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.None)

            If ProcessOutput.Length > 0 Then
                For Each Line As String In ProcessOutput
                    If Line.Contains("SLES_") Or Line.Contains("SLUS_") Or Line.Contains("SCES_") Or Line.Contains("SCUS_") Then
                        If Line.Contains("Volume:") Then 'ID found in the ISO Header
                            If Line.Split(New String() {"Volume: "}, StringSplitOptions.RemoveEmptyEntries).Length > 0 Then
                                GameID = Line.Split(New String() {"Volume: "}, StringSplitOptions.RemoveEmptyEntries)(1)
                                Exit For
                            End If
                        Else 'ID found in the ISO files
                            If String.Join(" ", Line.Split(New Char() {}, StringSplitOptions.RemoveEmptyEntries)).Split(" "c).Length > 4 Then
                                GameID = String.Join(" ", Line.Split(New Char() {}, StringSplitOptions.RemoveEmptyEntries)).Split(" "c)(5).Trim()
                                Exit For
                            End If
                        End If
                    End If
                Next
            End If

        End Using

        If GameID = "" Then
            Return "ID not found"
        Else
            Return GameID
        End If
    End Function

    Private Sub GameLoaderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles GameLoaderWorker.DoWork

        'PS2 ISOs
        For Each GameISO In Directory.GetFiles(e.Argument.ToString, "*.iso", SearchOption.AllDirectories)

            Dim NewPS2Game As New PS2Game()
            Dim GameID As String = GetGameID(GameISO)

            If GameID = "ID not found" Then
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
                    NewPS2Game.GameTitle = GameID
                End If
            End If
        Next

        'PS2 CSOs
        For Each GameCSO In Directory.GetFiles(e.Argument.ToString, "*.cso", SearchOption.AllDirectories)

            Dim NewPS2Game As New PS2Game()
            Dim GameID As String = GetGameID(GameCSO)

            If GameID = "ID not found" Then

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
                    NewPS2Game.GameTitle = GameID
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

    Private Sub PSXDatacenterBrowser_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles PSXDatacenterBrowser.DocumentCompleted
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
        Dim infoRows As HtmlElementCollection = Nothing
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
            Dim FBD As New FolderBrowserDialog() With {.Description = "Where do you want to save the selected game ?"}

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

End Class
