using System;
using System.Text;
using NetCoreServer;
using Youtube_GameOnlineServer.Applications.Interfaces;
using Youtube_GameOnlineServer.Applications.Messaging;
using Youtube_GameOnlineServer.Applications.Messaging.Constants;

namespace Youtube_GameOnlineServer.Applications.Handlers
{
    public class Player : WsSession, IPlayer
    {
        public string SessionId { get; set; }
        public string Name { get; set; }
        private bool IsDisconnected { get; set; }
        public Player(WsServer server) : base(server)
        {
            SessionId = this.Id.ToString();
            IsDisconnected = false;
        }

        public override void OnWsConnected(HttpRequest request)
        {
            //todo login on player connected
            var url = request.Url;
            Console.WriteLine("Player connected");
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
                        var x = 10;
                        break;
                    case WsTags.Register:
                        break;
                    case WsTags.Lobby:
                        break;
                    default:
                        break;
                        //throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                //todo send invalid message 
                Console.WriteLine(e);
            }
            //((WsGameServer) Server).SendAll($"{this.SessionId} send message {mess}");
        }

        public void SetDisconnect(bool value)
        {
            this.IsDisconnected = value;
        }

        public bool SendMessage(string mes)
        {
           return this.SendTextAsync(mes);
        }

        public void OnDisconnect()
        {
           //todo logic handle player disconnect
           Console.WriteLine("Player disconnected");
           //((WsGameServer) Server).PlayerManager.RemovePlayer(this);
        }
    }
}