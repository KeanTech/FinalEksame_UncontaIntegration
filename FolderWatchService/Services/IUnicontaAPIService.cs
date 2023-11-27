using Alaska.Library.Models.Service;
using System;
using System.IO;
using System.Threading.Tasks;
using Uniconta.API.System;
using Uniconta.ClientTools.DataModel;
using Uniconta.Common;

namespace FolderWatchService.Services
{
    public interface IUnicontaAPIService : IDisposable
    {
        Task<ErrorCodes> Login(LoginInfo loginInfo);
        Task<ErrorCodes> HandleFolderCreatedEvent(string filePath, string fileName);
        Task<InvItemClient[]> GetInventory();
        CrudAPI Api { get; }
    }
}
