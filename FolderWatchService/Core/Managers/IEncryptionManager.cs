using System.Configuration;

namespace FolderWatchService.Core.Managers
{
    public interface IEncryptionManager
    {
        string DecryptString(string input);
        KeyValueConfigurationCollection DecryptUserSettings(KeyValueConfigurationCollection settingsCollection);
        string EncryptString(string input);
        KeyValueConfigurationCollection EncryptUserSetting(KeyValueConfigurationCollection settingsCollection);
        void GenerateKeyAndIV();
    }
}