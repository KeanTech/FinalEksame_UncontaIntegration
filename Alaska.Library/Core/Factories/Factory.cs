using System;
using System.Collections.Generic;
using Uniconta.Common;
using Uniconta.DataModel;

namespace Alaska.Library.Core.Factories
{
    public class Factory : IFactory
    {
        public T Create<T>() where T : IEntity, new()
        {
            return new T();
        }

        public List<T> CreateListOf<T>() where T : IEntity, new() 
        {
            return new List<T>();
        }
    }
}
