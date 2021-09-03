using System.Collections.Generic;
using Youtube_GameOnlineServer.Rooms.Constants;

namespace Youtube_GameOnlineServer.Applications.Messaging.Constants
{
    public struct RoomInfo
    {
        public List<UserInfo> Players { get; set; }
        public RoomType RoomType { get; set; }
    }
}