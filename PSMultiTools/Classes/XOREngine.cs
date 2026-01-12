using System;

namespace PSMultiTools.Classes
{
    public class XOREngine
    {

        public static byte[] GetXOR(byte[] inByteArray, int offsetPos, int length, byte[] XORKey)
        {
            if (inByteArray.Length < offsetPos + length)
            {
                throw new Exception("Combination of chosen offset pos. & Length goes outside of the array to be xored.");
            }

            if (length % XORKey.Length != 0)
            {
                throw new Exception("Nr bytes to be xored isn't a mutiple of xor key length.");
            }

            int pieces = length / XORKey.Length;

            byte[] outByteArray = new byte[length];

            for (int i = 0, loopTo = pieces - 1; i <= loopTo; i++)
            {
                for (int pos = 0, loopTo1 = XORKey.Length - 1; pos <= loopTo1; pos++)
                    outByteArray[i * XORKey.Length + pos] += (byte)(inByteArray[offsetPos + i * XORKey.Length + pos] ^ XORKey[pos]);
            }

            return outByteArray;
        }

    }
}