using System;
using Script.Net;

namespace Script.Proto
{
    [Serializable]
    public class RequestDropLandlord:IMessage
    {
        public OperationCode opCode;
        public long playerId;
    }
}