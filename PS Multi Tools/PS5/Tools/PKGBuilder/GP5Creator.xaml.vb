Imports System.IO
Imports System.Windows.Forms

Public Class GP5Creator

    Dim PubToolsPath As String = Nothing
    'Dim ListOfChunks As New Dictionary(Of String, Structures.GP5ChunkFilesFolderListViewItem)()

    Private Sub GP5Creator_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Set the path of the pub tools
        If File.Exists(My.Computer.FileSystem.SpecialDirectories.ProgramFiles + "\SCE\Prospero\Tools\Publishing Tools\bin\prospero-pub-cmd.exe") Then
            PubToolsPath = My.Computer.FileSystem.SpecialDirectories.ProgramFiles + "\SCE\Prospero\Tools\Publishing Tools\bin\prospero-pub-cmd.exe"
            PubToolsFoundTextBlock.Text = PubToolsPath
            PubToolsFoundTextBlock.Foreground = Brushes.Green
        ElseIf File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") Then
            PubToolsPath = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
            PubToolsFoundTextBlock.Text = PubToolsPath
            PubToolsFoundTextBlock.Foreground = Brushes.Green
        Else
            MsgBox("Could not find any publishing tools." + vbCrLf + "Please add them inside the Tools\PS5 folder inside PS Multi Toools.", MsgBoxStyle.Information, "Pub Tools not available")
            IsEnabled = False
        End If
    End Sub

    Private Sub BrowseSavePathButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseSavePathButton.Click
        Dim SFD As New SaveFileDialog() With {.Title = "Save your project.", .Filter = "GP5 Files (*.gp5)|*.gp5", .DefaultExt = ".gp5", .AddExtension = True, .SupportMultiDottedExtensions = False}
        If SFD.ShowDialog() = Forms.DialogResult.OK Then
            SaveToTextBox.Text = SFD.FileName
        End If
    End Sub

    Private Sub CreateParamButton_Click(sender As Object, e As RoutedEventArgs) Handles CreateParamButton.Click
        Dim NewParamEditor As New PS5ParamEditor() With {.ShowActivated = True}
        NewParamEditor.Show()
    End Sub

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

    Private Sub BrowseFileSourcePathButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseFileSourcePathButton.Click
        Dim OFD As New OpenFileDialog() With {.Multiselect = False, .Title = "Select a file."}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            FileFolderSourcePathTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseFolderSourcePathButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseFolderSourcePathButton.Click
        Dim FBD As New FolderBrowserDialog() With {.Description = "Select a folder"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            FileFolderSourcePathTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub CreateProjectButton_Click(sender As Object, e As RoutedEventArgs) Handles CreateProjectButton.Click
        Try
            Dim DateAndTime As String = Date.Now.ToString("yyyy-MM-dd")

            Using PubCMD As New Process()
                PubCMD.StartInfo.FileName = PubToolsPath
                PubCMD.StartInfo.Arguments = "gp5_proj_create --volume_type prospero_app --passcode " + PasscodeTextBox.Text + " --c_date " + DateAndTime + " """ + SaveToTextBox.Text + """"
                PubCMD.StartInfo.UseShellExecute = False
                PubCMD.StartInfo.CreateNoWindow = True
                PubCMD.Start()
            End Using

            MsgBox("New gp5 project created at " + SaveToTextBox.Text, MsgBoxStyle.Information)
        Catch ex As Exception
            MsgBox("Could not create a gp5 project.", MsgBoxStyle.Critical, "Error")
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub AddToChunkButton_Click(sender As Object, e As RoutedEventArgs) Handles AddToChunkButton.Click
        If Not String.IsNullOrEmpty(SaveToTextBox.Text) Then
            If Not String.IsNullOrEmpty(FileFolderSourcePathTextBox.Text) Then
                If Not String.IsNullOrEmpty(FileFolderDestinationPathTextBox.Text) Then
                    If Not String.IsNullOrEmpty(SelectedChunkTextBox.Text) Then

                        Dim ChunkLvItem As New Structures.GP5ChunkFilesFolderListViewItem() With {.SourcePath = FileFolderSourcePathTextBox.Text, .DestinationPath = FileFolderDestinationPathTextBox.Text}

                        Try
                            Using PubCMD As New Process()
                                PubCMD.StartInfo.FileName = PubToolsPath

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
                            'ListOfChunks.Add(SelectedChunkTextBox.Text, ChunkLvItem)

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

    'Private Sub ChunksComboBox_SelectionChanged(sender As Object, e As Controls.SelectionChangedEventArgs) Handles ChunksComboBox.SelectionChanged
    '    If ChunksComboBox.SelectedItem IsNot Nothing Then

    '        If Not ListOfChunks.Count = 0 Then
    '            ChunkFilesFolderListView.Items.Clear()

    '            For Each KVP As KeyValuePair(Of String, Structures.GP5ChunkFilesFolderListViewItem) In ListOfChunks
    '                If KVP.Key = ChunksComboBox.Text Then
    '                    ChunkFilesFolderListView.Items.Add(New Structures.GP5ChunkFilesFolderListViewItem() With {.ChunkType = KVP.Value.ChunkType, .SourcePath = KVP.Value.SourcePath, .DestinationPath = KVP.Value.DestinationPath})
    '                End If
    '            Next
    '        End If

    '    End If
    'End Sub

    Private Sub ExtractButton_Click(sender As Object, e As RoutedEventArgs) Handles ExtractButton.Click
        If Not String.IsNullOrEmpty(FileToExtractTextBox.Text) Then
            If Not String.IsNullOrEmpty(ExtractPasscodeTextBox.Text) Then
                If Not String.IsNullOrEmpty(ExtractToTextBox.Text) Then

                    Try
                        Using PubCMD As New Process()
                            PubCMD.StartInfo.FileName = PubToolsPath
                            PubCMD.StartInfo.Arguments = "img_extract --passcode " + ExtractPasscodeTextBox.Text + " """ + FileToExtractTextBox.Text + """ """ + ExtractToTextBox.Text + """"
                            PubCMD.StartInfo.UseShellExecute = False
                            PubCMD.StartInfo.CreateNoWindow = True
                            PubCMD.Start()
                        End Using

                    Catch ex As Exception
                        MsgBox("Could not extract the selected pkg file.", MsgBoxStyle.Critical, "Error")
                        MsgBox(ex.Message)
                    End Try

                Else
                    MsgBox("No destionation path set.", MsgBoxStyle.Exclamation)
                End If
            Else
                MsgBox("No passcode entered.", MsgBoxStyle.Exclamation)
            End If
        Else
            MsgBox("No pkg for extraction selected.", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub BuildPKGButton_Click(sender As Object, e As RoutedEventArgs) Handles BuildPKGButton.Click
        Dim NewPKGBuilder As New PS5PKGBuilder() With {.PubToolsPath = PubToolsPath}
        If Not String.IsNullOrEmpty(SaveToTextBox.Text) Then
            NewPKGBuilder.SelectedProjectTextBox.Text = SaveToTextBox.Text
        End If
        NewPKGBuilder.Show()
    End Sub

End Class
