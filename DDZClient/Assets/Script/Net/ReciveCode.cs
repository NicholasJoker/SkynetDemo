using Script.Proto;

namespace Script.Net
{
    public enum ReciveCode
    {
        /// <summary>
        /// 登录返回码0
        /// </summary>
        LoginResponseCode = 0,
        /// <summary>
        /// 创建房间返回码1
        /// </summary>
        CreateRoomResponeseCode = 1,
        /// <summary>
        /// 加入房间返回码2
        /// </summary>
        JoinRoomResponseCode = 2,
        /// <summary>
        /// 准备的消息返回码3
        /// </summary>
        ReadyResponseCode =3,
        /// <summary>
        /// 发牌返回码 4
        /// </summary>
        DealCardsResponseCode =4,
        /// <summary>
        /// 抢地主返回码 5
        /// </summary>
        LandlordCode = 5,
        /// <summary>
        /// 出牌的回应 6
        /// </summary>
        PutCardsResponseCode = 6,
        /// <summary>
        /// 要不起过牌
        /// </summary>
        PassResponseCode = 7,
        /// <summary>
        /// 退出房间
        /// </summary>
        ExitRoomResponseCode = 8,
        /// <summary>
        /// 退出游戏
        /// </summary>
        ExitGameResponseCode = 9,
        /// <summary>
        /// 结算消息
        /// </summary>
        SettlementResponseCode = 10,
        
        
        
        /// <summary>
        /// 重连
        /// </summary>
        ReconnectResponseCode = 99,
        /// <summary>
        /// 所有人都已经准备抢地主
        /// </summary>
        AllReadyResponseCode = 100,
    }

    public enum ResponseErrorCode
    {
        /// <summary>
        /// 登录失败
        /// </summary>
        LoginError = -1,
        /// <summary>
        /// 创建房间失败
        /// </summary>
        CreateRoomError = -2,
        /// <summary>
        /// 房间不存在
        /// </summary>
        NotExitRoomError = -3,
        /// <summary>
        /// 房间人数已满
        /// </summary>
        JoinRoomError = -4,
        /// 准备请求失败
        /// </summary>
        ReadyError = -5,
        /// <summary>
        /// 开牌失败
        /// </summary>
        DealCardsError = -6,
        /// <summary>
        /// 抢地主失败
        /// </summary>
        LandlordError = -7,
        /// <summary>
        /// 客户端参数问题
        /// </summary>
        ArgsError = -100,
    }
}