namespace Script.Net
{
    public enum OperationCode
    {   
        /// <summary>
        /// 登录 0
        /// </summary>
        Login = 0,
        /// <summary>
        /// 创建房间 1
        /// </summary>
        CreateRoom = 1,
        /// <summary>
        /// 加入房间 2
        /// </summary>
        JoinRoom = 2,
        /// <summary>
        /// 准备 3
        /// </summary>
        Ready = 3,
        /// <summary>
        /// 不抢地主(判断是否需要重开)
        /// </summary>
        DropLandlord = 4,
        /// <summary>
        /// 抢地主 5
        /// </summary>
        RequestLandlord = 5,
        /// <summary>
        /// 出牌 6
        /// </summary>
        PutCards =6,
        /// <summary>
        /// 要不起 过牌 7 
        /// </summary>
        Pass =7,
        /// <summary>
        /// 未开牌退出房间 8
        /// </summary>
        ExitRoom = 8,
        /// <summary>
        /// 退出游戏 9
        /// </summary>
        ExitGame = 9,
        /// <summary>
        /// 再开一局
        /// </summary>
        RestartGame = 11,
        
        /// <summary>
        /// 重连
        /// </summary>
        Reconnect = 99
    }
}