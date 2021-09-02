using System;
using System.Net;
using GameDatabase.Mongodb.Handlers;
using Youtube_GameOnlineServer.Applications.Handlers;
using Youtube_GameOnlineServer.Applications.Interfaces;
using Youtube_GameOnlineServer.GameModels;
using Youtube_GameOnlineServer.Logging;

namespace Youtube_GameOnlineServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IGameLogger logger = new GameLogger();
            var mongodb = new MongoDb();
            IPlayerManager playerManager = new PlayersManager(logger);
            var wsServer = new WsGameServer(IPAddress.Any, 8080, playerManager, logger, mongodb);
            wsServer.StartServer();
            logger.Print("Game Server started");
            for (;;)
            {
                var type = Console.ReadLine();
                if (type == "restart")
                {
                    logger.Print("Game Server restarting...");
                    wsServer.RestartServer();
                }

                if (type == "shutdown")
                {
                    logger.Print("Game Server stopping...");
                    wsServer.StopServer();
                    break;
                }
            }
        }
    }
}