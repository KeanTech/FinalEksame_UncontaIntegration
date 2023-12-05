using System;
using System.Threading.Tasks;
using Uniconta.Common;

namespace FolderWatchService.Core.Handlers
{
    public interface IErrorHandler
    {
        /// <summary>
        /// Shows a dialog to end user
        /// </summary>
        /// <param name="message"></param>
        void ShowErrorMessage(string message);
        /// <summary>
        /// Writes the exception to a log
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        Task WriteError(Exception exception, ErrorCodes errorCode = ErrorCodes.Succes);
    }
}