using System.Collections.Generic;
using Youtube_GameOnlineServer.Rooms.Constants;

namespace Youtube_GameOnlineServer.Applications.Messaging.Constants
{
    public struct RoomInfo
    {
        public string RoomId { get; set; }
        public string OwnerId { get; set; }
        public List<UserInfo> Players { get; set; }
        public RoomType RoomType { get; set; }
    }
}