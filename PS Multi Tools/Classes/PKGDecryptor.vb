Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports PS4_Tools.Util

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
            EncryptedPKGStream.Read(EncryptedData, 0, InputSize)

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

    Public Function GetBytesFromFile(FileName As String) As Byte()
        Dim BytesFromFile As Byte()

        Try
            Dim ByteArray As Byte() = Nothing

            Using FileStream As New FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                ByteArray = New Byte(CInt(1048576 - 1L) + 1 - 1) {}
                FileStream.Read(ByteArray, 0, ByteArray.Length)
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

            If MsgBox("Pkg extracted successfully." + vbNewLine + "Open folder?", MsgBoxStyle.OkCancel, "Done") = MsgBoxResult.Ok Then
                Process.Start(".\")
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

    Private Function NewBitmapImage(imageData As Byte()) As BitmapImage
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

End Class