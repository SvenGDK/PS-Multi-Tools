Imports System.IO
Imports System.Windows.Forms

Public Class MergeBinTool

    Dim WithEvents BinMerge As New Process()

    Private Structure CueListViewItem
        Private _FileName As String

        Public Property FileName As String
            Get
                Return _FileName
            End Get
            Set
                _FileName = Value
            End Set
        End Property
    End Structure

    Private Sub BrowseCUEFilesButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseCUEFilesButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Multiselect = True, .Filter = "cue files (*.cue)|*.cue"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then

            If OFD.FileNames.Count > 1 Then
                For Each SelectedCUE In OFD.FileNames
                    Dim NewCUELVItem As New CueListViewItem() With {.FileName = SelectedCUE}
                    CUEsListView.Items.Add(NewCUELVItem)
                Next
            Else
                Dim NewCUELVItem As New CueListViewItem() With {.FileName = OFD.FileName}
                CUEsListView.Items.Add(NewCUELVItem)
            End If

        End If
    End Sub

    Private Sub MergeSelectedButton_Click(sender As Object, e As RoutedEventArgs) Handles MergeSelectedButton.Click
        If CUEsListView.SelectedItem IsNot Nothing Then

            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           LogTextBox.Clear()
                                           Cursor = Input.Cursors.Wait
                                       End Sub)
            Else
                LogTextBox.Clear()
                Cursor = Input.Cursors.Wait
            End If

            Dim SelectedCUEFile As CueListViewItem = CType(CUEsListView.SelectedItem, CueListViewItem)
            Dim NewBaseNameTitle As String = Path.GetFileNameWithoutExtension(SelectedCUEFile.FileName) + "_merged"
            Dim OutputPath As String = Path.GetDirectoryName(SelectedCUEFile.FileName)

            'Set BinMerge process properties
            BinMerge = New Process()
            BinMerge.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\binmerge.exe"
            BinMerge.StartInfo.Arguments = """" + SelectedCUEFile.FileName + """ " + """" + NewBaseNameTitle + """"
            BinMerge.StartInfo.RedirectStandardOutput = True
            BinMerge.StartInfo.RedirectStandardError = True
            BinMerge.StartInfo.UseShellExecute = False
            BinMerge.StartInfo.CreateNoWindow = True
            BinMerge.EnableRaisingEvents = True

            AddHandler BinMerge.OutputDataReceived, Sub(SenderProcess As Object, DataArgs As DataReceivedEventArgs)
                                                        If Not String.IsNullOrEmpty(DataArgs.Data) Then
                                                            'Append output log from BinMerge
                                                            If Dispatcher.CheckAccess() = False Then
                                                                Dispatcher.BeginInvoke(Sub()
                                                                                           LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                                                           LogTextBox.ScrollToEnd()
                                                                                       End Sub)
                                                            Else
                                                                LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                                LogTextBox.ScrollToEnd()
                                                            End If
                                                        End If
                                                    End Sub

            AddHandler BinMerge.ErrorDataReceived, Sub(SenderProcess As Object, DataArgs As DataReceivedEventArgs)
                                                       If Not String.IsNullOrEmpty(DataArgs.Data) Then
                                                           'Append error log from BinMerge
                                                           If Dispatcher.CheckAccess() = False Then
                                                               Dispatcher.BeginInvoke(Sub()
                                                                                          LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                                                          LogTextBox.ScrollToEnd()
                                                                                      End Sub)
                                                           Else
                                                               LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                               LogTextBox.ScrollToEnd()
                                                           End If
                                                       End If
                                                   End Sub

            'Start BinMerge & read process output data
            BinMerge.Start()
            BinMerge.BeginOutputReadLine()
            BinMerge.BeginErrorReadLine()
        End If
    End Sub

    Private Sub MergeAllButton_Click(sender As Object, e As RoutedEventArgs) Handles MergeAllButton.Click
        Dim FailCount As Integer = 0

        If Not CUEsListView.Items.Count = 0 Then

            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           LogTextBox.Clear()
                                           Cursor = Input.Cursors.Wait
                                       End Sub)
            Else
                LogTextBox.Clear()
                Cursor = Input.Cursors.Wait
            End If

            For Each CUE As CueListViewItem In CUEsListView.Items
                Dim NewBaseNameTitle As String = Path.GetFileNameWithoutExtension(CUE.FileName) + "_merged"

                'Set BinMerge process properties
                BinMerge = New Process()
                BinMerge.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\binmerge.exe"
                BinMerge.StartInfo.Arguments = """" + CUE.FileName + """ " + """" + NewBaseNameTitle + """"
                BinMerge.StartInfo.RedirectStandardOutput = True
                BinMerge.StartInfo.RedirectStandardError = True
                BinMerge.StartInfo.UseShellExecute = False
                BinMerge.StartInfo.CreateNoWindow = True
                BinMerge.EnableRaisingEvents = True

                AddHandler BinMerge.OutputDataReceived, Sub(SenderProcess As Object, DataArgs As DataReceivedEventArgs)
                                                            If Not String.IsNullOrEmpty(DataArgs.Data) Then
                                                                'Append output log from BinMerge
                                                                If Dispatcher.CheckAccess() = False Then
                                                                    Dispatcher.BeginInvoke(Sub()
                                                                                               LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                                                               LogTextBox.ScrollToEnd()
                                                                                           End Sub)
                                                                Else
                                                                    LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                                    LogTextBox.ScrollToEnd()
                                                                End If
                                                            End If
                                                        End Sub

                AddHandler BinMerge.ErrorDataReceived, Sub(SenderProcess As Object, DataArgs As DataReceivedEventArgs)
                                                           If Not String.IsNullOrEmpty(DataArgs.Data) Then
                                                               'Append error log from BinMerge
                                                               If Dispatcher.CheckAccess() = False Then
                                                                   Dispatcher.BeginInvoke(Sub()
                                                                                              LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                                                              LogTextBox.ScrollToEnd()
                                                                                          End Sub)
                                                               Else
                                                                   LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                                   LogTextBox.ScrollToEnd()
                                                               End If
                                                           End If
                                                       End Sub

                'Start BinMerge & read process output data
                BinMerge.Start()
                BinMerge.BeginOutputReadLine()
                BinMerge.BeginErrorReadLine()
                BinMerge.WaitForExit()
            Next

            Cursor = Input.Cursors.Arrow
        End If
    End Sub

    Private Sub BinMerge_Exited(sender As Object, e As EventArgs) Handles BinMerge.Exited
        BinMerge.Dispose()
        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub() Cursor = Input.Cursors.Arrow)
        Else
            Cursor = Input.Cursors.Arrow
        End If
    End Sub

End Class
