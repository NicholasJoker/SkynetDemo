using Script.Net;
using UnityEngine.Rendering;

namespace Script.Proto
{
    public class RequestCreateRoom:IMessage
    {
        public OperationCode opCode;
        public long playerId;
    }
}