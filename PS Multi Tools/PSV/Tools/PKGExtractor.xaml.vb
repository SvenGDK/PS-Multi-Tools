Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Windows.Forms

Public Class PKGExtractor

    Dim WithEvents PKG2ZIP As New Process()
    Dim WithEvents PKG2ZIPWoker As New BackgroundWorker()

    Dim DownloadsList As New List(Of Structures.Package)()
    Dim SelectedPKGContentID As String

    Private Sub BrowsePKGButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePKGButton.Click
        Dim OFD As New OpenFileDialog With {.Multiselect = False, .Filter = "", .CheckFileExists = True}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPKGTextBox.Text = OFD.FileName

            Using SFOReader As New Process()
                SFOReader.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\PSN_get_pkg_info.exe"
                SFOReader.StartInfo.Arguments = """" + OFD.FileName + """"
                SFOReader.StartInfo.RedirectStandardOutput = True
                SFOReader.StartInfo.UseShellExecute = False
                SFOReader.StartInfo.CreateNoWindow = True
                SFOReader.Start()

                Dim OutputReader As StreamReader = SFOReader.StandardOutput
                Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

                If ProcessOutput.Length > 0 Then
                    'Load game infos
                    For Each Line In ProcessOutput
                        If Line.StartsWith("Content ID:") Then
                            SelectedPKGContentID = Line.Split(":"c)(1).Trim(""""c).Trim()
                            Exit For
                        End If
                    Next
                End If
            End Using
        End If
    End Sub

    Private Sub BrowseOutputFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseOutputFolderButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .ShowNewFolderButton = True}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            OutputFolderTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Async Sub GetzRIFKeyButton_Click(sender As Object, e As RoutedEventArgs) Handles GetzRIFKeyButton.Click
        If MsgBox("Load from the latest database ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            Using NewWebClient As New WebClient
                Dim GamesList As String = Await NewWebClient.DownloadStringTaskAsync(New Uri("https://nopaystation.com/tsv/PSV_GAMES.tsv"))
                Dim GamesListLines As String() = GamesList.Split(CChar(vbCrLf))
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

    Private Sub ExtractButton_Click(sender As Object, e As RoutedEventArgs) Handles ExtractButton.Click
        If Not String.IsNullOrEmpty(SelectedPKGTextBox.Text) Then
            If Not String.IsNullOrEmpty(OutputFolderTextBox.Text) Then
                If Not String.IsNullOrEmpty(zRIFTextBox.Text) Then
                    If File.Exists(SelectedPKGTextBox.Text) And Directory.Exists(OutputFolderTextBox.Text) Then
                        Dim PKGPath As String = SelectedPKGTextBox.Text
                        Dim zRIFKey As String = zRIFTextBox.Text

                        PKG2ZIPWoker.RunWorkerAsync("-x " + """" + PKGPath + """ " + """" + zRIFKey + """")
                    Else
                        MsgBox("Could not find " + SelectedPKGTextBox.Text, MsgBoxStyle.Critical, "Error")
                    End If
                Else
                    MsgBox("No zRIF key specified.", MsgBoxStyle.Critical, "Error")
                End If
            Else
                MsgBox("No output folder specified.", MsgBoxStyle.Critical, "Error")
            End If
        Else
            MsgBox("No PKG file specified.", MsgBoxStyle.Critical, "Error")
        End If
    End Sub

    Private Sub PKG2ZIPWoker_DoWork(sender As Object, e As DoWorkEventArgs) Handles PKG2ZIPWoker.DoWork
        'Set PKG2ZIP process properties
        PKG2ZIP = New Process()
        PKG2ZIP.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\pkg2zip.exe"
        PKG2ZIP.StartInfo.Arguments = e.Argument.ToString()
        PKG2ZIP.StartInfo.RedirectStandardOutput = True
        PKG2ZIP.StartInfo.RedirectStandardError = True
        PKG2ZIP.StartInfo.UseShellExecute = False
        PKG2ZIP.StartInfo.CreateNoWindow = True
        PKG2ZIP.EnableRaisingEvents = True

        AddHandler PKG2ZIP.OutputDataReceived, Sub(SenderProcess As Object, DataArgs As DataReceivedEventArgs)
                                                   If Not String.IsNullOrEmpty(DataArgs.Data) Then
                                                       'Append output log from PKG2ZIP
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

        AddHandler PKG2ZIP.ErrorDataReceived, Sub(SenderProcess As Object, DataArgs As DataReceivedEventArgs)
                                                  If Not String.IsNullOrEmpty(DataArgs.Data) Then
                                                      'Append error log from PKG2ZIP
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

        'Start PKG2ZIP & read process output data
        PKG2ZIP.Start()
        PKG2ZIP.BeginOutputReadLine()
        PKG2ZIP.BeginErrorReadLine()
    End Sub

    Private Sub PKG2ZIPWoker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles PKG2ZIPWoker.RunWorkerCompleted
        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub()
                                       If LogTextBox.Text.Contains("done!") Then
                                           If MsgBox("PKG extracted! Do you want to open the folder containing the extracted folder ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                                               If Not String.IsNullOrEmpty(OutputFolderTextBox.Text) AndAlso Directory.Exists(OutputFolderTextBox.Text) Then
                                                   Process.Start("explorer", OutputFolderTextBox.Text)
                                               End If
                                           End If
                                       Else
                                           MsgBox("Could not extract the selected .pkg file.", MsgBoxStyle.Critical)
                                       End If
                                   End Sub)
        Else
            If LogTextBox.Text.Contains("done!") Then
                If MsgBox("PKG extracted! Do you want to open the folder containing the extracted folder ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                    If Not String.IsNullOrEmpty(OutputFolderTextBox.Text) AndAlso Directory.Exists(OutputFolderTextBox.Text) Then
                        Process.Start("explorer", OutputFolderTextBox.Text)
                    End If
                End If
            Else
                MsgBox("Could not extract the selected .pkg file.", MsgBoxStyle.Critical)
            End If
        End If
    End Sub

    Private Sub PKG2ZIP_Exited(sender As Object, e As EventArgs) Handles PKG2ZIP.Exited
        PKG2ZIP.Dispose()
    End Sub

End Class
