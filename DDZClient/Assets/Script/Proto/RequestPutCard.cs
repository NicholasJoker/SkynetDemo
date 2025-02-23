using System;
using Script.Game;
using Script.Net;

namespace Script.Proto
{
    [Serializable]
    public class RequestPutCard:IMessage
    {
        public OperationCode opCode;
        public int[] cards;
        public int seatNo;
        public int roomId;
        public long playerId;
        public CardGroupType cardGroup;
    }
}