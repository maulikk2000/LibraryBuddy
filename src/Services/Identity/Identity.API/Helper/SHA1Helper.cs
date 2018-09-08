using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Identity.API.Helper
{
    /// <summary>
    /// Helper class to convert password to SHA1 Hash
    /// </summary>
    public static class SHA1Helper
    {
        public static string ComputeSHA1Hash(string password)
        {
            SHA1 sha1 = SHA1.Create();

            byte[] bytes = Encoding.Default.GetBytes(password);

            //Hashing with SHA1 Algorithm in C#
            //https://stackoverflow.com/questions/17292366/hashing-with-sha1-algorithm-in-c-sharp
            byte[] hash = sha1.ComputeHash(bytes);

            var sb = new StringBuilder(hash.Length * 2);

            foreach (byte b in hash)
            {
                // can be "x2" if you want lowercase
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
