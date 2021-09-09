using System.Collections.Generic;
using System.Linq;
using Youtube_GameOnlineServer.Applications.Interfaces;
using Youtube_GameOnlineServer.Applications.Messaging;
using Youtube_GameOnlineServer.Applications.Messaging.Constants;
using Youtube_GameOnlineServer.Rooms.Constants;
using Youtube_GameOnlineServer.Rooms.Interfaces;

namespace Youtube_GameOnlineServer.Rooms.Handlers
{
    public class Lobby : BaseRoom
    {
        private readonly IRoomManager _roomManager;

        public Lobby(RoomType type, IRoomManager roomManager) : base(type)
        {
            this._roomManager = roomManager;
        }


        public override bool JoinRoom(IPlayer player)
        {
            if (base.JoinRoom(player))
            {
                this.RoomInfo();
                this.SendListMatch(player);
                return true;
            }
            return false;
        }

        public void SendListMatch(IPlayer player = null)
        {
            var listRoom = this._roomManager.ListRoom();
            var message =
                new WsMessage<List<RoomInfo>>(WsTags.ListRooms, listRoom.Select(item => item.GetRoomInfo()).ToList());
            if (player != null)
            {
                player.SendMessage(message);
            }
            else
            {
                this.SendMessage(message);
            }
        }
    }
}