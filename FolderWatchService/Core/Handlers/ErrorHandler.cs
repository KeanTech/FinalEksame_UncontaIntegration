using Alaska.Library.Core.Factories;
using Alaska.Library.Models;
using Alaska.Library.Models.Uniconta.Userdefined;
using FolderWatchService.Services;
using Renci.SshNet.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Uniconta.ClientTools.DataModel;
using Uniconta.Common;

namespace FolderWatchService.Core.Handlers
{
    public class ErrorHandler
    {
        private static string _path;
        public static void SetPathForErrorLog(string path) => _path = path;

        /// <summary>
        /// Creates a file called error.txt containing the information about the Exception
        /// </summary>
        /// <param name="path"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static async Task WriteError(Exception exception, ErrorCodes errorCode = ErrorCodes.Succes)
        {
            var logFileName = "error.txt";

            // Makes a using here to make sure that the stream gets disposed correctly after writing to the disk
            using (StreamWriter writer = new StreamWriter(_path + logFileName, true))
            {
                await writer.WriteLineAsync($"Error: at {DateTime.Now}");
                await writer.WriteLineAsync($"Errormessage: {exception.Message}");
                await writer.WriteLineAsync($"Inner exception: {exception.InnerException}");
                if(errorCode != ErrorCodes.Succes)
                    await writer.WriteLineAsync($"Api error: {errorCode}");

                await writer.WriteLineAsync($"-------------Record Done-------------");
            }
        }
    }
}
