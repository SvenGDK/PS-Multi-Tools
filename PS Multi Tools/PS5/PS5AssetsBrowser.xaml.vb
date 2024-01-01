Imports System.ComponentModel
Imports System.IO
Imports System.Windows.Media.Animation
Imports DDSReader
Imports MS.Internal
Imports PS_Multi_Tools.Structures

Public Class PS5AssetsBrowser

    Dim WithEvents FileLoaderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}
    Dim WithEvents NewLoadingWindow As New SyncWindow() With {.Title = "Loading asset files", .ShowActivated = True}

    Dim ShowAssetsAnimation As New DoubleAnimation With {.From = 0, .To = 350, .Duration = New Duration(TimeSpan.FromMilliseconds(400))}
    Dim HideAssetsAnimation As New DoubleAnimation With {.From = 350, .To = 0, .Duration = New Duration(TimeSpan.FromMilliseconds(400))}

    Dim ExpandPlayerAnimation As New ThicknessAnimation With {.From = New Thickness(350, 0, 0, 0), .To = New Thickness(0, 0, 0, 0), .Duration = New Duration(TimeSpan.FromMilliseconds(400))}
    Dim ResizePlayerAnimation As New ThicknessAnimation With {.From = New Thickness(0, 0, 0, 0), .To = New Thickness(350, 0, 0, 0), .Duration = New Duration(TimeSpan.FromMilliseconds(400))}

    Dim PlayerContextMenu As New Controls.ContextMenu()
    Dim WithEvents ShowHideAssetsMenuItem As New Controls.MenuItem() With {.Header = "Show assets browser", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Assets-icon.png", UriKind.Relative))}}
    Dim WithEvents PlayPauseMenuItem As New Controls.MenuItem() With {.Header = "Pause", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Pause-icon.png", UriKind.Relative))}}
    Dim WithEvents StopMenuItem As New Controls.MenuItem() With {.Header = "Stop playblack", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Stop-icon.png", UriKind.Relative))}}

    Public SelectedDirectory As String = ""
    Dim FilesCount As Integer = 0

    Private Sub PS5AssetsBrowser_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        PlayerContextMenu.Items.Add(ShowHideAssetsMenuItem)
        PlayerContextMenu.Items.Add(PlayPauseMenuItem)
        PlayerContextMenu.Items.Add(StopMenuItem)

        AssetPlayer.ContextMenu = PlayerContextMenu
    End Sub

    Private Sub PS5AssetsBrowser_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        'Load playable assets
        FilesCount = 0
        FilesCount += Directory.GetFiles(SelectedDirectory, "*.mp4", SearchOption.AllDirectories).Count
        FilesCount += Directory.GetFiles(SelectedDirectory, "*.webm", SearchOption.AllDirectories).Count
        FilesCount += Directory.GetFiles(SelectedDirectory, "*.ogg", SearchOption.AllDirectories).Count
        FilesCount += Directory.GetFiles(SelectedDirectory, "*.png", SearchOption.AllDirectories).Count
        FilesCount += Directory.GetFiles(SelectedDirectory, "*.jpg", SearchOption.AllDirectories).Count
        FilesCount += Directory.GetFiles(SelectedDirectory, "*.ttf", SearchOption.AllDirectories).Count

        NewLoadingWindow = New SyncWindow() With {.Title = "Loading asset files", .ShowActivated = True}
        NewLoadingWindow.LoadProgressBar.Maximum = FilesCount
        NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + FilesCount.ToString()
        NewLoadingWindow.Show()

        FileLoaderWorker.RunWorkerAsync()
    End Sub

    Private Sub ShowHideAssetsMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles ShowHideAssetsMenuItem.Click
        If ShowHideAssetsMenuItem.Header.ToString = "Show assets browser" Then
            AssetFilesListView.BeginAnimation(WidthProperty, ShowAssetsAnimation)
            AssetPlayer.BeginAnimation(MarginProperty, ResizePlayerAnimation)
            ShowHideAssetsMenuItem.Header = "Hide assets browser"
        Else
            AssetFilesListView.BeginAnimation(WidthProperty, HideAssetsAnimation)
            AssetPlayer.BeginAnimation(MarginProperty, ExpandPlayerAnimation)
            ShowHideAssetsMenuItem.Header = "Show assets browser"
        End If
    End Sub

    Private Sub PlayPauseMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PlayPauseMenuItem.Click
        If AssetPlayer.Source IsNot Nothing Then
            If PlayPauseMenuItem.Header.ToString = "Pause" Then
                AssetPlayer.Pause()
                PlayPauseMenuItem.Header = "Play"
                PlayPauseMenuItem.Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Play-icon.png", UriKind.Relative))}
            Else
                AssetPlayer.Play()
                PlayPauseMenuItem.Header = "Pause"
                PlayPauseMenuItem.Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Pause-icon.png", UriKind.Relative))}
            End If
        End If
    End Sub

    Private Sub AssetFilesListView_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles AssetFilesListView.MouseDoubleClick
        If AssetFilesListView.SelectedItem IsNot Nothing Then
            Dim SelectedAsset As AssetListViewItem = CType(AssetFilesListView.SelectedItem, AssetListViewItem)

            If SelectedAsset.Type = AssetType.Video Then
                ImageViewer.Visibility = Visibility.Hidden
                ImageViewer.Source = Nothing
                FontPreviewTextBlock.Visibility = Visibility.Hidden

                'Set source & play
                AssetPlayer.Source = New Uri(SelectedAsset.AssetFilePath, UriKind.RelativeOrAbsolute)
                AssetPlayer.Play()

                'Hide file browser and expand player
                AssetFilesListView.BeginAnimation(WidthProperty, HideAssetsAnimation)
                AssetPlayer.BeginAnimation(MarginProperty, ExpandPlayerAnimation)
            ElseIf SelectedAsset.Type = AssetType.Audio Then
                ImageViewer.Visibility = Visibility.Hidden
                ImageViewer.Source = Nothing
                FontPreviewTextBlock.Visibility = Visibility.Hidden

                AssetPlayer.Source = New Uri(SelectedAsset.AssetFilePath, UriKind.RelativeOrAbsolute)
                AssetPlayer.Play()
            ElseIf SelectedAsset.Type = AssetType.Image Then

                'Check if media is set and hide it to display the image
                If AssetPlayer.HasVideo Or AssetPlayer.HasAudio Then
                    AssetPlayer.Stop()
                    AssetPlayer.Source = Nothing
                End If
                FontPreviewTextBlock.Visibility = Visibility.Hidden

                If Path.GetExtension(SelectedAsset.AssetFilePath) = ".png" Then
                    ImageViewer.Visibility = Visibility.Visible
                    ImageViewer.Source = New BitmapImage(New Uri(SelectedAsset.AssetFilePath, UriKind.RelativeOrAbsolute))
                ElseIf Path.GetExtension(SelectedAsset.AssetFilePath) = ".jpg" Then
                    ImageViewer.Visibility = Visibility.Visible
                    ImageViewer.Source = New BitmapImage(New Uri(SelectedAsset.AssetFilePath, UriKind.RelativeOrAbsolute))
                ElseIf Path.GetExtension(SelectedAsset.AssetFilePath) = ".dds" Then
                    Dim img As New DDSImage(File.ReadAllBytes(SelectedAsset.AssetFilePath))
                    Dim bmp As System.Drawing.Bitmap = img.BitmapImage
                    Dim ms As New MemoryStream()
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png)
                    ms.Position = 0
                    Dim bi As New BitmapImage()
                    bi.BeginInit()
                    bi.StreamSource = ms
                    bi.EndInit()
                    ImageViewer.Source = bi
                End If

            ElseIf SelectedAsset.Type = AssetType.Font Then

                ImageViewer.Visibility = Visibility.Hidden
                ImageViewer.Source = Nothing

                If AssetPlayer.HasVideo Or AssetPlayer.HasAudio Then
                    AssetPlayer.Stop()
                    AssetPlayer.Source = Nothing
                End If

                'Load font and display
                Dim FontURI As New Uri(SelectedAsset.AssetFilePath)
                Dim NewPrivateFontCollection As New System.Drawing.Text.PrivateFontCollection()
                NewPrivateFontCollection.AddFontFile(FontURI.LocalPath)
                Dim NewFont As New System.Drawing.Font(NewPrivateFontCollection.Families(0), 24)
                FontPreviewTextBlock.FontFamily = New FontFamily(SelectedAsset.AssetFilePath + "#" + NewFont.Name)
                FontPreviewTextBlock.Visibility = Visibility.Visible
            End If

        End If
    End Sub

    Private Sub FileLoaderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles FileLoaderWorker.DoWork

        For Each MP4File In Directory.GetFiles(SelectedDirectory, "*.mp4", SearchOption.AllDirectories)
            Dim NewAssetFile As New AssetListViewItem() With {.AssetFilePath = MP4File, .AssetFileName = Path.GetFileName(MP4File), .Type = AssetType.Video}
            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           NewAssetFile.Icon = New BitmapImage(New Uri("/Images/Movie-File.png", UriKind.Relative))
                                           NewLoadingWindow.LoadProgressBar.Value += 1
                                           NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                                           AssetFilesListView.Items.Add(NewAssetFile)
                                       End Sub)
            Else
                NewAssetFile.Icon = New BitmapImage(New Uri("/Images/Movie-File.png", UriKind.Relative))
                NewLoadingWindow.LoadProgressBar.Value += 1
                NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                AssetFilesListView.Items.Add(NewAssetFile)
            End If
        Next

        For Each WEBMFile In Directory.GetFiles(SelectedDirectory, "*.webm", SearchOption.AllDirectories)
            Dim NewAssetFile As New AssetListViewItem() With {.AssetFilePath = WEBMFile, .AssetFileName = Path.GetFileName(WEBMFile), .Type = AssetType.Video}
            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           NewAssetFile.Icon = New BitmapImage(New Uri("/Images/Movie-File.png", UriKind.Relative))
                                           NewLoadingWindow.LoadProgressBar.Value += 1
                                           NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                                           AssetFilesListView.Items.Add(NewAssetFile)
                                       End Sub)
            Else
                NewAssetFile.Icon = New BitmapImage(New Uri("/Images/Movie-File.png", UriKind.Relative))
                NewLoadingWindow.LoadProgressBar.Value += 1
                NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                AssetFilesListView.Items.Add(NewAssetFile)
            End If
        Next

        For Each OGGFile In Directory.GetFiles(SelectedDirectory, "*.ogg", SearchOption.AllDirectories)
            Dim NewAssetFile As New AssetListViewItem() With {.AssetFilePath = OGGFile, .AssetFileName = Path.GetFileName(OGGFile), .Type = AssetType.Audio}

            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           NewAssetFile.Icon = New BitmapImage(New Uri("/Images/Music-File.png", UriKind.Relative))
                                           NewLoadingWindow.LoadProgressBar.Value += 1
                                           NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                                           AssetFilesListView.Items.Add(NewAssetFile)
                                       End Sub)
            Else
                NewAssetFile.Icon = New BitmapImage(New Uri("/Images/Music-File.png", UriKind.Relative))
                NewLoadingWindow.LoadProgressBar.Value += 1
                NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                AssetFilesListView.Items.Add(NewAssetFile)
            End If
        Next

        For Each PNGFile In Directory.GetFiles(SelectedDirectory, "*.png", SearchOption.AllDirectories)
            Dim NewAssetFile As New AssetListViewItem() With {.AssetFilePath = PNGFile, .AssetFileName = Path.GetFileName(PNGFile), .Type = AssetType.Image}
            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           NewAssetFile.Icon = New BitmapImage(New Uri("/Images/Image-File.png", UriKind.Relative))
                                           NewLoadingWindow.LoadProgressBar.Value += 1
                                           NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                                           AssetFilesListView.Items.Add(NewAssetFile)
                                       End Sub)
            Else
                NewAssetFile.Icon = New BitmapImage(New Uri("/Images/Image-File.png", UriKind.Relative))
                NewLoadingWindow.LoadProgressBar.Value += 1
                NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                AssetFilesListView.Items.Add(NewAssetFile)
            End If
        Next

        For Each JPGFile In Directory.GetFiles(SelectedDirectory, "*.jpg", SearchOption.AllDirectories)
            Dim NewAssetFile As New AssetListViewItem() With {.AssetFilePath = JPGFile, .AssetFileName = Path.GetFileName(JPGFile), .Type = AssetType.Image}
            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           NewAssetFile.Icon = New BitmapImage(New Uri("/Images/Image-File.png", UriKind.Relative))
                                           NewLoadingWindow.LoadProgressBar.Value += 1
                                           NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                                           AssetFilesListView.Items.Add(NewAssetFile)
                                       End Sub)
            Else
                NewAssetFile.Icon = New BitmapImage(New Uri("/Images/Image-File.png", UriKind.Relative))
                NewLoadingWindow.LoadProgressBar.Value += 1
                NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                AssetFilesListView.Items.Add(NewAssetFile)
            End If
        Next

        For Each DDSFile In Directory.GetFiles(SelectedDirectory, "*.dds", SearchOption.AllDirectories)
            Dim NewAssetFile As New AssetListViewItem() With {.AssetFilePath = DDSFile, .AssetFileName = Path.GetFileName(DDSFile), .Type = AssetType.Image}
            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           NewAssetFile.Icon = New BitmapImage(New Uri("/Images/Image-File.png", UriKind.Relative))
                                           NewLoadingWindow.LoadProgressBar.Value += 1
                                           NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                                           AssetFilesListView.Items.Add(NewAssetFile)
                                       End Sub)
            Else
                NewAssetFile.Icon = New BitmapImage(New Uri("/Images/Image-File.png", UriKind.Relative))
                NewLoadingWindow.LoadProgressBar.Value += 1
                NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                AssetFilesListView.Items.Add(NewAssetFile)
            End If
        Next

        For Each FontFile In Directory.GetFiles(SelectedDirectory, "*.ttf", SearchOption.AllDirectories)
            Dim NewAssetFile As New AssetListViewItem() With {.AssetFilePath = FontFile, .AssetFileName = Path.GetFileName(FontFile), .Type = AssetType.Font}
            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           NewAssetFile.Icon = New BitmapImage(New Uri("/Images/Font-File.png", UriKind.Relative))
                                           NewLoadingWindow.LoadProgressBar.Value += 1
                                           NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                                           AssetFilesListView.Items.Add(NewAssetFile)
                                       End Sub)
            Else
                NewAssetFile.Icon = New BitmapImage(New Uri("/Images/Image-File.png", UriKind.Relative))
                NewLoadingWindow.LoadProgressBar.Value += 1
                NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                AssetFilesListView.Items.Add(NewAssetFile)
            End If
        Next

    End Sub

    Private Sub StopMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles StopMenuItem.Click
        If AssetPlayer.Source IsNot Nothing Then
            AssetPlayer.Stop()
        End If
    End Sub

    Private Sub FileLoaderWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles FileLoaderWorker.RunWorkerCompleted
        NewLoadingWindow.Close()
        AssetFilesListView.Items.Refresh()
    End Sub

End Class
