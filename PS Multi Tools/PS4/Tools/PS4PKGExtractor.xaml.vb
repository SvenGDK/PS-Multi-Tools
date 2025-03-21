Imports System.IO
Imports System.Threading
Imports System.Windows.Forms
Imports PS4_Tools

Public Class PS4PKGExtractor

    Public PKGToExtract As String = ""

    Dim WithEvents OrbisPubCMD As New Process()

    Private Sub PS4PKGExtractor_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        If Not String.IsNullOrEmpty(PKGToExtract) Then
            SelectedPKGFileTextBox.Text = PKGToExtract

            'Load PKG infos
            Dim PS4PKGInfo As PKG.SceneRelated.Unprotected_PKG = PKG.SceneRelated.Read_PKG(PKGToExtract)
            If PS4PKGInfo IsNot Nothing Then

                PKGTitleTextBlock.Text = "Title : " + PS4PKGInfo.PS4_Title
                IDTextBlock.Text = "ID : " + PS4PKGInfo.Content_ID

                If PS4PKGInfo.Param IsNot Nothing Then
                    If Not String.IsNullOrEmpty(PS4PKGInfo.Param.Category) Then
                        TypeTextBlock.Text = "Type : " + GetPS4Category(PS4PKGInfo.Param.Category)
                    End If
                End If

                If PS4PKGInfo.Icon IsNot Nothing Then
                    Dispatcher.BeginInvoke(Sub() PKGICONImage.Source = Utils.BitmapSourceFromByteArray(PS4PKGInfo.Icon))
                End If

            End If
        End If
    End Sub

    Private Sub BrowsePKGButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePKGButton.Click
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Select a PKG file", .Filter = "PKG files (*.pkg)|*.pkg", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPKGFileTextBox.Text = OFD.FileName

            'Load PKG infos
            Dim PS4PKGInfo As PKG.SceneRelated.Unprotected_PKG = PKG.SceneRelated.Read_PKG(OFD.FileName)
            If PS4PKGInfo IsNot Nothing Then

                PKGTitleTextBlock.Text = "Title : " + PS4PKGInfo.PS4_Title
                IDTextBlock.Text = "ID : " + PS4PKGInfo.Content_ID

                If PS4PKGInfo.Param IsNot Nothing Then
                    If Not String.IsNullOrEmpty(PS4PKGInfo.Param.Category) Then
                        TypeTextBlock.Text = "Type : " + GetPS4Category(PS4PKGInfo.Param.Category)
                    End If
                End If

                If PS4PKGInfo.Icon IsNot Nothing Then
                    Dispatcher.BeginInvoke(Sub() PKGICONImage.Source = Utils.BitmapSourceFromByteArray(PS4PKGInfo.Icon))
                End If

            End If

        End If
    End Sub

    Private Sub BrowseOutputFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseOutputFolderButton.Click
        Dim FBD As New FolderBrowserDialog() With {.ShowNewFolderButton = True}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedOutputFolderTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub ExtractButton_Click(sender As Object, e As RoutedEventArgs) Handles ExtractButton.Click
        If Not String.IsNullOrEmpty(SelectedPKGFileTextBox.Text) AndAlso File.Exists(SelectedPKGFileTextBox.Text) Then

            If Not String.IsNullOrEmpty(SelectedOutputFolderTextBox.Text) AndAlso Directory.Exists(SelectedOutputFolderTextBox.Text) Then

                Cursor = Input.Cursors.Wait
                Dispatcher.BeginInvoke(Sub() LogTextBox.AppendText("PKG Extraction started. Please wait ..." & vbCrLf))
                Thread.Sleep(200)

                Dim Args As String = ""
                If Not String.IsNullOrEmpty(PKGPasscodeTextBox.Text) Then
                    Args = "img_extract --passcode " + PKGPasscodeTextBox.Text + " """ + SelectedPKGFileTextBox.Text + """ """ + SelectedOutputFolderTextBox.Text + """"
                Else
                    Args = "img_extract --passcode 00000000000000000000000000000000 """ + SelectedPKGFileTextBox.Text + """ """ + SelectedOutputFolderTextBox.Text + """"
                End If

                OrbisPubCMD = New Process()
                OrbisPubCMD.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\PS4\orbis-pub-cmd.exe"
                OrbisPubCMD.StartInfo.Arguments = Args
                OrbisPubCMD.StartInfo.RedirectStandardOutput = True
                OrbisPubCMD.StartInfo.RedirectStandardError = True
                OrbisPubCMD.StartInfo.UseShellExecute = False
                OrbisPubCMD.StartInfo.CreateNoWindow = True
                OrbisPubCMD.EnableRaisingEvents = True

                AddHandler OrbisPubCMD.OutputDataReceived, Sub(SenderProcess As Object, DataArgs As DataReceivedEventArgs)
                                                               If Not String.IsNullOrEmpty(DataArgs.Data) Then
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

                AddHandler OrbisPubCMD.ErrorDataReceived, Sub(SenderProcess As Object, DataArgs As DataReceivedEventArgs)
                                                              If Not String.IsNullOrEmpty(DataArgs.Data) Then
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

                OrbisPubCMD.Start()
                OrbisPubCMD.BeginOutputReadLine()
                OrbisPubCMD.BeginErrorReadLine()

            Else
                MsgBox("No output directory specified or selected directory does not exist.", MsgBoxStyle.Critical, "Error")
            End If

        Else
            MsgBox("No PKG file specified or pkg does not exist.", MsgBoxStyle.Critical, "Error")
        End If
    End Sub

    Private Sub OrbisPubCMD_Exited(sender As Object, e As EventArgs) Handles OrbisPubCMD.Exited
        OrbisPubCMD.Dispose()

        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub()
                                       Cursor = Input.Cursors.Arrow
                                       If MsgBox("Done! Do you want to open the output folder ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                                           If Not String.IsNullOrEmpty(SelectedOutputFolderTextBox.Text) AndAlso Directory.Exists(SelectedOutputFolderTextBox.Text) Then
                                               Process.Start("explorer", SelectedOutputFolderTextBox.Text)
                                           End If
                                       End If
                                   End Sub)
        Else
            Cursor = Input.Cursors.Arrow
            If MsgBox("Done! Do you want to open the output folder ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                If Not String.IsNullOrEmpty(SelectedOutputFolderTextBox.Text) AndAlso Directory.Exists(SelectedOutputFolderTextBox.Text) Then
                    Process.Start("explorer", SelectedOutputFolderTextBox.Text)
                End If
            End If
        End If
    End Sub

    Public Shared Function GetPS4Category(SFOCategory As String) As String
        Select Case SFOCategory
            Case "ac"
                Return "Additional Content"
            Case "bd"
                Return "Blu-ray Disc"
            Case "gc"
                Return "Game Content"
            Case "gd"
                Return "Game Digital Application"
            Case "gda"
                Return "System Application"
            Case "gdb"
                Return "Unknown"
            Case "gdc"
                Return "Non-Game Big Application"
            Case "gdd"
                Return "BG Application"
            Case "gde"
                Return "Non-Game Mini App / Video Service Native App"
            Case "gdk"
                Return "Video Service Web App"
            Case "gdl"
                Return "PS Cloud Beta App"
            Case "gdO"
                Return "PS2 Classic"
            Case "gp"
                Return "Game Application Patch"
            Case "gpc"
                Return "Non-Game Big App Patch"
            Case "gpd"
                Return "BG Application patch"
            Case "gpe"
                Return "Non-Game Mini App Patch / Video Service Native App Patch"
            Case "gpk"
                Return "Video Service Web App Patch"
            Case "gpl"
                Return "PS Cloud Beta App Patch"
            Case "sd"
                Return "Save Data"
            Case "la"
                Return "Live Area"
            Case "wda"
                Return "Unknown"
            Case Else
                Return "Unknown"
        End Select
    End Function

End Class
