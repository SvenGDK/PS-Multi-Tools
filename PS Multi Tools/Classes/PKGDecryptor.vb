Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports EndianIOExtension

Public Class PKGDecryptor

    Public PARAMSFO As Byte()
    Public ICON0 As BitmapSource
    Public PIC0 As BitmapSource
    Public PIC1 As BitmapSource
    Public PIC2 As BitmapSource
    Public SND0 As Byte()
    Public PBPBytes As Byte()
    Public PackageType As PKGType
    Public PKGContentID As String
    Public IsDecError As Boolean
    Public PSPAesKey As Byte() = New Byte(15) {7, 242, 198, 130, 144, 181, 13, 44, 51, 129, 141, 112, 155, 96, 230, 43}
    Public PS3AesKey As Byte() = New Byte(15) {46, 123, 113, 215, 201, 201, 161, 78, 163, 34, 31, 24, 136, 40, 184, 248}
    Public AesKey As Byte() = New Byte(15) {}
    Public PKGFileKey As Byte() = New Byte(15) {}
    Public UIEncryptedFileStartOffset As UInteger = 0
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
        IsDecError = False
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
            Dim Multiplicator As Integer = 65536
            Dim ByteArray As Byte() = New Byte(1048576 - 1 + 1 - 1) {}
            Dim EncryptedData As Byte() = New Byte(AesKey.Length * Multiplicator - 1) {}
            Dim DecryptedData As Byte() = New Byte(AesKey.Length * Multiplicator - 1) {}
            Dim PKGXorKey As Byte() = New Byte(AesKey.Length - 1) {}
            Dim EncryptedFileStartOffset As Byte() = New Byte(3) {}
            Dim EncryptedFileLenght As Byte() = New Byte(3) {}

            Using PKGReadStream As New FileStream(PKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using PKGBinaryReader As New BinaryReader(PKGReadStream)

                    Dim PKGMagic As Byte() = PKGBinaryReader.ReadBytes(4)
                    If PKGMagic(0) <> 127 OrElse PKGMagic(1) <> 80 OrElse PKGMagic(2) <> 75 OrElse PKGMagic(3) <> 71 Then
                        'Selected file isn't a Pkg file. Error!
                    End If

                    'Finalized byte
                    PKGReadStream.Seek(4, SeekOrigin.Begin)
                    Dim PKGFinalized As Byte = PKGBinaryReader.ReadByte()

                    If PKGFinalized <> 128 Then
                        'This is debug PKG and is not supported!
                    End If

                    PKGReadStream.Seek(48, SeekOrigin.Begin)
                    PKGContentID = Encoding.ASCII.GetString(PKGBinaryReader.ReadBytes(36))

                    'PKG Type PSP/PS3
                    PKGReadStream.Seek(7, SeekOrigin.Begin)
                    Dim PKGType As Byte = PKGBinaryReader.ReadByte()

                    Select Case PKGType
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
                    EncryptedFileStartOffset = PKGBinaryReader.ReadBytes(EncryptedFileStartOffset.Length)
                    Array.Reverse(EncryptedFileStartOffset)
                    UIEncryptedFileStartOffset = BitConverter.ToUInt32(EncryptedFileStartOffset, 0)

                    PKGReadStream.Seek(44, SeekOrigin.Begin)
                    EncryptedFileLenght = PKGBinaryReader.ReadBytes(EncryptedFileLenght.Length)
                    Array.Reverse(EncryptedFileLenght)
                    Dim UIEncryptedFileLenght As UInteger = BitConverter.ToUInt32(EncryptedFileLenght, 0)

                    PKGReadStream.Seek(112, SeekOrigin.Begin)
                    PKGFileKey = PKGBinaryReader.ReadBytes(16)
                    Dim IncPKGFileKey As Byte() = New Byte(15) {}
                    Array.Copy(PKGFileKey, IncPKGFileKey, PKGFileKey.Length)

                    PKGXorKey = AESEngine.Encrypt(PKGFileKey, AesKey, AesKey, CipherMode.ECB, PaddingMode.None)

                    Dim Division As Double = UIEncryptedFileLenght / AesKey.Length
                    Dim Pieces As ULong = Convert.ToUInt64(Math.Floor(Division))
                    Dim CustomMod As ULong = CULng(Convert.ToUInt64(UIEncryptedFileLenght) / Convert.ToUInt64(AesKey.Length))
                    If CustomMod > 0 Then
                        Pieces += CULng(1)
                    End If

                    Using DecryptedFileMemoryStream As New MemoryStream()
                        PKGReadStream.Seek(UIEncryptedFileStartOffset, SeekOrigin.Begin)

                        Dim FileDivision As Double = UIEncryptedFileLenght / (AesKey.Length * Multiplicator)
                        Dim FilePieces As ULong = Convert.ToUInt64(Math.Floor(FileDivision))
                        Dim FileMod As ULong = Convert.ToUInt64(UIEncryptedFileLenght) Mod Convert.ToUInt64(AesKey.Length * Multiplicator)
                        If FileMod > 0 Then
                            FilePieces += CULng(1)
                        End If

                        Dim MUInt64 As ULong = Convert.ToUInt64(Decimal.Subtract(New Decimal(FileDivision), 1D))
                        If 0 <= MUInt64 Then
                            If (FileMod > 0) AndAlso ((FilePieces - 1) = 0) Then
                                EncryptedData = New Byte(CInt(FileMod - 1)) {}
                                DecryptedData = New Byte(CInt(FileMod - 1)) {}
                            End If

                            'Read 16 bytes of Encrypted data
                            EncryptedData = PKGBinaryReader.ReadBytes(EncryptedData.Length)

                            'In order to retrieve a fast AES Encryption we pre-Increment the array
                            Dim PKGFileKeyConsec As Byte() = New Byte(EncryptedData.Length - 1) {}
                            Dim PKGXorKeyConsec As Byte() = New Byte(EncryptedData.Length - 1) {}

                            Dim Position As Integer = 0
                            While Position < EncryptedData.Length
                                Array.Copy(IncPKGFileKey, 0, PKGFileKeyConsec, Position, PKGFileKey.Length)
                                Utils.IncrementArray(IncPKGFileKey, PKGFileKey.Length - 1)
                                Position += AesKey.Length
                            End While

                            PKGXorKeyConsec = AESEngine.Encrypt(PKGFileKeyConsec, AesKey, AesKey, CipherMode.ECB, PaddingMode.None)
                            DecryptedData = XOREngine.GetXOR(EncryptedData, 0, PKGXorKeyConsec.Length, PKGXorKeyConsec)
                            DecryptedFileMemoryStream.Write(DecryptedData, 0, DecryptedData.Length)

                            If DecryptedData.Length >= 1048576 Then
                                ByteArray = DecryptedFileMemoryStream.ToArray()
                            End If
                        End If

                        'For i As ULong = 0 To filepieces - 1

                        ' Next

                    End Using
                End Using
            End Using

            Return ByteArray
        Catch ex As Exception
            Return Nothing
            'Could not read decrypt PKG file.
        End Try
    End Function

    Public Function DecryptPKGDataRead(DataSize As Integer, DataRelativeOffset As Long, PKGEncryptedFileStartOffset As Long, AESKey As Byte(), EncryptedPKGReadStream As Stream, EncryptedPKGStream As Stream) As Byte()
        Try
            Dim InputSize As Integer = DataSize Mod 16
            If InputSize > 0 Then
                InputSize = ((DataSize \ 16) + 1) * 16
            Else
                InputSize = DataSize
            End If

            Dim EncryptedData As Byte() = New Byte(InputSize - 1) {}
            Dim DecryptedData As Byte() = New Byte(InputSize - 1) {}
            Dim PKGFileKeyBytes As Byte() = New Byte(InputSize - 1) {}
            Dim PKGXorKeyBytes As Byte() = New Byte(InputSize - 1) {}
            Dim IncPKGFileKey As Byte() = New Byte(PKGFileKey.Length - 1) {}
            Array.Copy(PKGFileKey, IncPKGFileKey, PKGFileKey.Length)

            EncryptedPKGReadStream.Seek(DataRelativeOffset + PKGEncryptedFileStartOffset, SeekOrigin.Begin)
            EncryptedPKGStream.ReadExactly(EncryptedData, 0, InputSize)

            For Position As Integer = 0 To CInt(DataRelativeOffset - 1) Step 16
                Utils.IncrementArray(IncPKGFileKey, PKGFileKey.Length - 1)
            Next

            For Position As Integer = 0 To InputSize - 1 Step 16
                Array.Copy(IncPKGFileKey, 0, PKGFileKeyBytes, Position, PKGFileKey.Length)
                Utils.IncrementArray(IncPKGFileKey, PKGFileKey.Length - 1)
            Next

            PKGXorKeyBytes = AESEngine.Encrypt(PKGFileKeyBytes, AESKey, AESKey, CipherMode.ECB, PaddingMode.None)
            DecryptedData = XOREngine.GetXOR(EncryptedData, 0, PKGXorKeyBytes.Length, PKGXorKeyBytes)
            Return DecryptedData
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function ExtractPKGFilesRead(DecryptedPKGFileName As Byte(), EncryptedPKGFileName As Byte()) As Boolean
        Try
            Dim TwentyMB As Integer = 20971520
            Dim ExtractedFileOffset As UInteger = 0
            Dim ExtractedFileSize As UInteger = 0
            Dim OffsetShift As UInteger = 0
            Dim PositionIndex As Long = 0
            Dim FileTable As Byte() = New Byte(319999) {}
            Dim SDKVer As Byte() = New Byte(7) {}
            Dim FirstFileOffset As Byte() = New Byte(3) {}
            Dim FirstNameOffset As Byte() = New Byte(3) {}
            Dim FileNr As Byte() = New Byte(3) {}
            Dim IsDir As Byte() = New Byte(3) {}
            Dim Offset As Byte() = New Byte(3) {}
            Dim Size As Byte() = New Byte(3) {}
            Dim NameOffset As Byte() = New Byte(3) {}
            Dim NameSize As Byte() = New Byte(3) {}
            Dim Name As Byte() = New Byte(31) {}
            Dim BootMagic As Byte() = New Byte(7) {}
            Dim ContentType As Byte = 0
            Dim FileType As Byte = 0
            Dim IsFile As Boolean = False
            Dim DumpFile As Byte()
            Dim DecryptedPKGReadStream As New MemoryStream(DecryptedPKGFileName)
            Dim DecryptedPKGMemoryStream As MemoryStream = DecryptedPKGReadStream
            Dim EncryptedPKGReadStream As New MemoryStream(EncryptedPKGFileName)
            Dim EncryptedPKGMemoryStream As MemoryStream = EncryptedPKGReadStream

            'Read the file table
            DecryptedPKGReadStream.Seek(0, SeekOrigin.Begin)
            DecryptedPKGMemoryStream.Read(FileTable, 0, FileTable.Length)

            PositionIndex = 0
            OffsetShift = 0

            'Shift Relative to os.raw
            Array.Copy(FileTable, 0, FirstNameOffset, 0, FirstNameOffset.Length)
            Array.Reverse(FirstNameOffset)

            Dim UIFirstNameOffset As UInteger = BitConverter.ToUInt32(FirstNameOffset, 0)
            Dim UIFileNr As UInteger = CUInt(UIFirstNameOffset \ 32)

            Array.Copy(FileTable, 12, FirstFileOffset, 0, FirstFileOffset.Length)
            Array.Reverse(FirstFileOffset)

            Dim UIfirstFileOffset As UInteger = BitConverter.ToUInt32(FirstFileOffset, 0)

            'Read the file table
            DecryptedPKGReadStream.Seek(0, SeekOrigin.Begin)
            DecryptedPKGMemoryStream.Read(FileTable, 0, CInt(UIfirstFileOffset))

            If CInt(UIFileNr) < 0 Then
                'Unsupported PKG file
                Return False
            End If

            Dim WhileInt As Integer = 0
            While WhileInt <= CInt(UIFileNr) - 1
                Array.Copy(FileTable, PositionIndex + 12, Offset, 0, Offset.Length)
                Array.Reverse(Offset)
                ExtractedFileOffset = BitConverter.ToUInt32(Offset, 0) + OffsetShift

                Array.Copy(FileTable, PositionIndex + 20, Size, 0, Size.Length)
                Array.Reverse(Size)
                ExtractedFileSize = BitConverter.ToUInt32(Size, 0)

                Array.Copy(FileTable, PositionIndex, NameOffset, 0, NameOffset.Length)
                Array.Reverse(NameOffset)
                Dim ExtractedFileNameOffset As UInteger = BitConverter.ToUInt32(NameOffset, 0)

                Array.Copy(FileTable, PositionIndex + 4, NameSize, 0, NameSize.Length)
                Array.Reverse(NameSize)
                Dim ExtractedFileNameSize As UInteger = BitConverter.ToUInt32(NameSize, 0)

                ContentType = FileTable(CInt(PositionIndex + 24))
                FileType = FileTable(CInt(PositionIndex + 27))

                Name = New Byte(CInt(ExtractedFileNameSize - 1)) {}
                Array.Copy(FileTable, ExtractedFileNameOffset, Name, 0, ExtractedFileNameSize)
                Dim ExtractedFileName As String = Utils.ByteArrayToAscii(Name, 0, Name.Length, True)

                If FileType = 4 AndAlso ExtractedFileSize = 0 Then 'File / Directory
                    IsFile = False
                Else
                    IsFile = True
                End If

                If ContentType <> 144 AndAlso IsFile Then
                    If ExtractedFileName = "PARAM.SFO" Or ExtractedFileName = "ICON0.PNG" Or ExtractedFileName = "PIC0.PNG" Or ExtractedFileName = "PIC1.PNG" Or ExtractedFileName = "PIC2.PNG" Or ExtractedFileName = "SND0.AT3" Then
                        Using FileMemoryStream As New MemoryStream()
                            'Read File
                            DecryptedPKGReadStream.Seek(ExtractedFileOffset, SeekOrigin.Begin)

                            'Pieces calculation
                            Dim Division As Double = ExtractedFileSize / TwentyMB
                            Dim Pieces As ULong = Convert.ToUInt64(Math.Floor(Division))
                            Dim CustomMod As ULong = Convert.ToUInt64(ExtractedFileSize) Mod Convert.ToUInt64(TwentyMB)
                            If CustomMod > 0 Then
                                Pieces += CULng(1)
                            End If

                            DumpFile = New Byte(TwentyMB - 1) {}
                            Dim Elapsed As Long = 0

                            For i As ULong = 0 To CULng(Pieces - 1)
                                If (CustomMod > 0) AndAlso (i = (Pieces - 1)) Then
                                    DumpFile = New Byte(CInt(CustomMod - 1)) {}
                                End If

                                'Fill buffer
                                Dim DecryptedData As Byte() = DecryptPKGDataRead(DumpFile.Length, ExtractedFileOffset + Elapsed, UIEncryptedFileStartOffset, PS3AesKey, EncryptedPKGReadStream, EncryptedPKGMemoryStream)

                                Elapsed += DumpFile.Length
                                FileMemoryStream.Write(DecryptedData, 0, DumpFile.Length)
                            Next

                            If FileMemoryStream.ToArray().Length > 0 Then
                                Select Case ExtractedFileName
                                    Case "PARAM.SFO"
                                        PARAMSFO = FileMemoryStream.ToArray()
                                    Case "ICON0.PNG"
                                        ICON0 = NewBitmapImage(FileMemoryStream.ToArray())
                                    Case "PIC0.PNG"
                                        PIC0 = NewBitmapImage(FileMemoryStream.ToArray())
                                    Case "PIC1.PNG"
                                        PIC1 = NewBitmapImage(FileMemoryStream.ToArray())
                                    Case "PIC2.PNG"
                                        PIC2 = NewBitmapImage(FileMemoryStream.ToArray())
                                    Case "SND0.AT3"
                                        SND0 = FileMemoryStream.ToArray()
                                End Select
                            End If

                            FileMemoryStream.Close()
                        End Using
                    End If
                End If

                PositionIndex += 32
                WhileInt += 1
            End While

            'Close File
            EncryptedPKGReadStream.Close()
            EncryptedPKGMemoryStream.Close()

            DecryptedPKGReadStream.Close()
            DecryptedPKGMemoryStream.Close()

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Shared Function GetBytesFromFile(FileName As String) As Byte()
        Dim BytesFromFile As Byte()

        Try
            Dim ByteArray As Byte() = Nothing

            Using FileStream As New FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                ByteArray = New Byte(CInt(1048576 - 1L) + 1 - 1) {}
                FileStream.ReadExactly(ByteArray, 0, ByteArray.Length)
            End Using

            BytesFromFile = ByteArray
        Catch ex As Exception
            BytesFromFile = Nothing
        End Try

        Return BytesFromFile
    End Function

    Public Function ExtractFiles(DecryptedPKGFileName As String, EncryptedPKGFileName As String) As Boolean
        Try
            Dim TwentyMB As Integer = 1024 * 1024 * 20
            Dim ExtractedFileOffset As UInteger = 0
            Dim ExtractedFileSize As UInteger = 0
            Dim OffsetShift As UInteger = 0
            Dim PositionIndex As Long = 0
            Dim WorkDir As String = ""

            WorkDir = DecryptedPKGFileName & ".EXT"

            If Directory.Exists(WorkDir) Then
                Directory.Delete(WorkDir, True)
                Threading.Thread.Sleep(100)
                Directory.CreateDirectory(WorkDir)
                Threading.Thread.Sleep(100)
            End If

            Dim FileTable As Byte() = New Byte(319999) {}
            Dim DumpFile As Byte()
            Dim SDKVer As Byte() = New Byte(7) {}
            Dim FirstFileOffset As Byte() = New Byte(3) {}
            Dim FirstNameOffset As Byte() = New Byte(3) {}
            Dim FileNr As Byte() = New Byte(3) {}
            Dim IsDir As Byte() = New Byte(3) {}
            Dim Offset As Byte() = New Byte(3) {}
            Dim Size As Byte() = New Byte(3) {}
            Dim NameOffset As Byte() = New Byte(3) {}
            Dim NameSize As Byte() = New Byte(3) {}
            Dim Name As Byte() = New Byte(31) {}
            Dim BootMagic As Byte() = New Byte(7) {}
            Dim ContentType As Byte = 0
            Dim FileType As Byte = 0
            Dim IsFile As Boolean = False

            Dim DecryptedPKGReadStream As Stream = New FileStream(DecryptedPKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Dim DecryptedPKGBinaryReader As New BinaryReader(DecryptedPKGReadStream)

            Dim EncryptedPKGReadStream As Stream = New FileStream(EncryptedPKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Dim EncryptedPKGBinaryReader As New BinaryReader(EncryptedPKGReadStream)

            'Read the file Table
            DecryptedPKGReadStream.Seek(CLng(0), SeekOrigin.Begin)
            FileTable = DecryptedPKGBinaryReader.ReadBytes(FileTable.Length)

            PositionIndex = 0
            OffsetShift = 0

            'Shift Relative to os.raw
            Array.Copy(FileTable, 0, FirstNameOffset, 0, FirstNameOffset.Length)
            Array.Reverse(FirstNameOffset)
            Dim UIfirstNameOffset As UInteger = BitConverter.ToUInt32(FirstNameOffset, 0)
            Dim UIFileNr As UInteger = CUInt(UIfirstNameOffset \ 32)

            Array.Copy(FileTable, 12, FirstFileOffset, 0, FirstFileOffset.Length)
            Array.Reverse(FirstFileOffset)
            Dim UIFirstFileOffset As UInteger = BitConverter.ToUInt32(FirstFileOffset, 0)

            'Read the file Table
            DecryptedPKGReadStream.Seek(CLng(0), SeekOrigin.Begin)
            FileTable = DecryptedPKGBinaryReader.ReadBytes(CInt(UIFirstFileOffset))

            'If number of files is negative then something is wrong...
            If CInt(UIFileNr) < 0 Then
                Return False
            End If

            'Table:
            '0-3         4-7         8-11        12-15       16-19       20-23       24-27       28-31
            '|name loc | |name size| |   NULL  | |file loc | |  NULL   | |file size| |cont type| |   NULL  |

            For ii As Integer = 0 To CInt(UIFileNr) - 1
                Array.Copy(FileTable, PositionIndex + 12, Offset, 0, Offset.Length)
                Array.Reverse(Offset)
                ExtractedFileOffset = BitConverter.ToUInt32(Offset, 0) + OffsetShift

                Array.Copy(FileTable, PositionIndex + 20, Size, 0, Size.Length)
                Array.Reverse(Size)
                ExtractedFileSize = BitConverter.ToUInt32(Size, 0)

                Array.Copy(FileTable, PositionIndex, NameOffset, 0, NameOffset.Length)
                Array.Reverse(NameOffset)
                Dim ExtractedFileNameOffset As UInteger = BitConverter.ToUInt32(NameOffset, 0)

                Array.Copy(FileTable, PositionIndex + 4, NameSize, 0, NameSize.Length)
                Array.Reverse(NameSize)
                Dim ExtractedFileNameSize As UInteger = BitConverter.ToUInt32(NameSize, 0)

                ContentType = FileTable(CInt(PositionIndex + 24))
                FileType = FileTable(CInt(PositionIndex + 27))

                Name = New Byte(CInt(ExtractedFileNameSize - 1)) {}
                Array.Copy(FileTable, ExtractedFileNameOffset, Name, 0, ExtractedFileNameSize)
                Dim ExtractedFileName As String = Utils.ByteArrayToAscii(Name, 0, Name.Length, True)

                'Write Directory
                If Not Directory.Exists(WorkDir) Then
                    Directory.CreateDirectory(WorkDir)
                    Threading.Thread.Sleep(100)
                End If

                Dim ExtractedFileWriteStream As FileStream = Nothing

                'File / Directory
                If (FileType = &H4) AndAlso (ExtractedFileSize = &H0) Then
                    IsFile = False
                Else
                    IsFile = True
                End If

                'contentType == 0x90 = PSP file/dir
                If ContentType = &H90 Then
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
                    Dim DecryptedData As Byte() = DecryptData(CInt(ExtractedFileNameSize), ExtractedFileNameOffset, UIEncryptedFileStartOffset, PS3AesKey, EncryptedPKGReadStream, EncryptedPKGBinaryReader)
                    Array.Copy(DecryptedData, 0, Name, 0, ExtractedFileNameSize)
                    ExtractedFileName = Utils.ByteArrayToAscii(Name, 0, Name.Length, True)

                    If Not IsFile Then
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

                If ContentType = 144 AndAlso IsFile Then
                    'Read/Write File
                    Dim ExtractedFile As New BinaryWriter(ExtractedFileWriteStream)
                    DecryptedPKGReadStream.Seek(ExtractedFileOffset, SeekOrigin.Begin)

                    ' Pieces calculation
                    Dim Division As Double = ExtractedFileSize / TwentyMB
                    Dim Pieces As ULong = Convert.ToUInt64(Math.Floor(Division))
                    Dim Modi As ULong = Convert.ToUInt64(ExtractedFileSize) Mod Convert.ToUInt64(TwentyMB)
                    If Modi > 0 Then
                        Pieces += CULng(1)
                    End If

                    DumpFile = New Byte(TwentyMB - 1) {}
                    For i As ULong = 0 To CULng(Pieces - 1)
                        'If we have a mod and this is the last piece then...
                        If (Modi > 0) AndAlso (i = (Pieces - 1)) Then
                            DumpFile = New Byte(CInt(Modi - 1)) {}
                        End If

                        'Fill buffer
                        DecryptedPKGBinaryReader.Read(DumpFile, 0, DumpFile.Length)
                        ExtractedFile.Write(DumpFile)
                    Next

                    ExtractedFileWriteStream.Close()
                    ExtractedFile.Close()
                End If

                If ContentType <> &H90 AndAlso IsFile Then
                    'Read/Write File
                    Dim ExtractedFile As New BinaryWriter(ExtractedFileWriteStream)
                    DecryptedPKGReadStream.Seek(ExtractedFileOffset, SeekOrigin.Begin)

                    ' Pieces calculation
                    Dim Division As Double = ExtractedFileSize / TwentyMB

                    Dim Pieces As ULong = Convert.ToUInt64(Math.Floor(Division))
                    Dim Modi As ULong = Convert.ToUInt64(ExtractedFileSize) Mod Convert.ToUInt64(TwentyMB)
                    If Modi > 0 Then
                        Pieces += CULng(1)
                    End If

                    DumpFile = New Byte(TwentyMB - 1) {}
                    Dim Elapsed As Long = 0
                    For i As ULong = 0 To CULng(Pieces - 1)
                        'If we have a mod and this is the last piece then...
                        If (Modi > 0) AndAlso (i = (Pieces - 1)) Then
                            DumpFile = New Byte(CInt(Modi - 1)) {}
                        End If

                        'Fill buffer
                        Dim DecryptedData As Byte() = DecryptData(DumpFile.Length, ExtractedFileOffset + Elapsed, UIEncryptedFileStartOffset, PS3AesKey, EncryptedPKGReadStream, EncryptedPKGBinaryReader)
                        Elapsed = +DumpFile.Length

                        'To avoid decryption pad we use dumpFile.Length that's the actual decrypted file size!
                        ExtractedFile.Write(DecryptedData, 0, DumpFile.Length)
                    Next

                    ExtractedFileWriteStream.Close()
                    ExtractedFile.Close()
                End If

                PositionIndex += 32
            Next

            'Close File
            EncryptedPKGReadStream.Close()
            EncryptedPKGBinaryReader.Close()

            DecryptedPKGReadStream.Close()
            DecryptedPKGBinaryReader.Close()

            'Delete decrypted file
            If File.Exists(DecryptedPKGFileName) Then
                File.Delete(DecryptedPKGFileName)
            End If

            If MsgBox("Pkg extracted successfully." + vbCrLf + "Open folder?", MsgBoxStyle.OkCancel, "Done") = MsgBoxResult.Ok Then
                Process.Start("explorer", ".\")
            End If

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function DecryptData(DataSize As Integer, DataRelativeOffset As Long, PKGEncryptedFileStartOffset As Long, AesKey As Byte(), EncryptedPKGReadStream As Stream, EncryptedPKGBinaryReader As BinaryReader) As Byte()
        Dim InputSize As Integer = DataSize Mod 16
        If InputSize > 0 Then
            InputSize = ((DataSize \ 16) + 1) * 16
        Else
            InputSize = DataSize
        End If

        Dim EncryptedData As Byte()
        Dim DecryptedData As Byte()
        Dim PKGFileKeyConsec As Byte() = New Byte(InputSize - 1) {}
        Dim PKGXorKeyConsec As Byte()
        Dim IncPKGFileKey As Byte() = New Byte(PKGFileKey.Length - 1) {}
        Array.Copy(PKGFileKey, IncPKGFileKey, PKGFileKey.Length)

        EncryptedPKGReadStream.Seek(DataRelativeOffset + PKGEncryptedFileStartOffset, SeekOrigin.Begin)
        EncryptedData = EncryptedPKGBinaryReader.ReadBytes(InputSize)

        For Position As Integer = 0 To CInt(DataRelativeOffset - 1) Step 16
            Utils.IncrementArray(IncPKGFileKey, PKGFileKey.Length - 1)
        Next

        For Position As Integer = 0 To InputSize - 1 Step 16
            Array.Copy(IncPKGFileKey, 0, PKGFileKeyConsec, Position, PKGFileKey.Length)
            Utils.IncrementArray(IncPKGFileKey, PKGFileKey.Length - 1)
        Next

        PKGXorKeyConsec = AESEngine.Encrypt(PKGFileKeyConsec, AesKey, AesKey, CipherMode.ECB, PaddingMode.None)
        DecryptedData = XOREngine.GetXOR(EncryptedData, 0, PKGXorKeyConsec.Length, PKGXorKeyConsec)

        Return DecryptedData
    End Function

    Public Function DecryptPKGFile(PKGFileName As String) As String
        Try
            Dim Multiplicator As Integer = 65536
            Dim EncryptedData As Byte() = New Byte(AesKey.Length * Multiplicator - 1) {}
            Dim DecryptedData As Byte() = New Byte(AesKey.Length * Multiplicator - 1) {}

            Dim PKGXorKey As Byte() = New Byte(AesKey.Length - 1) {}
            Dim EncryptedFileStartOffset As Byte() = New Byte(3) {}
            Dim EncryptedFileLenght As Byte() = New Byte(3) {}

            Using PKGReadStream As Stream = New FileStream(PKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using PKGBinaryReader As New BinaryReader(PKGReadStream)

                    PKGReadStream.Seek(&H0, SeekOrigin.Begin)
                    Dim PKGMagic As Byte() = PKGBinaryReader.ReadBytes(4)
                    If PKGMagic(&H0) <> &H7F OrElse PKGMagic(&H1) <> &H50 OrElse PKGMagic(&H2) <> &H4B OrElse PKGMagic(&H3) <> &H47 Then

                        Return String.Empty
                    End If

                    'Finalized byte
                    PKGReadStream.Seek(&H4, SeekOrigin.Begin)
                    Dim PKGFinalized As Byte = PKGBinaryReader.ReadByte()

                    If PKGFinalized <> 128 Then

                        Return String.Empty
                    End If

                    'PKG Type PSP/PS3
                    PKGReadStream.Seek(&H7, SeekOrigin.Begin)
                    Dim PKGType As Byte = PKGBinaryReader.ReadByte()

                    Select Case PKGType
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
                    EncryptedFileStartOffset = PKGBinaryReader.ReadBytes(EncryptedFileStartOffset.Length)
                    Array.Reverse(EncryptedFileStartOffset)
                    UIEncryptedFileStartOffset = BitConverter.ToUInt32(EncryptedFileStartOffset, 0)

                    PKGReadStream.Seek(&H2C, SeekOrigin.Begin)
                    EncryptedFileLenght = PKGBinaryReader.ReadBytes(EncryptedFileLenght.Length)
                    Array.Reverse(EncryptedFileLenght)
                    Dim UIEncryptedFileLenght As UInteger = BitConverter.ToUInt32(EncryptedFileLenght, 0)

                    PKGReadStream.Seek(&H70, SeekOrigin.Begin)
                    PKGFileKey = PKGBinaryReader.ReadBytes(16)
                    Dim incPKGFileKey As Byte() = New Byte(15) {}
                    Array.Copy(PKGFileKey, incPKGFileKey, PKGFileKey.Length)

                    PKGXorKey = AESEngine.Encrypt(PKGFileKey, AesKey, AesKey, CipherMode.ECB, PaddingMode.None)

                    Dim Division As Double = UIEncryptedFileLenght / AesKey.Length
                    Dim Pieces As ULong = Convert.ToUInt64(Math.Floor(Division))
                    Dim Modi As ULong = CULng(Convert.ToUInt64(UIEncryptedFileLenght) / Convert.ToUInt64(AesKey.Length))
                    If Modi > 0 Then
                        Pieces += CULng(1)
                    End If

                    If File.Exists(PKGFileName & ".Dec") Then
                        File.Delete(PKGFileName & ".Dec")
                    End If

                    Dim DecryptedFileWriteStream As New FileStream(PKGFileName & ".Dec", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite)
                    Dim DecryptedFileBinaryWriter As New BinaryWriter(DecryptedFileWriteStream)

                    PKGReadStream.Seek(UIEncryptedFileStartOffset, SeekOrigin.Begin)

                    Dim FileDivision As Double = UIEncryptedFileLenght / (AesKey.Length * Multiplicator)
                    Dim FilePieces As ULong = Convert.ToUInt64(Math.Floor(FileDivision))
                    Dim FileMod As ULong = Convert.ToUInt64(UIEncryptedFileLenght) Mod Convert.ToUInt64(AesKey.Length * Multiplicator)
                    If FileMod > 0 Then
                        FilePieces += CULng(1)
                    End If

                    For i As ULong = 0 To CULng(FilePieces - 1)
                        'If we have a mod and this is the last piece then...
                        If (FileMod > 0) AndAlso (i = (FilePieces - 1)) Then
                            EncryptedData = New Byte(CInt(FileMod - 1)) {}
                            DecryptedData = New Byte(CInt(FileMod - 1)) {}
                        End If

                        'Read 16 bytes of Encrypted data
                        EncryptedData = PKGBinaryReader.ReadBytes(EncryptedData.Length)

                        'In order to retrieve a fast AES Encryption we pre-Increment the array
                        Dim PKGFileKeyBytes As Byte() = New Byte(EncryptedData.Length - 1) {}
                        Dim PKGXorKeyBytes As Byte() = New Byte(EncryptedData.Length - 1) {}

                        Dim Position As Integer = 0
                        While Position < EncryptedData.Length
                            Array.Copy(incPKGFileKey, 0, PKGFileKeyBytes, Position, PKGFileKey.Length)

                            Utils.IncrementArray(incPKGFileKey, PKGFileKey.Length - 1)
                            Position += AesKey.Length
                        End While

                        PKGXorKeyBytes = AESEngine.Encrypt(PKGFileKeyBytes, AesKey, AesKey, CipherMode.ECB, PaddingMode.None)
                        DecryptedData = XOREngine.GetXOR(EncryptedData, 0, PKGXorKeyBytes.Length, PKGXorKeyBytes)
                        DecryptedFileBinaryWriter.Write(DecryptedData)
                    Next

                    DecryptedFileWriteStream.Close()
                    DecryptedFileBinaryWriter.Close()
                End Using
            End Using

            Return PKGFileName & ".Dec"
        Catch ex As Exception

            Return String.Empty
        End Try
    End Function

    Private Shared Function NewBitmapImage(imageData As Byte()) As BitmapImage
        If imageData Is Nothing OrElse imageData.Length = 0 Then Return Nothing
        Dim image = New BitmapImage()
        Using mem = New MemoryStream(imageData)
            mem.Position = 0
            image.BeginInit()
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat
            image.CacheOption = BitmapCacheOption.OnLoad
            image.UriSource = Nothing
            image.StreamSource = mem
            image.EndInit()
        End Using
        image.Freeze()
        Return image
    End Function

#Region "PS5"

    'Converted from https://github.com/zecoxao/pkgdec5
    'Added EndianIO(Extension) as C# class library

    Public Class RSAKeyset
        ' d
        Public PrivateExponent As Byte()
        ' exponent1 = d mod (p - 1)
        Public Exponent1 As Byte()
        ' exponent2 = d mod (q - 1)
        Public Exponent2 As Byte()
        ' e
        Public PublicExponent As Byte()
        ' (InverseQ)(q) = 1 mod p
        Public Coefficient As Byte()
        ' n = p * q
        Public Modulus As Byte()
        ' p
        Public Prime1 As Byte()
        ' q
        Public Prime2 As Byte()

        ''' <summary>
        ''' Modulus is in PkgPublicKeys[3], fortunately we have the whole thing!
        ''' </summary>
        Public Shared PkgDerivedKey3Keyset As New RSAKeyset With {
            .Prime1 = New Byte() {
            &HD8, &H4F, &H78, &H93, &H8F, &H31, &HF4, &H56, &HE8, &H28, &HCF, &H28, &H90, &H62, &H4, &HD9,
            &H36, &H99, &HF6, &HA3, &H19, &H6E, &HC7, &H27, &H53, &H6D, &HFB, &H68, &H5E, &H63, &HC4, &HCF,
            &HAD, &H76, &H7, &H88, &H1F, &H6F, &H3F, &HBD, &H86, &HBD, &H3A, &H5, &H62, &HC5, &H22, &HFD,
            &HA, &H42, &H7D, &H12, &H2, &HC3, &H77, &HCE, &HE3, &H73, &HC9, &H51, &HE7, &H63, &H7, &H29,
            &H89, &H0, &HF2, &H91, &H5E, &HE5, &HDD, &HB1, &H3F, &H96, &H14, &HBA, &HC3, &H5F, &HD2, &H2B,
            &H34, &HBD, &HA8, &H5B, &HFF, &H86, &HBC, &HC7, &H1E, &H98, &H8F, &H64, &H22, &HE3, &HA0, &H2E,
            &HC9, &HD1, &H8D, &H44, &HE4, &HC0, &HD0, &H54, &H5D, &HBA, &H7E, &HC6, &H59, &H3A, &HAE, &HCB,
            &HE, &H1D, &H1E, &HB3, &HDD, &H7F, &H61, &H35, &H3B, &HF4, &H88, &H11, &HFB, &HBB, &H6F, &HA5,
            &HD, &HF5, &H35, &H7F, &H38, &HE8, &H7, &HE1, &HC3, &HC3, &HFE, &HF1, &H52, &HCB, &HC6, &HB2,
            &HC2, &HB4, &H67, &H4F, &H3D, &H7D, &H44, &H39, &HC8, &HEE, &HA0, &HEF, &H17, &HB4, &H0, &HA2,
            &H2, &HD2, &H3E, &H93, &H39, &H4A, &HA2, &HB2, &HF, &H57, &H7A, &H6, &H15, &H28, &HF1, &HB8,
            &HD5, &HC8, &H53, &HD0, &H7F, &H35, &HA7, &H53, &HCB, &H24, &H37, &H3E, &HE0, &H5, &HC5, &HC9},
            .Prime2 = New Byte() {
            &HCA, &H83, &H67, &H7F, &HF3, &H9E, &H73, &H47, &HD9, &HF, &H99, &H55, &HC5, &H5A, &H56, &H57,
            &HC3, &H54, &H3B, &HA9, &H66, &HBA, &H86, &H10, &HE0, &HB1, &H2F, &HC2, &H96, &HD5, &HF1, &HD1,
            &HD8, &HCF, &HF2, &H7D, &H3, &HAE, &HCE, &HEC, &HCC, &H77, &H6, &H5F, &H31, &H99, &H9E, &H3A,
            &H84, &H37, &HB1, &H86, &H24, &H13, &H75, &H75, &H9E, &HAA, &H8C, &H8D, &H66, &HCB, &H5F, &H4A,
            &HB7, &HAD, &H64, &H18, &H9C, &H5C, &H63, &H4C, &H7D, &HB3, &H73, &H70, &HE2, &H82, &H24, &HE3,
            &H2E, &HCB, &HCA, &H9, &HB0, &H8E, &HDF, &H64, &HA9, &H9E, &H3E, &H62, &HD9, &HB4, &HA1, &HA6,
            &HC7, &H5E, &HAC, &H51, &HB1, &H82, &HE3, &HD5, &H6D, &HD0, &H71, &HE2, &H38, &HBD, &H56, &H41,
            &HD9, &H9E, &HCB, &HE2, &H91, &HEB, &H5F, &H48, &HFB, &HFA, &H53, &H43, &H6, &HB8, &H7D, &H60,
            &HE4, &H40, &H1D, &H18, &H4B, &HE0, &H5A, &H23, &H69, &HCF, &H39, &HE0, &H59, &HFB, &H47, &HC3,
            &HB5, &H3, &HF4, &HAA, &HA8, &H82, &HF3, &H7D, &H37, &H61, &HDE, &HCE, &H5E, &HA7, &HD, &H87,
            &H1E, &H9, &HB3, &H76, &HAA, &H54, &HEF, &H33, &HAA, &HBD, &HF2, &H78, &HED, &H68, &HB2, &HE2,
            &H51, &H66, &H81, &H7, &H7C, &HEE, &H51, &H6F, &H2E, &H7C, &H59, &H3, &H35, &H8E, &H52, &H69},
            .PrivateExponent = New Byte() {
            &H8E, &H4, &HF3, &HC5, &H2C, &H71, &H85, &H76, &H5F, &H85, &H3C, &H55, &HE5, &H29, &H9C, &HD4,
            &HA3, &HCE, &H14, &HCB, &HAA, &HE4, &H89, &H1, &H3A, &HDF, &HB9, &H66, &H98, &H45, &HDF, &H9,
            &HAC, &H41, &H11, &H50, &H88, &HB, &H71, &HFD, &H55, &H52, &HFC, &HBC, &H46, &HFB, &H44, &H38,
            &H1E, &H26, &HE2, &HE6, &H29, &H7A, &H65, &HEB, &HA1, &HCF, &H1A, &H48, &H26, &H69, &H1E, &HE9,
            &H6E, &H7, &HB3, &H34, &H1D, &HD8, &H6A, &HB4, &H6B, &H51, &HA7, &H85, &HC8, &HC0, &H82, &HF5,
            &H93, &HFF, &H4B, &H42, &H17, &HCA, &H52, &HA5, &H8A, &HD7, &H33, &H33, &HC0, &HD6, &H27, &HFD,
            &HA9, &H92, &H88, &H85, &H22, &H92, &H70, &HC4, &HA6, &H49, &HCD, &HE9, &H18, &H60, &H26, &HC8,
            &HA5, &HA, &H63, &H6A, &HCF, &HC9, &H1F, &HCF, &HB7, &HCF, &H4F, &H8D, &HB1, &HC5, &HE3, &HAA,
            &HC, &H14, &H2, &HA, &HF1, &HC9, &H8, &HFD, &H51, &HCF, &H2, &H22, &H98, &HA4, &HE5, &HCD, &H20,
            &HEE, &H57, &H9B, &HA, &H61, &HBB, &H58, &HF6, &H98, &HD0, &H5C, &H41, &H96, &H8F, &H8C, &H24,
            &H4, &HF2, &HDA, &H79, &H64, &HE2, &HC, &HDB, &H54, &H65, &H9E, &HDF, &H6E, &HA0, &HFE, &HFD,
            &HC8, &H23, &H16, &HF9, &H58, &HFD, &H66, &HBC, &H40, &HCA, &H1, &H81, &HD7, &H67, &H90, &HF3,
            &H28, &HD2, &HE, &HC9, &H3B, &HF5, &HCA, &HF6, &HAB, &HDD, &HA3, &HFF, &H89, &HFE, &HA2, &H47,
            &H43, &H8A, &HC8, &H25, &HAF, &HD8, &H82, &H2E, &H13, &H89, &H70, &HFE, &H8E, &HFB, &H19, &HDD,
            &HD3, &H73, &HA5, &HCE, &HCB, &HBF, &HCC, &H2E, &H4, &H79, &H58, &HFC, &HD8, &HE7, &HAD, &H3A,
            &H5A, &H6C, &H33, &H9D, &H98, &HFB, &H79, &H47, &HEA, &H3, &H4D, &H72, &H4B, &H90, &H36, &H48,
            &H7A, &H8E, &H0, &H69, &H49, &H1E, &H1A, &HD4, &H97, &HE1, &HE8, &H57, &H95, &H74, &HE2, &H9E,
            &HEF, &HA6, &H2A, &HD2, &H25, &H1D, &H83, &HDA, &HD7, &H3A, &H4F, &H1A, &HAA, &HAC, &HF7, &H1E,
            &HDF, &H35, &H10, &H55, &H7D, &H8D, &HB4, &H71, &H4F, &HD0, &H5D, &H63, &HDC, &H74, &HEA, &HE3,
            &H62, &H1D, &H2B, &H4, &H6, &HC5, &H12, &H6F, &HC7, &HD6, &HA1, &HB, &H99, &H56, &H38, &H9C,
            &H75, &H56, &HCB, &HDA, &H51, &HC4, &H4B, &H5D, &HAC, &H87, &HBB, &H97, &HD6, &H46, &H8D, &HA7,
            &H1E, &H27, &HD5, &H83, &H2E, &HFA, &H96, &H0, &H48, &HD0, &H53, &HA4, &H0, &HC3, &HAC, &HFE,
            &H2A, &HBA, &H68, &HA3, &HA1, &HAF, &H4F, &H43, &H7E, &HA1, &HAB, &HBC, &H31, &HCD, &H79, &HA5,
            &H14, &H70, &H7D, &H61, &H80, &HBF, &HFD, &H58, &HDA, &H7C, &H2A, &H44, &HAB, &HBF, &H41},
            .Exponent1 = New Byte() {
            &H7, &H78, &H1F, &HA, &HC1, &H5C, &H11, &H3A, &HDB, &H3, &H65, &HBB, &HD9, &HD8, &H78, &HA0,
            &H63, &H81, &H47, &H81, &HF4, &H43, &HDD, &HFE, &H9E, &HA3, &HE2, &H95, &H85, &H4, &HDE, &HEB,
            &HE8, &HEA, &H75, &H72, &H1E, &HDB, &HC1, &H90, &HB2, &HD1, &H5F, &HEA, &H85, &HB1, &H96, &HF6,
            &HB3, &HDE, &HFD, &HE0, &H9C, &H55, &HD1, &H92, &H44, &H4A, &H60, &H3E, &H42, &HC6, &H29, &H9E,
            &H26, &H8B, &HF0, &HD4, &H52, &H39, &H8F, &HC1, &H2A, &H17, &HED, &H99, &H51, &H5B, &HC2, &HAF,
            &H19, &H40, &H1F, &H4B, &H25, &HF4, &HAA, &H1A, &H1A, &H15, &H5C, &H86, &H31, &HAA, &H38, &H82,
            &HC5, &H17, &H46, &H50, &H85, &HB1, &H9E, &HBF, &HFB, &H8, &H90, &H8E, &H1A, &HD0, &HAA, &HEE,
            &H7A, &HB, &H49, &H5F, &H1E, &H9B, &HE2, &H68, &H6B, &H2C, &H93, &H72, &H43, &H86, &H2, &H61,
            &HE9, &HAC, &H78, &HEF, &H6E, &HB0, &H9C, &H6D, &H10, &H4C, &H79, &H46, &H2D, &HFC, &HB9, &H5C,
            &HBC, &HDA, &H6B, &HE2, &HD1, &H95, &HBC, &HC0, &H5E, &HE, &HD7, &H61, &HCA, &H28, &HBE, &H8,
            &HDA, &H1E, &H16, &H69, &H11, &H6, &H61, &HBD, &HD2, &H47, &HCB, &HFF, &HDF, &HC5, &H2D, &H2B,
            &H9B, &HBE, &H32, &H1E, &HB5, &HF5, &HCD, &H54, &H58, &H64, &H64, &HBF, &HF8, &HE, &H5A, &HF9},
            .Exponent2 = New Byte() {
            &H3C, &H99, &H63, &HB0, &H43, &H1B, &H48, &HD, &HD8, &HE3, &H35, &H14, &H18, &H71, &H36, &HE3,
            &H1E, &H3D, &H27, &H79, &H42, &H97, &H50, &H24, &HDE, &HC7, &HC6, &HAD, &HE8, &HEA, &HEE, &H68,
            &HC8, &H3, &H39, &HE1, &HB4, &HE7, &H6B, &H5E, &H2A, &HB4, &HF7, &H40, &H27, &H1C, &H7B, &HDF,
            &HB0, &HCE, &HE5, &H9D, &H69, &H50, &H35, &H56, &HD3, &HFA, &HDF, &H2, &H35, &H1F, &H68, &H4D,
            &H78, &H77, &H37, &H3B, &HB2, &H16, &H67, &H54, &H6D, &H4C, &HF4, &H9F, &H73, &HF8, &H53, &HC7,
            &H73, &HAA, &H61, &HB3, &HD2, &H94, &H7E, &H3E, &HA6, &HF, &H7, &H46, &H17, &H35, &H59, &H26,
            &HA, &H4, &HC7, &H75, &HCE, &HB3, &H87, &H2F, &HC7, &HA3, &H97, &H60, &H85, &H70, &HA, &HCE,
            &HBB, &HAB, &H2C, &H1, &H89, &H7E, &HB0, &H4D, &HAB, &HB1, &H35, &H97, &H19, &HFC, &HBC, &HEF,
            &HF0, &H7D, &H4A, &HF7, &H89, &H45, &H2, &H54, &H14, &H86, &H81, &H20, &H24, &H6C, &HF0, &H5,
            &H9D, &H36, &H28, &HD1, &HA4, &H89, &H43, &H9, &H56, &H38, &H40, &H2E, &HEA, &HDD, &HFC, &H4B,
            &H51, &H6E, &HBF, &HB8, &H23, &HB2, &H34, &HBD, &HF6, &H3A, &HCE, &HC2, &HE6, &HEF, &HEC, &H8F,
            &H92, &HA2, &H24, &HBC, &H33, &HE3, &H30, &H95, &H1F, &H88, &HF0, &H2D, &HE8, &HA9, &HC4, &HF9},
            .Coefficient = New Byte() {
            &H5C, &H50, &HEF, &H23, &H14, &HDB, &HE1, &HCF, &H19, &H66, &H8A, &H93, &H4D, &HDC, &HE7, &H62,
            &H34, &H72, &HA5, &H2F, &HFD, &HA7, &H69, &H0, &HCE, &H5, &H6C, &H9A, &H7A, &H40, &H5A, &H55,
            &H9D, &H81, &H4E, &H49, &HFC, &HF3, &H72, &H36, &H18, &H62, &H7A, &H54, &H68, &H36, &H3D, &H90,
            &H8E, &HF4, &HEE, &H26, &H33, &H14, &H66, &H36, &H6A, &H1E, &H66, &H2D, &H5B, &H25, &H52, &H10,
            &H5D, &H85, &H21, &H11, &HB9, &H91, &HDE, &H79, &H10, &HE2, &H9A, &H25, &HAF, &H3B, &H14, &H2C,
            &H30, &HDF, &H3C, &H5B, &H8D, &HFF, &HE8, &H9C, &H35, &H96, &HC6, &HF5, &H63, &H9, &HE8, &H41,
            &H9E, &HD9, &H61, &H55, &H94, &H98, &H2F, &HD9, &H86, &H5, &H32, &H1, &H23, &H86, &H74, &HDC,
            &H12, &H4A, &HF9, &HD5, &HB4, &HFD, &HA5, &H9E, &H6D, &H28, &HAE, &H2, &HDB, &HEC, &HE0, &HCF,
            &HB2, &HC3, &HAC, &H6C, &HBE, &HEE, &H64, &H20, &H63, &HB4, &H8E, &HA7, &HF0, &H69, &H96, &HBD,
            &HEC, &H4D, &HA7, &HF8, &H16, &H14, &H3C, &HDA, &H67, &H69, &HFC, &HB5, &H84, &H47, &H10, &H71,
            &HAC, &H64, &H24, &HBD, &H94, &H3E, &H8A, &HE3, &HDF, &HB4, &HA9, &H54, &H73, &H1E, &H4C, &HD3,
            &HB8, &HF9, &H8, &HCC, &H1D, &H85, &H3B, &HC1, &HCC, &HA, &HCF, &H47, &HBB, &HAD, &H6B, &H7B},
            .Modulus = New Byte() {
            &HAB, &H1D, &HBD, &H43, &H39, &H49, &H33, &H16, &HA3, &H5C, &H40, &H4E, &H2C, &H22, &H97, &HB8,
            &H33, &H68, &H5C, &H1A, &HD3, &H54, &HE8, &HC5, &HBA, &H78, &H88, &HD1, &HB0, &HFA, &HF2, &H5A,
            &H8F, &H14, &HAA, &H6, &H52, &H8F, &HA4, &H65, &H86, &H6E, &HD4, &H23, &H3, &HD3, &H0, &H91,
            &HB, &HD9, &HD8, &H41, &H1, &HFE, &H54, &HC1, &H2B, &HFC, &H4F, &H7F, &H9C, &H3A, &H7A, &HC9,
            &H13, &H33, &HFD, &H2C, &HDC, &HCB, &H14, &H0, &H76, &H1A, &HDE, &H5C, &H2E, &HBC, &HA0, &H11,
            &H6D, &H8C, &H30, &H4B, &H8B, &H47, &HF3, &H3C, &H41, &H37, &H72, &H84, &H9E, &H9E, &H1D, &H18,
            &H3B, &H4D, &H7B, &HBC, &H99, &H4C, &H37, &HED, &H78, &H87, &HD4, &H86, &H94, &H23, &H4B, &H71,
            &HAC, &HCB, &H4D, &HB9, &H50, &H70, &H33, &H66, &H18, &H97, &H6E, &HD6, &H7B, &H1C, &H40, &H1A,
            &H21, &H13, &HD4, &H39, &H88, &H3, &H40, &H49, &H9F, &H65, &H6B, &H7A, &HEE, &HB3, &H86, &HC0,
            &H67, &H98, &HC2, &HD1, &H44, &HEB, &HB5, &H84, &HB5, &H65, &H7B, &H28, &HE2, &H90, &H94, &H49,
            &H31, &H79, &H9B, &HB, &H9, &HB2, &H71, &HA1, &HD9, &H37, &HB, &HFE, &H4F, &H84, &HBA, &HCC,
            &H78, &HEA, &H3C, &H91, &H7D, &H30, &HD, &H53, &HD5, &HC5, &H6A, &H34, &HB, &H2B, &H7, &H56,
            &H8, &HF, &H28, &H32, &H53, &H63, &HEB, &H9B, &HC8, &H4E, &HB9, &H1D, &H70, &H46, &H8E, &HEF,
            &H8B, &HD4, &HAB, &H30, &H2F, &H13, &HF3, &H0, &H41, &H70, &H95, &H79, &HCA, &HA5, &H4E, &H8B,
            &HD7, &H64, &H23, &H56, &HEC, &H85, &H23, &HA, &H15, &H14, &HE0, &H6, &H67, &H56, &H84, &H23,
            &H8, &H1D, &H64, &H39, &H96, &H88, &H33, &HA5, &H1C, &H5B, &H2F, &HC7, &HB6, &HEF, &H0, &H62,
            &H3F, &HB7, &H25, &H89, &H9A, &H29, &H67, &HCB, &HC1, &H4C, &HEE, &HAE, &HFE, &H87, &H47, &H28,
            &H2, &H95, &HA3, &H1C, &H90, &H89, &H59, &HB3, &H7E, &HCE, &HB0, &H6, &H41, &H82, &HC5, &H33,
            &H66, &H4D, &HED, &H63, &H55, &HFF, &H31, &H3C, &HF8, &H2A, &H89, &H1A, &H42, &HDC, &H88, &H65,
            &H5F, &HDD, &HFE, &H71, &HE6, &H50, &HE5, &H1B, &H14, &H90, &HA8, &H88, &HCE, &H38, &HD6, &HFB,
            &H85, &HE, &H20, &HD1, &H24, &H8, &HCD, &HB0, &HF0, &HEF, &HAB, &H2F, &HF1, &H9F, &H9A, &H95,
            &H80, &H2D, &H43, &H75, &H60, &HC0, &HC9, &H86, &HC5, &HF2, &HCB, &HB2, &HE, &H2B, &H89, &H7F,
            &H6B, &HCB, &H67, &HA5, &H65, &H7B, &H47, &H24, &HDB, &HDA, &H2C, &HB3, &H8F, &HE2, &H3D, &H73,
            &H8C, &HF2, &H6F, &H8C, &HC0, &H6E, &HF, &H12, &H21, &HFE, &H74, &HD, &HE, &H36, &H81, &H71},
            .PublicExponent = New Byte() {0, 1, 0, 1}
        }
    End Class

    Public Shared Function RSA2048Decrypt(ciphertext As Byte(), keyset As RSAKeyset) As Byte()
        Dim NewRSACryptoServiceProvider As New RSACryptoServiceProvider()
        NewRSACryptoServiceProvider.ImportParameters(New RSAParameters With {
                             .P = keyset.Prime1,
                             .Q = keyset.Prime2,
                             .Exponent = keyset.PublicExponent,
                             .Modulus = keyset.Modulus,
                             .DP = keyset.Exponent1,
                             .DQ = keyset.Exponent2,
                             .InverseQ = keyset.Coefficient,
                             .D = keyset.PrivateExponent
                             })
        Return NewRSACryptoServiceProvider.Decrypt(ciphertext, False)
    End Function

    Public Structure PackageEntry
        Public type As UInteger
        Public unk1 As UInteger
        Public flags1 As UInteger
        Public flags2 As UInteger
        Public offset As UInteger
        Public size As UInteger
        Public padding As Byte()

        Public key_index As UInteger
        Public is_encrypted As Boolean

        Public Function ToArray() As Byte()
            Dim ms = New MemoryStream()
            Dim writer = New EndianWriter(ms, EndianType.BigEndian)

            writer.Write(type)
            writer.Write(unk1)
            writer.Write(flags1)
            writer.Write(flags2)
            writer.Write(offset)
            writer.Write(size)
            writer.Write(padding)

            writer.Close()

            Return ms.ToArray()
        End Function
    End Structure

    Public Shared Function AesCbcCfb128Decrypt(output As Byte(), input As Byte(), size As UInteger, key As Byte(), iv As Byte()) As Integer
        Using cipher As Aes = Aes.Create()

            cipher.Mode = CipherMode.CBC
            cipher.KeySize = 128
            cipher.Key = key
            cipher.IV = iv
            cipher.Padding = PaddingMode.None
            cipher.BlockSize = 128

            Dim TempByte = New Byte(CInt(size - 1)) {}
            Using ct_stream = New MemoryStream(input)
                Using pt_stream = New MemoryStream(TempByte)
                    Using dec = cipher.CreateDecryptor(key, iv)
                        Using s = New CryptoStream(ct_stream, dec, CryptoStreamMode.Read)
                            s.CopyTo(pt_stream)
                        End Using
                    End Using
                End Using
            End Using

            Buffer.BlockCopy(TempByte, 0, output, 0, TempByte.Length)
        End Using

        Return 0
    End Function

#End Region

End Class