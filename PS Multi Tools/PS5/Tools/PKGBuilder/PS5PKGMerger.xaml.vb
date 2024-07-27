Public Class PS5PKGMerger

    Dim WithEvents PKGMerge As New Process()
    Dim SelectedPath As String = ""

    Public MergeBaseName As String = String.Empty
    Public MergeDownloadSourceFolder As String = String.Empty

    Private Sub BrowseFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseFolderButton.Click
        Dim FBD As New Forms.FolderBrowserDialog()

        If FBD.ShowDialog() = Windows.Forms.DialogResult.OK Then
            SelectedDirectoryTextBox.Text = FBD.SelectedPath
            SelectedPath = FBD.SelectedPath
            MergeButton.IsEnabled = True

            If Not String.IsNullOrEmpty(MergeLogTextBox.Text) Then
                MergeLogTextBox.Clear()
            End If
        End If
    End Sub

    Private Sub MergeButton_Click(sender As Object, e As RoutedEventArgs) Handles MergeButton.Click
        If Not String.IsNullOrEmpty(SelectedDirectoryTextBox.Text) Then

            Dispatcher.BeginInvoke(Sub()
                                       MergeButton.IsEnabled = False
                                       SelectedDirectoryTextBox.IsEnabled = False
                                       BrowseFolderButton.IsEnabled = False
                                   End Sub)

            PKGMerge = New Process()
            PKGMerge.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\pkg_merge.exe"
            PKGMerge.StartInfo.Arguments = """" + SelectedPath + """"
            PKGMerge.StartInfo.RedirectStandardOutput = True
            PKGMerge.StartInfo.RedirectStandardError = False
            PKGMerge.StartInfo.RedirectStandardInput = False
            PKGMerge.StartInfo.UseShellExecute = False
            PKGMerge.StartInfo.CreateNoWindow = True
            PKGMerge.EnableRaisingEvents = True

            AddHandler PKGMerge.OutputDataReceived, AddressOf PKGMergeDataRecieved

            PKGMerge.Start()
            PKGMerge.BeginOutputReadLine()
        Else
            MsgBox("No folder selected!", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub PKGMergeDataRecieved(sender As Object, e As DataReceivedEventArgs)
        If Not String.IsNullOrEmpty(e.Data) Then

            'Do not write every line or it will freeze
            If e.Data.Contains("beginning") Then
                Dispatcher.BeginInvoke(Sub()
                                           MergeLogTextBox.AppendText(e.Data & vbCrLf)
                                           MergeLogTextBox.ScrollToEnd()
                                       End Sub)
            ElseIf e.Data.Contains("25%") Then
                Dispatcher.BeginInvoke(Sub()
                                           MergeLogTextBox.AppendText(e.Data & vbCrLf)
                                           MergeLogTextBox.ScrollToEnd()
                                       End Sub)
            ElseIf e.Data.Contains("50%") Then
                Dispatcher.BeginInvoke(Sub()
                                           MergeLogTextBox.AppendText(e.Data & vbCrLf)
                                           MergeLogTextBox.ScrollToEnd()
                                       End Sub)
            ElseIf e.Data.Contains("75%") Then
                Dispatcher.BeginInvoke(Sub()
                                           MergeLogTextBox.AppendText(e.Data & vbCrLf)
                                           MergeLogTextBox.ScrollToEnd()
                                       End Sub)
            ElseIf e.Data.Contains("100%") Then
                Dispatcher.BeginInvoke(Sub()
                                           MergeLogTextBox.AppendText(e.Data & vbCrLf)
                                           MergeLogTextBox.ScrollToEnd()
                                       End Sub)
            ElseIf e.Data.Contains("[success]") Then
                Dispatcher.BeginInvoke(Sub()
                                           MergeLogTextBox.AppendText(e.Data & vbCrLf)
                                           MergeLogTextBox.ScrollToEnd()
                                       End Sub)
            End If

        End If
    End Sub

    Private Sub PKGMerge_Exited(sender As Object, e As EventArgs) Handles PKGMerge.Exited
        PKGMerge.CancelOutputRead()
        PKGMerge.Dispose()
        PKGMerge = Nothing

        Dispatcher.BeginInvoke(Sub()
                                   MergeButton.IsEnabled = True
                                   SelectedDirectoryTextBox.IsEnabled = True
                                   BrowseFolderButton.IsEnabled = True

                                   'For PS5 game patches
                                   If Not String.IsNullOrEmpty(MergeBaseName) Then
                                       'Update progress in PS5GamePatches (if open)
                                       Dim OpenGamePatchesWindow As PS5GamePatches
                                       For Each OpenWin In Windows.Application.Current.Windows()
                                           If OpenWin.ToString = "psmt_lib.PS5GamePatches" Then
                                               OpenGamePatchesWindow = CType(OpenWin, PS5GamePatches)

                                               For Each DownloadItem In OpenGamePatchesWindow.DownloadQueueListView.Items
                                                   Dim DownloadItemAsDownloadQueueItem As DownloadQueueItem = CType(DownloadItem, DownloadQueueItem)
                                                   If DownloadItemAsDownloadQueueItem.FileName.StartsWith(MergeBaseName) Then 'Mark every associated pkg of this patch as 'Merged'
                                                       DownloadItemAsDownloadQueueItem.MergeState = "Merged"
                                                   End If
                                               Next

                                               Exit For
                                           End If
                                       Next
                                   End If
                               End Sub)

        If MsgBox("Packages have been merged!" + vbCrLf + "Do you want to open the folder containing the merged PKG?", MsgBoxStyle.YesNo, "Done merging") = MsgBoxResult.Yes Then
            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           If Not String.IsNullOrEmpty(MergeDownloadSourceFolder) Then
                                               Process.Start("explorer", MergeDownloadSourceFolder)
                                           Else
                                               Process.Start("explorer", SelectedPath)
                                           End If
                                       End Sub)
            Else
                If Not String.IsNullOrEmpty(MergeDownloadSourceFolder) Then
                    Process.Start("explorer", MergeDownloadSourceFolder)
                Else
                    Process.Start("explorer", SelectedPath)
                End If
            End If
        End If
    End Sub

    Private Sub PS5PKGMerger_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        'Merge automatically if selected at game patches downloader
        If Not String.IsNullOrEmpty(MergeDownloadSourceFolder) Then
            Dispatcher.BeginInvoke(Sub()
                                       MergeButton.IsEnabled = False
                                       SelectedDirectoryTextBox.IsEnabled = False
                                       BrowseFolderButton.IsEnabled = False
                                   End Sub)

            PKGMerge = New Process()
            PKGMerge.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\pkg_merge.exe"
            PKGMerge.StartInfo.Arguments = """" + MergeDownloadSourceFolder + """"
            PKGMerge.StartInfo.RedirectStandardOutput = True
            PKGMerge.StartInfo.UseShellExecute = False
            PKGMerge.StartInfo.CreateNoWindow = True
            PKGMerge.EnableRaisingEvents = True

            AddHandler PKGMerge.OutputDataReceived, AddressOf PKGMergeDataRecieved

            PKGMerge.Start()
            PKGMerge.BeginOutputReadLine()
        End If
    End Sub

End Class
