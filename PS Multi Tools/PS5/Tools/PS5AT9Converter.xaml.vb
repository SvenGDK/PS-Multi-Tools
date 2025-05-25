Imports System.IO
Imports System.Windows.Forms

Public Class PS5AT9Converter

    Private Sub BrowseWavFileButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseWavFileButton.Click
        Dim OFD As New OpenFileDialog() With {.Filter = "WAV Files (*.wav)|*.wav", .Multiselect = False, .Title = "Select a .wav file."}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            InputWavTextBox.Text = OFD.FileName

            Dim WAVFileInfos As New WAVFile()
            Using reader As New BinaryReader(File.Open(OFD.FileName, FileMode.Open))

                ' Read the wave file header from the buffer. 

                WAVFileInfos.RIFF = reader.ReadInt32()
                WAVFileInfos.TotalLength = reader.ReadInt32()
                WAVFileInfos.Wave = reader.ReadInt32()
                WAVFileInfos.FormatChunkMarker = reader.ReadInt32()
                WAVFileInfos.Subchunk1Size = reader.ReadInt32()
                WAVFileInfos.AudioFormat = reader.ReadInt16()
                WAVFileInfos.Channels = reader.ReadInt16()
                WAVFileInfos.SampleRate = reader.ReadInt32()
                WAVFileInfos.ByteRate = reader.ReadInt32()
                WAVFileInfos.BlockAlign = reader.ReadInt16()
                WAVFileInfos.BitsPerSample = reader.ReadInt16()

                Dim ExtraPars As Short() = Nothing
                If WAVFileInfos.Subchunk1Size <> 16 Then
                    Dim NoOfEPs As Short = reader.ReadInt16()
                    ExtraPars = New Short(NoOfEPs - 1) {}

                    For i As Integer = 0 To NoOfEPs - 1
                        ExtraPars(i) = reader.ReadInt16()
                    Next
                End If

                WAVFileInfos.Data = reader.ReadInt32()
                WAVFileInfos.DataLength = reader.ReadInt32()

                Dim NewByteArray As Byte() = New Byte(WAVFileInfos.DataLength - 1) {}
                NewByteArray = reader.ReadBytes(WAVFileInfos.DataLength)
            End Using

            WAVInfoTextBlock.Text = "WAV File Info - Format: " + WAVFileInfos.BitsPerSample.ToString + "bit PCM / " + "Sample Rate: " + WAVFileInfos.SampleRate.ToString + "Hz"
        End If
    End Sub

    Private Sub BrowseAt9FileButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseAt9FileButton.Click
        Dim OFD As New OpenFileDialog() With {.Filter = "AT9 Files (*.at9)|*.at9", .Multiselect = False, .Title = "Select a .at9 file."}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            InputAt9TextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub ConvertToAt9Button_Click(sender As Object, e As RoutedEventArgs) Handles ConvertToAt9Button.Click
        If Not String.IsNullOrEmpty(InputWavTextBox.Text) Then

            Dim EncodingBitRate As String = String.Empty
            Select Case EncodeBitrateComboBox.Text
                Case "1ch:72kbps"
                    EncodingBitRate = "-br 72"
                Case "2ch:144kbps"
                    EncodingBitRate = "-br 144"
                Case "4.0ch:240kbps"
                    EncodingBitRate = "-br 240"
                Case "5.1ch:300kbps"
                    EncodingBitRate = "-br 300"
                Case "7.1ch:420kbps"
                    EncodingBitRate = "-br 420"
                Case "Vibration 1ch: 24kbps"
                    EncodingBitRate = "-br 24"
                Case "Vibration 2ch: 48kbps"
                    EncodingBitRate = "-br 48"
            End Select

            Dim EncodingSampleRate As String = String.Empty
            Select Case EncodeSamplingRateComboBox.Text
                Case "12000"
                    EncodingSampleRate = "-fs 12000"
                Case "24000"
                    EncodingSampleRate = "-fs 24000"
                Case "48000"
                    EncodingSampleRate = "-fs 48000"
            End Select

            Dim EncodingOptions As New Text.StringBuilder()
            If Not String.IsNullOrEmpty(EncodingBitRate) Then
                EncodingOptions.Append(EncodingBitRate + " ")
            End If
            If Not String.IsNullOrEmpty(EncodingSampleRate) Then
                EncodingOptions.Append(EncodingSampleRate + " ")
            End If

            Dim NewFileName As String = Path.GetFileNameWithoutExtension(InputWavTextBox.Text) + ".at9"
            Dim NewFilePath As String = Path.GetDirectoryName(InputWavTextBox.Text) + "\" + NewFileName

            Try
                Using PubCMD As New Process()
                    PubCMD.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\PS5\at9tool.exe"

                    If Not String.IsNullOrEmpty(EncodingOptions.ToString()) Then
                        PubCMD.StartInfo.Arguments = "-e " + EncodingOptions.ToString() + """" + InputWavTextBox.Text + """ """ + NewFilePath + """"
                    Else
                        PubCMD.StartInfo.Arguments = "-e """ + InputWavTextBox.Text + """ """ + NewFilePath + """"
                    End If

                    PubCMD.StartInfo.UseShellExecute = False
                    PubCMD.StartInfo.CreateNoWindow = True
                    PubCMD.Start()
                End Using

                MsgBox("WAV file converted to AT9. The file can be found in the same directory.", MsgBoxStyle.Information, "WAV converted")
            Catch ex As Exception
                MsgBox("Could not convert selected WAV file", MsgBoxStyle.Critical, "Error")
                MsgBox(ex.Message)
            End Try
        Else
            MsgBox("No WAV file specified.", MsgBoxStyle.Critical, "No input file")
        End If
    End Sub

    Private Sub ConvertToWavButton_Click(sender As Object, e As RoutedEventArgs) Handles ConvertToWavButton.Click
        If Not String.IsNullOrEmpty(InputAt9TextBox.Text) Then

            Dim DecodeFormat As String = String.Empty
            Select Case EncodeSamplingRateComboBox.Text
                Case "16bit Integer PCM"
                    DecodeFormat = "-int16"
                Case "24bit Integer PCM"
                    DecodeFormat = "-int24"
                Case "IEEE float PCM"
                    DecodeFormat = "-float"
            End Select

            Dim NewFileName As String = Path.GetFileNameWithoutExtension(InputAt9TextBox.Text) + ".wav"
            Dim NewFilePath As String = Path.GetDirectoryName(InputAt9TextBox.Text) + "\" + NewFileName

            Try
                Using PubCMD As New Process()
                    PubCMD.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\PS5\at9tool.exe"

                    If Not String.IsNullOrEmpty(DecodeFormat) Then
                        PubCMD.StartInfo.Arguments = "-d " + DecodeFormat + " """ + InputAt9TextBox.Text + """ """ + NewFilePath + """"
                    Else
                        PubCMD.StartInfo.Arguments = "-d """ + InputAt9TextBox.Text + """ """ + NewFilePath + """"
                    End If

                    PubCMD.StartInfo.UseShellExecute = False
                    PubCMD.StartInfo.CreateNoWindow = True
                    PubCMD.Start()
                End Using

                MsgBox("AT9 file converted to WAV. The file can be found in the same directory.", MsgBoxStyle.Information, "AT9 converted")
            Catch ex As Exception
                MsgBox("Could not convert selected AT9 file", MsgBoxStyle.Critical, "Error")
                MsgBox(ex.Message)
            End Try
        Else
            MsgBox("No AT9 file specified.", MsgBoxStyle.Critical, "No input file")
        End If
    End Sub

End Class
