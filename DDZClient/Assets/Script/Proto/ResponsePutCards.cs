using Script.Game;
using Script.Net;

namespace Script.Proto
{
    public class ResponsePutCards
    {
        public ReciveCode receiveCode;
        public long playerId;
        public int[] cards;
        public int seatNo;
        public CardGroupType groupType;
        public int ok;
        public bool isOver;
    }
}