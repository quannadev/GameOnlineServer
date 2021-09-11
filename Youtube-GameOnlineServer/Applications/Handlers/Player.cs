using System;
using System.Text;
using GameDatabase.Mongodb.Handlers;
using GameDatabase.Mongodb.Interfaces;
using MongoDB.Driver;
using NetCoreServer;
using Youtube_GameOnlineServer.Applications.Interfaces;
using Youtube_GameOnlineServer.Applications.Messaging;
using Youtube_GameOnlineServer.Applications.Messaging.Constants;
using Youtube_GameOnlineServer.Applications.Messaging.Constants.Match;
using Youtube_GameOnlineServer.GameModels;
using Youtube_GameOnlineServer.GameModels.Handlers;
using Youtube_GameOnlineServer.GameTiktaktoe.Constants;
using Youtube_GameOnlineServer.GameTiktaktoe.Room;
using Youtube_GameOnlineServer.Logging;
using Youtube_GameOnlineServer.Rooms.Handlers;

namespace Youtube_GameOnlineServer.Applications.Handlers
{
    public class Player : WsSession, IPlayer
    {
        public string SessionId { get; set; }
        public string Name { get; set; }
        private bool IsDisconnected { get; set; }
        private readonly IGameLogger _logger;
        private UserHandler UsersDb { get; set; }
        private User UserInfo { get; set; }
        private TiktakToeRoom CurrentRoom { get; set; }

        private PixelType PixelType { get; set; }


        public Player(WsServer server, IMongoDatabase database) : base(server)
        {
            SessionId = this.Id.ToString();
            IsDisconnected = false;
            _logger = new GameLogger();
            UsersDb = new UserHandler(database);
        }

        public override void OnWsConnected(HttpRequest request)
        {
            //todo login on player connected
            var url = request.Url;
            _logger.Info("Player connected");
            IsDisconnected = false;
        }

        public override void OnWsDisconnected()
        {
            OnDisconnect();
            base.OnWsDisconnected();
        }


        public override void OnWsReceived(byte[] buffer, long offset, long size)
        {
            var mess = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);
            try
            {
                var wsMess = GameHelper.ParseStruct<WsMessage<object>>(mess);
                switch (wsMess.Tags)
                {
                    case WsTags.Invalid:
                        break;
                    case WsTags.Login:
                        var loginData = GameHelper.ParseStruct<LoginData>(wsMess.Data.ToString());
                        UserInfo = UsersDb.FindByUserName(loginData.Username);
                        if (UserInfo != null)
                        {
                            var hashPass = GameHelper.HashPassword(loginData.Password);
                            if (hashPass == UserInfo.Password)
                            {
                                //todo move user to lobby
                                var messInfo = new WsMessage<UserInfo>(WsTags.UserInfo, this.GetUserInfo());
                                this.SendMessage(messInfo);
                                this.PlayerJoinLobby();
                                return;
                            }
                        }

                        var invalidMess = new WsMessage<string>(WsTags.Invalid, "Username Or Password is Invalid");
                        this.SendMessage(GameHelper.ParseString(invalidMess));
                        break;
                    case WsTags.Register:
                        var regData = GameHelper.ParseStruct<RegisterData>(wsMess.Data.ToString());
                        if (UserInfo != null)
                        {
                            invalidMess = new WsMessage<string>(WsTags.Invalid, "You are Loginned");
                            this.SendMessage(GameHelper.ParseString(invalidMess));
                            return;
                        }

                        var check = UsersDb.FindByUserName(regData.Username);
                        if (check != null)
                        {
                            invalidMess = new WsMessage<string>(WsTags.Invalid, "Username exits");
                            this.SendMessage(GameHelper.ParseString(invalidMess));
                            return;
                        }


                        var newUser = new User(regData.Username, regData.Password, regData.DisplayName);
                        UserInfo = UsersDb.Create(newUser);
                        if (UserInfo != null)
                        {
                            //todo move user to lobby
                            this.PlayerJoinLobby();
                        }

                        break;
                    case WsTags.RoomInfo:
                        break;
                    case WsTags.UserInfo:
                        break;
                    case WsTags.CreateRoom:
                        var createRoom = GameHelper.ParseStruct<CreatRoomData>(wsMess.Data.ToString());
                        this.OnUserCreateRoom(createRoom);
                        break;
                    case WsTags.QuickPlay:
                        break;
                    case WsTags.JoinRoom:
                        var roomInfo = GameHelper.ParseStruct<RoomInfoData>(wsMess.Data.ToString());
                        this.OnUserJoinRoom(roomInfo);
                        break;
                    case WsTags.ExitRoom:
                        this.OnUserExitRoom();
                        break;
                    case WsTags.StartGame:
                        this.OnUserStartGame();
                        break;
                    case WsTags.SetPlace:
                        this.CurrentRoom?.SetPlace(this, GameHelper.ParseStruct<PlaceData>(wsMess.Data.ToString()));
                        break;
                    default:
                        break;
                    //throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                _logger.Error("OnWsReceived error", e);
                //todo send invalid message 
            }
            //((WsGameServer) Server).SendAll($"{this.SessionId} send message {mess}");
        }

        private void OnUserStartGame()
        {
            if (CurrentRoom == null)
            {
                return;
            }

            CurrentRoom.StartGame(this);
        }

        private void OnUserCreateRoom(CreatRoomData data)
        {
            var room = (TiktakToeRoom) ((WsGameServer) Server).RoomManager.CreateRoom(data.Time);
            if (room != null && room.JoinRoom(this))
            {
                var lobby = ((WsGameServer) Server).RoomManager.Lobby;
                lobby.ExitRoom(this);
                lobby.SendListMatch();
                CurrentRoom = room;
            }
        }

        private void OnUserJoinRoom(RoomInfoData data)
        {
            var room = (TiktakToeRoom) ((WsGameServer) Server).RoomManager.FindRoom(data.RoomId);
            if (room != null && room.JoinRoom(this))
            {
                this.CurrentRoom = room;
            }
        }

        private void OnUserExitRoom()
        {
            if (CurrentRoom == null)
            {
                return;
            }

            if (CurrentRoom.ExitRoom(this))
            {
                this.PlayerJoinLobby();
            }
        }

        private void PlayerJoinLobby()
        {
            var lobby = ((WsGameServer) Server).RoomManager.Lobby;
            lobby.JoinRoom(this);
            //todo logic join lobby
        }

        public void SetDisconnect(bool value)
        {
            this.IsDisconnected = value;
        }

        public bool SendMessage(string mes)
        {
            return this.SendTextAsync(mes);
        }

        public bool SendMessage<T>(WsMessage<T> message)
        {
            var mes = GameHelper.ParseString(message);
            return this.SendMessage(mes);
        }

        public void OnDisconnect()
        {
            //todo logic handle player disconnect
            var lobby = ((WsGameServer) Server).RoomManager.Lobby;
            this.OnUserExitRoom();
            lobby.ExitRoom(this);
            _logger.Warning("Player disconnected", null);
            //((WsGameServer) Server).PlayerManager.RemovePlayer(this);
        }

        public UserInfo GetUserInfo()
        {
            if (UserInfo != null)
            {
                return new UserInfo
                {
                    Id = this.UserInfo.Id,
                    DisplayName = UserInfo.DisplayName,
                    Amount = UserInfo.Amount,
                    Avatar = UserInfo.Avatar,
                    Level = UserInfo.Level,
                    PixelType = this.PixelType
                };
            }

            return new UserInfo();
        }

        public void SetPixelType(PixelType type)
        {
            this.PixelType = type;
        }

        public PixelType GetPixelType()
        {
            return this.PixelType;
        }
    }
}