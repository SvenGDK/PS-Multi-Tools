Imports System.ComponentModel
Imports System.IO
Imports System.Windows.Media.Animation
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

    'Supplemental library menu items
    Dim WithEvents LoadFolderMenuItem As New Controls.MenuItem() With {.Header = "Load a new folder"}
    Dim WithEvents LoadDLFolderMenuItem As New Controls.MenuItem() With {.Header = "Open Downloads folder"}

    Private Sub PS4Library_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Add supplemental library menu items that will be handled in the app
        Dim LibraryMenuItem As MenuItem = CType(NewPS4Menu.Items(0), MenuItem)
        LibraryMenuItem.Items.Add(LoadFolderMenuItem)
        LibraryMenuItem.Items.Add(LoadDLFolderMenuItem)

        'Add the games context menu
        NewContextMenu.Items.Add(CopyToMenuItem)
        NewContextMenu.Items.Add(SendToMenuItem)
        NewContextMenu.Items.Add(PKGInfoMenuItem)
        NewContextMenu.Items.Add(PSNInfoMenuItem)
        NewContextMenu.Items.Add(PlayMenuItem)
        GamesListView.ContextMenu = NewContextMenu
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
            Dim NewPKGInfo As New PKGInfo() With {.SelectedPKG = SelectedPS4Game.GameFilePath, .Console = "PS4"}
            NewPKGInfo.Show()
        End If
    End Sub

#End Region

#Region "Menu Actions"

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

End Class
