using System;
using System.Security.Cryptography;
using System.Text;

namespace Finexa.Shared.Security
{
    public static class HashHelper
    {
        public static string GenerateSalt()
        {
            byte[] bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        public static string ComputeHash(string password, string salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] combinedBytes = new byte[passwordBytes.Length + saltBytes.Length];

            Buffer.BlockCopy(passwordBytes, 0, combinedBytes, 0, passwordBytes.Length);
            Buffer.BlockCopy(saltBytes, 0, combinedBytes, passwordBytes.Length, saltBytes.Length);

            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public static bool VerifyPassword(string password, string hash, string salt)
        {
            string computedHash = ComputeHash(password, salt);
            return computedHash == hash;
        }
    }
}
