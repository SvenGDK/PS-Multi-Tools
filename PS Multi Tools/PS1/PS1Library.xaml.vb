Imports System.ComponentModel
Imports System.IO
Imports System.Windows.Forms

Public Class PS1Library

    Dim WithEvents GameLoaderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}
    Dim WithEvents PSXDatacenterBrowser As New WebBrowser()
    Dim WithEvents NewLoadingWindow As New SyncWindow() With {.Title = "Loading PS1 files", .ShowActivated = True}

    Dim URLs As New List(Of String)()
    Dim CurrentKeyCount As Integer = 0
    Dim BINCount As Integer = 0

    'Selected game context menu
    Dim WithEvents NewContextMenu As New Controls.ContextMenu()
    Dim WithEvents CopyToMenuItem As New Controls.MenuItem() With {.Header = "Copy to", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/copy-icon.png", UriKind.Relative))}}

    'Supplemental library menu items
    Dim WithEvents LoadFolderMenuItem As New Controls.MenuItem() With {.Header = "Load a new folder"}
    Dim WithEvents LoadDLFolderMenuItem As New Controls.MenuItem() With {.Header = "Open Downloads folder"}

    Private Sub PS1Library_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Set the controls in the shared library
        NewPS1Menu.GamesLView = GamesListView

        'Add supplemental library menu items that will be handled in the app
        Dim LibraryMenuItem As Controls.MenuItem = CType(NewPS1Menu.Items(0), Controls.MenuItem)
        LibraryMenuItem.Items.Add(LoadFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem)

        NewContextMenu.Items.Add(CopyToMenuItem)
        GamesListView.ContextMenu = NewContextMenu
    End Sub

#Region "Game Loader"

    Private Sub GameLoaderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles GameLoaderWorker.DoWork

        Dim GamesList As New List(Of String)()
        Dim FailList As New List(Of String)()

        For Each Game In Directory.GetFiles(e.Argument.ToString, "*.bin", SearchOption.AllDirectories)

            Dim GameInfo As New FileInfo(Game)

            'Skip multiple "Track" files and only read the first one
            If GameInfo.Name.ToLower().Contains("track") Then
                If Not Game.ToLower().Contains("(track 1).bin") OrElse Not GameInfo.Name.ToLower().Contains("(track 01).bin") Then
                    'Skip
                    Continue For
                End If
            End If

            Dim GameStartLetter As String = GameInfo.Name.Substring(0, 1) 'Take the first letter of the file name (required to browse PSXDatacenter)
            Dim NewPS1Game As New PS1Game With {.GameFilePath = Game, .GameSize = FormatNumber(GameInfo.Length / 1048576, 2) + " MB"}

            'Search for the game ID within the first 7MB with strings & findstr
            'We could also use StreamReader & BinaryReader but there are many methods, this is an easy way and also used in PS Mac Tools
            Using WindowsCMD As New Process()
                WindowsCMD.StartInfo.FileName = "cmd"
                WindowsCMD.StartInfo.Arguments = "/c strings -nobanner -b 7340032 """ + Game + """ | findstr BOOT"
                WindowsCMD.StartInfo.RedirectStandardOutput = True
                WindowsCMD.StartInfo.UseShellExecute = False
                WindowsCMD.StartInfo.CreateNoWindow = True
                WindowsCMD.Start()
                WindowsCMD.WaitForExit()

                Dim OutputReader As StreamReader = WindowsCMD.StandardOutput
                Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
                Dim GameIDFound As Boolean = False

                If ProcessOutput.Length > 0 Then
                    For Each OutputLine In ProcessOutput
                        If OutputLine.Contains("BOOT =") Or OutputLine.Contains("BOOT=") Then
                            GameIDFound = True
                            Dim GameID As String = OutputLine.Replace("BOOT = cdrom:\", "").Replace("BOOT=cdrom:\", "").Replace("BOOT = cdrom:", "").Replace(";1", "").Replace("_", "-").Replace(".", "").Trim()
                            Dim RegionCharacter As String = GetRegionChar(GameID)

                            'Set known values
                            NewPS1Game.GameID = UCase(GameID)

                            'Check game id length & if the generated url is valid
                            If GameID.Length = 10 Then
                                If Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS1/" + GameID + ".jpg") Then
                                    If Dispatcher.CheckAccess() = False Then
                                        Dispatcher.BeginInvoke(Sub()
                                                                   Dim TempBitmapImage = New BitmapImage()
                                                                   TempBitmapImage.BeginInit()
                                                                   TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                                   TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                                   TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS1/" + GameID + ".jpg", UriKind.RelativeOrAbsolute)
                                                                   TempBitmapImage.EndInit()
                                                                   NewPS1Game.GameCoverSource = TempBitmapImage
                                                               End Sub)
                                    Else
                                        Dim TempBitmapImage = New BitmapImage()
                                        TempBitmapImage.BeginInit()
                                        TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                        TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                        TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS1/" + GameID + ".jpg", UriKind.RelativeOrAbsolute)
                                        TempBitmapImage.EndInit()
                                        NewPS1Game.GameCoverSource = TempBitmapImage
                                    End If

                                    If Utils.IsURLValid("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html") Then
                                        URLs.Add("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html")
                                    Else
                                        NewPS1Game.GameTitle = GameID
                                    End If
                                Else
                                    If Utils.IsURLValid("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html") Then
                                        URLs.Add("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html")
                                    Else
                                        NewPS1Game.GameTitle = GameID
                                    End If
                                End If
                            End If

                            GameIDFound = True
                            Exit For
                        Else
                            NewPS1Game.GameTitle = GameInfo.Name
                        End If
                    Next
                Else
                    NewPS1Game.GameTitle = GameInfo.Name
                End If

                If GameIDFound = False Then
                    FailList.Add(Game)
                Else
                    'Update progress
                    Dispatcher.BeginInvoke(Sub()
                                               NewLoadingWindow.LoadProgressBar.Value += 1
                                               NewLoadingWindow.LoadStatusTextBlock.Text = "Loading bin " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + BINCount.ToString()
                                           End Sub)

                    'Add to the ListView
                    If GamesListView.Dispatcher.CheckAccess() = False Then
                        GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPS1Game))
                    Else
                        GamesListView.Items.Add(NewPS1Game)
                    End If
                End If

            End Using
        Next

        If FailList.Count > 0 Then 'Ask for an extended search
            If MsgBox("Some Game IDs could not be found quickly, do you want to extend the search ? This requires about 1-5min for each game (depending on your hardware).", MsgBoxStyle.YesNo, "Not all Game IDs could be found") = MsgBoxResult.Yes Then

                'Update progress
                Dispatcher.BeginInvoke(Sub()
                                           NewLoadingWindow.LoadProgressBar.Value = 0
                                           NewLoadingWindow.LoadProgressBar.Maximum = FailList.Count
                                           NewLoadingWindow.LoadStatusTextBlock.Text = "Loading bin 1 of " + FailList.Count.ToString()
                                       End Sub)

                For Each Game In FailList

                    Dim GameInfo As New FileInfo(Game)

                    'Skip multiple "Track" files and only read the first one
                    If GameInfo.Name.ToLower().Contains("track") Then
                        If Not Game.ToLower().Contains("(track 1).bin") OrElse Not GameInfo.Name.ToLower().Contains("(track 01).bin") Then
                            'Skip
                            Continue For
                        End If
                    End If

                    Dim GameStartLetter As String = GameInfo.Name.Substring(0, 1) 'Take the first letter of the file name (required to browse PSXDatacenter)
                    Dim NewPS1Game As New PS1Game With {.GameFilePath = Game, .GameSize = FormatNumber(GameInfo.Length / 1048576, 2) + " MB"}
                    Dim GameFileSizeAsString As String = GameInfo.Length.ToString()

                    'Search for the game ID within the first 7MB with strings & findstr
                    'We could also use StreamReader & BinaryReader but there are many methods, this is an easy way and also used in PS Mac Tools
                    Using WindowsCMD As New Process()
                        WindowsCMD.StartInfo.FileName = "cmd"
                        WindowsCMD.StartInfo.Arguments = "/c strings -nobanner -b " + GameFileSizeAsString + " """ + Game + """ | findstr BOOT"
                        WindowsCMD.StartInfo.RedirectStandardOutput = True
                        WindowsCMD.StartInfo.UseShellExecute = False
                        WindowsCMD.StartInfo.CreateNoWindow = True
                        WindowsCMD.Start()
                        WindowsCMD.WaitForExit()

                        Dim OutputReader As StreamReader = WindowsCMD.StandardOutput
                        Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

                        If ProcessOutput.Length > 0 Then 'Game ID found
                            For Each OutputLine In ProcessOutput
                                If OutputLine.Contains("BOOT =") Or OutputLine.Contains("BOOT=") Then
                                    Dim GameID As String = OutputLine.Replace("BOOT = cdrom:\", "").Replace("BOOT=cdrom:\", "").Replace("BOOT = cdrom:", "").Replace(";1", "").Replace("_", "-").Replace(".", "").Trim()
                                    Dim RegionCharacter As String = GetRegionChar(GameID)

                                    'Set known values
                                    NewPS1Game.GameID = UCase(GameID)

                                    'Check game id length & if the generated url is valid
                                    If GameID.Length = 10 Then
                                        If Utils.IsURLValid("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS1/" + GameID + ".jpg") Then
                                            If Dispatcher.CheckAccess() = False Then
                                                Dispatcher.BeginInvoke(Sub()
                                                                           Dim TempBitmapImage = New BitmapImage()
                                                                           TempBitmapImage.BeginInit()
                                                                           TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                                           TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                                           TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS1/" + GameID + ".jpg", UriKind.RelativeOrAbsolute)
                                                                           TempBitmapImage.EndInit()
                                                                           NewPS1Game.GameCoverSource = TempBitmapImage
                                                                       End Sub)
                                            Else
                                                Dim TempBitmapImage = New BitmapImage()
                                                TempBitmapImage.BeginInit()
                                                TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                TempBitmapImage.UriSource = New Uri("https://raw.githubusercontent.com/SvenGDK/PSMT-Covers/main/PS1/" + GameID + ".jpg", UriKind.RelativeOrAbsolute)
                                                TempBitmapImage.EndInit()
                                                NewPS1Game.GameCoverSource = TempBitmapImage
                                            End If

                                            If Utils.IsURLValid("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html") Then
                                                URLs.Add("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html")
                                            Else
                                                NewPS1Game.GameTitle = GameID
                                            End If
                                        Else
                                            If Utils.IsURLValid("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html") Then
                                                URLs.Add("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html")
                                            Else
                                                NewPS1Game.GameTitle = GameID
                                            End If
                                        End If
                                    End If

                                    Exit For
                                Else
                                    NewPS1Game.GameTitle = GameInfo.Name
                                End If
                            Next
                        Else
                            NewPS1Game.GameTitle = GameInfo.Name
                        End If

                    End Using

                    'Update progress
                    Dispatcher.BeginInvoke(Sub()
                                               NewLoadingWindow.LoadProgressBar.Value += 1
                                               NewLoadingWindow.LoadStatusTextBlock.Text = "Loading bin " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FailList.Count.ToString()
                                           End Sub)

                    'Add to the ListView
                    If GamesListView.Dispatcher.CheckAccess() = False Then
                        GamesListView.Dispatcher.BeginInvoke(Sub()
                                                                 GamesListView.Items.Add(NewPS1Game)
                                                                 GamesListView.Items.Refresh()
                                                             End Sub)
                    Else
                        GamesListView.Items.Add(NewPS1Game)
                        GamesListView.Items.Refresh()
                    End If
                Next

            End If
        End If

    End Sub

    Private Sub GameLoaderWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles GameLoaderWorker.RunWorkerCompleted
        NewLoadingWindow.LoadStatusTextBlock.Text = "Getting " + URLs.Count.ToString() + " available game infos and missing covers."
        NewLoadingWindow.LoadProgressBar.Value = 0
        NewLoadingWindow.LoadProgressBar.Maximum = URLs.Count

        GetGameInfos()
    End Sub

    Private Sub GetGameInfos()
        If URLs.Count > 0 Then
            PSXDatacenterBrowser.Navigate(URLs.Item(0))
        End If
    End Sub

    Private Sub PSXDatacenterBrowser_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles PSXDatacenterBrowser.DocumentCompleted
        RemoveHandler PSXDatacenterBrowser.DocumentCompleted, AddressOf PSXDatacenterBrowser_DocumentCompleted

        Dim GameTitle As String = ""
        Dim GameCode As String = ""
        Dim GameRegion As String = ""
        Dim GameCoverSource As String = ""
        Dim GameGenre As String = ""
        Dim GameDeveloper As String = ""
        Dim GamePublisher As String = ""
        Dim GameReleaseDate As String = ""
        Dim GameDescription As String = ""

        'Get game infos
        Dim infoTable As HtmlElementCollection = Nothing
        If PSXDatacenterBrowser.Document.GetElementById("table4") IsNot Nothing AndAlso PSXDatacenterBrowser.Document.GetElementById("table4").GetElementsByTagName("tr").Count > 0 Then
                infoTable = PSXDatacenterBrowser.Document.GetElementById("table4").GetElementsByTagName("tr")
            End If
        Dim coverTableRows As HtmlElementCollection = Nothing
        If PSXDatacenterBrowser.Document.GetElementById("table2") IsNot Nothing AndAlso PSXDatacenterBrowser.Document.GetElementById("table2").GetElementsByTagName("tr").Count > 0 Then
            coverTableRows = PSXDatacenterBrowser.Document.GetElementById("table2").GetElementsByTagName("tr")
        End If

        If infoTable.Count >= 7 Then
            'Game Title
            If infoTable.Item(0).Children.Count >= 1 Then
                GameTitle = infoTable.Item(0).Children(1).InnerText.Trim()
            End If

            'GameCode
            If infoTable.Item(2).Children.Count >= 1 Then
                    GameCode = infoTable.Item(2).Children(1).InnerText.Trim()
                End If

            'Region
            If infoTable.Item(3).Children.Count >= 1 Then
                Dim Region As String = infoTable.Item(3).Children(1).InnerText.Trim()
                Select Case Region
                    Case "PAL"
                        GameRegion = "Europe"
                    Case "NTSC-U"
                        GameRegion = "US"
                    Case "NTSC-J"
                        GameRegion = "Japan"
                End Select
            End If

            'Genre
            If infoTable.Item(4).Children.Count >= 1 Then
                GameGenre = infoTable.Item(4).Children(1).InnerText.Trim()
            End If

            'Developer
            If infoTable.Item(5).Children.Count >= 1 Then
                    GameDeveloper = infoTable.Item(5).Children(1).InnerText.Trim()
                End If

            'Publisher
            If infoTable.Item(6).Children.Count >= 1 Then
                GamePublisher = infoTable.Item(6).Children(1).InnerText.Trim()
            End If

            'Release Date
            If infoTable.Item(7).Children.Count >= 1 Then
                GameReleaseDate = infoTable.Item(7).Children(1).InnerText.Trim()
            End If
        End If

        'Get the game description
        If PSXDatacenterBrowser.Document.GetElementById("table16") IsNot Nothing AndAlso PSXDatacenterBrowser.Document.GetElementById("table16").GetElementsByTagName("tr").Count >= 0 Then
            GameDescription = PSXDatacenterBrowser.Document.GetElementById("table16").GetElementsByTagName("tr")(0).InnerText
        End If

        If coverTableRows.Count >= 2 Then
            If coverTableRows.Item(2) IsNot Nothing AndAlso coverTableRows.Item(2).GetElementsByTagName("img").Count > 0 Then
                GameCoverSource = coverTableRows.Item(2).GetElementsByTagName("img")(0).GetAttribute("src").Trim()
            End If
        End If

        If Not String.IsNullOrEmpty(GameCode) Then
                For Each Game In GamesListView.Items
                    Dim FoundGame As PS1Game = CType(Game, PS1Game)
                    If Not String.IsNullOrEmpty(FoundGame.GameTitle) Then
                        If FoundGame.GameTitle.Contains(GameCode) Or FoundGame.GameTitle = GameCode Then
                        FoundGame.GameRegion = GameRegion
                        FoundGame.GameGenre = GameGenre
                        FoundGame.GameDeveloper = GameDeveloper
                        FoundGame.GamePublisher = GamePublisher
                        FoundGame.GameReleaseDate = GameReleaseDate
                        FoundGame.GameDescription = GameDescription

                        If FoundGame.GameCoverSource Is Nothing Then
                            If Dispatcher.CheckAccess() = False Then
                                Dispatcher.BeginInvoke(Sub()
                                                           Dim TempBitmapImage = New BitmapImage()
                                                           TempBitmapImage.BeginInit()
                                                           TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                           TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                           TempBitmapImage.UriSource = New Uri(GameCoverSource, UriKind.RelativeOrAbsolute)
                                                           TempBitmapImage.EndInit()
                                                           FoundGame.GameCoverSource = TempBitmapImage
                                                       End Sub)
                            Else
                                Dim TempBitmapImage = New BitmapImage()
                                TempBitmapImage.BeginInit()
                                TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                TempBitmapImage.UriSource = New Uri(GameCoverSource, UriKind.RelativeOrAbsolute)
                                TempBitmapImage.EndInit()
                                FoundGame.GameCoverSource = TempBitmapImage
                            End If
                        End If
                    End If
                    ElseIf Not String.IsNullOrEmpty(FoundGame.GameID) Then
                        If FoundGame.GameID.Contains(GameCode) Or FoundGame.GameID = GameCode Then
                        FoundGame.GameTitle = GameTitle
                        FoundGame.GameRegion = GameRegion
                        FoundGame.GameGenre = GameGenre
                        FoundGame.GameDeveloper = GameDeveloper
                        FoundGame.GamePublisher = GamePublisher
                        FoundGame.GameReleaseDate = GameReleaseDate
                        FoundGame.GameDescription = GameDescription

                        If FoundGame.GameCoverSource Is Nothing Then
                            If Dispatcher.CheckAccess() = False Then
                                Dispatcher.BeginInvoke(Sub()
                                                           Dim TempBitmapImage = New BitmapImage()
                                                           TempBitmapImage.BeginInit()
                                                           TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                                           TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                                           TempBitmapImage.UriSource = New Uri(GameCoverSource, UriKind.RelativeOrAbsolute)
                                                           TempBitmapImage.EndInit()
                                                           FoundGame.GameCoverSource = TempBitmapImage
                                                       End Sub)
                            Else
                                Dim TempBitmapImage = New BitmapImage()
                                TempBitmapImage.BeginInit()
                                TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
                                TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                TempBitmapImage.UriSource = New Uri(GameCoverSource, UriKind.RelativeOrAbsolute)
                                TempBitmapImage.EndInit()
                                FoundGame.GameCoverSource = TempBitmapImage
                            End If
                        End If
                    End If
                    End If
                Next
            End If

        AddHandler PSXDatacenterBrowser.DocumentCompleted, AddressOf PSXDatacenterBrowser_DocumentCompleted

        If CurrentKeyCount < URLs.Count Then
            PSXDatacenterBrowser.Navigate(URLs.Item(CurrentKeyCount))
            CurrentKeyCount += 1
            NewLoadingWindow.LoadProgressBar.Value = CurrentKeyCount
        Else
            CurrentKeyCount = 0
            URLs.Clear()
            NewLoadingWindow.Close()
            GamesListView.Items.Refresh()
        End If
    End Sub

#End Region

#Region "Menu Actions"

    Private Sub LoadFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadFolderMenuItem.Click
        Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Select your PS1 backup folder"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then

            For Each GameBIN In Directory.GetFiles(FBD.SelectedPath, "*.bin", SearchOption.AllDirectories)
                If GameBIN.Contains("Track") Then
                    If Not GameBIN.Contains("(Track 1).bin") Then
                        'Skip
                        Continue For
                    Else
                        BINCount += 1
                    End If
                Else
                    BINCount += 1
                End If
            Next

            NewLoadingWindow = New SyncWindow() With {.Title = "Loading PS1 files", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.Maximum = BINCount
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + BINCount.ToString()
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
            Dim SelectedPS1Game As PS1Game = CType(GamesListView.SelectedItem, PS1Game)
            Dim FBD As New FolderBrowserDialog() With {.Description = "Where do you want to save the selected game ?"}

            If FBD.ShowDialog() = Forms.DialogResult.OK Then
                Dim NewCopyWindow As New CopyWindow() With {.ShowActivated = True,
                    .WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    .BackupPath = SelectedPS1Game.GameFilePath,
                    .BackupDestinationPath = FBD.SelectedPath + "\",
                    .Title = "Copying " + SelectedPS1Game.GameID + " to " + FBD.SelectedPath}

                If NewCopyWindow.ShowDialog() = True Then
                    MsgBox("Game copied with success !", MsgBoxStyle.Information, "Completed")
                End If
            End If

        End If
    End Sub

#End Region

    Public Shared Function GetRegionChar(GameID As String) As String
        If GameID.StartsWith("SLES", StringComparison.OrdinalIgnoreCase) Then
            Return "P"
        ElseIf GameID.StartsWith("SCES", StringComparison.OrdinalIgnoreCase) Then
            Return "P"
        ElseIf GameID.StartsWith("SLUS", StringComparison.OrdinalIgnoreCase) Then
            Return "U"
        ElseIf GameID.StartsWith("SCUS", StringComparison.OrdinalIgnoreCase) Then
            Return "U"
        ElseIf GameID.StartsWith("SLPS", StringComparison.OrdinalIgnoreCase) Then
            Return "J"
        ElseIf GameID.StartsWith("SLPM", StringComparison.OrdinalIgnoreCase) Then
            Return "J"
        ElseIf GameID.StartsWith("SCCS", StringComparison.OrdinalIgnoreCase) Then
            Return "J"
        ElseIf GameID.StartsWith("SLKA", StringComparison.OrdinalIgnoreCase) Then
            Return "J"
        Else
            Return ""
        End If
    End Function

    Private Sub GamesListView_PreviewMouseWheel(sender As Object, e As MouseWheelEventArgs) Handles GamesListView.PreviewMouseWheel
        Dim OpenWindowsListViewScrollViewer As ScrollViewer = Utils.FindScrollViewer(GamesListView)
        Dim HorizontalOffset As Double = OpenWindowsListViewScrollViewer.HorizontalOffset
        OpenWindowsListViewScrollViewer.ScrollToHorizontalOffset(HorizontalOffset - (e.Delta / 100))
        e.Handled = True
    End Sub

    Private Sub GamesListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles GamesListView.SelectionChanged
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS1Game As PS1Game = CType(GamesListView.SelectedItem, PS1Game)

            GameTitleTextBlock.Text = SelectedPS1Game.GameTitle
            GameIDTextBlock.Text = "Title ID: " & SelectedPS1Game.GameID
            GameRegionTextBlock.Text = "Region: " & SelectedPS1Game.GameRegion
            GameGenreTextBlock.Text = "Genre: " & SelectedPS1Game.GameGenre
            GameDeveloperTextBlock.Text = "Developer: " & SelectedPS1Game.GameDeveloper

            GameDescriptionTextBlock.Text = "Hover for Game Description"
            GameDescriptionTextBlock.ToolTip = SelectedPS1Game.GameDescription

            GameSizeTextBlock.Text = "Size: " & SelectedPS1Game.GameSize
            GamePublisherTextBlock.Text = "Publisher: " & SelectedPS1Game.GamePublisher
            GameReleaseDateTextBlock.Text = "Release Date: " & SelectedPS1Game.GameReleaseDate

            If Not String.IsNullOrEmpty(SelectedPS1Game.GameFilePath) Then
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " & New DirectoryInfo(Path.GetDirectoryName(SelectedPS1Game.GameFilePath)).Name
            Else
                GameBackupFolderNameTextBlock.Text = "Backup Folder: " & New DirectoryInfo(SelectedPS1Game.GameFolderPath).Name
            End If

        End If
    End Sub

End Class
