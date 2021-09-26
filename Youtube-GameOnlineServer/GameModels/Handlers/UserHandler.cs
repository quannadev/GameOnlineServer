using System;
using System.Collections.Generic;
using GameDatabase.Mongodb.Handlers;
using GameDatabase.Mongodb.Interfaces;
using MongoDB.Driver;
using Youtube_GameOnlineServer.GameModels.Interfaces;

namespace Youtube_GameOnlineServer.GameModels.Handlers
{
    public class UserHandler : IDbHandler<User>
    {
        private readonly IGameDb<User> _userDb;

        public UserHandler(IMongoDatabase database)
        {
            _userDb = new MongoHandler<User>(database);
        }

        public User Find(string id)
        {
            var filter = Builders<User>.Filter.Eq(i => i.Id, id);
            return _userDb.Get(filter);
        }

        public List<User> FindAll()
        {
            return _userDb.GetAll();
        }

        public List<User> GetTop(int limit)
        {
            var filter = Builders<User>.Filter
                .Gt(i => i.Point, 0);
            var list = _userDb.GetCollection()
                .Find(filter)
                .Limit(limit)
                .SortByDescending(u => u.Point);
            return list.ToList();
        }

        public User FindByUserName(string username)
        {
            var filter = Builders<User>.Filter.Eq(i => i.Username, username);
            return _userDb.Get(filter);
        }

        public User Create(User item)
        {
            var user = _userDb.Create(item);
            return user;
        }

        public User Update(string id, User item)
        {
            var filter = Builders<User>.Filter.Eq(i => i.Id, id);
            var updater = Builders<User>.Update
                .Set(i => i.Password, item.Password)
                .Set(i => i.DisplayName, item.DisplayName)
                .Set(i => i.Amount, item.Amount)
                .Set(i => i.Level, item.Level)
                .Set(i => i.Avatar, item.Avatar)
                .Set(i => i.Point, item.Point)
                .Set(i => i.UpdateAt, DateTime.Now);
            _userDb.Update(filter, updater);
            return item;
        }

        public void Remove(string id)
        {
            var filter = Builders<User>.Filter.Eq(i => i.Id, id);
            _userDb.Remove(filter);
        }
    }
}