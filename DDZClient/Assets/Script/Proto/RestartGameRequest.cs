using System;
using Script.Net;

namespace Script.Proto
{   [Serializable]
    public class RestartGameRequest:IMessage
    {
        public OperationCode opCode;
        public long playerId;
    }
}