using Alaska.Library.Core.Enums;
using FolderWatchService.Core.Generators;
using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;

namespace FolderWatchService.Core.Managers
{
    public class EncryptionManager : IDisposable
    {
        private readonly string[] _keys = { ConfigKey.ApiKey.ToString(), ConfigKey.Username.ToString(), ConfigKey.Password.ToString() };
        private readonly KeyGenerator _keyGenerator;
        private const int _keySize = 32;
        private const int _ivSize = 16;
        private byte[] _key;
        private byte[] _iv;

        public EncryptionManager(KeyGenerator keyGenerator)
        {
            _keyGenerator = keyGenerator;
        }

        private void GenerateRandomKey() => _key = _keyGenerator.GenerateRandomByteArray(_keySize);
        private void GenerateRandomIV() => _iv = _keyGenerator.GenerateRandomByteArray(_ivSize);
       
        /// <summary>
        /// Generates a internal key and iv used in the <see cref="EncryptionManager"/>
        /// </summary>
        public void GenerateKeyAndIV()
        {
            GenerateRandomKey();
            GenerateRandomIV(); 
        }

        /// <summary>
        /// It will encrypt the content used to login
        /// </summary>
        /// <param name="settingsCollection"></param>
        /// <returns>A <see cref="KeyValueConfigurationCollection"/> with the encrypted values</returns>
        public KeyValueConfigurationCollection EncryptAppSetting(KeyValueConfigurationCollection settingsCollection)
        {
            // Return null because theres nothing to encrypt
            if (settingsCollection == null)
                return null;

            // Return null because it cant run the encryption without the key
            if (_key == null)
                return null;

            // Return null because it cant run the encryption without the iv
            if (_iv == null)
                return null;

            foreach (string key in _keys)
            {
                // Get the specific setting by key
                string settingValue = settingsCollection[key]?.Value;

                if (settingValue == null)
                    continue;

                // Encrypt the setting value using the fixed key and IV
                string encryptedValue = EncryptString(settingValue);

                // Update the encrypted value in the configuration file
                settingsCollection[key].Value = encryptedValue;

            }

            return settingsCollection;
        }

        /// <summary>
        /// Decrypt content for login
        /// </summary>
        /// <param name="settingsCollection"></param>
        /// <returns>A <see cref="KeyValueConfigurationCollection"/> with the decrypted values</returns>
        public KeyValueConfigurationCollection DecryptUserSettings(KeyValueConfigurationCollection settingsCollection)
        {
            // Return null because theres nothing to decrypt
            if (settingsCollection == null)
                return null;

            // Return null because it cant run the decryption without the key
            if (_key == null)
                return null;

            // Return null because it cant run the decryption without the iv
            if (_iv == null)
                return null;

            foreach (var key in _keys)
            {
                // Get the encrypted setting value
                string encryptedValue = settingsCollection[key]?.Value;

                if (encryptedValue == null)
                    continue;

                // Decrypt the setting value using the fixed key and IV
                string decryptedValue = DecryptString(encryptedValue);
                // Update the decrypted value in the configuration file
                settingsCollection[key].Value = encryptedValue;
            }

            return settingsCollection;
        }

        /// <summary>
        /// Uses the Aes managed algorithm to encrypt the input string
        /// 
        /// <para>The key and iv has to be set before this can run</para>
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The encrypted string</returns>
        public string EncryptString(string input)
        {
            // adds a using to make sure that it gets disposed after 
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = _key;
                aesAlg.IV = _iv;

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

        /// <summary>
        /// Uses the Aes managed algorithm to decrypt the input string
        /// 
        /// <para>The key and iv has to be set before this can run</para>
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The decrypted string</returns>
        public string DecryptString(string input)
        {
            // adds a using to make sure that it gets disposed after
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = _key;
                aesAlg.IV = _iv;

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

        public void Dispose()
        {
            _key = null;
            _iv = null;
        }
    }
}
