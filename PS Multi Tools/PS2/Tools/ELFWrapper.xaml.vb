Public Class ELFWrapper

    Dim WithEvents SCEDoormat_NoME As New Process()

    Private Sub BrowseELFButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseELFButton.Click
        Dim OFD As New Forms.OpenFileDialog() With {.CheckFileExists = True, .Filter = "ELF files (*.ELF)|*.ELF", .Multiselect = False}
        If OFD.ShowDialog() = Windows.Forms.DialogResult.OK Then
            SelectedELFTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseOutputFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseOutputFolderButton.Click
        Dim FBD As New Forms.FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select a folder where you want to save the KELF file"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            KELFOutputFolderTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub WrapButton_Click(sender As Object, e As RoutedEventArgs) Handles WrapButton.Click
        If Not String.IsNullOrEmpty(SelectedELFTextBox.Text) Then
            If Not String.IsNullOrEmpty(KELFOutputFolderTextBox.Text) Then
                Cursor = Cursors.Wait

                Dim RetailKHNPath As String = My.Computer.FileSystem.CurrentDirectory + "\Tools\KRYPTO(CEX).KHN"
                Dim PSXKHNPath As String = My.Computer.FileSystem.CurrentDirectory + "\Tools\KRYPTO.KHN"
                Dim AllKHNPath As String = My.Computer.FileSystem.CurrentDirectory + "\Tools\KRYPTO(All).KHN"

                If LogTextBox.Dispatcher.CheckAccess() = False Then
                    LogTextBox.Dispatcher.BeginInvoke(Sub() LogTextBox.Clear())
                Else
                    LogTextBox.Clear()
                End If

                'Set SCEDoormat_NoME process properties
                SCEDoormat_NoME = New Process()
                SCEDoormat_NoME.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\SCEDoormat_NoME.exe"

                If ForRetailCheckBox.IsChecked Then
                    SCEDoormat_NoME.StartInfo.Arguments = """" + SelectedELFTextBox.Text + """ """ + KELFOutputFolderTextBox.Text + "\EXECUTE.KELF"" """ + RetailKHNPath + """"
                ElseIf ForPSXCheckBox.IsChecked Then
                    SCEDoormat_NoME.StartInfo.Arguments = """" + SelectedELFTextBox.Text + """ """ + KELFOutputFolderTextBox.Text + "\EXECUTE.KELF"" """ + PSXKHNPath + """"
                ElseIf ForAllCheckBox.IsChecked Then
                    SCEDoormat_NoME.StartInfo.Arguments = """" + SelectedELFTextBox.Text + """ """ + KELFOutputFolderTextBox.Text + "\EXECUTE.KELF"" """ + AllKHNPath + """"
                End If

                SCEDoormat_NoME.StartInfo.RedirectStandardOutput = True
                SCEDoormat_NoME.StartInfo.RedirectStandardError = True
                SCEDoormat_NoME.StartInfo.UseShellExecute = False
                SCEDoormat_NoME.StartInfo.CreateNoWindow = True
                SCEDoormat_NoME.EnableRaisingEvents = True

                AddHandler SCEDoormat_NoME.OutputDataReceived, Sub(SenderProcess As Object, DataArgs As DataReceivedEventArgs)
                                                                   If Not String.IsNullOrEmpty(DataArgs.Data) Then
                                                                       'Append output log from SCEDoormat_NoME
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

                AddHandler SCEDoormat_NoME.ErrorDataReceived, Sub(SenderProcess As Object, DataArgs As DataReceivedEventArgs)
                                                                  If Not String.IsNullOrEmpty(DataArgs.Data) Then
                                                                      'Append error log from SCEDoormat_NoME
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

                'Start SCEDoormat_NoME & read process output data
                SCEDoormat_NoME.Start()
                SCEDoormat_NoME.BeginOutputReadLine()
                SCEDoormat_NoME.BeginErrorReadLine()
            Else
                MsgBox("No output folder specified.", MsgBoxStyle.Critical, "Error")
            End If
        Else
            MsgBox("No ELF file selected.", MsgBoxStyle.Critical, "Error")
        End If
    End Sub

    Private Sub ForRetailCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ForRetailCheckBox.Checked
        ForPSXCheckBox.IsChecked = False
        ForAllCheckBox.IsChecked = False
    End Sub

    Private Sub ForPSXCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ForPSXCheckBox.Checked
        ForRetailCheckBox.IsChecked = False
        ForAllCheckBox.IsChecked = False
    End Sub

    Private Sub ForAllCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ForAllCheckBox.Checked
        ForPSXCheckBox.IsChecked = False
        ForRetailCheckBox.IsChecked = False
    End Sub

    Private Sub SCEDoormat_NoME_Exited(sender As Object, e As EventArgs) Handles SCEDoormat_NoME.Exited
        SCEDoormat_NoME.Dispose()

        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub() Cursor = Cursors.Arrow)
        Else
            Cursor = Cursors.Arrow
        End If
    End Sub

End Class
