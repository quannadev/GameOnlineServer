using System.Collections.Generic;

namespace Youtube_GameOnlineServer.Applications.Messaging.Constants
{
    public struct LobbyInfo
    {
        public List<UserInfo> Players { get; set; }
    }
}