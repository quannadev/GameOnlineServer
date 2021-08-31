using System;
using System.Net;
using System.Net.Sockets;
using NetCoreServer;
using Youtube_GameOnlineServer.Applications.Interfaces;

namespace Youtube_GameOnlineServer.Applications.Handlers
{
    public class WsGameServer: WsServer, IWsGameServer
    {
        private int _port;
        public WsGameServer(IPAddress address, int port) : base(address, port)
        {
            _port = port;
        }

        protected override TcpSession CreateSession()
        {
            //todo handle new session
            Console.WriteLine("New Session connected");
            return base.CreateSession();
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