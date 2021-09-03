using Youtube_GameOnlineServer.Rooms.Handlers;

namespace Youtube_GameOnlineServer.Rooms.Interfaces
{
    public interface IRoomManager
    {
        Lobby Lobby { get; set; }
        BaseRoom CreateRoom(int timer);
        BaseRoom FindRoom(string id);
        bool RemoveRoom(string id);
    }
}