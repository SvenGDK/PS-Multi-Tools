Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Windows.Forms
Imports FluentFTP

Public Class PSVPFSTools

    Dim WithEvents PSVPFSParser As New Process()

    Dim DownloadsList As New List(Of Structures.Package)()
    Dim SelectedPKGContentID As String

    Private Sub BrowseFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseFolderButton.Click
        Dim FBD As New FolderBrowserDialog()
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedFolderTextBox.Text = FBD.SelectedPath

            'Check if \sce_sys\param.sfo exists to get the CONTENT ID
            If File.Exists(FBD.SelectedPath + "\sce_sys\param.sfo") Then
                Dim ParamSFOFilePath As String = FBD.SelectedPath + "\sce_sys\param.sfo"

                Using SFOReader As New Process()
                    SFOReader.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\sfo.exe"
                    SFOReader.StartInfo.Arguments = """" + ParamSFOFilePath + """"
                    SFOReader.StartInfo.RedirectStandardOutput = True
                    SFOReader.StartInfo.UseShellExecute = False
                    SFOReader.StartInfo.CreateNoWindow = True
                    SFOReader.Start()

                    Dim OutputReader As StreamReader = SFOReader.StandardOutput
                    Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
                    If ProcessOutput.Length > 0 Then

                        'Load game infos
                        For Each Line In ProcessOutput
                            If Line.StartsWith("CONTENT_ID=") Then
                                SelectedPKGContentID = Line.Split("="c)(1).Trim(""""c).Trim()
                                Exit For
                            End If
                        Next
                    End If
                End Using
            End If

        End If
    End Sub

    Private Sub BrowseOutputFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseOutputFolderButton.Click
        Dim FBD As New FolderBrowserDialog() With {.ShowNewFolderButton = True}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            OutputFolderTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Async Sub GetzRIFKeyButton_Click(sender As Object, e As RoutedEventArgs) Handles GetzRIFKeyButton.Click
        If MsgBox("Load from the latest database ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            Using NewWebClient As New HttpClient()
                Dim GamesList As String = Await NewWebClient.GetStringAsync("https://nopaystation.com/tsv/PSV_GAMES.tsv")
                Dim GamesListLines As String() = GamesList.Split(New String() {Environment.NewLine}, StringSplitOptions.None)

                For Each GameLine As String In GamesListLines.Skip(1)
                    Dim SplittedValues As String() = GameLine.Split(CChar(vbTab))
                    Dim AdditionalInfo As Structures.PackageInfo = Utils.GetFileSizeAndDate(SplittedValues(8).Trim(), SplittedValues(6).Trim())
                    Dim NewPackage As New Structures.Package() With {.PackageName = SplittedValues(2).Trim(),
                        .PackageURL = SplittedValues(3).Trim(),
                        .PackageTitleID = SplittedValues(0).Trim(),
                        .PackageContentID = SplittedValues(5).Trim(),
                        .PackagezRIF = SplittedValues(4).Trim(),
                        .PackageDate = AdditionalInfo.FileDate,
                        .PackageSize = AdditionalInfo.FileSize,
                        .PackageRegion = SplittedValues(1).Trim()}
                    If Not SplittedValues(3).Trim() = "MISSING" Then 'Only add available PKGs
                        DownloadsList.Add(NewPackage)
                    End If
                Next
            End Using
        Else 'Use local .tsv file
            If File.Exists(Environment.CurrentDirectory + "\Databases\PSV_GAMES.tsv") Then
                Dim FileReader As String() = File.ReadAllLines(Environment.CurrentDirectory + "\Databases\PSV_GAMES.tsv", Text.Encoding.UTF8)
                For Each GameLine As String In FileReader.Skip(1) 'Skip 1st line in TSV
                    Dim SplittedValues As String() = GameLine.Split(CChar(vbTab))
                    Dim AdditionalInfo As Structures.PackageInfo = Utils.GetFileSizeAndDate(SplittedValues(8), SplittedValues(6))
                    Dim NewPackage As New Structures.Package() With {.PackageName = SplittedValues(2),
                        .PackageURL = SplittedValues(3),
                        .PackageTitleID = SplittedValues(0),
                        .PackageContentID = SplittedValues(5),
                        .PackagezRIF = SplittedValues(4),
                        .PackageDate = AdditionalInfo.FileDate,
                        .PackageSize = AdditionalInfo.FileSize,
                        .PackageRegion = SplittedValues(1)}
                    If Not SplittedValues(3) = "MISSING" Then 'Only add available PKGs
                        DownloadsList.Add(NewPackage)
                    End If
                Next
            Else
                MsgBox("Nothing available. Please add TSV files to the 'Databases' directory.", MsgBoxStyle.Exclamation, "Could not load list")
            End If
        End If

        'Check if we have a zRIF for the selected .pkg
        For Each AvailablePKG As Structures.Package In DownloadsList
            If AvailablePKG.PackageContentID = SelectedPKGContentID Then
                If AvailablePKG.PackagezRIF IsNot Nothing Then
                    zRIFTextBox.Text = AvailablePKG.PackagezRIF
                End If
            End If
        Next
    End Sub

    Private Async Sub ExtractButton_Click(sender As Object, e As RoutedEventArgs) Handles ExtractButton.Click
        If Not String.IsNullOrEmpty(SelectedFolderTextBox.Text) AndAlso Directory.Exists(SelectedFolderTextBox.Text) Then
            If Not String.IsNullOrEmpty(OutputFolderTextBox.Text) Then
                If Not String.IsNullOrEmpty(zRIFTextBox.Text) Then

                    If Await Utils.IsURLValid("http://cma.henkaku.xyz/") Then
                        Dim InputPath As String = SelectedFolderTextBox.Text
                        Dim OutputPath As String = OutputFolderTextBox.Text
                        Dim zRIFKey As String = zRIFTextBox.Text

                        Cursor = Input.Cursors.Wait

                        'Set PSVPFSParser process properties
                        PSVPFSParser = New Process()
                        PSVPFSParser.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\psvpfsparser.exe"
                        PSVPFSParser.StartInfo.Arguments = "-i """ + InputPath + """ -o """ + OutputPath + """ -z " + zRIFKey + " -f cma.henkaku.xyz"
                        PSVPFSParser.StartInfo.RedirectStandardOutput = True
                        PSVPFSParser.StartInfo.RedirectStandardError = True
                        PSVPFSParser.StartInfo.UseShellExecute = False
                        PSVPFSParser.StartInfo.CreateNoWindow = True
                        PSVPFSParser.EnableRaisingEvents = True

                        AddHandler PSVPFSParser.OutputDataReceived, Sub(SenderProcess As Object, DataArgs As DataReceivedEventArgs)
                                                                        If Not String.IsNullOrEmpty(DataArgs.Data) Then
                                                                            'Append output log from PSVPFSParser
                                                                            If Dispatcher.CheckAccess() = False Then
                                                                                Dispatcher.BeginInvoke(Sub()
                                                                                                           LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                                                                           LogTextBox.ScrollToEnd()
                                                                                                       End Sub)
                                                                            Else
                                                                                LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                                                LogTextBox.ScrollToEnd()
                                                                            End If
                                                                        End If
                                                                    End Sub

                        AddHandler PSVPFSParser.ErrorDataReceived, Sub(SenderProcess As Object, DataArgs As DataReceivedEventArgs)
                                                                       If Not String.IsNullOrEmpty(DataArgs.Data) Then
                                                                           'Append error log from PSVPFSParser
                                                                           If Dispatcher.CheckAccess() = False Then
                                                                               Dispatcher.BeginInvoke(Sub()
                                                                                                          LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                                                                          LogTextBox.ScrollToEnd()
                                                                                                      End Sub)
                                                                           Else
                                                                               LogTextBox.AppendText(DataArgs.Data & vbCrLf)
                                                                               LogTextBox.ScrollToEnd()
                                                                           End If
                                                                       End If
                                                                   End Sub

                        'Start PSVPFSParser & read process output data
                        PSVPFSParser.Start()
                        PSVPFSParser.BeginOutputReadLine()
                        PSVPFSParser.BeginErrorReadLine()

                    Else
                        MsgBox("F00D service not available.", MsgBoxStyle.Critical, "Error")
                    End If

                Else
                    MsgBox("No zRIF key specified.", MsgBoxStyle.Critical, "Error")
                End If
            Else
                MsgBox("No output folder specified.", MsgBoxStyle.Critical, "Error")
            End If
        Else
            MsgBox("No input folder specified or folder does not exist.", MsgBoxStyle.Critical, "Error")
        End If
    End Sub

    Private Sub PSVPFSParser_Exited(sender As Object, e As EventArgs) Handles PSVPFSParser.Exited
        PSVPFSParser.Dispose()

        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub()
                                       Cursor = Input.Cursors.Arrow
                                       If LogTextBox.Text.Contains("keystone: matched retail hmac") Then
                                           If MsgBox("Decryption done! Do you want to open the decrypted folder ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                                               If Not String.IsNullOrEmpty(OutputFolderTextBox.Text) AndAlso Directory.Exists(OutputFolderTextBox.Text) Then
                                                   Process.Start("explorer", OutputFolderTextBox.Text)
                                               End If
                                           End If
                                       Else
                                           MsgBox("Could not decrypt the selected input folder.", MsgBoxStyle.Critical)
                                       End If
                                   End Sub)
        Else
            Cursor = Input.Cursors.Arrow
            If LogTextBox.Text.Contains("keystone: matched retail hmac") Then
                If MsgBox("Decryption done! Do you want to open the decrypted folder ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                    If Not String.IsNullOrEmpty(OutputFolderTextBox.Text) AndAlso Directory.Exists(OutputFolderTextBox.Text) Then
                        Process.Start("explorer", OutputFolderTextBox.Text)
                    End If
                End If
            Else
                MsgBox("Could not decrypted the selected input folder.", MsgBoxStyle.Critical)
            End If
        End If
    End Sub

End Class
