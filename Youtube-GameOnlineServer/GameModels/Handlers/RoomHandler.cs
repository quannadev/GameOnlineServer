using System.Collections.Generic;
using GameDatabase.Mongodb.Handlers;
using GameDatabase.Mongodb.Interfaces;
using MongoDB.Driver;
using Youtube_GameOnlineServer.GameModels.Interfaces;

namespace Youtube_GameOnlineServer.GameModels.Handlers
{
    public class RoomHandler : IDbHandler<RoomModel>
    {
        private readonly IGameDb<RoomModel> _roomDb;

        public RoomHandler(IMongoDatabase database)
        {
            _roomDb = new MongoHandler<RoomModel>(database);
        }

        public RoomModel Find(string id)
        {
            throw new System.NotImplementedException();
        }

        public List<RoomModel> FindAll()
        {
            throw new System.NotImplementedException();
        }

        public RoomModel Create(RoomModel item)
        {
            throw new System.NotImplementedException();
        }

        public RoomModel Update(string id, RoomModel item)
        {
            throw new System.NotImplementedException();
        }

        public void Remove(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}