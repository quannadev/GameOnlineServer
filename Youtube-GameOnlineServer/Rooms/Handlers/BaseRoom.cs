using System;
using System.Collections.Concurrent;
using System.Linq;
using Youtube_GameOnlineServer.Applications.Handlers;
using Youtube_GameOnlineServer.Applications.Interfaces;
using Youtube_GameOnlineServer.Applications.Messaging;
using Youtube_GameOnlineServer.Applications.Messaging.Constants;
using Youtube_GameOnlineServer.Rooms.Interfaces;

namespace Youtube_GameOnlineServer.Rooms.Handlers
{
    public class BaseRoom : IBaseRoom
    {
        public string Id { get; set; }
        public ConcurrentDictionary<string, IPlayer> Players { get; set; }

        public BaseRoom()
        {
            Id = GameHelper.RandomString(10);
            Players = new ConcurrentDictionary<string, IPlayer>();
        }

        public bool JoinRoom(IPlayer player)
        {
            if (FindPlayer(player.SessionId) == null)
            {
                if (Players.TryAdd(player.SessionId, player))
                {
                    this.RoomInfo();
                    return true;
                }
            }

            return false;
        }

        private void RoomInfo()
        {
            var lobby = new LobbyInfo
            {
                Players = Players.Values.Select(p => p.GetUserInfo()).ToList()
            };
            var mess = new WsMessage<LobbyInfo>(WsTags.RoomInfo, lobby);
            this.SendMessage(mess);
        }

        public bool ExitRoom(IPlayer player)
        {
            return this.ExitRoom(player.SessionId);
        }

        public bool ExitRoom(string id)
        {
            var player = FindPlayer(id);
            if (player != null)
            {
                Players.TryRemove(player.SessionId, out player);
                this.RoomInfo();
                return true;
            }

            return false;
        }

        public IPlayer FindPlayer(string id)
        {
            return Players.FirstOrDefault(p => p.Key.Equals(id)).Value;
        }

        public void SendMessage(string mes)
        {
            lock (Players)
            {
                foreach (var player in Players.Values)
                {
                    player.SendMessage(mes);
                }
            }
        }

        public void SendMessage<T>(WsMessage<T> message)
        {
            lock (Players)
            {
                foreach (var player in Players.Values)
                {
                    player.SendMessage(message);
                }
            }
        }

        public void SendMessage<T>(WsMessage<T> message, string idIgnore)
        {
            lock (Players)
            {
                foreach (var player in Players.Values.Where(p => p.SessionId != idIgnore))
                {
                    player.SendMessage(message);
                }
            }
        }
    }
}