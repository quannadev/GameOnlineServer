using System;
using System.Text;
using GameDatabase.Mongodb.Handlers;
using GameDatabase.Mongodb.Interfaces;
using MongoDB.Driver;
using NetCoreServer;
using Youtube_GameOnlineServer.Applications.Interfaces;
using Youtube_GameOnlineServer.Applications.Messaging;
using Youtube_GameOnlineServer.Applications.Messaging.Constants;
using Youtube_GameOnlineServer.GameModels;
using Youtube_GameOnlineServer.Logging;

namespace Youtube_GameOnlineServer.Applications.Handlers
{
    public class Player : WsSession, IPlayer
    {
        public string SessionId { get; set; }
        public string Name { get; set; }
        private bool IsDisconnected { get; set; }
        private readonly IGameLogger _logger;
        private IGameDB<User> UsersDb { get; set; }
        public Player(WsServer server, IMongoDatabase database) : base(server)
        {
            SessionId = this.Id.ToString();
            IsDisconnected = false;
            _logger = new GameLogger();
            UsersDb = new MongoHandler<User>(database);
        }

        public override void OnWsConnected(HttpRequest request)
        {
            //todo login on player connected
            var url = request.Url;
            _logger.Info("Player connected");
            IsDisconnected = false;
        }

        public override void OnWsDisconnected()
        {
            OnDisconnect();
            base.OnWsDisconnected();
        }


        public override void OnWsReceived(byte[] buffer, long offset, long size)
        {
            var mess = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);
            try
            {
                var wsMess = GameHelper.ParseStruct<WsMessage<object>>(mess);
                switch (wsMess.Tags)
                {
                    case WsTags.Invalid:
                        break;
                    case WsTags.Login:
                        var loginData = GameHelper.ParseStruct<LoginData>(wsMess.Data.ToString());
                        var user = new User("codephui", "abcg@123", "Admin");
                        var newUser = UsersDb.Create(user);
                        break;
                    case WsTags.Register:
                        break;
                    case WsTags.Lobby:
                        break;
                    default:
                        break;
                        //throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                _logger.Error("OnWsReceived error", e);
                //todo send invalid message 
            }
            //((WsGameServer) Server).SendAll($"{this.SessionId} send message {mess}");
        }

        public void SetDisconnect(bool value)
        {
            this.IsDisconnected = value;
        }

        public bool SendMessage(string mes)
        {
           return this.SendTextAsync(mes);
        }

        public void OnDisconnect()
        {
           //todo logic handle player disconnect
           _logger.Warning("Player disconnected", null);
           //((WsGameServer) Server).PlayerManager.RemovePlayer(this);
        }
    }
}