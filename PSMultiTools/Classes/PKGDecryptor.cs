using IronSoftware.Drawing;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PSMultiTools.Classes
{

    public class PKGDecryptor
    {

        public byte[] PARAMSFO;
        public AnyBitmap? ICON0;
        public AnyBitmap? PIC0;
        public AnyBitmap? PIC1;
        public AnyBitmap? PIC2;
        public byte[]? SND0;
        public byte[] PBPBytes;
        public PKGType PackageType;
        public PKGPlatform PackagePlatform;
        public string PKGContentID;
        public bool IsDecError;
        public byte[] PSPAesKey = [7, 242, 198, 130, 144, 181, 13, 44, 51, 129, 141, 112, 155, 96, 230, 43];
        public byte[] PS3AesKey = [46, 123, 113, 215, 201, 201, 161, 78, 163, 34, 31, 24, 136, 40, 184, 248];
        public byte[] AesKey = new byte[16];
        public byte[] PKGFileKey = new byte[16];
        public uint UIEncryptedFileStartOffset = 0U;
        public bool IsSupportedFiles;

        public PKGDecryptor()
        {
            PARAMSFO = new byte[524289];
            ICON0 = null;
            PIC0 = null;
            PIC1 = null;
            PIC2 = null;
            SND0 = null;
            PBPBytes = new byte[5242881];
            PackageType = new PKGType();
            PKGContentID = string.Empty;
            IsDecError = false;
            IsSupportedFiles = false;
        }

        public enum PKGFiles
        {
            ICON0,
            PIC0,
            PIC1,
            PIC2,
            SND0
        }

        public enum PKGType
        {
            Debug,
            Retail,
            Retail_PSX_PSP
        }

        public enum PKGPlatform
        {
            PS3,
            PSP
        }

        public AnyBitmap? GetImage(PKGFiles PKGIMG)
        {
            switch (PKGIMG)
            {
                case PKGFiles.ICON0:
                    {
                        return ICON0;
                    }
                case PKGFiles.PIC0:
                    {
                        return PIC0;
                    }
                case PKGFiles.PIC1:
                    {
                        return PIC1;
                    }
                case PKGFiles.PIC2:
                    {
                        return PIC2;
                    }

                default:
                    {
                        return null;
                    }
            }
        }

        public byte[] GetPARAMSFO
        {
            get
            {
                return PARAMSFO;
            }
        }

        public byte[] GetSND
        {
            get
            {
                return SND0!;
            }
        }

        public byte[] GetPBPBytes
        {
            get
            {
                return PBPBytes;
            }
        }

        public PKGType GetPKGType
        {
            get
            {
                return PackageType;
            }
        }

        public PKGPlatform GetPKGPlatform
        {
            get
            {
                return PackagePlatform;
            }
        }

        public string ContentID
        {
            get
            {
                return PKGContentID;
            }
        }

        public void ProcessPKGFile(string PKGFile)
        {
            byte[] DecryptedPKG = DecryptPKGFileRead(PKGFile)!;
            byte[] EncryptedPKG = GetBytesFromFile(PKGFile)!;
            if (DecryptedPKG is not null)
            {
                ExtractPKGFilesRead(DecryptedPKG, EncryptedPKG);
            }
        }

        public byte[]? DecryptPKGFileRead(string PKGFileName)
        {
            try
            {
                int Multiplicator = 65536;
                byte[] ByteArray = new byte[1048576];
                byte[] EncryptedData = new byte[(AesKey.Length * Multiplicator)];
                byte[] DecryptedData = new byte[(AesKey.Length * Multiplicator)];
                byte[] PKGXorKey = new byte[AesKey.Length];
                byte[] EncryptedFileStartOffset = new byte[4];
                byte[] EncryptedFileLenght = new byte[4];

                using (var PKGReadStream = new FileStream(PKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using var PKGBinaryReader = new BinaryReader(PKGReadStream);

                    byte[] PKGMagic = PKGBinaryReader.ReadBytes(4);
                    if (PKGMagic[0] != 127 || PKGMagic[1] != 80 || PKGMagic[2] != 75 || PKGMagic[3] != 71)
                    {
                        // Selected file isn't a Pkg file. Error!
                    }

                    // Finalized byte
                    PKGReadStream.Seek(4L, SeekOrigin.Begin);
                    byte PKGFinalized = PKGBinaryReader.ReadByte();

                    if (PKGFinalized != 128)
                    {
                        // This is debug PKG and is not supported!
                    }

                    PKGReadStream.Seek(48L, SeekOrigin.Begin);
                    PKGContentID = Encoding.ASCII.GetString(PKGBinaryReader.ReadBytes(36));

                    // PKG Type PSP/PS3
                    PKGReadStream.Seek(7L, SeekOrigin.Begin);
                    byte PKGType = PKGBinaryReader.ReadByte();

                    switch (PKGType)
                    {
                        case 1:
                            {
                                // PS3
                                PackagePlatform = PKGPlatform.PS3;
                                AesKey = PS3AesKey;
                                break;
                            }
                        case 2:
                            {
                                // PSP
                                PackagePlatform = PKGPlatform.PSP;
                                AesKey = PSPAesKey;
                                break;
                            }

                        default:
                            {
                                // Invalid PKG file
                                break;
                            }
                    }

                    PKGReadStream.Seek(36L, SeekOrigin.Begin);
                    EncryptedFileStartOffset = PKGBinaryReader.ReadBytes(EncryptedFileStartOffset.Length);
                    Array.Reverse(EncryptedFileStartOffset);
                    UIEncryptedFileStartOffset = BitConverter.ToUInt32(EncryptedFileStartOffset, 0);

                    PKGReadStream.Seek(44L, SeekOrigin.Begin);
                    EncryptedFileLenght = PKGBinaryReader.ReadBytes(EncryptedFileLenght.Length);
                    Array.Reverse(EncryptedFileLenght);
                    uint UIEncryptedFileLenght = BitConverter.ToUInt32(EncryptedFileLenght, 0);

                    PKGReadStream.Seek(112L, SeekOrigin.Begin);
                    PKGFileKey = PKGBinaryReader.ReadBytes(16);
                    byte[] IncPKGFileKey = new byte[16];
                    Array.Copy(PKGFileKey, IncPKGFileKey, PKGFileKey.Length);

                    PKGXorKey = AESEngine.Encrypt(PKGFileKey, AesKey, AesKey, CipherMode.ECB, PaddingMode.None);

                    double Division = UIEncryptedFileLenght / (double)AesKey.Length;
                    ulong Pieces = Convert.ToUInt64(Math.Floor(Division));
                    ulong CustomMod = (ulong)Math.Round(Convert.ToUInt64(UIEncryptedFileLenght) / (double)Convert.ToUInt64(AesKey.Length));
                    if (CustomMod > 0m)
                    {
                        Pieces += 1UL;
                    }

                    using var DecryptedFileMemoryStream = new MemoryStream();
                    PKGReadStream.Seek(UIEncryptedFileStartOffset, SeekOrigin.Begin);

                    double FileDivision = UIEncryptedFileLenght / (double)(AesKey.Length * Multiplicator);
                    ulong FilePieces = Convert.ToUInt64(Math.Floor(FileDivision));
                    ulong FileMod = Convert.ToUInt64(UIEncryptedFileLenght) % Convert.ToUInt64(AesKey.Length * Multiplicator);
                    if (FileMod > 0m)
                    {
                        FilePieces += 1UL;
                    }

                    ulong MUInt64 = Convert.ToUInt64(decimal.Subtract(new decimal(FileDivision), 1m));
                    if (0m <= MUInt64)
                    {
                        if (FileMod > 0m && FilePieces - 1m == 0m)
                        {
                            EncryptedData = new byte[(int)Math.Round(FileMod - 1m) + 1];
                            DecryptedData = new byte[(int)Math.Round(FileMod - 1m) + 1];
                        }

                        // Read 16 bytes of Encrypted data
                        EncryptedData = PKGBinaryReader.ReadBytes(EncryptedData.Length);

                        // In order to retrieve a fast AES Encryption we pre-Increment the array
                        byte[] PKGFileKeyConsec = new byte[EncryptedData.Length];
                        byte[] PKGXorKeyConsec = new byte[EncryptedData.Length];

                        int Position = 0;
                        while (Position < EncryptedData.Length)
                        {
                            Array.Copy(IncPKGFileKey, 0, PKGFileKeyConsec, Position, PKGFileKey.Length);
                            Utils.IncrementArray(ref IncPKGFileKey, PKGFileKey.Length - 1);
                            Position += AesKey.Length;
                        }

                        PKGXorKeyConsec = AESEngine.Encrypt(PKGFileKeyConsec, AesKey, AesKey, CipherMode.ECB, PaddingMode.None);
                        DecryptedData = XOREngine.GetXOR(EncryptedData, 0, PKGXorKeyConsec.Length, PKGXorKeyConsec);
                        DecryptedFileMemoryStream.Write(DecryptedData, 0, DecryptedData.Length);

                        if (DecryptedData.Length >= 1048576)
                        {
                            ByteArray = DecryptedFileMemoryStream.ToArray();
                        }
                    }

                    // For i As ULong = 0 To filepieces - 1

                    // Next

                }

                return ByteArray;
            }
            catch (Exception)
            {
                // Could not read decrypt PKG file.
                return null;
            }
        }

        public byte[]? DecryptPKGDataRead(int DataSize, long DataRelativeOffset, long PKGEncryptedFileStartOffset, byte[] AESKey, Stream EncryptedPKGReadStream, Stream EncryptedPKGStream)
        {
            try
            {
                int InputSize = DataSize % 16;
                if (InputSize > 0)
                {
                    InputSize = (DataSize / 16 + 1) * 16;
                }
                else
                {
                    InputSize = DataSize;
                }

                byte[] EncryptedData = new byte[InputSize];
                byte[] DecryptedData = new byte[InputSize];
                byte[] PKGFileKeyBytes = new byte[InputSize];
                byte[] PKGXorKeyBytes = new byte[InputSize];
                byte[] IncPKGFileKey = new byte[PKGFileKey.Length];
                Array.Copy(PKGFileKey, IncPKGFileKey, PKGFileKey.Length);

                EncryptedPKGReadStream.Seek(DataRelativeOffset + PKGEncryptedFileStartOffset, SeekOrigin.Begin);
                EncryptedPKGStream.ReadExactly(EncryptedData, 0, InputSize);

                for (int Position = 0, loopTo = (int)(DataRelativeOffset - 1L); Position <= loopTo; Position += 16)
                    Utils.IncrementArray(ref IncPKGFileKey, PKGFileKey.Length - 1);

                for (int Position = 0, loopTo1 = InputSize - 1; Position <= loopTo1; Position += 16)
                {
                    Array.Copy(IncPKGFileKey, 0, PKGFileKeyBytes, Position, PKGFileKey.Length);
                    Utils.IncrementArray(ref IncPKGFileKey, PKGFileKey.Length - 1);
                }

                PKGXorKeyBytes = AESEngine.Encrypt(PKGFileKeyBytes, AESKey, AESKey, CipherMode.ECB, PaddingMode.None);
                DecryptedData = XOREngine.GetXOR(EncryptedData, 0, PKGXorKeyBytes.Length, PKGXorKeyBytes);
                return DecryptedData;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool ExtractPKGFilesRead(byte[] DecryptedPKGFileName, byte[] EncryptedPKGFileName)
        {
            try
            {
                int TwentyMB = 20971520;
                uint ExtractedFileOffset = 0U;
                uint ExtractedFileSize = 0U;
                uint OffsetShift = 0U;
                long PositionIndex = 0L;
                byte[] FileTable = new byte[320000];
                byte[] SDKVer = new byte[8];
                byte[] FirstFileOffset = new byte[4];
                byte[] FirstNameOffset = new byte[4];
                byte[] FileNr = new byte[4];
                byte[] IsDir = new byte[4];
                byte[] Offset = new byte[4];
                byte[] Size = new byte[4];
                byte[] NameOffset = new byte[4];
                byte[] NameSize = new byte[4];
                byte[] Name = new byte[32];
                byte[] BootMagic = new byte[8];
                byte ContentType = 0;
                byte FileType = 0;
                bool IsFile = false;
                byte[] DumpFile;
                var DecryptedPKGReadStream = new MemoryStream(DecryptedPKGFileName);
                var DecryptedPKGMemoryStream = DecryptedPKGReadStream;
                var EncryptedPKGReadStream = new MemoryStream(EncryptedPKGFileName);
                var EncryptedPKGMemoryStream = EncryptedPKGReadStream;

                // Read the file table
                DecryptedPKGReadStream.Seek(0L, SeekOrigin.Begin);
                DecryptedPKGMemoryStream.Read(FileTable, 0, FileTable.Length);

                PositionIndex = 0L;
                OffsetShift = 0U;

                // Shift Relative to os.raw
                Array.Copy(FileTable, 0, FirstNameOffset, 0, FirstNameOffset.Length);
                Array.Reverse(FirstNameOffset);

                uint UIFirstNameOffset = BitConverter.ToUInt32(FirstNameOffset, 0);
                uint UIFileNr = (uint)(UIFirstNameOffset / 32L);

                Array.Copy(FileTable, 12, FirstFileOffset, 0, FirstFileOffset.Length);
                Array.Reverse(FirstFileOffset);

                uint UIfirstFileOffset = BitConverter.ToUInt32(FirstFileOffset, 0);

                // Read the file table
                DecryptedPKGReadStream.Seek(0L, SeekOrigin.Begin);
                DecryptedPKGMemoryStream.Read(FileTable, 0, (int)UIfirstFileOffset);

                if ((int)UIFileNr < 0)
                {
                    // Unsupported PKG file
                    return false;
                }

                int WhileInt = 0;
                while (WhileInt <= (int)UIFileNr - 1)
                {
                    Array.Copy(FileTable, PositionIndex + 12L, Offset, 0L, Offset.Length);
                    Array.Reverse(Offset);
                    ExtractedFileOffset = BitConverter.ToUInt32(Offset, 0) + OffsetShift;

                    Array.Copy(FileTable, PositionIndex + 20L, Size, 0L, Size.Length);
                    Array.Reverse(Size);
                    ExtractedFileSize = BitConverter.ToUInt32(Size, 0);

                    Array.Copy(FileTable, PositionIndex, NameOffset, 0L, NameOffset.Length);
                    Array.Reverse(NameOffset);
                    uint ExtractedFileNameOffset = BitConverter.ToUInt32(NameOffset, 0);

                    Array.Copy(FileTable, PositionIndex + 4L, NameSize, 0L, NameSize.Length);
                    Array.Reverse(NameSize);
                    uint ExtractedFileNameSize = BitConverter.ToUInt32(NameSize, 0);

                    ContentType = FileTable[(int)(PositionIndex + 24L)];
                    FileType = FileTable[(int)(PositionIndex + 27L)];

                    Name = new byte[(int)(ExtractedFileNameSize - 1L) + 1];
                    Array.Copy(FileTable, ExtractedFileNameOffset, Name, 0L, ExtractedFileNameSize);
                    string ExtractedFileName = Utils.ByteArrayToAscii(Name, 0, Name.Length, true);

                    if (FileType == 4 && ExtractedFileSize == 0L) // File / Directory
                    {
                        IsFile = false;
                    }
                    else
                    {
                        IsFile = true;
                    }

                    if (ContentType != 144 && IsFile)
                    {
                        if (ExtractedFileName == "PARAM.SFO" | ExtractedFileName == "ICON0.PNG" | ExtractedFileName == "PIC0.PNG" | ExtractedFileName == "PIC1.PNG" | ExtractedFileName == "PIC2.PNG" | ExtractedFileName == "SND0.AT3")
                        {
                            using var FileMemoryStream = new MemoryStream();
                            // Read File
                            DecryptedPKGReadStream.Seek(ExtractedFileOffset, SeekOrigin.Begin);

                            // Pieces calculation
                            double Division = ExtractedFileSize / (double)TwentyMB;
                            ulong Pieces = Convert.ToUInt64(Math.Floor(Division));
                            ulong CustomMod = Convert.ToUInt64(ExtractedFileSize) % Convert.ToUInt64(TwentyMB);
                            if (CustomMod > 0m)
                            {
                                Pieces += 1UL;
                            }

                            DumpFile = new byte[TwentyMB];
                            long Elapsed = 0L;

                            for (ulong i = 0UL, loopTo = (ulong)Math.Round(Pieces - 1m); i <= loopTo; i++)
                            {
                                if (CustomMod > 0m && i == Pieces - 1m)
                                {
                                    DumpFile = new byte[(int)Math.Round(CustomMod - 1m) + 1];
                                }

                                // Fill buffer
                                byte[] DecryptedData = DecryptPKGDataRead(DumpFile.Length, ExtractedFileOffset + Elapsed, UIEncryptedFileStartOffset, PS3AesKey, EncryptedPKGReadStream, EncryptedPKGMemoryStream)!;

                                Elapsed += DumpFile.Length;
                                FileMemoryStream.Write(DecryptedData, 0, DumpFile.Length);
                            }

                            if (FileMemoryStream.ToArray().Length > 0)
                            {
                                switch (ExtractedFileName ?? "")
                                {
                                    case "PARAM.SFO":
                                        {
                                            PARAMSFO = FileMemoryStream.ToArray();
                                            break;
                                        }
                                    case "ICON0.PNG":
                                        {
                                            ICON0 = NewBitmapImage(FileMemoryStream.ToArray());
                                            break;
                                        }
                                    case "PIC0.PNG":
                                        {
                                            PIC0 = NewBitmapImage(FileMemoryStream.ToArray());
                                            break;
                                        }
                                    case "PIC1.PNG":
                                        {
                                            PIC1 = NewBitmapImage(FileMemoryStream.ToArray());
                                            break;
                                        }
                                    case "PIC2.PNG":
                                        {
                                            PIC2 = NewBitmapImage(FileMemoryStream.ToArray());
                                            break;
                                        }
                                    case "SND0.AT3":
                                        {
                                            SND0 = FileMemoryStream.ToArray();
                                            break;
                                        }
                                }
                            }

                            FileMemoryStream.Close();
                        }
                    }

                    PositionIndex += 32L;
                    WhileInt += 1;
                }

                // Close File
                EncryptedPKGReadStream.Close();
                EncryptedPKGMemoryStream.Close();

                DecryptedPKGReadStream.Close();
                DecryptedPKGMemoryStream.Close();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static byte[]? GetBytesFromFile(string FileName)
        {
            byte[]? BytesFromFile;

            try
            {
                byte[]? ByteArray = null;

                using (var FileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    ByteArray = new byte[1048576];
                    FileStream.ReadExactly(ByteArray, 0, ByteArray.Length);
                }

                BytesFromFile = ByteArray;
                return BytesFromFile;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> ExtractFilesAsync(string DecryptedPKGFileName, string EncryptedPKGFileName)
        {
            try
            {
                int TwentyMB = 1024 * 1024 * 20;
                uint ExtractedFileOffset = 0U;
                uint ExtractedFileSize = 0U;
                uint OffsetShift = 0U;
                long PositionIndex = 0L;
                string WorkDir = "";

                WorkDir = DecryptedPKGFileName + ".EXT";

                if (Directory.Exists(WorkDir))
                {
                    Directory.Delete(WorkDir, true);
                    System.Threading.Thread.Sleep(100);
                    Directory.CreateDirectory(WorkDir);
                    System.Threading.Thread.Sleep(100);
                }

                byte[] FileTable = new byte[320000];
                byte[] DumpFile;
                byte[] SDKVer = new byte[8];
                byte[] FirstFileOffset = new byte[4];
                byte[] FirstNameOffset = new byte[4];
                byte[] FileNr = new byte[4];
                byte[] IsDir = new byte[4];
                byte[] Offset = new byte[4];
                byte[] Size = new byte[4];
                byte[] NameOffset = new byte[4];
                byte[] NameSize = new byte[4];
                byte[] Name = new byte[32];
                byte[] BootMagic = new byte[8];
                byte ContentType = 0;
                byte FileType = 0;
                bool IsFile = false;

                FileStream DecryptedPKGReadStream = new(DecryptedPKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var DecryptedPKGBinaryReader = new BinaryReader(DecryptedPKGReadStream);

                FileStream EncryptedPKGReadStream = new(EncryptedPKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var EncryptedPKGBinaryReader = new BinaryReader(EncryptedPKGReadStream);

                // Read the file Table
                DecryptedPKGReadStream.Seek(0L, SeekOrigin.Begin);
                FileTable = DecryptedPKGBinaryReader.ReadBytes(FileTable.Length);

                PositionIndex = 0L;
                OffsetShift = 0U;

                // Shift Relative to os.raw
                Array.Copy(FileTable, 0, FirstNameOffset, 0, FirstNameOffset.Length);
                Array.Reverse(FirstNameOffset);
                uint UIfirstNameOffset = BitConverter.ToUInt32(FirstNameOffset, 0);
                uint UIFileNr = (uint)(UIfirstNameOffset / 32L);

                Array.Copy(FileTable, 12, FirstFileOffset, 0, FirstFileOffset.Length);
                Array.Reverse(FirstFileOffset);
                uint UIFirstFileOffset = BitConverter.ToUInt32(FirstFileOffset, 0);

                // Read the file Table
                DecryptedPKGReadStream.Seek(0L, SeekOrigin.Begin);
                FileTable = DecryptedPKGBinaryReader.ReadBytes((int)UIFirstFileOffset);

                // If number of files is negative then something is wrong...
                if ((int)UIFileNr < 0)
                {
                    return false;
                }

                // Table:
                // 0-3         4-7         8-11        12-15       16-19       20-23       24-27       28-31
                // |name loc | |name size| |   NULL  | |file loc | |  NULL   | |file size| |cont type| |   NULL  |

                for (int ii = 0, loopTo = (int)UIFileNr - 1; ii <= loopTo; ii++)
                {
                    Array.Copy(FileTable, PositionIndex + 12L, Offset, 0L, Offset.Length);
                    Array.Reverse(Offset);
                    ExtractedFileOffset = BitConverter.ToUInt32(Offset, 0) + OffsetShift;

                    Array.Copy(FileTable, PositionIndex + 20L, Size, 0L, Size.Length);
                    Array.Reverse(Size);
                    ExtractedFileSize = BitConverter.ToUInt32(Size, 0);

                    Array.Copy(FileTable, PositionIndex, NameOffset, 0L, NameOffset.Length);
                    Array.Reverse(NameOffset);
                    uint ExtractedFileNameOffset = BitConverter.ToUInt32(NameOffset, 0);

                    Array.Copy(FileTable, PositionIndex + 4L, NameSize, 0L, NameSize.Length);
                    Array.Reverse(NameSize);
                    uint ExtractedFileNameSize = BitConverter.ToUInt32(NameSize, 0);

                    ContentType = FileTable[(int)(PositionIndex + 24L)];
                    FileType = FileTable[(int)(PositionIndex + 27L)];

                    Name = new byte[(int)(ExtractedFileNameSize - 1L) + 1];
                    Array.Copy(FileTable, ExtractedFileNameOffset, Name, 0L, ExtractedFileNameSize);
                    string ExtractedFileName = Utils.ByteArrayToAscii(Name, 0, Name.Length, true);

                    // Write Directory
                    if (!Directory.Exists(WorkDir))
                    {
                        Directory.CreateDirectory(WorkDir);
                        System.Threading.Thread.Sleep(100);
                    }

                    FileStream? ExtractedFileWriteStream = null;

                    // File / Directory
                    if (FileType == 0x4 && ExtractedFileSize == 0x0L)
                    {
                        IsFile = false;
                    }
                    else
                    {
                        IsFile = true;
                    }

                    // contentType == 0x90 = PSP file/dir
                    if (ContentType == 0x90)
                    {
                        string FileDir = Path.Combine(WorkDir, ExtractedFileName);
                        FileDir = FileDir.Replace("/", @"\");
                        var FileDirectory = Directory.GetParent(FileDir);

                        if (!Directory.Exists(FileDirectory!.ToString()))
                        {
                            Directory.CreateDirectory(FileDirectory.ToString());
                        }
                        ExtractedFileWriteStream = new FileStream(FileDir, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
                    }
                    else
                    {
                        // contentType == (0x80 || 0x00) = PS3 file/dir
                        // fileType == 0x01 = NPDRM File
                        // fileType == 0x03 = Raw File
                        // fileType == 0x04 = Directory

                        // Decrypt PS3 Filename
                        byte[] DecryptedData = DecryptData((int)ExtractedFileNameSize, ExtractedFileNameOffset, UIEncryptedFileStartOffset, PS3AesKey, EncryptedPKGReadStream, EncryptedPKGBinaryReader);
                        Array.Copy(DecryptedData, 0, Name, 0, ExtractedFileNameSize);
                        ExtractedFileName = Utils.ByteArrayToAscii(Name, 0, Name.Length, true);

                        if (!IsFile)
                        {
                            // Directory
                            try
                            {
                                if (!Directory.Exists(ExtractedFileName))
                                {
                                    Directory.CreateDirectory(Path.Combine(WorkDir, ExtractedFileName));
                                }
                            }
                            catch (Exception)
                            {
                                ExtractedFileName = ii.ToString() + ".raw";
                                if (!Directory.Exists(ExtractedFileName))
                                {
                                    Directory.CreateDirectory(Path.Combine(WorkDir, ExtractedFileName));
                                }
                            }
                        }
                        else
                        {
                            // File
                            try
                            {
                                ExtractedFileWriteStream = new FileStream(Path.Combine(WorkDir, ExtractedFileName), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
                            }
                            catch (Exception)
                            {
                                ExtractedFileName = ii.ToString() + ".raw";
                                ExtractedFileWriteStream = new FileStream(Path.Combine(WorkDir, ExtractedFileName), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
                            }
                        }
                    }

                    if (ContentType == 144 && IsFile)
                    {
                        // Read/Write File
                        var ExtractedFile = new BinaryWriter(ExtractedFileWriteStream!);
                        DecryptedPKGReadStream.Seek(ExtractedFileOffset, SeekOrigin.Begin);

                        // Pieces calculation
                        double Division = ExtractedFileSize / (double)TwentyMB;
                        ulong Pieces = Convert.ToUInt64(Math.Floor(Division));
                        ulong Modi = Convert.ToUInt64(ExtractedFileSize) % Convert.ToUInt64(TwentyMB);
                        if (Modi > 0m)
                        {
                            Pieces += 1UL;
                        }

                        DumpFile = new byte[TwentyMB];
                        for (ulong i = 0UL, loopTo1 = (ulong)Math.Round(Pieces - 1m); i <= loopTo1; i++)
                        {
                            // If we have a mod and this is the last piece then...
                            if (Modi > 0m && i == Pieces - 1m)
                            {
                                DumpFile = new byte[(int)Math.Round(Modi - 1m) + 1];
                            }

                            // Fill buffer
                            DecryptedPKGBinaryReader.Read(DumpFile, 0, DumpFile.Length);
                            ExtractedFile.Write(DumpFile);
                        }

                        ExtractedFileWriteStream!.Close();
                        ExtractedFile.Close();
                    }

                    if (ContentType != 0x90 && IsFile)
                    {
                        // Read/Write File
                        var ExtractedFile = new BinaryWriter(ExtractedFileWriteStream!);
                        DecryptedPKGReadStream.Seek(ExtractedFileOffset, SeekOrigin.Begin);

                        // Pieces calculation
                        double Division = ExtractedFileSize / (double)TwentyMB;

                        ulong Pieces = Convert.ToUInt64(Math.Floor(Division));
                        ulong Modi = Convert.ToUInt64(ExtractedFileSize) % Convert.ToUInt64(TwentyMB);
                        if (Modi > 0m)
                        {
                            Pieces += 1UL;
                        }

                        DumpFile = new byte[TwentyMB];
                        long Elapsed = 0L;
                        for (ulong i = 0UL, loopTo2 = (ulong)Math.Round(Pieces - 1m); i <= loopTo2; i++)
                        {
                            // If we have a mod and this is the last piece then...
                            if (Modi > 0m && i == Pieces - 1m)
                            {
                                DumpFile = new byte[(int)Math.Round(Modi - 1m) + 1];
                            }

                            // Fill buffer
                            byte[] DecryptedData = DecryptData(DumpFile.Length, ExtractedFileOffset + Elapsed, UIEncryptedFileStartOffset, PS3AesKey, EncryptedPKGReadStream, EncryptedPKGBinaryReader);
                            Elapsed = +DumpFile.Length;

                            // To avoid decryption pad we use dumpFile.Length that's the actual decrypted file size!
                            ExtractedFile.Write(DecryptedData, 0, DumpFile.Length);
                        }

                        ExtractedFileWriteStream!.Close();
                        ExtractedFile.Close();
                    }

                    PositionIndex += 32L;
                }

                // Close File
                EncryptedPKGReadStream.Close();
                EncryptedPKGBinaryReader.Close();

                DecryptedPKGReadStream.Close();
                DecryptedPKGBinaryReader.Close();

                // Delete decrypted file
                if (File.Exists(DecryptedPKGFileName))
                {
                    File.Delete(DecryptedPKGFileName);
                }

                var box = MessageBoxManager.GetMessageBoxStandard("Done", "Pkg extracted successfully." + Environment.NewLine + "Open folder?", ButtonEnum.YesNo);
                var boxresult = await box.ShowWindowAsync();
                if (boxresult == ButtonResult.Yes)
                {
                    Utils.OpenFolder(@".\");
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public byte[] DecryptData(int DataSize, long DataRelativeOffset, long PKGEncryptedFileStartOffset, byte[] AesKey, Stream EncryptedPKGReadStream, BinaryReader EncryptedPKGBinaryReader)
        {
            int InputSize = DataSize % 16;
            if (InputSize > 0)
            {
                InputSize = (DataSize / 16 + 1) * 16;
            }
            else
            {
                InputSize = DataSize;
            }

            byte[] EncryptedData;
            byte[] DecryptedData;
            byte[] PKGFileKeyConsec = new byte[InputSize];
            byte[] PKGXorKeyConsec;
            byte[] IncPKGFileKey = new byte[PKGFileKey.Length];
            Array.Copy(PKGFileKey, IncPKGFileKey, PKGFileKey.Length);

            EncryptedPKGReadStream.Seek(DataRelativeOffset + PKGEncryptedFileStartOffset, SeekOrigin.Begin);
            EncryptedData = EncryptedPKGBinaryReader.ReadBytes(InputSize);

            for (int Position = 0, loopTo = (int)(DataRelativeOffset - 1L); Position <= loopTo; Position += 16)
                Utils.IncrementArray(ref IncPKGFileKey, PKGFileKey.Length - 1);

            for (int Position = 0, loopTo1 = InputSize - 1; Position <= loopTo1; Position += 16)
            {
                Array.Copy(IncPKGFileKey, 0, PKGFileKeyConsec, Position, PKGFileKey.Length);
                Utils.IncrementArray(ref IncPKGFileKey, PKGFileKey.Length - 1);
            }

            PKGXorKeyConsec = AESEngine.Encrypt(PKGFileKeyConsec, AesKey, AesKey, CipherMode.ECB, PaddingMode.None);
            DecryptedData = XOREngine.GetXOR(EncryptedData, 0, PKGXorKeyConsec.Length, PKGXorKeyConsec);

            return DecryptedData;
        }

        public string DecryptPKGFile(string PKGFileName)
        {
            try
            {
                int Multiplicator = 65536;
                byte[] EncryptedData = new byte[(AesKey.Length * Multiplicator)];
                byte[] DecryptedData = new byte[(AesKey.Length * Multiplicator)];

                byte[] PKGXorKey = new byte[AesKey.Length];
                byte[] EncryptedFileStartOffset = new byte[4];
                byte[] EncryptedFileLenght = new byte[4];

                using (FileStream PKGReadStream = new(PKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using var PKGBinaryReader = new BinaryReader(PKGReadStream);

                    PKGReadStream.Seek(0x0L, SeekOrigin.Begin);
                    byte[] PKGMagic = PKGBinaryReader.ReadBytes(4);
                    if (PKGMagic[0x0] != 0x7F || PKGMagic[0x1] != 0x50 || PKGMagic[0x2] != 0x4B || PKGMagic[0x3] != 0x47)
                    {

                        return string.Empty;
                    }

                    // Finalized byte
                    PKGReadStream.Seek(0x4L, SeekOrigin.Begin);
                    byte PKGFinalized = PKGBinaryReader.ReadByte();

                    if (PKGFinalized != 128)
                    {

                        return string.Empty;
                    }

                    // PKG Type PSP/PS3
                    PKGReadStream.Seek(0x7L, SeekOrigin.Begin);
                    byte PKGType = PKGBinaryReader.ReadByte();

                    switch (PKGType)
                    {
                        case 0x1:
                            {
                                // PS3
                                PackagePlatform = PKGPlatform.PS3;
                                AesKey = PS3AesKey;
                                break;
                            }
                        case 0x2:
                            {
                                // PSP
                                PackagePlatform = PKGPlatform.PSP;
                                AesKey = PSPAesKey;
                                break;
                            }

                        default:
                            {

                                return string.Empty;
                            }
                    }

                    PKGReadStream.Seek(0x24L, SeekOrigin.Begin);
                    EncryptedFileStartOffset = PKGBinaryReader.ReadBytes(EncryptedFileStartOffset.Length);
                    Array.Reverse(EncryptedFileStartOffset);
                    UIEncryptedFileStartOffset = BitConverter.ToUInt32(EncryptedFileStartOffset, 0);

                    PKGReadStream.Seek(0x2CL, SeekOrigin.Begin);
                    EncryptedFileLenght = PKGBinaryReader.ReadBytes(EncryptedFileLenght.Length);
                    Array.Reverse(EncryptedFileLenght);
                    uint UIEncryptedFileLenght = BitConverter.ToUInt32(EncryptedFileLenght, 0);

                    PKGReadStream.Seek(0x70L, SeekOrigin.Begin);
                    PKGFileKey = PKGBinaryReader.ReadBytes(16);
                    byte[] incPKGFileKey = new byte[16];
                    Array.Copy(PKGFileKey, incPKGFileKey, PKGFileKey.Length);

                    PKGXorKey = AESEngine.Encrypt(PKGFileKey, AesKey, AesKey, CipherMode.ECB, PaddingMode.None);

                    double Division = UIEncryptedFileLenght / (double)AesKey.Length;
                    ulong Pieces = Convert.ToUInt64(Math.Floor(Division));
                    ulong Modi = (ulong)Math.Round(Convert.ToUInt64(UIEncryptedFileLenght) / (double)Convert.ToUInt64(AesKey.Length));
                    if (Modi > 0m)
                    {
                        Pieces += 1UL;
                    }

                    if (File.Exists(PKGFileName + ".Dec"))
                    {
                        File.Delete(PKGFileName + ".Dec");
                    }

                    var DecryptedFileWriteStream = new FileStream(PKGFileName + ".Dec", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
                    var DecryptedFileBinaryWriter = new BinaryWriter(DecryptedFileWriteStream);

                    PKGReadStream.Seek(UIEncryptedFileStartOffset, SeekOrigin.Begin);

                    double FileDivision = UIEncryptedFileLenght / (double)(AesKey.Length * Multiplicator);
                    ulong FilePieces = Convert.ToUInt64(Math.Floor(FileDivision));
                    ulong FileMod = Convert.ToUInt64(UIEncryptedFileLenght) % Convert.ToUInt64(AesKey.Length * Multiplicator);
                    if (FileMod > 0m)
                    {
                        FilePieces += 1UL;
                    }

                    for (ulong i = 0UL, loopTo = (ulong)Math.Round(FilePieces - 1m); i <= loopTo; i++)
                    {
                        // If we have a mod and this is the last piece then...
                        if (FileMod > 0m && i == FilePieces - 1m)
                        {
                            EncryptedData = new byte[(int)Math.Round(FileMod - 1m) + 1];
                            DecryptedData = new byte[(int)Math.Round(FileMod - 1m) + 1];
                        }

                        // Read 16 bytes of Encrypted data
                        EncryptedData = PKGBinaryReader.ReadBytes(EncryptedData.Length);

                        // In order to retrieve a fast AES Encryption we pre-Increment the array
                        byte[] PKGFileKeyBytes = new byte[EncryptedData.Length];
                        byte[] PKGXorKeyBytes = new byte[EncryptedData.Length];

                        int Position = 0;
                        while (Position < EncryptedData.Length)
                        {
                            Array.Copy(incPKGFileKey, 0, PKGFileKeyBytes, Position, PKGFileKey.Length);

                            Utils.IncrementArray(ref incPKGFileKey, PKGFileKey.Length - 1);
                            Position += AesKey.Length;
                        }

                        PKGXorKeyBytes = AESEngine.Encrypt(PKGFileKeyBytes, AesKey, AesKey, CipherMode.ECB, PaddingMode.None);
                        DecryptedData = XOREngine.GetXOR(EncryptedData, 0, PKGXorKeyBytes.Length, PKGXorKeyBytes);
                        DecryptedFileBinaryWriter.Write(DecryptedData);
                    }

                    DecryptedFileWriteStream.Close();
                    DecryptedFileBinaryWriter.Close();
                }

                return PKGFileName + ".Dec";
            }
            catch (Exception)
            {

                return string.Empty;
            }
        }

        private static AnyBitmap? NewBitmapImage(byte[] imageData)
        {
            try
            {
                var image = AnyBitmap.FromBytes(imageData);
                return image;
            }
            catch
            {
                return null;
            }
        }

        #region PS5

        // From https://github.com/zecoxao/pkgdec5

        public class RSAKeyset
        {
            // d
            public byte[]? PrivateExponent;
            // exponent1 = d mod (p - 1)
            public byte[]? Exponent1;
            // exponent2 = d mod (q - 1)
            public byte[]? Exponent2;
            // e
            public byte[]? PublicExponent;
            // (InverseQ)(q) = 1 mod p
            public byte[]? Coefficient;
            // n = p * q
            public byte[]? Modulus;
            // p
            public byte[]? Prime1;
            // q
            public byte[]? Prime2;

            /// <summary>
            /// Modulus is in PkgPublicKeys[3], fortunately we have the whole thing!
            /// </summary>
            public static RSAKeyset PkgDerivedKey3Keyset = new()
            {
                Prime1 = [0xD8, 0x4F, 0x78, 0x93, 0x8F, 0x31, 0xF4, 0x56, 0xE8, 0x28, 0xCF, 0x28, 0x90, 0x62, 0x4, 0xD9, 0x36, 0x99, 0xF6, 0xA3, 0x19, 0x6E, 0xC7, 0x27, 0x53, 0x6D, 0xFB, 0x68, 0x5E, 0x63, 0xC4, 0xCF, 0xAD, 0x76, 0x7, 0x88, 0x1F, 0x6F, 0x3F, 0xBD, 0x86, 0xBD, 0x3A, 0x5, 0x62, 0xC5, 0x22, 0xFD, 0xA, 0x42, 0x7D, 0x12, 0x2, 0xC3, 0x77, 0xCE, 0xE3, 0x73, 0xC9, 0x51, 0xE7, 0x63, 0x7, 0x29, 0x89, 0x0, 0xF2, 0x91, 0x5E, 0xE5, 0xDD, 0xB1, 0x3F, 0x96, 0x14, 0xBA, 0xC3, 0x5F, 0xD2, 0x2B, 0x34, 0xBD, 0xA8, 0x5B, 0xFF, 0x86, 0xBC, 0xC7, 0x1E, 0x98, 0x8F, 0x64, 0x22, 0xE3, 0xA0, 0x2E, 0xC9, 0xD1, 0x8D, 0x44, 0xE4, 0xC0, 0xD0, 0x54, 0x5D, 0xBA, 0x7E, 0xC6, 0x59, 0x3A, 0xAE, 0xCB, 0xE, 0x1D, 0x1E, 0xB3, 0xDD, 0x7F, 0x61, 0x35, 0x3B, 0xF4, 0x88, 0x11, 0xFB, 0xBB, 0x6F, 0xA5, 0xD, 0xF5, 0x35, 0x7F, 0x38, 0xE8, 0x7, 0xE1, 0xC3, 0xC3, 0xFE, 0xF1, 0x52, 0xCB, 0xC6, 0xB2, 0xC2, 0xB4, 0x67, 0x4F, 0x3D, 0x7D, 0x44, 0x39, 0xC8, 0xEE, 0xA0, 0xEF, 0x17, 0xB4, 0x0, 0xA2, 0x2, 0xD2, 0x3E, 0x93, 0x39, 0x4A, 0xA2, 0xB2, 0xF, 0x57, 0x7A, 0x6, 0x15, 0x28, 0xF1, 0xB8, 0xD5, 0xC8, 0x53, 0xD0, 0x7F, 0x35, 0xA7, 0x53, 0xCB, 0x24, 0x37, 0x3E, 0xE0, 0x5, 0xC5, 0xC9],
                Prime2 = [0xCA, 0x83, 0x67, 0x7F, 0xF3, 0x9E, 0x73, 0x47, 0xD9, 0xF, 0x99, 0x55, 0xC5, 0x5A, 0x56, 0x57, 0xC3, 0x54, 0x3B, 0xA9, 0x66, 0xBA, 0x86, 0x10, 0xE0, 0xB1, 0x2F, 0xC2, 0x96, 0xD5, 0xF1, 0xD1, 0xD8, 0xCF, 0xF2, 0x7D, 0x3, 0xAE, 0xCE, 0xEC, 0xCC, 0x77, 0x6, 0x5F, 0x31, 0x99, 0x9E, 0x3A, 0x84, 0x37, 0xB1, 0x86, 0x24, 0x13, 0x75, 0x75, 0x9E, 0xAA, 0x8C, 0x8D, 0x66, 0xCB, 0x5F, 0x4A, 0xB7, 0xAD, 0x64, 0x18, 0x9C, 0x5C, 0x63, 0x4C, 0x7D, 0xB3, 0x73, 0x70, 0xE2, 0x82, 0x24, 0xE3, 0x2E, 0xCB, 0xCA, 0x9, 0xB0, 0x8E, 0xDF, 0x64, 0xA9, 0x9E, 0x3E, 0x62, 0xD9, 0xB4, 0xA1, 0xA6, 0xC7, 0x5E, 0xAC, 0x51, 0xB1, 0x82, 0xE3, 0xD5, 0x6D, 0xD0, 0x71, 0xE2, 0x38, 0xBD, 0x56, 0x41, 0xD9, 0x9E, 0xCB, 0xE2, 0x91, 0xEB, 0x5F, 0x48, 0xFB, 0xFA, 0x53, 0x43, 0x6, 0xB8, 0x7D, 0x60, 0xE4, 0x40, 0x1D, 0x18, 0x4B, 0xE0, 0x5A, 0x23, 0x69, 0xCF, 0x39, 0xE0, 0x59, 0xFB, 0x47, 0xC3, 0xB5, 0x3, 0xF4, 0xAA, 0xA8, 0x82, 0xF3, 0x7D, 0x37, 0x61, 0xDE, 0xCE, 0x5E, 0xA7, 0xD, 0x87, 0x1E, 0x9, 0xB3, 0x76, 0xAA, 0x54, 0xEF, 0x33, 0xAA, 0xBD, 0xF2, 0x78, 0xED, 0x68, 0xB2, 0xE2, 0x51, 0x66, 0x81, 0x7, 0x7C, 0xEE, 0x51, 0x6F, 0x2E, 0x7C, 0x59, 0x3, 0x35, 0x8E, 0x52, 0x69],
                PrivateExponent = [0x8E, 0x4, 0xF3, 0xC5, 0x2C, 0x71, 0x85, 0x76, 0x5F, 0x85, 0x3C, 0x55, 0xE5, 0x29, 0x9C, 0xD4, 0xA3, 0xCE, 0x14, 0xCB, 0xAA, 0xE4, 0x89, 0x1, 0x3A, 0xDF, 0xB9, 0x66, 0x98, 0x45, 0xDF, 0x9, 0xAC, 0x41, 0x11, 0x50, 0x88, 0xB, 0x71, 0xFD, 0x55, 0x52, 0xFC, 0xBC, 0x46, 0xFB, 0x44, 0x38, 0x1E, 0x26, 0xE2, 0xE6, 0x29, 0x7A, 0x65, 0xEB, 0xA1, 0xCF, 0x1A, 0x48, 0x26, 0x69, 0x1E, 0xE9, 0x6E, 0x7, 0xB3, 0x34, 0x1D, 0xD8, 0x6A, 0xB4, 0x6B, 0x51, 0xA7, 0x85, 0xC8, 0xC0, 0x82, 0xF5, 0x93, 0xFF, 0x4B, 0x42, 0x17, 0xCA, 0x52, 0xA5, 0x8A, 0xD7, 0x33, 0x33, 0xC0, 0xD6, 0x27, 0xFD, 0xA9, 0x92, 0x88, 0x85, 0x22, 0x92, 0x70, 0xC4, 0xA6, 0x49, 0xCD, 0xE9, 0x18, 0x60, 0x26, 0xC8, 0xA5, 0xA, 0x63, 0x6A, 0xCF, 0xC9, 0x1F, 0xCF, 0xB7, 0xCF, 0x4F, 0x8D, 0xB1, 0xC5, 0xE3, 0xAA, 0xC, 0x14, 0x2, 0xA, 0xF1, 0xC9, 0x8, 0xFD, 0x51, 0xCF, 0x2, 0x22, 0x98, 0xA4, 0xE5, 0xCD, 0x20, 0xEE, 0x57, 0x9B, 0xA, 0x61, 0xBB, 0x58, 0xF6, 0x98, 0xD0, 0x5C, 0x41, 0x96, 0x8F, 0x8C, 0x24, 0x4, 0xF2, 0xDA, 0x79, 0x64, 0xE2, 0xC, 0xDB, 0x54, 0x65, 0x9E, 0xDF, 0x6E, 0xA0, 0xFE, 0xFD, 0xC8, 0x23, 0x16, 0xF9, 0x58, 0xFD, 0x66, 0xBC, 0x40, 0xCA, 0x1, 0x81, 0xD7, 0x67, 0x90, 0xF3, 0x28, 0xD2, 0xE, 0xC9, 0x3B, 0xF5, 0xCA, 0xF6, 0xAB, 0xDD, 0xA3, 0xFF, 0x89, 0xFE, 0xA2, 0x47, 0x43, 0x8A, 0xC8, 0x25, 0xAF, 0xD8, 0x82, 0x2E, 0x13, 0x89, 0x70, 0xFE, 0x8E, 0xFB, 0x19, 0xDD, 0xD3, 0x73, 0xA5, 0xCE, 0xCB, 0xBF, 0xCC, 0x2E, 0x4, 0x79, 0x58, 0xFC, 0xD8, 0xE7, 0xAD, 0x3A, 0x5A, 0x6C, 0x33, 0x9D, 0x98, 0xFB, 0x79, 0x47, 0xEA, 0x3, 0x4D, 0x72, 0x4B, 0x90, 0x36, 0x48, 0x7A, 0x8E, 0x0, 0x69, 0x49, 0x1E, 0x1A, 0xD4, 0x97, 0xE1, 0xE8, 0x57, 0x95, 0x74, 0xE2, 0x9E, 0xEF, 0xA6, 0x2A, 0xD2, 0x25, 0x1D, 0x83, 0xDA, 0xD7, 0x3A, 0x4F, 0x1A, 0xAA, 0xAC, 0xF7, 0x1E, 0xDF, 0x35, 0x10, 0x55, 0x7D, 0x8D, 0xB4, 0x71, 0x4F, 0xD0, 0x5D, 0x63, 0xDC, 0x74, 0xEA, 0xE3, 0x62, 0x1D, 0x2B, 0x4, 0x6, 0xC5, 0x12, 0x6F, 0xC7, 0xD6, 0xA1, 0xB, 0x99, 0x56, 0x38, 0x9C, 0x75, 0x56, 0xCB, 0xDA, 0x51, 0xC4, 0x4B, 0x5D, 0xAC, 0x87, 0xBB, 0x97, 0xD6, 0x46, 0x8D, 0xA7, 0x1E, 0x27, 0xD5, 0x83, 0x2E, 0xFA, 0x96, 0x0, 0x48, 0xD0, 0x53, 0xA4, 0x0, 0xC3, 0xAC, 0xFE, 0x2A, 0xBA, 0x68, 0xA3, 0xA1, 0xAF, 0x4F, 0x43, 0x7E, 0xA1, 0xAB, 0xBC, 0x31, 0xCD, 0x79, 0xA5, 0x14, 0x70, 0x7D, 0x61, 0x80, 0xBF, 0xFD, 0x58, 0xDA, 0x7C, 0x2A, 0x44, 0xAB, 0xBF, 0x41],
                Exponent1 = [0x7, 0x78, 0x1F, 0xA, 0xC1, 0x5C, 0x11, 0x3A, 0xDB, 0x3, 0x65, 0xBB, 0xD9, 0xD8, 0x78, 0xA0, 0x63, 0x81, 0x47, 0x81, 0xF4, 0x43, 0xDD, 0xFE, 0x9E, 0xA3, 0xE2, 0x95, 0x85, 0x4, 0xDE, 0xEB, 0xE8, 0xEA, 0x75, 0x72, 0x1E, 0xDB, 0xC1, 0x90, 0xB2, 0xD1, 0x5F, 0xEA, 0x85, 0xB1, 0x96, 0xF6, 0xB3, 0xDE, 0xFD, 0xE0, 0x9C, 0x55, 0xD1, 0x92, 0x44, 0x4A, 0x60, 0x3E, 0x42, 0xC6, 0x29, 0x9E, 0x26, 0x8B, 0xF0, 0xD4, 0x52, 0x39, 0x8F, 0xC1, 0x2A, 0x17, 0xED, 0x99, 0x51, 0x5B, 0xC2, 0xAF, 0x19, 0x40, 0x1F, 0x4B, 0x25, 0xF4, 0xAA, 0x1A, 0x1A, 0x15, 0x5C, 0x86, 0x31, 0xAA, 0x38, 0x82, 0xC5, 0x17, 0x46, 0x50, 0x85, 0xB1, 0x9E, 0xBF, 0xFB, 0x8, 0x90, 0x8E, 0x1A, 0xD0, 0xAA, 0xEE, 0x7A, 0xB, 0x49, 0x5F, 0x1E, 0x9B, 0xE2, 0x68, 0x6B, 0x2C, 0x93, 0x72, 0x43, 0x86, 0x2, 0x61, 0xE9, 0xAC, 0x78, 0xEF, 0x6E, 0xB0, 0x9C, 0x6D, 0x10, 0x4C, 0x79, 0x46, 0x2D, 0xFC, 0xB9, 0x5C, 0xBC, 0xDA, 0x6B, 0xE2, 0xD1, 0x95, 0xBC, 0xC0, 0x5E, 0xE, 0xD7, 0x61, 0xCA, 0x28, 0xBE, 0x8, 0xDA, 0x1E, 0x16, 0x69, 0x11, 0x6, 0x61, 0xBD, 0xD2, 0x47, 0xCB, 0xFF, 0xDF, 0xC5, 0x2D, 0x2B, 0x9B, 0xBE, 0x32, 0x1E, 0xB5, 0xF5, 0xCD, 0x54, 0x58, 0x64, 0x64, 0xBF, 0xF8, 0xE, 0x5A, 0xF9],
                Exponent2 = [0x3C, 0x99, 0x63, 0xB0, 0x43, 0x1B, 0x48, 0xD, 0xD8, 0xE3, 0x35, 0x14, 0x18, 0x71, 0x36, 0xE3, 0x1E, 0x3D, 0x27, 0x79, 0x42, 0x97, 0x50, 0x24, 0xDE, 0xC7, 0xC6, 0xAD, 0xE8, 0xEA, 0xEE, 0x68, 0xC8, 0x3, 0x39, 0xE1, 0xB4, 0xE7, 0x6B, 0x5E, 0x2A, 0xB4, 0xF7, 0x40, 0x27, 0x1C, 0x7B, 0xDF, 0xB0, 0xCE, 0xE5, 0x9D, 0x69, 0x50, 0x35, 0x56, 0xD3, 0xFA, 0xDF, 0x2, 0x35, 0x1F, 0x68, 0x4D, 0x78, 0x77, 0x37, 0x3B, 0xB2, 0x16, 0x67, 0x54, 0x6D, 0x4C, 0xF4, 0x9F, 0x73, 0xF8, 0x53, 0xC7, 0x73, 0xAA, 0x61, 0xB3, 0xD2, 0x94, 0x7E, 0x3E, 0xA6, 0xF, 0x7, 0x46, 0x17, 0x35, 0x59, 0x26, 0xA, 0x4, 0xC7, 0x75, 0xCE, 0xB3, 0x87, 0x2F, 0xC7, 0xA3, 0x97, 0x60, 0x85, 0x70, 0xA, 0xCE, 0xBB, 0xAB, 0x2C, 0x1, 0x89, 0x7E, 0xB0, 0x4D, 0xAB, 0xB1, 0x35, 0x97, 0x19, 0xFC, 0xBC, 0xEF, 0xF0, 0x7D, 0x4A, 0xF7, 0x89, 0x45, 0x2, 0x54, 0x14, 0x86, 0x81, 0x20, 0x24, 0x6C, 0xF0, 0x5, 0x9D, 0x36, 0x28, 0xD1, 0xA4, 0x89, 0x43, 0x9, 0x56, 0x38, 0x40, 0x2E, 0xEA, 0xDD, 0xFC, 0x4B, 0x51, 0x6E, 0xBF, 0xB8, 0x23, 0xB2, 0x34, 0xBD, 0xF6, 0x3A, 0xCE, 0xC2, 0xE6, 0xEF, 0xEC, 0x8F, 0x92, 0xA2, 0x24, 0xBC, 0x33, 0xE3, 0x30, 0x95, 0x1F, 0x88, 0xF0, 0x2D, 0xE8, 0xA9, 0xC4, 0xF9],
                Coefficient = [0x5C, 0x50, 0xEF, 0x23, 0x14, 0xDB, 0xE1, 0xCF, 0x19, 0x66, 0x8A, 0x93, 0x4D, 0xDC, 0xE7, 0x62, 0x34, 0x72, 0xA5, 0x2F, 0xFD, 0xA7, 0x69, 0x0, 0xCE, 0x5, 0x6C, 0x9A, 0x7A, 0x40, 0x5A, 0x55, 0x9D, 0x81, 0x4E, 0x49, 0xFC, 0xF3, 0x72, 0x36, 0x18, 0x62, 0x7A, 0x54, 0x68, 0x36, 0x3D, 0x90, 0x8E, 0xF4, 0xEE, 0x26, 0x33, 0x14, 0x66, 0x36, 0x6A, 0x1E, 0x66, 0x2D, 0x5B, 0x25, 0x52, 0x10, 0x5D, 0x85, 0x21, 0x11, 0xB9, 0x91, 0xDE, 0x79, 0x10, 0xE2, 0x9A, 0x25, 0xAF, 0x3B, 0x14, 0x2C, 0x30, 0xDF, 0x3C, 0x5B, 0x8D, 0xFF, 0xE8, 0x9C, 0x35, 0x96, 0xC6, 0xF5, 0x63, 0x9, 0xE8, 0x41, 0x9E, 0xD9, 0x61, 0x55, 0x94, 0x98, 0x2F, 0xD9, 0x86, 0x5, 0x32, 0x1, 0x23, 0x86, 0x74, 0xDC, 0x12, 0x4A, 0xF9, 0xD5, 0xB4, 0xFD, 0xA5, 0x9E, 0x6D, 0x28, 0xAE, 0x2, 0xDB, 0xEC, 0xE0, 0xCF, 0xB2, 0xC3, 0xAC, 0x6C, 0xBE, 0xEE, 0x64, 0x20, 0x63, 0xB4, 0x8E, 0xA7, 0xF0, 0x69, 0x96, 0xBD, 0xEC, 0x4D, 0xA7, 0xF8, 0x16, 0x14, 0x3C, 0xDA, 0x67, 0x69, 0xFC, 0xB5, 0x84, 0x47, 0x10, 0x71, 0xAC, 0x64, 0x24, 0xBD, 0x94, 0x3E, 0x8A, 0xE3, 0xDF, 0xB4, 0xA9, 0x54, 0x73, 0x1E, 0x4C, 0xD3, 0xB8, 0xF9, 0x8, 0xCC, 0x1D, 0x85, 0x3B, 0xC1, 0xCC, 0xA, 0xCF, 0x47, 0xBB, 0xAD, 0x6B, 0x7B],
                Modulus = [0xAB, 0x1D, 0xBD, 0x43, 0x39, 0x49, 0x33, 0x16, 0xA3, 0x5C, 0x40, 0x4E, 0x2C, 0x22, 0x97, 0xB8, 0x33, 0x68, 0x5C, 0x1A, 0xD3, 0x54, 0xE8, 0xC5, 0xBA, 0x78, 0x88, 0xD1, 0xB0, 0xFA, 0xF2, 0x5A, 0x8F, 0x14, 0xAA, 0x6, 0x52, 0x8F, 0xA4, 0x65, 0x86, 0x6E, 0xD4, 0x23, 0x3, 0xD3, 0x0, 0x91, 0xB, 0xD9, 0xD8, 0x41, 0x1, 0xFE, 0x54, 0xC1, 0x2B, 0xFC, 0x4F, 0x7F, 0x9C, 0x3A, 0x7A, 0xC9, 0x13, 0x33, 0xFD, 0x2C, 0xDC, 0xCB, 0x14, 0x0, 0x76, 0x1A, 0xDE, 0x5C, 0x2E, 0xBC, 0xA0, 0x11, 0x6D, 0x8C, 0x30, 0x4B, 0x8B, 0x47, 0xF3, 0x3C, 0x41, 0x37, 0x72, 0x84, 0x9E, 0x9E, 0x1D, 0x18, 0x3B, 0x4D, 0x7B, 0xBC, 0x99, 0x4C, 0x37, 0xED, 0x78, 0x87, 0xD4, 0x86, 0x94, 0x23, 0x4B, 0x71, 0xAC, 0xCB, 0x4D, 0xB9, 0x50, 0x70, 0x33, 0x66, 0x18, 0x97, 0x6E, 0xD6, 0x7B, 0x1C, 0x40, 0x1A, 0x21, 0x13, 0xD4, 0x39, 0x88, 0x3, 0x40, 0x49, 0x9F, 0x65, 0x6B, 0x7A, 0xEE, 0xB3, 0x86, 0xC0, 0x67, 0x98, 0xC2, 0xD1, 0x44, 0xEB, 0xB5, 0x84, 0xB5, 0x65, 0x7B, 0x28, 0xE2, 0x90, 0x94, 0x49, 0x31, 0x79, 0x9B, 0xB, 0x9, 0xB2, 0x71, 0xA1, 0xD9, 0x37, 0xB, 0xFE, 0x4F, 0x84, 0xBA, 0xCC, 0x78, 0xEA, 0x3C, 0x91, 0x7D, 0x30, 0xD, 0x53, 0xD5, 0xC5, 0x6A, 0x34, 0xB, 0x2B, 0x7, 0x56, 0x8, 0xF, 0x28, 0x32, 0x53, 0x63, 0xEB, 0x9B, 0xC8, 0x4E, 0xB9, 0x1D, 0x70, 0x46, 0x8E, 0xEF, 0x8B, 0xD4, 0xAB, 0x30, 0x2F, 0x13, 0xF3, 0x0, 0x41, 0x70, 0x95, 0x79, 0xCA, 0xA5, 0x4E, 0x8B, 0xD7, 0x64, 0x23, 0x56, 0xEC, 0x85, 0x23, 0xA, 0x15, 0x14, 0xE0, 0x6, 0x67, 0x56, 0x84, 0x23, 0x8, 0x1D, 0x64, 0x39, 0x96, 0x88, 0x33, 0xA5, 0x1C, 0x5B, 0x2F, 0xC7, 0xB6, 0xEF, 0x0, 0x62, 0x3F, 0xB7, 0x25, 0x89, 0x9A, 0x29, 0x67, 0xCB, 0xC1, 0x4C, 0xEE, 0xAE, 0xFE, 0x87, 0x47, 0x28, 0x2, 0x95, 0xA3, 0x1C, 0x90, 0x89, 0x59, 0xB3, 0x7E, 0xCE, 0xB0, 0x6, 0x41, 0x82, 0xC5, 0x33, 0x66, 0x4D, 0xED, 0x63, 0x55, 0xFF, 0x31, 0x3C, 0xF8, 0x2A, 0x89, 0x1A, 0x42, 0xDC, 0x88, 0x65, 0x5F, 0xDD, 0xFE, 0x71, 0xE6, 0x50, 0xE5, 0x1B, 0x14, 0x90, 0xA8, 0x88, 0xCE, 0x38, 0xD6, 0xFB, 0x85, 0xE, 0x20, 0xD1, 0x24, 0x8, 0xCD, 0xB0, 0xF0, 0xEF, 0xAB, 0x2F, 0xF1, 0x9F, 0x9A, 0x95, 0x80, 0x2D, 0x43, 0x75, 0x60, 0xC0, 0xC9, 0x86, 0xC5, 0xF2, 0xCB, 0xB2, 0xE, 0x2B, 0x89, 0x7F, 0x6B, 0xCB, 0x67, 0xA5, 0x65, 0x7B, 0x47, 0x24, 0xDB, 0xDA, 0x2C, 0xB3, 0x8F, 0xE2, 0x3D, 0x73, 0x8C, 0xF2, 0x6F, 0x8C, 0xC0, 0x6E, 0xF, 0x12, 0x21, 0xFE, 0x74, 0xD, 0xE, 0x36, 0x81, 0x71],
                PublicExponent = [0, 1, 0, 1]
            };
        }

        public static byte[] RSA2048Decrypt(byte[] ciphertext, RSAKeyset keyset)
        {
            var NewRSACryptoServiceProvider = new RSACryptoServiceProvider();
            NewRSACryptoServiceProvider.ImportParameters(new RSAParameters()
            {
                P = keyset.Prime1,
                Q = keyset.Prime2,
                Exponent = keyset.PublicExponent,
                Modulus = keyset.Modulus,
                DP = keyset.Exponent1,
                DQ = keyset.Exponent2,
                InverseQ = keyset.Coefficient,
                D = keyset.PrivateExponent
            });
            return NewRSACryptoServiceProvider.Decrypt(ciphertext, false);
        }

        public struct PackageEntry
        {
            public uint @type;
            public uint unk1;
            public uint flags1;
            public uint flags2;
            public uint offset;
            public uint size;
            public byte[] padding;

            public uint key_index;
            public bool is_encrypted;

            public byte[] ToArray()
            {
                var ms = new MemoryStream();
                var writer = new EndianWriter(ms, EndianType.BigEndian);

                writer.Write(@type);
                writer.Write(unk1);
                writer.Write(flags1);
                writer.Write(flags2);
                writer.Write(offset);
                writer.Write(size);
                writer.Write(padding);

                writer.Close();

                return ms.ToArray();
            }
        }

        public static int AesCbcCfb128Decrypt(byte[] output, byte[] input, uint size, byte[] key, byte[] iv)
        {
            using var cipher = Aes.Create();

            cipher.Mode = CipherMode.CBC;
            cipher.KeySize = 128;
            cipher.Key = key;
            cipher.IV = iv;
            cipher.Padding = PaddingMode.None;
            cipher.BlockSize = 128;

            byte[] TempByte = new byte[(int)(size - 1L) + 1];
            using (var ct_stream = new MemoryStream(input))
            {
                using var pt_stream = new MemoryStream(TempByte);
                using var dec = cipher.CreateDecryptor(key, iv);
                using var s = new CryptoStream(ct_stream, dec, CryptoStreamMode.Read);
                s.CopyTo(pt_stream);
            }

            Buffer.BlockCopy(TempByte, 0, output, 0, TempByte.Length);

            return 0;
        }

        #endregion

    }
}