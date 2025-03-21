Imports System.IO
Imports System.Windows.Forms

Public Class PS5RcoExtractor

    Private Sub BrowseFileButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseFileButton.Click
        Dim OFD As New OpenFileDialog() With {.Filter = "RCO file (*.rco)|*.rco", .Title = "Select a .rco file", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedRCOPathTextBox.Text = OFD.FileName
            ExtractButton.IsEnabled = True
        End If
    End Sub

    Private Sub BrowseFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseFolderButton.Click
        Dim FBD As New FolderBrowserDialog() With {.Description = "Select a folder containing .rco files"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedRCOPathTextBox.Text = FBD.SelectedPath
            ExtractButton.IsEnabled = True
        End If
    End Sub

    Private Sub ExtractButton_Click(sender As Object, e As RoutedEventArgs) Handles ExtractButton.Click
        If Not String.IsNullOrEmpty(SelectedRCOPathTextBox.Text) Then

            Cursor = Input.Cursors.Wait
            Dim RCODirectory As String = ""

            If SelectedRCOPathTextBox.Text.EndsWith(".rco") Then
                RCODirectory = Path.GetDirectoryName(SelectedRCOPathTextBox.Text)
                Dim RCOFilename As String = Path.GetFileName(SelectedRCOPathTextBox.Text)

                If File.Exists(Environment.CurrentDirectory + "\Tools\sngre.exe") Then
                    'Copy sngre to the file's path and extract the selected .rco file
                    File.Copy(Environment.CurrentDirectory + "\Tools\sngre.exe", RCODirectory + "\sngre.exe", True)

                    'Switch to the directory
                    Directory.SetCurrentDirectory(RCODirectory)

                    'Start extraction
                    Using SNGRE As New Process()
                        SNGRE.StartInfo.FileName = "sngre.exe"
                        SNGRE.StartInfo.Arguments = "-f " + RCOFilename
                        SNGRE.StartInfo.RedirectStandardOutput = True
                        SNGRE.StartInfo.UseShellExecute = False
                        SNGRE.StartInfo.CreateNoWindow = True
                        SNGRE.Start()
                    End Using
                Else
                    MsgBox("Could not find the extraction tool. Make sure sngre.exe exists in the Tools folder.", MsgBoxStyle.Exclamation, "Error trying to extract")
                End If
            Else
                'Copy sngre to the folder and extract all .rco files
                File.Copy(Environment.CurrentDirectory + "\Tools\sngre.exe", SelectedRCOPathTextBox.Text + "\sngre.exe", True)

                'Switch to the directory
                Directory.SetCurrentDirectory(SelectedRCOPathTextBox.Text)

                'Enumerate all files at the given directory
                For Each RCOFile In Directory.GetFiles(SelectedRCOPathTextBox.Text)
                    'Get the filename
                    Dim RCOFilename As String = Path.GetFileName(RCOFile)
                    'Start extraction
                    Using SNGRE As New Process()
                        SNGRE.StartInfo.FileName = "sngre.exe"
                        SNGRE.StartInfo.Arguments = "-f " + RCOFilename
                        SNGRE.StartInfo.RedirectStandardOutput = True
                        SNGRE.StartInfo.UseShellExecute = False
                        SNGRE.StartInfo.CreateNoWindow = True
                        SNGRE.Start()
                    End Using
                Next

            End If

            Cursor = Input.Cursors.Arrow

            'Set the current directory back
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory)

            If MsgBox("Extraction completed!" + vbCrLf + "Do you want to open the folder containing the extracted files?", MsgBoxStyle.YesNo, "Extraction done") = MsgBoxResult.Yes Then
                If SelectedRCOPathTextBox.Text.EndsWith(".rco") Then
                    Process.Start("explorer", RCODirectory)
                Else
                    Process.Start("explorer", SelectedRCOPathTextBox.Text)
                End If
            End If
        Else
            MsgBox("No file or folder selected.", MsgBoxStyle.Exclamation, "Error trying to extract")
        End If
    End Sub

End Class
