Imports System.ComponentModel
Imports System.IO
Imports System.Security.Cryptography
Imports System.Threading

Public Class PS3PKGExtractor

    Public SelectedPKG As String

    Dim WithEvents ImageWorker As New BackgroundWorker
    Dim WithEvents DecryptWorker As New BackgroundWorker With {.WorkerReportsProgress = True}
    Dim WithEvents ExtractionWorker As New BackgroundWorker With {.WorkerReportsProgress = True}

    Dim PSPAesKey As Byte() = New Byte(15) {7, 242, 198, 130, 144, 181, 13, 44, 51, 129, 141, 112, 155, 96, 230, 43}
    Dim PS3AesKey As Byte() = New Byte(15) {46, 123, 113, 215, 201, 201, 161, 78, 163, 34, 31, 24, 136, 40, 184, 248}
    Dim AESKey As Byte() = New Byte(15) {}
    Dim PKGFileKey As Byte() = New Byte(15) {}
    Dim UIEncryptedFileStartOffset As UInteger = 0
    Dim DecryptedPKG As String

    Private Sub PS3PKGExtractor_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Not String.IsNullOrEmpty(SelectedPKG) Then
            ExtractProgressTextBlock.Text = "Getting ICON0 from PKG ..."
            ImageWorker.RunWorkerAsync()
        End If
    End Sub

    Private Sub LockUI()
        If SelectedPKGFileTextBox.IsEnabled Then
            SelectedPKGFileTextBox.IsEnabled = False
            BrowsePKGButton.IsEnabled = False
            ExtractButton.IsEnabled = False
        Else
            SelectedPKGFileTextBox.IsEnabled = True
            BrowsePKGButton.IsEnabled = True
            ExtractButton.IsEnabled = True
        End If
    End Sub

    Private Sub BrowsePKGButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePKGButton.Click
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Select a PKG file", .Filter = "PKG files (*.pkg)|*.pkg", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPKGFileTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub ExtractButton_Click(sender As Object, e As RoutedEventArgs) Handles ExtractButton.Click
        If Not String.IsNullOrEmpty(SelectedPKGFileTextBox.Text) And File.Exists(SelectedPKGFileTextBox.Text) Then
            SelectedPKG = SelectedPKGFileTextBox.Text
            ExtractProgressTextBlock.Text = "Getting ICON0 from PKG ..."
            LockUI()
            ImageWorker.RunWorkerAsync()
        End If
    End Sub

    Private Sub ImageWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles ImageWorker.DoWork
        Dim NewPKGDecryptor As New PKGDecryptor()

        NewPKGDecryptor.ProcessPKGFile(SelectedPKG)

        If NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.ICON0) IsNot Nothing Then
            e.Result = NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.ICON0)
        Else
            e.Result = Nothing
        End If
    End Sub

    Private Sub ImageWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles ImageWorker.RunWorkerCompleted
        Dim ICONFromBGWorker As BitmapSource = CType(e.Result, BitmapSource)
        PKGICONImage.Source = ICONFromBGWorker
        ImageWorker.Dispose()

        ExtractProgressTextBlock.Text = "Decrypting PKG ..."
        DecryptWorker.RunWorkerAsync(New Structures.ExtractionPKGProcess With {.PKGFileName = SelectedPKG})
    End Sub

    Private Sub DecryptWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles DecryptWorker.DoWork
        Dim Args As Structures.ExtractionPKGProcess = CType(e.Argument, Structures.ExtractionPKGProcess)

        Try
            Dim Multiplicator As Integer = 65536
            Dim EncryptedData As Byte() = New Byte(AESKey.Length * Multiplicator - 1) {}
            Dim DecryptedData As Byte() = New Byte(AESKey.Length * Multiplicator - 1) {}

            Dim PKGXorKey As Byte() = New Byte(AESKey.Length - 1) {}
            Dim EncryptedFileStartOffset As Byte() = New Byte(3) {}
            Dim EncryptedFileLenght As Byte() = New Byte(3) {}

            Using PKGReadStream As Stream = New FileStream(Args.PKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using brPKG As New BinaryReader(PKGReadStream)

                    PKGReadStream.Seek(&H0, SeekOrigin.Begin)

                    Dim pkgMagic As Byte() = brPKG.ReadBytes(4)
                    If pkgMagic(&H0) <> &H7F OrElse pkgMagic(&H1) <> &H50 OrElse pkgMagic(&H2) <> &H4B OrElse pkgMagic(&H3) <> &H47 Then
                        e.Cancel = True
                    End If

                    PKGReadStream.Seek(&H4, SeekOrigin.Begin)

                    Dim pkgFinalized As Byte = brPKG.ReadByte()
                    If pkgFinalized <> 128 Then
                        e.Cancel = True
                    End If

                    PKGReadStream.Seek(&H7, SeekOrigin.Begin)

                    Dim pkgType As Byte = brPKG.ReadByte()
                    Select Case pkgType
                        Case &H1
                            'PS3
                            AESKey = PS3AesKey
                            Exit Select
                        Case &H2
                            'PSP
                            AESKey = PSPAesKey
                            Exit Select
                        Case Else
                            e.Cancel = True
                    End Select

                    PKGReadStream.Seek(&H24, SeekOrigin.Begin)
                    EncryptedFileStartOffset = brPKG.ReadBytes(EncryptedFileStartOffset.Length)
                    Array.Reverse(EncryptedFileStartOffset)
                    UIEncryptedFileStartOffset = BitConverter.ToUInt32(EncryptedFileStartOffset, 0)

                    PKGReadStream.Seek(&H2C, SeekOrigin.Begin)
                    EncryptedFileLenght = brPKG.ReadBytes(EncryptedFileLenght.Length)
                    Array.Reverse(EncryptedFileLenght)
                    Dim uiEncryptedFileLenght As UInteger = BitConverter.ToUInt32(EncryptedFileLenght, 0)

                    PKGReadStream.Seek(&H70, SeekOrigin.Begin)
                    PKGFileKey = brPKG.ReadBytes(16)
                    Dim incPKGFileKey As Byte() = New Byte(15) {}
                    Array.Copy(PKGFileKey, incPKGFileKey, PKGFileKey.Length)

                    PKGXorKey = AESEngine.Encrypt(PKGFileKey, AESKey, AESKey, CipherMode.ECB, PaddingMode.None)

                    Dim division As Double = uiEncryptedFileLenght / AESKey.Length
                    Dim pieces As ULong = Convert.ToUInt64(Math.Floor(division))
                    Dim Modi As ULong = CULng(Convert.ToUInt64(uiEncryptedFileLenght) / Convert.ToUInt64(AESKey.Length))
                    If Modi > 0 Then
                        pieces += CULng(1)
                    End If

                    If File.Exists(Args.PKGFileName & ".DEC") Then
                        File.Delete(Args.PKGFileName & ".DEC")
                    End If

                    Dim DecryptedFileWriteStream As New FileStream(Args.PKGFileName & ".DEC", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite)
                    Dim bwDecryptedFile As New BinaryWriter(DecryptedFileWriteStream)

                    PKGReadStream.Seek(UIEncryptedFileStartOffset, SeekOrigin.Begin)

                    Dim filedivision As Double = uiEncryptedFileLenght / (AESKey.Length * Multiplicator)
                    Dim filepieces As ULong = Convert.ToUInt64(Math.Floor(filedivision))
                    Dim filemod As ULong = Convert.ToUInt64(uiEncryptedFileLenght) Mod Convert.ToUInt64(AESKey.Length * Multiplicator)
                    If filemod > 0 Then
                        filepieces += CULng(1)
                    End If

                    DecryptWorker.ReportProgress(0, New Structures.ExtractionWorkerProgress With {.FileCount = CInt(filepieces) - 1, .FileName = ""}) 'Report FileCount

                    For i As ULong = 0 To CULng(filepieces - 1)
                        'If we have a mod and this is the last piece then...
                        If (filemod > 0) AndAlso (i = (filepieces - 1)) Then
                            EncryptedData = New Byte(CInt(filemod - 1)) {}
                            DecryptedData = New Byte(CInt(filemod - 1)) {}
                        End If

                        'Read 16 bytes of Encrypted data
                        EncryptedData = brPKG.ReadBytes(EncryptedData.Length)

                        'In order to retrieve a fast AES Encryption we pre-Increment the array
                        Dim PKGFileKeyConsec As Byte() = New Byte(EncryptedData.Length - 1) {}
                        Dim PKGXorKeyConsec As Byte() = New Byte(EncryptedData.Length - 1) {}

                        Dim pos As Integer = 0
                        While pos < EncryptedData.Length
                            Array.Copy(incPKGFileKey, 0, PKGFileKeyConsec, pos, PKGFileKey.Length)

                            Utils.IncrementArray(incPKGFileKey, PKGFileKey.Length - 1)
                            pos += AESKey.Length
                        End While

                        PKGXorKeyConsec = AESEngine.Encrypt(PKGFileKeyConsec, AESKey, AESKey, CipherMode.ECB, PaddingMode.None)
                        DecryptedData = XOREngine.GetXOR(EncryptedData, 0, PKGXorKeyConsec.Length, PKGXorKeyConsec)
                        DecryptWorker.ReportProgress(1)
                        bwDecryptedFile.Write(DecryptedData)
                    Next

                    DecryptedFileWriteStream.Close()
                    bwDecryptedFile.Close()
                End Using
            End Using

            e.Result = Args.PKGFileName & ".DEC"
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Sub

    Private Sub DecryptWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles DecryptWorker.RunWorkerCompleted
        DecryptedPKG = e.Result.ToString
        ExtractProgressBar.Value = 0
        DecryptWorker.Dispose()

        ExtractProgressTextBlock.Text = "Extracting PKG ..."
        If (DecryptedPKG IsNot Nothing) AndAlso (DecryptedPKG <> String.Empty) Then
            ExtractionWorker.RunWorkerAsync(New Structures.ExtractionPKGProcess With {.DecryptedPKGFileName = DecryptedPKG, .EncryptedPKGFileName = SelectedPKG})
        End If
    End Sub

    Private Sub DecryptWorker_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles DecryptWorker.ProgressChanged
        If e.ProgressPercentage = 0 Then
            Dim Progr As Structures.ExtractionWorkerProgress = CType(e.UserState, Structures.ExtractionWorkerProgress)
            ExtractProgressBar.Maximum = Progr.FileCount
        End If
        ExtractProgressBar.Value += e.ProgressPercentage
    End Sub

    Private Sub ExtractionWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles ExtractionWorker.DoWork
        Dim Args As Structures.ExtractionPKGProcess = CType(e.Argument, Structures.ExtractionPKGProcess)

        Try
            Dim twentyMb As Integer = 1024 * 1024 * 20
            Dim ExtractedFileOffset As UInteger = 0
            Dim ExtractedFileSize As UInteger = 0
            Dim OffsetShift As UInteger = 0
            Dim positionIdx As Long = 0
            Dim WorkDir As String = ""

            WorkDir = Args.DecryptedPKGFileName & ".EXT"

            If Directory.Exists(WorkDir) Then
                Directory.Delete(WorkDir, True)
                Thread.Sleep(100)
                Directory.CreateDirectory(WorkDir)
                Thread.Sleep(100)
            End If

            Dim FileTable As Byte() = New Byte(319999) {}
            Dim dumpFile As Byte()
            Dim sdkVer As Byte() = New Byte(7) {}
            Dim firstFileOffset As Byte() = New Byte(3) {}
            Dim firstNameOffset As Byte() = New Byte(3) {}
            Dim fileNr As Byte() = New Byte(3) {}
            Dim isDir As Byte() = New Byte(3) {}
            Dim Offset As Byte() = New Byte(3) {}
            Dim Size As Byte() = New Byte(3) {}
            Dim NameOffset As Byte() = New Byte(3) {}
            Dim NameSize As Byte() = New Byte(3) {}
            Dim Name As Byte() = New Byte(31) {}
            Dim bootMagic As Byte() = New Byte(7) {}
            Dim contentType As Byte = 0
            Dim fileType As Byte = 0
            Dim isFile As Boolean = False

            Dim decrPKGReadStream As Stream = New FileStream(Args.DecryptedPKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Dim brDecrPKG As New BinaryReader(decrPKGReadStream)

            Dim encrPKGReadStream As Stream = New FileStream(Args.EncryptedPKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Dim brEncrPKG As New BinaryReader(encrPKGReadStream)

            'Read the file Table
            decrPKGReadStream.Seek(0, SeekOrigin.Begin)
            FileTable = brDecrPKG.ReadBytes(FileTable.Length)

            positionIdx = 0
            OffsetShift = 0

            'Shift Relative to os.raw
            Array.Copy(FileTable, 0, firstNameOffset, 0, firstNameOffset.Length)
            Array.Reverse(firstNameOffset)
            Dim uifirstNameOffset As UInteger = BitConverter.ToUInt32(firstNameOffset, 0)

            Dim uiFileNr As UInteger = CUInt(uifirstNameOffset \ 32)

            Array.Copy(FileTable, 12, firstFileOffset, 0, firstFileOffset.Length)
            Array.Reverse(firstFileOffset)
            Dim uifirstFileOffset As UInteger = BitConverter.ToUInt32(firstFileOffset, 0)

            'Read the file Table
            decrPKGReadStream.Seek(0, SeekOrigin.Begin)
            FileTable = brDecrPKG.ReadBytes(CInt(uifirstFileOffset))

            'If number of files is negative then something is wrong...
            If CInt(uiFileNr) < 0 Then
                e.Cancel = True
            End If

            ExtractionWorker.ReportProgress(0, New Structures.ExtractionWorkerProgress With {.FileCount = CInt(uiFileNr) - 1, .FileName = ""}) 'Report FileCount

            'Table:
            '0-3         4-7         8-11        12-15       16-19       20-23       24-27       28-31
            '|name loc | |name size| |   NULL  | |file loc | |  NULL   | |file size| |cont type| |   NULL  |

            For ii As Integer = 0 To CInt(uiFileNr) - 1
                Array.Copy(FileTable, positionIdx + 12, Offset, 0, Offset.Length)
                Array.Reverse(Offset)
                ExtractedFileOffset = BitConverter.ToUInt32(Offset, 0) + OffsetShift

                Array.Copy(FileTable, positionIdx + 20, Size, 0, Size.Length)
                Array.Reverse(Size)
                ExtractedFileSize = BitConverter.ToUInt32(Size, 0)

                Array.Copy(FileTable, positionIdx, NameOffset, 0, NameOffset.Length)
                Array.Reverse(NameOffset)
                Dim ExtractedFileNameOffset As UInteger = BitConverter.ToUInt32(NameOffset, 0)

                Array.Copy(FileTable, positionIdx + 4, NameSize, 0, NameSize.Length)
                Array.Reverse(NameSize)
                Dim ExtractedFileNameSize As UInteger = BitConverter.ToUInt32(NameSize, 0)

                contentType = FileTable(CInt(positionIdx + 24))
                fileType = FileTable(CInt(positionIdx + 27))

                Name = New Byte(CInt(ExtractedFileNameSize - 1)) {}
                Array.Copy(FileTable, ExtractedFileNameOffset, Name, 0, ExtractedFileNameSize)
                Dim ExtractedFileName As String = Utils.ByteArrayToAscii(Name, 0, Name.Length, True)

                'Write Directory
                If Not Directory.Exists(WorkDir) Then
                    Directory.CreateDirectory(WorkDir)
                    Thread.Sleep(100)
                End If

                Dim ExtractedFileWriteStream As FileStream = Nothing

                'File / Directory
                If (fileType = &H4) AndAlso (ExtractedFileSize = &H0) Then
                    isFile = False
                Else
                    isFile = True
                End If

                'contentType == 0x90 = PSP file/dir
                If contentType = &H90 Then
                    Dim FileDir As String = WorkDir & "\" & ExtractedFileName
                    FileDir = FileDir.Replace("/", "\")
                    Dim FileDirectory As DirectoryInfo = Directory.GetParent(FileDir)

                    If Not Directory.Exists(FileDirectory.ToString()) Then
                        Directory.CreateDirectory(FileDirectory.ToString())
                    End If
                    ExtractedFileWriteStream = New FileStream(FileDir, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite)
                Else
                    'contentType == (0x80 || 0x00) = PS3 file/dir
                    'fileType == 0x01 = NPDRM File
                    'fileType == 0x03 = Raw File
                    'fileType == 0x04 = Directory

                    'Decrypt PS3 Filename
                    Dim DecryptedData As Byte() = DecryptData(CInt(ExtractedFileNameSize), ExtractedFileNameOffset, UIEncryptedFileStartOffset, PS3AesKey, encrPKGReadStream, brEncrPKG)
                    Array.Copy(DecryptedData, 0, Name, 0, ExtractedFileNameSize)
                    ExtractedFileName = Utils.ByteArrayToAscii(Name, 0, Name.Length, True)

                    If Not isFile Then
                        'Directory
                        Try
                            If Not Directory.Exists(ExtractedFileName) Then
                                Directory.CreateDirectory(WorkDir & "\" & ExtractedFileName)
                            End If
                        Catch ex As Exception
                            'This should not happen
                            ExtractedFileName = ii.ToString() & ".raw"
                            If Not Directory.Exists(ExtractedFileName) Then
                                Directory.CreateDirectory(WorkDir & "\" & ExtractedFileName)
                            End If
                        End Try
                    Else
                        'File
                        Try
                            ExtractedFileWriteStream = New FileStream(WorkDir & "\" & ExtractedFileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite)
                        Catch ex As Exception
                            'This should not happen
                            ExtractedFileName = ii.ToString() & ".raw"
                            ExtractedFileWriteStream = New FileStream(WorkDir & "\" & ExtractedFileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite)
                        End Try
                    End If
                End If

                If contentType = 144 AndAlso isFile Then
                    'Read/Write File
                    Dim ExtractedFile As New BinaryWriter(ExtractedFileWriteStream)
                    decrPKGReadStream.Seek(ExtractedFileOffset, SeekOrigin.Begin)

                    ' Pieces calculation
                    Dim division As Double = ExtractedFileSize / twentyMb
                    Dim pieces As ULong = Convert.ToUInt64(Math.Floor(division))
                    Dim Modi As ULong = Convert.ToUInt64(ExtractedFileSize) Mod Convert.ToUInt64(twentyMb)
                    If Modi > 0 Then
                        pieces += CULng(1)
                    End If

                    dumpFile = New Byte(twentyMb - 1) {}
                    For i As ULong = 0 To CULng(pieces - 1)
                        'If we have a mod and this is the last piece then...
                        If (Modi > 0) AndAlso (i = (pieces - 1)) Then
                            dumpFile = New Byte(CInt(Modi - 1)) {}
                        End If

                        'Fill buffer
                        brDecrPKG.Read(dumpFile, 0, dumpFile.Length)
                        ExtractedFile.Write(dumpFile)
                    Next

                    ExtractedFileWriteStream.Close()
                    ExtractedFile.Close()
                End If

                If contentType <> &H90 AndAlso isFile Then
                    'Read/Write File
                    Dim ExtractedFile As New BinaryWriter(ExtractedFileWriteStream)
                    decrPKGReadStream.Seek(ExtractedFileOffset, SeekOrigin.Begin)

                    ' Pieces calculation
                    Dim division As Double = ExtractedFileSize / twentyMb

                    Dim pieces As ULong = Convert.ToUInt64(Math.Floor(division))
                    Dim Modi As ULong = Convert.ToUInt64(ExtractedFileSize) Mod Convert.ToUInt64(twentyMb)
                    If Modi > 0 Then
                        pieces += CULng(1)
                    End If

                    dumpFile = New Byte(twentyMb - 1) {}
                    Dim elapsed As Long = 0
                    For i As ULong = 0 To CULng(pieces - 1)
                        'If we have a mod and this is the last piece then
                        If (Modi > 0) AndAlso (i = (pieces - 1)) Then
                            dumpFile = New Byte(CInt(Modi - 1)) {}
                        End If

                        'Fill buffer
                        Dim DecryptedData As Byte() = DecryptData(dumpFile.Length, ExtractedFileOffset + elapsed, UIEncryptedFileStartOffset, PS3AesKey, encrPKGReadStream, brEncrPKG)
                        elapsed = +dumpFile.Length

                        'To avoid decryption pad we use dumpFile.Length that's the actual decrypted file size!
                        ExtractedFile.Write(DecryptedData, 0, dumpFile.Length)
                    Next

                    ExtractedFileWriteStream.Close()
                    ExtractedFile.Close()
                End If

                positionIdx += 32
                ExtractionWorker.ReportProgress(1)
            Next

            'Close Filestreams
            encrPKGReadStream.Close()
            brEncrPKG.Close()

            decrPKGReadStream.Close()
            brDecrPKG.Close()

            'Delete decrypted file
            If File.Exists(Args.DecryptedPKGFileName) Then
                File.Delete(Args.DecryptedPKGFileName)
            End If

        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Sub

    Private Sub ExtractionWorker_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles ExtractionWorker.ProgressChanged
        If e.ProgressPercentage = 0 Then
            Dim Progr As Structures.ExtractionWorkerProgress = CType(e.UserState, Structures.ExtractionWorkerProgress)
            ExtractProgressBar.Maximum = Progr.FileCount
        End If
        ExtractProgressBar.Value += e.ProgressPercentage
    End Sub

    Private Sub ExtractionWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles ExtractionWorker.RunWorkerCompleted
        ExtractionWorker.Dispose()
        ExtractProgressTextBlock.Text = "PKG extracted!"

        Dim FiInfo As New FileInfo(SelectedPKG)
        Dim OutputDir As String = FiInfo.DirectoryName + "\" + FiInfo.Name + ".DEC.EXT"

        LockUI()

        If MsgBox("PKG extracted!" + vbCrLf + "Open folder ?", MsgBoxStyle.OkCancel, "Done") = MsgBoxResult.Ok Then
            Process.Start("explorer", OutputDir)
            Close()
        Else
            Close()
        End If
    End Sub

    Public Function DecryptData(dataSize As Integer, dataRelativeOffset As Long, pkgEncryptedFileStartOffset As Long, AesKey As Byte(), encrPKGReadStream As Stream, brEncrPKG As BinaryReader) As Byte()
        Dim size As Integer = dataSize Mod 16
        If size > 0 Then
            size = ((dataSize \ 16) + 1) * 16
        Else
            size = dataSize
        End If

        Dim PKGFileKeyConsec As Byte() = New Byte(size - 1) {}
        Dim incPKGFileKey As Byte() = New Byte(PKGFileKey.Length - 1) {}
        Array.Copy(PKGFileKey, incPKGFileKey, PKGFileKey.Length)

        encrPKGReadStream.Seek(dataRelativeOffset + pkgEncryptedFileStartOffset, SeekOrigin.Begin)

        For pos As Integer = 0 To CInt(dataRelativeOffset - 1) Step 16
            Utils.IncrementArray(incPKGFileKey, PKGFileKey.Length - 1)
        Next

        For pos As Integer = 0 To size - 1 Step 16
            Array.Copy(incPKGFileKey, 0, PKGFileKeyConsec, pos, PKGFileKey.Length)
            Utils.IncrementArray(incPKGFileKey, PKGFileKey.Length - 1)
        Next

        Dim EncryptedData As Byte() = brEncrPKG.ReadBytes(size)
        Dim PKGXorKeyConsec As Byte() = AESEngine.Encrypt(PKGFileKeyConsec, AesKey, AesKey, CipherMode.ECB, PaddingMode.None)
        Dim DecryptedData As Byte() = XOREngine.GetXOR(EncryptedData, 0, PKGXorKeyConsec.Length, PKGXorKeyConsec)

        Return DecryptedData
    End Function

End Class
