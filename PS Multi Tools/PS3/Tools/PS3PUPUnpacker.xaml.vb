Imports System.IO
Imports System.Windows.Forms

Public Class PS3PUPUnpacker

#Region "Browse Buttons"

    Private Sub BrowsePUPButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePUPButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "pup files (*.pup)|*.pup", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPUPTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseOutputButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseOutputButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select a folder where you want to save the extracted PUP"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedOutputFolderTextBox.Text = FBD.SelectedPath
        End If
    End Sub

#End Region

    Private Sub UnpackButton_Click(sender As Object, e As RoutedEventArgs) Handles UnpackButton.Click

        If Not String.IsNullOrEmpty(SelectedPUPTextBox.Text) And File.Exists(SelectedPUPTextBox.Text) And Not String.IsNullOrEmpty(SelectedOutputFolderTextBox.Text) And Directory.Exists(SelectedOutputFolderTextBox.Text) Then
            Dim ProcessOutput As String = ""
            Using PUPUnpack As New Process()
                PUPUnpack.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\pupunpack.exe"
                PUPUnpack.StartInfo.Arguments = """" + SelectedPUPTextBox.Text + """ " + """" + SelectedOutputFolderTextBox.Text + """"
                PUPUnpack.StartInfo.RedirectStandardOutput = True
                PUPUnpack.StartInfo.UseShellExecute = False
                PUPUnpack.StartInfo.CreateNoWindow = True
                PUPUnpack.Start()

                'Read the output
                Dim OutputReader As StreamReader = PUPUnpack.StandardOutput
                ProcessOutput = OutputReader.ReadToEnd()
            End Using
            MsgBox(ProcessOutput)
        End If

    End Sub

End Class
