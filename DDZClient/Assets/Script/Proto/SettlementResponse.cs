using System;
using Script.Net;
using UnityEngine;

namespace Script.Proto
{
    [Serializable]
    public class SettlementResponse:IMessage
    {
        public ReciveCode reciveCode;
        public int score;
        [SerializeField]
        public SettlementInfo[] settlementInfos;
        public long winPlayerId;
    }
    [Serializable]
    public class SettlementInfo
    {
        public int score;
        public string name;
        public long playerId;
    }
}