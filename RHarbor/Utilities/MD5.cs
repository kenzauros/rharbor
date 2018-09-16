using System;
using System.IO;
using System.Text;

namespace kenzauros.RHarbor
{
    internal static class MD5
    {
        /// <summary>
        /// Compute the hash of the text.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetMD5Hash(this string s)
        {
            var data = Encoding.UTF8.GetBytes(s);
            var crypt = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var bs = crypt.ComputeHash(data);
            crypt.Clear();
            var result = new StringBuilder();
            foreach (byte b in bs)
            {
                result.Append(b.ToString("x2"));
            }
            return result.ToString();
        }

        public static string GetFileMD5Hash(string filepath)
        {
            using (var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (var crypt = new System.Security.Cryptography.MD5CryptoServiceProvider())
            {
                var hashedBytes = crypt.ComputeHash(fs);
                return BitConverter.ToString(hashedBytes).ToLower();
            }
        }
    }
}
