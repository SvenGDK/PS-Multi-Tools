using System;

namespace PSMultiTools.Classes
{
    public class BytesConverter
    {

        public static byte[] ToLittleEndian(uint Value)
        {
            byte[] buffer = BitConverter.GetBytes(Value);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }

            return buffer;
        }

        public static byte[] ToLittleEndian(ulong Value)
        {
            byte[] buffer = BitConverter.GetBytes(Value);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }

            return buffer;
        }

    }
}