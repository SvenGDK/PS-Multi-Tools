Imports System.IO
Imports System.Windows.Forms
Imports DiscUtils.Iso9660

Public Class PS3ISOTools

    Private WithEvents PS3NetSrvProcess As Process = Nothing
    Private PS3NetSrvProcessAction As String = ""

    Public ISOToCreate As String = String.Empty
    Public ISOToExtract As String = String.Empty
    Public ISOToSplit As String = String.Empty
    Public ISOToPatch As String = String.Empty
    Public ISOToDecrypt As String = String.Empty

    Private Created As Boolean = False
    Private Patched As Boolean = False
    Private Extracted As Boolean = False
    Private Splitted As Boolean = False
    Private Decrypted As Boolean = False

    Private Sub PS3ISOTools_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Read input from the library
        If Not String.IsNullOrEmpty(ISOToCreate) Then
            SelectedGameBackupFolderTextBox.Text = ISOToCreate
        ElseIf Not String.IsNullOrEmpty(ISOToExtract) Then
            SelectedISOTextBox.Text = ISOToExtract
            ExtractISOCheckBox.IsChecked = True
        ElseIf Not String.IsNullOrEmpty(ISOToSplit) Then
            SelectedISOTextBox.Text = ISOToSplit
            SplitCustomISOCheckBox.IsChecked = True
        ElseIf Not String.IsNullOrEmpty(ISOToPatch) Then
            SelectedISOTextBox.Text = ISOToPatch
            PatchISOCheckBox.IsChecked = True
        ElseIf Not String.IsNullOrEmpty(ISOToDecrypt) Then
            SelectedISOTextBox.Text = ISOToDecrypt
            DecryptISOCheckBox.IsChecked = True
        End If
    End Sub

    Private Sub PS3ISOTools_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        If Not String.IsNullOrEmpty(ISOToDecrypt) AndAlso DecryptISOCheckBox.IsChecked Then
            If MsgBox("An ISO file has been selected for decryption. Do you want to retrieve the decryption key automatically ?", MsgBoxStyle.YesNo, "ISO Decryption Key") = MsgBoxResult.Yes Then

                Dim GameTitleID As String = ""
                Using NewISOStream As FileStream = File.Open(ISOToDecrypt, FileMode.Open)
                    Dim NewCDReader As New CDReader(NewISOStream, True)
                    Try
                        Using NewFileStream As Stream = NewCDReader.OpenFile("PS3_GAME\PARAM.SFO", FileMode.Open)
                            Try
                                Dim SFOKeys As Dictionary(Of String, Object) = SFONew.ReadSfo(NewFileStream)
                                If SFOKeys IsNot Nothing AndAlso SFOKeys.Count > 0 Then
                                    Dim TITLEIDValue As Object = Nothing
                                    If SFOKeys.TryGetValue("TITLE_ID", TITLEIDValue) Then
                                        GameTitleID = TITLEIDValue.ToString
                                    End If
                                End If
                            Finally
                                NewFileStream.Close()
                            End Try
                        End Using
                    Catch ex As Exception
                        'No valid PS3 ISO
                        MsgBox("The selected ISO file seems not to have include the required PS3_GAME\PARAM.SFO file and might not be decryptable.", MsgBoxStyle.Information)
                    End Try

                    NewCDReader.Dispose()
                    NewISOStream.Close()
                End Using

                If Not String.IsNullOrEmpty(GameTitleID) Then
                    Dim RetrievedDKey As String = Utils.GetDKeyFromGameID(Environment.CurrentDirectory + "\Tools\dkeydb.html", GameTitleID)
                    If Not String.IsNullOrEmpty(RetrievedDKey) Then
                        ISODecryptionKeyTextBox.Text = RetrievedDKey
                    Else
                        MsgBox($"No decryption key found for {GameTitleID}. You have to input it manually.", MsgBoxStyle.Information)
                    End If
                End If
            End If
        End If
    End Sub

#Region "Browse Buttons"

    Private Sub BrowseBackupFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseBackupFolderButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select a game backup folder"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedGameBackupFolderTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub BrowseISOOutputFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseISOOutputFolderButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select an output folder"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedISOOutputTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub BrowseISOButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseISOButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "ISO files (*.iso)|*.iso", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedISOTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseExtractionFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseExtractionFolderButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select an output folder"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedISOExtractionOutputFolderTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub BrowseISODecryptionFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseISODecryptionFolderButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select an output folder"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedISODecryptionFolderTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub BrowseISODecryptionKeyButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseISODecryptionKeyButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "DKEY files (*.dkey)|*.dkey", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            ISODecryptionKeyTextBox.Text = OFD.FileName
        End If
    End Sub

#End Region

#Region "CheckBox Changes"

    Private Sub SplitISOCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles SplitISOCheckBox.Checked
        Useps3netsrvCheckBox.IsEnabled = False
    End Sub

    Private Sub Useps3netsrvCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles Useps3netsrvCheckBox.Checked
        SplitISOCheckBox.IsEnabled = False
    End Sub

    Private Sub SplitISOCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles SplitISOCheckBox.Unchecked
        Useps3netsrvCheckBox.IsEnabled = True
    End Sub

    Private Sub Useps3netsrvCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles Useps3netsrvCheckBox.Unchecked
        SplitISOCheckBox.IsEnabled = True
    End Sub

    Private Sub PatchISOCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles PatchISOCheckBox.Checked
        ExtractISOCheckBox.IsEnabled = False
        SplitCustomISOCheckBox.IsEnabled = False
        DecryptISOCheckBox.IsEnabled = False

        ModifyISOButton.IsEnabled = True
        ModifyISOButton.Content = "Patch ISO"
    End Sub

    Private Sub ExtractISOCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ExtractISOCheckBox.Checked
        PatchISOCheckBox.IsEnabled = False
        SplitCustomISOCheckBox.IsEnabled = False
        DecryptISOCheckBox.IsEnabled = False

        ModifyISOButton.IsEnabled = True
        ModifyISOButton.Content = "Extract ISO"
    End Sub

    Private Sub SplitCustomISOCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles SplitCustomISOCheckBox.Checked
        PatchISOCheckBox.IsEnabled = False
        ExtractISOCheckBox.IsEnabled = False
        DecryptISOCheckBox.IsEnabled = False

        ModifyISOButton.IsEnabled = True
        ModifyISOButton.Content = "Split ISO"
    End Sub

    Private Sub DecryptISOCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles DecryptISOCheckBox.Checked
        PatchISOCheckBox.IsEnabled = False
        ExtractISOCheckBox.IsEnabled = False
        SplitCustomISOCheckBox.IsEnabled = False

        ModifyISOButton.IsEnabled = True
        ModifyISOButton.Content = "Decrypt ISO"
    End Sub

    Private Sub PatchISOCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles PatchISOCheckBox.Unchecked
        ExtractISOCheckBox.IsEnabled = True
        SplitCustomISOCheckBox.IsEnabled = True
        DecryptISOCheckBox.IsEnabled = True

        ModifyISOButton.IsEnabled = False
        ModifyISOButton.Content = "Modify ISO"
    End Sub

    Private Sub ExtractISOCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ExtractISOCheckBox.Unchecked
        PatchISOCheckBox.IsEnabled = True
        SplitCustomISOCheckBox.IsEnabled = True
        DecryptISOCheckBox.IsEnabled = True

        ModifyISOButton.IsEnabled = False
        ModifyISOButton.Content = "Modify ISO"
    End Sub

    Private Sub SplitCustomISOCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles SplitCustomISOCheckBox.Unchecked
        PatchISOCheckBox.IsEnabled = True
        ExtractISOCheckBox.IsEnabled = True
        DecryptISOCheckBox.IsEnabled = True

        ModifyISOButton.IsEnabled = False
        ModifyISOButton.Content = "Modify ISO"
    End Sub

    Private Sub DecryptISOCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles DecryptISOCheckBox.Unchecked
        PatchISOCheckBox.IsEnabled = True
        ExtractISOCheckBox.IsEnabled = True
        SplitCustomISOCheckBox.IsEnabled = True

        ModifyISOButton.IsEnabled = False
        ModifyISOButton.Content = "Modify ISO"
    End Sub

#End Region

    Private Sub CreateISOButton_Click(sender As Object, e As RoutedEventArgs) Handles CreateISOButton.Click
        If Not Useps3netsrvCheckBox.IsChecked Then
            If Not String.IsNullOrEmpty(SelectedGameBackupFolderTextBox.Text) And Not String.IsNullOrEmpty(SelectedISOOutputTextBox.Text) Then

                Created = False
                Cursor = Input.Cursors.Wait
                IsEnabled = False

                Dim makeps3iso As New Process() With {.EnableRaisingEvents = True}
                Dim makeps3isoStartInfo As New ProcessStartInfo With {
                    .FileName = Environment.CurrentDirectory + "\Tools\makeps3iso.exe",
                    .RedirectStandardOutput = True,
                    .UseShellExecute = False,
                    .CreateNoWindow = True
                }

                If SplitLargeExtractedISOFilesCheckBox.IsChecked Then
                    makeps3isoStartInfo.Arguments = "-s """ + SelectedISOTextBox.Text + """ """ + SelectedISOExtractionOutputFolderTextBox.Text + """"
                Else
                    makeps3isoStartInfo.Arguments = """" + SelectedISOTextBox.Text + """ """ + SelectedISOExtractionOutputFolderTextBox.Text + """"
                End If

                AddHandler makeps3iso.OutputDataReceived, AddressOf Makeps3iso_OutputDataReceived
                AddHandler makeps3iso.Exited, AddressOf Makeps3iso_Exited

                makeps3iso.StartInfo = makeps3isoStartInfo
                makeps3iso.Start()
                makeps3iso.BeginOutputReadLine()
            Else
                MsgBox("No game backup folder or output folder specified, please check your input.", MsgBoxStyle.Critical)
            End If
        Else
            If Not String.IsNullOrEmpty(SelectedGameBackupFolderTextBox.Text) And Not String.IsNullOrEmpty(SelectedISOOutputTextBox.Text) Then

                Cursor = Input.Cursors.Wait
                IsEnabled = False

                PS3NetSrvProcessAction = "CreateISO"
                PS3NetSrvProcess = New Process() With {.EnableRaisingEvents = True}
                PS3NetSrvProcess.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\ps3netsrv\ps3netsrv.exe"
                PS3NetSrvProcess.StartInfo.Arguments = """" + SelectedGameBackupFolderTextBox.Text + """ ISO"
                PS3NetSrvProcess.Start()
                PS3NetSrvProcess.WaitForExit()
            Else
                MsgBox("No ISO file specified, please check your input.", MsgBoxStyle.Critical)
            End If
        End If
    End Sub

    Private Sub ModifyISOButton_Click(sender As Object, e As RoutedEventArgs) Handles ModifyISOButton.Click
        If PatchISOCheckBox.IsChecked Then
            If Not String.IsNullOrEmpty(SelectedISOTextBox.Text) And SelectedPatchVersionComboBox.SelectedItem IsNot Nothing Then

                Patched = False
                Cursor = Input.Cursors.Wait
                IsEnabled = False

                Dim PatchToVersion As String = SelectedPatchVersionComboBox.Text

                Dim patchps3iso As New Process() With {.EnableRaisingEvents = True}
                Dim patchps3isoStartInfo As New ProcessStartInfo With {
                    .FileName = Environment.CurrentDirectory + "\Tools\patchps3iso.exe",
                    .Arguments = $"""{SelectedISOTextBox.Text}"" {PatchToVersion}",
                    .RedirectStandardOutput = True,
                    .UseShellExecute = False,
                    .CreateNoWindow = True
                }

                AddHandler patchps3iso.OutputDataReceived, AddressOf Patchps3iso_OutputDataReceived
                AddHandler patchps3iso.Exited, AddressOf Patchps3iso_Exited


                patchps3iso.StartInfo = patchps3isoStartInfo
                patchps3iso.Start()
                patchps3iso.BeginOutputReadLine()
            Else
                MsgBox("No ISO or Version specified, please check your input.", MsgBoxStyle.Critical)
            End If
        ElseIf ExtractISOCheckBox.IsChecked Then
            If Not String.IsNullOrEmpty(SelectedISOTextBox.Text) And Not String.IsNullOrEmpty(SelectedISOExtractionOutputFolderTextBox.Text) Then

                Extracted = False
                Cursor = Input.Cursors.Wait
                IsEnabled = False

                Dim extractps3iso As New Process() With {.EnableRaisingEvents = True}
                Dim extractps3isoStartInfo As New ProcessStartInfo With {
                    .FileName = Environment.CurrentDirectory + "\Tools\extractps3iso.exe",
                    .RedirectStandardOutput = True,
                    .UseShellExecute = False,
                    .CreateNoWindow = True
                }

                If SplitLargeExtractedISOFilesCheckBox.IsChecked Then
                    extractps3isoStartInfo.Arguments = "-s """ + SelectedISOTextBox.Text + """ """ + SelectedISOExtractionOutputFolderTextBox.Text + """"
                Else
                    extractps3isoStartInfo.Arguments = """" + SelectedISOTextBox.Text + """ """ + SelectedISOExtractionOutputFolderTextBox.Text + """"
                End If

                AddHandler extractps3iso.OutputDataReceived, AddressOf Extractps3iso_OutputDataReceived
                AddHandler extractps3iso.Exited, AddressOf Extractps3iso_Exited

                extractps3iso.StartInfo = extractps3isoStartInfo
                extractps3iso.Start()
                extractps3iso.BeginOutputReadLine()
            Else
                MsgBox("No ISO or output folder specified, please check your input.", MsgBoxStyle.Critical)
            End If
        ElseIf SplitCustomISOCheckBox.IsChecked Then
            If Not String.IsNullOrEmpty(SelectedISOTextBox.Text) Then

                Splitted = False
                Cursor = Input.Cursors.Wait
                IsEnabled = False

                Dim splitps3iso As New Process() With {.EnableRaisingEvents = True}
                Dim splitps3isoStartInfo As New ProcessStartInfo With {
                    .FileName = Environment.CurrentDirectory + "\Tools\splitps3iso.exe",
                    .RedirectStandardOutput = True,
                    .Arguments = """" + SelectedISOTextBox.Text + """",
                    .UseShellExecute = False,
                    .CreateNoWindow = True
                }

                AddHandler splitps3iso.OutputDataReceived, AddressOf Splitps3iso_OutputDataReceived
                AddHandler splitps3iso.Exited, AddressOf Splitps3iso_Exited

                splitps3iso.StartInfo = splitps3isoStartInfo
                splitps3iso.Start()
                splitps3iso.BeginOutputReadLine()
            Else
                MsgBox("No ISO specified, please check your input.", MsgBoxStyle.Critical)
            End If
        ElseIf DecryptISOCheckBox.IsChecked Then
            If Not String.IsNullOrEmpty(SelectedISOTextBox.Text) AndAlso Not String.IsNullOrEmpty(ISODecryptionKeyTextBox.Text) Then

                Decrypted = False
                Cursor = Input.Cursors.Wait
                IsEnabled = False

                Dim DecryptionKey As String
                If ISODecryptionKeyTextBox.Text.EndsWith(".dkey") Then
                    DecryptionKey = File.ReadLines(ISODecryptionKeyTextBox.Text)(0).Trim()
                Else
                    DecryptionKey = ISODecryptionKeyTextBox.Text
                End If

                'Start decryption using ps3dec(remake_cli)
                Dim ps3dec As New Process() With {.EnableRaisingEvents = True}
                Dim ps3decStartInfo As New ProcessStartInfo With {
                    .FileName = Environment.CurrentDirectory + "\Tools\ps3dec.exe",
                    .Arguments = $"--iso ""{SelectedISOTextBox.Text}"" --dk {DecryptionKey} --skip",
                    .RedirectStandardOutput = True,
                    .UseShellExecute = False,
                    .CreateNoWindow = True
                }

                AddHandler ps3dec.OutputDataReceived, AddressOf PS3dec_OutputDataReceived
                AddHandler ps3dec.Exited, AddressOf PS3dec_Exited

                ps3dec.StartInfo = ps3decStartInfo
                ps3dec.Start()
                ps3dec.BeginOutputReadLine()
            Else
                MsgBox("No ISO specified, please check your input.", MsgBoxStyle.Critical)
            End If
        Else
            MsgBox("No action selected.", MsgBoxStyle.Information)
        End If
    End Sub

#Region "Output Data and Exit Handlers"

    Public Sub PS3dec_OutputDataReceived(sender As Object, e As DataReceivedEventArgs)
        If e IsNot Nothing Then
            If Not String.IsNullOrEmpty(e.Data) Then
                If e.Data.Contains("Data written to") Then
                    Decrypted = True
                End If
            End If
        End If
    End Sub

    Private Sub PS3dec_Exited(sender As Object, e As EventArgs)
        If Decrypted Then
            Dispatcher.BeginInvoke(Sub()
                                       Dim InputISOFolderPath As String = Path.GetDirectoryName(SelectedISOTextBox.Text)
                                       Dim NewISODestinationFolderPath As String = ""
                                       Dim OutputISOFileName As String = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(SelectedISOTextBox.Text)) + ".iso_decrypted.iso"
                                       Dim OutputISOFilePath As String = Path.Combine(InputISOFolderPath, OutputISOFileName)
                                       Dim NewISODestinationPath As String = Path.Combine(SelectedISODecryptionFolderTextBox.Text, OutputISOFileName)
                                       NewISODestinationFolderPath = Path.GetDirectoryName(NewISODestinationPath)

                                       'Move decrypted ISO to selected output folder
                                       If Not String.IsNullOrEmpty(SelectedISODecryptionFolderTextBox.Text) Then
                                           If File.Exists(OutputISOFilePath) Then
                                               Try
                                                   File.Move(OutputISOFilePath, NewISODestinationPath)
                                               Catch ex As Exception
                                                   MsgBox("An ISO file with this name already exists in the selected output folder.", MsgBoxStyle.Critical)
                                               End Try
                                           End If
                                       End If

                                       Cursor = Input.Cursors.Arrow
                                       IsEnabled = True

                                       If MsgBox("ISO decrypted! Open output folder ?", MsgBoxStyle.YesNo, "Success") = MsgBoxResult.Yes Then
                                           If Not String.IsNullOrEmpty(NewISODestinationFolderPath) Then
                                               Process.Start("explorer", NewISODestinationFolderPath)
                                           Else
                                               Process.Start("explorer", InputISOFolderPath)
                                           End If
                                       End If
                                   End Sub)
        Else
            Dispatcher.BeginInvoke(Sub()
                                       Cursor = Input.Cursors.Arrow
                                       IsEnabled = True
                                       MsgBox("ISO decryption failed, please check your input.", MsgBoxStyle.Critical)
                                   End Sub)
        End If
    End Sub

    Public Sub Makeps3iso_OutputDataReceived(sender As Object, e As DataReceivedEventArgs)
        If e IsNot Nothing Then
            If Not String.IsNullOrEmpty(e.Data) Then
                If e.Data.Contains("Finish!") Then
                    Created = True
                End If
            End If
        End If
    End Sub

    Private Sub Makeps3iso_Exited(sender As Object, e As EventArgs)
        If Created Then
            Dispatcher.BeginInvoke(Sub()
                                       Cursor = Input.Cursors.Arrow
                                       IsEnabled = True
                                       If MsgBox("ISO created! Open output folder ?", MsgBoxStyle.YesNo, "Success") = MsgBoxResult.Yes Then
                                           Process.Start("explorer", SelectedISOOutputTextBox.Text)
                                       End If
                                   End Sub)
        Else
            Dispatcher.BeginInvoke(Sub()
                                       Cursor = Input.Cursors.Arrow
                                       IsEnabled = True
                                       MsgBox("Could not create an ISO file.", MsgBoxStyle.Critical)
                                   End Sub)
        End If
    End Sub

    Public Sub Extractps3iso_OutputDataReceived(sender As Object, e As DataReceivedEventArgs)
        If e IsNot Nothing Then
            If Not String.IsNullOrEmpty(e.Data) Then
                If e.Data.Contains("Finish!") Then
                    Extracted = True
                End If
            End If
        End If
    End Sub

    Private Sub Extractps3iso_Exited(sender As Object, e As EventArgs)
        If Extracted Then
            Dispatcher.BeginInvoke(Sub()
                                       Cursor = Input.Cursors.Arrow
                                       IsEnabled = True
                                       If MsgBox("ISO extracted! Open output folder ?", MsgBoxStyle.YesNo, "Success") = MsgBoxResult.Yes Then
                                           Process.Start("explorer", SelectedISOExtractionOutputFolderTextBox.Text)
                                       End If
                                   End Sub)
        Else
            Dispatcher.BeginInvoke(Sub()
                                       Cursor = Input.Cursors.Arrow
                                       IsEnabled = True
                                       MsgBox("Could not extract the selected ISO file.", MsgBoxStyle.Critical)
                                   End Sub)
        End If
    End Sub

    Public Sub Splitps3iso_OutputDataReceived(sender As Object, e As DataReceivedEventArgs)
        If e IsNot Nothing Then
            If Not String.IsNullOrEmpty(e.Data) Then
                If e.Data.Contains("Finish!") Then
                    Splitted = True
                End If
            End If
        End If
    End Sub

    Private Sub Splitps3iso_Exited(sender As Object, e As EventArgs)
        If Splitted Then
            Dispatcher.BeginInvoke(Sub()
                                       Cursor = Input.Cursors.Arrow
                                       IsEnabled = True
                                       If MsgBox("ISO splitted! Open output folder ?", MsgBoxStyle.YesNo, "Success") = MsgBoxResult.Yes Then
                                           Process.Start("explorer", Path.GetDirectoryName(SelectedISOTextBox.Text))
                                       End If
                                   End Sub)
        Else
            Dispatcher.BeginInvoke(Sub()
                                       Cursor = Input.Cursors.Arrow
                                       IsEnabled = True
                                       MsgBox("Could not split the selected ISO file.", MsgBoxStyle.Critical)
                                   End Sub)
        End If
    End Sub

    Public Sub Patchps3iso_OutputDataReceived(sender As Object, e As DataReceivedEventArgs)
        If e IsNot Nothing Then
            If Not String.IsNullOrEmpty(e.Data) Then
                If e.Data.Contains("Finish!") Then
                    Patched = True
                End If
            End If
        End If
    End Sub

    Private Sub Patchps3iso_Exited(sender As Object, e As EventArgs)
        If Patched Then
            Dispatcher.BeginInvoke(Sub()
                                       Cursor = Input.Cursors.Arrow
                                       IsEnabled = True
                                       If MsgBox("ISO patched! Open output folder ?", MsgBoxStyle.YesNo, "Success") = MsgBoxResult.Yes Then
                                           Process.Start("explorer", Path.GetDirectoryName(SelectedISOTextBox.Text))
                                       End If
                                   End Sub)
        Else
            Dispatcher.BeginInvoke(Sub()
                                       Cursor = Input.Cursors.Arrow
                                       IsEnabled = True
                                       MsgBox("Could not patch the selected ISO file.", MsgBoxStyle.Critical)
                                   End Sub)
        End If
    End Sub

    Private Sub PS3NetSrvProcess_Exited(sender As Object, e As EventArgs) Handles PS3NetSrvProcess.Exited

        Select Case PS3NetSrvProcessAction
            Case "CreateISO"
                Dim InputFolderName As String = New DirectoryInfo(SelectedGameBackupFolderTextBox.Text).Name

                If File.Exists(Environment.CurrentDirectory + "\Tools\ps3netsrv\" + InputFolderName + ".iso") Then
                    File.Move(Environment.CurrentDirectory + "\Tools\ps3netsrv\" + InputFolderName + ".iso", SelectedISOOutputTextBox.Text + "\" + InputFolderName + ".iso")
                ElseIf File.Exists(Environment.CurrentDirectory + "\" + InputFolderName + ".iso") Then
                    File.Move(Environment.CurrentDirectory + "\" + InputFolderName + ".iso", SelectedISOOutputTextBox.Text + "\" + InputFolderName + ".iso")
                End If

                Dispatcher.BeginInvoke(Sub()
                                           Cursor = Input.Cursors.Arrow
                                           IsEnabled = True

                                           If MsgBox("ISO file created with success!" + vbCrLf + SelectedISOOutputTextBox.Text + "\" + InputFolderName + ".iso" + vbCrLf + "Open output folder ?", MsgBoxStyle.YesNo, "Success") = MsgBoxResult.Yes Then
                                               Process.Start("explorer", Path.GetDirectoryName(SelectedISOOutputTextBox.Text + "\" + InputFolderName + ".iso"))
                                           End If
                                       End Sub)
        End Select

        PS3NetSrvProcess.Dispose()
        PS3NetSrvProcess = Nothing
    End Sub

#End Region

End Class
