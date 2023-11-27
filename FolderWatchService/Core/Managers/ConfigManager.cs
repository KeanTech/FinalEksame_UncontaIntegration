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
        private readonly EncryptionManager _encryptionManager;
        private readonly IFactory _factory;
        private readonly string _baseDir = AppDomain.CurrentDomain.BaseDirectory;
        private const string _configFile = nameof(FolderWatchService) + ".exe.config";
        private bool _isEnctypted = false;
        private static Dictionary<string, string> _settings { get; set; }
        public ConfigurationErrorsException LastError { get; set; }
        public ConfigManager(EncryptionManager encryptionManager, IFactory factory)
        {
            _encryptionManager = encryptionManager;
            _factory = factory;
            try
            {
                ReadConfigurations();
            }
            catch (ConfigurationErrorsException ex)
            {
                LastError = ex;
                ErrorHandler.WriteError(ex).ConfigureAwait(false);
            }
        }

        public void ReadConfigurations() 
        {
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = Path.Combine(_baseDir, _configFile)
            };

            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            var keys = config.AppSettings.Settings.AllKeys;
            EncryptionManager.SetKey(config.AppSettings.Settings["ENCKey"].Value);
                
            if (_settings == null)
                _settings = new Dictionary<string, string>();

            foreach (var key in keys)
            {
                _settings.Add(key, config.AppSettings.Settings[key].Value);
            }

            EncryptConfigFile(config);
            _isEnctypted = true;
            config.Save(ConfigurationSaveMode.Modified);
        }

        public string GetConfigFor(string key) 
        {
            if(_settings.ContainsKey(key))
                return _settings[key];

            return "";
        }

        public LoginInfo GetLoginInfo() 
        {
            var loginInfo = _factory.Create<LoginInfo>();
            loginInfo.ApiKey = GetConfigFor(ConfigKey.ApiKey.ToString());
            loginInfo.Username = GetConfigFor(ConfigKey.Username.ToString());
            loginInfo.Password = GetConfigFor(ConfigKey.Password.ToString());
            loginInfo.CompanyId = int.Parse(GetConfigFor(ConfigKey.Company.ToString()));
            return loginInfo;
        }

        public void ResetConfiguration() 
        {
            _settings = new Dictionary<string, string>();
            ReadConfigurations();
            _isEnctypted = false;
        }

        private void EncryptConfigFile(Configuration configuration) 
        {
            _encryptionManager.EncryptAppSetting(configuration.AppSettings.Settings);
        }

        private void DecryptKeyValues(Configuration configuration) 
        {
            _encryptionManager.DecryptUserSettings(configuration.AppSettings.Settings);
        }
        public void Dispose()
        {
            _settings = null;
            LastError = null;
        }
    }
}
