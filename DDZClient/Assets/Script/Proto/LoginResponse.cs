using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Script.Net;
using UnityEngine;

namespace Script.Proto
{
    [Serializable]
    public class LoginResponse:IMessage
    {
        public ReciveCode reciveCode;
        public long id;
        public string username;
        public int errCode;
        /// <summary>
        /// 如果是被顶掉那就是0 正常是1
        /// </summary>
        public int isOk;
        /// <summary>
        /// 是否需要重连到游戏(在房间中已经开局的情况下)
        /// </summary>
        public int isReconnect;

        public int reconnectRoomId;
    }
}