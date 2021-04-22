using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace kenzauros.RHarbor
{
    public static class AesUtil
    {
        const string DefaultIV = @"SHNKY0x2bTdiUDRGdlVYaw==";
        const string DefaultSalt = "afjsHd2i43&n)Ro-ada";

        public static AesManaged GetStrategy(string iv, string key) =>
           new()
           {
               KeySize = 256,
               BlockSize = 128,
               Mode = CipherMode.CBC,
               IV = Convert.FromBase64String(iv),
               Key = Encoding.UTF8.GetBytes(key),
               Padding = PaddingMode.PKCS7,
           };

        /// <summary>
        /// Generates a 32 bytes key from the password.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string GenerateKeyFromPassword(string password, string salt = null)
        {
            salt ??= DefaultSalt;
            var key = (password + salt).GetMD5Hash();
            return key;
        }

        /// <summary>
        /// Enrypts the data with the key.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CryptographicException">Failed to encrypt</exception>
        public static byte[] Encrypt(byte[] bytes, string key, string iv = null)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (key?.Length != 32) // 32 bytes
                throw new ArgumentException("Key length must be 32 bytes.", nameof(key));

            using var aes = GetStrategy(iv ?? DefaultIV, key);
            return aes.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Decrypts the data with the key.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="CryptographicException">Failed to decrypt</exception>
        public static byte[] Decrypt(byte[] bytes, string key, string iv = null)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (key?.Length != 32) // 32 bytes
                throw new ArgumentException("Key length must be 32 bytes.", nameof(key));

            using var aes = GetStrategy(iv ?? DefaultIV, key);
            using var decryptor = aes.CreateDecryptor();
            using var sourceStream = new MemoryStream(bytes);
            using var decryptStream = new CryptoStream(sourceStream, decryptor, CryptoStreamMode.Read);
            using var destStream = new MemoryStream();
            decryptStream.CopyTo(destStream);
            return destStream.ToArray();
        }


    }
}
