using System;
using System.Threading.Tasks;
using Uniconta.Common;

namespace FolderWatchService.Core.Handlers
{
    public interface IErrorHandler
    {
        void ShowErrorMessage(string message);
        Task WriteError(Exception exception, ErrorCodes errorCode = ErrorCodes.Succes);
    }
}