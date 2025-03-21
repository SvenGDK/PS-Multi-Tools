Imports System.IO
Imports System.Windows.Forms

Public Class PS3CoreOSTools

#Region "Browse Buttons"

    Private Sub BrowseCoreOSPKGButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseCoreOSPKGButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "PKG files (*.pkg)|*.pkg", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedCoreOSPKGForDecTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseDecOutputFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseDecOutputFolderButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select a folder where you want to save the decrypted core_os", .ShowNewFolderButton = True}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedOutputFolderForDecTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub BrowseExtrOutputFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseExtrOutputFolderButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select the decrypted core_os folder that will be extracted", .ShowNewFolderButton = True}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedDecFolderForExtrTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub BrowseDecFolderForEncButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseDecFolderForEncButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select a decrypted core_os folder", .ShowNewFolderButton = True}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedDecFolderForEncTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub BrowseOutFolderForEncButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseOutFolderForEncButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select a folder where you want to save the encrypted core_os", .ShowNewFolderButton = True}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedNewFolderForEncTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub BrowseReadButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseReadButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "PKG files (*.pkg)|*.pkg", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedCoreOSPKGForReadTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseDumpButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseDumpButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select a decrypted core_os folder", .ShowNewFolderButton = True}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedDecFolderForDumpTextBox.Text = FBD.SelectedPath
        End If
    End Sub



#End Region

    Private Sub DecryptButton_Click(sender As Object, e As RoutedEventArgs) Handles DecryptButton.Click
        If Not String.IsNullOrEmpty(SelectedCoreOSPKGForDecTextBox.Text) And File.Exists(SelectedCoreOSPKGForDecTextBox.Text) And Not String.IsNullOrEmpty(SelectedOutputFolderForDecTextBox.Text) And Directory.Exists(SelectedOutputFolderForDecTextBox.Text) Then

            Dim ProcessOutput As String = ""
            Using FWPKG As New Process()
                FWPKG.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\fwpkg.exe"
                FWPKG.StartInfo.Arguments = "d """ + SelectedCoreOSPKGForDecTextBox.Text + """ """ + SelectedOutputFolderForDecTextBox.Text + """"
                FWPKG.StartInfo.RedirectStandardOutput = True
                FWPKG.StartInfo.UseShellExecute = False
                FWPKG.StartInfo.CreateNoWindow = True
                FWPKG.Start()

                'Read the output
                Dim OutputReader As StreamReader = FWPKG.StandardOutput
                ProcessOutput = OutputReader.ReadToEnd()
            End Using
            MsgBox(ProcessOutput)
        End If
    End Sub

    Private Sub ExtractButton_Click(sender As Object, e As RoutedEventArgs) Handles ExtractButton.Click
        If Not String.IsNullOrEmpty(SelectedDecFolderForExtrTextBox.Text) And Directory.Exists(SelectedDecFolderForExtrTextBox.Text) Then

            Dim ProcessOutput As String = ""
            Using Discore As New Process()
                Discore.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\discore.exe"
                Discore.StartInfo.Arguments = """" + SelectedDecFolderForExtrTextBox.Text + """"
                Discore.StartInfo.RedirectStandardOutput = True
                Discore.StartInfo.UseShellExecute = False
                Discore.StartInfo.CreateNoWindow = True
                Discore.Start()

                'Read the output
                Dim OutputReader As StreamReader = Discore.StandardOutput
                ProcessOutput = OutputReader.ReadToEnd()
            End Using
            MsgBox(ProcessOutput)
        End If
    End Sub

    Private Sub EncryptButton_Click(sender As Object, e As RoutedEventArgs) Handles EncryptButton.Click
        If Not String.IsNullOrEmpty(SelectedDecFolderForEncTextBox.Text) And Directory.Exists(SelectedDecFolderForEncTextBox.Text) And Not String.IsNullOrEmpty(SelectedNewFolderForEncTextBox.Text) And Directory.Exists(SelectedNewFolderForEncTextBox.Text) Then

            Dim ProcessOutput As String = ""
            Using FWPKG As New Process()
                FWPKG.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\fwpkg.exe"
                FWPKG.StartInfo.Arguments = "e """ + SelectedDecFolderForEncTextBox.Text + """ """ + SelectedNewFolderForEncTextBox.Text + """"
                FWPKG.StartInfo.RedirectStandardOutput = True
                FWPKG.StartInfo.UseShellExecute = False
                FWPKG.StartInfo.CreateNoWindow = True
                FWPKG.Start()

                'Read the output
                Dim OutputReader As StreamReader = FWPKG.StandardOutput
                ProcessOutput = OutputReader.ReadToEnd()
            End Using
            MsgBox(ProcessOutput)
        End If
    End Sub

    Private Sub ReadButton_Click(sender As Object, e As RoutedEventArgs) Handles ReadButton.Click
        If Not String.IsNullOrEmpty(SelectedCoreOSPKGForReadTextBox.Text) And File.Exists(SelectedCoreOSPKGForReadTextBox.Text) Then

            Dim ProcessOutput As String = ""
            Using COS As New Process()
                COS.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\cos.exe"
                COS.StartInfo.Arguments = "-i """ + SelectedCoreOSPKGForReadTextBox.Text + """"
                COS.StartInfo.RedirectStandardOutput = True
                COS.StartInfo.UseShellExecute = False
                COS.StartInfo.CreateNoWindow = True
                COS.Start()

                'Read the output
                Dim OutputReader As StreamReader = COS.StandardOutput
                ProcessOutput = OutputReader.ReadToEnd()
            End Using
            MsgBox(ProcessOutput)
        End If
    End Sub

    Private Sub DumpButton_Click(sender As Object, e As RoutedEventArgs) Handles DumpButton.Click
        If Not String.IsNullOrEmpty(SelectedDecFolderForDumpTextBox.Text) And Directory.Exists(SelectedDecFolderForDumpTextBox.Text) Then

            Dim ProcessOutput As String = ""
            'Create a file name based on the selected folder
            Dim NewSplit As String() = SelectedDecFolderForDumpTextBox.Text.Split("\"c)
            Dim FolderName As String = NewSplit(NewSplit.Length - 2) + ".hex"

            Using Hexdump As New Process()
                Hexdump.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\hexdump.exe"
                Hexdump.StartInfo.Arguments = """" + SelectedCoreOSPKGForReadTextBox.Text + """ > """ + FolderName + """"
                Hexdump.StartInfo.RedirectStandardOutput = True
                Hexdump.StartInfo.UseShellExecute = False
                Hexdump.StartInfo.CreateNoWindow = True
                Hexdump.Start()

                'Read the output
                Dim OutputReader As StreamReader = Hexdump.StandardOutput
                ProcessOutput = OutputReader.ReadToEnd()
            End Using
            MsgBox(ProcessOutput)
        End If
    End Sub

End Class
