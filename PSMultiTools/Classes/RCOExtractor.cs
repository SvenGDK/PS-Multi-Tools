using System;
using System.IO;
using System.IO.Compression;

namespace PSMultiTools.Classes
{
    public class RCOExtractor
    {

        private static byte[] gimMagic = [0x4D, 0x49, 0x47, 0x2E, 0x30, 0x30, 0x2E, 0x31, 0x50, 0x53, 0x50, 0x00, 0x00, 0x00, 0x00, 0x00,];
        private static byte[] pngMagic = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,];
        private static byte[] ngrcoMagic = [0x52, 0x43, 0x4F, 0x46, 0x10, 0x01, 0x00, 0x00,];
        private static byte[] ngCXML = [0x52, 0x43, 0x53, 0x46, 0x10, 0x01, 0x00, 0x00,];
        private static byte[] vagMagic = [0x56, 0x41, 0x47, 0x70, 0x00, 0x02, 0x00, 0x01,];
        private static byte[] ddsMagic = "DDS "u8.ToArray();
        private static byte[] wavMagic = "RIFF"u8.ToArray();
        private static byte[] gtfMagic = [0x02, 0x02, 0x00, 0xFF,];
        private static byte[] zlibMagic = [0x00, 0x78, 0xDA,];
        private static byte[] singlZL = [0x78, 0xDA,];
        private static byte[] _vag = [];
        private static byte[] _png = [];
        private static byte[] _cxml = [];
        private static byte[] _zlib = [];
        private static byte[] _wav = [];
        private static byte[] _gtf = [];
        private static byte[] _dds = [];
        private static byte[] zlib = new byte[2];
        private static byte[] vag = new byte[8];
        private static byte[] cxml = new byte[8];
        private static byte[] png = new byte[16];
        private static byte[] temp = new byte[1];
        private static byte[] dds = new byte[4];
        private static byte[] gtf = new byte[4];
        private static byte[] wav = new byte[4];
        private static int i = 0;
        private static int dumped = 0;
        private static int end = 0;
        private static int count = 0;
        private static int countCXML = 0;
        private static int countGim = 0;
        private static int countDDS = 0;
        private static int countPNG = 0;
        private static int countGTF = 0;
        private static int countWAV = 0;
        private static int countZLIB = 0;

        private static string BaseDirectory = "";
        private static string ConvertedDirectory = "";
        private static string CorrectExtension = "";
        private static string move = "";
        private static string dest = "";

        public static bool CompareBytes(byte[] byteArray1, byte[] byteArray2)
        {
            int s = 0;
            for (int z = 0; z < byteArray1.Length; z++)
            {
                if (byteArray1[z] != byteArray2[z])
                    s++;
            }

            if (s == 0)
                return true;

            return false;
        }

        public static void ZLibDeCompress(string FileToDeCompress)
        {
            try
            {
                using (Stream input = File.OpenRead(FileToDeCompress))
                {
                    using (MemoryStream mem = new())
                    {
                        using (Stream output = new ZLibStream(input, CompressionMode.Decompress))
                        {
                            using (var fileRenamed = File.Create(FileToDeCompress + ".decompressed"))
                            {
                                int workingBufferSize = 4096;

                                byte[] buffer = new byte[workingBufferSize];
                                int len;
                                while ((len = output.Read(buffer, 0, buffer.Length)) != 0)
                                {
                                    mem.Write(buffer, 0, len);
                                }
                                mem.WriteTo(fileRenamed);
                                fileRenamed.Close();
                            }
                            output.Close();
                        }
                        mem.Flush();
                    }
                    input.Close();
                }

                File.Delete(FileToDeCompress);
            }
            catch (Exception)
            {

            }
        }

        public static void CreateConvertedDirectory(string InputFile)
        {
            try
            {
                BaseDirectory = Path.GetDirectoryName(InputFile)!;
                ConvertedDirectory = Path.Combine(Path.GetDirectoryName(InputFile)!, "Converted");

                Console.WriteLine(ConvertedDirectory);

                if (!Directory.Exists(ConvertedDirectory))
                {
                    Directory.CreateDirectory(ConvertedDirectory);
                }
            }
            catch (Exception)
            {

            }
        }

        public static void ExtractFiles(string InputFile)
        {
            try
            {
                // Reading Header
                byte[] magic = new byte[8];
                byte[] offset = new byte[4];
                string outFile = "notDefined";
                using BinaryReader NewBinaryReader = new(new FileStream(InputFile, FileMode.Open, FileAccess.Read));

                // Check Magic
                NewBinaryReader.Read(magic, 0, 8);

                if (!CompareBytes(magic, ngrcoMagic))
                {
                    // Wrong
                }

                // Get Data Table Offset and Length
                offset = new byte[4];
                byte[] eof = new byte[4];
                NewBinaryReader.BaseStream.Seek(0x48, SeekOrigin.Begin);
                NewBinaryReader.Read(offset, 0, 4);
                NewBinaryReader.Read(eof, 0, 4);
                Array.Reverse(offset);
                Array.Reverse(eof);

                // Check for zlib Header '0x78DA' (compression level=9) or VAG & PNG files and write to file
                end = Convert.ToInt32(Convert.ToHexString(eof), 16);
                count = Convert.ToInt32(Convert.ToHexString(offset), 16);
                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                NewBinaryReader.Read(zlib, 0, 2);

                // main loop
                if (!CompareBytes(zlib, singlZL))
                {
                    temp = new byte[16];
                    while ((i = NewBinaryReader.Read(temp, 0, 16)) != 0)
                    {
                        // In case of we now also have PS4 RCO's to work down and to not compromise the routine, we swapped the Extraction here 
                        // and placed the search for zlib files under the VAG and PNG file search
                        // For ZLib i removed the second routine that would read after the first Zlib compressed block, adding a 0 byte 0x00 on top of 0x78DA
                        // Instead of that, we simple counted +1 byte on end of dumping process and continue as usually

                        // Now we first fill the buffer's for the header's which we will compare after
                        NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                        NewBinaryReader.Read(vag, 0, 8);
                        NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                        NewBinaryReader.Read(cxml, 0, 8);
                        NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                        NewBinaryReader.Read(zlib, 0, 2);
                        NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                        NewBinaryReader.Read(png, 0, 16);
                        NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                        NewBinaryReader.Read(dds, 0, 4);
                        NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                        NewBinaryReader.Read(gtf, 0, 4);
                        NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                        NewBinaryReader.Read(wav, 0, 4);


                        #region pngExtract
                        if (CompareBytes(png, pngMagic))
                        {
                            outFile = Path.Combine(ConvertedDirectory, countPNG + ".png");
                            File.Create(outFile).Close();
                            byte[] toWrite = new byte[16];
                            _zlib = new byte[2];
                            _wav = new byte[4];
                            _gtf = new byte[4];
                            _dds = new byte[4];
                            _vag = new byte[8];
                            _cxml = new byte[8];
                            NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                            NewBinaryReader.Read(toWrite, 0, 16);

                            using (BinaryWriter bw = new(new FileStream(outFile, FileMode.Append, FileAccess.Write)))
                            {
                                // Before we Jump into the Loop we need to write out the first readed 16 bytes which are the PNG Magic.
                                // This is needed cause we need to compare for new Magic's / Header's to know if we reached the eof of current file.
                                // Otherwise the routine would detect a PNG Magic and stop right after we jumped in, resulting in not extracting the PNG and loosing
                                // the allready readed 16 bytes.
                                bw.Write(toWrite, 0, 16);

                                // count up the readed bytes and read next one before the loop start
                                dumped += 16;
                                count += 16;
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_zlib, 0, 2);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_dds, 0, 4);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_gtf, 0, 4);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_wav, 0, 4);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_vag, 0, 8);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_cxml, 0, 8);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(toWrite, 0, 16);

                                // Now let's start the Loop and extract the PNG
                                while (true)
                                {
                                    // Have we reached EOF ?
                                    if (!CompareBytes(toWrite, pngMagic))
                                    {
                                        if (!CompareBytes(_cxml, ngCXML))
                                        {
                                            if (!CompareBytes(_zlib, singlZL))
                                            {
                                                if (!CompareBytes(_vag, vagMagic))
                                                {
                                                    if (!CompareBytes(_wav, wavMagic))
                                                    {
                                                        if (!CompareBytes(_dds, ddsMagic))
                                                        {
                                                            if (!CompareBytes(_gtf, gtfMagic))
                                                            {
                                                                // Write out the readed data
                                                                bw.Write(toWrite, 0, 16);
                                                                dumped += 16;
                                                                count += 16;
                                                                if (dumped != end)
                                                                {
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_zlib, 0, 2);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_dds, 0, 4);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_gtf, 0, 4);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_wav, 0, 4);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_vag, 0, 8);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_cxml, 0, 8);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(toWrite, 0, 16);
                                                                }
                                                                else
                                                                    break;
                                                            }
                                                            else
                                                                break;
                                                        }
                                                        else
                                                            break;
                                                    }
                                                    else
                                                        break;
                                                }
                                                else
                                                    break;
                                            }
                                            else
                                                break;
                                        }
                                        else
                                            break;
                                    }
                                    else
                                        break;
                                }
                                bw.Close();
                            }
                            countPNG++;
                        }
                        #endregion pngExtract
                        #region cxmlExtract
                        else if (CompareBytes(cxml, ngCXML))
                        {
                            outFile = Path.Combine(ConvertedDirectory, countCXML + ".cxml");
                            File.Create(outFile).Close();
                            byte[] toWrite = new byte[16];
                            _vag = new byte[8];
                            _zlib = new byte[2];
                            _wav = new byte[4];
                            _gtf = new byte[4];
                            _dds = new byte[4];
                            _cxml = new byte[8];
                            NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                            NewBinaryReader.Read(toWrite, 0, 16);

                            using (BinaryWriter bw = new(new FileStream(outFile, FileMode.Append, FileAccess.Write)))
                            {
                                bw.Write(toWrite, 0, 16);
                                dumped += 16;
                                count += 16;
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_zlib, 0, 2);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_dds, 0, 4);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_gtf, 0, 4);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_wav, 0, 4);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_vag, 0, 8);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_cxml, 0, 8);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(toWrite, 0, 16);

                                while (true)
                                {
                                    if (!CompareBytes(_cxml, ngCXML))
                                    {
                                        if (!CompareBytes(toWrite, pngMagic))
                                        {
                                            if (!CompareBytes(_wav, wavMagic))
                                            {
                                                if (!CompareBytes(_dds, ddsMagic))
                                                {
                                                    if (!CompareBytes(_gtf, gtfMagic))
                                                    {
                                                        if (!CompareBytes(_zlib, singlZL))
                                                        {
                                                            if (!CompareBytes(_vag, vagMagic))
                                                            {
                                                                if (dumped != end)
                                                                {
                                                                    bw.Write(toWrite, 0, 16);
                                                                    dumped += 16;
                                                                    count += 16;
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_zlib, 0, 2);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_dds, 0, 4);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_gtf, 0, 4);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_wav, 0, 4);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_vag, 0, 8);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_cxml, 0, 8);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(toWrite, 0, 16);
                                                                }
                                                                else
                                                                    break;
                                                            }
                                                            else
                                                                break;
                                                        }
                                                        else
                                                            break;
                                                    }
                                                    else
                                                        break;
                                                }
                                                else
                                                    break;
                                            }
                                            else
                                                break;
                                        }
                                        else
                                            break;
                                    }
                                    else
                                        break;
                                }
                                bw.Close();
                            }
                            countCXML++;
                        }
                        #endregion cxmlExtract
                        #region ddsExtract
                        else if (CompareBytes(dds, ddsMagic))
                        {
                            outFile = Path.Combine(ConvertedDirectory, countDDS + ".dds");
                            File.Create(outFile).Close();
                            byte[] toWrite = new byte[16];
                            _vag = new byte[8];
                            _zlib = new byte[2];
                            _wav = new byte[4];
                            _gtf = new byte[4];
                            _dds = new byte[4];
                            _cxml = new byte[8];
                            NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                            NewBinaryReader.Read(toWrite, 0, 16);

                            using (BinaryWriter bw = new(new FileStream(outFile, FileMode.Append, FileAccess.Write)))
                            {
                                bw.Write(toWrite, 0, 16);
                                dumped += 16;
                                count += 16;
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_zlib, 0, 2);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_dds, 0, 4);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_gtf, 0, 4);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_wav, 0, 4);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_vag, 0, 8);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_cxml, 0, 8);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(toWrite, 0, 16);

                                while (true)
                                {
                                    if (!CompareBytes(_cxml, ngCXML))
                                    {
                                        if (!CompareBytes(toWrite, pngMagic))
                                        {
                                            if (!CompareBytes(_wav, wavMagic))
                                            {
                                                if (!CompareBytes(_dds, ddsMagic))
                                                {
                                                    if (!CompareBytes(_gtf, gtfMagic))
                                                    {
                                                        if (!CompareBytes(_zlib, singlZL))
                                                        {
                                                            if (!CompareBytes(_vag, vagMagic))
                                                            {
                                                                if (dumped != end)
                                                                {
                                                                    bw.Write(toWrite, 0, 16);
                                                                    dumped += 16;
                                                                    count += 16;
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_zlib, 0, 2);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_dds, 0, 4);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_gtf, 0, 4);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_wav, 0, 4);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_vag, 0, 8);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_cxml, 0, 8);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(toWrite, 0, 16);
                                                                }
                                                                else
                                                                    break;
                                                            }
                                                            else
                                                                break;
                                                        }
                                                        else
                                                            break;
                                                    }
                                                    else
                                                        break;
                                                }
                                                else
                                                    break;
                                            }
                                            else
                                                break;
                                        }
                                        else
                                            break;
                                    }
                                    else
                                        break;
                                }
                                bw.Close();
                            }
                            countDDS++;
                        }
                        #endregion ddsExtract
                        #region gtfExtract
                        else if (CompareBytes(gtf, gtfMagic))
                        {
                            outFile = Path.Combine(ConvertedDirectory, countGTF + ".gtf");
                            File.Create(outFile).Close();
                            byte[] toWrite = new byte[16];
                            _vag = new byte[8];
                            _zlib = new byte[2];
                            _wav = new byte[4];
                            _gtf = new byte[4];
                            _dds = new byte[4];
                            _cxml = new byte[8];
                            NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                            NewBinaryReader.Read(toWrite, 0, 16);

                            using (BinaryWriter bw = new(new FileStream(outFile, FileMode.Append, FileAccess.Write)))
                            {
                                bw.Write(toWrite, 0, 16);
                                dumped += 16;
                                count += 16;
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_zlib, 0, 2);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_dds, 0, 4);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_gtf, 0, 4);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_wav, 0, 4);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_vag, 0, 8);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_cxml, 0, 8);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(toWrite, 0, 16);

                                while (true)
                                {
                                    if (!CompareBytes(_cxml, ngCXML))
                                    {
                                        if (!CompareBytes(toWrite, pngMagic))
                                        {
                                            if (!CompareBytes(_wav, wavMagic))
                                            {
                                                if (!CompareBytes(_dds, ddsMagic))
                                                {
                                                    if (!CompareBytes(_gtf, gtfMagic))
                                                    {
                                                        if (!CompareBytes(_zlib, singlZL))
                                                        {
                                                            if (!CompareBytes(_vag, vagMagic))
                                                            {
                                                                if (dumped != end)
                                                                {
                                                                    bw.Write(toWrite, 0, 16);
                                                                    dumped += 16;
                                                                    count += 16;
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_zlib, 0, 2);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_dds, 0, 4);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_gtf, 0, 4);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_wav, 0, 4);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_vag, 0, 8);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_cxml, 0, 8);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(toWrite, 0, 16);
                                                                }
                                                                else
                                                                    break;
                                                            }
                                                            else
                                                                break;
                                                        }
                                                        else
                                                            break;
                                                    }
                                                    else
                                                        break;
                                                }
                                                else
                                                    break;
                                            }
                                            else
                                                break;
                                        }
                                        else
                                            break;
                                    }
                                    else
                                        break;
                                }
                                bw.Close();
                            }
                            countGTF++;
                        }
                        #endregion gtfExtract
                        #region wavExtract
                        else if (CompareBytes(wav, wavMagic))
                        {
                            outFile = Path.Combine(ConvertedDirectory, countWAV + ".wav");
                            File.Create(outFile).Close();
                            byte[] toWrite = new byte[16];
                            _vag = new byte[8];
                            _zlib = new byte[2];
                            _wav = new byte[4];
                            _gtf = new byte[4];
                            _dds = new byte[4];
                            _cxml = new byte[8];
                            NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                            NewBinaryReader.Read(toWrite, 0, 16);

                            using (BinaryWriter bw = new(new FileStream(outFile, FileMode.Append, FileAccess.Write)))
                            {
                                bw.Write(toWrite, 0, 16);
                                dumped += 16;
                                count += 16;
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_zlib, 0, 2);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_dds, 0, 4);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_gtf, 0, 4);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_wav, 0, 4);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_vag, 0, 8);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(_cxml, 0, 8);
                                NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                NewBinaryReader.Read(toWrite, 0, 16);

                                while (true)
                                {
                                    if (!CompareBytes(_cxml, ngCXML))
                                    {
                                        if (!CompareBytes(toWrite, pngMagic))
                                        {
                                            if (!CompareBytes(_wav, wavMagic))
                                            {
                                                if (!CompareBytes(_dds, ddsMagic))
                                                {
                                                    if (!CompareBytes(_gtf, gtfMagic))
                                                    {
                                                        if (!CompareBytes(_zlib, singlZL))
                                                        {
                                                            if (!CompareBytes(_vag, vagMagic))
                                                            {
                                                                if (dumped != end)
                                                                {
                                                                    bw.Write(toWrite, 0, 16);
                                                                    dumped += 16;
                                                                    count += 16;
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_zlib, 0, 2);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_dds, 0, 4);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_gtf, 0, 4);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_wav, 0, 4);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_vag, 0, 8);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(_cxml, 0, 8);
                                                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                                                    NewBinaryReader.Read(toWrite, 0, 16);
                                                                }
                                                                else
                                                                    break;
                                                            }
                                                            else
                                                                break;
                                                        }
                                                        else
                                                            break;
                                                    }
                                                    else
                                                        break;
                                                }
                                                else
                                                    break;
                                            }
                                            else
                                                break;
                                        }
                                        else
                                            break;
                                    }
                                    else
                                        break;
                                }
                                bw.Close();
                            }
                            countWAV++;
                        }
                        #endregion wavExtract
                        else
                        {
                            break;
                        }
                        if (dumped == end)
                            break;
                        else
                            NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                    }
                }
                else if (CompareBytes(zlib, singlZL))
                {
                    while ((i = NewBinaryReader.Read(temp, 0, 1)) != 0)
                    {
                        #region zlibExtract
                        _zlib = new byte[3];
                        NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                        NewBinaryReader.Read(_zlib, 0, 3);

                        byte[] toWrite = new byte[1];
                        outFile = Path.Combine(ConvertedDirectory, countZLIB + ".compressed");
                        CorrectExtension = "";
                        File.Create(outFile).Close();

                        using (BinaryWriter bw = new(new FileStream(outFile, FileMode.Append, FileAccess.Write)))
                        {
                            while (true)
                            {
                                if (!CompareBytes(_zlib, zlibMagic))  // Next Byte is not the start of a Header from a other file ?
                                {
                                    // write out data to file
                                    NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                    NewBinaryReader.Read(toWrite, 0, 1);
                                    bw.Write(toWrite, 0, 1);

                                    // Count up 1 and read the next byte(s) before the loop start again
                                    count++;
                                    dumped++;

                                    // Have we reached the end of data table?
                                    if (dumped != end)
                                    {
                                        NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                                        NewBinaryReader.Read(_zlib, 0, 3);
                                    }
                                    else
                                        break;
                                }
                                else
                                    break;
                            }

                            // In case of we need to compare zlibHeader with a additional 0x00 on top to know if it is really a zlibHeader and not just a 0x78DA data value
                            // we add that 0x00 on end of the extracted file and count dumped var +1
                            if (CompareBytes(_zlib, zlibMagic))
                            {
                                toWrite = new byte[1];
                                bw.Write(toWrite, 0, 1);
                                dumped++;
                                count++;
                                countZLIB++;
                            }
                            bw.Close();
                        }

                        // Decompress dumped File
                        if (outFile != "notDefined")
                            ZLibDeCompress(outFile);

                        // Check Header of Decompressed File and rename
                        outFile += ".decompressed";

                        using (BinaryReader _br = new(new FileStream(outFile, FileMode.Open, FileAccess.Read)))
                        {
                            byte[] xmlHeader = new byte[8];
                            byte[] gimHeader = new byte[16];
                            byte[] ddsHeader = new byte[4];
                            _br.Read(xmlHeader, 0, 8);
                            _br.BaseStream.Seek(0, SeekOrigin.Begin);
                            _br.Read(gimHeader, 0, 16);
                            _br.BaseStream.Seek(0, SeekOrigin.Begin);
                            _br.Read(ddsHeader, 0, 4);

                            if (CompareBytes(xmlHeader, ngCXML))
                            {
                                countCXML++;
                                CorrectExtension = outFile.Replace(".compressed.decompressed", ".cxml");

                            }
                            else if (CompareBytes(gimHeader, gimMagic))
                            {
                                countGim++;
                                CorrectExtension = outFile.Replace(".compressed.decompressed", ".gim");
                                move = CorrectExtension.Replace(".gim", ".png");
                                dest = Path.Combine(ConvertedDirectory, countGim + ".png");
                            }
                            else if (CompareBytes(ddsHeader, ddsMagic))
                            {
                                countDDS++;
                                CorrectExtension = outFile.Replace(".compressed.decompressed", ".dds");
                                move = CorrectExtension.Replace(".dds", ".gtf");
                                dest = Path.Combine(ConvertedDirectory, countDDS + ".gtf");
                            }

                            _br.Close();
                        }

                        // Finally Rename to the correct extension
                        File.Move(outFile, CorrectExtension);

                        #endregion ZlibExtract

                        if (dumped == end)
                        {
                            string FinalDestination = "";
                            string[] ExtractedFiles = Directory.GetFiles(BaseDirectory);

                            foreach (string foundFile in ExtractedFiles)
                            {
                                if (foundFile.Contains(".png") || foundFile.Contains(".gtf"))
                                {
                                    FinalDestination = Path.Combine(ConvertedDirectory, Path.GetFileName(foundFile));
                                    File.Move(foundFile, FinalDestination);
                                }
                            }
                            break;
                        }
                        else
                            NewBinaryReader.BaseStream.Seek(count, SeekOrigin.Begin);
                    }
                    NewBinaryReader.Close();
                }
            }
            catch (Exception)
            {

            }
        }

    }
}
