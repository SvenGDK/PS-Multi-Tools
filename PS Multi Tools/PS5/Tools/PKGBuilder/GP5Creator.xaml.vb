Imports System.IO
Imports System.Windows.Forms

Public Class GP5Creator

    Private Sub BrowseFileSourcePathButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseFileSourcePathButton.Click
        Dim OFD As New OpenFileDialog() With {.Multiselect = False, .Title = "Select a file."}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            FileFolderSourcePathTextBox.Text = OFD.FileName
            FileFolderDestinationPathTextBox.Text = "\" & Path.GetFileName(OFD.FileName)
        End If
    End Sub

    Private Sub BrowseFolderSourcePathButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseFolderSourcePathButton.Click
        Dim FBD As New FolderBrowserDialog() With {.Description = "Select a folder"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            FileFolderSourcePathTextBox.Text = FBD.SelectedPath
            FileFolderDestinationPathTextBox.Text = "\" & Path.GetDirectoryName(FBD.SelectedPath)
        End If
    End Sub

    Private Sub AddToChunkButton_Click(sender As Object, e As RoutedEventArgs) Handles AddToChunkButton.Click
        If Not String.IsNullOrEmpty(SaveToTextBox.Text) Then
            If Not String.IsNullOrEmpty(FileFolderSourcePathTextBox.Text) Then
                If Not String.IsNullOrEmpty(FileFolderDestinationPathTextBox.Text) Then
                    If Not String.IsNullOrEmpty(SelectedChunkTextBox.Text) Then

                        Dim ChunkLvItem As New Structures.GP5ChunkFilesFolderListViewItem() With {.SourcePath = FileFolderSourcePathTextBox.Text, .DestinationPath = FileFolderDestinationPathTextBox.Text}

                        Try
                            Using PubCMD As New Process()
                                PubCMD.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"

                                If Path.HasExtension(FileFolderSourcePathTextBox.Text) Then
                                    ChunkLvItem.ChunkType = "File"
                                    PubCMD.StartInfo.Arguments = "gp5_file_add --src_path """ + FileFolderSourcePathTextBox.Text + """ --dst_path " + FileFolderDestinationPathTextBox.Text + " --chunk " + SelectedChunkTextBox.Text + " """ + SaveToTextBox.Text + """"
                                Else
                                    ChunkLvItem.ChunkType = "Folder"
                                    PubCMD.StartInfo.Arguments = "gp5_dir_add --src_path """ + FileFolderSourcePathTextBox.Text + """ --dst_path " + FileFolderDestinationPathTextBox.Text + " --chunk " + SelectedChunkTextBox.Text + " """ + SaveToTextBox.Text + """"
                                End If

                                PubCMD.StartInfo.UseShellExecute = False
                                PubCMD.StartInfo.CreateNoWindow = True
                                PubCMD.Start()
                            End Using

                            ChunkFilesFolderListView.Items.Add(ChunkLvItem)

                            MsgBox(FileFolderSourcePathTextBox.Text + " added.", MsgBoxStyle.Information, "Project updated")
                        Catch ex As Exception
                            MsgBox("Could not add to gp5 project.", MsgBoxStyle.Critical, "Error")
                            MsgBox(ex.Message)
                        End Try

                    Else
                        MsgBox("No chunk set.", MsgBoxStyle.Exclamation)
                    End If
                Else
                    MsgBox("No destination path set.", MsgBoxStyle.Exclamation)
                End If
            Else
                MsgBox("No source path selected.", MsgBoxStyle.Exclamation)
            End If
        Else
            MsgBox("No project save path selected.", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub NewGP5ProjectMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles NewGP5ProjectMenuItem.Click
        Try
            Dim SFD As New SaveFileDialog() With {.Title = "Select a save destination your GP5 project.", .Filter = "GP5 Files (*.gp5)|*.gp5", .DefaultExt = ".gp5", .AddExtension = True, .SupportMultiDottedExtensions = False}
            If SFD.ShowDialog() = Forms.DialogResult.OK Then
                SaveToTextBox.Text = SFD.FileName
            End If

            Dim DateAndTime As String = Date.Now.ToString("yyyy-MM-dd")
            Using PubCMD As New Process()
                PubCMD.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                PubCMD.StartInfo.Arguments = "gp5_proj_create --volume_type prospero_app --passcode " + PasscodeTextBox.Text + " --c_date " + DateAndTime + " """ + SFD.FileName + """"
                PubCMD.StartInfo.UseShellExecute = False
                PubCMD.StartInfo.CreateNoWindow = True
                PubCMD.Start()
                PubCMD.WaitForExit()
            End Using

            MsgBox("New gp5 project created at " + SaveToTextBox.Text, MsgBoxStyle.Information)
        Catch ex As Exception
            MsgBox("Could not create a gp5 project.", MsgBoxStyle.Critical, "Error")
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub LoadGP5ProjectMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadGP5ProjectMenuItem.Click
        Dim SFD As New SaveFileDialog() With {.Title = "Save your project.", .Filter = "GP5 Files (*.gp5)|*.gp5", .DefaultExt = ".gp5", .AddExtension = True, .SupportMultiDottedExtensions = False}
        If SFD.ShowDialog() = Forms.DialogResult.OK Then
            SaveToTextBox.Text = SFD.FileName
        End If
    End Sub

#Region "Quick Tools"

    Private Sub CreateParamButton_Click(sender As Object, e As RoutedEventArgs) Handles CreateParamButton.Click
        Dim NewParamEditor As New PS5ParamEditor() With {.ShowActivated = True}
        NewParamEditor.Show()
    End Sub

    Private Sub CreateManifestButton_Click(sender As Object, e As RoutedEventArgs) Handles CreateManifestButton.Click
        Dim NewManifestEditor As New PS5ManifestEditor() With {.ShowActivated = True}
        NewManifestEditor.Show()
    End Sub

    Private Sub BuildPKGButton_Click(sender As Object, e As RoutedEventArgs) Handles BuildPKGButton.Click
        Dim NewPKGBuilder As New PS5PKGBuilder() With {.ShowActivated = True}
        If Not String.IsNullOrEmpty(SaveToTextBox.Text) Then
            NewPKGBuilder.SelectedProjectTextBox.Text = SaveToTextBox.Text
        End If
        NewPKGBuilder.Show()
    End Sub

#End Region

End Class
