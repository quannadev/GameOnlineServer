using Youtube_GameOnlineServer.GameTiktaktoe.Constants;

namespace Youtube_GameOnlineServer.Applications.Messaging.Constants
{
    public struct UserInfo
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
        public int Level { get; set; }
        public long Amount { get; set; }
        public long Point { get; set; }
        
        public PixelType PixelType { get; set; }
        
    }
}