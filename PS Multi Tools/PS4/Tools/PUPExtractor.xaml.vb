Imports System.Windows.Forms

Public Class PUPExtractor

    Private Sub ExtractButton_Click(sender As Object, e As RoutedEventArgs) Handles ExtractButton.Click

        Dim InputPUPDec As String = SelectedFileTextBox.Text
        Dim OutputFolder As String = SelectedOutputFolderTextBox.Text

        Using PUPUnpacker As New Process()
            PUPUnpacker.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\pup_unpacker.exe"
            PUPUnpacker.StartInfo.Arguments = """" + InputPUPDec + """ " + """" + OutputFolder + """"
            PUPUnpacker.StartInfo.RedirectStandardOutput = True
            AddHandler PUPUnpacker.OutputDataReceived, AddressOf OutputHandler
            PUPUnpacker.StartInfo.UseShellExecute = False
            PUPUnpacker.StartInfo.CreateNoWindow = True
            PUPUnpacker.Start()
            PUPUnpacker.BeginOutputReadLine()
            PUPUnpacker.WaitForExit()
        End Using

    End Sub

    Public Sub OutputHandler(sender As Object, e As DataReceivedEventArgs)
        If Not String.IsNullOrEmpty(e.Data) Then
            If LogTextBox.Dispatcher.CheckAccess() = False Then
                LogTextBox.Dispatcher.BeginInvoke(Sub()
                                                      LogTextBox.AppendText(vbCrLf + e.Data)
                                                      LogTextBox.ScrollToEnd()
                                                  End Sub)
            Else
                LogTextBox.AppendText(vbCrLf + e.Data)
                LogTextBox.ScrollToEnd()
            End If
        End If
    End Sub

    Private Sub BrowseButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseButton.Click
        Dim OFD As New OpenFileDialog() With {.Filter = "Decrypted PS4 Update File (PUP.dec)|*.PUP.dec", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedFileTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseOutputButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseOutputButton.Click
        Dim FBD As New FolderBrowserDialog() With {.Description = "Select your extraction folder", .RootFolder = Environment.SpecialFolder.Desktop}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedOutputFolderTextBox.Text = FBD.SelectedPath
        End If
    End Sub

End Class
