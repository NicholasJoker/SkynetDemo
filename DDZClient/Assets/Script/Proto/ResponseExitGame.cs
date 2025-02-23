using Script.Net;

namespace Script.Proto
{
    public class ResponseExitGame:IMessage
    {
        public ReciveCode receiveCode;
        public long playerId;
        public int roomId;
    }
}