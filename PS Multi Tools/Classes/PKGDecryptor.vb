Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

Public Class PKGDecryptor

    Private PARAMSFO As Byte()
    Private ICON0 As BitmapSource
    Private PIC0 As BitmapSource
    Private PIC1 As BitmapSource
    Private PIC2 As BitmapSource
    Private SND0 As Byte()
    Private PBPBytes As Byte()
    Private PackageType As PKGType
    Private PKGContentID As String
    Public isDecError As Boolean
    Public Shared PSPAesKey As Byte() = New Byte(15) {7, 242, 198, 130, 144, 181, 13, 44, 51, 129, 141, 112, 155, 96, 230, 43}
    Public Shared PS3AesKey As Byte() = New Byte(15) {46, 123, 113, 215, 201, 201, 161, 78, 163, 34, 31, 24, 136, 40, 184, 248}
    Public Shared AesKey As Byte() = New Byte(15) {}
    Public Shared PKGFileKey As Byte() = New Byte(15) {}
    Public Shared uiEncryptedFileStartOffset As UInteger = 0
    Public IsSupportedFiles As Boolean

    Public Sub New()
        PARAMSFO = New Byte(524288) {}
        ICON0 = Nothing
        PIC0 = Nothing
        PIC1 = Nothing
        PIC2 = Nothing
        SND0 = Nothing
        PBPBytes = New Byte(5242880) {}
        PackageType = New PKGType()
        PKGContentID = String.Empty
        isDecError = False
        IsSupportedFiles = False
    End Sub

    Public Enum PKGFiles
        ICON0
        PIC0
        PIC1
        PIC2
        SND0
    End Enum

    Public Enum PKGType
        Debug
        Retail
        Retail_PSX_PSP
    End Enum

    Public Function GetImage(PKGIMG As PKGFiles) As BitmapSource
        Select Case PKGIMG
            Case PKGFiles.ICON0
                Return ICON0
            Case PKGFiles.PIC0
                Return PIC0
            Case PKGFiles.PIC1
                Return PIC1
            Case PKGFiles.PIC2
                Return PIC2
            Case Else
                Return Nothing
        End Select
    End Function

    Public ReadOnly Property GetPARAMSFO As Byte()
        Get
            Return PARAMSFO
        End Get
    End Property

    Public ReadOnly Property GetSND As Byte()
        Get
            Return SND0
        End Get
    End Property

    Public ReadOnly Property GetPBPBytes As Byte()
        Get
            Return PBPBytes
        End Get
    End Property

    Public ReadOnly Property GetPKGType As PKGType
        Get
            Return PackageType
        End Get
    End Property

    Public ReadOnly Property ContentID As String
        Get
            Return PKGContentID
        End Get
    End Property

    Public Sub ProcessPKGFile(PKGFile As String)
        Dim DecryptedPKG As Byte() = DecryptPKGFileRead(PKGFile)
        Dim EncryptedPKG As Byte() = GetBytesFromFile(PKGFile)
        If DecryptedPKG IsNot Nothing Then
            ExtractPKGFilesRead(DecryptedPKG, EncryptedPKG)
        End If
    End Sub

    Public Function DecryptPKGFileRead(PKGFileName As String) As Byte()
        Try
            Dim moltiplicator As Integer = 65536
            Dim numArray1 As Byte() = New Byte(1048576 - 1 + 1 - 1) {}
            Dim EncryptedData As Byte() = New Byte(AesKey.Length * moltiplicator - 1) {}
            Dim DecryptedData As Byte() = New Byte(AesKey.Length * moltiplicator - 1) {}
            Dim PKGXorKey As Byte() = New Byte(AesKey.Length - 1) {}
            Dim EncryptedFileStartOffset As Byte() = New Byte(3) {}
            Dim EncryptedFileLenght As Byte() = New Byte(3) {}

            Using PKGReadStream As New FileStream(PKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using brPKG As New BinaryReader(PKGReadStream)

                    Dim pkgMagic As Byte() = brPKG.ReadBytes(4)
                    If pkgMagic(0) <> 127 OrElse pkgMagic(1) <> 80 OrElse pkgMagic(2) <> 75 OrElse pkgMagic(3) <> 71 Then
                        'Selected file isn't a Pkg file. Error!
                    End If

                    'Finalized byte
                    PKGReadStream.Seek(4, SeekOrigin.Begin)
                    Dim pkgFinalized As Byte = brPKG.ReadByte()

                    If pkgFinalized <> 128 Then
                        'This is debug PKG and is not supported!
                    End If

                    PKGReadStream.Seek(48, SeekOrigin.Begin)
                    PKGContentID = Encoding.ASCII.GetString(brPKG.ReadBytes(36))

                    'PKG Type PSP/PS3
                    PKGReadStream.Seek(7, SeekOrigin.Begin)
                    Dim pkgType As Byte = brPKG.ReadByte()

                    Select Case pkgType
                        Case 1
                            'PS3
                            AesKey = PS3AesKey
                            Exit Select
                        Case 2
                            'PSP
                            AesKey = PSPAesKey
                            Exit Select
                        Case Else
                            'Invalid PKG file
                    End Select

                    PKGReadStream.Seek(36, SeekOrigin.Begin)
                    EncryptedFileStartOffset = brPKG.ReadBytes(EncryptedFileStartOffset.Length)
                    Array.Reverse(EncryptedFileStartOffset)
                    uiEncryptedFileStartOffset = BitConverter.ToUInt32(EncryptedFileStartOffset, 0)

                    PKGReadStream.Seek(44, SeekOrigin.Begin)
                    EncryptedFileLenght = brPKG.ReadBytes(EncryptedFileLenght.Length)
                    Array.Reverse(EncryptedFileLenght)
                    Dim uiEncryptedFileLenght As UInteger = BitConverter.ToUInt32(EncryptedFileLenght, 0)

                    PKGReadStream.Seek(112, SeekOrigin.Begin)
                    PKGFileKey = brPKG.ReadBytes(16)
                    Dim incPKGFileKey As Byte() = New Byte(15) {}
                    Array.Copy(PKGFileKey, incPKGFileKey, PKGFileKey.Length)

                    PKGXorKey = AESEngine.Encrypt(PKGFileKey, AesKey, AesKey, CipherMode.ECB, PaddingMode.None)

                    Dim division As Double = uiEncryptedFileLenght / AesKey.Length
                    Dim pieces As ULong = Convert.ToUInt64(Math.Floor(division))
                    Dim CustomMod As ULong = Convert.ToUInt64(uiEncryptedFileLenght) / Convert.ToUInt64(AesKey.Length)
                    If CustomMod > 0 Then
                        pieces += 1
                    End If

                    Using bwDecryptedFile As New MemoryStream()
                        PKGReadStream.Seek(uiEncryptedFileStartOffset, SeekOrigin.Begin)

                        Dim filedivision As Double = uiEncryptedFileLenght / (AesKey.Length * moltiplicator)
                        Dim filepieces As ULong = Convert.ToUInt64(Math.Floor(filedivision))
                        Dim filemod As ULong = Convert.ToUInt64(uiEncryptedFileLenght) Mod Convert.ToUInt64(AesKey.Length * moltiplicator)
                        If filemod > 0 Then
                            filepieces += 1
                        End If

                        Dim muint64 As ULong = Convert.ToUInt64(Decimal.Subtract(New Decimal(filedivision), 1D))
                        Dim num7 As ULong = 0
                        If num7 <= muint64 Then
                            If (filemod > 0) AndAlso (num7 = (filepieces - 1)) Then
                                EncryptedData = New Byte(filemod - 1) {}
                                DecryptedData = New Byte(filemod - 1) {}
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
                                pos += AesKey.Length
                            End While

                            PKGXorKeyConsec = AESEngine.Encrypt(PKGFileKeyConsec, AesKey, AesKey, CipherMode.ECB, PaddingMode.None)
                            DecryptedData = XOREngine.GetXOR(EncryptedData, 0, PKGXorKeyConsec.Length, PKGXorKeyConsec)
                            bwDecryptedFile.Write(DecryptedData, 0, DecryptedData.Length)

                            If DecryptedData.Length >= 1048576 Then
                                numArray1 = bwDecryptedFile.ToArray()
                            End If
                        End If

                        'For i As ULong = 0 To filepieces - 1

                        ' Next

                    End Using
                End Using
            End Using

            Return numArray1
        Catch ex As Exception
            'Could not read decrypt PKG file.
        End Try
    End Function

    Public Function DecryptPKGDataRead(dataSize As Integer, dataRelativeOffset As Long, pkgEncryptedFileStartOffset As Long, AesKey As Byte(), encrPKGReadStream As Stream, brEncrPKG As Stream) As Byte()
        Try
            Dim size As Integer = dataSize Mod 16
            If size > 0 Then
                size = ((dataSize \ 16) + 1) * 16
            Else
                size = dataSize
            End If

            Dim EncryptedData As Byte() = New Byte(size - 1) {}
            Dim DecryptedData As Byte() = New Byte(size - 1) {}
            Dim PKGFileKeyConsec As Byte() = New Byte(size - 1) {}
            Dim PKGXorKeyConsec As Byte() = New Byte(size - 1) {}
            Dim incPKGFileKey As Byte() = New Byte(PKGFileKey.Length - 1) {}
            Array.Copy(PKGFileKey, incPKGFileKey, PKGFileKey.Length)

            encrPKGReadStream.Seek(dataRelativeOffset + pkgEncryptedFileStartOffset, SeekOrigin.Begin)
            brEncrPKG.Read(EncryptedData, 0, size)

            For pos As Integer = 0 To dataRelativeOffset - 1 Step 16
                Utils.IncrementArray(incPKGFileKey, PKGFileKey.Length - 1)
            Next

            For pos As Integer = 0 To size - 1 Step 16
                Array.Copy(incPKGFileKey, 0, PKGFileKeyConsec, pos, PKGFileKey.Length)
                Utils.IncrementArray(incPKGFileKey, PKGFileKey.Length - 1)
            Next

            PKGXorKeyConsec = AESEngine.Encrypt(PKGFileKeyConsec, AesKey, AesKey, CipherMode.ECB, PaddingMode.None)
            DecryptedData = XOREngine.GetXOR(EncryptedData, 0, PKGXorKeyConsec.Length, PKGXorKeyConsec)
            Return DecryptedData
        Catch ex As Exception
            MsgBox("Could not decrypt PKG data.", MsgBoxStyle.Critical, "Error")
            Return Nothing
        End Try
    End Function

    Public Function ExtractPKGFilesRead(decryptedPKGFileName As Byte(), encryptedPKGFileName As Byte()) As Boolean
        Try
            Dim twentyMb As Integer = 20971520
            Dim ExtractedFileOffset As UInteger = 0
            Dim ExtractedFileSize As UInteger = 0
            Dim OffsetShift As UInteger = 0
            Dim positionIdx As Long = 0
            Dim FileTable As Byte() = New Byte(319999) {}
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
            Dim dumpFile As Byte()
            Dim decrPKGReadStream As New MemoryStream(decryptedPKGFileName)
            Dim brDecrPKG As MemoryStream = decrPKGReadStream
            Dim encrPKGReadStream As New MemoryStream(encryptedPKGFileName)
            Dim brEncrPKG As MemoryStream = encrPKGReadStream

            'Read the file table
            decrPKGReadStream.Seek(0, SeekOrigin.Begin)
            brDecrPKG.Read(FileTable, 0, FileTable.Length)

            positionIdx = 0
            OffsetShift = 0

            'Shift Relative to os.raw
            Array.Copy(FileTable, 0, firstNameOffset, 0, firstNameOffset.Length)
            Array.Reverse(firstNameOffset)

            Dim uifirstNameOffset As UInteger = BitConverter.ToUInt32(firstNameOffset, 0)
            Dim uiFileNr As UInteger = uifirstNameOffset \ 32

            Array.Copy(FileTable, 12, firstFileOffset, 0, firstFileOffset.Length)
            Array.Reverse(firstFileOffset)

            Dim uifirstFileOffset As UInteger = BitConverter.ToUInt32(firstFileOffset, 0)

            'Read the file table
            decrPKGReadStream.Seek(0, SeekOrigin.Begin)
            brDecrPKG.Read(FileTable, 0, uifirstFileOffset)

            If CInt(uiFileNr) < 0 Then
                'Unsupported PKG file
                Return False
            End If

            Dim myInt As Integer = 0

            While myInt <= CInt(uiFileNr) - 1
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

                contentType = FileTable(positionIdx + 24)
                fileType = FileTable(positionIdx + 27)

                Name = New Byte(ExtractedFileNameSize - 1) {}
                Array.Copy(FileTable, ExtractedFileNameOffset, Name, 0, ExtractedFileNameSize)
                Dim ExtractedFileName As String = Utils.ByteArrayToAscii(Name, 0, Name.Length, True)

                If fileType = 4 AndAlso ExtractedFileSize = 0 Then 'File / Directory
                    isFile = False
                Else
                    isFile = True
                End If

                If contentType <> 144 AndAlso isFile Then
                    If ExtractedFileName = "PARAM.SFO" Or ExtractedFileName = "ICON0.PNG" Or ExtractedFileName = "PIC0.PNG" Or ExtractedFileName = "PIC1.PNG" Or ExtractedFileName = "PIC2.PNG" Or ExtractedFileName = "SND0.AT3" Then
                        Using FileMemoryStream As New MemoryStream()
                            'Read File
                            decrPKGReadStream.Seek(ExtractedFileOffset, SeekOrigin.Begin)

                            'Pieces calculation
                            Dim division As Double = ExtractedFileSize / twentyMb
                            Dim pieces As ULong = Convert.ToUInt64(Math.Floor(division))
                            Dim CustomMod As ULong = Convert.ToUInt64(ExtractedFileSize) Mod Convert.ToUInt64(twentyMb)
                            If CustomMod > 0 Then
                                pieces += 1
                            End If

                            dumpFile = New Byte(twentyMb - 1) {}
                            Dim elapsed As Long = 0

                            For i As ULong = 0 To pieces - 1
                                If (CustomMod > 0) AndAlso (i = (pieces - 1)) Then
                                    dumpFile = New Byte(CustomMod - 1) {}
                                End If

                                'Fill buffer
                                Dim DecryptedData As Byte() = DecryptPKGDataRead(dumpFile.Length, ExtractedFileOffset + elapsed, uiEncryptedFileStartOffset, PS3AesKey, encrPKGReadStream, brEncrPKG)

                                elapsed = +dumpFile.Length
                                FileMemoryStream.Write(DecryptedData, 0, dumpFile.Length)
                            Next

                            Select Case ExtractedFileName
                                Case "PARAM.SFO"
                                    PARAMSFO = FileMemoryStream.ToArray()
                                Case "ICON0.PNG"
                                    ICON0 = Utils.BitmapSourceFromByteArray(FileMemoryStream.ToArray())
                                Case "PIC0.PNG"
                                    PIC0 = Utils.BitmapSourceFromByteArray(FileMemoryStream.ToArray())
                                Case "PIC1.PNG"
                                    PIC1 = Utils.BitmapSourceFromByteArray(FileMemoryStream.ToArray())
                                Case "PIC2.PNG"
                                    PIC2 = Utils.BitmapSourceFromByteArray(FileMemoryStream.ToArray())
                                Case "SND0.AT3"
                                    SND0 = FileMemoryStream.ToArray()
                            End Select

                            FileMemoryStream.Close()
                        End Using
                    End If
                End If

                positionIdx += +32
                myInt += 1
            End While

            'Close File
            encrPKGReadStream.Close()
            brEncrPKG.Close()

            decrPKGReadStream.Close()
            brDecrPKG.Close()

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetBytesFromFile(fileName As String) As Byte()
        Dim bytesFromFile As Byte()

        Try
            Dim array As Byte() = Nothing

            Using fileStream As New FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                array = New Byte(CInt(1048576 - 1L) + 1 - 1) {}
                fileStream.Read(array, 0, array.Length)
            End Using

            bytesFromFile = array
        Catch ex As Exception
            bytesFromFile = Nothing
        End Try

        Return bytesFromFile
    End Function

    Public Shared Function ExtractFiles(decryptedPKGFileName As String, encryptedPKGFileName As String) As Boolean
        Try
            Dim twentyMb As Integer = 1024 * 1024 * 20
            Dim ExtractedFileOffset As UInteger = 0
            Dim ExtractedFileSize As UInteger = 0
            Dim OffsetShift As UInteger = 0
            Dim positionIdx As Long = 0
            Dim WorkDir As String = ""

            WorkDir = decryptedPKGFileName & ".EXT"

            If Directory.Exists(WorkDir) Then
                Directory.Delete(WorkDir, True)
                Threading.Thread.Sleep(100)
                Directory.CreateDirectory(WorkDir)
                Threading.Thread.Sleep(100)
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

            Dim decrPKGReadStream As Stream = New FileStream(decryptedPKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Dim brDecrPKG As New BinaryReader(decrPKGReadStream)

            Dim encrPKGReadStream As Stream = New FileStream(encryptedPKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Dim brEncrPKG As New BinaryReader(encrPKGReadStream)

            'Read the file Table
            decrPKGReadStream.Seek(CLng(0), SeekOrigin.Begin)
            FileTable = brDecrPKG.ReadBytes(FileTable.Length)

            positionIdx = 0
            OffsetShift = 0

            'Shift Relative to os.raw
            Array.Copy(FileTable, 0, firstNameOffset, 0, firstNameOffset.Length)
            Array.Reverse(firstNameOffset)
            Dim uifirstNameOffset As UInteger = BitConverter.ToUInt32(firstNameOffset, 0)

            Dim uiFileNr As UInteger = uifirstNameOffset \ 32

            Array.Copy(FileTable, 12, firstFileOffset, 0, firstFileOffset.Length)
            Array.Reverse(firstFileOffset)
            Dim uifirstFileOffset As UInteger = BitConverter.ToUInt32(firstFileOffset, 0)

            'Read the file Table
            decrPKGReadStream.Seek(CLng(0), SeekOrigin.Begin)
            FileTable = brDecrPKG.ReadBytes(CInt(uifirstFileOffset))

            'If number of files is negative then something is wrong...
            If CInt(uiFileNr) < 0 Then

                Return False
            End If

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

                contentType = FileTable(positionIdx + 24)
                fileType = FileTable(positionIdx + 27)

                Name = New Byte(ExtractedFileNameSize - 1) {}
                Array.Copy(FileTable, ExtractedFileNameOffset, Name, 0, ExtractedFileNameSize)
                Dim ExtractedFileName As String = Utils.ByteArrayToAscii(Name, 0, Name.Length, True)

                'Write Directory
                If Not Directory.Exists(WorkDir) Then
                    Directory.CreateDirectory(WorkDir)
                    Threading.Thread.Sleep(100)
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
                    Dim DecryptedData As Byte() = DecryptData(ExtractedFileNameSize, ExtractedFileNameOffset, uiEncryptedFileStartOffset, PS3AesKey, encrPKGReadStream, brEncrPKG)
                    Array.Copy(DecryptedData, 0, Name, 0, ExtractedFileNameSize)
                    ExtractedFileName = Utils.ByteArrayToAscii(Name, 0, Name.Length, True)

                    If Not isFile Then
                        'Directory
                        Try
                            If Not Directory.Exists(ExtractedFileName) Then
                                Directory.CreateDirectory(WorkDir & "\" & ExtractedFileName)
                            End If
                        Catch ex As Exception
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
                    Dim [mod] As ULong = Convert.ToUInt64(ExtractedFileSize) Mod Convert.ToUInt64(twentyMb)
                    If [mod] > 0 Then
                        pieces += 1
                    End If

                    dumpFile = New Byte(twentyMb - 1) {}
                    For i As ULong = 0 To pieces - 1
                        'If we have a mod and this is the last piece then...
                        If ([mod] > 0) AndAlso (i = (pieces - 1)) Then
                            dumpFile = New Byte([mod] - 1) {}
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
                    Dim [mod] As ULong = Convert.ToUInt64(ExtractedFileSize) Mod Convert.ToUInt64(twentyMb)
                    If [mod] > 0 Then
                        pieces += 1
                    End If

                    dumpFile = New Byte(twentyMb - 1) {}
                    Dim elapsed As Long = 0
                    For i As ULong = 0 To pieces - 1
                        'If we have a mod and this is the last piece then...
                        If ([mod] > 0) AndAlso (i = (pieces - 1)) Then
                            dumpFile = New Byte([mod] - 1) {}
                        End If

                        'Fill buffer
                        Dim DecryptedData As Byte() = DecryptData(dumpFile.Length, ExtractedFileOffset + elapsed, uiEncryptedFileStartOffset, PS3AesKey, encrPKGReadStream, brEncrPKG)
                        elapsed = +dumpFile.Length

                        'To avoid decryption pad we use dumpFile.Length that's the actual decrypted file size!
                        ExtractedFile.Write(DecryptedData, 0, dumpFile.Length)
                    Next

                    ExtractedFileWriteStream.Close()
                    ExtractedFile.Close()
                End If

                positionIdx += 32
            Next

            'Close File
            encrPKGReadStream.Close()
            brEncrPKG.Close()

            decrPKGReadStream.Close()
            brDecrPKG.Close()

            'Delete decrypted file
            If File.Exists(decryptedPKGFileName) Then
                File.Delete(decryptedPKGFileName)
            End If

            If MsgBox("Pkg extracted successfully." + vbNewLine + "Open folder?", MsgBoxStyle.OkCancel, "Done") = MsgBoxResult.Ok Then
                Process.Start(".\")
            End If

            Return True
        Catch ex As Exception
            MsgBox(ex.ToString)
            Return False
        End Try
    End Function

    Public Shared Function DecryptData(dataSize As Integer, dataRelativeOffset As Long, pkgEncryptedFileStartOffset As Long, AesKey As Byte(), encrPKGReadStream As Stream, brEncrPKG As BinaryReader) As Byte()
        Dim size As Integer = dataSize Mod 16
        If size > 0 Then
            size = ((dataSize \ 16) + 1) * 16
        Else
            size = dataSize
        End If

        Dim EncryptedData As Byte() = New Byte(size - 1) {}
        Dim DecryptedData As Byte() = New Byte(size - 1) {}
        Dim PKGFileKeyConsec As Byte() = New Byte(size - 1) {}
        Dim PKGXorKeyConsec As Byte() = New Byte(size - 1) {}
        Dim incPKGFileKey As Byte() = New Byte(PKGFileKey.Length - 1) {}
        Array.Copy(PKGFileKey, incPKGFileKey, PKGFileKey.Length)

        encrPKGReadStream.Seek(dataRelativeOffset + pkgEncryptedFileStartOffset, SeekOrigin.Begin)
        EncryptedData = brEncrPKG.ReadBytes(size)

        For pos As Integer = 0 To dataRelativeOffset - 1 Step 16
            Utils.IncrementArray(incPKGFileKey, PKGFileKey.Length - 1)
        Next

        For pos As Integer = 0 To size - 1 Step 16
            Array.Copy(incPKGFileKey, 0, PKGFileKeyConsec, pos, PKGFileKey.Length)

            Utils.IncrementArray(incPKGFileKey, PKGFileKey.Length - 1)
        Next

        PKGXorKeyConsec = AESEngine.Encrypt(PKGFileKeyConsec, AesKey, AesKey, CipherMode.ECB, PaddingMode.None)
        DecryptedData = XOREngine.GetXOR(EncryptedData, 0, PKGXorKeyConsec.Length, PKGXorKeyConsec)

        Return DecryptedData
    End Function

    Public Shared Function DecryptPKGFile(PKGFileName As String) As String
        Try
            Dim moltiplicator As Integer = 65536
            Dim EncryptedData As Byte() = New Byte(AesKey.Length * moltiplicator - 1) {}
            Dim DecryptedData As Byte() = New Byte(AesKey.Length * moltiplicator - 1) {}

            Dim PKGXorKey As Byte() = New Byte(AesKey.Length - 1) {}
            Dim EncryptedFileStartOffset As Byte() = New Byte(3) {}
            Dim EncryptedFileLenght As Byte() = New Byte(3) {}

            Using PKGReadStream As Stream = New FileStream(PKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using brPKG As New BinaryReader(PKGReadStream)

                    PKGReadStream.Seek(&H0, SeekOrigin.Begin)
                    Dim pkgMagic As Byte() = brPKG.ReadBytes(4)
                    If pkgMagic(&H0) <> &H7F OrElse pkgMagic(&H1) <> &H50 OrElse pkgMagic(&H2) <> &H4B OrElse pkgMagic(&H3) <> &H47 Then

                        Return String.Empty
                    End If

                    'Finalized byte
                    PKGReadStream.Seek(&H4, SeekOrigin.Begin)
                    Dim pkgFinalized As Byte = brPKG.ReadByte()

                    If pkgFinalized <> 128 Then

                        Return String.Empty
                    End If

                    'PKG Type PSP/PS3
                    PKGReadStream.Seek(&H7, SeekOrigin.Begin)
                    Dim pkgType As Byte = brPKG.ReadByte()

                    Select Case pkgType
                        Case &H1
                            'PS3
                            AesKey = PS3AesKey
                            Exit Select
                        Case &H2
                            'PSP
                            AesKey = PSPAesKey
                            Exit Select
                        Case Else

                            Return String.Empty
                    End Select

                    PKGReadStream.Seek(&H24, SeekOrigin.Begin)
                    EncryptedFileStartOffset = brPKG.ReadBytes(EncryptedFileStartOffset.Length)
                    Array.Reverse(EncryptedFileStartOffset)
                    uiEncryptedFileStartOffset = BitConverter.ToUInt32(EncryptedFileStartOffset, 0)

                    PKGReadStream.Seek(&H2C, SeekOrigin.Begin)
                    EncryptedFileLenght = brPKG.ReadBytes(CInt(EncryptedFileLenght.Length))
                    Array.Reverse(EncryptedFileLenght)
                    Dim uiEncryptedFileLenght As UInteger = BitConverter.ToUInt32(EncryptedFileLenght, 0)

                    PKGReadStream.Seek(&H70, SeekOrigin.Begin)
                    PKGFileKey = brPKG.ReadBytes(16)
                    Dim incPKGFileKey As Byte() = New Byte(15) {}
                    Array.Copy(PKGFileKey, incPKGFileKey, PKGFileKey.Length)

                    PKGXorKey = AESEngine.Encrypt(PKGFileKey, AesKey, AesKey, CipherMode.ECB, PaddingMode.None)

                    Dim division As Double = uiEncryptedFileLenght / AesKey.Length
                    Dim pieces As ULong = Convert.ToUInt64(Math.Floor(division))
                    Dim [mod] As ULong = Convert.ToUInt64(uiEncryptedFileLenght) / Convert.ToUInt64(AesKey.Length)
                    If [mod] > 0 Then
                        pieces += 1
                    End If

                    If File.Exists(PKGFileName & ".Dec") Then
                        File.Delete(PKGFileName & ".Dec")
                    End If

                    Dim DecryptedFileWriteStream As New FileStream(PKGFileName & ".Dec", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite)
                    Dim bwDecryptedFile As New BinaryWriter(DecryptedFileWriteStream)

                    PKGReadStream.Seek(uiEncryptedFileStartOffset, SeekOrigin.Begin)

                    Dim filedivision As Double = uiEncryptedFileLenght / (AesKey.Length * moltiplicator)
                    Dim filepieces As ULong = Convert.ToUInt64(Math.Floor(filedivision))
                    Dim filemod As ULong = Convert.ToUInt64(uiEncryptedFileLenght) Mod Convert.ToUInt64(AesKey.Length * moltiplicator)
                    If filemod > 0 Then
                        filepieces += 1
                    End If

                    For i As ULong = 0 To filepieces - 1
                        'If we have a mod and this is the last piece then...
                        If (filemod > 0) AndAlso (i = (filepieces - 1)) Then
                            EncryptedData = New Byte(filemod - 1) {}
                            DecryptedData = New Byte(filemod - 1) {}
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
                            pos += AesKey.Length
                        End While

                        PKGXorKeyConsec = AESEngine.Encrypt(PKGFileKeyConsec, AesKey, AesKey, CipherMode.ECB, PaddingMode.None)
                        DecryptedData = XOREngine.GetXOR(EncryptedData, 0, PKGXorKeyConsec.Length, PKGXorKeyConsec)
                        bwDecryptedFile.Write(DecryptedData)
                    Next

                    DecryptedFileWriteStream.Close()
                    bwDecryptedFile.Close()
                End Using
            End Using

            Return PKGFileName & ".Dec"
        Catch ex As Exception

            Return String.Empty
        End Try
    End Function

End Class