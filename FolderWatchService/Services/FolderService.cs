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

        /// <summary>
        /// Gets the value from the config file with key FilePath
        /// </summary>
        private string _folderPath => _configManager?.GetConfigFor(ConfigKey.FilePath.ToString());
        /// <summary>
        /// Name on the first folder that gets created by the service 
        /// </summary>
        private const string _pendingFolder = "\\PendingFiles";
        /// <summary>
        /// Name on the first folder that gets created by the service
        /// </summary>
        private const string _filesReadFolder = "\\FilesRead";
        /// <summary>
        /// File name suffix 
        /// <para>Example: myFile.txt => myFile-02-12-2023_10.55.23.32.txt</para>
        /// </summary>
        private string _fileSuffix = $"-{DateTime.Now.ToString("dd-MM-yyyy_H.mm.ss.ff")}.txt";
        /// <summary>
        /// Get the value from key name CreationOfProduction
        /// </summary>
        private bool _createProductions => _configManager?.GetConfigFor(ConfigKey.CreationOfProductions.ToString()) == "1" ? true : false;
        /// <summary>
        /// Get the value from key name ReportAsFinished
        /// </summary>
        private bool _reportAsFinished => _configManager?.GetConfigFor(ConfigKey.ReportAsFinished.ToString()) == "1" ? true : false;
        /// <summary>
        /// Get the value from key name EvnetDelay
        /// </summary>
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
        /// <summary>
        /// Entry point to the service.
        /// <para>Sets up the <see cref="FileSystemWatcher"/> and creates the needed folders</para>
        /// <para>It will try to use the usersettings to login to Uniconta</para>
        /// </summary>
        public void Start()
        {
            try
            {
                // Set up the FileSystemWatcher and subscribe to the folder event
                InitializeFileSystemWatcher();
                // Try to login with the information from the config aka Username, Password and ApiKey
                _unicontaAPIService.Login(_configManager.GetLoginInfo()).Wait();
                // Create the needed folders for the service
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

        /// <summary>
        /// Exit point from the service
        /// </summary>
        public void Stop()
        {
            _fileSystemWatcher.Dispose();
            // To make sure the service closes down
            Environment.Exit(0);
        }

        /// <summary>
        /// Creates a new FileSystemWatcher and sets up the needed subscriptions
        /// </summary>
        /// <exception cref="DirectoryNotFoundException"></exception>
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

        /// <summary>
        /// When the event triggers from the FileSystemWatcher it will call this method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == _folderPath + _pendingFolder || e.FullPath == _folderPath + _filesReadFolder)
                return;

            // Sets a delay on the thread to make sure the scanner is done writing to the file
            Thread.Sleep(_eventDelay);

            // remove the extention from the filename
            var orgFileName = e.Name.Split('.').FirstOrDefault();
            // Make new file name with the suffix appended to it
            var newFileName = orgFileName + _fileSuffix;
            // Directory to put the file in 
            var pendingPath = _folderPath + _pendingFolder + "\\" + newFileName;
            // Move the file
            File.Move(e.FullPath, pendingPath);

            /// Handle data creation in Uniconta
            _unicontaAPIService.HandleFolderCreatedEvent(pendingPath, newFileName).Wait();

            // If CreationOfProductions is set to 1 then
            if (_createProductions)
                // Creates the needed productions  
                _productionManager.HandleCreateProduction(newFileName, _reportAsFinished).Wait();

            /// Make a new timeStamp on the file name
            newFileName = $"{orgFileName ?? "stykliste"}{_fileSuffix}";
            /// combine the new file name with the path to place the file in
            var newFilePath = _folderPath + _filesReadFolder + "\\" + newFileName;
            // Move the file to the directory
            File.Move(pendingPath, newFilePath);
        }
    }
}
