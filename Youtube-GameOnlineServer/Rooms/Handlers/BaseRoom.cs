using System;
using System.Collections.Concurrent;
using System.Linq;
using Youtube_GameOnlineServer.Applications.Handlers;
using Youtube_GameOnlineServer.Applications.Interfaces;
using Youtube_GameOnlineServer.Applications.Messaging;
using Youtube_GameOnlineServer.Applications.Messaging.Constants;
using Youtube_GameOnlineServer.GameTiktaktoe.Constants;
using Youtube_GameOnlineServer.Rooms.Constants;
using Youtube_GameOnlineServer.Rooms.Interfaces;

namespace Youtube_GameOnlineServer.Rooms.Handlers
{
    public class BaseRoom : IBaseRoom
    {
        public string Id { get; set; }
        public RoomType RoomType { get; set; }
        public ConcurrentDictionary<string, IPlayer> Players { get; set; }
        protected string OwnerId { get; set; }

        protected BaseRoom(RoomType type)
        {
            RoomType = type;
            Id = GameHelper.RandomString(10);
            Players = new ConcurrentDictionary<string, IPlayer>();
        }

        public virtual bool JoinRoom(IPlayer player)
        {
            if (FindPlayer(player.GetUserInfo().Id) == null)
            {
                if (Players.TryAdd(player.GetUserInfo().Id, player))
                {
                    this.OwnerId ??= player.GetUserInfo().Id;
                    return true;
                }
            }

            return false;
        }

        public void RoomInfo()
        {
            var mess = new WsMessage<RoomInfo>(WsTags.RoomInfo, this.GetRoomInfo());
            this.SendMessage(mess);
        }

        public virtual bool ExitRoom(IPlayer player)
        {
            return this.ExitRoom(player.GetUserInfo().Id);
        }

        private void ChangeOwner(PixelType exitPixelType)
        {
            if (this.Players.Count <= 0) return;
            var player = Players.Values.ToList()[0];
            OwnerId = player.GetUserInfo().Id;
            player.SetPixelType(exitPixelType);

        }

        public virtual bool ExitRoom(string id)
        {
            var player = FindPlayer(id);
            if (player == null) return false;
            Players.TryRemove(player.GetUserInfo().Id, out var playerRemove);
            if (Players.IsEmpty)
            {
                RoomManager.Instance.RemoveRoom(this.Id);
                return true;
            }

            if (player.GetUserInfo().Id == OwnerId)
            {
                this.ChangeOwner(player.GetPixelType());
            }
            return true;

        }

        public virtual IPlayer FindPlayer(string id)
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

        public RoomInfo GetRoomInfo()
        {
            return new()
            {
                RoomId = this.Id,
                RoomType = RoomType,
                OwnerId = OwnerId,
                Players = Players.Values.Select(p => p.GetUserInfo()).ToList()
            };
        }
    }
}