using Alaska.Library.Core.Enums;
using FolderWatchService.Core.Handlers;
using FolderWatchService.Core.Helpers;
using FolderWatchService.Core.Managers;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace FolderWatchService.Services
{
    public class FolderService
    {
        private FileSystemWatcher _fileSystemWatcher;
        private string _folderPath => _configManager?.GetConfigFor(ConfigKey.FilePath.ToString());
        private const string _pendingFolder = "\\PendingFiles";
        private const string _filesReadFolder = "\\FilesRead";
        private string _fileSuffix = $"-{DateTime.Now.ToString("dd-MM-yyyy_H.mm.ss.ff")}.txt";
        private bool _createProductions => _configManager?.GetConfigFor(ConfigKey.CreationOfProductions.ToString()) == "1" ? true : false;
        private bool _reportAsFinished => _configManager?.GetConfigFor(ConfigKey.ReportAsFinished.ToString()) == "1" ? true : false;
        private int _eventDelay => int.Parse(_configManager?.GetConfigFor(ConfigKey.EventDelay.ToString()));
        private readonly IConfigManager _configManager;
        private readonly IUnicontaAPIService _unicontaAPIService;
        private readonly IErrorHandler _errorHandler;
        private readonly IProductionManager _productionManager;

        public FolderService(IProductionManager productionManager, IConfigManager configManager, IUnicontaAPIService unicontaAPIService, IErrorHandler errorHandler)
        {
            _productionManager = productionManager;
            _configManager = configManager;
            _unicontaAPIService = unicontaAPIService;
            _errorHandler = errorHandler;
        }

        public void Start()
        {
            try
            {
                InitializeFileSystemWatcher();
                _unicontaAPIService.Login(_configManager.GetLoginInfo()).Wait();
                FileAndPathHelper.CreateNeededFolders(_folderPath, _folderPath + _pendingFolder, _folderPath + _filesReadFolder);
            }
            catch (Exception ex)
            {
                _errorHandler.ShowErrorMessage(ex.Message);
                // Spins up a background thread when theres one available
                // Writes to error.txt 
                _errorHandler.WriteError(ex).ConfigureAwait(false);
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
                _errorHandler.ShowErrorMessage("Error while initializing FileSystemWatcher no path");
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
            if (e.FullPath == _folderPath + _pendingFolder || e.FullPath == _folderPath + _filesReadFolder)
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
                _productionManager.HandleCreateProduction(newFileName, _reportAsFinished).Wait();

            newFileName = $"{orgFileName ?? "stykliste"}{_fileSuffix}";
            var newFilePath = _folderPath + _filesReadFolder + "\\" + newFileName;

            File.Move(pendingPath, newFilePath);
        }
    }
}
