using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Script.Net;
using Script.Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityWebSocket;
using ErrorEventArgs = UnityWebSocket.ErrorEventArgs;

namespace Script.Game
{
    public class GameManager : MonoBehaviour
    {
        #region 字段
        private static GameManager instance;

        public static GameManager Instance
        {
            get
            {
                return instance;
            }
        }
        private GameObject table;
        /// <summary>
        /// 牌背
        /// </summary>
        private GameObject backCard;
        /// <summary>
        /// 牌面
        /// </summary>
        public GameObject prefabCard;
        /// <summary>
        /// 发牌位置
        /// </summary>
        /// <returns></returns>
        private Transform heapPos;
        /// <summary>
        /// 当前的牌局状态
        /// </summary>
        private CardManagerStates currentState;
        /// <summary>
        /// 位置索引
        /// </summary>
        private int termCurrentIndex;
        /// <summary>
        /// 开始出牌的玩家位置索引 也就是地主
        /// </summary>
        private int termStartIndex;
        /// <summary>
        /// 自己的位置索引 永远是0
        /// </summary>
        private int selfPlayerIndex = 0;
        /// <summary>
        /// 仅玩家自己
        /// </summary>
        private ClientPlayer player;
        /// <summary>
        /// 玩家们发牌的位置
        /// </summary>
        public List<Transform> playerHeapPos;
        /// <summary>
        /// 玩家们
        /// </summary>
        private List<ClientPlayer> clientPlayers;
        /// <summary>
        /// 所有牌集合
        /// </summary>
        private string[] cardNames;
        /// <summary>
        /// 地主牌
        /// </summary>
        private GameObject landlord;

        /// <summary>
        /// 庄家座位号
        /// </summary>
        private int bankerIndex;
        /// <summary>
        ///庄家的id
        /// </summary>
        private long bankerId;
        /// <summary>
        /// 发牌速度
        /// </summary>
        private float dealCardSpeed =10.0f;
        /// <summary>
        /// 准备
        /// </summary>
        private Button startButton;
        /// <summary>
        /// 抢地主
        /// </summary>
        private Button landlordButton;
        /// <summary>
        /// 不抢
        /// </summary>
        private Button cancelButton;
        /// <summary>
        /// 选择创建房间
        /// </summary>
        private Button selectCreateRoom;
        
        /// <summary>
        /// 选择加入房间
        /// </summary>
        private Button selectJoinRoom;

        /// <summary>
        /// 点击加入房间
        /// </summary>
        private Button joinBtn;

        /// <summary>
        /// 输入的房间号
        /// </summary>
        private TMP_InputField roomNumber;
        /// <summary>
        /// 显示的房间号
        /// </summary>
        private TMP_Text roomNoText;
        /// <summary>
        /// 离开房间
        /// </summary>
        private Button exitRoomBtn;
        /// <summary>
        /// todo 直接缓存的登录返回
        /// </summary>
        private LoginResponse response;

        private Button joinRoomWinCloseBtn;
        /// <summary>
        /// 结算
        /// </summary>
        private GameObject gameOver;
        
        private Button gameOverBtn;
        private Button gameBeginBtn;

        /// <summary>
        /// 登录窗口
        /// </summary>
        private GameObject loginWin;
        /// <summary>
        /// 自己的id
        /// </summary>
        private long playerId;
        /// <summary>
        /// 所在房间号
        /// </summary>
        private int roomId;

        /// <summary>
        /// 桌面上出牌的玩家的id
        /// </summary>
        public long tableCardId;
        // [SerializeField]
        // public string addrsss = "ws://192.168.1.87:8886";
        //private TWebSocket webSocket;
        #endregion
        public void InitGameManager(LoginResponse message)
        {
            Application.runInBackground = true;
            response = message;
            
        }
        private string[] GetCardNames()
        {
            //路径  
            string fullPath = "Assets/Resources/Card/";
            if (Directory.Exists(fullPath))
            {
                DirectoryInfo direction = new DirectoryInfo(fullPath);
                FileInfo[] files = direction.GetFiles("*.png", SearchOption.AllDirectories);

                return files.Select(s => Path.GetFileNameWithoutExtension(s.Name)).ToArray();
            }
            return null;
        }
        /// <summary>
        /// 点击创建房间
        /// </summary>
        public void SelectCreateRoom()
        {
            SendCreateRoom();
        }
        /// <summary>
        /// 打开输入房间号界面
        /// </summary>
        public void OpenJoinRoomWin()
        {
            if (roomNumber != null)
            {
                roomNumber.transform.parent.gameObject.SetActive(true);
            }
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public void CloseJoinRoomWin()
        {
            if (roomNumber != null)
            {
                roomNumber.transform.parent.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// 加入房间
        /// </summary>
        public void JoinRoom(JoinRoomRequest message)
        {
            TWebSocket.Instance.Send(message);
        }
        /// <summary>
        /// 点击退出房间
        /// </summary>
        public void ClickExitRoom()
        {
            if (exitRoomBtn != null)
            {
                if (currentState == CardManagerStates.Ready)
                {
                    SendExitRoom(player.playerId);
                }
                else
                {
                    //直接退出游戏
                    SendExitGame();
                }
            }
        }
        #region 生命周期
        public void Awake()
        {
            Debug.Log("GameManager");
            // TWebSocket.Instance.socket.OnMessage += SocketOnMessage;
            // TWebSocket.Instance.socket.OnError += SocketOnError;
            // TWebSocket.Instance.socket.OnClose += SocketOnClose;
            // TWebSocket.Instance.socket.OnError += SocketOnError;
            // //UI
            // instance = this;
            // loginWin = gameObject.transform.parent.Find("Login").gameObject;
            // table = gameObject.transform.Find("Table").gameObject;
            // heapPos = table.transform.Find("HeapPos").transform;
            // landlord = table.transform.Find("Landlord").gameObject;
            // backCard = Resources.Load<GameObject>("Prefab/BackCard");
            //
            // cardNames = GetCardNames();
            // // Debug.Log($"cardNames.ength:{cardNames.Length}");
            //
            // startButton = table.transform.Find("StartBtn").GetComponent<Button>();
            // landlordButton = table.transform.Find("LandlordBtn").GetComponent<Button>();
            // cancelButton = table.transform.Find("CancelBtn").GetComponent<Button>();
            // selectCreateRoom = gameObject.transform.Find("Select/CreateRoom").GetComponent<Button>();
            // selectJoinRoom = gameObject.transform.Find("Select/JoinRoom").GetComponent<Button>();
            // roomNoText = table.transform.Find("RoomId").GetComponent<TMP_Text>();
            //
            // joinRoomWinCloseBtn = transform.Find("JoinRoomWin").GetComponent<Button>();
            // joinBtn = transform.Find("JoinRoomWin/RoomNumber/JoinBtn").gameObject.GetComponent<Button>();
            // roomNumber = transform.Find("JoinRoomWin/RoomNumber").GetComponent<TMP_InputField>();
            // gameOver = gameObject.transform.Find("GameOver").gameObject;
            // gameOverBtn = gameOver.transform.Find("GameOverCloseBtn").GetComponent<Button>();
            // gameBeginBtn = gameObject.transform.Find("GameBeginBtn").GetComponent<Button>();
            // exitRoomBtn = table.transform.Find("ExitRoomBtn").GetComponent<Button>();
            // startButton.onClick.AddListener(Ready);
            // gameOverBtn.onClick.AddListener(CloseSettlement);
            // gameBeginBtn.onClick.AddListener(ResetGame);
            // selectCreateRoom.onClick.AddListener(SelectCreateRoom);
            // selectJoinRoom.onClick.AddListener(OpenJoinRoomWin);
            // joinBtn.onClick.AddListener(()=>SendJoinRoom(roomNumber.text));
            // landlordButton.onClick.AddListener(LandlordButtonClick);
            // cancelButton.onClick.AddListener(CancelLandlordOnClick);
            // joinRoomWinCloseBtn.onClick.AddListener(CloseJoinRoomWin);
            // exitRoomBtn.onClick.AddListener(ClickExitRoom);
            // //牌桌玩家和位置
            // playerHeapPos = new List<Transform>();
            // clientPlayers = new List<ClientPlayer>();
            // for (int i = 0; i < 3; i++)
            // {
            //     var client = table.transform.GetChild(i);
            //     clientPlayers.Add(client.GetComponent<ClientPlayer>());
            //     playerHeapPos.Add(client);
            // }
            // //默认客户端序列0
            // player = clientPlayers[0];
            // player.playerId = response.id;
            // playerId = response.id;
        }

        private void OnEnable()
        {
            TWebSocket.Instance.socket.OnMessage += SocketOnMessage;
            TWebSocket.Instance.socket.OnError += SocketOnError;
            TWebSocket.Instance.socket.OnClose += SocketOnClose;
            TWebSocket.Instance.socket.OnError += SocketOnError;
            //UI
            instance = this;
            loginWin = gameObject.transform.parent.Find("Login").gameObject;
            table = gameObject.transform.Find("Table").gameObject;
            heapPos = table.transform.Find("HeapPos").transform;
            landlord = table.transform.Find("Landlord").gameObject;
            backCard = Resources.Load<GameObject>("Prefab/BackCard");
            
            cardNames = GetCardNames();
            // Debug.Log($"cardNames.ength:{cardNames.Length}");

            startButton = table.transform.Find("StartBtn").GetComponent<Button>();
            landlordButton = table.transform.Find("LandlordBtn").GetComponent<Button>();
            cancelButton = table.transform.Find("CancelBtn").GetComponent<Button>();
            selectCreateRoom = gameObject.transform.Find("Select/CreateRoom").GetComponent<Button>();
            selectJoinRoom = gameObject.transform.Find("Select/JoinRoom").GetComponent<Button>();
            roomNoText = table.transform.Find("RoomId").GetComponent<TMP_Text>();

            joinRoomWinCloseBtn = transform.Find("JoinRoomWin").GetComponent<Button>();
            joinBtn = transform.Find("JoinRoomWin/RoomNumber/JoinBtn").gameObject.GetComponent<Button>();
            roomNumber = transform.Find("JoinRoomWin/RoomNumber").GetComponent<TMP_InputField>();
            gameOver = gameObject.transform.Find("GameOver").gameObject;
            gameOverBtn = gameOver.transform.Find("GameOverCloseBtn").GetComponent<Button>();
            gameBeginBtn = gameObject.transform.Find("GameBeginBtn").GetComponent<Button>();
            exitRoomBtn = table.transform.Find("ExitRoomBtn").GetComponent<Button>();
            startButton.onClick.AddListener(Ready);
            gameOverBtn.onClick.AddListener(CloseSettlement);
            gameBeginBtn.onClick.AddListener(ResetGame);
            selectCreateRoom.onClick.AddListener(SelectCreateRoom);
            selectJoinRoom.onClick.AddListener(OpenJoinRoomWin);
            joinBtn.onClick.AddListener(()=>SendJoinRoom(roomNumber.text));
            landlordButton.onClick.AddListener(LandlordButtonClick);
            cancelButton.onClick.AddListener(CancelLandlordOnClick);
            joinRoomWinCloseBtn.onClick.AddListener(CloseJoinRoomWin);
            exitRoomBtn.onClick.AddListener(ClickExitRoom);
            //牌桌玩家和位置
            // playerHeapPos = new List<Transform>();
            clientPlayers = new List<ClientPlayer>();
            if (selectCreateRoom != null)
            {
                selectCreateRoom.transform.parent.gameObject.SetActive(true);
            }
            for (int i = 0; i < 3; i++)
            {
                var client = table.transform.GetChild(i);
                clientPlayers.Add(client.GetComponent<ClientPlayer>());
                // playerHeapPos.Add(client);
            }
            //默认客户端序列0
            player = clientPlayers[0];
            player.playerId = response.id;
            playerId = response.id;
            //判断是否需要重连
            if (response.isReconnect == 1)
            {
                SendReconnectGame(response.id);
            }
        }

        void Start()
        {
            //测试
            // var xx = Resources.Load<Sprite>("Card/Diamond_0x01");
            // CardInfo cardInfo = new CardInfo(xx.name);
            // Debug.Log($"cardInfo:{cardInfo.cardIndex}--{cardInfo.cardType}");
            //todo 先不做准备流程 直接开始
       
        }
        
        void Update()
        {
        
        }

        private void OnDisable()
        {
            TWebSocket.Instance.socket.OnMessage -= SocketOnMessage;
            TWebSocket.Instance.socket.OnError -= SocketOnError;
            TWebSocket.Instance.socket.OnClose -= SocketOnClose;
            TWebSocket.Instance.socket.OnError -= SocketOnError;
            startButton.onClick.RemoveAllListeners();
            gameOverBtn.onClick.RemoveAllListeners();
            gameBeginBtn.onClick.RemoveAllListeners();
            selectCreateRoom.onClick.RemoveAllListeners();
            selectJoinRoom.onClick.RemoveAllListeners();
            joinBtn.onClick.RemoveAllListeners();
            landlordButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            joinRoomWinCloseBtn.onClick.RemoveAllListeners();
            exitRoomBtn.onClick.RemoveAllListeners();
        }

        #endregion

        #region 显示界面逻辑

        public void ReconnectGame(ResponseReconnectGame data)
        {
            CloseJoinRoomWin();
            //显示玩家信息
            var playerStatus = data.playerstatus.First((status) => player.playerId == status.playerId);
            if (playerStatus != null)
            {
                player.ShowPlayerInfo(data.roomId,playerStatus.playerId,playerStatus.username,playerStatus.seatNo,playerStatus.cardCount);
                Array.Sort(data.playerstatus, (p1, p2) => p1.seatNo.CompareTo(p2.seatNo));
                if (playerStatus.seatNo==1)
                {
                    Array.Sort(data.playerstatus, (p1, p2) => p2.seatNo.CompareTo(p1.seatNo));
                }
                
                data.playerstatus = data.playerstatus.Where(p => p.playerId != player.playerId).ToArray();
                for (int i = 0; i < data.playerstatus.Length; i++)
                {
                    Debug.Log($"clientPlayers[i+1]:{clientPlayers[i+1].name}---{clientPlayers[i+1].isJoinRoom}----{clientPlayers[i+1].playerId}--{clientPlayers[i+1].playerName.text}");
                    clientPlayers[i+1].ShowPlayerInfo(data.roomId,data.playerstatus[i].playerId,data.playerstatus[i].username,data.playerstatus[i].seatNo,playerStatus.cardCount);
                }
            }
            if (table!=null)
            {
                table.SetActive(true);
            }
            if (selectJoinRoom!=null)
            {
                selectJoinRoom.transform.parent.gameObject.SetActive(false);
            }
            if (roomNoText != null)
            {
                roomNoText.text = "房间号:"+data.roomId.ToString();
            }
            Debug.Log("ReconnectGame:"+data.cards.Length);
            //未开局
            if (data.cards.Length == 0)
            {
                if (startButton != null)
                {
                    startButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (startButton != null)
                {
                    startButton.gameObject.SetActive(false);
                }
                if (data.putCardPlayerId == 0 && data.landlordPlayerId == 0)
                {
                    if (landlordButton != null)
                    {
                        landlordButton.gameObject.SetActive(true);
                    }

                    if (cancelButton != null)
                    {
                        cancelButton.gameObject.SetActive(true);
                    }
                }
                //显示自己的手牌
                player.handCards.Clear();
                for (int i = 0; i < data.cards.Length; i++)
                {
                    string hexString = "0x" + data.cards[i].ToString("X2");
                    string cardName = hexString;
                    var cardInfo = new CardInfo(cardName);
                    player.AddCard(cardInfo);
                }
                Debug.Log($"---------handCards-------{player.handCards.Count}");
                ShowPlayerSelfCards();
            }
           
            //显示打出来的牌
            List<CardInfo> cardInfos = new List<CardInfo>();
            if (data.putCardPlayerId != 0)
            {
                var clientPlayer = clientPlayers.First((p) => p.playerId == data.putCardPlayerId);
                for (int j = 0; j < data.tabCards.Length; j++)
                {
                    string hexString = "0x" + data.tabCards[j].ToString("X2");
                    string cardName = hexString;
                    cardInfos.Add(new CardInfo(cardName));
                }
                clientPlayer.ShowOtherPlayPutCards(cardInfos);
            }
            //轮到哪个玩家了
            if (data.activePlayerId != 0)
            {
                var client = clientPlayers.First((p) => p.playerId == data.activePlayerId);
                Debug.Log("p.playerId,"+client.playerName.text+","+client.playerId.ToString());
                if (client.playerId == data.activePlayerId)
                {
                    Debug.Log($"client.playerId == data.putCardPlayerId---{client.playerId}---{data.putCardPlayerId}");
                    if (client.playerId == data.putCardPlayerId)
                    {
                        Debug.Log("client.ShowPutCardsTips:"+1);
                        client.ShowPutCardsTips(1);
                    }
                    else
                    {
                        Debug.Log("client.ShowPutCardsTips:"+2);
                        player.TableCards = cardInfos;
                        client.ShowPutCardsTips(2);
                    }
                }
                else
                {
                    Debug.Log("client.ShowPutCardsTips *******:"+1+","+cardInfos);
 
                    client.ShowPutCardsTips(1);
                }
            }
        }
        public void LoginOut()
        {
            ResetTable();
            if (loginWin!=null)
            {
                loginWin.SetActive(true);
            }
            if (table != null)
            {
                Debug.Log("隐藏桌面 和game");
                table.gameObject.SetActive(false);
                this.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// 重置游戏
        /// </summary>
        public void ResetGame()
        {
            if (gameBeginBtn != null)
            {
                gameBeginBtn.gameObject.SetActive(false);
            }
            ResetTable();
            if (startButton!=null)
            {
                startButton.gameObject.SetActive(true);
            }
            // SendRestartGame(player.playerId);
        }
        /// <summary>
        /// 显示牌桌
        /// </summary>
        /// <param name="data"></param>
        public void ShowTable()
        {
            CloseJoinRoomWin();
            //AudioManager.Instance.PlayBackgroundMusic("Bgm");
            if (table!=null)
            {
                table.SetActive(true);
            }
            if (selectJoinRoom!=null)
            {
                selectJoinRoom.transform.parent.gameObject.SetActive(false);
            }

            if (roomNoText != null)
            {
                roomNoText.text = "房间号:"+roomId.ToString();
            }

            if (player != null)
            {
                player.isJoinRoom = true;
                player.playerName.text = response.username;
            }
        }
        /// <summary>
        /// 显示牌桌
        /// </summary>
        /// <param name="data"></param>
        public void ShowTable(JoinRoomResponse data)
        {
            CloseJoinRoomWin();
            if (table!=null)
            {
                table.SetActive(true);
            }
            if (selectJoinRoom!=null)
            {
                selectJoinRoom.transform.parent.gameObject.SetActive(false);
            }
            if (roomNoText != null)
            {
                roomNoText.text = "房间号:"+data.roomId.ToString();
            }

            if (data.playerstatus.Length == 3)
            {
                startButton.gameObject.SetActive(true);
            }
            var playerStatus = data.playerstatus.First((status) => player.playerId == status.playerId);
            if (playerStatus != null)
            {
                player.ShowPlayerInfo(data.roomId,playerStatus.playerId,playerStatus.username,playerStatus.seatNo);
                Debug.Log($"playerInfo:{player.playerId} playerName:{player.playerName.text},player.isJoinRoom:{player.isJoinRoom}");
                Array.Sort(data.playerstatus, (p1, p2) => p1.seatNo.CompareTo(p2.seatNo));
                if (playerStatus.seatNo==1)
                {
                    Array.Sort(data.playerstatus, (p1, p2) => p2.seatNo.CompareTo(p1.seatNo));
                }
                data.playerstatus = data.playerstatus.Where(p => p.playerId != player.playerId).ToArray();
                for (int i = 0; i < data.playerstatus.Length; i++)
                {
                    Debug.Log($"clientPlayers[i+1]:{clientPlayers[i+1].name}---{clientPlayers[i+1].isJoinRoom}----{clientPlayers[i+1].playerId}--{clientPlayers[i+1].playerName.text}");
                    clientPlayers[i+1].ShowPlayerInfo(data.roomId,data.playerstatus[i].playerId,data.playerstatus[i].username,data.playerstatus[i].seatNo);
                }
            }
        }
        /// <summary>
        /// 重置牌桌
        /// </summary>
        public void ResetTable()
        {
            for (int i = 0; i < clientPlayers.Count; i++)
            {
                clientPlayers[i].DestroyPlayPutCards();
                clientPlayers[i].HidePutCardsTips();
            }
            player.DestroyAllCards();
        }
        /// <summary>
        /// 展示抢地主
        /// </summary>
        public void ShowLandlordCard()
        {
            if (startButton != null)
            {
                startButton.gameObject.SetActive(false);
            }
            if (landlordButton != null)
            {
                landlordButton.transform.gameObject.SetActive(true);
            }

            if (cancelButton != null)
            {
                cancelButton.transform.gameObject.SetActive(true);
            }

            currentState = CardManagerStates.LandlordCall;
        }

        /// <summary>
        /// 抢地主
        /// </summary>
        public void LandlordButtonClick()
        {
            Debug.Log("抢地主---------");
            RequestLandlord landlord = new RequestLandlord()
            {
                opCode = OperationCode.RequestLandlord,
                playerId = player.playerId,
                roomId = this.roomId
            };
            TWebSocket.Instance.Send(landlord);
            if (landlordButton != null)
            {
                landlordButton.transform.gameObject.SetActive(false);
            }

            if (cancelButton != null)
            {
                cancelButton.transform.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// 不抢地主
        /// </summary>
        public void CancelLandlordOnClick()
        {
            DropLandlordCard();
        }
        /// <summary>
        /// 放弃隐藏抢地主按钮
        /// </summary>
        public void DropLandlordCard()
        {
            if (landlordButton != null)
            {
                landlordButton.transform.gameObject.SetActive(false);
            }

            if (cancelButton != null)
            {
                cancelButton.transform.gameObject.SetActive(false);
            }
            SendDropLand();
        }

        public void SendDropLand()
        {
            RequestDropLandlord requestDropLandlord = new RequestDropLandlord()
            {
                opCode = OperationCode.DropLandlord,
                playerId = player.playerId
            };
            TWebSocket.Instance.Send(requestDropLandlord);
        }
        /// <summary>
        /// 展示自己的手牌 其他玩家客户端不用
        /// </summary>
        public void ShowPlayerSelfCards()
        {
            Debug.LogError("展示手牌------------ShowPlayerSelfCards---------");
            player.InitCards();
        }
        public void ClearCards()
        {
            //清空所有玩家卡牌
            clientPlayers.ToList().ForEach(s => s.DropCards());
            //销毁玩家手牌
            clientPlayers.ToList().ForEach(s =>
            {
                if (s != null)
                {
                   s.DestroyAllCards();
                }
            });
        }

        /// <summary>
        /// 显示结算
        /// </summary>
        public void ShowSettlement(SettlementInfo[] settlements)
        {
            if (gameOver != null)
            {
                var list = gameOver.transform.Find("List");
                for (int i = 0; i < list.childCount; i++)
                {
                    var info = list.GetChild(i).Find("Info").GetComponent<TMP_Text>();
                    if (info != null)
                    {
                        info.text = "玩家 " + settlements[i].name + "得分 " + settlements[i].score;
                    }
                }
                gameOver.SetActive(true);
            }
        }
        /// <summary>
        /// 隐藏结算
        /// </summary>
        public void HideSettlement()
        {
            if (gameOver != null)
            {
                var list = gameOver.transform.Find("List");
                for (int i = 0; i < list.childCount; i++)
                {
                    var info = list.GetChild(i).Find("Info").GetComponent<TMP_Text>();
                    if (info != null)
                    {
                        info.text = "";
                    }
                }
                gameOver.SetActive(false);
            }
        }
        /// <summary>
        /// 关闭结算
        /// </summary>
        public void CloseSettlement()
        {
            HideSettlement();
            ResetGame();
            // if (gameBeginBtn != null)
            // {
            //     gameBeginBtn.transform.gameObject.SetActive(true);
            // }
        }

        public void ReturnSelectRoom()
        {
            ResetTable();
            for (int i = 0; i < clientPlayers.Count; i++)
            {
                if (clientPlayers[i].playerId!=player.playerId)
                {
                    clientPlayers[i].RestPlayerInfo();
                }
            }
            if (table != null)
            {
                table.SetActive(false);
            }
            if (selectJoinRoom != null)
            {
                selectJoinRoom.transform.parent.gameObject.SetActive(true);
            }
        }
        /// <summary>
        /// 有人退出房间
        /// </summary>
        /// <param name="responseExitRoom"></param>
        public void ExitRoom(ResponseExitRoom responseExitRoom)
        {
            
            if (responseExitRoom.playerId == player.playerId)
            {
                ReturnSelectRoom();
            }
            else
            {
                //有人离开房间重新准备
                startButton.gameObject.SetActive(true);
                for (int i = 0; i < clientPlayers.Count; i++)
                {
                    if (clientPlayers[i].playerId == responseExitRoom.playerId)
                    {
                        clientPlayers[i].RestPlayerInfo();
                    }
                }
            }
        }
        #endregion
        
        #region 打牌逻辑
        /// <summary>
        /// 发牌
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public IEnumerator DealCards(int[] cards)
        {
            //进入发牌阶段
            player.DestroyAllCards();
            player.handCards.Clear();
            currentState = CardManagerStates.DealCards;
            termCurrentIndex =0;
            yield return DealHeapCards(false,cards);
        }
        /// <summary>
        /// 发牌堆上的牌（如果现在不是抢地主阶段，发普通牌，如果是，发地主牌）
        /// </summary>
        /// <returns></returns>
        private IEnumerator DealHeapCards(bool ifForBid,int[]cards)
        {
            //显示牌堆
            heapPos.gameObject.SetActive(true);
            playerHeapPos.ForEach(s => { s.gameObject.SetActive(true); });
            var cardNames = new string[cards.Length];        
            for (int i = 0; i < cards.Length; i++)
            {
                string hexString = "0x" + cards[i].ToString("X2");
                cardNames[i] = hexString;
            }
            int cardIndex = 0;
            foreach (var cardName in cardNames)
            {
                CardInfo cardInfo = new CardInfo(cardName);
                //给当前玩家发一张牌
                clientPlayers[selfPlayerIndex].AddCard(cardInfo);
                
                var cover = Instantiate(backCard, heapPos.position, Quaternion.identity, heapPos.transform);
                cover.GetComponent<RectTransform>().localScale = Vector3.one;
                //移动动画，动画结束后自动销毁
                var tween = cover.transform.DOMove(playerHeapPos[termCurrentIndex].position, 0.3f);
                tween.OnComplete(() =>
                {
                    Destroy(cover);
                    clientPlayers[termCurrentIndex].ShowCardCount(17);
                });
                yield return new WaitForSeconds(1 / dealCardSpeed);
                SetNextPlayer();
                cardIndex++;
            }

            //隐藏牌堆
            heapPos.gameObject.SetActive(false);
            //发普通牌
            ShowPlayerSelfCards();
            ShowLandlordCard();
        }
        /// <summary>
        /// 下一位发牌者
        /// </summary>
        public void SetNextPlayer()
        {
            termCurrentIndex = (termCurrentIndex + 1) % clientPlayers.Count;
        }
        /// <summary>
        /// 设置地主
        /// </summary>
        /// <param name="bankerIndex"></param>
        /// <param name="cards"></param>
        public void SetLandlord(long bankerId,int[] cards)
        {
            // Debug.Log("SetLandlord");
            this.bankerId = bankerId;
            LoadLandlordCard(cards);
            landlordButton.transform.gameObject.SetActive(false);
            cancelButton.transform.gameObject.SetActive(false);
            var client = clientPlayers.First((x)=>x.playerId == bankerId);
            this.bankerIndex = client.SeatNo;
            client.ShowCardCount(20);
            client.landlordTag.SetActive(true);
            StartFollowing(bankerId);
        }
        /// <summary>
        /// 展示地主牌
        /// </summary>
        /// <param name="cards"></param>
        public void LoadLandlordCard(int[] cards)
        {
            var width = (landlord.GetComponent<RectTransform>().sizeDelta.x - 2) / 3;
            var centerBidPos = Vector3.zero;
            var leftBidPos = centerBidPos - Vector3.left * width;
            var rightBidPos = centerBidPos + Vector3.left * width;
            List<Vector3> bidPoss = new List<Vector3> { leftBidPos, centerBidPos, rightBidPos };
            bool isLanlord = playerId == bankerId;
            // Debug.Log($"playerId---bankerId---{playerId}--{bankerId}");
            for (int i = 0; i < cards.Length; i++)
            {
                string hexString = "0x" + cards[i].ToString("X2");
                string cardName = hexString;
                var bidcard = Instantiate(prefabCard, landlord.transform.TransformPoint(bidPoss[i]), Quaternion.identity, landlord.transform);
                bidcard.transform.localScale = Vector3.one;
                var cardInfo = new CardInfo(cardName);
                bidcard.GetComponent<Card>().InitImage(cardInfo);
                if (isLanlord)
                {
                    player.AddCard(cardInfo);
                }
            }
            if (isLanlord)
            {
                ShowPlayerSelfCards();
            }
        }
        /// <summary>
        /// 出牌阶段
        /// </summary>
        private void StartFollowing(long id)
        {
            currentState = CardManagerStates.Playing;
            //开始出牌流程
            var client = clientPlayers.First((x)=>x.playerId == id);
            client.StartFollowing(false);
        }
        /// <summary>
        /// 轮到下一个玩家出牌
        /// </summary>
        private void NextPlayerPutCard(int seatNo)
        {
            for (int i = 0; i < clientPlayers.Count; i++)
            {
                if (clientPlayers[i].SeatNo == seatNo)
                {
                    clientPlayers[i].ShowPutCardsTips(2);
                }
                else
                {
                    clientPlayers[i].HidePutCardsTips();
                }
            }
        }
        /// <summary>
        /// 服务器校验牌型通过
        /// </summary>
        public void CheckSuccess(long playerId,int seatNo,int[]cards,bool isOver)
        {
            if (player.playerId == playerId)
            {
                player.PutCardsAright();
                if (isOver)
                {
                    Debug.Log("进入结算");
                }
            }
            else
            {
                var clientPlayer = clientPlayers.First((p) => p.SeatNo == seatNo);
                List<CardInfo> cardInfos = new List<CardInfo>();
                for (int i = 0; i < cards.Length; i++)
                {
                    string hexString = "0x" + cards[i].ToString("X2");
                    string cardName = hexString;
                    cardInfos.Add(new CardInfo(cardName));
                }
                player.TableCards = cardInfos;
                clientPlayer.ShowOtherPlayPutCards(cardInfos);
                if (!isOver)
                {
                    int nextSeatNo = seatNo + 1;
                    if (nextSeatNo>2)
                    {
                        nextSeatNo = 0;
                    }
                    NextPlayerPutCard(nextSeatNo);
                }
            }
        }
        #endregion
        
        #region WebSocket服务器的消息
        private void SocketOnOpen(object sender, OpenEventArgs e)
        {
            Debug.Log("Launch---SocketOnOpen--------{add}");
        }
        
        private void SocketOnMessage(object sender, MessageEventArgs e)
        {
            if (e.IsBinary)
            {
                Debug.Log(e.Data);
            }
            else if (e.IsText)
            {
                Debug.Log($"GameManager收到消息--------{e.Data}");
                var json = JObject.Parse(e.Data);
                var opStr = json["reciveCode"].ToString();
                ReciveCode key;
                ReciveCode.TryParse(opStr, out key);
                var errorCode = int.Parse(json["reciveCode"].ToString());
                if (errorCode < 0)
                {
                    ResponseErrorCode ERROR_CODE = JsonConvert.DeserializeObject<ResponseErrorCode>(json.ToString());
                    switch (errorCode)
                    {
                        case -1:
                            Debug.Log($"登录失败:errorCode");
                            break;
                    }
                    Debug.Log($"服务器返回错误,请检测错误码{ERROR_CODE}");
                    return;
                }
                switch (key)
                {
                   case ReciveCode.LoginResponseCode:
                       //这里收到登录表示有人顶号了
                       Debug.Log("有人顶号 注意！！！");
                       LoginResponse loginResponse = JsonConvert.DeserializeObject<LoginResponse>(json.ToString());
                       Debug.Log($"loginResponse-----{loginResponse.username}---{loginResponse.isOk}");
                       if (loginResponse != null&&loginResponse.isOk==0)
                       {
                           LoginOut();
                       }
                       break;
                   case ReciveCode.CreateRoomResponeseCode:
                       ResponseCreateRoom responseCreateRoom = JsonConvert.DeserializeObject<ResponseCreateRoom>(json.ToString());
                       if (responseCreateRoom != null)
                       {
                           this.roomId = responseCreateRoom.roomId;
                           Debug.Log($"创建房间-----------{this.roomId}");
                           ShowTable();
                           currentState = CardManagerStates.Ready;
                       }
                       break;
                   case ReciveCode.JoinRoomResponseCode:
                       JoinRoomResponse jr = JsonConvert.DeserializeObject<JoinRoomResponse>(json.ToString());
                       if (jr != null)
                       {
                           this.roomId = jr.roomId;
                           ShowTable(jr);
                           currentState = CardManagerStates.Ready;
                       }
                       break;
                   case ReciveCode.ReadyResponseCode:
                       ResponseReady responseReady = JsonConvert.DeserializeObject<ResponseReady>(json.ToString());
                       // Debug.Log($"玩家------已经准备--------{json.ToString()}---{responseReady.status[0]}");
                       // startButton.gameObject.SetActive(false);
                       for (int i = 0; i < responseReady.status.Length; i++)
                       {    
                           Debug.Log($"responseReady.status[i]----:{responseReady.status[i].ready}");
                           if (responseReady.status[i].playerId == player.playerId&&responseReady.status[i].ready)
                           {
                               startButton.gameObject.SetActive(false);
                           }
                       }
                       break;
                   case ReciveCode.DealCardsResponseCode:
                       ResponseCards responseCards = JsonConvert.DeserializeObject<ResponseCards>(json.ToString());
                       StartCoroutine(DealCards(responseCards.cards));
                       break;
                   case ReciveCode.LandlordCode:
                       ResponseLandlord responseLandlord = JsonConvert.DeserializeObject<ResponseLandlord>(json.ToString());
                       SetLandlord(responseLandlord.playerId,responseLandlord.cards); ;
                       break;
                   case ReciveCode.PutCardsResponseCode:
                       ResponsePutCards responsePutCards = JsonConvert.DeserializeObject<ResponsePutCards>(json.ToString());
                       if (responsePutCards!=null)
                       {
                           tableCardId = responsePutCards.playerId;
                           CheckSuccess(responsePutCards.playerId,responsePutCards.seatNo,responsePutCards.cards,responsePutCards.isOver);
                       }
                       else
                       {
                           Debug.LogError("出牌的时候服务器返回出牌出错");
                       }
                       break;
                   case ReciveCode.PassResponseCode:
                       PassResponse passResponse = JsonConvert.DeserializeObject<PassResponse>(json.ToString());
                       if (passResponse!=null)
                       {
                           for (int i = 0; i < clientPlayers.Count; i++)
                           {
                               if (clientPlayers[i].playerId == passResponse.nextPlayerId)
                               {
                                   Debug.Log($"clientPlayers[i].playerId,passResponse.nextPlayerId,passResponse.state--{clientPlayers[i].playerId}---{passResponse.nextPlayerId}---{passResponse.state}");
                                   clientPlayers[i].ShowPutCardsTips(passResponse.state);
                               }
                               else
                               {
                                   clientPlayers[i].HidePutCardsTips();
                               }
                           }
                       }
                       break;
                   case ReciveCode.ExitRoomResponseCode:
                       ResponseExitRoom responseExitRoom = JsonConvert.DeserializeObject<ResponseExitRoom>(json.ToString());
                       if (responseExitRoom != null)
                       {
                           if (responseExitRoom.isDisband == 1)
                           {
                               ReturnSelectRoom();
                           }
                           else
                           {
                               ExitRoom(responseExitRoom);
                           }
                       }
                       break;
                   case ReciveCode.ExitGameResponseCode:
                       ResponseExitGame responseExitGame = JsonConvert.DeserializeObject<ResponseExitGame>(json.ToString());
                       if (responseExitGame != null)
                       {
                           Debug.Log("这里要加入机器人代打");
                           if (player.playerId == responseExitGame.playerId)
                           {
                               LoginOut();
                           }
                       }
                       break;
                    case ReciveCode.SettlementResponseCode:
                        SettlementResponse settlementResponse = JsonConvert.DeserializeObject<SettlementResponse>(json.ToString());
                        if (settlementResponse != null)
                        {
                            ShowSettlement(settlementResponse.settlementInfos);
                        }
                        break;
                    case ReciveCode.ReconnectResponseCode:
                        ResponseReconnectGame responseReconnectGame = JsonConvert.DeserializeObject<ResponseReconnectGame>(json.ToString());
                        if (responseReconnectGame != null)
                        {
                            ReconnectGame(responseReconnectGame);
                        }
                        break;
                }
            }
        }
                
        private void SocketOnClose(object sender, CloseEventArgs e)
        {
            Debug.Log(string.Format("Closed: StatusCode: {0}, Reason: {1}", e.StatusCode, e.Reason));
            if (e.StatusCode==CloseStatusCode.Abnormal)
            {
                
            }
        }
        
        private void SocketOnError(object sender, ErrorEventArgs e)
        {
            Debug.Log(string.Format("Error: {0}", e.Message));
        }
        #endregion

        #region 发送消息
        /// <summary>
        /// 创建房间发送
        /// </summary>
        public void SendCreateRoom()
        {
            RequestCreateRoom request = new RequestCreateRoom()
            {
                opCode = OperationCode.CreateRoom,
                playerId = response.id,
            };
            TWebSocket.Instance.Send(request);
        }
        /// <summary>
        /// 加入房间
        /// </summary>
        /// <param name="index"></param>
        public void SendJoinRoom(string roomId)
        {
            //Debug.Log(response.playerId);
            JoinRoomRequest request = new JoinRoomRequest()
            {
                opCode = OperationCode.JoinRoom,
                playerId =response.id,
                roomId = int.Parse(roomId)
            };
            JoinRoom(request);
        }
        /// <summary>
        /// 发送准备
        /// </summary>
        public void Ready()
        {
            RequestReady ready = new RequestReady()
            {
                opCode = OperationCode.Ready,
                playerId = response.id,
                roomId = this.roomId
            };
            TWebSocket.Instance.Send(ready);
        }
        /// <summary>
        /// 出牌服务器校验
        /// </summary>
        /// <param name="cardGroupType"></param>
        /// <param name="cards"></param>
        public void SendOutCard(CardGroupType cardGroupType,int[]cards,long id,int seatIdnex)
        {
            RequestPutCard requestPutCard = new RequestPutCard()
            {
                opCode = OperationCode.PutCards,
                cardGroup = cardGroupType,
                roomId = this.roomId,
                cards = cards,
                playerId = id,
                seatNo = seatIdnex
            };
            TWebSocket.Instance.Send(requestPutCard);
        }
        /// <summary>
        /// 过牌
        /// </summary>
        public void SendPass(long id,int seatIdnex)
        {
            PassRequest  passRequest = new PassRequest()
            {
                opCode = OperationCode.Pass,
                playerId = id,
                tableCardId = this.tableCardId,
                seatNo = seatIdnex
            };
            TWebSocket.Instance.Send(passRequest);
        }
        /// <summary>
        /// 重开游戏
        /// </summary>
        /// <param name="id"></param>
        public void SendRestartGame(long id)
        {
            RestartGameRequest  restartGameRequest = new RestartGameRequest()
            {
                opCode = OperationCode.RestartGame,
                playerId = id,
            };
            TWebSocket.Instance.Send(restartGameRequest);
        }
        /// <summary>
        /// 未开牌退出房间
        /// </summary>
        /// <param name="id"></param>
        public void SendExitRoom(long id)
        {
            RequestExitRoom exitRoomRequest = new RequestExitRoom()
            {
                opCode = OperationCode.ExitRoom,
                roomId = this.roomId,
                playerId = player.playerId,
            };
            TWebSocket.Instance.Send(exitRoomRequest);
        }
        /// <summary>
        /// 房间中退出游戏
        /// </summary>
        public void SendExitGame()
        {
            RequestExitGame exitRoomRequest = new RequestExitGame()
            {
                opCode = OperationCode.ExitGame,
                roomId = player.roomId,
                playerId = player.playerId,
            };
            TWebSocket.Instance.Send(exitRoomRequest);
        }

        public void SendReconnectGame(long id)
        {
            RequestReconnectGame requestReconnectGame = new RequestReconnectGame()
            {
                opCode = OperationCode.Reconnect,
                playerId = id
            };
            TWebSocket.Instance.Send(requestReconnectGame);
        }
        /// <summary>
        /// 智能牌组
        /// </summary>
        public List<List<CardInfo>> AgentCardGroup(List<CardInfo> cardInfos,List<CardInfo>tableCards)
        {
            return null;
        }
        /// <summary>
        //获取所有可能的牌组
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public List<List<CardInfo>> GetAllCardGroups(List<CardInfo> cards)
        {
            List<List<int>> allGroups = new List<List<int>>();
            return null;
        }
        #endregion
    }
}