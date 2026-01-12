using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PSMultiTools.Classes
{

    public class AESEngine
    {

        public static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV, CipherMode cipherMode, PaddingMode paddingMode)
        {
            using var ms = new MemoryStream();
            using (var alg = Aes.Create())
            {
                alg.Mode = cipherMode;
                alg.Padding = paddingMode;
                alg.Key = Key;
                alg.IV = IV;

                using var cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(clearData, 0, clearData.Length);
            }
            return ms.ToArray();
        }

        public static string Encrypt(string clearText, string Password, CipherMode cipherMode, PaddingMode paddingMode)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            byte[] salt = [0x49, 0x76, 0x61, 0x6E, 0x20, 0x4D, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76];
            var pdb = new Rfc2898DeriveBytes(Password, salt, 10000, HashAlgorithmName.SHA256);  // New overload using SHA256

            byte[] encryptedData = Encrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16), cipherMode, paddingMode);
            return Convert.ToBase64String(encryptedData);
        }

        public static byte[] Encrypt(byte[] clearData, string Password, CipherMode cipherMode, PaddingMode paddingMode)
        {
            byte[] salt = [0x49, 0x76, 0x61, 0x6E, 0x20, 0x4D, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76];
            var pdb = new Rfc2898DeriveBytes(Password, salt, 10000, HashAlgorithmName.SHA256);
            return Encrypt(clearData, pdb.GetBytes(32), pdb.GetBytes(16), cipherMode, paddingMode);
        }

        public static void Encrypt(string fileIn, string fileOut, string Password, CipherMode cipherMode, PaddingMode paddingMode)
        {
            byte[] salt = [0x49, 0x76, 0x61, 0x6E, 0x20, 0x4D, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76];
            var pdb = new Rfc2898DeriveBytes(Password, salt, 10000, HashAlgorithmName.SHA256);
            using var fsIn = new FileStream(fileIn, FileMode.Open, FileAccess.Read);
            using var fsOut = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write);
            using var alg = Aes.Create();
            alg.Mode = cipherMode;
            alg.Padding = paddingMode;
            alg.Key = pdb.GetBytes(32);
            alg.IV = pdb.GetBytes(16);

            using var cs = new CryptoStream(fsOut, alg.CreateEncryptor(), CryptoStreamMode.Write);
            int bufferLen = 4096;
            byte[] buffer = new byte[bufferLen];
            int bytesRead;

            do
            {
                bytesRead = fsIn.Read(buffer, 0, bufferLen);
                if (bytesRead > 0)
                {
                    cs.Write(buffer, 0, bytesRead);
                }
            }
            while (bytesRead != 0);
        }

        public static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV, CipherMode cipherMode, PaddingMode paddingMode)
        {
            using var ms = new MemoryStream();
            using (var alg = Aes.Create())
            {
                alg.Mode = cipherMode;
                alg.Padding = paddingMode;
                alg.Key = Key;
                alg.IV = IV;

                using var cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(cipherData, 0, cipherData.Length);
            }
            return ms.ToArray();
        }

        public static string Decrypt(string cipherText, string Password, CipherMode cipherMode, PaddingMode paddingMode)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] salt = [0x49, 0x76, 0x61, 0x6E, 0x20, 0x4D, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76];
            var pdb = new Rfc2898DeriveBytes(Password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16), cipherMode, paddingMode);
            return Encoding.Unicode.GetString(decryptedData);
        }

        public static byte[] Decrypt(byte[] cipherData, string Password, CipherMode cipherMode, PaddingMode paddingMode)
        {
            byte[] salt = [0x49, 0x76, 0x61, 0x6E, 0x20, 0x4D, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76];
            var pdb = new Rfc2898DeriveBytes(Password, salt, 10000, HashAlgorithmName.SHA256);
            return Decrypt(cipherData, pdb.GetBytes(32), pdb.GetBytes(16), cipherMode, paddingMode);
        }

        public static void Decrypt(string fileIn, string fileOut, string Password, CipherMode cipherMode, PaddingMode paddingMode)
        {
            byte[] salt = [0x49, 0x76, 0x61, 0x6E, 0x20, 0x4D, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76];
            var pdb = new Rfc2898DeriveBytes(Password, salt, 10000, HashAlgorithmName.SHA256);
            using var fsIn = new FileStream(fileIn, FileMode.Open, FileAccess.Read);
            using var fsOut = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write);
            using var alg = Aes.Create();
            alg.Mode = cipherMode;
            alg.Padding = paddingMode;
            alg.Key = pdb.GetBytes(32);
            alg.IV = pdb.GetBytes(16);

            using var cs = new CryptoStream(fsOut, alg.CreateDecryptor(), CryptoStreamMode.Write);
            int bufferLen = 4096;
            byte[] buffer = new byte[bufferLen];
            int bytesRead;

            do
            {
                bytesRead = fsIn.Read(buffer, 0, bufferLen);
                if (bytesRead > 0)
                {
                    cs.Write(buffer, 0, bytesRead);
                }
            }
            while (bytesRead != 0);
        }

    }
}