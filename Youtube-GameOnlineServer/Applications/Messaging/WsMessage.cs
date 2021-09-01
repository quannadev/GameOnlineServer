namespace Youtube_GameOnlineServer.Applications.Messaging
{
    public class WsMessage<T> : IMessage<T>
    {
        public WsTags Tags { get; set; }
        public T Data { get; set; }

        public WsMessage(WsTags tags, T data)
        {
            Tags = tags;
            Data = data;
        }
    }
}