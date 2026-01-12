using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PSMultiTools.Classes
{

    public class MD5Hash
    {

        public static string MD5StringHash(string InputString)
        {
            byte[] data = Encoding.ASCII.GetBytes(InputString);
            byte[] hash = MD5.HashData(data);
            var sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public static string MD5FileHash(string InputFile)
        {
            using var stream = new FileStream(InputFile, FileMode.Open, FileAccess.Read, FileShare.Read, 8192);
            byte[] hash = MD5.HashData(stream);
            var sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

    }
}