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
    }
}
