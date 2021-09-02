using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Youtube_GameOnlineServer.Applications.Interfaces;
using Youtube_GameOnlineServer.Logging;

namespace Youtube_GameOnlineServer.Applications.Handlers
{
    public class PlayersManager : IPlayerManager
    {
        public ConcurrentDictionary<string, IPlayer> Players { get; set; }
        private readonly IGameLogger _logger;
        public PlayersManager(IGameLogger logger)
        {
            _logger = logger;
            Players = new ConcurrentDictionary<string, IPlayer>();
        }
        public void AddPlayer(IPlayer player)
        {
            if (FindPlayer(player) == null)
            {
                Players.TryAdd(player.SessionId, player);
                _logger.Info($"List Players {Players.Count}");
            }
            
        }

        public void RemovePlayer(string id)
        {
            if (FindPlayer(id) != null)
            {
                Players.TryRemove(id, out var player);
                if (player != null)
                {
                    _logger.Info($"Remove {id} success");
                    _logger.Info($"List Players {Players.Count}");
                }
            }
        }

        public void RemovePlayer(IPlayer player)
        {
            this.RemovePlayer(player.SessionId);
        }

        public IPlayer FindPlayer(string id)
        {
            return Players.FirstOrDefault(p => p.Key.Equals(id)).Value;
        }

        public IPlayer FindPlayer(IPlayer player)
        {
            return Players.FirstOrDefault(p => p.Value.Equals(player)).Value;
        }

        public List<IPlayer> GetPlayers() => Players.Values.ToList();
    }
}