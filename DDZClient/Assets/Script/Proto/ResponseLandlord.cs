using System;
using Script.Net;

namespace Script.Proto
{
    [Serializable]
    public class ResponseLandlord
    {
        public ReciveCode receiveCode;
        public long playerId;
        public int[] cards;
    }
}