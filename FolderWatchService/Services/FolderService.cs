using Alaska.Library.Core.Enums;
using FolderWatchService.Core.Handlers;
using FolderWatchService.Core.Helpers;
using FolderWatchService.Core.Managers;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Uniconta.API.System;
using static System.Net.Mime.MediaTypeNames;

namespace FolderWatchService.Services
{
    public class FolderService
    {
        private FileSystemWatcher _fileSystemWatcher;
        private const string _pendingFolder = "\\PendingFiles";
        private const string _FilesReadFolder = "\\FilesRead";
        private string _fileSuffix = $"-{DateTime.Now.ToString("dd-MM-yyyy_H.mm.ss.ff")}.txt";
        private string _folderPath => _configManager?.GetConfigFor(ConfigKey.FilePath.ToString());
        private bool _createProductions => _configManager?.GetConfigFor(ConfigKey.CreationOfProductions.ToString()) == "1" ? true : false;
        private int _eventDelay => int.Parse(_configManager?.GetConfigFor(ConfigKey.EventDelay.ToString()));
        private readonly IConfigManager _configManager;
        private readonly IUnicontaAPIService _unicontaAPIService;
        private readonly ProductionManager _productionManager;

        public FolderService(ProductionManager productionManager, IConfigManager configManager, IUnicontaAPIService unicontaAPIService)
        {
            _productionManager = productionManager;
            _configManager = configManager;
            _unicontaAPIService = unicontaAPIService;
        }

        public void Start()
        {
            try
            {
                InitializeFileSystemWatcher();
                _unicontaAPIService.Login(_configManager.GetLoginInfo()).Wait();
                FileAndPathHelper.CreateNeededFolders(_folderPath + _pendingFolder, _folderPath + _FilesReadFolder);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowErrorMessage(ex.Message);
                // Spins up a background thread when theres one available
                // Writes to error.txt 
                ErrorHandler.WriteError(ex).ConfigureAwait(false);
            }
        }

        public void Stop()
        {
            _fileSystemWatcher.Dispose();
        }

        private void InitializeFileSystemWatcher()
        {
            _fileSystemWatcher = new FileSystemWatcher();
            if (string.IsNullOrEmpty(_folderPath))
            {
                ErrorHandler.ShowErrorMessage("Error while initializing FileSystemWatcher no path");
                throw new DirectoryNotFoundException("No path found", new FileNotFoundException("Could not find any value with key FilePath\nIn FolderWatchService.exe.config"));
            }

            _fileSystemWatcher.Path = _folderPath;

            // Subscribe to the events you are interested in
            _fileSystemWatcher.Created += OnFileCreated;
            
            // Enable the watcher
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == _folderPath + _pendingFolder || e.FullPath == _folderPath + _FilesReadFolder)
                return;

            FileSystemWatcher watcher = (FileSystemWatcher)sender;
            // Sets a delay on the thread to make sure the scanner is done writing to the file
            Thread.Sleep(_eventDelay);

            var orgFileName = e.Name.Split('.').FirstOrDefault();
            var newFileName = orgFileName + _fileSuffix;
            var pendingPath = _folderPath + _pendingFolder + "\\" + newFileName;
            File.Move(e.FullPath, pendingPath);

            _unicontaAPIService.HandleFolderCreatedEvent(pendingPath, newFileName).Wait();

            if (_createProductions)
                _productionManager.HandleCreateProduction(_unicontaAPIService.Api, newFileName).Wait();

            newFileName = $"{orgFileName ?? "stykliste"}{_fileSuffix}";
            var newFilePath = _folderPath + _FilesReadFolder + "\\" + newFileName;

            File.Move(pendingPath, newFilePath);
        }
    }
}
