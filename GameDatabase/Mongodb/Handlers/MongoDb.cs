using MongoDB.Driver;

namespace GameDatabase.Mongodb.Handlers
{
    public class MongoDb
    {
        private readonly IMongoClient _client;
        private IMongoDatabase Database => _client.GetDatabase("GameOnline");
        public MongoDb()
        {
            var setting = MongoClientSettings.FromConnectionString("mongodb://localhost:27017/");
            _client = new MongoClient(setting);
        }

        public IMongoDatabase GetDatabase()
        {
            return Database;
        }
        
    }
}