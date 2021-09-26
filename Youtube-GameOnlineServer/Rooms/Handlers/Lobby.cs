using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using MongoDB.Driver;
using Youtube_GameOnlineServer.Applications.Interfaces;
using Youtube_GameOnlineServer.Applications.Messaging;
using Youtube_GameOnlineServer.Applications.Messaging.Constants;
using Youtube_GameOnlineServer.GameModels.Handlers;
using Youtube_GameOnlineServer.Rooms.Constants;
using Youtube_GameOnlineServer.Rooms.Interfaces;

namespace Youtube_GameOnlineServer.Rooms.Handlers
{
    public class Lobby : BaseRoom
    {
        private readonly IRoomManager _roomManager;
        private List<TopUser> TopUsers { get; set; }
        private UserHandler UserHandler { get; set; }

        public Lobby(RoomType type, IRoomManager roomManager, IMongoDatabase database) : base(type)
        {
            this._roomManager = roomManager;
            UserHandler = new UserHandler(database);
            this.GetListTop();
        }

        private void GetListTop()
        {
            var timer = new Timer(TimeSpan.FromSeconds(30).TotalMilliseconds) {AutoReset = true};
            timer.Elapsed += (sender, args) =>
            {
                this.TopUsers = new List<TopUser>();
                var listTop = UserHandler.GetTop(10);
                listTop.ForEach(item =>
                {
                    var topUser = new TopUser
                    {
                        Id = item.Id,
                        Avatar = item.Avatar,
                        DisplayName = item.DisplayName,
                        Point = item.Point
                    };
                    this.TopUsers.Add(topUser);
                });
            };
            timer.Start();
        }

        public override bool JoinRoom(IPlayer player)
        {
            if (base.JoinRoom(player))
            {
                this.RoomInfo();
                this.SendListMatch(player);
                var lobbyData = new LobbyData
                {
                    ListTop = this.TopUsers
                };
                var message = new WsMessage<LobbyData>(WsTags.LobbyInfo, lobbyData);
                player.SendMessage(message);
                return true;
            }

            return false;
        }

        public void SendListMatch(IPlayer player = null)
        {
            var listRoom = this._roomManager.ListRoom();
            var message =
                new WsMessage<List<RoomInfo>>(WsTags.ListRooms, listRoom.Select(item => item.GetRoomInfo()).ToList());
            if (player != null)
            {
                player.SendMessage(message);
            }
            else
            {
                this.SendMessage(message);
            }
        }
    }
}