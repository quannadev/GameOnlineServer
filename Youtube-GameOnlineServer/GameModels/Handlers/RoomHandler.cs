using System;
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
            var filter = Builders<RoomModel>.Filter.Eq(i => i.Id, id);
            return _roomDb.Get(filter);
        }

        public RoomModel FindByRoomId(string rId)
        {
            var filter = Builders<RoomModel>.Filter.Eq(i => i.RoomId, rId);
            return _roomDb.Get(filter);
        }

        public List<RoomModel> FindAll()
        {
            return _roomDb.GetAll();
        }

        public RoomModel Create(RoomModel item)
        {
            var room = _roomDb.Create(item);
            return room;
        }

        public RoomModel Update(string id, RoomModel item)
        {
            var filter = Builders<RoomModel>.Filter.Eq(i => i.Id, id);
            var updater = Builders<RoomModel>.Update
                .Set(i => i.Players, item.Players)
                .Set(i => i.Match, item.Match)
                .Set(i => i.UpdateAt, DateTime.Now);
            _roomDb.Update(filter, updater);
            return item;
        }

        public void Remove(string id)
        {
            var filter = Builders<RoomModel>.Filter.Eq(i => i.Id, id);
            _roomDb.Remove(filter);
        }
    }
}