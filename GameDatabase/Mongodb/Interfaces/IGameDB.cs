using System.Collections.Generic;
using MongoDB.Driver;

namespace GameDatabase.Mongodb.Interfaces
{
    public interface IGameDB<T> where T : class
    {
        IMongoDatabase GetDatabase();
        T Get(string id);
        List<T> GetAll();
        T Create(T item);
        bool Remove(string id);
        T Update(string id, T item);
    }
}