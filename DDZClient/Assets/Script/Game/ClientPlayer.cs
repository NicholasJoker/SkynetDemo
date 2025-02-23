using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Script.Game
{
    public class ClientPlayer : MonoBehaviour
    {
        public long playerId = -1;
        public int roomId = -1;
        /// <summary>
        /// 牌桌的座位号
        /// </summary>
        private int seatNo;
        public int SeatNo
        {
            get
            {
                return seatNo;
            }
            set
            {
                seatNo = value;
            }
        }
        /// <summary>
        /// 手牌
        /// </summary>
        public List<CardInfo> handCards = new List<CardInfo>();
        /// <summary>
        /// 当前的牌组的
        /// </summary>
        private List<CardInfo> currentCard;
        /// <summary>
        /// 手牌
        /// </summary>
        private List<Card> cards = new List<Card>();
    
        /// <summary>
        /// 叫地主
        /// </summary>
        private Button landlordCallBtn;
        /// <summary>
        /// 抢地主
        /// </summary>
        private Button playTipsBtn;
        /// <summary>
        /// 准备
        /// </summary>
        public Button readyBtn;

        /// <summary>
        /// 出牌提示
        /// </summary>
        public Button putTisBtn;
        /// <summary>
        /// 牌的预制
        /// </summary>
        public GameObject cardPrefab;
        /// <summary>
        /// 牌的初始位置
        /// </summary>
        private Transform originPos1;
        /// <summary>
        /// 牌的初始位置
        /// </summary>
        /// <returns></returns>
        private Transform originPos2;
        /// <summary>
        /// 出牌位置
        /// </summary>
        public Transform putOutCards;
    
        /// <summary>
        /// 出牌
        /// </summary>
        public Button putOutCardBtn;

        public Button passBtn;
        public TMP_Text cardCount;
        /// <summary>
        /// 打出的牌
        /// </summary>
        private List<GameObject> outCards = new List<GameObject>();
        
        private List<CardInfo> tableCards = new List<CardInfo>();

        public List<CardInfo> TableCards
        {
            get
            {
                return tableCards;
            }
            set
            {
                tableCards = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public TMP_Text playerName;

        /// <summary>
        /// 地主标志
        /// </summary>
        public GameObject landlordTag;
        /// <summary>
        /// 出牌
        /// </summary>
        public GameObject putCardTag;
        
        public GameObject handleCardObj;
    
        public TMP_Text seatNoText;
        /// <summary>
        /// 地主牌能选
        /// </summary>
        private bool canSelectCard;

        /// <summary>
        /// 是否已经加入房间
        /// </summary>
        public bool isJoinRoom = false;

        private void Awake()
        {
            originPos1 = gameObject.transform.Find("originPos1");
            originPos2 = gameObject.transform.Find("originPos2");
            playerName = gameObject.transform.Find("PlayerName").GetComponent<TMP_Text>();
            landlordTag = gameObject.transform.Find("LandlordTag").gameObject;
            if (putOutCardBtn != null)
            {
                putOutCardBtn.onClick.AddListener(PutOutCards);
            }

            if (passBtn != null)
            {
                passBtn.onClick.AddListener(PassCards);
            }
        }

        void Start()
        {
        
        }
        // Update is called once per frame
        void Update()
        {
        
        }

        #region UI显示
        /// <summary>
        /// 其他玩家加入房间的展示
        /// </summary>
        public void ShowPlayerInfo(int roomId,long playerId,string playerName,int seatNo,int cardCount = 0,string iconUrl = "")
        {
            isJoinRoom = true;
            this.gameObject.SetActive(true);
            this.playerId = playerId;
            this.roomId = roomId;
            this.playerName.text = playerName;
            this.SeatNo = seatNo;
            Debug.Log($"SeatNo: {seatNo}");
            this.seatNoText.text = seatNo.ToString();
            if (this.cardCount != null)
            {
                this.cardCount.gameObject.SetActive(true);
                if (cardCount > 0)
                {
                    this.cardCount.text = "";
                }
                else
                {
                    this.cardCount.text = "";
                }
            }
        }
        /// <summary>
        /// 开始
        /// </summary>
        public void StartFollowing(bool isFollow)
        {
            if (putCardTag != null)
            {
                putCardTag.gameObject.SetActive(true);
            }
        }

        public void ShowCardCount(int cardCount)
        {
            if (this.cardCount != null)
            {
                this.cardCount.text = cardCount.ToString();
            }
            
        }
        
        /// <summary>
        /// 显示出牌提示
        /// </summary>
        /// <param name="state"></param>
        public void ShowPutCardsTips(int state)
        {
            //刪除展示的牌型
            DestroyPlayPutCards();
            if (state == 1)
            {
                tableCards.Clear();
                if (putCardTag != null)
                {
                    putCardTag.gameObject.SetActive(true);
                }
                if (passBtn != null)
                {
                    passBtn.gameObject.SetActive(false);
                }

                if (putTisBtn != null)
                {
                    putTisBtn.gameObject.SetActive(false);
                }
            }
            if (state == 2)
            {
                if (putCardTag != null)
                {
                    putCardTag.gameObject.SetActive(true);
                }
                if (passBtn != null)
                {
                    passBtn.gameObject.SetActive(true);
                }
                if (putTisBtn != null)
                {
                    putTisBtn.gameObject.SetActive(true);
                }
            }
        }
        /// <summary>
        /// 隱藏出牌提示
        /// </summary>
        public void HidePutCardsTips()
        {
            if (putCardTag != null)
            {
                putCardTag.gameObject.SetActive(false);
            }
            if (passBtn != null)
            {
                passBtn.gameObject.SetActive(false);
            }

            if (putTisBtn != null)
            {
                putTisBtn.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// 显示他人出的牌
        /// </summary>
        /// <param name="otherCards"></param>
        public void ShowOtherPlayPutCards(List<CardInfo> otherCards)
        {
            for (int i = 0; i < otherCards.Count(); i++)
            {
                var card = Instantiate(cardPrefab, putOutCards.position + Vector3.right * 12 * i, Quaternion.identity, putOutCards.transform);
                card.GetComponent<RectTransform>().localScale = Vector3.one * 0.75f;
                card.GetComponent<Image>().sprite = Resources.Load("Card/" + otherCards[i].cardName, typeof(Sprite)) as Sprite;
                card.transform.SetAsLastSibling();
            }
            if (cardCount != null)
            {
                int count = int.Parse(cardCount.text) - otherCards.Count;
                cardCount.text = count.ToString();
            }
        }
        /// <summary>
        /// 销毁打出去的牌
        /// </summary>
        public void DestroyPlayPutCards()
        {
            if (putOutCards!=null)
            {
                for (int i = 0; i < putOutCards.transform.childCount; i++)
                {
                    var child = putOutCards.GetChild(i).gameObject;
                    Destroy(child);
                }
            }
        }
        /// <summary>
        /// 重置信息
        /// </summary>
        public void RestPlayerInfo()
        {
            this.gameObject.SetActive(false);
            this.playerId = -1;
            this.roomId = -1;
            this.playerName.text = "";
            this.SeatNo = -1;
            this.seatNoText.text = "";
            if (cardCount != null)
            {
                cardCount.text = "";
            }
            handCards.Clear();
        }
        #endregion
    
        #region 逻辑
        /// <summary>
        /// 增加一张牌
        /// </summary>
        public void AddCard(CardInfo cardInfo)
        {
            handCards.Add(cardInfo);
        }
        /// <summary>
        /// 清空所有卡片
        /// </summary>
        public void DropCards()
        {
            handCards.Clear();
        }
        /// <summary>
        /// 销毁所有卡牌对象
        /// </summary>
        public void DestroyAllCards()
        {
            if (cards!=null)
            {
                cards.Clear();
                Debug.Log($"clientPlayer-----{name}");
                for (int i = 0; i < handleCardObj.transform.childCount; i++)
                {
                    Destroy(handleCardObj.transform.GetChild(i).gameObject);
                }
            }
        }
        /// <summary>
        /// 如果是玩家初始化自己的牌
        /// </summary>
        public void InitCards()
        {
            Debug.LogError($"InitCards-------------------------{handCards.Count}");
            DestroyAllCards();
            //根据牌值排序
            handCards.Sort((x,y)=>x.cardValue.CompareTo(y.cardValue));
            handCards.Reverse();
            //计算每张牌的偏移
            var offsetX = originPos2.position.x - originPos1.position.x;
            //获取最左边的起点
            int leftCount = (handCards.Count / 2);
            var startPos = originPos1.position + Vector3.left * offsetX * leftCount;

            for (int i = 0; i < handCards.Count; i++)
            {
                //生成卡牌
                var card = Instantiate(cardPrefab, originPos1.position, Quaternion.identity, handleCardObj.transform);
                card.GetComponent<RectTransform>().localScale = Vector3.one;
                card.GetComponent<Card>().InitImage(handCards[i]);
                card.transform.SetAsLastSibling();
                cards.Add(card.GetComponent<Card>());
                //动画移动
                var tween = card.transform.DOMoveX(startPos.x + offsetX * i, 1f);
                if (i == handCards.Count - 1) //最后一张动画
                {
                    tween.OnComplete(() => { canSelectCard = true; });
                }
            }
        }
        /// <summary>
        /// 出牌
        /// </summary>
        public void PutOutCards()
        {
            //选择的牌，添加到出牌区域
            var selectedCards = handCards.Where(s => s.isSelected).ToList();
            if (selectedCards.Count > 0)
            {
                var cardGroup = LandlordGame.GetCardType(selectedCards);
                Debug.Log("tableCards.Count"+tableCards.Count);
                if (tableCards.Count > 0&&cardGroup!=CardGroupType.CT_ERROR)
                {
                    bool compareResult = LandlordGame.CompareGroupValue(tableCards,selectedCards);
                    Debug.Log($"牌型比较的结果--{compareResult}");
                    if (compareResult)
                    {
                        if (putCardTag!=null)
                        {
                            putCardTag.SetActive(false);
                        }
                        OutCards(selectedCards);
                        var cardGroupType = LandlordGame.GetCardType(selectedCards);
                        HideOrShowCards(selectedCards,false);
                        int[] cardIndexes = selectedCards.Select(card => card.cardIndex).ToArray();
                        GameManager.Instance.SendOutCard(cardGroupType, cardIndexes,playerId,seatNo);
                    }
                    else
                    {
                        var cancelSelect = cards.Where(card=>selectedCards.Any(select=>select.cardIndex==card.cardInfo.cardIndex)).ToList();
                        for (int i = 0; i < cancelSelect.Count; i++)
                        {
                            cancelSelect[i].OnButtonClick();
                        }
                        Debug.Log("要不起！！！！");
                    }
                }
                if (tableCards.Count ==0&&cardGroup!=CardGroupType.CT_ERROR)
                {
                    if (putCardTag!=null)
                    {
                        putCardTag.SetActive(false);
                    }
                    OutCards(selectedCards);
                    var cardGroupType = LandlordGame.GetCardType(selectedCards);
                    HideOrShowCards(selectedCards,false);
                    int[] cardIndexes = selectedCards.Select(card => card.cardIndex).ToArray();
                    GameManager.Instance.SendOutCard(cardGroupType, cardIndexes,playerId,seatNo);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void OutCards(List<CardInfo>selectedCards)
        {
            var offset = 10;
            for (int i = 0; i < selectedCards.Count(); i++)
            {
                var card = Instantiate(cardPrefab, putOutCards.position + Vector3.right * offset * i, Quaternion.identity, putOutCards.transform);
                card.GetComponent<RectTransform>().localScale = Vector3.one * 0.75f;
                card.GetComponent<Image>().sprite = Resources.Load("Card/" + selectedCards[i].cardName, typeof(Sprite)) as Sprite;
                card.transform.SetAsLastSibling();
                outCards.Add(card);
            }
        }
        /// <summary>
        /// 过牌
        /// </summary>
        public void PassCards()
        {
            GameManager.Instance.SendPass(playerId,seatNo);
        }
        /// <summary>
        /// 打出去的手牌先隐藏
        /// </summary>
        /// <param name="cardInfos"></param>
        public void HideOrShowCards(List<CardInfo> cardInfos,bool tf)
        {
            for (int i = 0; i < cardInfos.Count; i++)
            {
                var hideCard = cards.First((card) => card.cardInfo.cardIndex == cardInfos[i].cardIndex);
                if (hideCard!=null)
                {
                    hideCard.gameObject.SetActive(tf);
                }
            }
        }
        /// <summary>
        /// 正确的出牌
        /// </summary>
        public void PutCardsAright()
        {
            if (putCardTag != null)
            {
                putCardTag.SetActive(false);
            }

            if (passBtn!=null)
            {
                passBtn.gameObject.SetActive(false);
            }

            if (putTisBtn!=null)
            {
                passBtn.gameObject.SetActive(false);
            }
            handCards = handCards.Where(s => !s.isSelected).ToList();
            //重新组牌
            InitCards();
        }
        #endregion

        #region 牌组
        #endregion
    }
}
