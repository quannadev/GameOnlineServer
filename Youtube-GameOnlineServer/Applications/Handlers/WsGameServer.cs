using System;
using System.Net;
using System.Net.Sockets;
using NetCoreServer;
using Youtube_GameOnlineServer.Applications.Interfaces;

namespace Youtube_GameOnlineServer.Applications.Handlers
{
    public class WsGameServer: WsServer, IWsGameServer
    {
        private readonly int _port;
        public readonly IPlayerManager PlayerManager;
        public WsGameServer(IPAddress address, int port, IPlayerManager playerManager) : base(address, port)
        {
            _port = port;
            PlayerManager = playerManager;
        }

        protected override TcpSession CreateSession()
        {
            //todo handle new session
            Console.WriteLine("New Session connected");
            var player = new Player(this);
            PlayerManager.AddPlayer(player);
            return player;
        }

        protected override void OnDisconnected(TcpSession session)
        {
            Console.WriteLine("Session disconnected");
            var player = PlayerManager.FindPlayer(session.Id.ToString());
            if (player != null)
            {
                //player.SetDisconnect(true);
                PlayerManager.RemovePlayer(player);
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
                Console.WriteLine($"Server Ws started at {_port}");
                return;
            }
            
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Server Ws error");
            base.OnError(error);
        }

        public void StopServer()
        {
            //todo logic before stop server
            this.Stop();
        }

        public void RestartServer()
        {
            //todo logic before stop server
            this.Restart();
        }
    }
}