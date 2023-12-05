using System;
using System.Configuration;

namespace FolderWatchService.Core.Managers
{
    public interface IEncryptionManager : IDisposable
    {
        string DecryptString(string input);
        KeyValueConfigurationCollection DecryptUserSettings(KeyValueConfigurationCollection settingsCollection);
        string EncryptString(string input);
        KeyValueConfigurationCollection EncryptUserSetting(KeyValueConfigurationCollection settingsCollection);
        void GenerateKeyAndIV();
    }
}