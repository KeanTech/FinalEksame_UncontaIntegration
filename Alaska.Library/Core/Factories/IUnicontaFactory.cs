using System.Collections.Generic;
using Uniconta.API.Inventory;
using Uniconta.API.Service;
using Uniconta.Common;

namespace Alaska.Library.Core.Factories
{
    public interface IUnicontaFactory : IFactory<UnicontaBaseEntity>
    {
        new T Create<T>() where T : UnicontaBaseEntity, new();
        new List<T> CreateListOf<T>() where T : UnicontaBaseEntity, new();
        ProductionAPI CreateProductionApi(BaseAPI api);
        UnicontaBaseEntity CreateUnicontaObjectWithMaster(UnicontaBaseEntity master);
    }
}