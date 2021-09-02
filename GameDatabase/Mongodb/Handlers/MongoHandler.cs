using System.Collections.Generic;
using GameDatabase.Mongodb.Interfaces;
using MongoDB.Driver;

namespace GameDatabase.Mongodb.Handlers
{
    public class MongoHandler<T> : IGameDB<T> where T: class
    {
        private readonly IMongoDatabase _database;
        private IMongoCollection<T> _collection;

        public MongoHandler(IMongoDatabase database)
        {
            _database = database;
            _collection = _database.GetCollection<T>("Users");
        }

        public IMongoDatabase GetDatabase()
        {
            return _database;
        }

        public T Get(string id)
        {
           //return _collection.Find<User>(it => it.Id == id).FirstOrDefault();
           return default(T);
        }

        public List<T> GetAll()
        {
            throw new System.NotImplementedException();
        }

        public T Create(T item)
        {
            _collection.InsertOne(item);
            return item;
        }

        public bool Remove(string id)
        {
            throw new System.NotImplementedException();
        }

        public T Update(string id, T item)
        {
            throw new System.NotImplementedException();
        }
    }
}