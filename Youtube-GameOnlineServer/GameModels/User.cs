using Youtube_GameOnlineServer.Applications.Handlers;
using Youtube_GameOnlineServer.GameModels.Base;

namespace Youtube_GameOnlineServer.GameModels
{
    public class User : BaseModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
        public int Level { get; set; }
        public long Amount { get; set; }
        public long Point { get; set; }

        public User(string username, string password, string displayName)
        {
            Username = username;
            DisplayName = displayName;
            Password = GameHelper.HashPassword(password);
            Avatar = "";
            Level = 1;
            Amount = 0;
            Point = 0;
        }
    }
}