using Alaska.Library.Models.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Uniconta.API.Inventory;
using Uniconta.API.System;
using Uniconta.ClientTools.DataModel;
using Uniconta.Common;
using Uniconta.DataModel;

namespace FolderWatchService.Services
{
    public interface IUnicontaAPIService : IDisposable
    {
        Company Company { get; }
        ProductionAPI CreateProductionApi();
        Task<ErrorCodes> Login(LoginInfo loginInfo);
        Task<T[]> Query<T>(List<PropValuePair> filters) where T : class, UnicontaBaseEntity, new();
        Task<T[]> Query<T>(UnicontaBaseEntity master) where T : class, UnicontaBaseEntity, new();
        Task<T[]> Query<T>() where T : class, UnicontaBaseEntity, new();
        Task<ErrorCodes> Insert(UnicontaBaseEntity entity);
        Task<ErrorCodes> Insert(IEnumerable<UnicontaBaseEntity> entities);
        Task<ErrorCodes> Update(UnicontaBaseEntity entity);
        Task<ErrorCodes> Update(IEnumerable<UnicontaBaseEntity> entities);
        Task<ErrorCodes> HandleFolderCreatedEvent(string filePath, string fileName);
        Task<InvItemClient[]> GetInventory();
    }
}
