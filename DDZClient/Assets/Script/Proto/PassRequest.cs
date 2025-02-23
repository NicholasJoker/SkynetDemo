using System;
using Script.Net;
using UnityEngine.Rendering;

namespace Script.Proto
{
    [Serializable]
    public class PassRequest:IMessage
    {
        public OperationCode opCode;
        /// <summary>
        /// 桌面上打出牌的玩家
        /// </summary>
        public long tableCardId;
        /// <summary>
        /// 接下来的出牌的人
        /// </summary>
        public long playerId;
        /// <summary>
        /// 座位号
        /// </summary>
        public int seatNo;
    }
}