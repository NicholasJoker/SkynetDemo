using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Script.Net;
using UnityEngine;

namespace Script.Proto
{
    [Serializable]
    public class JoinRoomResponse:IMessage
    {  
        public ReciveCode reciveCode;
        [Serializable]
        public class PlayerStatus
        {
            [SerializeField]
            public string username;
            public long playerId;
            public int seatNo;
        }
        [SerializeField]
        public PlayerStatus[]playerstatus;
        public int roomId;
    }

}