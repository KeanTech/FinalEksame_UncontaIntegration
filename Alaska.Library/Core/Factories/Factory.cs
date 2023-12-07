using Alaska.Library.Models.Uniconta.Userdefined;
using System;
using System.Collections.Generic;
using Uniconta.API.Inventory;
using Uniconta.API.Service;
using Uniconta.API.System;
using Uniconta.ClientTools.DataModel;
using Uniconta.Common;
using Uniconta.DataModel;

namespace Alaska.Library.Core.Factories
{
    public class Factory : IFactory<IEntity>
    {
        public T Create<T>() where T : IEntity, new()
        {
            return new T();
        }

        public List<T> CreateListOf<T>() where T : IEntity, new() 
        {
            return new List<T>();
        }

        public ProductionAPI CreateProductionApi(BaseAPI api)
        {
            return new ProductionAPI(api);
        }
        public T CreateUnicontaObject<T>() where T : UnicontaBaseEntity, new()
        {
            return new T();
        } 
    }
}