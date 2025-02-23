using System;
using Script.Net;

namespace Script.Proto
{
    [Serializable]
    public class ResponseCards
    {
        public ReciveCode receiveCode;
        public int[] cards;
    }
}