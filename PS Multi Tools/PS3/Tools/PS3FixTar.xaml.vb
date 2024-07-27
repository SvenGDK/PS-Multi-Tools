Imports System.IO
Imports System.Windows.Forms

Public Class PS3FixTar

    Private Sub BrowseTarButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseTarButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "tar files (*.tar)|*.tar", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedTarFileTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub FixTarButton_Click(sender As Object, e As RoutedEventArgs) Handles FixTarButton.Click
        If Not String.IsNullOrEmpty(SelectedTarFileTextBox.Text) And File.Exists(SelectedTarFileTextBox.Text) Then
            Dim ProcessOutput As String = ""
            Using FixTar As New Process()
                FixTar.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\fix_tar_v3.exe"
                FixTar.StartInfo.Arguments = """" + SelectedTarFileTextBox.Text + """"
                FixTar.StartInfo.RedirectStandardOutput = True
                FixTar.StartInfo.UseShellExecute = False
                FixTar.StartInfo.CreateNoWindow = True
                FixTar.Start()

                'Read the output
                Dim OutputReader As StreamReader = FixTar.StandardOutput
                ProcessOutput = OutputReader.ReadToEnd()
            End Using
            MsgBox(ProcessOutput)
        End If
    End Sub

End Class
