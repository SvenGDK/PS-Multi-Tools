using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using IronSoftware.Drawing;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace PSMultiTools.PS3.Tools;

public partial class PS3PKGExtractor : Window
{

    public string SelectedPKG = "";
    public string DecryptedPKG = "";

    public BackgroundWorker ImageWorker = new() { WorkerReportsProgress = true };
    public BackgroundWorker DecryptWorker = new() { WorkerReportsProgress = true };
    public BackgroundWorker ExtractionWorker = new() { WorkerReportsProgress = true };

    public byte[] PSPAesKey = [7, 242, 198, 130, 144, 181, 13, 44, 51, 129, 141, 112, 155, 96, 230, 43];
    public byte[] PS3AesKey = [46, 123, 113, 215, 201, 201, 161, 78, 163, 34, 31, 24, 136, 40, 184, 248];
    public byte[] AESKey = new byte[16];
    public byte[] PKGFileKey = new byte[16];
    public uint UIEncryptedFileStartOffset = 0U;

    public PS3PKGExtractor()
    {
        InitializeComponent();

        Loaded += PS3PKGExtractor_Loaded;

        ImageWorker.DoWork += ImageWorker_DoWork;
        ImageWorker.RunWorkerCompleted += ImageWorker_RunWorkerCompleted;
        DecryptWorker.DoWork += DecryptWorker_DoWork;
        DecryptWorker.RunWorkerCompleted += DecryptWorker_RunWorkerCompleted;
        DecryptWorker.ProgressChanged += DecryptWorker_ProgressChanged;
        ExtractionWorker.DoWork += ExtractionWorker_DoWork;
        ExtractionWorker.ProgressChanged += ExtractionWorker_ProgressChanged;
        ExtractionWorker.RunWorkerCompleted += ExtractionWorker_RunWorkerCompleted;
    }

    private void PS3PKGExtractor_Loaded(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedPKG))
        {
            ExtractProgressTextBlock.Text = "Getting ICON0 from PKG ...";
            ImageWorker.RunWorkerAsync();
        }
    }

    private void LockUI()
    {
        if (SelectedPKGFileTextBox.IsEnabled)
        {
            SelectedPKGFileTextBox.IsEnabled = false;
            BrowsePKGButton.IsEnabled = false;
            ExtractButton.IsEnabled = false;
        }
        else
        {
            SelectedPKGFileTextBox.IsEnabled = true;
            BrowsePKGButton.IsEnabled = true;
            ExtractButton.IsEnabled = true;
        }
    }

    private async void BrowsePKGButton_Click(object? sender, RoutedEventArgs e)
    {
        var pkgFileFilter = new FileDialogFilter
        {
            Name = "PKG File",
            Extensions = ["pkg"]
        };
        var OFD = new OpenFileDialog() { Title = "Select a PKG file", Filters = { pkgFileFilter }, AllowMultiple = false };

        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedPKGFileTextBox.Text = OFDResult[0];
        }
    }

    private void ExtractButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedPKGFileTextBox.Text) & File.Exists(SelectedPKGFileTextBox.Text))
        {
            SelectedPKG = SelectedPKGFileTextBox.Text!;
            ExtractProgressTextBlock.Text = "Getting ICON0 from PKG ...";
            LockUI();
            ImageWorker.RunWorkerAsync();
        }
    }

    private void ImageWorker_DoWork(object? sender, DoWorkEventArgs e)
    {
        var NewPKGDecryptor = new PKGDecryptor();

        NewPKGDecryptor.ProcessPKGFile(SelectedPKG);

        if (NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.ICON0) is not null)
        {
            e.Result = NewPKGDecryptor.GetImage(PKGDecryptor.PKGFiles.ICON0);
        }
        else
        {
            e.Result = null;
        }
    }

    private void ImageWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Result == null)
        {
            AnyBitmap ICONFromBGWorker = (AnyBitmap)e.Result!;
            using MemoryStream memory = new();
            ICONFromBGWorker.ExportStream(memory);
            memory.Position = 0;
            Avalonia.Media.Imaging.Bitmap avaloniaBitmap = new(memory);
            PKGICONImage.Source = avaloniaBitmap;
        }

        ImageWorker.Dispose();

        ExtractProgressTextBlock.Text = "Decrypting PKG ...";
        DecryptWorker.RunWorkerAsync(new Structures.ExtractionPKGProcess() { PKGFileName = SelectedPKG });
    }

    private async void DecryptWorker_DoWork(object? sender, DoWorkEventArgs e)
    {
        Structures.ExtractionPKGProcess Args = (Structures.ExtractionPKGProcess)e.Argument!;

        try
        {
            int Multiplicator = 65536;
            byte[] EncryptedData = new byte[(AESKey.Length * Multiplicator)];
            byte[] DecryptedData = new byte[(AESKey.Length * Multiplicator)];

            byte[] PKGXorKey = new byte[AESKey.Length];
            byte[] EncryptedFileStartOffset = new byte[4];
            byte[] EncryptedFileLenght = new byte[4];

            using (FileStream PKGReadStream = new(Args.PKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using var brPKG = new BinaryReader(PKGReadStream);

                PKGReadStream.Seek(0x0L, SeekOrigin.Begin);

                byte[] pkgMagic = brPKG.ReadBytes(4);
                if (pkgMagic[0x0] != 0x7F || pkgMagic[0x1] != 0x50 || pkgMagic[0x2] != 0x4B || pkgMagic[0x3] != 0x47)
                {
                    e.Cancel = true;
                }

                PKGReadStream.Seek(0x4L, SeekOrigin.Begin);

                byte pkgFinalized = brPKG.ReadByte();
                if (pkgFinalized != 128)
                {
                    e.Cancel = true;
                }

                PKGReadStream.Seek(0x7L, SeekOrigin.Begin);

                byte pkgType = brPKG.ReadByte();
                switch (pkgType)
                {
                    case 0x1:
                        {
                            // PS3
                            AESKey = PS3AesKey;
                            break;
                        }
                    case 0x2:
                        {
                            // PSP
                            AESKey = PSPAesKey;
                            break;
                        }

                    default:
                        {
                            e.Cancel = true;
                            break;
                        }
                }

                PKGReadStream.Seek(0x24L, SeekOrigin.Begin);
                EncryptedFileStartOffset = brPKG.ReadBytes(EncryptedFileStartOffset.Length);
                Array.Reverse(EncryptedFileStartOffset);
                UIEncryptedFileStartOffset = BitConverter.ToUInt32(EncryptedFileStartOffset, 0);

                PKGReadStream.Seek(0x2CL, SeekOrigin.Begin);
                EncryptedFileLenght = brPKG.ReadBytes(EncryptedFileLenght.Length);
                Array.Reverse(EncryptedFileLenght);
                uint uiEncryptedFileLenght = BitConverter.ToUInt32(EncryptedFileLenght, 0);

                PKGReadStream.Seek(0x70L, SeekOrigin.Begin);
                PKGFileKey = brPKG.ReadBytes(16);
                byte[] incPKGFileKey = new byte[16];
                Array.Copy(PKGFileKey, incPKGFileKey, PKGFileKey.Length);

                PKGXorKey = AESEngine.Encrypt(PKGFileKey, AESKey, AESKey, CipherMode.ECB, PaddingMode.None);

                double division = uiEncryptedFileLenght / (double)AESKey.Length;
                ulong pieces = Convert.ToUInt64(Math.Floor(division));
                ulong Modi = (ulong)Math.Round(Convert.ToUInt64(uiEncryptedFileLenght) / (double)Convert.ToUInt64(AESKey.Length));
                if (Modi > 0m)
                {
                    pieces += 1UL;
                }

                if (File.Exists(Args.PKGFileName + ".DEC"))
                {
                    File.Delete(Args.PKGFileName + ".DEC");
                }

                var DecryptedFileWriteStream = new FileStream(Args.PKGFileName + ".DEC", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
                var bwDecryptedFile = new BinaryWriter(DecryptedFileWriteStream);

                PKGReadStream.Seek(UIEncryptedFileStartOffset, SeekOrigin.Begin);

                double filedivision = uiEncryptedFileLenght / (double)(AESKey.Length * Multiplicator);
                ulong filepieces = Convert.ToUInt64(Math.Floor(filedivision));
                ulong filemod = Convert.ToUInt64(uiEncryptedFileLenght) % Convert.ToUInt64(AESKey.Length * Multiplicator);
                if (filemod > 0m)
                {
                    filepieces += 1UL;
                }

                DecryptWorker.ReportProgress(0, new Structures.ExtractionWorkerProgress() { FileCount = (int)filepieces - 1, FileName = "" }); // Report FileCount

                for (ulong i = 0UL, loopTo = (ulong)Math.Round(filepieces - 1m); i <= loopTo; i++)
                {
                    // If we have a mod and this is the last piece then...
                    if (filemod > 0m && i == filepieces - 1m)
                    {
                        EncryptedData = new byte[(int)Math.Round(filemod - 1m) + 1];
                        DecryptedData = new byte[(int)Math.Round(filemod - 1m) + 1];
                    }

                    // Read 16 bytes of Encrypted data
                    EncryptedData = brPKG.ReadBytes(EncryptedData.Length);

                    // In order to retrieve a fast AES Encryption we pre-Increment the array
                    byte[] PKGFileKeyConsec = new byte[EncryptedData.Length];
                    byte[] PKGXorKeyConsec = new byte[EncryptedData.Length];

                    int pos = 0;
                    while (pos < EncryptedData.Length)
                    {
                        Array.Copy(incPKGFileKey, 0, PKGFileKeyConsec, pos, PKGFileKey.Length);

                        Utils.IncrementArray(ref incPKGFileKey, PKGFileKey.Length - 1);
                        pos += AESKey.Length;
                    }

                    PKGXorKeyConsec = AESEngine.Encrypt(PKGFileKeyConsec, AESKey, AESKey, CipherMode.ECB, PaddingMode.None);
                    DecryptedData = XOREngine.GetXOR(EncryptedData, 0, PKGXorKeyConsec.Length, PKGXorKeyConsec);
                    DecryptWorker.ReportProgress(1);
                    bwDecryptedFile.Write(DecryptedData);
                }

                DecryptedFileWriteStream.Close();
                bwDecryptedFile.Close();
            }

            e.Result = Args.PKGFileName + ".DEC";
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            });
        }
    }

    private void DecryptWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        DecryptedPKG = e.Result!.ToString()!;
        ExtractProgressBar.Value = 0d;
        DecryptWorker.Dispose();

        ExtractProgressTextBlock.Text = "Extracting PKG ...";
        if (DecryptedPKG is not null && !string.IsNullOrEmpty(DecryptedPKG))
        {
            ExtractionWorker.RunWorkerAsync(new Structures.ExtractionPKGProcess() { DecryptedPKGFileName = DecryptedPKG, EncryptedPKGFileName = SelectedPKG });
        }
    }

    private void DecryptWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        if (e.ProgressPercentage == 0)
        {
            Structures.ExtractionWorkerProgress Progr = (Structures.ExtractionWorkerProgress)e.UserState!;
            ExtractProgressBar.Maximum = (double)Progr.FileCount;
        }
        ExtractProgressBar.Value += (double)e.ProgressPercentage;
    }

    private async void ExtractionWorker_DoWork(object? sender, DoWorkEventArgs e)
    {
        Structures.ExtractionPKGProcess Args = (Structures.ExtractionPKGProcess)e.Argument!;

        try
        {
            int twentyMb = 1024 * 1024 * 20;
            uint ExtractedFileOffset = 0U;
            uint ExtractedFileSize = 0U;
            uint OffsetShift = 0U;
            long positionIdx = 0L;
            string WorkDir = "";

            WorkDir = Args.DecryptedPKGFileName + ".EXT";

            if (Directory.Exists(WorkDir))
            {
                Directory.Delete(WorkDir, true);
                Thread.Sleep(100);
                Directory.CreateDirectory(WorkDir);
                Thread.Sleep(100);
            }

            byte[] FileTable = new byte[320000];
            byte[] dumpFile;
            byte[] sdkVer = new byte[8];
            byte[] firstFileOffset = new byte[4];
            byte[] firstNameOffset = new byte[4];
            byte[] fileNr = new byte[4];
            byte[] isDir = new byte[4];
            byte[] Offset = new byte[4];
            byte[] Size = new byte[4];
            byte[] NameOffset = new byte[4];
            byte[] NameSize = new byte[4];
            byte[] Name = new byte[32];
            byte[] bootMagic = new byte[8];
            byte contentType = 0;
            byte fileType = 0;
            bool isFile = false;

            FileStream decrPKGReadStream = new(Args.DecryptedPKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var brDecrPKG = new BinaryReader(decrPKGReadStream);

            FileStream encrPKGReadStream = new(Args.EncryptedPKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var brEncrPKG = new BinaryReader(encrPKGReadStream);

            // Read the file Table
            decrPKGReadStream.Seek(0L, SeekOrigin.Begin);
            FileTable = brDecrPKG.ReadBytes(FileTable.Length);

            positionIdx = 0L;
            OffsetShift = 0U;

            // Shift Relative to os.raw
            Array.Copy(FileTable, 0, firstNameOffset, 0, firstNameOffset.Length);
            Array.Reverse(firstNameOffset);
            uint uifirstNameOffset = BitConverter.ToUInt32(firstNameOffset, 0);

            uint uiFileNr = (uint)(uifirstNameOffset / 32L);

            Array.Copy(FileTable, 12, firstFileOffset, 0, firstFileOffset.Length);
            Array.Reverse(firstFileOffset);
            uint uifirstFileOffset = BitConverter.ToUInt32(firstFileOffset, 0);

            // Read the file Table
            decrPKGReadStream.Seek(0L, SeekOrigin.Begin);
            FileTable = brDecrPKG.ReadBytes((int)uifirstFileOffset);

            // If number of files is negative then something is wrong...
            if ((int)uiFileNr < 0)
            {
                e.Cancel = true;
            }

            ExtractionWorker.ReportProgress(0, new Structures.ExtractionWorkerProgress() { FileCount = (int)uiFileNr - 1, FileName = "" }); // Report FileCount

            // Table:
            // 0-3         4-7         8-11        12-15       16-19       20-23       24-27       28-31
            // |name loc | |name size| |   NULL  | |file loc | |  NULL   | |file size| |cont type| |   NULL  |

            for (int ii = 0, loopTo = (int)uiFileNr - 1; ii <= loopTo; ii++)
            {
                Array.Copy(FileTable, positionIdx + 12L, Offset, 0L, Offset.Length);
                Array.Reverse(Offset);
                ExtractedFileOffset = BitConverter.ToUInt32(Offset, 0) + OffsetShift;

                Array.Copy(FileTable, positionIdx + 20L, Size, 0L, Size.Length);
                Array.Reverse(Size);
                ExtractedFileSize = BitConverter.ToUInt32(Size, 0);

                Array.Copy(FileTable, positionIdx, NameOffset, 0L, NameOffset.Length);
                Array.Reverse(NameOffset);
                uint ExtractedFileNameOffset = BitConverter.ToUInt32(NameOffset, 0);

                Array.Copy(FileTable, positionIdx + 4L, NameSize, 0L, NameSize.Length);
                Array.Reverse(NameSize);
                uint ExtractedFileNameSize = BitConverter.ToUInt32(NameSize, 0);

                contentType = FileTable[(int)(positionIdx + 24L)];
                fileType = FileTable[(int)(positionIdx + 27L)];

                Name = new byte[(int)(ExtractedFileNameSize - 1L) + 1];
                Array.Copy(FileTable, ExtractedFileNameOffset, Name, 0L, ExtractedFileNameSize);
                string ExtractedFileName = Utils.ByteArrayToAscii(Name, 0, Name.Length, true);

                // Write Directory
                if (!Directory.Exists(WorkDir))
                {
                    Directory.CreateDirectory(WorkDir);
                    Thread.Sleep(100);
                }

                FileStream ExtractedFileWriteStream = null!;

                // File / Directory
                if (fileType == 0x4 && ExtractedFileSize == 0x0L)
                {
                    isFile = false;
                }
                else
                {
                    isFile = true;
                }

                // contentType == 0x90 = PSP file/dir
                if (contentType == 0x90)
                {
                    string FileDir = WorkDir + @"\" + ExtractedFileName;
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
                    byte[] DecryptedData = DecryptData((int)ExtractedFileNameSize, ExtractedFileNameOffset, UIEncryptedFileStartOffset, PS3AesKey, encrPKGReadStream, brEncrPKG);
                    Array.Copy(DecryptedData, 0L, Name, 0L, ExtractedFileNameSize);
                    ExtractedFileName = Utils.ByteArrayToAscii(Name, 0, Name.Length, true);

                    if (!isFile)
                    {
                        // Directory
                        try
                        {
                            if (!Directory.Exists(ExtractedFileName))
                            {
                                Directory.CreateDirectory(WorkDir + @"\" + ExtractedFileName);
                            }
                        }
                        catch (Exception)
                        {
                            // This should not happen
                            ExtractedFileName = ii.ToString() + ".raw";
                            if (!Directory.Exists(ExtractedFileName))
                            {
                                Directory.CreateDirectory(WorkDir + @"\" + ExtractedFileName);
                            }
                        }
                    }
                    else
                    {
                        // File
                        try
                        {
                            ExtractedFileWriteStream = new FileStream(WorkDir + @"\" + ExtractedFileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
                        }
                        catch (Exception)
                        {
                            // This should not happen
                            ExtractedFileName = ii.ToString() + ".raw";
                            ExtractedFileWriteStream = new FileStream(WorkDir + @"\" + ExtractedFileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
                        }
                    }
                }

                if (contentType == 144 && isFile)
                {
                    // Read/Write File
                    var ExtractedFile = new BinaryWriter(ExtractedFileWriteStream!);
                    decrPKGReadStream.Seek(ExtractedFileOffset, SeekOrigin.Begin);

                    // Pieces calculation
                    double division = ExtractedFileSize / (double)twentyMb;
                    ulong pieces = Convert.ToUInt64(Math.Floor(division));
                    ulong Modi = Convert.ToUInt64(ExtractedFileSize) % Convert.ToUInt64(twentyMb);
                    if (Modi > 0m)
                    {
                        pieces += 1UL;
                    }

                    dumpFile = new byte[twentyMb];
                    for (ulong i = 0UL, loopTo1 = (ulong)Math.Round(pieces - 1m); i <= loopTo1; i++)
                    {
                        // If we have a mod and this is the last piece then...
                        if (Modi > 0m && i == pieces - 1m)
                        {
                            dumpFile = new byte[(int)Math.Round(Modi - 1m) + 1];
                        }

                        // Fill buffer
                        brDecrPKG.Read(dumpFile, 0, dumpFile.Length);
                        ExtractedFile.Write(dumpFile);
                    }

                    ExtractedFileWriteStream!.Close();
                    ExtractedFile.Close();
                }

                if (contentType != 0x90 && isFile)
                {
                    // Read/Write File
                    var ExtractedFile = new BinaryWriter(ExtractedFileWriteStream!);
                    decrPKGReadStream.Seek(ExtractedFileOffset, SeekOrigin.Begin);

                    // Pieces calculation
                    double division = ExtractedFileSize / (double)twentyMb;

                    ulong pieces = Convert.ToUInt64(Math.Floor(division));
                    ulong Modi = Convert.ToUInt64(ExtractedFileSize) % Convert.ToUInt64(twentyMb);
                    if (Modi > 0m)
                    {
                        pieces += 1UL;
                    }

                    dumpFile = new byte[twentyMb];
                    long elapsed = 0L;
                    for (ulong i = 0UL, loopTo2 = (ulong)Math.Round(pieces - 1m); i <= loopTo2; i++)
                    {
                        // If we have a mod and this is the last piece then
                        if (Modi > 0m && i == pieces - 1m)
                        {
                            dumpFile = new byte[(int)Math.Round(Modi - 1m) + 1];
                        }

                        // Fill buffer
                        byte[] DecryptedData = DecryptData(dumpFile.Length, ExtractedFileOffset + elapsed, UIEncryptedFileStartOffset, PS3AesKey, encrPKGReadStream, brEncrPKG);
                        elapsed = +dumpFile.Length;

                        // To avoid decryption pad we use dumpFile.Length that's the actual decrypted file size!
                        ExtractedFile.Write(DecryptedData, 0, dumpFile.Length);
                    }

                    ExtractedFileWriteStream!.Close();
                    ExtractedFile.Close();
                }

                positionIdx += 32L;
                ExtractionWorker.ReportProgress(1);
            }

            // Close Filestreams
            encrPKGReadStream.Close();
            brEncrPKG.Close();

            decrPKGReadStream.Close();
            brDecrPKG.Close();

            // Delete decrypted file
            if (File.Exists(Args.DecryptedPKGFileName))
            {
                File.Delete(Args.DecryptedPKGFileName);
            }
        }

        catch (Exception ex)
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            });
        }
    }

    private void ExtractionWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        if (e.ProgressPercentage == 0)
        {
            Structures.ExtractionWorkerProgress Progr = (Structures.ExtractionWorkerProgress)e.UserState!;
            ExtractProgressBar.Maximum = Progr.FileCount;
        }
        ExtractProgressBar.Value += e.ProgressPercentage;
    }

    private async void ExtractionWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        ExtractionWorker.Dispose();
        ExtractProgressTextBlock.Text = "PKG extracted!";

        var FiInfo = new FileInfo(SelectedPKG);
        string OutputDir = FiInfo.DirectoryName + @"\" + FiInfo.Name + ".DEC.EXT";

        LockUI();

        var box = MessageBoxManager.GetMessageBoxStandard("Done", "PKG extracted!" + Environment.NewLine + "Open folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
        var boxresult = await box.ShowWindowDialogAsync(this);
        if (boxresult == ButtonResult.Yes)
        {
            Utils.OpenFolder(OutputDir);
            Close();
        }
        else
        {
            Close();
        }
    }

    public byte[] DecryptData(int dataSize, long dataRelativeOffset, long pkgEncryptedFileStartOffset, byte[] AesKey, Stream encrPKGReadStream, BinaryReader brEncrPKG)
    {
        int size = dataSize % 16;
        if (size > 0)
        {
            size = (dataSize / 16 + 1) * 16;
        }
        else
        {
            size = dataSize;
        }

        byte[] PKGFileKeyConsec = new byte[size];
        byte[] incPKGFileKey = new byte[PKGFileKey.Length];
        Array.Copy(PKGFileKey, incPKGFileKey, PKGFileKey.Length);

        encrPKGReadStream.Seek(dataRelativeOffset + pkgEncryptedFileStartOffset, SeekOrigin.Begin);

        for (int pos = 0, loopTo = (int)(dataRelativeOffset - 1L); pos <= loopTo; pos += 16)
            Utils.IncrementArray(ref incPKGFileKey, PKGFileKey.Length - 1);

        for (int pos = 0, loopTo1 = size - 1; pos <= loopTo1; pos += 16)
        {
            Array.Copy(incPKGFileKey, 0, PKGFileKeyConsec, pos, PKGFileKey.Length);
            Utils.IncrementArray(ref incPKGFileKey, PKGFileKey.Length - 1);
        }

        byte[] EncryptedData = brEncrPKG.ReadBytes(size);
        byte[] PKGXorKeyConsec = AESEngine.Encrypt(PKGFileKeyConsec, AesKey, AesKey, CipherMode.ECB, PaddingMode.None);
        byte[] DecryptedData = XOREngine.GetXOR(EncryptedData, 0, PKGXorKeyConsec.Length, PKGXorKeyConsec);

        return DecryptedData;
    }

}