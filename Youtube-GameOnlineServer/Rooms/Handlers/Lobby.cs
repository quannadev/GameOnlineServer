using Youtube_GameOnlineServer.Applications.Interfaces;
using Youtube_GameOnlineServer.Rooms.Constants;

namespace Youtube_GameOnlineServer.Rooms.Handlers
{
    public class Lobby : BaseRoom
    {

        public Lobby(RoomType type): base(type)
        {
            
        }

       
        public override bool JoinRoom(IPlayer player)
        {
            return base.JoinRoom(player);
        }
    }
}