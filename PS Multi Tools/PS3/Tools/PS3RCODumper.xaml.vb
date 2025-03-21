Imports System.IO
Imports System.Windows.Forms

Public Class PS3RCODumper

#Region "Browse Buttons"

    Private Sub BrowseRCOButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseRCOButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "RCO files (*.rco)|*.rco", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedRCOFileTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseOutputButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseOutputButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select a folder where you want to dump the rco file"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedOutputFolderTextBox.Text = FBD.SelectedPath
        End If
    End Sub

#End Region

    Private Sub UnpackButton_Click(sender As Object, e As RoutedEventArgs) Handles UnpackButton.Click
        If Not String.IsNullOrEmpty(SelectedRCOFileTextBox.Text) And File.Exists(SelectedRCOFileTextBox.Text) And Not String.IsNullOrEmpty(SelectedOutputFolderTextBox.Text) Then

            Dim XMLFileName As String = Path.GetFileNameWithoutExtension(SelectedRCOFileTextBox.Text) + ".xml"
            Dim ResDir As String = SelectedOutputFolderTextBox.Text + "\RCO"
            Dim ProcessOutput As String = ""

            If Not Directory.Exists(ResDir) Then
                Directory.CreateDirectory(ResDir)
            End If

            Using PS3RCOMage As New Process()
                PS3RCOMage.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\rcomage.exe"
                PS3RCOMage.StartInfo.Arguments = "dump """ + SelectedRCOFileTextBox.Text + """ """ + SelectedOutputFolderTextBox.Text + "\" + XMLFileName + """ --resdir """ + ResDir + """"
                PS3RCOMage.StartInfo.RedirectStandardOutput = True
                PS3RCOMage.StartInfo.UseShellExecute = False
                PS3RCOMage.StartInfo.CreateNoWindow = True
                PS3RCOMage.Start()

                'Read the output
                Dim OutputReader As StreamReader = PS3RCOMage.StandardOutput
                ProcessOutput = OutputReader.ReadToEnd()
            End Using
            MsgBox(ProcessOutput)
        End If
    End Sub

End Class
