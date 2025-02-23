using Script.Net;

namespace Script.Proto
{
    public class ResponseCreateRoom:IMessage
    {
        public ReciveCode receiveCode;
        public int roomId;
    }
}