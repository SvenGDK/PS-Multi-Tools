Imports System.Text

Public Class CUE2POPSConverter

    Dim WithEvents CUE2POPS As New Process()

    Private Sub BrowseCueButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseCueButton.Click
        Dim OFD As New Forms.OpenFileDialog() With {.CheckFileExists = True, .Filter = "CUE files (*.cue)|*.cue", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedCueTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseOutputFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseOutputFolderButton.Click
        Dim FBD As New Forms.FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select a folder where you want to save the VCD file"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            POPSOutputFolderTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub ConvertButton_Click(sender As Object, e As RoutedEventArgs) Handles ConvertButton.Click
        If Not String.IsNullOrEmpty(SelectedCueTextBox.Text) Then
            If Not String.IsNullOrEmpty(POPSOutputFolderTextBox.Text) Then
                Cursor = Cursors.Wait

                If LogTextBox.Dispatcher.CheckAccess() = False Then
                    LogTextBox.Dispatcher.BeginInvoke(Sub() LogTextBox.Clear())
                Else
                    LogTextBox.Clear()
                End If

                'Set CUE2POPS process properties
                CUE2POPS = New Process()
                CUE2POPS.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\cue2pops.exe"

                'Build the arguments string
                Dim NewStringBuilder As New StringBuilder()
                NewStringBuilder.Append("""" + SelectedCueTextBox.Text + """")

                If Add2SecToAllTrackIndexesCheckBox.IsChecked Then
                    NewStringBuilder.Append(" gap++")
                ElseIf Sub2SecToAllTrackIndexesCheckBox.IsChecked Then
                    NewStringBuilder.Append(" gap--")
                End If
                If PatchVideoModeCheckBox.IsChecked Then
                    NewStringBuilder.Append(" vmode")
                End If
                If EnableCheatsCheckBox.IsChecked Then
                    NewStringBuilder.Append(" trainer")
                End If

                NewStringBuilder.Append(" """ + POPSOutputFolderTextBox.Text + "\IMAGE0.VCD""")

                CUE2POPS.StartInfo.Arguments = NewStringBuilder.ToString()
                CUE2POPS.StartInfo.RedirectStandardOutput = True
                CUE2POPS.StartInfo.RedirectStandardError = True
                CUE2POPS.StartInfo.UseShellExecute = False
                CUE2POPS.StartInfo.CreateNoWindow = True
                CUE2POPS.EnableRaisingEvents = True

                AddHandler CUE2POPS.OutputDataReceived, Sub(SenderProcess As Object, DataArgs As DataReceivedEventArgs)
                                                            If Not String.IsNullOrEmpty(DataArgs.Data) Then
                                                                'Append output log from CUE2POPS
                                                                If LogTextBox.Dispatcher.CheckAccess() = False Then
                                                                    LogTextBox.Dispatcher.BeginInvoke(Sub()
                                                                                                          LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                                                                          LogTextBox.ScrollToEnd()
                                                                                                      End Sub)
                                                                Else
                                                                    LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                                    LogTextBox.ScrollToEnd()
                                                                End If
                                                            End If
                                                        End Sub

                AddHandler CUE2POPS.ErrorDataReceived, Sub(SenderProcess As Object, DataArgs As DataReceivedEventArgs)
                                                           If Not String.IsNullOrEmpty(DataArgs.Data) Then
                                                               'Append error log from CUE2POPS
                                                               If LogTextBox.Dispatcher.CheckAccess() = False Then
                                                                   LogTextBox.Dispatcher.BeginInvoke(Sub()
                                                                                                         LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                                                                         LogTextBox.ScrollToEnd()
                                                                                                     End Sub)
                                                               Else
                                                                   LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                                   LogTextBox.ScrollToEnd()
                                                               End If
                                                           End If
                                                       End Sub

                'Start CUE2POPS & read process output data
                CUE2POPS.Start()
                CUE2POPS.BeginOutputReadLine()
                CUE2POPS.BeginErrorReadLine()
            Else
                MsgBox("No output folder specified.", MsgBoxStyle.Critical, "Error")
            End If
        Else
            MsgBox("No cue file selected.", MsgBoxStyle.Critical, "Error")
        End If
    End Sub

    Private Sub Add2SecToAllTrackIndexesCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles Add2SecToAllTrackIndexesCheckBox.Checked
        Sub2SecToAllTrackIndexesCheckBox.IsChecked = False
    End Sub

    Private Sub Sub2SecToAllTrackIndexesCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles Sub2SecToAllTrackIndexesCheckBox.Checked
        Add2SecToAllTrackIndexesCheckBox.IsChecked = False
    End Sub

    Private Sub CUE2POPS_Exited(sender As Object, e As EventArgs) Handles CUE2POPS.Exited
        CUE2POPS.Dispose()

        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub()
                                       Cursor = Cursors.Arrow
                                       If MsgBox("Done !" + vbCrLf + "Do you want to open the output folder ?", MsgBoxStyle.YesNo, "Success") = MsgBoxResult.Yes Then
                                           Process.Start("explorer", POPSOutputFolderTextBox.Text)
                                       End If
                                   End Sub)
        Else
            Cursor = Cursors.Arrow
            If MsgBox("Done !" + vbCrLf + "Do you want to open the output folder ?", MsgBoxStyle.YesNo, "Success") = MsgBoxResult.Yes Then
                Process.Start("explorer", POPSOutputFolderTextBox.Text)
            End If
        End If
    End Sub

End Class
