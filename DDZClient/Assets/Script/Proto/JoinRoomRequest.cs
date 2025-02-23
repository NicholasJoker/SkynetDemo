using System;
using Script.Net;

namespace Script.Proto
{
    [Serializable]
    public class JoinRoomRequest:IMessage
    {
        public OperationCode opCode;
        public long playerId;
        public int roomId;
    }
}