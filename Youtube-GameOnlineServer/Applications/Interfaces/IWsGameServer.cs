namespace Youtube_GameOnlineServer.Applications.Interfaces
{
    public interface IWsGameServer
    {
        void StartServer();
        void StopServer();
        void RestartServer();
    }
}