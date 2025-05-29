Imports System.IO
Imports System.Windows.Forms

Public Class BINCUEConverter

    Public ConvertForPS1 As Boolean = False
    Public NewBaseName As String = ""
    Dim CUEFile As String = ""
    Dim BINFile As String = ""

    Private Sub BINCUEConverter_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If ConvertForPS1 Then
            IsForPSXCheckBox.IsChecked = True
        End If
    End Sub

    Private Sub BrowseCueButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseCueButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "CUE files (*.cue)|*.cue", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then

            SelectedCueTextBox.Text = OFD.FileName
            CUEFile = OFD.FileName
            NewBaseName = Path.GetFileNameWithoutExtension(CUEFile)

            Dim CheckForBinFile As String = Path.GetDirectoryName(OFD.FileName) + "\" + Path.GetFileNameWithoutExtension(OFD.FileName) + ".bin"
            If File.Exists(CheckForBinFile) Then
                SelectedBinTextBox.Text = CheckForBinFile
                BINFile = CheckForBinFile
            End If
        End If
    End Sub

    Private Sub BrowseBinButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseBinButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "BIN files (*.bin)|*.bin", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedBinTextBox.Text = OFD.FileName
            BINFile = OFD.FileName
        End If
    End Sub

    Private Sub ConvertButton_Click(sender As Object, e As RoutedEventArgs) Handles ConvertButton.Click
        If Not String.IsNullOrEmpty(SelectedCueTextBox.Text) AndAlso File.Exists(SelectedCueTextBox.Text) Then

            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub() LogTextBox.Clear())
            Else
                LogTextBox.Clear()
            End If

            'Create Converted folder if not exists
            If Not Directory.Exists(Environment.CurrentDirectory + "\Converted") Then
                Directory.CreateDirectory(Environment.CurrentDirectory + "\Converted")
                Directory.CreateDirectory(Environment.CurrentDirectory + "\Converted\ISO")
            End If

            Using BChunk As New Process()
                BChunk.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\bchunk.exe"

                If IsForPSXCheckBox.IsChecked Then
                    BChunk.StartInfo.Arguments = "-p """ + BINFile + """ """ + CUEFile + """ """ + NewBaseName + """"
                Else
                    BChunk.StartInfo.Arguments = """" + BINFile + """ """ + CUEFile + """ """ + NewBaseName + """"
                End If

                BChunk.StartInfo.RedirectStandardOutput = True
                BChunk.StartInfo.RedirectStandardError = True
                BChunk.StartInfo.UseShellExecute = False
                BChunk.StartInfo.CreateNoWindow = True
                BChunk.EnableRaisingEvents = True

                AddHandler BChunk.OutputDataReceived, AddressOf ConverToISOOutputDataRecieved

                BChunk.Start()
                BChunk.BeginOutputReadLine()
            End Using
        End If
    End Sub

    Private Sub ConverToISOOutputDataRecieved(sender As Object, e As DataReceivedEventArgs)
        If Not String.IsNullOrEmpty(e.Data) Then

            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           LogTextBox.AppendText(e.Data & vbCrLf)
                                           LogTextBox.ScrollToEnd()
                                       End Sub)
            Else
                LogTextBox.AppendText(e.Data & vbCrLf)
                LogTextBox.ScrollToEnd()
            End If

            If e.Data.Contains("End of Conversion") Then
                If File.Exists(NewBaseName + "01.iso") Then

                    File.Move(NewBaseName + "01.iso", Environment.CurrentDirectory + "\Converted\ISO\" + NewBaseName + "01.iso")

                    If MsgBox("Converted ! Do you want the open the folder containing the new ISO file ?", MsgBoxStyle.YesNo, "Completed") = MsgBoxResult.Yes Then
                        Process.Start("explorer", Environment.CurrentDirectory + "\Converted\ISO")
                    End If
                Else
                    If MsgBox("Converted, but the file could not be found. Do you want to check the Tools folder ?", MsgBoxStyle.YesNo, "Completed") = MsgBoxResult.Yes Then
                        Process.Start("explorer", Environment.CurrentDirectory + "\Tools")
                    End If
                End If
            End If

        End If
    End Sub

End Class
