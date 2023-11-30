using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FolderWatchService.Core.Generators
{
    public class KeyGenerator
    {
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
