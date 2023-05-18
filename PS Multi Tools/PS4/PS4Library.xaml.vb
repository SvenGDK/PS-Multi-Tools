Imports System.ComponentModel
Imports System.IO
Imports PS4_Tools
Imports psmt_lib

Public Class PS4Library

    Dim WithEvents GameLoaderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}
    Dim WithEvents NewLoadingWindow As New SyncWindow() With {.Title = "Loading PS4 pkg files", .ShowActivated = True}

    'Selected game context menu
    Dim WithEvents NewContextMenu As New ContextMenu()
    Dim WithEvents CopyToMenuItem As New Controls.MenuItem() With {.Header = "Copy to", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/copy-icon.png", UriKind.Relative))}}
    Dim WithEvents SendToMenuItem As New Controls.MenuItem() With {.Header = "Send to PS4", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/send-icon.png", UriKind.Relative))}}
    Dim WithEvents PKGInfoMenuItem As New Controls.MenuItem() With {.Header = "PKG Details", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/information-button.png", UriKind.Relative))}}
    Dim WithEvents PSNInfoMenuItem As New Controls.MenuItem() With {.Header = "Store Details", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/information-button.png", UriKind.Relative))}}
    Dim WithEvents PlayMenuItem As New Controls.MenuItem() With {.Header = "Play Soundtrack", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Play-icon.png", UriKind.Relative))}}

    Dim PKGCount As Integer = 0
    Dim IsSoundPlaying As Boolean = False

    Dim WithEvents LoadFolderMenuItem As New Controls.MenuItem() With {.Header = "Load a new folder"}

    Private Sub PS4Library_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Add supplemental library menu items that will be handled in the app
        Dim LibraryMenuItem As MenuItem = CType(NewPS4Menu.Items(0), MenuItem)
        LibraryMenuItem.Items.Add(LoadFolderMenuItem)

        'Add the games context menu
        NewContextMenu.Items.Add(CopyToMenuItem)
        NewContextMenu.Items.Add(SendToMenuItem)
        NewContextMenu.Items.Add(PKGInfoMenuItem)
        NewContextMenu.Items.Add(PSNInfoMenuItem)
        NewContextMenu.Items.Add(PlayMenuItem)
        GamesListView.ContextMenu = NewContextMenu
    End Sub

#Region "Game Loader"

    Private Sub LoadFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadFolderMenuItem.Click
        Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Select your PS4 backup folder"}

        If FBD.ShowDialog() = Forms.DialogResult.OK Then

            'Set the count of pkg files
            PKGCount = Directory.GetFiles(FBD.SelectedPath, "*.pkg", SearchOption.AllDirectories).Count

            'Show the loading progress window
            NewLoadingWindow = New SyncWindow() With {.Title = "Loading PS4 pkg files", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.Maximum = PKGCount
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + PKGCount.ToString()
            NewLoadingWindow.Show()

            'Load the pkg files
            GameLoaderWorker.RunWorkerAsync(FBD.SelectedPath)
        End If

    End Sub

    Private Sub GameLoaderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles GameLoaderWorker.DoWork

        'PS4 PKGs
        For Each Game In Directory.GetFiles(e.Argument.ToString, "*.pkg", SearchOption.AllDirectories)
            Dim NewPS4Game As New PS4Game()
            Dim GamePKG As PKG.SceneRelated.Unprotected_PKG = PKG.SceneRelated.Read_PKG(Game)
            Dim PKGImgBitmap As BitmapSource = Nothing

            'Set game infos
            NewPS4Game.GameTitle = GamePKG.PS4_Title
            NewPS4Game.GameID = GamePKG.Content_ID
            NewPS4Game.GameContentID = GamePKG.Content_ID
            NewPS4Game.GameFilePath = Game
            NewPS4Game.GameRegion = GamePKG.Region.Replace("(", "").Replace(")", "").Trim()
            NewPS4Game.GameRequiredFW = GamePKG.Firmware_Version
            NewPS4Game.GameSize = GamePKG.Size

            If GamePKG.Param IsNot Nothing And Not String.IsNullOrEmpty(GamePKG.Param.Category) Then
                NewPS4Game.GameCategory = PS4Game.GetCategory(GamePKG.Param.Category)
            End If
            If GamePKG.Icon IsNot Nothing Then
                Dispatcher.BeginInvoke(Sub() NewPS4Game.GameCoverSource = Utils.BitmapSourceFromByteArray(GamePKG.Icon))
            End If
            If GamePKG.Sound IsNot Nothing Then
                NewPS4Game.GameSoundtrackBytes = Media.Atrac9.LoadAt9(GamePKG.Sound)
            End If

            Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadProgressBar.Value += 1)
            Dispatcher.BeginInvoke(Sub() NewLoadingWindow.LoadStatusTextBlock.Text = "Loading PKG " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + PKGCount.ToString())

            Select Case GamePKG.Param.Category
                Case "ac"
                    'Add to the ListView
                    If DLCsListView.Dispatcher.CheckAccess() = False Then
                        DLCsListView.Dispatcher.BeginInvoke(Sub() DLCsListView.Items.Add(NewPS4Game))
                    Else
                        DLCsListView.Items.Add(NewPS4Game)
                    End If
                Case "gd"
                    'Add to the ListView
                    If GamesListView.Dispatcher.CheckAccess() = False Then
                        GamesListView.Dispatcher.BeginInvoke(Sub() GamesListView.Items.Add(NewPS4Game))
                    Else
                        GamesListView.Items.Add(NewPS4Game)
                    End If
                Case "gp"
                    'Add to the ListView
                    If UpdatesListView.Dispatcher.CheckAccess() = False Then
                        UpdatesListView.Dispatcher.BeginInvoke(Sub() UpdatesListView.Items.Add(NewPS4Game))
                    Else
                        UpdatesListView.Items.Add(NewPS4Game)
                    End If
                Case Else
                    'Add to the ListView
                    If OthersListView.Dispatcher.CheckAccess() = False Then
                        OthersListView.Dispatcher.BeginInvoke(Sub() OthersListView.Items.Add(NewPS4Game))
                    Else
                        OthersListView.Items.Add(NewPS4Game)
                    End If
            End Select

        Next

    End Sub

    Private Sub GameLoaderWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles GameLoaderWorker.RunWorkerCompleted
        NewLoadingWindow.Close()
    End Sub

#End Region

#Region "Contextmenu Actions"

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
            Dim NewPKGInfo As New PKGInfo() With {.SelectedPKG = SelectedPS4Game.GameFilePath}
            NewPKGInfo.Show()
        End If
    End Sub

#End Region

End Class
