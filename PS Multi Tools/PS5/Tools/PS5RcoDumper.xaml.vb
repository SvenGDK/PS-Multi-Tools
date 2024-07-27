Imports System.IO
Imports System.Security.Authentication
Imports System.Windows.Forms
Imports FluentFTP

Public Class PS5RcoDumper

    Public ConsoleIP As String

    Private Sub BrowseButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select a folder where you want to dump the rco files"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SaveToTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub LoadFilesButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadFilesButton.Click
        If Not String.IsNullOrEmpty(SaveToTextBox.Text) Then
            If Not String.IsNullOrEmpty(ConsoleIP) Then
                If Directory.Exists(SaveToTextBox.Text) Then

                    Dim SavePath As String = SaveToTextBox.Text
                    Cursor = Input.Cursors.Wait

                    Try
                        Using conn As New FtpClient(ConsoleIP, "anonymous", "anonymous", 1337)
                            'Configurate the FTP connection
                            conn.Config.EncryptionMode = FtpEncryptionMode.None
                            conn.Config.SslProtocols = SslProtocols.None
                            conn.Config.DataConnectionEncryption = False

                            'Connect
                            conn.Connect()

                            'Enumerate .rco files in /system_ex/vsh_asset and download them
                            For Each FileInFTP In conn.GetListing("/system_ex/vsh_asset")
                                If FileInFTP.Name.EndsWith(".rco") Then
                                    conn.DownloadFile(SavePath + "\" + FileInFTP.Name, FileInFTP.FullName, FtpLocalExists.Overwrite)
                                End If
                            Next

                            'Disconnect
                            conn.Disconnect()

                            Cursor = Input.Cursors.Arrow

                            MsgBox("Files have been saved at " + SaveToTextBox.Text + "." + vbCrLf + "To extract them, please use the PS5 RCO Extractor.", MsgBoxStyle.Information, "Success")
                        End Using
                    Catch ex As Exception
                        MsgBox("Could not dump .rco files, please verify your connection.", MsgBoxStyle.Exclamation)
                        Cursor = Input.Cursors.Arrow
                    End Try

                End If
            Else
                MsgBox("Please enter your IP and port in the settings before continuing.", MsgBoxStyle.Exclamation, "Error")
            End If
        Else
            MsgBox("Please select a save directory before continuing.", MsgBoxStyle.Exclamation, "Error")
        End If
    End Sub

End Class
