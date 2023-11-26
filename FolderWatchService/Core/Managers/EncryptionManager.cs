using Alaska.Library.Models;
using FromXSDFile.OIOUBL.ExportImport;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FolderWatchService.Core.Managers
{
    public class EncryptionManager
    {
        private readonly string[] _keys = { "ApiKey", "UserName", "Password", "ENCKey" };
        private static string _key;

        /// <summary>
        /// If the key has not been set it will be 
        /// </summary>
        /// <param name="yourKey"></param>
        /// <returns></returns>
        public static void SetKey(string yourKey) => _key = yourKey;

        /// <summary>
        /// It will encrypt the content used to login
        /// </summary>
        /// <param name="settingsCollection"></param>
        /// <returns></returns>
        public KeyValueConfigurationCollection EncryptAppSetting(KeyValueConfigurationCollection settingsCollection)
        {
            if (settingsCollection != null)
            {
                foreach (string key in _keys)
                {
                    // Get the specific setting by key
                    string settingValue = settingsCollection[key]?.Value;

                    if (!string.IsNullOrEmpty(settingValue))
                    {
                        // Use fixed key and IV for testing (replace with your own values)
                        byte[] fixedKey = Encoding.UTF8.GetBytes(_key);
                        byte[] fixedIV = Encoding.UTF8.GetBytes(_key);

                        // Encrypt the setting value using the fixed key and IV
                        string encryptedValue = EncryptString(settingValue, fixedKey, fixedIV);

                        // Update the encrypted value in the configuration file
                        settingsCollection[key].Value = encryptedValue;
                    }
                }
            }

            return settingsCollection;
        }

        /// <summary>
        /// Decrypt content for login
        /// </summary>
        /// <param name="settingsCollection"></param>
        /// <returns>A <see cref="string[]"/> with the decrypted values</returns>
        public KeyValueConfigurationCollection DecryptUserSettings(KeyValueConfigurationCollection settingsCollection)
        {
            if (settingsCollection == null)
                return null;

            foreach (var key in _keys)
            {
                // Get the encrypted setting value
                string encryptedValue = settingsCollection[key]?.Value;

                if (encryptedValue == null)
                    continue;
                
                // Use fixed key and IV for testing (replace with your own values)
                byte[] fixedKey = Encoding.UTF8.GetBytes(key);
                byte[] fixedIV = Encoding.UTF8.GetBytes(key);

                // Decrypt the setting value using the fixed key and IV
                string decryptedValue = DecryptString(encryptedValue, fixedKey, fixedIV);
                // Update the decrypted value in the configuration file
                settingsCollection[key].Value = encryptedValue;
            }

            return settingsCollection;
        }

        private string EncryptString(string input, byte[] key, byte[] iv)
        {
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(input);
                        }
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        private string DecryptString(string input, byte[] key, byte[] iv)
        {
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(input)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
