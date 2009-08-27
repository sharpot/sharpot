using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace SharpOT.Util
{
    public static class Hash
    {
        /// <summary>
        /// Encrypts a string using the SHA256 (Secure Hash Algorithm) algorithm.
        /// Details: http://www.itl.nist.gov/fipspubs/fip180-1.htm
        /// This works in the same manner as MD5, providing however 256bit encryption.
        /// </summary>
        /// <param name="Data">A string containing the data to encrypt.</param>
        /// <returns>A string containing the string, encrypted with the SHA256 algorithm.</returns>
        public static string SHA256Hash(string data)
        {
            SHA256 sha = new SHA256Managed();
            byte[] hash = sha.ComputeHash(Encoding.Unicode.GetBytes(data));

            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hash)
            {
                stringBuilder.AppendFormat("{0:x2}", b);
            }
            return stringBuilder.ToString();
        }

    }
}
