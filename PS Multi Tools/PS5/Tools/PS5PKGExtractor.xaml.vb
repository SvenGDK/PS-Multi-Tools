Imports System.Security.Cryptography
Imports System.Windows.Forms
Imports EndianIOExtension

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
            If UsePKGDec5CheckBox.IsChecked Then
                If Not String.IsNullOrEmpty(ExtractToTextBox.Text) Then
                    Try
                        Dim OutputPath As String = ExtractToTextBox.Text
                        Dim NewEndianIO As New EndianIO(FileToExtractTextBox.Text, EndianType.BigEndian, True)

                        NewEndianIO.SeekTo(9600)

                        Dim DecryptedData As Byte() = PKGDecryptor.RSA2048Decrypt(NewEndianIO.In.ReadBytes(384), PKGDecryptor.RSAKeyset.PkgDerivedKey3Keyset)
                        Dim PKGEntriesCount As UInteger = NewEndianIO.In.SeekNReadUInt32(16)
                        Dim PKGFileTableOffset As UInteger = NewEndianIO.In.SeekNReadUInt32(24)

                        NewEndianIO.SeekTo(PKGFileTableOffset)

                        'Get package entries
                        Dim PKGEntries As PKGDecryptor.PackageEntry() = New PKGDecryptor.PackageEntry(CInt(PKGEntriesCount - 1)) {}
                        Dim i As Integer = 0
                        While i < PKGEntriesCount
                            PKGEntries(i).type = NewEndianIO.In.ReadUInt32()
                            PKGEntries(i).unk1 = NewEndianIO.In.ReadUInt32()
                            PKGEntries(i).flags1 = NewEndianIO.In.ReadUInt32()
                            PKGEntries(i).flags2 = NewEndianIO.In.ReadUInt32()
                            PKGEntries(i).offset = NewEndianIO.In.ReadUInt32()
                            PKGEntries(i).size = NewEndianIO.In.ReadUInt32()
                            PKGEntries(i).padding = NewEndianIO.In.ReadBytes(8)

                            PKGEntries(i).key_index = CUInt((PKGEntries(i).flags2 And 61440) >> 12)
                            PKGEntries(i).is_encrypted = (PKGEntries(i).flags1 And 2147483648UI) <> 0
                            i += 1
                        End While

                        i = 0
                        While i < PKGEntriesCount
                            If PKGEntries(i).is_encrypted Then
                                'Extract the encrypted package entry to \decrypted
                                Dim PKGEntryData As Byte() = New Byte(63) {}
                                Array.Copy(PKGEntries(i).ToArray(), PKGEntryData, 32)
                                Array.Copy(DecryptedData, 0, PKGEntryData, 32, 32)

                                Dim IV As Byte() = New Byte(15) {}, key = New Byte(15) {}
                                Dim Hash As Byte() = SHA3_256.HashData(PKGEntryData)
                                Array.Copy(Hash, 0, IV, 0, 16)
                                Array.Copy(Hash, 16, key, 0, 16)

                                NewEndianIO.In.BaseStream.Position = PKGEntries(i).offset
                                Dim PKGEntrySize As UInteger = PKGEntries(i).size
                                If PKGEntrySize Mod 16 <> 0 Then
                                    PKGEntrySize = CUInt(PKGEntrySize + 16 - PKGEntrySize Mod 16)
                                End If

                                ExtractionLogTextBox.AppendText("Extracted decrypted entry: " & IO.Path.GetFileName(FileToExtractTextBox.Text) & "_" & PKGEntries(i).type.ToString("X") + $"- Size: {PKGEntrySize}" & vbCrLf)
                                ExtractionLogTextBox.ScrollToEnd()

                                Dim PKGEntryFileData As Byte() = New Byte(CInt(PKGEntrySize - 1)) {}
                                PKGDecryptor.AesCbcCfb128Decrypt(PKGEntryFileData, NewEndianIO.In.ReadBytes(PKGEntrySize), PKGEntrySize, key, IV)
                                If Not IO.Directory.Exists(OutputPath & "\decrypted") Then IO.Directory.CreateDirectory(OutputPath & "\decrypted")
                                Dim OutPath As String = IO.Path.Combine(OutputPath & "\decrypted\" & IO.Path.GetFileName(FileToExtractTextBox.Text) & "_" & PKGEntries(i).type.ToString("X"))
                                IO.File.WriteAllBytes(OutPath, PKGEntryFileData)
                            Else
                                'Extract the unencrypted package entry anyway to \unencrypted
                                Dim PKGEntryData As Byte() = New Byte(63) {}
                                Array.Copy(PKGEntries(i).ToArray(), PKGEntryData, 32)
                                Array.Copy(DecryptedData, 0, PKGEntryData, 32, 32)

                                Dim IV As Byte() = New Byte(15) {}, key = New Byte(15) {}
                                Dim Hash As Byte() = SHA3_256.HashData(PKGEntryData)
                                Array.Copy(Hash, 0, IV, 0, 16)
                                Array.Copy(Hash, 16, key, 0, 16)

                                NewEndianIO.In.BaseStream.Position = PKGEntries(i).offset
                                Dim PKGEntrySize As UInteger = PKGEntries(i).size
                                If PKGEntrySize Mod 16 <> 0 Then
                                    PKGEntrySize = CUInt(PKGEntrySize + 16 - PKGEntrySize Mod 16)
                                End If

                                ExtractionLogTextBox.AppendText("Extracted unencrypted entry: " & IO.Path.GetFileName(FileToExtractTextBox.Text) & "_" & PKGEntries(i).type.ToString("X") + $"- Size: {PKGEntrySize}" & vbCrLf)
                                ExtractionLogTextBox.ScrollToEnd()

                                Dim PKGEntryFileData As Byte() = New Byte(CInt(PKGEntrySize - 1)) {}
                                PKGDecryptor.AesCbcCfb128Decrypt(PKGEntryFileData, NewEndianIO.In.ReadBytes(PKGEntrySize), PKGEntrySize, key, IV)
                                If Not IO.Directory.Exists(OutputPath & "\unencrypted") Then IO.Directory.CreateDirectory(OutputPath & "\unencrypted")
                                Dim OutPath As String = IO.Path.Combine(OutputPath & "\unencrypted\" & IO.Path.GetFileName(FileToExtractTextBox.Text) & "_" & PKGEntries(i).type.ToString("X"))
                                IO.File.WriteAllBytes(OutPath, PKGEntryFileData)
                            End If

                            i += 1
                        End While

                        MsgBox("Done!", MsgBoxStyle.Information)
                    Catch ex As Exception
                        MsgBox(ex.ToString())
                    End Try
                End If
            Else
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
