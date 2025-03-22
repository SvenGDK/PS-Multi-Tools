Imports System.ComponentModel
Imports System.IO
Imports System.Windows.Forms
Imports System.Windows.Media.Animation
Imports DDSReader
Imports MS.Internal
Imports PS_Multi_Tools.Structures

Public Class PSXAssetsBrowser

    Dim WithEvents FileLoaderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}
    Dim WithEvents NewLoadingWindow As New PSXSyncWindow() With {.Title = "Loading XMB files", .ShowActivated = True}

    Dim ShowAssetsAnimation As New DoubleAnimation With {.From = 0, .To = 350, .Duration = New Duration(TimeSpan.FromMilliseconds(400))}
    Dim HideAssetsAnimation As New DoubleAnimation With {.From = 350, .To = 0, .Duration = New Duration(TimeSpan.FromMilliseconds(400))}

    Dim ExpandPlayerAnimation As New ThicknessAnimation With {.From = New Thickness(350, 0, 0, 0), .To = New Thickness(0, 0, 0, 0), .Duration = New Duration(TimeSpan.FromMilliseconds(400))}
    Dim ResizePlayerAnimation As New ThicknessAnimation With {.From = New Thickness(0, 0, 0, 0), .To = New Thickness(350, 0, 0, 0), .Duration = New Duration(TimeSpan.FromMilliseconds(400))}

    Dim PlayerContextMenu As New Controls.ContextMenu()
    Dim WithEvents ShowHideAssetsMenuItem As New Controls.MenuItem() With {.Header = "Show assets", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Assets-icon.png", UriKind.Relative))}}
    Dim WithEvents PlayPauseMenuItem As New Controls.MenuItem() With {.Header = "Pause", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Pause-icon.png", UriKind.Relative))}}
    Dim WithEvents StopMenuItem As New Controls.MenuItem() With {.Header = "Stop playblack", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Stop-icon.png", UriKind.Relative))}}

    Dim FileBrowserContextMenu As New Controls.ContextMenu()
    Dim WithEvents OpenFileLocationMenuItem As New Controls.MenuItem() With {.Header = "Open file location", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Folder-Open.png", UriKind.Relative))}}
    Dim WithEvents ReplaceFileMenuItem As New Controls.MenuItem() With {.Header = "Replace file", .Icon = New Controls.Image() With {.Source = New BitmapImage(New Uri("/Images/Replace-File.png", UriKind.Relative))}}

    Public SelectedDirectory As String = ""
    Dim FilesCount As Integer = 0

    Private Sub AssetsBrowser_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        PlayerContextMenu.Items.Add(ShowHideAssetsMenuItem)
        PlayerContextMenu.Items.Add(PlayPauseMenuItem)
        PlayerContextMenu.Items.Add(StopMenuItem)

        AssetPlayer.ContextMenu = PlayerContextMenu

        FileBrowserContextMenu.Items.Add(OpenFileLocationMenuItem)
        FileBrowserContextMenu.Items.Add(ReplaceFileMenuItem)
        AssetFilesListView.ContextMenu = FileBrowserContextMenu
    End Sub

    Private Sub AssetsBrowser_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        If Not String.IsNullOrEmpty(SelectedDirectory) Then
            'Load playable assets
            FilesCount = 0
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.mp4", SearchOption.AllDirectories).Length
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.webm", SearchOption.AllDirectories).Length
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.ogg", SearchOption.AllDirectories).Length
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.png", SearchOption.AllDirectories).Length
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.jpg", SearchOption.AllDirectories).Length
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.ttf", SearchOption.AllDirectories).Length
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.xml", SearchOption.AllDirectories).Length
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.dic", SearchOption.AllDirectories).Length

            NewLoadingWindow = New PSXSyncWindow() With {.Title = "Loading XMB files", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.Maximum = FilesCount
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + FilesCount.ToString()
            NewLoadingWindow.Show()

            FileLoaderWorker.RunWorkerAsync()
        End If
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

            Try
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
                        Try
                            Dim img As New DDSImage(File.ReadAllBytes(SelectedAsset.AssetFilePath))
                            If img IsNot Nothing Then
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
                        Catch ex As InvalidFileHeaderException
                            MsgBox("DDS file seems to be invalid.", MsgBoxStyle.Information)
                        End Try
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

                ElseIf SelectedAsset.Type = AssetType.XML Then
                    Dim NewAdvancedEditor As New PSXAdvancedEditor() With {.SetSyntax = "XML", .FileContent = File.ReadAllText(SelectedAsset.AssetFilePath), .FilePath = SelectedAsset.AssetFilePath, .Title = "Advanced Editor - " + SelectedAsset.AssetFilePath, .ShowActivated = True}
                    NewAdvancedEditor.Show()
                ElseIf SelectedAsset.Type = AssetType.DIC Then
                    Dim NewAdvancedEditor As New PSXAdvancedEditor() With {.SetSyntax = "DIC", .FileContent = File.ReadAllText(SelectedAsset.AssetFilePath), .FilePath = SelectedAsset.AssetFilePath, .Title = "Advanced Editor - " + SelectedAsset.AssetFilePath, .ShowActivated = True}
                    NewAdvancedEditor.Show()
                End If
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try

        End If
    End Sub

    Private Sub FileLoaderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles FileLoaderWorker.DoWork

        For Each DetectedFile In Directory.GetFiles(SelectedDirectory, "*.*", SearchOption.AllDirectories)
            Dim NewFileInfo As New FileInfo(DetectedFile)
            Select Case NewFileInfo.Extension
                Case ".mp4"
                    Dim NewAssetFile As New AssetListViewItem() With {.AssetFilePath = DetectedFile, .AssetFileName = Path.GetFileName(DetectedFile), .Type = AssetType.Video}
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
                Case ".ogg"
                    Dim NewAssetFile As New AssetListViewItem() With {.AssetFilePath = DetectedFile, .AssetFileName = Path.GetFileName(DetectedFile), .Type = AssetType.Audio}

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
                Case ".png"
                    Dim NewAssetFile As New AssetListViewItem() With {.AssetFilePath = DetectedFile, .AssetFileName = Path.GetFileName(DetectedFile), .Type = AssetType.Image}
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
                Case ".jpg"
                    Dim NewAssetFile As New AssetListViewItem() With {.AssetFilePath = DetectedFile, .AssetFileName = Path.GetFileName(DetectedFile), .Type = AssetType.Image}
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
                Case ".jpeg"
                    Dim NewAssetFile As New AssetListViewItem() With {.AssetFilePath = DetectedFile, .AssetFileName = Path.GetFileName(DetectedFile), .Type = AssetType.Image}
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
                Case ".dds"
                    Dim NewAssetFile As New AssetListViewItem() With {.AssetFilePath = DetectedFile, .AssetFileName = Path.GetFileName(DetectedFile), .Type = AssetType.Image}
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
                Case ".ttf"
                    Dim NewAssetFile As New AssetListViewItem() With {.AssetFilePath = DetectedFile, .AssetFileName = Path.GetFileName(DetectedFile), .Type = AssetType.Font}
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
                Case ".xml"
                    Dim NewAssetFile As New AssetListViewItem() With {.AssetFilePath = DetectedFile, .AssetFileName = Path.GetFileName(DetectedFile), .Type = AssetType.XML}
                    If Dispatcher.CheckAccess() = False Then
                        Dispatcher.BeginInvoke(Sub()
                                                   NewAssetFile.Icon = New BitmapImage(New Uri("/Images/XML-File.png", UriKind.Relative))
                                                   NewLoadingWindow.LoadProgressBar.Value += 1
                                                   NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                                                   AssetFilesListView.Items.Add(NewAssetFile)
                                               End Sub)
                    Else
                        NewAssetFile.Icon = New BitmapImage(New Uri("/Images/XML-File.png", UriKind.Relative))
                        NewLoadingWindow.LoadProgressBar.Value += 1
                        NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                        AssetFilesListView.Items.Add(NewAssetFile)
                    End If
                Case ".dic"
                    Dim NewAssetFile As New AssetListViewItem() With {.AssetFilePath = DetectedFile, .AssetFileName = Path.GetFileName(DetectedFile), .Type = AssetType.DIC}
                    If Dispatcher.CheckAccess() = False Then
                        Dispatcher.BeginInvoke(Sub()
                                                   NewAssetFile.Icon = New BitmapImage(New Uri("/Images/DIC-File.png", UriKind.Relative))
                                                   NewLoadingWindow.LoadProgressBar.Value += 1
                                                   NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                                                   AssetFilesListView.Items.Add(NewAssetFile)
                                               End Sub)
                    Else
                        NewAssetFile.Icon = New BitmapImage(New Uri("/Images/DIC-File.png", UriKind.Relative))
                        NewLoadingWindow.LoadProgressBar.Value += 1
                        NewLoadingWindow.LoadStatusTextBlock.Text = "Loading " + NewLoadingWindow.LoadProgressBar.Value.ToString + " of " + FilesCount.ToString()
                        AssetFilesListView.Items.Add(NewAssetFile)
                    End If
            End Select
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

    Private Sub OpenFileLocationMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenFileLocationMenuItem.Click
        If AssetFilesListView.SelectedItem IsNot Nothing Then
            Dim SelectedAsset As AssetListViewItem = CType(AssetFilesListView.SelectedItem, AssetListViewItem)
            Dim SelectedAssetDirectory As String = Path.GetDirectoryName(SelectedAsset.AssetFilePath)
            Process.Start("explorer", SelectedAssetDirectory)
        End If
    End Sub

    Private Sub OpenFolderImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles OpenFolderImage.MouseLeftButtonDown
        Dim FBD As New FolderBrowserDialog() With {.Description = "Select your .../xosd/packages folder"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedDirectory = FBD.SelectedPath

            'Load playable assets
            FilesCount = 0
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.mp4", SearchOption.AllDirectories).Length
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.webm", SearchOption.AllDirectories).Length
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.ogg", SearchOption.AllDirectories).Length
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.png", SearchOption.AllDirectories).Length
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.jpg", SearchOption.AllDirectories).Length
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.ttf", SearchOption.AllDirectories).Length
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.xml", SearchOption.AllDirectories).Length
            FilesCount += Directory.GetFiles(SelectedDirectory, "*.dic", SearchOption.AllDirectories).Length

            NewLoadingWindow = New PSXSyncWindow() With {.Title = "Loading XMB files", .ShowActivated = True}
            NewLoadingWindow.LoadProgressBar.Maximum = FilesCount
            NewLoadingWindow.LoadStatusTextBlock.Text = "Loading file 1 of " + FilesCount.ToString()
            NewLoadingWindow.Show()

            FileLoaderWorker.RunWorkerAsync()
        End If
    End Sub

    Private Sub OpenImageEditorImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles OpenImageEditorImage.MouseLeftButtonDown
        If AssetFilesListView.SelectedItem IsNot Nothing Then
            Dim SelectedAsset As AssetListViewItem = CType(AssetFilesListView.SelectedItem, AssetListViewItem)
            Dim NewSimpleGraphicsEditor As New PSXSimpleGraphicsEditor() With {.Title = "Editing " + SelectedAsset.AssetFilePath, .LoadedImageFilePath = SelectedAsset.AssetFilePath}
            NewSimpleGraphicsEditor.Show()
        End If
    End Sub

    Private Sub AssetFilesListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles AssetFilesListView.SelectionChanged
        If AssetFilesListView.SelectedItem IsNot Nothing Then
            Dim SelectedAsset As AssetListViewItem = CType(AssetFilesListView.SelectedItem, AssetListViewItem)
            Select Case SelectedAsset.Type
                Case AssetType.Image
                    'Show option to open the built-in Graphics Editor
                    OpenImageEditorImage.Visibility = Visibility.Visible
                    OpenImageEditorTextBlock.Visibility = Visibility.Visible
                Case Else
                    OpenImageEditorImage.Visibility = Visibility.Hidden
                    OpenImageEditorTextBlock.Visibility = Visibility.Hidden
            End Select
        End If
    End Sub

End Class
