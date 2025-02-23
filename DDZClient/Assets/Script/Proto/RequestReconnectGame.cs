using System;
using Script.Net;

namespace Script.Proto
{
    [Serializable]
    public class RequestReconnectGame:IMessage
    {
        public OperationCode opCode;
        public long playerId;
    }
}