using Script.Net;

namespace Script.Proto
{
    public class ResponseExitRoom:IMessage
    {
        public ReciveCode reciveCode;
        public long playerId;
        public int roomId;
        public int ready;
        /// <summary>
        /// 是否解散
        /// </summary>
        public int isDisband;
    }
}