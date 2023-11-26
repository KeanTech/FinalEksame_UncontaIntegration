using FolderWatchService.Core.Handlers;
using FolderWatchService.Core.Managers;
using System.IO;
using System.Threading.Tasks;

namespace FolderWatchService.Services
{
    public class FolderService
    {
        private FileSystemWatcher _fileSystemWatcher;
        private string _folderPath;
        private ErrorHandler _errorHandler;
        private readonly IConfigManager _configManager;

        public FolderService(ErrorHandler logFileManager, IConfigManager configManager)
        {
            _errorHandler = logFileManager;
            _configManager = configManager;
        }

        public void Start() 
        {
            InitializeFileSystemWatcher();
            _configManager.ReadConfigurations();
            _configManager.GetConfigFor("FilePath");
        }

        public void Stop()
        {
            _fileSystemWatcher.Dispose();
        }

        private void InitializeFileSystemWatcher()
        {
            _fileSystemWatcher = new FileSystemWatcher();
            if (string.IsNullOrEmpty(_folderPath))
                _fileSystemWatcher.Path = @"C:\Users\KennethAndersen\Desktop\Scannerfiler";
            else
                _fileSystemWatcher.Path = _folderPath;

            // Subscribe to the events you are interested in
            _fileSystemWatcher.Created += OnFileCreated;
            _fileSystemWatcher.Changed += OnFileChanged;

            // Enable the watcher
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            // Handle file creation event
            _errorHandler.HandleFolderEvent(sender, e).ConfigureAwait(false);
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Handle file change event
            _errorHandler.HandleFolderEvent(sender, e).ConfigureAwait(false);
        }
    }
}
