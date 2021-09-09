using Youtube_GameOnlineServer.Applications.Messaging;
using Youtube_GameOnlineServer.Applications.Messaging.Constants;
using Youtube_GameOnlineServer.GameTiktaktoe.Constants;

namespace Youtube_GameOnlineServer.Applications.Interfaces
{
    public interface IPlayer
    {
        public string SessionId { get; set; }
        public string Name { get; set; }
        void SetDisconnect(bool value);
        bool SendMessage(string mes);
        bool SendMessage<T>(WsMessage<T> message);
        void OnDisconnect();
        UserInfo GetUserInfo();
        void SetPixelType(PixelType type);
        PixelType GetPixelType();
    }
}