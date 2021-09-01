namespace Youtube_GameOnlineServer.Applications.Interfaces
{
    public interface IPlayer
    {
        public string SessionId { get; set; }
        public string Name { get; set; }

        void SetDisconnect(bool value);
        bool SendMessage(string mes);
        void OnDisconnect();
    }
}