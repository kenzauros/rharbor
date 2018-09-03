using System;
using System.Security;

namespace kenzauros.RHarbor.Models
{
    internal interface IPassword
    {
        string Password { get; set; }
        SecureString SecurePassword { get; set; }
    }

    internal static class IPasswordEx
    {
        public static void WritebackPassword(this IPassword model)
        {
            model.Password = model.SecurePassword?.GetEncryptedString();
        }

        public static void InitSecurePassword(this IPassword model)
        {
            if (model.Password != null)
            {
                try
                {
                    var ss = new SecureString();
                    foreach (var c in AESEncryption.Decrypt(model.Password).ToCharArray())
                    {
                        ss.AppendChar(c);
                    }
                    model.SecurePassword = ss;
                }
                catch (Exception)
                {
                    model.SecurePassword = null;
                }
            }
        }
    }
}
