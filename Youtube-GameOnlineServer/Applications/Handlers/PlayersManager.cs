using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Youtube_GameOnlineServer.Applications.Interfaces;

namespace Youtube_GameOnlineServer.Applications.Handlers
{
    public class PlayersManager : IPlayerManager
    {
        public ConcurrentDictionary<string, IPlayer> Players { get; set; }

        public PlayersManager()
        {
            Players = new ConcurrentDictionary<string, IPlayer>();
        }
        public void AddPlayer(IPlayer player)
        {
            if (FindPlayer(player) == null)
            {
                Players.TryAdd(player.SessionId, player);
                Console.WriteLine($"List Players {Players.Count}");
            }
            
        }

        public void RemovePlayer(string id)
        {
            if (FindPlayer(id) != null)
            {
                Players.TryRemove(id, out var player);
                if (player != null)
                {
                    Console.WriteLine($"Remove {id} success");
                    Console.WriteLine($"List Players {Players.Count}");
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