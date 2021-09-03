using Youtube_GameOnlineServer.Rooms.Constants;

namespace Youtube_GameOnlineServer.Rooms.Handlers
{
    public class TictacToeRoom : BaseRoom
    {
        private readonly int _time;

        public TictacToeRoom(int time = 10) : base(RoomType.Battle)
        {
            _time = time;
        }
    }
}