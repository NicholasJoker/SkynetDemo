using System;
using Script.Net;
using UnityEngine;

namespace Script.Proto
{
    [Serializable]
    public class ResponseReconnectGame:IMessage
    {
        public ReciveCode reciveCode;
        public int[] cards;
        public int roomId;
        /// <summary>
        /// 桌面牌 
        /// </summary>
        public int[] tabCards;
        /// <summary>
        /// 谁出的桌面牌
        /// </summary>
        public long putCardPlayerId;
        /// <summary>
        /// 当前响应玩家
        /// </summary>
        public long activePlayerId;

        /// <summary>
        /// 地主id
        /// </summary>
        public long landlordPlayerId;
        /// <summary>
        /// 玩家信息
        /// </summary>
        [SerializeField]
        public ReconnectStatus[]playerstatus;
    }
    [Serializable]
    public class ReconnectStatus
    {
        [SerializeField]
        public string username;
        public long playerId;
        public int seatNo;
        public int cardCount;
    }
}