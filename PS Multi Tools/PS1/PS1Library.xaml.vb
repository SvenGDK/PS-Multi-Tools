Imports System.ComponentModel
Imports System.IO
Imports System.Windows.Forms

Public Class PS1Library

    Dim WithEvents GameLoaderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}
    Dim WithEvents PSXDatacenterBrowser As New Forms.WebBrowser()
    Dim WithEvents NewLoadingWindow As New SyncWindow() With {.Title = "Loading PS1 files", .ShowActivated = True}

    Dim URLs As New List(Of String)
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

        Dim GamesList As New List(Of String)
        Dim FailList As New List(Of String)

        For Each Game In Directory.GetFiles(e.Argument.ToString, "*.bin", SearchOption.AllDirectories)

            'Skip multiple "Track" files and only read the first one
            If Game.Contains("Track") Then
                If Not Game.Contains("(Track 1).bin") Then
                    'Skip
                    Continue For
                End If
            End If

            Dim GameInfo As New FileInfo(Game)
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
                Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {"BOOT"}, StringSplitOptions.RemoveEmptyEntries)

                If ProcessOutput.Count > 0 Then 'Game ID found
                    Dim GameID As String = ProcessOutput(0).Replace("= cdrom:\", "").Replace("=cdrom:\", "").Replace("= cdrom:", "").Replace(";1", "").Replace("_", "-").Replace(".", "").Trim()
                    Dim RegionCharacter As String = GetRegionChar(GameID)

                    'Set known values
                    NewPS1Game.GameID = UCase(GameID)

                    'Update progress
                    Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                    Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading bin " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + BINCount.ToString())

                    'Add to the ListView
                    If GamesListView.Dispatcher.CheckAccess() = False Then
                        GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPS1Game))
                    Else
                        GamesListView.Items.Add(NewPS1Game)
                    End If

                    'Check game id length & if the generated url is valid
                    If GameID.Length = 10 Then
                        If Utils.IsURLValid("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html") Then
                            URLs.Add("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html")
                        End If
                    End If

                Else 'Game ID not found, add to the fail list
                    FailList.Add(Game)
                End If

            End Using

        Next

        If FailList.Count > 0 Then 'Ask for an extended search
            If MsgBox("Some game IDs could not be found quickly, do you want to extend the search ? This requires will require about 2-5min per game, depending on your hardware.", MsgBoxStyle.YesNo, "Not all game IDs found") = MsgBoxResult.Yes Then

                For Each Game In FailList
                    Dim GameInfo As New FileInfo(Game)
                    Dim GameStartLetter As String = GameInfo.Name.Substring(0, 1)
                    Dim NewPS1Game As New PS1Game With {.GameFilePath = Game, .GameSize = FormatNumber(GameInfo.Length / 1048576, 2) + " MB"}

                    'Search for the game ID within the first 500MB with strings & findstr
                    Using WindowsCMD As New Process()
                        WindowsCMD.StartInfo.FileName = "cmd"
                        WindowsCMD.StartInfo.Arguments = "/c strings /accepteula -b 500000000 """ + Game + """ | findstr BOOT"
                        WindowsCMD.StartInfo.RedirectStandardOutput = True
                        WindowsCMD.StartInfo.UseShellExecute = False
                        WindowsCMD.StartInfo.CreateNoWindow = True
                        WindowsCMD.Start()
                        WindowsCMD.WaitForExit()

                        Dim OutputReader As StreamReader = WindowsCMD.StandardOutput
                        Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {"BOOT"}, StringSplitOptions.RemoveEmptyEntries)

                        If ProcessOutput.Count > 0 Then
                            Dim GameID As String = ProcessOutput(0).Replace("= cdrom:\", "").Replace("=cdrom:\", "").Replace("= cdrom:", "").Replace(";1", "").Replace("_", "-").Replace(".", "").Trim()
                            Dim RegionCharacter As String = GetRegionChar(GameID)

                            'Set known values
                            NewPS1Game.GameID = UCase(GameID)

                            'Update progress
                            Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
                            Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading bin " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + BINCount.ToString())

                            'Add to the ListView
                            If GamesListView.Dispatcher.CheckAccess() = False Then
                                GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPS1Game))
                            Else
                                GamesListView.Items.Add(NewPS1Game)
                            End If

                            'Check game id length & if the generated url is valid
                            If GameID.Length = 10 Then
                                If Utils.IsURLValid("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html") Then
                                    URLs.Add("https://psxdatacenter.com/games/" + RegionCharacter + "/" + GameStartLetter + "/" + GameID + ".html")
                                End If
                            End If
                        End If

                    End Using
                Next

            End If
        End If

    End Sub

    Private Sub GameLoaderWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles GameLoaderWorker.RunWorkerCompleted
        NewLoadingWindow.LoadStatusTextBlock.Text = "Getting " + URLs.Count.ToString() + " available game infos and covers"
        NewLoadingWindow.LoadProgressBar.Value = 0
        NewLoadingWindow.LoadProgressBar.Maximum = URLs.Count

        GetGameCovers()
    End Sub

    Private Sub GetGameCovers()
        If Not URLs.Count = 0 Then
            PSXDatacenterBrowser.Navigate(URLs.Item(0))
        End If
    End Sub

    Private Sub PSXDatacenterBrowser_DocumentCompleted(sender As Object, e As Forms.WebBrowserDocumentCompletedEventArgs) Handles PSXDatacenterBrowser.DocumentCompleted

        On Error Resume Next
        RemoveHandler PSXDatacenterBrowser.DocumentCompleted, AddressOf PSXDatacenterBrowser_DocumentCompleted

        Dim GameTitle As String = ""
        Dim GameCode As String = ""
        Dim GameRegion As String = ""
        Dim GameCoverSource As String = ""

        'Get game infos
        Dim infoTable As Forms.HtmlElementCollection = Nothing
        If PSXDatacenterBrowser.Document.GetElementById("table4") IsNot Nothing Then
            infoTable = PSXDatacenterBrowser.Document.GetElementById("table4").GetElementsByTagName("tr")
        End If
        Dim coverTableRows As Forms.HtmlElementCollection = Nothing
        If PSXDatacenterBrowser.Document.GetElementById("table2") IsNot Nothing Then
            coverTableRows = PSXDatacenterBrowser.Document.GetElementById("table2").GetElementsByTagName("tr")
        End If

        If infoTable IsNot Nothing Then
            'Game Title
            If infoTable.Item(0).Children(1) IsNot Nothing Then
                GameTitle = infoTable.Item(0).Children(1).InnerText.Trim()
            End If

            'GameCode
            If infoTable.Item(2).Children(1) IsNot Nothing Then
                GameCode = infoTable.Item(2).Children(1).InnerText.Trim()
            End If

            'Region
            If infoTable.Item(3).Children(1) IsNot Nothing Then
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
        End If

        If coverTableRows IsNot Nothing Then
            If coverTableRows.Item(2).GetElementsByTagName("img") IsNot Nothing Then
                GameCoverSource = coverTableRows.Item(2).GetElementsByTagName("img")(0).GetAttribute("src").Trim()
            End If
        End If

        For Each Game In GamesListView.Items
            Dim FoundGame As PS1Game = CType(Game, PS1Game)

            If FoundGame.GameID.Contains(GameCode) Or FoundGame.GameID = GameCode Then
                FoundGame.GameTitle = GameTitle
                FoundGame.GameRegion = GameRegion
                FoundGame.GameCoverSource = New BitmapImage(New Uri(GameCoverSource, UriKind.RelativeOrAbsolute))
            End If
        Next

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

End Class
