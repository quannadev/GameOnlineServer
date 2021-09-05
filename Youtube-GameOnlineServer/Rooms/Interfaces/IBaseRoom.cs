using System.Collections.Concurrent;
using Youtube_GameOnlineServer.Applications.Interfaces;
using Youtube_GameOnlineServer.Applications.Messaging;
using Youtube_GameOnlineServer.Applications.Messaging.Constants;
using Youtube_GameOnlineServer.Rooms.Constants;

namespace Youtube_GameOnlineServer.Rooms.Interfaces
{
    public interface IBaseRoom
    {
        public string Id { get; set; }
        public RoomType RoomType { get; set; }
        public ConcurrentDictionary<string, IPlayer> Players { get; set; }

        bool JoinRoom(IPlayer player);
        bool ExitRoom(IPlayer player);
        bool ExitRoom(string id);
        IPlayer FindPlayer(string id);
        void SendMessage(string mes);
        void SendMessage<T>(WsMessage<T> message);
        void SendMessage<T>(WsMessage<T> message, string idIgnore);
        RoomInfo GetRoomInfo();
    }
}