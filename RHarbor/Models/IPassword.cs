using System;

namespace kenzauros.RHarbor.Models
{
    internal interface IPassword
    {
        string Password { get; set; }
        string RawPassword { get; set; }
    }

    internal static class IPasswordEx
    {
        public static void WritebackPassword(this IPassword model)
        {
            model.Password = AESEncryption.Encrypt(model.RawPassword);
        }

        public static void DecryptPassword(this IPassword model)
        {
            if (model.Password != null)
            {
                try
                {
                    model.RawPassword = AESEncryption.Decrypt(model.Password);
                }
                catch (Exception)
                {
                    model.RawPassword = null;
                }
            }
        }
    }
}
