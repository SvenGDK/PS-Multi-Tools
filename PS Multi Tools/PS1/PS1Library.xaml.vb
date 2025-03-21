Imports System.ComponentModel
Imports System.IO

Public Class PS1Library

    Dim WithEvents GameLoaderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}
    Dim WithEvents PSXDatacenterBrowser As New Forms.WebBrowser()
    Dim WithEvents NewLoadingWindow As New SyncWindow() With {.Title = "Loading PS1 files", .ShowActivated = True}

    Dim URLs As New List(Of String)()
    Dim CurrentKeyCount As Integer = 0

    Dim BINCount As Integer = 0
    Dim VCDCount As Integer = 0

    'Selected game context menu
    Dim WithEvents NewContextMenu As New Controls.ContextMenu()
    Dim WithEvents CopyToMenuItem As New Controls.MenuItem() With {.Header = "Copy to", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/copy-icon.png", UriKind.Relative))}}
    Dim WithEvents PlayGameMenuItem As New Controls.MenuItem() With {.Header = "Play with ePSXe", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/controller.png", UriKind.Relative))}}
    Dim WithEvents CreateProjectMenuItem As New Controls.MenuItem() With {.Header = "Create a game project for the PSX", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/copy-icon.png", UriKind.Relative))}}

    'Supplemental library menu items
    Dim WithEvents LoadFolderMenuItem As New Controls.MenuItem() With {.Header = "Load a new folder"}
    Dim WithEvents LoadDLFolderMenuItem As New Controls.MenuItem() With {.Header = "Open Downloads folder"}

    'Supplemental emulator menu item
    Dim WithEvents EMU_Settings As New Controls.MenuItem() With {.Header = "ePSXe Settings"}

    Private Sub PS1Library_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Set the controls in the shared library
        NewPS1Menu.GamesLView = GamesListView

        'Add supplemental library menu items that will be handled in the app
        Dim LibraryMenuItem As Controls.MenuItem = CType(NewPS1Menu.Items(0), Controls.MenuItem)
        LibraryMenuItem.Items.Add(LoadFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem)

        NewContextMenu.Items.Add(CopyToMenuItem)
        NewContextMenu.Items.Add(PlayGameMenuItem)
        NewContextMenu.Items.Add(CreateProjectMenuItem)
        GamesListView.ContextMenu = NewContextMenu

        'Add supplemental emulator menu item
        If File.Exists(Environment.CurrentDirectory + "\Emulators\ePSXe\ePSXe.exe") Then
            NewPS1Menu.Items.Add(EMU_Settings)
        End If
    End Sub

#Region "Game Loader"

    Private Sub GameLoaderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles GameLoaderWorker.DoWork

        Dim GamesList As New List(Of String)()
        Dim FailList As New List(Of String)()

        Dim FoundGames As IEnumerable(Of String) = Directory.EnumerateFiles(e.Argument.ToString, "*.*", SearchOption.AllDirectories).Where(Function(s) s.EndsWith(".bin") OrElse s.EndsWith(".BIN") OrElse s.EndsWith(".VCD"))

        For Each Game In FoundGames

            Dim GameInfo As New FileInfo(Game)

            'Skip multiple "Track" files and only read the first one
            If GameInfo.Name.Contains("track", StringComparison.CurrentCultureIgnoreCase) Then
                If Not Game.Contains("(track 1).bin", StringComparison.CurrentCultureIgnoreCase) OrElse Not GameInfo.Name.Contains("(track 01).bin", StringComparison.CurrentCultureIgnoreCase) Then
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
                WindowsCMD.StartInfo.Arguments = "/c strings.exe /accepteula -nobanner -b 7340032 """ + Game + """ | findstr BOOT"
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
                            Dim GameID As String = OutputLine.Replace("BOOT = cdrom:\", "").Replace("BOOT=cdrom:\", "").Replace("BOOT = cdrom:", "").Replace(";1", "").Replace("_", "-").Replace(".", "").Replace("MGS\", "").Trim()
                            Dim RegionCharacter As String = PS1Game.GetRegionChar(GameID)

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
                                        NewPS1Game.GameTitle = PS1Game.GetPS1GameTitleFromDatabaseList(UCase(GameID).Trim())
                                    End If
                                Else
                                    If Utils.IsURLValid("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html") Then
                                        URLs.Add("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html")
                                    Else
                                        NewPS1Game.GameTitle = PS1Game.GetPS1GameTitleFromDatabaseList(UCase(GameID).Trim())
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
                                               NewLoadingWindow.LoadStatusTextBlock.Text = "Loading bin " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + (BINCount + VCDCount).ToString()
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
                                    Dim GameID As String = OutputLine.Replace("BOOT = cdrom:\", "").Replace("BOOT=cdrom:\", "").Replace("BOOT = cdrom:", "").Replace(";1", "").Replace("_", "-").Replace(".", "").Replace("MGS\", "").Trim()
                                    Dim RegionCharacter As String = PS1Game.GetRegionChar(GameID)

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
                                                NewPS1Game.GameTitle = PS1Game.GetPS1GameTitleFromDatabaseList(UCase(GameID).Trim())
                                            End If
                                        Else
                                            If Utils.IsURLValid("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html") Then
                                                URLs.Add("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html")
                                            Else
                                                NewPS1Game.GameTitle = PS1Game.GetPS1GameTitleFromDatabaseList(UCase(GameID).Trim())
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

    Private Sub PSXDatacenterBrowser_DocumentCompleted(sender As Object, e As Forms.WebBrowserDocumentCompletedEventArgs) Handles PSXDatacenterBrowser.DocumentCompleted
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
        Dim infoTable As Forms.HtmlElementCollection = Nothing
        If PSXDatacenterBrowser.Document.GetElementById("table4") IsNot Nothing AndAlso PSXDatacenterBrowser.Document.GetElementById("table4").GetElementsByTagName("tr").Count > 0 Then
            infoTable = PSXDatacenterBrowser.Document.GetElementById("table4").GetElementsByTagName("tr")
        End If
        Dim coverTableRows As Forms.HtmlElementCollection = Nothing
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

            GamesListView.Items.Clear()
            URLs.Clear()
            BINCount = 0
            VCDCount = 0

            For Each GameBIN In Directory.GetFiles(FBD.SelectedPath, "*.bin", SearchOption.AllDirectories)
                Dim GameInfo As New FileInfo(GameBIN)
                If GameInfo.Name.ToLower().Contains("track") Then
                    If Not GameBIN.ToLower().Contains("(track 1).bin") OrElse Not GameInfo.Name.ToLower().Contains("(track 01).bin") Then
                        'Skip
                        Continue For
                    Else
                        BINCount += 1
                    End If
                Else
                    BINCount += 1
                End If
            Next

            VCDCount = Directory.GetFiles(FBD.SelectedPath, "*.VCD", SearchOption.AllDirectories).Count

            NewLoadingWindow = New SyncWindow() With {.Title = "Loading PS1 files", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.Maximum = BINCount + VCDCount
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + (BINCount + VCDCount).ToString()
            NewLoadingWindow.Show()

            GameLoaderWorker.RunWorkerAsync(FBD.SelectedPath)
        End If
    End Sub

    Private Sub LoadDLFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadDLFolderMenuItem.Click
        If Directory.Exists(Environment.CurrentDirectory + "\Downloads") Then
            Process.Start("explorer", Environment.CurrentDirectory + "\Downloads")
        End If
    End Sub

#End Region

#Region "Contextmenu Actions"

    Private Sub CopyToMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles CopyToMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS1Game As PS1Game = CType(GamesListView.SelectedItem, PS1Game)
            Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Where do you want to save the selected game ?"}

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

    Private Sub PlayGameMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PlayGameMenuItem.Click
        If File.Exists(Environment.CurrentDirectory + "\Emulators\ePSXe\ePSXe.exe") Then
            If GamesListView.SelectedItem IsNot Nothing Then
                Dim SelectedPS1Game As PS1Game = CType(GamesListView.SelectedItem, PS1Game)

                'Check if any PS1 BIOS file is available
                If Not Directory.GetFiles(Environment.CurrentDirectory + "\Emulators\ePSXe\bios", "*.bin", SearchOption.TopDirectoryOnly).Count > 0 Then
                    If MsgBox("No PS1 BIOS file available." + vbCrLf + "You need at least one BIOS file installed in order to play " + SelectedPS1Game.GameTitle + "." + vbCrLf +
                              "Do you want to copy a BIOS file to the Emulators folder of PS Multi Tools ?", MsgBoxStyle.YesNo, "Cannot launch game") = MsgBoxResult.Yes Then

                        'Get a BIOS file from OpenFileDialog
                        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Select a PS1 BIOS file", .Filter = "PS1 BIOS (*.bin)|*.bin", .Multiselect = False}
                        If OFD.ShowDialog() = Forms.DialogResult.OK Then
                            Dim SelectedBIOSFile As String = OFD.FileName
                            Dim SelectedBIOSFileName As String = Path.GetFileName(SelectedBIOSFile)

                            'Copy to the BIOS folder
                            File.Copy(SelectedBIOSFile, Environment.CurrentDirectory + "\Emulators\ePSXe\bios\" + SelectedBIOSFileName, True)

                            'Proceed
                            If MsgBox("Start " + SelectedPS1Game.GameTitle + " using ePSXe ?" + vbCrLf + vbCrLf +
                                      "If the game doesn't start then you have to set the BIOS manually using ePSXe.exe in \Emulators\ePSXe (Config -> BIOS).", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then
                                Dim EmulatorLauncherStartInfo As New ProcessStartInfo()
                                Dim EmulatorLauncher As New Process() With {.StartInfo = EmulatorLauncherStartInfo}
                                EmulatorLauncherStartInfo.FileName = Environment.CurrentDirectory + "\Emulators\ePSXe\ePSXe.exe"
                                EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(Environment.CurrentDirectory + "\Emulators\ePSXe\ePSXe.exe")
                                EmulatorLauncherStartInfo.Arguments = "-nogui -loadbin """ + SelectedPS1Game.GameFilePath + """"
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
                    If MsgBox("Start " + SelectedPS1Game.GameTitle + " using ePSXe ?" + vbCrLf + vbCrLf +
                                      "If the game doesn't start then you have to set the BIOS manually using ePSXe.exe in \Emulators\ePSXe (Config -> BIOS).", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then
                        Dim EmulatorLauncherStartInfo As New ProcessStartInfo()
                        Dim EmulatorLauncher As New Process() With {.StartInfo = EmulatorLauncherStartInfo}
                        EmulatorLauncherStartInfo.FileName = Environment.CurrentDirectory + "\Emulators\ePSXe\ePSXe.exe"
                        EmulatorLauncherStartInfo.WorkingDirectory = Path.GetDirectoryName(Environment.CurrentDirectory + "\Emulators\ePSXe\ePSXe.exe")
                        EmulatorLauncherStartInfo.Arguments = "-nogui -loadbin """ + SelectedPS1Game.GameFilePath + """"
                        EmulatorLauncher.Start()
                    End If

                End If
            End If
        Else
            MsgBox("Cannot start ePSXe." + vbCrLf + "Emulator pack is not installed.", MsgBoxStyle.Critical, "Error")
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

    Private Sub EMU_Settings_Click(sender As Object, e As RoutedEventArgs) Handles EMU_Settings.Click
        Dim NewPS1EmulatorSettingsWindow As New PS1EmulatorSettings() With {.ShowActivated = True}
        NewPS1EmulatorSettingsWindow.Show()
    End Sub

    Private Sub CreateProjectMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles CreateProjectMenuItem.Click
        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedPS1Game As PS1Game = CType(GamesListView.SelectedItem, PS1Game)

            If Path.GetExtension(SelectedPS1Game.GameFilePath) = ".VCD" Then
                Dim GameProjectDirectory As String = SelectedPS1Game.GameTitle + " [" + SelectedPS1Game.GameID + "]"
                Dim NewGameProjectDirectory As String = Environment.CurrentDirectory + "\Projects\" + SelectedPS1Game.GameTitle + " [" + SelectedPS1Game.GameID + "]"

                Dim NewGameProjectWindow As New PSXNewPS1GameProject() With {.ShowActivated = True}
                Dim NewGameEditor As New PSXPS1GameEditor() With {.ProjectDirectory = NewGameProjectDirectory, .Title = "Game Ressources Editor - " + NewGameProjectDirectory}

                'Set project information
                NewGameProjectWindow.ImportFromPSMT(SelectedPS1Game.GameFilePath, SelectedPS1Game.GameTitle, NewGameProjectDirectory, SelectedPS1Game.GameID)

                'Create game project directory
                If Not Directory.Exists(NewGameProjectDirectory) Then
                    Directory.CreateDirectory(NewGameProjectDirectory)
                End If

                'Write Project settings to .CFG
                Using ProjectWriter As New StreamWriter(Environment.CurrentDirectory + "\Projects\" + SelectedPS1Game.GameTitle + ".CFG", False)
                    ProjectWriter.WriteLine("TITLE=" + SelectedPS1Game.GameTitle)
                    ProjectWriter.WriteLine("ID=" + SelectedPS1Game.GameID)
                    ProjectWriter.WriteLine("DIR=" + NewGameProjectDirectory)
                    ProjectWriter.WriteLine("ELForISO=" + SelectedPS1Game.GameFilePath)
                    ProjectWriter.WriteLine("TYPE=GAME")
                    ProjectWriter.WriteLine("SIGNED=FALSE")
                    ProjectWriter.WriteLine("GAMETYPE=PS1")
                End Using

                'Write SYSTEM.CNF to project directory
                Using CNFWriter As New StreamWriter(NewGameProjectDirectory + "\SYSTEM.CNF", False)
                    CNFWriter.WriteLine("BOOT2 = pfs:/EXECUTE.KELF")
                    CNFWriter.WriteLine("VER = 1.01")
                    CNFWriter.WriteLine("VMODE = NTSC")
                    CNFWriter.WriteLine("HDDUNITPOWER = NICHDD")
                End Using

                'Write icon.sys to project directory
                Using CNFWriter As New StreamWriter(NewGameProjectDirectory + "\icon.sys", False)
                    CNFWriter.WriteLine("PS2X")
                    CNFWriter.WriteLine("title0=" + SelectedPS1Game.GameTitle)
                    CNFWriter.WriteLine("title1=" + SelectedPS1Game.GameID)
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
                    SYSWriter.WriteLine("title = " + SelectedPS1Game.GameTitle)
                    SYSWriter.WriteLine("title_id = " + SelectedPS1Game.GameID)
                    SYSWriter.WriteLine("title_sub_id = 0")
                    SYSWriter.WriteLine("release_date = " + SelectedPS1Game.GameReleaseDate)
                    SYSWriter.WriteLine("developer_id = " + SelectedPS1Game.GameDeveloper)
                    SYSWriter.WriteLine("publisher_id = " + SelectedPS1Game.GamePublisher)
                    SYSWriter.WriteLine("note = ")
                    SYSWriter.WriteLine("content_web = ")
                    SYSWriter.WriteLine("image_topviewflag = 0")
                    SYSWriter.WriteLine("image_type = 0")
                    SYSWriter.WriteLine("image_count = 1")
                    SYSWriter.WriteLine("image_viewsec = 600")
                    SYSWriter.WriteLine("copyright_viewflag = 0")
                    SYSWriter.WriteLine("copyright_imgcount = 1")
                    SYSWriter.WriteLine("genre = " + SelectedPS1Game.GameGenre)
                    SYSWriter.WriteLine("parental_lock = 1")
                    SYSWriter.WriteLine("effective_date = 0")
                    SYSWriter.WriteLine("expire_date = 0")

                    Select Case SelectedPS1Game.GameRegion
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
                    MANWriter.WriteLine("<TITLE id=""TOP-TITLE"" label=""" + SelectedPS1Game.GameTitle + """ />")
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
                Dim GameStartLetter As String = SelectedPS1Game.GameTitle.Substring(0, 1)
                Dim RegionCharacter As String = PS1Game.GetRegionChar(SelectedPS1Game.GameID)

                If Utils.IsURLValid("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + SelectedPS1Game.GameID + ".html") Then
                    NewGameEditor.PSXDatacenterBrowser.Navigate("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + SelectedPS1Game.GameID + ".html")
                Else
                    'Apply cover, title and region only if no data is available on PSXDatacenter
                    NewGameEditor.ApplyKnownValues(SelectedPS1Game.GameID, SelectedPS1Game.GameTitle)
                End If
            Else
                MsgBox("Games in BIN format cannot be installed directly on the HDD, please convert it with cue2pops using PS Multi Tools.", MsgBoxStyle.Information, "BIN files not supported")
            End If

        End If
    End Sub

End Class
