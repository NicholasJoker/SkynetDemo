using System;
using Script.Net;

namespace Script.Proto
{
    [Serializable]
    public class RequestExitRoom:IMessage
    {
        public OperationCode opCode;
        public int roomId;
        public long playerId;
    }
}