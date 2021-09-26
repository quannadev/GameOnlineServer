using System.Collections.Generic;
using GameDatabase.Mongodb.Interfaces;
using MongoDB.Driver;

namespace GameDatabase.Mongodb.Handlers
{
    public class MongoHandler<T> : IGameDb<T> where T : class
    {
        private readonly IMongoDatabase _database;
        private IMongoCollection<T> Collection { get; set; }
        public MongoHandler(IMongoDatabase database)
        {
            _database = database;
            this.SetCollection();
        }

        private void SetCollection()
        {
            switch (typeof(T).Name)
            {
                case "User":
                    Collection = _database.GetCollection<T>("Users");
                    break;
                case "RoomModel":
                    Collection = _database.GetCollection<T>("Match");
                    break;
            }
        }

        public IMongoDatabase GetDatabase()
        {
            return _database;
        }

        public IMongoCollection<T> GetCollection()
        {
            return Collection;
        }

        public T Get(FilterDefinition<T> filter)
        {
            return Collection.Find(filter).FirstOrDefault();
        }

        public List<T> GetAll()
        {
            var filter = Builders<T>.Filter.Empty;
            return Collection.Find(filter).ToList();
        }

        public T Create(T item)
        {
            Collection.InsertOne(item);
            return item;
        }

        public void Remove(FilterDefinition<T> filter)
        {
            Collection.DeleteOne(filter);
        }

        public T Update(FilterDefinition<T> filter, UpdateDefinition<T> updater)
        {
            Collection.UpdateOne(filter, updater);
            return Get(filter);
        }
    }
}