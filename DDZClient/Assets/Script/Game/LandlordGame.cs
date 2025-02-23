using System;
using System.Collections.Generic;
using Script.Game;
using UnityEngine;

public enum CardGroupType
{
    CT_ERROR = 0,
    CT_SINGLE = 1,
    CT_DOUBLE = 2,
    CT_THREE = 3,
    CT_BOMB_CARD = 4,
    CT_THREE_TAKE_ONE = 5,
    CT_THREE_TAKE_TWO = 6,
    CT_FOUR_TAKE_ONE = 7,
    CT_FOUR_TAKE_TWO = 8,
    CT_THREE_LINE = 9,
    CT_DOUBLE_LINE = 10,
    CT_SINGLE_LINE = 11,
    CT_MISSILE_CARD = 12
}

public class LandlordGame
{
    /// <summary>
    /// 获取花色
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public static int GetSuit(int card)
    {
        return card / 0x10;
    }
    
    /// <summary>
    /// 点数
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public static int GetRank(int card)
    {
        return card % 0x10;
    }
    
    public static int GetRankLevel(int card)
    {
        int rank = GetRank(card);
        if (rank < 3)
        {
            return 1;
        }
        if (rank > 13)
        {
            return 2;
        }
        return 0;
    }
    
    /// <summary>
    /// 比较牌值
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool CompareCardValue(int v1, int v2)
    {

        int level1 = GetRankLevel(v1);
        int level2 = GetRankLevel(v2);
        Debug.Log($"比较---------v1,v2,level1,level2,{v1},{v2},{level1},{level2}");
        if (level1 > level2)
        {
            return true;
        }
        if (level1 == level2)
        {
            return GetRank(v1) > GetRank(v2);
        }
        return false;
    }
    /// <summary>
    /// 比较点数
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool CompareCardRank(int a, int b)
    {
        return GetRank(a) < GetRank(b);
    }
    /// <summary>
    /// 获取牌型
    /// </summary>
    /// <param name="cardsData"></param>
    /// <returns></returns>
    public static CardGroupType GetCardType(List<CardInfo> cardsData)
    {
        int cardsCount = cardsData.Count;
        if (cardsCount == 0)
        {
            return CardGroupType.CT_ERROR;
        }
        else if (cardsCount == 1)
        {
            return CardGroupType.CT_SINGLE;
        }
        else if (cardsCount == 2)
        {
            if (IsMissile(cardsData))
            {
                return CardGroupType.CT_MISSILE_CARD;
            }
            else if (IsDouble(cardsData))
            {
                return CardGroupType.CT_DOUBLE;
            }
            return CardGroupType.CT_ERROR;
        }
        else if (cardsCount == 3)
        {
            if (IsThree(cardsData))
            {
                return CardGroupType.CT_THREE;
            }
        }
        else if (cardsCount == 4)
        {
            if (IsBomb(cardsData))
            {
                return CardGroupType.CT_BOMB_CARD;
            }
            else if (IsThreeTakeOne(cardsData))
            {
                return CardGroupType.CT_THREE_TAKE_ONE;
            }
        }
        else if (cardsCount >= 5)
        {
            if (cardsCount == 5 && IsThreeTakeTwo(cardsData))
            {
                return CardGroupType.CT_THREE_TAKE_TWO;
            }
            else if (cardsCount == 6 && IsFourTakeTwo(cardsData))
            {
                return CardGroupType.CT_FOUR_TAKE_ONE;
            }
            else if (cardsCount == 8 && IsFourTakeTwoDouble(cardsData))
            {
                return CardGroupType.CT_FOUR_TAKE_TWO;
            }

            if (cardsCount >= 8 && IsThreeLine(cardsData))
            {
                return CardGroupType.CT_THREE_LINE;
            }
            if (cardsCount > 5 && cardsCount % 2 == 0 && IsDoubleLine(cardsData))
            {
                return CardGroupType.CT_DOUBLE_LINE;
            }
            if (IsSingleLine(cardsData))
            {
                return CardGroupType.CT_SINGLE_LINE;
            }
        }
        return CardGroupType.CT_ERROR;
    }
    public static bool IsDouble(List<CardInfo> cardsData)
    {
        return GetRank(cardsData[0].cardIndex) == GetRank(cardsData[1].cardIndex);
    }
    public static bool IsMissile(List<CardInfo> cardsData)
    {
        return (cardsData[0].cardIndex == 0x4E && cardsData[1].cardIndex == 0x4F) || (cardsData[0].cardIndex == 0x4F && cardsData[1].cardIndex == 0x4E);
    }
    public static bool IsThree(List<CardInfo> cardsData)
    {
        return GetRank(cardsData[0].cardIndex) == GetRank(cardsData[1].cardIndex) && GetRank(cardsData[0].cardIndex) == GetRank(cardsData[2].cardIndex);
    }
    public static bool IsBomb(List<CardInfo> cardsData)
    {
        return GetRank(cardsData[0].cardIndex) == GetRank(cardsData[1].cardIndex) &&
               GetRank(cardsData[0].cardIndex) == GetRank(cardsData[2].cardIndex) &&
               GetRank(cardsData[0].cardIndex) == GetRank(cardsData[3].cardIndex);
    }
    public static bool IsThreeTakeOne(List<CardInfo> cardsData)
    {
        cardsData.Sort((a, b) => CompareCardRank(a.cardIndex, b.cardIndex) ? -1 : 1);
        if (IsBomb(cardsData)) return false;

        if (GetRank(cardsData[0].cardIndex) == GetRank(cardsData[1].cardIndex) && GetRank(cardsData[0].cardIndex) == GetRank(cardsData[2].cardIndex))
        {
            return true;
        }
        else if (GetRank(cardsData[1].cardIndex) == GetRank(cardsData[2].cardIndex) && GetRank(cardsData[1].cardIndex) == GetRank(cardsData[3].cardIndex))
        {
            return true;
        }
        return false;
    }
    public static bool IsThreeTakeTwo(List<CardInfo> cardsData)
    {
        cardsData.Sort((a, b) => CompareCardRank(a.cardIndex, b.cardIndex) ? -1 : 1);
        if (GetRank(cardsData[0].cardIndex) == GetRank(cardsData[1].cardIndex) && GetRank(cardsData[0].cardIndex) == GetRank(cardsData[2].cardIndex))
        {
            if (GetRank(cardsData[3].cardIndex) == GetRank(cardsData[4].cardIndex))
            {
                return true;
            }
        }
        else if (GetRank(cardsData[2].cardIndex) == GetRank(cardsData[3].cardIndex) && GetRank(cardsData[2].cardIndex) == GetRank(cardsData[4].cardIndex))
        {
            if (GetRank(cardsData[0].cardIndex) == GetRank(cardsData[1].cardIndex))
            {
                return true;
            }
        }
        return false;
    }
    public static bool IsThreeLine(List<CardInfo> cardsData)
    {
    cardsData.Sort((a, b) => CompareCardRank(a.cardIndex, b.cardIndex) ? -1 : 1);

    int length = cardsData.Count;
    if (length < 6)
    {
        return false;
    }

    List<int> threeLine = new List<int>();
    List<int> remainingCards = new List<int>();
    int i = 0;
    while (i <= length - 3)
    {
        if (GetRank(cardsData[i].cardIndex) == GetRank(cardsData[i + 1].cardIndex) && GetRank(cardsData[i].cardIndex) == GetRank(cardsData[i + 2].cardIndex))
        {
            threeLine.Add(GetRank(cardsData[i].cardIndex));
            i += 3;
        }
        else
        {
            remainingCards.Add(cardsData[i].cardIndex);
            i += 1;
        }
    }

    if (threeLine.Count == 0)
    {
        return false;
    }
    foreach (var value in threeLine)
    {
        if (value == 2)
        {
            return false;
        }
    }

    int remainingCount = remainingCards.Count;
    int threeLineCount = threeLine.Count;

    // 1. The number of remaining cards cannot exceed twice the count of threeLine
    if (remainingCount > threeLineCount * 2)
    {
        return false;
    }

    // 2. Check if the remaining count is odd or even
    bool isOdd = remainingCount % 2 == 1;

    // 3. If the remaining count is odd, it must be less than threeLine count
    if (isOdd && remainingCount >= threeLineCount)
    {
        return false;
    }
    if (!isOdd)
    {
        if (remainingCount > threeLineCount * 2)
        {
            return false;
        }

        Dictionary<int, int> pairCount = new Dictionary<int, int>();
        foreach (var card in remainingCards)
        {
            int rank = GetRank(card);
            if (!pairCount.ContainsKey(rank))
            {
                pairCount[rank] = 0;
            }
            pairCount[rank]++;
        }
        foreach (var count in pairCount.Values)
        {
            if (count != 2)
            {
                return false;
            }
        }
    }

    return true;
    }
    // public static bool IsThreeLine(List<CardInfo> cardsData)
    // {
    //     cardsData.Sort((a, b) => CompareCardRank(a.cardIndex, b.cardIndex) ? -1 : 1);
    //     int length = cardsData.Count;
    //     if (length < 6)
    //     {
    //         return false;
    //     }
    //
    //     List<int> threeLine = new List<int>();
    //     List<CardInfo> remainingCards = new List<CardInfo>();
    //     int i = 0;
    //
    //     while (i <= length - 3)
    //     {
    //         if (GetRank(cardsData[i].cardIndex) == GetRank(cardsData[i + 1].cardIndex) && GetRank(cardsData[i].cardIndex) == GetRank(cardsData[i + 2].cardIndex))
    //         {
    //             threeLine.Add(GetRank(cardsData[i].cardIndex));
    //             i += 3;
    //         }
    //         else
    //         {
    //             remainingCards.Add(cardsData[i]);
    //             i++;
    //         }
    //     }
    //
    //     if (threeLine.Count == 0)
    //     {
    //         return false;
    //     }
    //
    //     int remainingCount = remainingCards.Count;
    //     int threeLineCount = threeLine.Count;
    //
    //     if (remainingCount % threeLineCount != 0)
    //     {
    //         return false;
    //     }
    //
    //     var groupCount = new Dictionary<int, int>();
    //     foreach (var card in remainingCards)
    //     {
    //         if (!groupCount.ContainsKey(GetRank(card.cardIndex)))
    //         {
    //             groupCount[GetRank(card.cardIndex)] = 0;
    //         }
    //         groupCount[GetRank(card.cardIndex)]++;
    //     }
    //
    //     foreach (var count in groupCount.Values)
    //     {
    //         if (count != 1 && count != 2)
    //         {
    //             return false;
    //         }
    //     }
    //
    //     return true;
    // }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cardsData"></param>
    /// <returns></returns>
    public static bool IsFourTakeTwo(List<CardInfo> cardsData)
    {
        cardsData.Sort((a, b) => CompareCardRank(a.cardIndex, b.cardIndex) ? -1 : 1);

        if (GetRank(cardsData[0].cardIndex) == GetRank(cardsData[1].cardIndex) && GetRank(cardsData[0].cardIndex) == GetRank(cardsData[2].cardIndex) && GetRank(cardsData[0].cardIndex) == GetRank(cardsData[3].cardIndex))
        {
            return true;
        }
        else if (GetRank(cardsData[2].cardIndex) == GetRank(cardsData[3].cardIndex) && GetRank(cardsData[2].cardIndex) == GetRank(cardsData[4].cardIndex) && GetRank(cardsData[2].cardIndex) == GetRank(cardsData[5].cardIndex))
        {
            return true;
        }
        else if (GetRank(cardsData[1].cardIndex) == GetRank(cardsData[2].cardIndex) && GetRank(cardsData[3].cardIndex) == GetRank(cardsData[4].cardIndex) && GetRank(cardsData[2].cardIndex) == GetRank(cardsData[3].cardIndex))
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// 四带一对
    /// </summary>
    /// <param name="cardsData"></param>
    /// <returns></returns>
    public static bool IsFourTakeTwoDouble(List<CardInfo> cardsData)
    {
        cardsData.Sort((a, b) => CompareCardRank(a.cardIndex, b.cardIndex) ? -1 : 1);

        if (GetRank(cardsData[0].cardIndex) == GetRank(cardsData[1].cardIndex) && GetRank(cardsData[0].cardIndex) == GetRank(cardsData[2].cardIndex) && GetRank(cardsData[0].cardIndex) == GetRank(cardsData[3].cardIndex))
        {
            return GetRank(cardsData[4].cardIndex) == GetRank(cardsData[5].cardIndex);
        }
        else if (GetRank(cardsData[2].cardIndex) == GetRank(cardsData[3].cardIndex) && GetRank(cardsData[2].cardIndex) == GetRank(cardsData[4].cardIndex) && GetRank(cardsData[2].cardIndex) == GetRank(cardsData[5].cardIndex))
        {
            return GetRank(cardsData[0].cardIndex) == GetRank(cardsData[1].cardIndex);
        }

        return false;
    }
    /// <summary>
    /// 单顺
    /// </summary>
    /// <param name="cardsData"></param>
    /// <returns></returns>
    public static bool IsSingleLine(List<CardInfo> cardsData)
    {
        cardsData.Sort((a, b) => CompareCardRank(a.cardIndex, b.cardIndex) ? -1 : 1);

        for (int i = 0; i < cardsData.Count - 1; i++)
        {
            if (GetRank(cardsData[i + 1].cardIndex) - GetRank(cardsData[i].cardIndex) != 1)
            {
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// 连对
    /// </summary>
    /// <param name="cardsData"></param>
    /// <returns></returns>
    public static bool IsDoubleLine(List<CardInfo> cardsData)
    {
        if (cardsData.Count % 2 != 0)
        {
            return false;
        }

        cardsData.Sort((a, b) => CompareCardRank(a.cardIndex, b.cardIndex) ? -1 : 1);

        for (int i = 0; i < cardsData.Count - 1; i += 2)
        {
            if (GetRank(cardsData[i].cardIndex) != GetRank(cardsData[i + 1].cardIndex))
            {
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// 比较牌组大小
    /// </summary>
    /// <param name="tabPutCards"></param>
    /// <param name="currentPutCards"></param>
    /// <returns></returns>
    public static bool CompareGroupValue(List<CardInfo> tabPutCards, List<CardInfo> currentPutCards)
    {
        CardGroupType currentCardGroup = GetCardType(currentPutCards);
        CardGroupType tabCardsGroup = GetCardType(tabPutCards);

        if (currentCardGroup == CardGroupType.CT_ERROR)
        {
            return false;
        }
        if (currentCardGroup == CardGroupType.CT_MISSILE_CARD)
        {
            return true;
        }
        if (tabCardsGroup == CardGroupType.CT_MISSILE_CARD)
        {
            return false;
        }
        if (currentCardGroup == tabCardsGroup)
        {
            int currentCardLen = currentPutCards.Count;
            int tabCardLen = tabPutCards.Count;

            if (currentCardLen != tabCardLen)
            {
                return false;
            }
            //
            if (currentCardGroup == CardGroupType.CT_SINGLE || currentCardGroup == CardGroupType.CT_DOUBLE || currentCardGroup == CardGroupType.CT_THREE)
            {
                return CompareCardValue(currentPutCards[0].cardIndex, tabPutCards[0].cardIndex);
            }
            tabPutCards.Sort((a, b) => CompareCardRank(a.cardIndex, b.cardIndex) ? -1 : 1);
            currentPutCards.Sort((a, b) => CompareCardRank(a.cardIndex, b.cardIndex) ? -1 : 1);
            if (currentCardGroup == CardGroupType.CT_THREE_TAKE_ONE || currentCardGroup == CardGroupType.CT_THREE_TAKE_TWO ||
                currentCardGroup == CardGroupType.CT_FOUR_TAKE_ONE || currentCardGroup == CardGroupType.CT_FOUR_TAKE_TWO)
            {
                return CompareCardValue(currentPutCards[2].cardIndex, tabPutCards[2].cardIndex);
            }
            if (currentCardGroup == CardGroupType.CT_THREE_LINE)
            {
                int count = currentCardLen / 3;
                for (int i = 0; i < count; i++)
                {
                    int c1 = currentPutCards[3 * i].cardIndex;
                    int ct = tabPutCards[3 * i].cardIndex;
                    if (CompareCardValue(ct, c1))
                    {
                        return false;
                    }
                }
                return true;
            }
            if (currentCardGroup == CardGroupType.CT_SINGLE_LINE || currentCardGroup == CardGroupType.CT_DOUBLE_LINE)
            {
                return CompareCardValue(currentPutCards[0].cardIndex, tabPutCards[0].cardIndex) && 
                       CompareCardValue(currentPutCards[currentCardLen - 1].cardIndex, tabPutCards[tabCardLen - 1].cardIndex);
            }
        }
        else
        {
            if (currentCardGroup == CardGroupType.CT_BOMB_CARD)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
}
