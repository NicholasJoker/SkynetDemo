using System;
using Script.Net;

namespace Script.Proto
{
    [Serializable]
    public class RequestCards:IMessage
    {
        public OperationCode opCode;
        public long playerId;
        public string roomId;
    }
}