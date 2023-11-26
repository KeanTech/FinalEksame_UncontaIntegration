using System.Collections.Generic;

namespace Alaska.Library.Core.Factories
{
    // This interface has the methods needed for creating objects used in this project
    // The object/class needs to implement IEntity and have an empty constructor
    public interface IFactory
    {
        /// <summary>
        /// Uses generic types to make it open for any object that implements <see cref="IEntity"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A new instance of the type parsed in generic parameter</returns>
        T Create<T>() where T : IEntity, new();

        /// <summary>
        /// Uses generic types to make it open for any object that implements <see cref="IEntity"/>
        /// It also needs to have an empty constructor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A new instance of <see cref="List{T}"/> parsed in generic parameter</returns>
        List<T> CreateListOf<T>() where T : IEntity, new();
    }
}
