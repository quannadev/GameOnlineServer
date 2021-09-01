using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Youtube_GameOnlineServer.Applications.Interfaces
{
    public interface IPlayerManager
    {
        ConcurrentDictionary<string, IPlayer> Players { get; set; }
        void AddPlayer(IPlayer player);
        void RemovePlayer(string id);
        void RemovePlayer(IPlayer player);
        IPlayer FindPlayer(string id);
        IPlayer FindPlayer(IPlayer player);
        List<IPlayer> GetPlayers();
    }
}