using System;
using Script.Net;

namespace Script.Proto
{
    [Serializable]
    public class PassResponse:IMessage
    {
        ReciveCode reciveCode;
        public long playerId;
        public int seatNo;
        public long nextPlayerId;
        public int state;
    }
}