Imports System.IO
Imports Newtonsoft.Json
Imports PS_Multi_Tools.PS5ParamClass

Public Class PS5MakefSelfs

    'PS5 Make Fake Self Batch Script by EchoStretch
    'PS5 SDK Patch (Auto_backport) Utility by Markus95
    'Translated VB.NET code by SvenGDK

    Dim AlreadyPatched As Boolean = False
    Dim TotalSize As Long = 0

    Const PT_SCE_PROCPARAM As UInteger = 1627389953UI
    Const PT_SCE_MODULE_PARAM As UInteger = 1627389954UI

    Const SCE_PROCESS_PARAM_MAGIC As UInteger = 1229083215UI
    Const SCE_MODULE_PARAM_MAGIC As UInteger = 1007940799UI

    Const SCE_PARAM_PS5_SDK_OFFSET As Long = 12
    Const SCE_PARAM_BASE_SDK_OFFSET As Long = 8

    Const PHT_OFFSET_OFFSET As Long = 32
    Const PHT_OFFSET_SIZE As Integer = 8
    Const PHT_COUNT_OFFSET As Long = 56
    Const PHT_COUNT_SIZE As Integer = 2

    Const PHDR_ENTRY_SIZE As Long = 56
    Const PHDR_TYPE_OFFSET As Long = 0
    Const PHDR_TYPE_SIZE As Integer = 4
    Const PHDR_OFFSET_OFFSET As Long = 8
    Const PHDR_OFFSET_SIZE As Integer = 8

    ReadOnly ELF_MAGIC() As Byte = {127, 69, 76, 70}
    ReadOnly PS4_FSELF_MAGIC() As Byte = {79, 21, 61, 29}
    ReadOnly PS5_FSELF_MAGIC() As Byte = {84, 20, 245, 238}

    ReadOnly ExecutableExtensions() As String = {".bin", ".elf", ".self", ".prx", ".sprx"}

    Private Sub BrowseFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseFolderButton.Click
        Dim FBD As New Forms.FolderBrowserDialog()

        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedDirectoryTextBox.Text = FBD.SelectedPath
            MakeButton.IsEnabled = True

            If Not String.IsNullOrEmpty(MakeLogTextBox.Text) Then
                MakeLogTextBox.Clear()
            End If

            CheckFiles(FBD.SelectedPath)
        End If
    End Sub

    Private Sub CheckFiles(FilePath As String)

        'Check if game is an extracted PS5 backup
        If Not File.Exists(FilePath + "\sce_sys\param.json") Then
            MsgBox("This is not a valid PS5 backup.", MsgBoxStyle.Critical, "Cannot process this backup")
            Exit Sub
        Else
            MakeLogTextBox.AppendText("Found valid '" + FilePath + "\sce_sys\param.json" + "'" + vbCrLf)
            MakeLogTextBox.ScrollToEnd()
        End If

        'Check if .esbak backup files exist in selected backup
        If File.Exists(FilePath + "\eboot.bin.esbak") Then
            AlreadyPatched = True
            MakeLogTextBox.AppendText("Existing '.esbak' files found." + vbCrLf)
            MakeLogTextBox.ScrollToEnd()
        End If

        'Display icon if exists
        If File.Exists(FilePath + "\sce_sys\icon0.png") Then
            Dim TempBitmapImage = New BitmapImage()
            TempBitmapImage.BeginInit()
            TempBitmapImage.CacheOption = BitmapCacheOption.OnLoad
            TempBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache
            TempBitmapImage.UriSource = New Uri(FilePath + "\sce_sys\icon0.png", UriKind.RelativeOrAbsolute)
            TempBitmapImage.EndInit()
            BackupIconImage.Source = TempBitmapImage
        End If

        'Load backup data
        Dim ParamData As PS5Param = JsonConvert.DeserializeObject(Of PS5Param)(File.ReadAllText(FilePath + "\sce_sys\param.json"))
        Dim ParamFileInfo As New FileInfo(FilePath + "\sce_sys\param.json")
        Dim BackupFolderPath As String = Directory.GetParent(ParamFileInfo.FullName).Parent.FullName

        If ParamData.TitleId IsNot Nothing Then
            TitleIDTextBlock.Text = "Title ID: " + ParamData.TitleId
            RegionTextBlock.Text = "Region: " + PS5Game.GetGameRegion(ParamData.TitleId)
        End If
        If ParamData.LocalizedParameters.EnUS IsNot Nothing Then
            TitleTextBlock.Text = ParamData.LocalizedParameters.EnUS.TitleName
        End If
        If ParamData.ContentId IsNot Nothing Then
            ContentIDTextBlock.Text = "Content ID: " + ParamData.ContentId
        End If
        If ParamData.ApplicationCategoryType = 0 Then
            TypeTextBlock.Text = "Type: Game"
        ElseIf ParamData.ApplicationCategoryType = 65536 Then
            TypeTextBlock.Text = "Type: Native Media App"
        ElseIf ParamData.ApplicationCategoryType = 65792 Then
            TypeTextBlock.Text = "Type: RNPS Media App"
        ElseIf ParamData.ApplicationCategoryType = 131328 Then
            TypeTextBlock.Text = "Type: System Built-in App"
        ElseIf ParamData.ApplicationCategoryType = 131584 Then
            TypeTextBlock.Text = "Type: Big Daemon"
        ElseIf ParamData.ApplicationCategoryType = 16777216 Then
            TypeTextBlock.Text = "Type: ShellUI"
        ElseIf ParamData.ApplicationCategoryType = 33554432 Then
            TypeTextBlock.Text = "Type: Daemon"
        ElseIf ParamData.ApplicationCategoryType = 67108864 Then
            TypeTextBlock.Text = "Type: ShellApp"
        Else
            TypeTextBlock.Text = "Type: Unknown"
        End If

        SizeTextBlock.Text = "Size: " + FormatNumber(GetDirSize(BackupFolderPath) / 1073741824, 2) + " GB"

        'Set Base SDK Version based on param.json
        If ParamData.SdkVersion IsNot Nothing Then
            BaseSDKVersionComboBox.Text = ParamData.SdkVersion.Remove(9, 8)
            MakeLogTextBox.AppendText("Retrieved Base SDK Version from param.json: '" + ParamData.SdkVersion.Remove(9, 8) + "'" + vbCrLf)
            MakeLogTextBox.ScrollToEnd()
        End If

    End Sub

    Private Sub MakeButton_Click(sender As Object, e As RoutedEventArgs) Handles MakeButton.Click
        If Not String.IsNullOrEmpty(SelectedDirectoryTextBox.Text) Then

            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           MakeButton.IsEnabled = False
                                           SelectedDirectoryTextBox.IsEnabled = False
                                           BrowseFolderButton.IsEnabled = False
                                       End Sub)
            Else
                MakeButton.IsEnabled = False
                SelectedDirectoryTextBox.IsEnabled = False
                BrowseFolderButton.IsEnabled = False
            End If

            'Check if files should be patched before fake signing
            If PatchSDKCheckBox.IsChecked Then
                'Get selected Base SDK version
                Dim SelectedBaseSDKVersionValue As String = BaseSDKVersionComboBox.Text
                Dim ConvertedBaseSDKVersionValue As UInteger = 0
                Try
                    If SelectedBaseSDKVersionValue.StartsWith("0x") Then
                        ConvertedBaseSDKVersionValue = Convert.ToUInt32(SelectedBaseSDKVersionValue.Substring(2), 16)
                    Else
                        ConvertedBaseSDKVersionValue = Convert.ToUInt32(SelectedBaseSDKVersionValue)
                    End If
                Catch ex As Exception
                    MsgBox("Invalid value for --original_sdk", MsgBoxStyle.Critical)
                    Exit Sub
                End Try

                'Get selected Target SDK version
                Dim SelectedTargetSDKVersionValue As String = TargetSDKVersionComboBox.Text
                Dim ConvertedTargetSDKVersionValue As UInteger = 0
                Try
                    If SelectedTargetSDKVersionValue.StartsWith("0x") Then
                        ConvertedTargetSDKVersionValue = Convert.ToUInt32(SelectedTargetSDKVersionValue.Substring(2), 16)
                    Else
                        ConvertedTargetSDKVersionValue = Convert.ToUInt32(SelectedTargetSDKVersionValue)
                    End If
                Catch ex As Exception
                    MsgBox("Invalid value for --target_sdk")
                    Exit Sub
                End Try

                Dim CreateBackupFiles As Boolean = False
                If CreateBackupCheckBox.IsChecked Then
                    CreateBackupFiles = True
                End If

                If Directory.Exists(SelectedDirectoryTextBox.Text) Then
                    Dim BackupFiles As IEnumerable(Of String) = Directory.GetFiles(SelectedDirectoryTextBox.Text, "*.*", SearchOption.AllDirectories).Where(Function(f) ExecutableExtensions.Contains(Path.GetExtension(f).ToLower()))
                    For Each BackupFilePath In BackupFiles
                        ProcessFile(BackupFilePath, CreateBackupFiles, ConvertedBaseSDKVersionValue, ConvertedTargetSDKVersionValue)
                    Next
                Else
                    MsgBox("The backup folder cannot be found.", MsgBoxStyle.Critical, "Error")
                    Exit Sub
                End If

                MsgBox("Done patching Base SDK Version.", MsgBoxStyle.Information, "Patch SDK")
            End If

            'Collect all files that need to be signed
            Dim FilesToSign As IEnumerable(Of String) = Directory.EnumerateFiles(SelectedDirectoryTextBox.Text, "*.*", SearchOption.AllDirectories).Where(Function(s) s.EndsWith(".prx") OrElse s.EndsWith(".sprx") OrElse s.EndsWith(".elf") OrElse s.EndsWith(".self") OrElse s.EndsWith(".bin"))
            'Fake sign each file with make_fself_python3-1
            For Each FileToSign In FilesToSign
                Dim FileToSignDirectory As String = Path.GetDirectoryName(FileToSign)
                Dim FileToSignFileName As String = Path.GetFileName(FileToSign)
                Dim FileToSignTempFileName As String = FileToSignFileName + ".estemp"
                Dim BackupFileName As String = FileToSignFileName + ".esbak"
                Dim FullTempFilePath As String = Path.Combine(FileToSignDirectory, FileToSignTempFileName)
                Dim BackupFilePath As String = Path.Combine(FileToSignDirectory, BackupFileName)

                Using Make_fSELF As New Process()
                    Make_fSELF.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\make_fself_python3-1.exe"
                    Make_fSELF.StartInfo.Arguments = $"""{FileToSign}"" ""{FullTempFilePath}"""
                    Make_fSELF.StartInfo.RedirectStandardOutput = True
                    Make_fSELF.StartInfo.UseShellExecute = False
                    Make_fSELF.StartInfo.CreateNoWindow = True
                    Make_fSELF.Start()
                    Make_fSELF.WaitForExit()

                    'Read the output
                    Dim OutputReader As StreamReader = Make_fSELF.StandardOutput
                    Dim ProcessOutput As String = OutputReader.ReadToEnd()

                    If Not String.IsNullOrEmpty(ProcessOutput) Then
                        If Dispatcher.CheckAccess() = False Then
                            Dispatcher.BeginInvoke(Sub()
                                                       MakeLogTextBox.AppendText(ProcessOutput + vbCrLf)
                                                       MakeLogTextBox.ScrollToEnd()
                                                   End Sub)
                        Else
                            MakeLogTextBox.AppendText(ProcessOutput + vbCrLf)
                            MakeLogTextBox.ScrollToEnd()
                        End If
                    End If
                End Using

                'Backup original file
                File.Move(FileToSign, BackupFilePath)
            Next

            'Rename the estemp files to their origin file name
            For Each TempFile In Directory.GetFiles(SelectedDirectoryTextBox.Text, "*.estemp", SearchOption.AllDirectories)
                Dim TempFileDirectory As String = Path.GetDirectoryName(TempFile)
                Dim TempFileNameWithoutTempExtension As String = Path.GetFileNameWithoutExtension(TempFile)
                Dim NewFilePath As String = Path.Combine(TempFileDirectory, TempFileNameWithoutTempExtension)
                File.Move(TempFile, NewFilePath)
            Next

            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           MakeButton.IsEnabled = True
                                           SelectedDirectoryTextBox.IsEnabled = True
                                           BrowseFolderButton.IsEnabled = True
                                       End Sub)
            Else
                MakeButton.IsEnabled = True
                SelectedDirectoryTextBox.IsEnabled = True
                BrowseFolderButton.IsEnabled = True
            End If

            MsgBox("SELF files fake signed!", MsgBoxStyle.Information, "Done")
        Else
            MsgBox("No folder selected!", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Function PatchFile(FS As FileStream, OriginalSDKVersion As UInteger, TargetSDKVersion As UInteger) As Boolean
        'Read the segment count (2 bytes at PHT_COUNT_OFFSET)
        Dim PHTCountBuffer(PHT_COUNT_SIZE - 1) As Byte
        FS.Seek(PHT_COUNT_OFFSET, SeekOrigin.Begin)
        FS.ReadExactly(PHTCountBuffer, 0, PHT_COUNT_SIZE)
        Dim SegmentCount As UShort = BitConverter.ToUInt16(PHTCountBuffer, 0)

        'Read the offset to the program header table (8 bytes at PHT_OFFSET_OFFSET)
        Dim PHTOffsetBuffer(PHT_OFFSET_SIZE - 1) As Byte
        FS.Seek(PHT_OFFSET_OFFSET, SeekOrigin.Begin)
        FS.ReadExactly(PHTOffsetBuffer, 0, PHT_OFFSET_SIZE)
        Dim PHTOffset As ULong = BitConverter.ToUInt64(PHTOffsetBuffer, 0)

        'Loop through each segment
        For i As UInteger = 0 To CUInt(SegmentCount - 1)
            Dim EntryOffset As Long = CLng(PHTOffset + i * PHDR_ENTRY_SIZE)

            'Read segment type (4 bytes)
            FS.Seek(EntryOffset + PHDR_TYPE_OFFSET, SeekOrigin.Begin)
            Dim SegmentTypeBuffer(PHDR_TYPE_SIZE - 1) As Byte
            FS.ReadExactly(SegmentTypeBuffer, 0, PHDR_TYPE_SIZE)
            Dim SegmentType As UInteger = BitConverter.ToUInt32(SegmentTypeBuffer, 0)

            'Read segment file offset (8 bytes)
            FS.Seek(EntryOffset + PHDR_OFFSET_OFFSET, SeekOrigin.Begin)
            Dim SegOffsetBuffer(PHDR_OFFSET_SIZE - 1) As Byte
            FS.ReadExactly(SegOffsetBuffer, 0, PHDR_OFFSET_SIZE)
            Dim StructStartOffset As ULong = BitConverter.ToUInt64(SegOffsetBuffer, 0)

            'Read the parameter magic (first 4 bytes at the structure)
            FS.Seek(CLng(StructStartOffset), SeekOrigin.Begin)
            Dim TParamMagicBuffer(3) As Byte
            FS.ReadExactly(TParamMagicBuffer, 0, 4)
            Dim ParamMagic As UInteger = BitConverter.ToUInt32(TParamMagicBuffer, 0)

            'Check magic values depending on segment type
            If SegmentType = PT_SCE_PROCPARAM Then
                If ParamMagic <> SCE_PROCESS_PARAM_MAGIC Then
                    StructStartOffset += CULng(8)
                    FS.Seek(CLng(StructStartOffset), SeekOrigin.Begin)
                    FS.ReadExactly(TParamMagicBuffer, 0, 4)
                    ParamMagic = BitConverter.ToUInt32(TParamMagicBuffer, 0)
                    If ParamMagic <> SCE_PROCESS_PARAM_MAGIC Then
                        If Dispatcher.CheckAccess() = False Then
                            Dispatcher.BeginInvoke(Sub()
                                                       MakeLogTextBox.AppendText("Invalid process param magic" + vbCrLf)
                                                       MakeLogTextBox.ScrollToEnd()
                                                   End Sub)
                        Else
                            MakeLogTextBox.AppendText("Invalid process param magic" + vbCrLf)
                            MakeLogTextBox.ScrollToEnd()
                        End If
                    End If
                End If
            ElseIf SegmentType = PT_SCE_MODULE_PARAM Then
                If ParamMagic <> SCE_MODULE_PARAM_MAGIC Then
                    StructStartOffset += CULng(8)
                    FS.Seek(CLng(StructStartOffset), SeekOrigin.Begin)
                    FS.ReadExactly(TParamMagicBuffer, 0, 4)
                    ParamMagic = BitConverter.ToUInt32(TParamMagicBuffer, 0)
                    If ParamMagic <> SCE_MODULE_PARAM_MAGIC Then
                        If Dispatcher.CheckAccess() = False Then
                            Dispatcher.BeginInvoke(Sub()
                                                       MakeLogTextBox.AppendText("Invalid module param magic for file '" + FS.Name + "', skipping" + vbCrLf)
                                                       MakeLogTextBox.ScrollToEnd()
                                                   End Sub)
                        Else
                            MakeLogTextBox.AppendText("Invalid module param magic for file '" + FS.Name + "', skipping" + vbCrLf)
                            MakeLogTextBox.ScrollToEnd()
                        End If
                        Continue For
                    End If
                End If
            Else
                Continue For
            End If

            'Patch the PS5 SDK version at SCE_PARAM_PS5_SDK_OFFSET
            FS.Seek(CLng(StructStartOffset + SCE_PARAM_PS5_SDK_OFFSET), SeekOrigin.Begin)
            Dim SDKBuffer(3) As Byte
            FS.ReadExactly(SDKBuffer, 0, 4)
            Dim CurrentSDK As UInteger = BitConverter.ToUInt32(SDKBuffer, 0)

            Dim TargetSDKBytes() As Byte = BitConverter.GetBytes(TargetSDKVersion)
            FS.Seek(CLng(StructStartOffset + SCE_PARAM_PS5_SDK_OFFSET), SeekOrigin.Begin)
            FS.Write(TargetSDKBytes, 0, 4)

            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           MakeLogTextBox.AppendText("Patched PS5 SDK version from 0x" + CurrentSDK.ToString("X8") + " to 0x" + TargetSDKVersion.ToString("X8") + " for file '" + FS.Name + "'" + vbCrLf)
                                           MakeLogTextBox.ScrollToEnd()
                                       End Sub)
            Else
                MakeLogTextBox.AppendText("Patched PS5 SDK version from 0x" + CurrentSDK.ToString("X8") + " to 0x" + TargetSDKVersion.ToString("X8") + " for file '" + FS.Name + "'" + vbCrLf)
                MakeLogTextBox.ScrollToEnd()
            End If

            'Patch the base SDK version at SCE_PARAM_BASE_SDK_OFFSET
            FS.Seek(CLng(StructStartOffset + SCE_PARAM_BASE_SDK_OFFSET), SeekOrigin.Begin)
            FS.ReadExactly(SDKBuffer, 0, 4)
            Dim baseSDK As UInteger = BitConverter.ToUInt32(SDKBuffer, 0)
            FS.Seek(CLng(StructStartOffset + SCE_PARAM_BASE_SDK_OFFSET), SeekOrigin.Begin)
            FS.Write(TargetSDKBytes, 0, 4)

            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           MakeLogTextBox.AppendText("Patched base SDK version from 0x" + baseSDK.ToString("X8") + " to 0x" + TargetSDKVersion.ToString("X8") + " for file '" + FS.Name + "'" + vbCrLf)
                                           MakeLogTextBox.ScrollToEnd()
                                       End Sub)
            Else
                MakeLogTextBox.AppendText("Patched base SDK version from 0x" + baseSDK.ToString("X8") + " to 0x" + TargetSDKVersion.ToString("X8") + " for file '" + FS.Name + "'" + vbCrLf)
                MakeLogTextBox.ScrollToEnd()
            End If

            Return True
        Next

        Return False
    End Function

    Private Sub ProcessFile(FilePath As String, CreateBackup As Boolean, OriginalSDKVersion As UInteger, TargetSDKVersion As UInteger)
        Using NewFileStream As New FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite)
            Dim NewBinaryReader As New BinaryReader(NewFileStream)
            NewFileStream.Seek(0, SeekOrigin.Begin)
            Dim FileMagic() As Byte = NewBinaryReader.ReadBytes(4)

            'File checks
            If Not FileMagic.SequenceEqual(ELF_MAGIC) Then
                If FileMagic.SequenceEqual(PS4_FSELF_MAGIC) OrElse FileMagic.SequenceEqual(PS5_FSELF_MAGIC) Then
                    If Dispatcher.CheckAccess() = False Then
                        Dispatcher.BeginInvoke(Sub()
                                                   MakeLogTextBox.AppendText("Aborting, File '" + FilePath + "' is a signed file. This script expects unsigned ELF files." + vbCrLf)
                                                   MakeLogTextBox.ScrollToEnd()
                                               End Sub)
                    Else
                        MakeLogTextBox.AppendText("Aborting, File '" + FilePath + "' is a signed file. This script expects unsigned ELF files." + vbCrLf)
                        MakeLogTextBox.ScrollToEnd()
                    End If
                End If
                Exit Sub
            End If

            NewFileStream.Seek(0, SeekOrigin.Begin)

            'Create backup file of original file
            If CreateBackup AndAlso Not File.Exists(FilePath + ".psbak") Then
                File.Copy(FilePath, FilePath + ".psbak")

                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub()
                                               MakeLogTextBox.AppendText("Backup file created for '" + FilePath + "'" + vbCrLf)
                                               MakeLogTextBox.ScrollToEnd()
                                           End Sub)
                Else
                    MakeLogTextBox.AppendText("Backup file created for '" + FilePath + "'" + vbCrLf)
                    MakeLogTextBox.ScrollToEnd()
                End If
            End If

            'Patch the file
            Dim FilePatched As Boolean = PatchFile(NewFileStream, OriginalSDKVersion, TargetSDKVersion)
            If FilePatched Then
                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub()
                                               MakeLogTextBox.AppendText("Patched '" + FilePath + "'" + vbCrLf)
                                               MakeLogTextBox.ScrollToEnd()
                                           End Sub)
                Else
                    MakeLogTextBox.AppendText("Patched '" + FilePath + "'" + vbCrLf)
                    MakeLogTextBox.ScrollToEnd()
                End If
            Else
                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub()
                                               MakeLogTextBox.AppendText("Failed to patch '" + FilePath + "'" + vbCrLf)
                                               MakeLogTextBox.ScrollToEnd()
                                           End Sub)
                Else
                    MakeLogTextBox.AppendText("Failed to patch '" + FilePath + "'" + vbCrLf)
                    MakeLogTextBox.ScrollToEnd()
                End If
            End If
        End Using
    End Sub

    Private Function GetDirSize(RootFolder As String) As Long
        Dim FolderInfo = New DirectoryInfo(RootFolder)
        For Each File In FolderInfo.GetFiles
            TotalSize += File.Length
        Next
        For Each SubFolderInfo In FolderInfo.GetDirectories : GetDirSize(SubFolderInfo.FullName)
        Next
        Return TotalSize
    End Function

    Private Sub RestoreOriginalBackup(BackupPath As String)
        'Get already patched files & delete them
        Dim FilesToDelete As IEnumerable(Of String) = Directory.EnumerateFiles(SelectedDirectoryTextBox.Text, "*.*", SearchOption.AllDirectories).Where(Function(s) s.EndsWith(".prx") OrElse s.EndsWith(".elf") OrElse s.EndsWith(".bin") OrElse s.EndsWith(".sprx") OrElse s.EndsWith(".self"))
        For Each FileToDelete In FilesToDelete
            File.Delete(FileToDelete)
        Next

        'Get backup files & restore them
        Dim FilesToRecover As IEnumerable(Of String) = Directory.EnumerateFiles(SelectedDirectoryTextBox.Text, "*.*", SearchOption.AllDirectories).Where(Function(s) s.EndsWith(".esbak"))
        For Each FileToRecover In FilesToRecover
            Dim NewFilePath As String = Path.Combine(Path.GetDirectoryName(FileToRecover), Path.GetFileNameWithoutExtension(FileToRecover))
            File.Move(FileToRecover, NewFilePath, True)
        Next

        AlreadyPatched = False
        MsgBox("Backup files have been restored. The selected backup can now be patched and/or faked signed again.", MsgBoxStyle.Information, "Done recovering")
    End Sub

    Private Sub PatchSDKCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles PatchSDKCheckBox.Checked
        If Not String.IsNullOrEmpty(SelectedDirectoryTextBox.Text) Then
            If AlreadyPatched Then
                If MsgBox("This backup has been already modified and is probably patched." + vbCrLf + "Do you want to restore the original backup files before patching this backup again ? (Recommended)", MsgBoxStyle.YesNoCancel, "PS Multi Tools") = MsgBoxResult.Yes Then
                    RestoreOriginalBackup(SelectedDirectoryTextBox.Text)
                End If
            End If
        Else
            'Reset AlreadyPatched when SelectedDirectoryTextBox.Text is empty and PatchSDK got checked
            AlreadyPatched = False
        End If
    End Sub

End Class
