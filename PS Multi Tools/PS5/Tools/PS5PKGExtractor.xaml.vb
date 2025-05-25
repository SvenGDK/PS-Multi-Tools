Imports System.Windows.Forms

Public Class PS5PKGExtractor

    Private Sub BrowseFileToExtractButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseFileToExtractButton.Click
        Dim OFD As New OpenFileDialog() With {.Filter = "PKG Files (*.pkg)|*.pkg", .Multiselect = False, .Title = "Select a .pkg file created for PS5."}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            FileToExtractTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseExtractDestinationPathButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseExtractDestinationPathButton.Click
        Dim FBD As New FolderBrowserDialog() With {.Description = "Select a destination path for the extraction", .ShowNewFolderButton = True}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            ExtractToTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub ExtractButton_Click(sender As Object, e As RoutedEventArgs) Handles ExtractButton.Click
        If Not String.IsNullOrEmpty(FileToExtractTextBox.Text) Then
            If Not String.IsNullOrEmpty(ExtractPasscodeTextBox.Text) Then
                If Not String.IsNullOrEmpty(ExtractToTextBox.Text) Then

                    Cursor = Input.Cursors.Wait
                    IsEnabled = False

                    Dim PubCMD As New Process() With {.EnableRaisingEvents = True}
                    Dim PubCMDStartInfo As New ProcessStartInfo With {
                    .FileName = Environment.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe",
                    .Arguments = "img_extract --passcode " + ExtractPasscodeTextBox.Text + " """ + FileToExtractTextBox.Text + """ """ + ExtractToTextBox.Text + """",
                    .RedirectStandardOutput = True,
                    .UseShellExecute = False,
                    .CreateNoWindow = True
                    }

                    AddHandler PubCMD.OutputDataReceived, AddressOf PubCMD_OutputDataReceived
                    AddHandler PubCMD.Exited, AddressOf PubCMD_Exited

                    PubCMD.StartInfo = PubCMDStartInfo
                    PubCMD.Start()
                    PubCMD.BeginOutputReadLine()

                Else
                    MsgBox("No destionation path set.", MsgBoxStyle.Exclamation)
                End If
            Else
                MsgBox("No passcode entered.", MsgBoxStyle.Exclamation)
            End If
        Else
            MsgBox("No pkg for extraction selected.", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Public Sub PubCMD_OutputDataReceived(sender As Object, e As DataReceivedEventArgs)
        If e IsNot Nothing Then
            If Not String.IsNullOrEmpty(e.Data) Then
                Dispatcher.BeginInvoke(Sub()
                                           ExtractionLogTextBox.AppendText(e.Data & vbCrLf)
                                           ExtractionLogTextBox.ScrollToEnd()
                                       End Sub)
            End If
        End If
    End Sub

    Private Sub PubCMD_Exited(sender As Object, e As EventArgs)
        Dispatcher.BeginInvoke(Sub()
                                   Cursor = Input.Cursors.Arrow
                                   IsEnabled = True
                                   If MsgBox("PKG extraction done! Open output folder ?", MsgBoxStyle.YesNo, "Success") = MsgBoxResult.Yes Then
                                       Process.Start("explorer", IO.Path.GetDirectoryName(ExtractToTextBox.Text))
                                   End If
                               End Sub)
    End Sub

End Class
