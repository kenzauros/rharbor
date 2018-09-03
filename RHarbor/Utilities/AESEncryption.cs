using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace kenzauros.RHarbor
{
    internal class AESEncryption
    {
        // 16 bytes
        const string iv = @"XkssbU19Nl5BTi50flhNNg==";
        // 32 bytes
        const string key = @"RjduPzI+RUFadnNGOSNnRSU4M2EsTEVFPXN5Lmt5QSo=";

        private static AesManaged Aes =>
            new AesManaged
            {
                KeySize = 256,
                BlockSize = 128,
                Mode = CipherMode.CBC,
                IV = Convert.FromBase64String(iv),
                Key = Convert.FromBase64String(key),
                Padding = PaddingMode.PKCS7
            };


        public static string Encrypt(string text)
        {
            var byteText = Encoding.UTF8.GetBytes(text);
            var ecrypted = Aes.CreateEncryptor().TransformFinalBlock(byteText, 0, byteText.Length);
            return Convert.ToBase64String(ecrypted);
        }

        public static string Decrypt(string ecrypted)
        {
            var byteText = Convert.FromBase64String(ecrypted);
            //var plain = Aes.CreateDecryptor().TransformFinalBlock(byteText, 0, byteText.Length);
            using (var aes = Aes)
            using (var decryptor = aes.CreateDecryptor())
            using (var msDecrypt = new MemoryStream(byteText))
            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (var srDecrypt = new StreamReader(csDecrypt))
            {

                // Read the decrypted bytes from the decrypting stream
                // and place them in a string.
                var plain = srDecrypt.ReadToEnd();
                return plain;
            }
        }
    }
}
