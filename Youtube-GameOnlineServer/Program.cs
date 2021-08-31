using System;
using System.Net;
using Youtube_GameOnlineServer.Applications.Handlers;

namespace Youtube_GameOnlineServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var wsServer = new WsGameServer(IPAddress.Any, 8080);
            wsServer.StartServer();
            for (;;)
            {
                var type = Console.ReadLine();
                if (type == "restart")
                {
                    wsServer.RestartServer();
                }

                if (type == "shutdown")
                {
                    wsServer.StopServer();
                    break;
                }
            }
        }
    }
}