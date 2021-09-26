using System.Collections.Generic;
using MongoDB.Driver;

namespace GameDatabase.Mongodb.Interfaces
{
    public interface IGameDb<T> where T : class
    {
        IMongoDatabase GetDatabase();
        IMongoCollection<T> GetCollection();
        T Get(FilterDefinition<T> filter);
        List<T> GetAll();
        T Create(T item);
        void Remove(FilterDefinition<T> filter);
        T Update(FilterDefinition<T> filter, UpdateDefinition<T> updater);
    }
}