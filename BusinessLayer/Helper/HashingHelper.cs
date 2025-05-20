using System.Security.Cryptography;
using System.Text;

namespace BusinessLayer.Helper;

public class HashingHelper
    {
        public static string ComputeSHA256(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2")); // Convert byte to hex string
                }
                return builder.ToString();
            }
        }

        internal static object ComputeSHA256(object oldPassword)
        {
            throw new NotImplementedException();
        }

    }