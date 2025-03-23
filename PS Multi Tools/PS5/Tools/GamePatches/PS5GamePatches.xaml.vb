Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.IO

Public Class PS5GamePatches

    Dim TotalPatches As Integer = 0
    Public SearchForGamePatchWithID As String = String.Empty

    Public Property DownloadQueueItemCollection As New ObservableCollection(Of DownloadQueueItem)

    Dim GameIDHeaderSorting As ListSortDirection = ListSortDirection.Ascending
    Dim FileNameHeaderSorting As ListSortDirection = ListSortDirection.Ascending
    Dim PKGSizeHeaderSorting As ListSortDirection = ListSortDirection.Ascending
    Dim DownloadStateHeaderSorting As ListSortDirection = ListSortDirection.Ascending
    Dim MergeStateHeaderSorting As ListSortDirection = ListSortDirection.Ascending

    Dim DownloadsContextMenu As New Controls.ContextMenu()
    Dim WithEvents MergeNowMenuItem As New Controls.MenuItem() With {.Header = "Merge selected patch from this source pkg"}
    Dim WithEvents DeleteMenuItem As New Controls.MenuItem() With {.Header = "Delete selected pkg"}

    Private Sub PS5GamePatches_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        DownloadsContextMenu.Items.Add(MergeNowMenuItem)
        DownloadsContextMenu.Items.Add(DeleteMenuItem)
        DownloadQueueListView.ContextMenu = DownloadsContextMenu

        'Add already downloaded patch pkgs
        If Directory.Exists(Environment.CurrentDirectory + "\Downloads") Then
            For Each PKG In Directory.GetFiles(Environment.CurrentDirectory + "\Downloads", "*.pkg", SearchOption.AllDirectories)
                Dim PKGFileInfo As New FileInfo(PKG)
                Dim PKGFileName As String = PKGFileInfo.Name

                If PKGFileName.Split("-"c).Length > 1 Then
                    If PKGFileName.Split("-"c)(1).Split("_"c).Length > 1 Then
                        Dim PKGID As String = PKGFileName.Split("-"c)(1).Split("_"c)(0)
                        Dim PKGFileSize As String = FormatNumber(PKGFileInfo.Length / 1073741824, 2) + " GB"
                        Dim NewQueueItem As New DownloadQueueItem() With {.FileName = PKGFileName, .GameID = PKGID, .DownloadURL = PKG, .DownloadState = "Downloaded", .PKGSize = PKGFileSize}

                        'Get the basename of this pkg
                        Dim BaseName As String = PKGFileName.Split(New String() {".pkg"}, StringSplitOptions.None)(0)
                        'Check if has been already merged
                        If BaseName.EndsWith("-merged") Then
                            NewQueueItem.MergeState = "Merged"
                        Else
                            NewQueueItem.MergeState = "Not merged"
                        End If

                        DownloadQueueItemCollection.Add(NewQueueItem)
                    End If
                End If
            Next
        End If
    End Sub

    Private Async Sub SearchButton_Click(sender As Object, e As RoutedEventArgs) Handles SearchButton.Click
        If Not String.IsNullOrEmpty(SearchGameIDTextBox.Text) Then
            If Await Utils.IsURLValid("https://prosperopatches.com/" + SearchGameIDTextBox.Text) Then
                Dim NewWin As New PS5GamePatchSelector() With {.CurrentGameID = SearchGameIDTextBox.Text}
                NewWin.GamePatchesWebView.Source = New Uri("https://prosperopatches.com/" + SearchGameIDTextBox.Text)
                NewWin.Show()
            End If
        End If
    End Sub

    Private Sub VisitButton_Click(sender As Object, e As RoutedEventArgs) Handles VisitButton.Click
        Process.Start("https://prosperopatches.com/" + SearchGameIDTextBox.Text)
    End Sub

    Private Async Sub PS5GamePatches_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        If Not String.IsNullOrEmpty(SearchForGamePatchWithID) Then
            SearchGameIDTextBox.Text = SearchForGamePatchWithID

            If Await Utils.IsURLValid("https://prosperopatches.com/" + SearchForGamePatchWithID) Then
                Dim NewWin As New PS5GamePatchSelector() With {.CurrentGameID = SearchForGamePatchWithID}
                NewWin.GamePatchesWebView.Source = New Uri("https://prosperopatches.com/" + SearchForGamePatchWithID)
                NewWin.Show()
            End If
        End If
    End Sub

    Private Sub DownloadQueueListView_SelectionChanged(sender As Object, e As Controls.SelectionChangedEventArgs) Handles DownloadQueueListView.SelectionChanged
        If DownloadQueueListView.SelectedItem IsNot Nothing Then
            Dim SelectedItemAsQueueItem As DownloadQueueItem = CType(DownloadQueueListView.SelectedItem, DownloadQueueItem)
            If SelectedItemAsQueueItem.DownloadState = "Downloaded" Then
                DownloadButton.IsEnabled = False
            Else
                DownloadButton.IsEnabled = True
            End If
        End If
    End Sub

    Private Async Sub DownloadButton_Click(sender As Object, e As RoutedEventArgs) Handles DownloadButton.Click
        If DownloadQueueListView.SelectedItem IsNot Nothing Then
            If DownloadQueueListView.SelectedItems.Count > 1 Then
                'Create a new download window for each selected item
                For Each SelectedItem In DownloadQueueListView.SelectedItems
                    Dim SelectedItemAsQueueItem As DownloadQueueItem = CType(SelectedItem, DownloadQueueItem)
                    SelectedItemAsQueueItem.DownloadState = "Download started"

                    Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5", .DownloadQueueItem = SelectedItemAsQueueItem}
                    NewDownloader.Show()
                    If Await NewDownloader.CreateNewDownload(SelectedItemAsQueueItem.DownloadURL, FileSize:=SelectedItemAsQueueItem.PKGSize) = False Then
                        MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
                        NewDownloader.Close()
                        SelectedItemAsQueueItem.DownloadState = "Download failed"
                    Else
                    End If
                Next
            Else
                'Download only selected item
                Dim SelectedItemAsQueueItem As DownloadQueueItem = CType(DownloadQueueListView.SelectedItem, DownloadQueueItem)
                SelectedItemAsQueueItem.DownloadState = "Download started"

                Dim NewDownloader As New Downloader() With {.ShowActivated = True, .PackageConsole = "PS5", .DownloadQueueItem = SelectedItemAsQueueItem}
                NewDownloader.Show()
                If Await NewDownloader.CreateNewDownload(SelectedItemAsQueueItem.DownloadURL, FileSize:=SelectedItemAsQueueItem.PKGSize) = False Then
                    MsgBox("Could not download the selected file.", MsgBoxStyle.Critical)
                    NewDownloader.Close()
                    SelectedItemAsQueueItem.DownloadState = "Download failed"
                Else
                End If
            End If
        End If
    End Sub

    Private Sub MergeNowMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles MergeNowMenuItem.Click
        If DownloadQueueListView.SelectedItem IsNot Nothing Then
            Dim SelectedItemAsQueueItem As DownloadQueueItem = CType(DownloadQueueListView.SelectedItem, DownloadQueueItem)

            If SelectedItemAsQueueItem.FileName.EndsWith("_sc.pkg") Then
                If Not SelectedItemAsQueueItem.MergeState = "Merged" Then
                    SelectedItemAsQueueItem.MergeState = "Merge started"

                    'Create a folder to move the downloaded packages into it for merging
                    Dim BaseName As String = SelectedItemAsQueueItem.FileName.Split(New String() {"_sc.pkg"}, StringSplitOptions.None)(0)
                    Dim NewMergeFolder As String = Environment.CurrentDirectory + "\Downloads\" + BaseName
                    If Not Directory.Exists(NewMergeFolder) Then
                        Directory.CreateDirectory(NewMergeFolder)
                    End If

                    'Get all downloaded packages of this patch and move to the new folder
                    For Each DownloadedPatchPKG In Directory.GetFiles(Environment.CurrentDirectory + "\Downloads", BaseName + "_*.pkg", SearchOption.TopDirectoryOnly)
                        Dim PackageFileName As String = Path.GetFileName(DownloadedPatchPKG)
                        File.Move(DownloadedPatchPKG, NewMergeFolder + "\" + PackageFileName)
                    Next

                    'Rename _sc.pkg
                    Dim PKGCount As Integer = Directory.GetFiles(NewMergeFolder).Length - 1
                    Dim SCPKGName As String = BaseName + "_sc.pkg"
                    Dim NewPKGName As String = BaseName + "_" + PKGCount.ToString + ".pkg"
                    If File.Exists(NewMergeFolder + "\" + SCPKGName) Then
                        File.Move(NewMergeFolder + "\" + SCPKGName, NewMergeFolder + "\" + NewPKGName)
                    End If

                    'Open a new PKGMerger window
                    Dim NewPKGMerger As New PS5PKGMerger() With {.MergeDownloadSourceFolder = NewMergeFolder, .MergeBaseName = BaseName}
                    NewPKGMerger.Show()
                Else
                    MsgBox("Selected patch has already been merged.", MsgBoxStyle.Information)
                End If
            Else
                MsgBox("Selected package is not a source package." + vbCrLf + "Select the _sc.pkg of the downloaded patch.", MsgBoxStyle.Information)
            End If

        End If
    End Sub

    Private Sub ColumnHeader_Click(sender As Object, e As RoutedEventArgs)
        Dim ClickedColumn As GridViewColumnHeader = TryCast(sender, GridViewColumnHeader)
        Dim ClickedHeaderTagName As String = ClickedColumn.Tag.ToString()
        Dim DownloadQueueCollectionView As CollectionView = CType(CollectionViewSource.GetDefaultView(DownloadQueueListView.ItemsSource), CollectionView)

        'Clear previous sorting
        DownloadQueueCollectionView.SortDescriptions.Clear()

        If ClickedHeaderTagName = "GameIDHeader" Then
            If GameIDHeaderSorting = ListSortDirection.Descending Then
                DownloadQueueCollectionView.SortDescriptions.Add(New SortDescription("GameID", ListSortDirection.Ascending))
                GameIDHeaderSorting = ListSortDirection.Ascending
            Else
                DownloadQueueCollectionView.SortDescriptions.Add(New SortDescription("GameID", ListSortDirection.Descending))
                GameIDHeaderSorting = ListSortDirection.Descending
            End If
        ElseIf ClickedHeaderTagName = "FileNameHeader" Then
            If FileNameHeaderSorting = ListSortDirection.Descending Then
                DownloadQueueCollectionView.SortDescriptions.Add(New SortDescription("FileName", ListSortDirection.Ascending))
                FileNameHeaderSorting = ListSortDirection.Ascending
            Else
                DownloadQueueCollectionView.SortDescriptions.Add(New SortDescription("FileName", ListSortDirection.Descending))
                FileNameHeaderSorting = ListSortDirection.Descending
            End If
        ElseIf ClickedHeaderTagName = "PKGSizeHeader" Then
            If PKGSizeHeaderSorting = ListSortDirection.Descending Then
                DownloadQueueCollectionView.SortDescriptions.Add(New SortDescription("PKGSize", ListSortDirection.Ascending))
                PKGSizeHeaderSorting = ListSortDirection.Ascending
            Else
                DownloadQueueCollectionView.SortDescriptions.Add(New SortDescription("PKGSize", ListSortDirection.Descending))
                PKGSizeHeaderSorting = ListSortDirection.Descending
            End If
        ElseIf ClickedHeaderTagName = "DownloadStateHeader" Then
            If DownloadStateHeaderSorting = ListSortDirection.Descending Then
                DownloadQueueCollectionView.SortDescriptions.Add(New SortDescription("DownloadState", ListSortDirection.Ascending))
                DownloadStateHeaderSorting = ListSortDirection.Ascending
            Else
                DownloadQueueCollectionView.SortDescriptions.Add(New SortDescription("DownloadState", ListSortDirection.Descending))
                DownloadStateHeaderSorting = ListSortDirection.Descending
            End If
        ElseIf ClickedHeaderTagName = "MergeStateHeader" Then
            If MergeStateHeaderSorting = ListSortDirection.Descending Then
                DownloadQueueCollectionView.SortDescriptions.Add(New SortDescription("MergeState", ListSortDirection.Ascending))
                MergeStateHeaderSorting = ListSortDirection.Ascending
            Else
                DownloadQueueCollectionView.SortDescriptions.Add(New SortDescription("MergeState", ListSortDirection.Descending))
                MergeStateHeaderSorting = ListSortDirection.Descending
            End If
        End If
    End Sub

    Private Sub DeleteMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles DeleteMenuItem.Click
        If DownloadQueueListView.SelectedItem IsNot Nothing Then
            If MsgBox("Do you really want to delete this package ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                Dim SelectedItemAsQueueItem As DownloadQueueItem = CType(DownloadQueueListView.SelectedItem, DownloadQueueItem)
                File.Delete(SelectedItemAsQueueItem.DownloadURL)
                DownloadQueueItemCollection.Remove(SelectedItemAsQueueItem)
            End If
        End If
    End Sub

End Class
