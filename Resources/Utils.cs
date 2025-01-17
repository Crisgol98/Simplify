using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Simplify.Resources.Utils
{
    public class Utils
    {
        public static string EncryptPassword(string password)
        {
            StringBuilder sb = new StringBuilder();
            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;

                byte[] result = hash.ComputeHash(enc.GetBytes(password));
                foreach (byte b in result)
                {
                    sb.Append(b.ToString("x2"));
                }
            }
            return sb.ToString();
        }
        public static T? ParseOrDefault<T>(string input, T? defaultValue = null) where T : struct
        {
            if (typeof(T).IsEnum)
            {
                return Enum.TryParse(input, true, out T result) ? result : defaultValue;
            }

            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(input, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        public static DateTime GetWeekStart()
        {
            DateTime today = DateTime.Now;
            int diff = today.DayOfWeek - DayOfWeek.Monday;
            return today.AddDays(-diff).Date;
        }
        public static string TextAbbreviation(string text, int length)
        {
            if (string.IsNullOrEmpty(text) || length <= 0)
                return string.Empty;

            return text.Length > length
                ? text.Substring(0, length) + "..."
                : text;
        }
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);

                byte[] hashBytes = new byte[48];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 32);

                return Convert.ToBase64String(hashBytes);
            }
        }
        public static bool VerifyPassword(string password, string storedHash)
        {
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);

                for (int i = 0; i < 32; i++)
                {
                    if (hashBytes[i + 16] != hash[i])
                        return false;
                }
            }

            return true;
        }
    }
}
