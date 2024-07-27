Imports System.IO

Public Class PS5MakefSelfs

    'PS5 Make Fake Self Batch Script by EchoStretch, translated in VB.NET

    Dim SelectedPath As String = ""

    Private Sub BrowseFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseFolderButton.Click
        Dim FBD As New Forms.FolderBrowserDialog()

        If FBD.ShowDialog() = Windows.Forms.DialogResult.OK Then
            SelectedDirectoryTextBox.Text = FBD.SelectedPath
            SelectedPath = FBD.SelectedPath
            MakeButton.IsEnabled = True

            If Not String.IsNullOrEmpty(MakeLogTextBox.Text) Then
                MakeLogTextBox.Clear()
            End If
        End If
    End Sub

    Private Sub MakeButton_Click(sender As Object, e As RoutedEventArgs) Handles MakeButton.Click
        If Not String.IsNullOrEmpty(SelectedDirectoryTextBox.Text) Then

            Dispatcher.BeginInvoke(Sub()
                                       MakeButton.IsEnabled = False
                                       SelectedDirectoryTextBox.IsEnabled = False
                                       BrowseFolderButton.IsEnabled = False
                                   End Sub)

            'Collect all files that need to be signed
            Dim FilesToSign = Directory.EnumerateFiles(SelectedPath, "*.*", SearchOption.AllDirectories).Where(Function(s) s.EndsWith(".prx") OrElse s.EndsWith(".sprx") OrElse s.EndsWith(".elf") OrElse s.EndsWith(".self") OrElse s.EndsWith(".bin"))
            'Fake sign each file with make_fself_python3-1
            For Each FileToSign In FilesToSign
                Dim FileToSignDirectory As String = Path.GetDirectoryName(FileToSign) + "\"
                Dim FileToSignFileName As String = Path.GetFileName(FileToSign)

                Using Make_fSELF As New Process()
                    Make_fSELF.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\make_fself_python3-1.exe"
                    Make_fSELF.StartInfo.Arguments = FileToSign + " " + FileToSignDirectory + FileToSignFileName + ".estemp"
                    Make_fSELF.StartInfo.RedirectStandardOutput = True
                    Make_fSELF.StartInfo.UseShellExecute = False
                    Make_fSELF.StartInfo.CreateNoWindow = True
                    Make_fSELF.Start()

                    'Read the output
                    Dim OutputReader As StreamReader = Make_fSELF.StandardOutput
                    Dim ProcessOutput As String = OutputReader.ReadToEnd()

                    If Not String.IsNullOrEmpty(ProcessOutput) Then
                        If Dispatcher.CheckAccess() = False Then
                            Dispatcher.BeginInvoke(Sub()
                                                       MakeLogTextBox.AppendText(ProcessOutput + vbCrLf)
                                                       MakeLogTextBox.ScrollToEnd()
                                                   End Sub)
                        Else
                            MakeLogTextBox.AppendText(ProcessOutput + vbCrLf)
                            MakeLogTextBox.ScrollToEnd()
                        End If
                    End If
                End Using

                'Backup original file
                File.Move(FileToSign, FileToSignDirectory + FileToSignFileName + ".esbak")
            Next

            'Rename the temp files to their origin file name
            For Each TempFile In Directory.GetFiles(SelectedPath, "*.estemp", SearchOption.AllDirectories)
                Dim TempFileDirectory As String = Path.GetDirectoryName(TempFile) + "\"
                Dim TempFileNameWithoutTempExtension As String = Path.GetFileNameWithoutExtension(TempFile)
                File.Move(TempFile, TempFileDirectory + TempFileNameWithoutTempExtension)
            Next

            Dispatcher.BeginInvoke(Sub()
                                       MakeButton.IsEnabled = True
                                       SelectedDirectoryTextBox.IsEnabled = True
                                       BrowseFolderButton.IsEnabled = True
                                   End Sub)

            MsgBox("SELF files fake signed!", MsgBoxStyle.Information, "Done")
        Else
            MsgBox("No folder selected!", MsgBoxStyle.Exclamation)
        End If
    End Sub

End Class
