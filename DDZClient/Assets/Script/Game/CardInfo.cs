using System;
using UnityEngine;
using Random = System.Random;

namespace Script.Game
{     
    public class CardInfo:IComparable
    {
        //16进制枚举所有的牌型
        public int[] allCards =
        {
            0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,0x0B,0x0C,0x0D,//	--方块 A - K
            0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A,0x1B,0x1C,0x1D,//	--梅花 A - K
            0x21,0x22,0x23,0x24,0x25,0x26,0x27,0x28,0x29,0x2A,0x2B,0x2C,0x2D,//	--红桃 A - K
            0x31,0x32,0x33,0x34,0x35,0x36,0x37,0x38,0x39,0x3A,0x3B,0x3C,0x3D,//	--黑桃 A - K
            0x4E,0x4F
        };
        public ushort Card;
        public int[] cardValues = {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15};
        //扑克牌名称
        public string cardName;
        //牌值
        public int cardValue;
        //花色 1方块 2 梅花 3 红桃 4 黑桃 5大小王
        public int cardType;
        //索引
        public int cardIndex;
        //是否选中
        public bool isSelected;

        public CardInfo(string cardName)
        {
            //所有牌都是按照allCards 规则命令的
            var x_10 = Convert.ToInt32(cardName,16);
            // Debug.Log($"X_10---------CardName:{x_10},{cardName}");
            this.cardIndex = x_10;
            if (cardIndex < 77)
            {
                this.cardType = GetType(x_10);
                var valueIndex = GetValueIndex(x_10,this.cardType);
                // Debug.Log($"valueIndex----x_10:{valueIndex},{x_10}");
                this.cardValue = cardValues[valueIndex];
            }
            else
            {
                this.cardType = 5;
                if (cardIndex == 78)
                {
                    this.cardValue = cardValues[13];
                }
                else if (cardIndex == 79)
                {
                    this.cardValue = cardValues[14];
                }
            }
            this.cardName = cardName;
        }
        /// <summary>
        /// 获取花色
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetType(int index)
        { 
            // 使用模运算快速判断
            if (index >= 1 && index <= 14)
            {
                return 1;  // 方块 A - K
            }
            else if (index >= 17 && index <= 29)
            {
                return 2;  // 梅花 A - K
            }
            else if (index >= 33 && index <= 45)
            {
                return 3;  // 红桃 A - K
            }
            else if (index >= 49 && index <= 61)
            {
                return 4;  // 黑桃 A - K
            }
            else
            {
                return 5;  // 小王、大王
            }
        }

        public int GetValueIndex(int index,int type)
        {
            switch (type)
            {
                case 1:
                    return index - 1;
                case 2:
                case 3:
                case 4:
                    return (1 - type) * 16 + index - 1;
                case 5:
                    if (index==78)
                    {
                        return 13;
                    }
                    else
                    {
                        return 14;
                    }
            }

            return -1;
        }
        //卡牌大小比较
        public int CompareTo(object obj)
        {
            CardInfo other = obj as CardInfo;

            if (other == null)
                throw new Exception("比较对象类型非法！");

            //如果当前是大小王
            if (cardType == 5)
            {
                //对方也是大小王
                if (other.cardType == 5)
                {
                    return cardIndex.CompareTo(other.cardIndex);
                }
                //对方不是大小王
                return 1;
            }
            //如果是一般的牌
            else
            {
                //对方是大小王
                if (other.cardType == 5)
                {
                    return -1;
                }
                //如果对方也是一般的牌
                else
                {
                    //计算牌力
                    if (cardIndex == other.cardIndex)
                    {
                        return -cardType.CompareTo(other.cardType);
                    }
                    return cardIndex.CompareTo(other.cardIndex);
                }
            }
        }
    }
    public enum CardType
    {
        //方块
        Diamonds,
        //梅
        Clubs,
        //红
        Hearts,
        //黑
        Spades,
        //大小王
        Joker
    }
    public enum CardManagerStates
    {
        /// <summary>
        /// 准备阶段
        /// </summary>
        Ready,
        /// <summary>
        /// 发牌
        /// </summary>
        DealCards,
        /// <summary>
        /// 叫地主
        /// </summary>
        LandlordCall,
        /// <summary>
        /// 打牌阶段
        /// </summary>
        Playing,
        /// <summary>
        /// 结算阶段
        /// </summary>
        Settlement
        
    }
}