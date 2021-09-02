using System;
using System.Net;
using System.Net.Sockets;
using GameDatabase.Mongodb.Handlers;
using NetCoreServer;
using Youtube_GameOnlineServer.Applications.Interfaces;
using Youtube_GameOnlineServer.Logging;
using Youtube_GameOnlineServer.Rooms.Interfaces;

namespace Youtube_GameOnlineServer.Applications.Handlers
{
    public class WsGameServer: WsServer, IWsGameServer
    {
        private readonly int _port;
        private readonly IPlayerManager _playerManager;
        private readonly IGameLogger _logger;
        private readonly MongoDb _mongoDb;
        public readonly IRoomManager RoomManager;
        public WsGameServer(IPAddress address, int port, IPlayerManager playerManager, IGameLogger logger, MongoDb mongoDb, IRoomManager roomManager) : base(address, port)
        {
            _port = port;
            _playerManager = playerManager;
            _logger = logger;
            _mongoDb = mongoDb;
            RoomManager = roomManager;
        }

        protected override TcpSession CreateSession()
        {
            //todo handle new session
            _logger.Info("New Session connected");
            var player = new Player(this, _mongoDb.GetDatabase());
            _playerManager.AddPlayer(player);
            return player;
        }

        protected override void OnDisconnected(TcpSession session)
        {
            _logger.Info("Session disconnected");
            var player = _playerManager.FindPlayer(session.Id.ToString());
            if (player != null)
            {
                //player.SetDisconnect(true);
                _playerManager.RemovePlayer(player);
                //todo mark player disconnected
            }
            base.OnDisconnected(session);
        }

        public void SendAll(string mes)
        {
            this.MulticastText(mes);
        }

        public void StartServer()
        {
            //todo logic before start server
            if (this.Start())
            {
                _logger.Print($"Server Ws started at {_port}");
                return;
            }
            
        }

        protected override void OnError(SocketError error)
        {
            _logger.Error($"Server Ws error");
            base.OnError(error);
        }

        public void StopServer()
        {
            //todo logic before stop server
            this.Stop();
            _logger.Print("Server Ws stopped");
        }

        public void RestartServer()
        {
            //todo logic before stop server
            if (this.Restart())
            {
                _logger.Print("Server Ws restarted");
            }
        }
    }
}