Imports System.IO
Imports DiscUtils.Iso9660
Imports PS_Multi_Tools.Utils

Public Class BatchRename

    Private Sub BrowseFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseFolderButton.Click
        Dim FBD As New Forms.FolderBrowserDialog() With {.Description = "Select your backups folder"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedFolderTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub StartButton_Click(sender As Object, e As RoutedEventArgs) Handles StartButton.Click
        If Not String.IsNullOrEmpty(SelectedFolderTextBox.Text) Then

            RenameLogTextBox.Clear()

            If RenameOnlyFoldersCheckBox.IsChecked Then

                Try
                    Dim FolderBackups As IEnumerable(Of String) = Directory.EnumerateFiles(SelectedFolderTextBox.Text, "*.SFO", SearchOption.AllDirectories)
                    Dim DirectoriesContainingFiles As IEnumerable(Of String) = FolderBackups.Select(Function(filePath) Path.GetDirectoryName(filePath)).Distinct()
                    'Ensure to not target the PS3_GAME folder
                    Dim AdjustedDirectories As IEnumerable(Of String) = DirectoriesContainingFiles.Select(Function(dir)
                                                                                                              If Path.GetFileName(dir).Equals("PS3_GAME", StringComparison.OrdinalIgnoreCase) Then
                                                                                                                  Return Directory.GetParent(dir).FullName
                                                                                                              Else
                                                                                                                  Return dir
                                                                                                              End If
                                                                                                          End Function).Distinct()

                    'Append found folders to the log
                    RenameLogTextBox.AppendText($"Backups found: {AdjustedDirectories.Count}" & vbCrLf & "Backups Listing:" & vbCrLf)
                    For Each FoundBackup In AdjustedDirectories
                        RenameLogTextBox.AppendText($"{FoundBackup}" & vbCrLf)
                    Next

                    If MsgBox("Changing a directory name requires admin privileges in most cases, especially if the user access is broken." +
                                  "A PowerShell window requesting admin privileges will pop up for each directory. Please confirm it order to rename the folder properly." + Environment.NewLine +
                                  "Confirm with 'Yes' or 'No' to abort the renaming operation.",
                                  MsgBoxStyle.YesNo,
                                  "Rename confirmation") = MsgBoxResult.Yes Then

                        For Each BackupDirectory In AdjustedDirectories

                            'Get the game title
                            Dim GameTitle As String = ""
                            Dim GameTitleID As String = ""
                            If File.Exists(BackupDirectory + "\PS3_GAME\PARAM.SFO") Then

                                Using ParamFileStream As New FileStream(BackupDirectory + "\PS3_GAME\PARAM.SFO", FileMode.Open, FileAccess.Read, FileShare.Read)
                                    Try
                                        Dim SFOKeys As Dictionary(Of String, Object) = SFONew.ReadSfo(ParamFileStream)
                                        If SFOKeys IsNot Nothing AndAlso SFOKeys.Count > 0 Then
                                            Dim TITLEValue As Object = Nothing
                                            If SFOKeys.TryGetValue("TITLE", TITLEValue) Then
                                                GameTitle = CleanTitle(TITLEValue.ToString)
                                            End If
                                            Dim TITLEIDValue As Object = Nothing
                                            If SFOKeys.TryGetValue("TITLE_ID", TITLEIDValue) Then
                                                GameTitleID = TITLEIDValue.ToString
                                            End If
                                        End If
                                    Finally
                                        ParamFileStream.Close()
                                    End Try
                                End Using

                                If Not String.IsNullOrEmpty(GameTitle) AndAlso Not String.IsNullOrEmpty(GameTitleID) Then

                                    If UseDefaultFolderNameCheckBox.IsChecked Then

                                        Dim DestinationPath As String = Path.Combine(Directory.GetParent(BackupDirectory).FullName, GameTitle)

                                        Try
                                            RenameLogTextBox.AppendText($"Moving {BackupDirectory} -> {DestinationPath}" & vbCrLf)
                                            If RenameFolderUsingPowershell(BackupDirectory, GameTitle) Then
                                                RenameLogTextBox.AppendText($"Processed {BackupDirectory} -> {DestinationPath}" & vbCrLf)
                                            Else
                                                RenameLogTextBox.AppendText($"Failed moving {BackupDirectory} -> {DestinationPath}" & vbCrLf)
                                            End If
                                        Catch ex As Exception
                                            'Append the error to the log
                                            RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                            'Skip and do not rename
                                            Continue For
                                        End Try

                                        RenameLogTextBox.ScrollToEnd()

                                    ElseIf UseDefaultFolderNameWithGameIDCheckBox.IsChecked Then

                                        Dim DestinationFolderName As String = GameTitle + " [" + GameTitleID + "]"
                                        Dim DestinationPath As String = Path.Combine(Directory.GetParent(BackupDirectory).FullName, DestinationFolderName)

                                        Try
                                            RenameLogTextBox.AppendText($"Moving {BackupDirectory} -> {DestinationPath}" & vbCrLf)
                                            If RenameFolderUsingPowershell(BackupDirectory, DestinationFolderName) Then
                                                RenameLogTextBox.AppendText($"Processed {BackupDirectory} -> {DestinationPath}" & vbCrLf)
                                            Else
                                                RenameLogTextBox.AppendText($"Failed moving {BackupDirectory} -> {DestinationPath}" & vbCrLf)
                                            End If
                                        Catch ex As Exception
                                            'Append the error to the log
                                            RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                            'Skip and do not rename
                                            Continue For
                                        End Try

                                        RenameLogTextBox.ScrollToEnd()

                                    ElseIf UseCustomCheckBox.IsChecked Then

                                        Dim GameRegion As String = PS3Game.GetGameRegion(GameTitleID)

                                        If Not String.IsNullOrEmpty(CustomRenamingSchemeTextBox.Text) Then
                                            Dim DestinationFolderName As String = BuildFileOrFolderName(CustomRenamingSchemeTextBox.Text, GameTitle, GameTitleID, GameRegion)
                                            Dim DestinationPath As String = Path.Combine(Directory.GetParent(BackupDirectory).FullName, DestinationFolderName)

                                            Try
                                                RenameLogTextBox.AppendText($"Moving {BackupDirectory} -> {DestinationPath}" & vbCrLf)
                                                If RenameFolderUsingPowershell(BackupDirectory, DestinationFolderName) Then
                                                    RenameLogTextBox.AppendText($"Processed {BackupDirectory} -> {DestinationPath}" & vbCrLf)
                                                Else
                                                    RenameLogTextBox.AppendText($"Failed moving {BackupDirectory} -> {DestinationPath}" & vbCrLf)
                                                End If
                                            Catch ex As Exception
                                                'Append the error to the log
                                                RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                                'Skip and do not rename
                                                Continue For
                                            End Try

                                            RenameLogTextBox.ScrollToEnd()

                                        Else
                                            MsgBox("Please enter a custom renaming scheme.", MsgBoxStyle.Critical)
                                        End If

                                    Else
                                        MsgBox("Please select at least one folder name renaming scheme.", MsgBoxStyle.Critical)
                                    End If

                                Else
                                    'Append the error to the log
                                    RenameLogTextBox.AppendText($"No game title found for: {BackupDirectory}. Skipping." & vbCrLf)
                                    'Skip and do not rename
                                    Continue For
                                End If

                            Else
                                'Append the error to the log
                                RenameLogTextBox.AppendText("File " + BackupDirectory + "\PS3_GAME\PARAM.SFO does not exist. Skipping." & vbCrLf)
                                'Skip and do not rename
                                Continue For
                            End If

                        Next

                        RenameLogTextBox.ScrollToEnd()
                        MsgBox("Done!", MsgBoxStyle.Information)
                    End If

                Catch ex As Exception
                    MsgBox("Error accessing folders. Please retry while running as Administrator.", MsgBoxStyle.Critical, "Error")
                End Try

            ElseIf RenameOnlyFilesCheckBox.IsChecked Then
                If RenameOnlyPKGFilesCheckBox.IsChecked Then

                    Dim PKGBackups As IEnumerable(Of String) = Directory.EnumerateFiles(SelectedFolderTextBox.Text, "*.pkg", SearchOption.AllDirectories)

                    'Append found pkg files to the log
                    RenameLogTextBox.AppendText($"Found PKG files: {PKGBackups.Count}" & vbCrLf & "Backups Listing:" & vbCrLf)
                    For Each FoundBackup In PKGBackups
                        RenameLogTextBox.AppendText($"{FoundBackup}" & vbCrLf)
                    Next

                    For Each PKGBackup In PKGBackups
                        Try
                            'Decrypt pkg file
                            Dim NewPKGDecryptor As New PKGDecryptor()
                            NewPKGDecryptor.ProcessPKGFile(PKGBackup)

                            Dim GameTitle As String = ""
                            Dim GameTitleID As String = ""
                            If NewPKGDecryptor.GetPARAMSFO IsNot Nothing Then
                                Dim SFOKeys As Dictionary(Of String, Object) = SFONew.ReadSfo(NewPKGDecryptor.GetPARAMSFO)
                                Dim TITLEValue As Object = Nothing
                                If SFOKeys.TryGetValue("TITLE", TITLEValue) Then
                                    GameTitle = CleanTitle(TITLEValue.ToString)
                                End If
                                Dim TITLEIDValue As Object = Nothing
                                If SFOKeys.TryGetValue("TITLE_ID", TITLEIDValue) Then
                                    GameTitleID = TITLEIDValue.ToString
                                End If
                            Else
                                'Append the error to the log
                                RenameLogTextBox.AppendText("File " + PKGBackup + " does not contain a PARAM.SFO file. Skipping." & vbCrLf)
                                Continue For
                            End If

                            NewPKGDecryptor = Nothing

                            If Not String.IsNullOrEmpty(GameTitle) AndAlso Not String.IsNullOrEmpty(GameTitleID) Then

                                Dim GameRegion As String = PS3Game.GetGameRegion(GameTitleID)
                                Dim PKGParentFolderPath As String = Directory.GetParent(PKGBackup).FullName

                                If UseDefaultCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = GameTitle & ".pkg"
                                    Dim DestinationPath As String = Path.Combine(PKGParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(PKGBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {PKGBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                    RenameLogTextBox.ScrollToEnd()

                                ElseIf UseWithGameIDCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = GameTitle & " [" & GameTitleID & "].pkg"
                                    Dim DestinationPath As String = Path.Combine(PKGParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(PKGBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {PKGBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                    RenameLogTextBox.ScrollToEnd()

                                ElseIf UseWithGameTitleCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = GameTitleID & "-[" & GameTitle & "].pkg"
                                    Dim DestinationPath As String = Path.Combine(PKGParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(PKGBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {PKGBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                    RenameLogTextBox.ScrollToEnd()

                                ElseIf UseWithBracketsCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = GameTitle & " (" & GameTitleID & ").pkg"
                                    Dim DestinationPath As String = Path.Combine(PKGParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(PKGBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {PKGBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                    RenameLogTextBox.ScrollToEnd()

                                ElseIf UseWithRegionLanguagesCheckBox.IsChecked Then

                                    Dim DatabaseTitle As String = GetRegionalTitleFromGameID(Environment.CurrentDirectory + "\Tools\dkeydb.html", GameTitleID)
                                    If Not String.IsNullOrEmpty(DatabaseTitle) Then

                                        Dim DestinationFileName As String = DatabaseTitle & ".pkg"
                                        Dim DestinationPath As String = Path.Combine(PKGParentFolderPath, DestinationFileName)

                                        Try
                                            If File.Exists(DestinationPath) Then
                                                RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                                Continue For
                                            End If
                                            File.Move(PKGBackup, DestinationPath)
                                            RenameLogTextBox.AppendText($"Processed {PKGBackup} -> {DestinationPath}" & vbCrLf)
                                        Catch ex As Exception
                                            'Append the error to the log
                                            RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                            'Skip and do not rename
                                            Continue For
                                        End Try

                                    Else
                                        RenameLogTextBox.AppendText("A regional title for: " & PKGBackup & " could not be found. Skipping." & vbCrLf)
                                        Continue For
                                    End If

                                    RenameLogTextBox.ScrollToEnd()

                                ElseIf UseCustomCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = BuildFileOrFolderName(CustomRenamingSchemeTextBox.Text, GameTitle, GameTitleID, GameRegion, ".pkg")
                                    Dim DestinationPath As String = Path.Combine(PKGParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(PKGBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {PKGBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                    RenameLogTextBox.ScrollToEnd()

                                End If
                            Else
                                'Append the error to the log
                                RenameLogTextBox.AppendText("File " + PKGBackup + " does not contain a PARAM.SFO file. Skipping." & vbCrLf)
                                Continue For
                            End If

                        Catch ex As Exception
                            'Append the error to the log
                            RenameLogTextBox.AppendText("File " + PKGBackup + " is not a valid PKG. Skipping." & vbCrLf)
                            'Skip and do not rename
                            Continue For
                        End Try
                    Next

                    RenameLogTextBox.ScrollToEnd()
                    MsgBox("Done!", MsgBoxStyle.Information)
                ElseIf RenameOnlyISOFilesCheckBox.IsChecked Then

                    Dim ISOBackups As IEnumerable(Of String) = Directory.EnumerateFiles(SelectedFolderTextBox.Text, "*.iso", SearchOption.AllDirectories)

                    'Append found ISO files to the log
                    RenameLogTextBox.AppendText($"Found ISO files: {ISOBackups.Count}" & vbCrLf & "Backups Listing:" & vbCrLf)
                    For Each FoundBackup In ISOBackups
                        RenameLogTextBox.AppendText($"{FoundBackup}" & vbCrLf)
                    Next

                    For Each ISOBackup In ISOBackups
                        Try
                            'Get game title & id from the ISO
                            Dim GameTitle As String = ""
                            Dim GameTitleID As String = ""
                            Using NewISOStream As FileStream = File.Open(ISOBackup, FileMode.Open)
                                Dim NewCDReader As New CDReader(NewISOStream, True)
                                Try
                                    Using NewFileStream As Stream = NewCDReader.OpenFile("PS3_GAME\PARAM.SFO", FileMode.Open)
                                        Try
                                            Dim SFOKeys As Dictionary(Of String, Object) = SFONew.ReadSfo(NewFileStream)
                                            If SFOKeys IsNot Nothing AndAlso SFOKeys.Count > 0 Then
                                                Dim TITLEValue As Object = Nothing
                                                If SFOKeys.TryGetValue("TITLE", TITLEValue) Then
                                                    GameTitle = CleanTitle(TITLEValue.ToString)
                                                End If
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
                                    RenameLogTextBox.AppendText("File " + ISOBackup + " does not contain a PARAM.SFO file. Skipping." & vbCrLf)
                                    Continue For
                                End Try

                                NewCDReader.Dispose()
                                NewISOStream.Close()
                            End Using

                            If Not String.IsNullOrEmpty(GameTitle) AndAlso Not String.IsNullOrEmpty(GameTitleID) Then

                                Dim GameRegion As String = PS3Game.GetGameRegion(GameTitleID)
                                Dim ISOParentFolderPath As String = Directory.GetParent(ISOBackup).FullName

                                If UseDefaultCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = GameTitle & ".iso"
                                    Dim DestinationPath As String = Path.Combine(ISOParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(ISOBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {ISOBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                ElseIf UseWithGameIDCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = GameTitle & " [" & GameTitleID & "].iso"
                                    Dim DestinationPath As String = Path.Combine(ISOParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(ISOBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {ISOBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                ElseIf UseWithGameTitleCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = GameTitleID & "-[" & GameTitle & "].iso"
                                    Dim DestinationPath As String = Path.Combine(ISOParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(ISOBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {ISOBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                ElseIf UseWithBracketsCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = GameTitle & " (" & GameTitleID & ").iso"
                                    Dim DestinationPath As String = Path.Combine(ISOParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(ISOBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {ISOBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                ElseIf UseWithRegionLanguagesCheckBox.IsChecked Then

                                    Dim DatabaseTitle As String = GetRegionalTitleFromGameID(Environment.CurrentDirectory + "\Tools\dkeydb.html", GameTitleID)
                                    If Not String.IsNullOrEmpty(DatabaseTitle) Then

                                        Dim DestinationFileName As String = DatabaseTitle & ".iso"
                                        Dim DestinationPath As String = Path.Combine(ISOParentFolderPath, DestinationFileName)

                                        Try
                                            If File.Exists(DestinationPath) Then
                                                RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                                Continue For
                                            End If
                                            File.Move(ISOBackup, DestinationPath)
                                            RenameLogTextBox.AppendText($"Processed {ISOBackup} -> {DestinationPath}" & vbCrLf)
                                        Catch ex As Exception
                                            'Append the error to the log
                                            RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                            'Skip and do not rename
                                            Continue For
                                        End Try

                                    Else
                                        RenameLogTextBox.AppendText("A regional title for: " & ISOBackup & " could not be found. Skipping." & vbCrLf)
                                        Continue For
                                    End If

                                ElseIf UseCustomCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = BuildFileOrFolderName(CustomRenamingSchemeTextBox.Text, GameTitle, GameTitleID, GameRegion, ".iso")
                                    Dim DestinationPath As String = Path.Combine(ISOParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(ISOBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {ISOBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                    RenameLogTextBox.ScrollToEnd()

                                End If

                            Else
                                'Append the error to the log
                                RenameLogTextBox.AppendText("No title found for " + ISOBackup + ". Skipping." & vbCrLf)
                                Continue For
                            End If

                        Catch ex As Exception
                            'Append the error to the log
                            RenameLogTextBox.AppendText("File " + ISOBackup + " is not a valid ISO. Skipping." & vbCrLf)
                            'Skip and do not rename
                            Continue For
                        End Try
                    Next

                    RenameLogTextBox.ScrollToEnd()
                    MsgBox("Done!", MsgBoxStyle.Information)
                ElseIf RenameBothCheckBox.IsChecked Then

                    Dim PKGBackups As IEnumerable(Of String) = Directory.EnumerateFiles(SelectedFolderTextBox.Text, "*.pkg", SearchOption.AllDirectories)
                    Dim ISOBackups As IEnumerable(Of String) = Directory.EnumerateFiles(SelectedFolderTextBox.Text, "*.iso", SearchOption.AllDirectories)

                    'Append found pkg files to the log
                    RenameLogTextBox.AppendText($"Found PKG files: {PKGBackups.Count}" & vbCrLf & "Backups Listing:" & vbCrLf)
                    For Each FoundBackup In PKGBackups
                        RenameLogTextBox.AppendText($"{FoundBackup}" & vbCrLf)
                    Next

                    'Append found ISO files to the log
                    RenameLogTextBox.AppendText($"Found ISO files: {ISOBackups.Count}" & vbCrLf & "Backups Listing:" & vbCrLf)
                    For Each FoundBackup In ISOBackups
                        RenameLogTextBox.AppendText($"{FoundBackup}" & vbCrLf)
                    Next

                    For Each PKGBackup In PKGBackups
                        Try
                            'Decrypt pkg file
                            Dim NewPKGDecryptor As New PKGDecryptor()
                            NewPKGDecryptor.ProcessPKGFile(PKGBackup)

                            Dim GameTitle As String = ""
                            Dim GameTitleID As String = ""
                            If NewPKGDecryptor.GetPARAMSFO IsNot Nothing Then
                                Dim SFOKeys As Dictionary(Of String, Object) = SFONew.ReadSfo(NewPKGDecryptor.GetPARAMSFO)
                                Dim TITLEValue As Object = Nothing
                                If SFOKeys.TryGetValue("TITLE", TITLEValue) Then
                                    GameTitle = CleanTitle(TITLEValue.ToString)
                                End If
                                Dim TITLEIDValue As Object = Nothing
                                If SFOKeys.TryGetValue("TITLE_ID", TITLEIDValue) Then
                                    GameTitleID = TITLEIDValue.ToString
                                End If
                            Else
                                'Append the error to the log
                                RenameLogTextBox.AppendText("File " + PKGBackup + " does not contain a PARAM.SFO file. Skipping." & vbCrLf)
                                Continue For
                            End If

                            NewPKGDecryptor = Nothing

                            If Not String.IsNullOrEmpty(GameTitle) AndAlso Not String.IsNullOrEmpty(GameTitleID) Then

                                Dim GameRegion As String = PS3Game.GetGameRegion(GameTitleID)
                                Dim PKGParentFolderPath As String = Directory.GetParent(PKGBackup).FullName

                                If UseDefaultCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = GameTitle & ".pkg"
                                    Dim DestinationPath As String = Path.Combine(PKGParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(PKGBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {PKGBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                    RenameLogTextBox.ScrollToEnd()

                                ElseIf UseWithGameIDCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = GameTitle & " [" & GameTitleID & "].pkg"
                                    Dim DestinationPath As String = Path.Combine(PKGParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(PKGBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {PKGBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                    RenameLogTextBox.ScrollToEnd()

                                ElseIf UseWithGameTitleCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = GameTitleID & "-[" & GameTitle & "].pkg"
                                    Dim DestinationPath As String = Path.Combine(PKGParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(PKGBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {PKGBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                    RenameLogTextBox.ScrollToEnd()

                                ElseIf UseWithBracketsCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = GameTitle & " (" & GameTitleID & ").pkg"
                                    Dim DestinationPath As String = Path.Combine(PKGParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(PKGBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {PKGBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                    RenameLogTextBox.ScrollToEnd()

                                ElseIf UseWithRegionLanguagesCheckBox.IsChecked Then

                                    Dim DatabaseTitle As String = GetRegionalTitleFromGameID(Environment.CurrentDirectory + "\Tools\dkeydb.html", GameTitleID)
                                    If Not String.IsNullOrEmpty(DatabaseTitle) Then

                                        Dim DestinationFileName As String = DatabaseTitle & ".pkg"
                                        Dim DestinationPath As String = Path.Combine(PKGParentFolderPath, DestinationFileName)

                                        Try
                                            If File.Exists(DestinationPath) Then
                                                RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                                Continue For
                                            End If
                                            File.Move(PKGBackup, DestinationPath)
                                            RenameLogTextBox.AppendText($"Processed {PKGBackup} -> {DestinationPath}" & vbCrLf)
                                        Catch ex As Exception
                                            'Append the error to the log
                                            RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                            'Skip and do not rename
                                            Continue For
                                        End Try

                                    Else
                                        RenameLogTextBox.AppendText("A regional title for: " & PKGBackup & " could not be found. Skipping." & vbCrLf)
                                        Continue For
                                    End If

                                    RenameLogTextBox.ScrollToEnd()

                                ElseIf UseCustomCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = BuildFileOrFolderName(CustomRenamingSchemeTextBox.Text, GameTitle, GameTitleID, GameRegion, ".pkg")
                                    Dim DestinationPath As String = Path.Combine(PKGParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(PKGBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {PKGBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                    RenameLogTextBox.ScrollToEnd()

                                End If
                            Else
                                'Append the error to the log
                                RenameLogTextBox.AppendText("File " + PKGBackup + " does not contain a PARAM.SFO file. Skipping." & vbCrLf)
                                Continue For
                            End If

                        Catch ex As Exception
                            'Append the error to the log
                            RenameLogTextBox.AppendText("File " + PKGBackup + " is not a valid PKG. Skipping." & vbCrLf)
                            'Skip and do not rename
                            Continue For
                        End Try
                    Next

                    For Each ISOBackup In ISOBackups
                        Try
                            'Get game title & id from the ISO
                            Dim GameTitle As String = ""
                            Dim GameTitleID As String = ""
                            Using NewISOStream As FileStream = File.Open(ISOBackup, FileMode.Open)
                                Dim NewCDReader As New CDReader(NewISOStream, True)
                                Try
                                    Using NewFileStream As Stream = NewCDReader.OpenFile("PS3_GAME\PARAM.SFO", FileMode.Open)
                                        Try
                                            Dim SFOKeys As Dictionary(Of String, Object) = SFONew.ReadSfo(NewFileStream)
                                            If SFOKeys IsNot Nothing AndAlso SFOKeys.Count > 0 Then
                                                Dim TITLEValue As Object = Nothing
                                                If SFOKeys.TryGetValue("TITLE", TITLEValue) Then
                                                    GameTitle = CleanTitle(TITLEValue.ToString)
                                                End If
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
                                    RenameLogTextBox.AppendText("File " + ISOBackup + " does not contain a PARAM.SFO file. Skipping." & vbCrLf)
                                    Continue For
                                End Try

                                NewCDReader.Dispose()
                                NewISOStream.Close()
                            End Using

                            If Not String.IsNullOrEmpty(GameTitle) AndAlso Not String.IsNullOrEmpty(GameTitleID) Then

                                Dim GameRegion As String = PS3Game.GetGameRegion(GameTitleID)
                                Dim ISOParentFolderPath As String = Directory.GetParent(ISOBackup).FullName

                                If UseDefaultCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = GameTitle & ".iso"
                                    Dim DestinationPath As String = Path.Combine(ISOParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(ISOBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {ISOBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                ElseIf UseWithGameIDCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = GameTitle & " [" & GameTitleID & "].iso"
                                    Dim DestinationPath As String = Path.Combine(ISOParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(ISOBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {ISOBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                ElseIf UseWithGameTitleCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = GameTitleID & "-[" & GameTitle & "].iso"
                                    Dim DestinationPath As String = Path.Combine(ISOParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(ISOBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {ISOBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                ElseIf UseWithBracketsCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = GameTitle & " (" & GameTitleID & ").iso"
                                    Dim DestinationPath As String = Path.Combine(ISOParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(ISOBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {ISOBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                ElseIf UseWithRegionLanguagesCheckBox.IsChecked Then

                                    Dim DatabaseTitle As String = GetRegionalTitleFromGameID(Environment.CurrentDirectory + "\Tools\dkeydb.html", GameTitleID)
                                    If Not String.IsNullOrEmpty(DatabaseTitle) Then

                                        Dim DestinationFileName As String = DatabaseTitle & ".iso"
                                        Dim DestinationPath As String = Path.Combine(ISOParentFolderPath, DestinationFileName)

                                        Try
                                            If File.Exists(DestinationPath) Then
                                                RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                                Continue For
                                            End If
                                            File.Move(ISOBackup, DestinationPath)
                                            RenameLogTextBox.AppendText($"Processed {ISOBackup} -> {DestinationPath}" & vbCrLf)
                                        Catch ex As Exception
                                            'Append the error to the log
                                            RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                            'Skip and do not rename
                                            Continue For
                                        End Try

                                    Else
                                        RenameLogTextBox.AppendText("A regional title for: " & ISOBackup & " could not be found. Skipping." & vbCrLf)
                                        Continue For
                                    End If

                                ElseIf UseCustomCheckBox.IsChecked Then

                                    Dim DestinationFileName As String = BuildFileOrFolderName(CustomRenamingSchemeTextBox.Text, GameTitle, GameTitleID, GameRegion, ".iso")
                                    Dim DestinationPath As String = Path.Combine(ISOParentFolderPath, DestinationFileName)

                                    Try
                                        If File.Exists(DestinationPath) Then
                                            RenameLogTextBox.AppendText("A file with the new name already exists: " & DestinationPath & ". Skipping." & vbCrLf)
                                            Continue For
                                        End If
                                        File.Move(ISOBackup, DestinationPath)
                                        RenameLogTextBox.AppendText($"Processed {ISOBackup} -> {DestinationPath}" & vbCrLf)
                                    Catch ex As Exception
                                        'Append the error to the log
                                        RenameLogTextBox.AppendText(ex.Message & vbCrLf)
                                        'Skip and do not rename
                                        Continue For
                                    End Try

                                    RenameLogTextBox.ScrollToEnd()

                                End If

                            Else
                                'Append the error to the log
                                RenameLogTextBox.AppendText("No title found for " + ISOBackup + ". Skipping." & vbCrLf)
                                Continue For
                            End If

                        Catch ex As Exception
                            'Append the error to the log
                            RenameLogTextBox.AppendText("File " + ISOBackup + " is not a valid ISO. Skipping." & vbCrLf)
                            'Skip and do not rename
                            Continue For
                        End Try
                    Next

                    RenameLogTextBox.ScrollToEnd()
                    MsgBox("Done!", MsgBoxStyle.Information)
                Else
                    MsgBox("Please select at least one file name renaming option (ISO/PKG/BOTH).", MsgBoxStyle.Critical)
                End If
            Else
                MsgBox("Please select a renaming option.", MsgBoxStyle.Critical)
            End If
        Else
            MsgBox("Please select a backup folder.", MsgBoxStyle.Critical)
        End If
    End Sub

#Region "Options"

    Private Sub RenameOnlyFoldersCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles RenameOnlyFoldersCheckBox.Checked
        RenameOnlyFilesCheckBox.IsEnabled = False
    End Sub

    Private Sub RenameOnlyFilesCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles RenameOnlyFilesCheckBox.Checked
        RenameOnlyFoldersCheckBox.IsEnabled = False
    End Sub

    Private Sub RenameOnlyFoldersCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles RenameOnlyFoldersCheckBox.Unchecked
        RenameOnlyFilesCheckBox.IsEnabled = True
    End Sub

    Private Sub RenameOnlyFilesCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles RenameOnlyFilesCheckBox.Unchecked
        RenameOnlyFoldersCheckBox.IsEnabled = True
    End Sub

    Private Sub RenameOnlyPKGFilesCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles RenameOnlyPKGFilesCheckBox.Checked
        RenameBothCheckBox.IsEnabled = False
        RenameOnlyISOFilesCheckBox.IsEnabled = False
    End Sub

    Private Sub RenameOnlyISOFilesCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles RenameOnlyISOFilesCheckBox.Checked
        RenameBothCheckBox.IsEnabled = False
        RenameOnlyPKGFilesCheckBox.IsEnabled = False
    End Sub

    Private Sub RenameBothCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles RenameBothCheckBox.Checked
        RenameOnlyISOFilesCheckBox.IsEnabled = False
        RenameOnlyPKGFilesCheckBox.IsEnabled = False
    End Sub

    Private Sub RenameOnlyPKGFilesCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles RenameOnlyPKGFilesCheckBox.Unchecked
        RenameBothCheckBox.IsEnabled = True
        RenameOnlyISOFilesCheckBox.IsEnabled = True
    End Sub

    Private Sub RenameOnlyISOFilesCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles RenameOnlyISOFilesCheckBox.Unchecked
        RenameBothCheckBox.IsEnabled = True
        RenameOnlyPKGFilesCheckBox.IsEnabled = True
    End Sub

    Private Sub RenameBothCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles RenameBothCheckBox.Unchecked
        RenameOnlyISOFilesCheckBox.IsEnabled = True
        RenameOnlyPKGFilesCheckBox.IsEnabled = True
    End Sub

#End Region

#Region "Rename Options"

    Private Sub UseDefaultCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles UseDefaultCheckBox.Checked
        UseDefaultFolderNameCheckBox.IsEnabled = False
        UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = False
        UseWithRegionLanguagesCheckBox.IsEnabled = False
        UseWithGameIDCheckBox.IsEnabled = False
        UseWithGameTitleCheckBox.IsEnabled = False
        UseWithBracketsCheckBox.IsEnabled = False
        UseCustomCheckBox.IsEnabled = False
    End Sub

    Private Sub UseDefaultCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles UseDefaultCheckBox.Unchecked
        UseDefaultFolderNameCheckBox.IsEnabled = True
        UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = True
        UseWithRegionLanguagesCheckBox.IsEnabled = True
        UseWithGameIDCheckBox.IsEnabled = True
        UseWithGameTitleCheckBox.IsEnabled = True
        UseWithBracketsCheckBox.IsEnabled = True
        UseCustomCheckBox.IsEnabled = True
    End Sub

    Private Sub UseWithGameIDCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles UseWithGameIDCheckBox.Checked
        UseDefaultFolderNameCheckBox.IsEnabled = False
        UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = False
        UseWithRegionLanguagesCheckBox.IsEnabled = False
        UseDefaultCheckBox.IsEnabled = False
        UseWithGameTitleCheckBox.IsEnabled = False
        UseWithBracketsCheckBox.IsEnabled = False
        UseCustomCheckBox.IsEnabled = False
    End Sub

    Private Sub UseWithGameIDCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles UseWithGameIDCheckBox.Unchecked
        UseDefaultFolderNameCheckBox.IsEnabled = True
        UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = True
        UseWithRegionLanguagesCheckBox.IsEnabled = True
        UseDefaultCheckBox.IsEnabled = True
        UseWithGameTitleCheckBox.IsEnabled = True
        UseWithBracketsCheckBox.IsEnabled = True
        UseCustomCheckBox.IsEnabled = True
    End Sub

    Private Sub UseWithGameTitleCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles UseWithGameTitleCheckBox.Checked
        UseDefaultFolderNameCheckBox.IsEnabled = False
        UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = False
        UseWithRegionLanguagesCheckBox.IsEnabled = False
        UseDefaultCheckBox.IsEnabled = False
        UseWithGameIDCheckBox.IsEnabled = False
        UseWithBracketsCheckBox.IsEnabled = False
        UseCustomCheckBox.IsEnabled = False
    End Sub

    Private Sub UseWithGameTitleCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles UseWithGameTitleCheckBox.Unchecked
        UseDefaultFolderNameCheckBox.IsEnabled = True
        UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = True
        UseWithRegionLanguagesCheckBox.IsEnabled = True
        UseDefaultCheckBox.IsEnabled = True
        UseWithGameIDCheckBox.IsEnabled = True
        UseWithBracketsCheckBox.IsEnabled = True
        UseCustomCheckBox.IsEnabled = True
    End Sub

    Private Sub UseWithBracketsCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles UseWithBracketsCheckBox.Checked
        UseDefaultFolderNameCheckBox.IsEnabled = True
        UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = True
        UseWithRegionLanguagesCheckBox.IsEnabled = True
        UseDefaultFolderNameCheckBox.IsEnabled = False
        UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = False
        UseWithRegionLanguagesCheckBox.IsEnabled = False
        UseDefaultCheckBox.IsEnabled = False
        UseWithGameIDCheckBox.IsEnabled = False
        UseWithGameTitleCheckBox.IsEnabled = False
        UseCustomCheckBox.IsEnabled = False
    End Sub

    Private Sub UseWithBracketsCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles UseWithBracketsCheckBox.Unchecked
        UseDefaultFolderNameCheckBox.IsEnabled = True
        UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = True
        UseWithRegionLanguagesCheckBox.IsEnabled = True
        UseDefaultCheckBox.IsEnabled = True
        UseWithGameIDCheckBox.IsEnabled = True
        UseWithGameTitleCheckBox.IsEnabled = True
        UseCustomCheckBox.IsEnabled = True
    End Sub

    Private Sub UseCustomCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles UseCustomCheckBox.Checked
        UseDefaultFolderNameCheckBox.IsEnabled = False
        UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = False
        UseWithRegionLanguagesCheckBox.IsEnabled = False
        UseDefaultCheckBox.IsEnabled = False
        UseWithGameIDCheckBox.IsEnabled = False
        UseWithGameTitleCheckBox.IsEnabled = False
        UseWithBracketsCheckBox.IsEnabled = False
    End Sub

    Private Sub UseCustomCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles UseCustomCheckBox.Unchecked
        UseDefaultFolderNameCheckBox.IsEnabled = True
        UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = True
        UseWithRegionLanguagesCheckBox.IsEnabled = True
        UseDefaultCheckBox.IsEnabled = True
        UseWithGameIDCheckBox.IsEnabled = True
        UseWithGameTitleCheckBox.IsEnabled = True
        UseWithBracketsCheckBox.IsEnabled = True
    End Sub

    Private Sub UseDefaultFolderNameCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles UseDefaultFolderNameCheckBox.Checked
        UseCustomCheckBox.IsEnabled = False
        UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = False
        UseWithRegionLanguagesCheckBox.IsEnabled = False
        UseDefaultCheckBox.IsEnabled = False
        UseWithGameIDCheckBox.IsEnabled = False
        UseWithGameTitleCheckBox.IsEnabled = False
        UseWithBracketsCheckBox.IsEnabled = False
    End Sub

    Private Sub UseDefaultFolderNameCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles UseDefaultFolderNameCheckBox.Unchecked
        UseCustomCheckBox.IsEnabled = True
        UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = True
        UseWithRegionLanguagesCheckBox.IsEnabled = True
        UseDefaultCheckBox.IsEnabled = True
        UseWithGameIDCheckBox.IsEnabled = True
        UseWithGameTitleCheckBox.IsEnabled = True
        UseWithBracketsCheckBox.IsEnabled = True
    End Sub

    Private Sub UseDefaultFolderNameWithGameIDCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles UseDefaultFolderNameWithGameIDCheckBox.Checked
        UseCustomCheckBox.IsEnabled = False
        UseDefaultFolderNameCheckBox.IsEnabled = False
        UseWithRegionLanguagesCheckBox.IsEnabled = False
        UseDefaultCheckBox.IsEnabled = False
        UseWithGameIDCheckBox.IsEnabled = False
        UseWithGameTitleCheckBox.IsEnabled = False
        UseWithBracketsCheckBox.IsEnabled = False
    End Sub

    Private Sub UseDefaultFolderNameWithGameIDCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles UseDefaultFolderNameWithGameIDCheckBox.Unchecked
        UseCustomCheckBox.IsEnabled = True
        UseDefaultFolderNameCheckBox.IsEnabled = True
        UseWithRegionLanguagesCheckBox.IsEnabled = True
        UseDefaultCheckBox.IsEnabled = True
        UseWithGameIDCheckBox.IsEnabled = True
        UseWithGameTitleCheckBox.IsEnabled = True
        UseWithBracketsCheckBox.IsEnabled = True
    End Sub

    Private Sub UseWithRegionLanguagesCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles UseWithRegionLanguagesCheckBox.Checked
        UseCustomCheckBox.IsEnabled = False
        UseDefaultFolderNameCheckBox.IsEnabled = False
        UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = False
        UseDefaultCheckBox.IsEnabled = False
        UseWithGameIDCheckBox.IsEnabled = False
        UseWithGameTitleCheckBox.IsEnabled = False
        UseWithBracketsCheckBox.IsEnabled = False
    End Sub

    Private Sub UseWithRegionLanguagesCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles UseWithRegionLanguagesCheckBox.Unchecked
        UseCustomCheckBox.IsEnabled = True
        UseDefaultFolderNameCheckBox.IsEnabled = True
        UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = True
        UseDefaultCheckBox.IsEnabled = True
        UseWithGameIDCheckBox.IsEnabled = True
        UseWithGameTitleCheckBox.IsEnabled = True
        UseWithBracketsCheckBox.IsEnabled = True
    End Sub

#End Region

    Private Function BuildFileOrFolderName(CustomScheme As String, GameTitle As String, GameID As String, Region As String, Optional FileExtension As String = "") As String
        Dim FinalFileName As String = CustomScheme

        FinalFileName = FinalFileName.Replace("GAMETITLE", GameTitle)
        FinalFileName = FinalFileName.Replace("GAMEID", GameID)
        FinalFileName = FinalFileName.Replace("REGION", Region)

        FinalFileName = FinalFileName.Replace("(GAMETITLE)", $"({GameTitle})")
        FinalFileName = FinalFileName.Replace("(GAMEID)", $"({GameID})")
        FinalFileName = FinalFileName.Replace("(REGION)", $"({Region})")

        FinalFileName = FinalFileName.Replace("[GAMETITLE]", $"[{GameTitle}]")
        FinalFileName = FinalFileName.Replace("[GAMEID]", $"[{GameID}]")
        FinalFileName = FinalFileName.Replace("[REGION]", $"[{Region}]")

        If Not String.IsNullOrEmpty(FileExtension) Then
            FinalFileName = FinalFileName.Replace("EXTENSION", FileExtension)
            FinalFileName = FinalFileName.Replace("(EXTENSION)", $"({FileExtension})")
            FinalFileName = FinalFileName.Replace("[EXTENSION]", $"[{FileExtension}]")
        End If

        For Each InvalidChar As Char In Path.GetInvalidFileNameChars()
            FinalFileName = FinalFileName.Replace(InvalidChar.ToString(), "")
        Next

        Return FinalFileName
    End Function

End Class
