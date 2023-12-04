using Alaska.Library.Core.Enums;
using Alaska.Library.Core.Factories;
using Alaska.Library.Models;
using Alaska.Library.Models.Service;
using FolderWatchService.Core.Handlers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace FolderWatchService.Core.Managers
{
    /// <summary>
    /// This class is used to access the config file
    /// </summary>
    public class ConfigManager : IConfigManager, IDisposable
    {
        private readonly IEncryptionManager _encryptionManager;
        private readonly IFactory<IEntity> _factory;
        private readonly IErrorHandler _errorHandler;
        private readonly string _baseDir = AppDomain.CurrentDomain.BaseDirectory;
        private const string _configFileName = nameof(FolderWatchService) + ".exe.config";
        private static Dictionary<string, string> _settings { get; set; }
        
        public ConfigurationErrorsException LastError { get; set; }

        public ConfigManager(IEncryptionManager encryptionManager, IFactory<IEntity> factory, IErrorHandler errorHandler)
        {
            _encryptionManager = encryptionManager;
            _factory = factory;
            _errorHandler = errorHandler;
            try
            {
                ReadConfigurations();
            }
            catch (ConfigurationErrorsException ex)
            {
                LastError = ex;
                _errorHandler.WriteError(ex).ConfigureAwait(false);
            }
        }

        public void ReadConfigurations() 
        {
            Configuration config = GetConfigFile();
            var keys = config.AppSettings.Settings.AllKeys;

            _encryptionManager.GenerateKeyAndIV();
           
            if (_settings == null)
                _settings = new Dictionary<string, string>();

            EncryptConfigFile(config);

            // inserts all key and values from the appSettings section of the App.config
            foreach (var key in keys)
            {
                /// add the key and value to the <see cref="_settings"/>
                _settings.Add(key, config.AppSettings.Settings[key].Value);
            }

            config.Save(ConfigurationSaveMode.Modified);
        }

        /// <summary>
        /// Used to get the stored configurations
        /// 
        /// <para>It will decrypt the value if needed</para>
        /// </summary>
        /// <param name="key">Key for the <see cref="KeyValuePair{TKey, TValue}"/></param>
        /// <returns></returns>
        public string GetConfigFor(string key) 
        {
            if (_settings.ContainsKey(key))
            {
                // Only Password, Username and ApiKey gets decrypted
                if (key == ConfigKey.Password.ToString() || key == ConfigKey.Username.ToString() || key == ConfigKey.ApiKey.ToString())
                { 
                    var keyValue = _settings[key];
                    var decryptedValue = _encryptionManager.DecryptString(keyValue);
                    return decryptedValue;
                }

                return _settings[key];
            }

            return "";
        }

        /// <summary>
        /// Reads the App.config file and return it as <see cref="Configuration"/>
        /// </summary>
        /// <returns></returns>
        private Configuration GetConfigFile() 
        {
            // Has to make a FileMap
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap
            {
                // Sets the path on the App.config
                ExeConfigFilename = Path.Combine(_baseDir, _configFileName)
            };

            // Load the file as Configuration
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            return config;
        }

        /// <summary>
        /// Gets the configurations for Uniconta login
        /// </summary>
        /// <returns>A <see cref="LoginInfo"/> with the needed info to login</returns>
        public LoginInfo GetLoginInfo() 
        {
            var loginInfo = _factory.Create<LoginInfo>();
            // Get the key value from the config Dictionary 
            loginInfo.ApiKey = GetConfigFor(ConfigKey.ApiKey.ToString());
            loginInfo.Username = GetConfigFor(ConfigKey.Username.ToString());
            loginInfo.Password = GetConfigFor(ConfigKey.Password.ToString());
            loginInfo.CompanyId = int.Parse(GetConfigFor(ConfigKey.Company.ToString()));
            
            return loginInfo;
        }

        private void EncryptConfigFile(Configuration configuration) 
        {
            var settings = _encryptionManager.EncryptUserSetting(configuration.AppSettings.Settings);

            if (settings == null)
                _errorHandler.WriteError(new Exception("Error while trying to encrypt value cannot be null")).ConfigureAwait(false);
        }

        private void DecryptConfigFile(Configuration configuration) 
        {
            var settings = _encryptionManager.DecryptUserSettings(configuration.AppSettings.Settings);

            if (settings == null)
                _errorHandler.WriteError(new Exception("Error while trying to decrypt value cannot be null")).ConfigureAwait(false);
        }
        public void Dispose()
        {
            _encryptionManager.Dispose();
            _settings = null;
            LastError = null;
        }
    }
}
