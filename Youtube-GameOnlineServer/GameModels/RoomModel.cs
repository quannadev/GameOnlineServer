using System;
using System.Collections.Generic;
using Youtube_GameOnlineServer.Applications.Messaging.Constants.Match;
using Youtube_GameOnlineServer.GameModels.Base;

namespace Youtube_GameOnlineServer.GameModels
{
    public class RoomModel : BaseModel
    {
        public string RoomId { get; set; }
        public List<string> Players { get; set; }
        public List<MatchModel> Match { get; set; }

    }

    public class GameAction
    {
        public string UserId { get; set; }
        public PlaceData PlaceData { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MatchModel
    {
        public string Winner { get; set; }
        public int Point { get; set; }
        public string Loser { get; set; }
        public List<GameAction> GameActions { get; set; }
    }
}