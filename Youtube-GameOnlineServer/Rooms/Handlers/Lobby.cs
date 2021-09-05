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
            base.JoinRoom(player);
            var listRoom = this._roomManager.ListRoom();
            var message = new WsMessage<List<RoomInfo>>(WsTags.ListRooms, listRoom.Select(item => item.GetRoomInfo()).ToList());
            player.SendMessage(message);
            return true;
        }
    }
}