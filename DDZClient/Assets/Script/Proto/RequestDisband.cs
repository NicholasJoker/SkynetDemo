using System;
using Script.Net;

namespace Script.Proto
{
    [Serializable]
    public class RequestDisband:IMessage
    {
        OperationCode opCode;
        public long playerId;
    }
}