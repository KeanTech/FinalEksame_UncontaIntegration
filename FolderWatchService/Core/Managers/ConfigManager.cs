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
    public class ConfigManager : IConfigManager
    {
        private readonly IEncryptionManager _encryptionManager;
        private readonly IFactory<IEntity> _factory;
        private readonly IErrorHandler _errorHandler;
        /// <summary>
        /// Gets the apps base directory.
        /// </summary>
        private readonly string _baseDir = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// Using a const here because the config file never changes unless the project changes
        /// and thats why theres a <see cref="nameof(FolderWatchService)"/> then i will throw a compiler error.
        /// </summary>
        private const string _configFileName = nameof(FolderWatchService) + ".exe.config";
        
        /// <summary>
        /// Used to store the settings from the config file user settings will be encrypted
        /// </summary>
        private static Dictionary<string, string> _settings { get; set; }
        
        /// <summary>
        /// This property is used to see what error the ConfigManager has encountered last
        /// </summary>
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

        /// <summary>
        /// Used to read the config file into memory by storing it in a dictionary
        /// </summary>
        public void ReadConfigurations() 
        {
            // Read the config file as a Configuration
            Configuration config = GetConfigFile();
            // Get the keys from the appSettings section 
            var keys = config.AppSettings.Settings.AllKeys;

            // Generate Key and IV in the encryptionManager
            _encryptionManager.GenerateKeyAndIV();
            
            if (_settings == null)
                _settings = new Dictionary<string, string>();

            // Encrypt the user parts of the config file
            EncryptConfigFile(config);

            // inserts all key and values from the appSettings section of the App.config
            foreach (var key in keys)
            {
                /// add the key and value to the <see cref="_settings"/>
                _settings.Add(key, config.AppSettings.Settings[key].Value);
            }

            // Save the changes to the config file
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

        /// <summary>
        /// Used to encrypt the usersettings in the config file such as Username, Password and APIkey
        /// </summary>
        /// <param name="configuration"></param>
        private void EncryptConfigFile(Configuration configuration) 
        {
            // Call the encryptionManager with the configuration 
            // Gets a key value pair back with encrypted values
            var settings = _encryptionManager.EncryptUserSetting(configuration.AppSettings.Settings);
            
            // if the settings variable is null somthing went wrong 
            if (settings == null)
                // Use configureAwait here because the service don't have to wait for the ErrorHandler
                _errorHandler.WriteError(new Exception("Error while trying to encrypt value cannot be null")).ConfigureAwait(false);
        }

        /// <summary>
        /// Used to decrypt the usersettings in the config file such as Username, Password and APIkey
        /// </summary>
        /// <param name="configuration"></param>
        private void DecryptConfigFile(Configuration configuration) 
        {
            var settings = _encryptionManager.DecryptUserSettings(configuration.AppSettings.Settings);

            if (settings == null)
                _errorHandler.WriteError(new Exception("Error while trying to decrypt value cannot be null")).ConfigureAwait(false);
        }

        /// <summary>
        /// Is used to make sure that data in the ConfigManager gets removed 
        /// </summary>
        public void Dispose()
        {
            _encryptionManager.Dispose();
            _settings = null;
            LastError = null;
        }
    }
}
