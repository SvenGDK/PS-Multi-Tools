using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PSMultiTools.Classes
{

    public class SFONew
    {

        public static Dictionary<string, object> ReadSfo(Stream ParamSFO)
        {
            var SfoValues = new Dictionary<string, object>();
            var NewSFOContent = new Structures.ParamSFOContent();
            uint Magic = ReadUInt32(ParamSFO);
            uint Version = ReadUInt32(ParamSFO);
            uint KeyOffset = ReadUInt32(ParamSFO);
            uint ValueOffset = ReadUInt32(ParamSFO);
            uint Count = ReadUInt32(ParamSFO);

            if (Magic == 1179865088L)
            {
                for (int i = 0, loopTo = (int)(Count - 1L); i <= loopTo; i++)
                {
                    ushort NameOffset = ReadUInt16(ParamSFO);
                    byte Alignment = (byte)ParamSFO.ReadByte();
                    byte Type = (byte)ParamSFO.ReadByte();
                    uint ValueSize = ReadUInt32(ParamSFO);
                    uint TotalSize = ReadUInt32(ParamSFO);
                    uint DataOffset = ReadUInt32(ParamSFO);
                    int KeyLocation = Convert.ToInt32(KeyOffset + NameOffset);
                    string KeyName = ReadStringAt(ParamSFO, KeyLocation);
                    int ValueLocation = Convert.ToInt32(ValueOffset + DataOffset);
                    object Value = "Unknown Type";

                    switch (Type)
                    {
                        case 2:
                            {
                                Value = ReadStringAt(ParamSFO, ValueLocation);
                                NewSFOContent.ParamType = 2;
                                break;
                            }
                        case 4:
                            {
                                Value = ReadUint32At(ParamSFO, ValueLocation + i);
                                NewSFOContent.ParamType = 4;
                                break;
                            }
                        case 0:
                            {
                                Value = ReadBytesAt(ParamSFO, ValueLocation + i, Convert.ToInt32(ValueSize));
                                NewSFOContent.ParamType = 2;
                                break;
                            }
                    }

                    SfoValues[KeyName] = Value;
                }
            }

            return SfoValues;
        }

        public static Dictionary<string, object> ReadSfo(byte[] ParamSFO)
        {
            var SfoStream = new MemoryStream(ParamSFO);
            return ReadSfo(SfoStream);
        }

        public static void CopyString(byte[] str, string Text, int Index)
        {
            byte[] TextBytes = Encoding.UTF8.GetBytes(Text);
            Array.ConstrainedCopy(TextBytes, 0, str, Index, TextBytes.Length);
        }

        public static void CopyInt32(byte[] str, int Value, int Index)
        {
            byte[] ValueBytes = BitConverter.GetBytes(Value);
            Array.ConstrainedCopy(ValueBytes, 0, str, Index, ValueBytes.Length);
        }

        public static uint ReadUInt32(Stream Str)
        {
            byte[] IntBytes = new byte[4];
            Str.ReadExactly(IntBytes, 0x0, IntBytes.Length);
            return BitConverter.ToUInt32(IntBytes, 0x0);
        }

        public static uint ReadInt32(Stream Str)
        {
            byte[] IntBytes = new byte[4];
            Str.ReadExactly(IntBytes, 0x0, IntBytes.Length);
            return BitConverter.ToUInt32(IntBytes, 0x0);
        }

        public static ulong ReadUInt64(Stream Str)
        {
            byte[] IntBytes = new byte[8];
            Str.ReadExactly(IntBytes, 0x0, IntBytes.Length);
            return BitConverter.ToUInt64(IntBytes, 0x0);
        }

        public static long ReadInt64(Stream Str)
        {
            byte[] IntBytes = new byte[8];
            Str.ReadExactly(IntBytes, 0x0, IntBytes.Length);
            return BitConverter.ToInt64(IntBytes, 0x0);
        }

        public static ushort ReadUInt16(Stream Str)
        {
            byte[] IntBytes = new byte[2];
            Str.ReadExactly(IntBytes, 0x0, IntBytes.Length);
            return BitConverter.ToUInt16(IntBytes, 0x0);
        }

        public static short ReadInt16(Stream Str)
        {
            byte[] IntBytes = new byte[2];
            Str.ReadExactly(IntBytes, 0x0, IntBytes.Length);
            return BitConverter.ToInt16(IntBytes, 0x0);
        }

        public static uint ReadUint32At(Stream Str, int location)
        {
            long oldPos = Str.Position;
            Str.Seek(location, SeekOrigin.Begin);
            uint outp = ReadUInt32(Str);
            Str.Seek(oldPos, SeekOrigin.Begin);
            return outp;
        }

        public static byte[] ReadBytesAt(Stream Str, int location, int length)
        {
            long oldPos = Str.Position;
            Str.Seek(location, SeekOrigin.Begin);
            byte[] work_buf = new byte[length];
            Str.ReadExactly(work_buf, 0x0, work_buf.Length);
            Str.Seek(oldPos, SeekOrigin.Begin);
            return work_buf;
        }

        public static string ReadStringAt(Stream Str, int location)
        {
            long oldPos = Str.Position;
            Str.Seek(location, SeekOrigin.Begin);
            string outp = ReadString(Str);
            Str.Seek(oldPos, SeekOrigin.Begin);
            return outp;
        }

        public static string ReadString(Stream Str)
        {
            var ms = new MemoryStream();

            while (true)
            {
                byte c = (byte)Str.ReadByte();
                if (c == 0)
                    break;
                ms.WriteByte(c);
            }

            ms.Seek(0x0L, SeekOrigin.Begin);
            string outp = Encoding.UTF8.GetString(ms.ToArray());
            ms.Dispose();
            return outp;
        }

        public static void WriteUInt32(Stream Str, uint Numb)
        {
            byte[] IntBytes = BitConverter.GetBytes(Numb);
            Str.Write(IntBytes, 0x0, IntBytes.Length);
        }

        public static void WriteInt32(Stream Str, int Numb)
        {
            byte[] IntBytes = BitConverter.GetBytes(Numb);
            Str.Write(IntBytes, 0x0, IntBytes.Length);
        }

        public static void WriteUInt64(Stream dst, ulong value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x0, 0x8);
        }

        public static void WriteInt64(Stream dst, long value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x0, 0x8);
        }

        public static void WriteUInt16(Stream dst, ushort value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x0, 0x2);
        }

        public static void WriteInt16(Stream dst, short value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x0, 0x2);
        }

        public static void WriteString(Stream Str, string Text, int len = -1)
        {
            if (len < 0)
            {
                len = Text.Length;
            }

            byte[] TextBytes = Encoding.UTF8.GetBytes(Text);
            Str.Write(TextBytes, 0x0, len);
        }

    }
}