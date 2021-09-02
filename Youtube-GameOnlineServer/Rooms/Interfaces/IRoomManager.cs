using Youtube_GameOnlineServer.Rooms.Handlers;

namespace Youtube_GameOnlineServer.Rooms.Interfaces
{
    public interface IRoomManager
    {
        BaseRoom Lobby { get; set; }
        BaseRoom CreateRoom();
        BaseRoom FindRoom(string id);
        bool RemoveRoom(string id);
    }
}