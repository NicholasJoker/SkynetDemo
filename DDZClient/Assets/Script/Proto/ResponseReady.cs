using System;
using Script.Net;
using UnityEngine;

namespace Script.Proto
{
    [Serializable]
    public class ResponseReady
    {   [Serializable]
        public class Status
        {   [SerializeField]
            public long playerId;
            public int roomId;
            /// <summary>
            /// 座位号
            /// </summary>
            public int seatNo;
            //0未准备 1准备
            public bool ready;
        }
        [SerializeField]
        public Status[] status;
        public ReciveCode receiveCode;
    }
}