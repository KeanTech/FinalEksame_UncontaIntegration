using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Uniconta.Common;

namespace FolderWatchService.Core.Handlers
{
    /// <summary>
    /// This class is handles errors by writing the message to the user
    /// </summary>
    public class ErrorHandler : IErrorHandler
    {
        private readonly string _path = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Creates a file called error.txt containing the information about the Exception
        /// </summary>
        /// <param name="path"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public async Task WriteError(Exception exception, ErrorCodes errorCode = ErrorCodes.Succes)
        {
            string fullPath = Path.Combine(_path, $"Errors\\{DateTime.Now.ToString("dd-MM-yy")}_error.txt");

            if (Directory.Exists(_path + "Errors") == false)
                Directory.CreateDirectory(_path + "Errors");

            // Makes a using here to make sure that the stream gets disposed correctly after writing to the disk
            using (StreamWriter writer = new StreamWriter(fullPath, true))
            {
                await writer.WriteLineAsync($"Error: at {DateTime.Now}");
                await writer.WriteLineAsync($"Errormessage: {exception.Message}");
                await writer.WriteLineAsync($"Inner exception: {exception.InnerException}");
                if (errorCode != ErrorCodes.Succes)
                    await writer.WriteLineAsync($"Api error: {errorCode}");

                await writer.WriteLineAsync($"-------------Record Done-------------");
            }
        }
        
        /// <summary>
        /// Used to show a error message box to the user 
        /// </summary>
        /// <param name="message"></param>
        public void ShowErrorMessage(string message)
        {
            MessageBox.Show("Error occurred in FolderService:\n" + message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
