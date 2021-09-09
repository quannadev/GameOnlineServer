using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Youtube_GameOnlineServer.GameTiktaktoe.Room;
using Youtube_GameOnlineServer.Rooms.Constants;
using Youtube_GameOnlineServer.Rooms.Interfaces;

namespace Youtube_GameOnlineServer.Rooms.Handlers
{
    public class RoomManager : IRoomManager
    {
        public static IRoomManager Instance { get; set; }
        public Lobby Lobby { get; set; }
        private ConcurrentDictionary<string, BaseRoom> Rooms { get; set; }

        public RoomManager()
        {
            Rooms = new ConcurrentDictionary<string, BaseRoom>();
            Lobby = new Lobby(RoomType.Lobby, this);
            Instance = this;
        }

        public BaseRoom CreateRoom(int timer)
        {
            var newRoom = new TiktakToeRoom(timer);
            Rooms.TryAdd(newRoom.Id, newRoom);
            return newRoom;
        }

        public BaseRoom FindRoom(string id)
        {
            return Rooms.FirstOrDefault(r => r.Key == id).Value;
        }

        public List<BaseRoom> ListRoom()
        {
            return this.Rooms.Values.ToList();
        }

        public bool RemoveRoom(string id)
        {
            var oldRoom = FindRoom(id);
            if (oldRoom != null)
            {
                Rooms.TryRemove(id, out var room);
                return room != null;
            }

            return false;
        }
    }
}