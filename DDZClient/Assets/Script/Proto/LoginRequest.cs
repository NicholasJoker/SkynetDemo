using System;
using Script.Net;

namespace Script.Proto
{
    [Serializable]
    public class LoginRequest:IMessage
    {
        public  OperationCode opCode;
        public  string username;
        public  string password;
    }
}