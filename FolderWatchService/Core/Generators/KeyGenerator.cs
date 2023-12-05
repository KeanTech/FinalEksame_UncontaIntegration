using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FolderWatchService.Core.Generators
{
    /// <summary>
    /// This class is used to generate random arrays used in key generation.
    /// </summary>
    public class KeyGenerator
    {
        /// <summary>
        /// Generates a random byte array using <see cref="RNGCryptoServiceProvider"/>
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] GenerateRandomByteArray(int length)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] key = new byte[length];
                rng.GetBytes(key);
                return key;
            }
        }
    }
}
