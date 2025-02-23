local common = require("lib.common")
local ddz_game = {
    shuffle_card = {},
    player_card = {}, --玩家的手牌
}
    --方块 梅花 红桃 黑桃
cards = {
0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D,
0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D,
0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D,
0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D,
0x4E, 0x4F
}
-- 0 小王 255 大王
local CardType = {
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

function ddz_game:shuffle()
    -- 设置随机种子
    math.randomseed(os.time())
    local len = #cards
    --牌组算法需要修改 尽可能出现多的三张和炸弹
    for i=len,2,-1 do
        local j = math.random(1, i)
        cards[i],cards[j] = cards[j],cards[i]
    end
    return cards
end

-- 获取牌的花色
function ddz_game:get_suit(card)
    return math.floor(card / 0x10)  -- 高4位表示花色
end

-- 获取牌的点数
function ddz_game:get_rank(card)
    return card % 0x10  -- 低4位表示点数
end
-- 获取牌值的优先级 点数3-13 优先级是0  1，2 优先级是1  14，15 优先级是2

function ddz_game:get_rank_level(card)
    local rank = self:get_rank(card)
    if rank <3 then 
        return 1
    end
    if rank>13 then
        return 2
    end
    return 0
end
--比较牌值
function ddz_game:compare_card_value(v1,v2)
    local level1 = self:get_rank_level(v1)
    local level2 = self:get_rank_level(v2)
    if level1 > level2 then
        return true
    elseif level1==level2 then
        return self:get_rank(v1)>self:get_rank(v2)
    else
        return false
    end
end
-- 比较牌的点数大小
function ddz_game:compare_card_rank(a, b)
    return self:get_rank(a) < self:get_rank(b)
end

-- 获取牌型
function ddz_game:get_card_type(cardsData)
    local cardsCount = #cardsData
    if cardsCount == 0 then
        return CardType.CT_ERROR
    elseif cardsCount == 1 then
        return CardType.CT_SINGLE
    elseif cardsCount == 2 then
        if self:is_missile(cardsData) then
            return CardType.CT_MISSILE_CARD
        elseif self:is_double(cardsData) then
            return CardType.CT_DOUBLE
        end
        return CardType.CT_ERROR
    elseif cardsCount == 3 then
        if self:is_three(cardsData) then
            return CardType.CT_THREE
        end
    elseif cardsCount == 4 then
        if self:is_bomb(cardsData) then
            return CardType.CT_BOMB_CARD
        elseif self:is_three_take_one(cardsData) then
            return CardType.CT_THREE_TAKE_ONE
        end
    elseif cardsCount >= 5 then
        if cardsCount == 5 then
            if self:is_three_take_two(cardsData) then
                return CardType.CT_THREE_TAKE_TWO
            end
        elseif cardsCount == 6 then
            if self:is_four_take_two(cardsData) then
                return CardType.CT_FOUR_TAKE_ONE
            end
        elseif cardsCount == 8 then
            if self:is_four_take_two_double(cardsData) then
                return CardType.CT_FOUR_TAKE_TWO
            end
        end
        if cardsCount >= 8 then
            if self:is_three_line(cardsData) then
                return CardType.CT_THREE_LINE
            end
        end
        if cardsCount > 5 and cardsCount % 2 == 0 then
            if self:is_double_line(cardsData) then
                return CardType.CT_DOUBLE_LINE
            end
        end
        if self:is_single_line(cardsData) then
            return CardType.CT_SINGLE_LINE
        end
    end
    return CardType.CT_ERROR
end

-- 判断对子
function ddz_game:is_double(cardsData)
    return self:get_rank(cardsData[1]) == self:get_rank(cardsData[2])
end

-- 判断火箭(王炸)
function ddz_game:is_missile(cardsData)
    return (cardsData[1] == 0x4E and cardsData[2] == 0x4F) or (cardsData[1] == 0x4F and cardsData[2] == 0x4E)
end

-- 判断三条
function ddz_game:is_three(cardsData)
    return self:get_rank(cardsData[1]) == self:get_rank(cardsData[2]) and self:get_rank(cardsData[1]) == self:get_rank(cardsData[3])
end

-- 判断炸弹
function ddz_game:is_bomb(cardsData)
    return self:get_rank(cardsData[1]) == self:get_rank(cardsData[2]) and self:get_rank(cardsData[1]) == self:get_rank(cardsData[3]) and self:get_rank(cardsData[1]) == self:get_rank(cardsData[4])
end

-- 判断三带一
function ddz_game:is_three_take_one(cardsData)
    table.sort(cardsData, function(a, b) return self:compare_card_rank(a, b) end)
    if self:is_bomb(cardsData) then return false end
    if self:get_rank(cardsData[1]) == self:get_rank(cardsData[2]) and self:get_rank(cardsData[1]) == self:get_rank(cardsData[3]) then
        return true
    elseif self:get_rank(cardsData[2]) == self:get_rank(cardsData[3]) and self:get_rank(cardsData[2]) == self:get_rank(cardsData[4]) then
        return true
    end
    return false
end

-- 判断三带二
function ddz_game:is_three_take_two(cardsData)
    table.sort(cardsData, function(a, b) return self:compare_card_rank(a, b) end)
    if self:get_rank(cardsData[1]) == self:get_rank(cardsData[2]) and self:get_rank(cardsData[1]) == self:get_rank(cardsData[3]) then
        if self:get_rank(cardsData[4]) == self:get_rank(cardsData[5]) then
            return true
        end
    elseif self:get_rank(cardsData[3]) == self:get_rank(cardsData[4]) and self:get_rank(cardsData[3]) == self:get_rank(cardsData[5]) then
        if self:get_rank(cardsData[1]) == self:get_rank(cardsData[2]) then
            return true
        end
    end
    return false
end

function ddz_game:is_three_line(cardsData)
    table.sort(cardsData, function(a, b) return self:compare_card_rank(a, b) end)
    local length = #cardsData
    if length < 6 then
        return false
    end
    local threeLine = {}
    local remainingCards = {}
    local i = 1
    while i <= length - 2 do
        if self:get_rank(cardsData[i]) == self:get_rank(cardsData[i+1]) and self:get_rank(cardsData[i]) == self:get_rank(cardsData[i+2]) then
            table.insert(threeLine, self:get_rank(cardsData[i]))
            i = i + 3
        else
            table.insert(remainingCards, cardsData[i])
            i = i + 1
        end
    end
    if #threeLine == 0 then
        return false
    end
    for key, value in pairs(threeLine) do
        if value == 2 then
            return false
        end
    end
    local remainingCount = #remainingCards
    local threeLineCount = #threeLine
    if remainingCount > threeLineCount * 2 then
        return false
    end
    local isOdd = remainingCount % 2 == 1
    if isOdd and remainingCount >= threeLineCount then
        return false
    end
    if not isOdd then
        if remainingCount > threeLineCount * 2 then
            return false
        end
        local pairCount = 0
        for _, card in ipairs(remainingCards) do
            local rank = self:get_rank(card)
            if not pairCount[rank] then
                pairCount[rank] = 0
            end
            pairCount[rank] = pairCount[rank] + 1
        end
        for _, count in pairs(pairCount) do
            if count ~= 2 then
                return false
            end
        end
    end
    return true
end
-- 判断四带二
function ddz_game:is_four_take_two(cardsData)
    table.sort(cardsData, function(a, b) return self:compare_card_rank(a, b) end)
    if self:get_rank(cardsData[1]) == self:get_rank(cardsData[2]) and self:get_rank(cardsData[1]) == self:get_rank(cardsData[3]) and self:get_rank(cardsData[1]) == self:get_rank(cardsData[4]) then
        return true
    elseif self:get_rank(cardsData[3]) == self:get_rank(cardsData[4]) and self:get_rank(cardsData[3]) == self:get_rank(cardsData[5]) and self:get_rank(cardsData[3]) == self:get_rank(cardsData[6]) then
        return true
    elseif self:get_rank(cardsData[2]) == self:get_rank(cardsData[3]) and self:get_rank(cardsData[4]) == self:get_rank(cardsData[5]) and self:get_rank(cardsData[3]) == self:get_rank(cardsData[4]) then
        return true
    end
    return false
end
-- 判断四带一对
function ddz_game:is_four_take_two_double(cardsData)
    table.sort(cardsData, function(a, b) return self:compare_card_rank(a, b) end)
    -- 四张相同的牌 + 两张相同的牌
    if self:get_rank(cardsData[1]) == self:get_rank(cardsData[2]) and self:get_rank(cardsData[1]) == self:get_rank(cardsData[3]) and self:get_rank(cardsData[1]) == self:get_rank(cardsData[4]) then
        if self:get_rank(cardsData[5]) == self:get_rank(cardsData[6]) then
            return true
        end
    elseif self:get_rank(cardsData[3]) == self:get_rank(cardsData[4]) and self:get_rank(cardsData[3]) == self:get_rank(cardsData[5]) and self:get_rank(cardsData[3]) == self:get_rank(cardsData[6]) then
        if self:get_rank(cardsData[1]) == self:get_rank(cardsData[2]) then
            return true
        end
    end
    return false
end

-- 判断顺子
function ddz_game:is_single_line(cardsData)
    skynet.log("is_single_line------1111111111111-------------")
    table.sort(cardsData, function(a, b) return self:compare_card_rank(a, b) end)
    local length = #cardsData
    for i = 1, length - 1 do
        if self:get_rank(cardsData[i + 1]) - self:get_rank(cardsData[i]) ~= 1 then
            return false
        end
    end
    return true
end

-- 判断连对
function ddz_game:is_double_line(cardsData)
    if #cardsData % 2 ~= 0 then
        return false
    end
    for key, value in pairs(cardsData) do
        if self:get_rank(value) == 2 then
            return false
        end
    end
    table.sort(cardsData, function(a, b) return self:compare_card_rank(a, b) end)
    for i = 1, #cardsData - 1, 2 do
        if self:get_rank(cardsData[i]) ~= self:get_rank(cardsData[i + 1]) then
            return false
        end
        if i + 2 <= #cardsData then
            if self:get_rank(cardsData[i]) + 1 ~= self:get_rank(cardsData[i + 2]) then
                return false
            end
        end
    end
    return true
end

-- cardgroup compare
function ddz_game:compare_group_value(tabPutCards,currentPutCards)
    local currentCardGroup = ddz_game:get_card_type(currentPutCards)
    local tabCardsGroup = ddz_game:get_card_type(tabPutCards)
    if currentCardGroup == CardType.CT_ERROR then
        return false
    end
    if currentCardGroup == CardType.CT_MISSILE_CARD then
        return true
    end
    if tabCardsGroup == CardType.CT_MISSILE_CARD then
        return false
    end
    -- cardGroup equals 
    if currentCardGroup == tabCardsGroup then
        local currentCardLen = #currentPutCards
        local tabCardLen = #tabPutCards

        if currentCardLen~=tabCardLen then
            return false
        end
        -- CT_SINGLE = 1  CT_DOUBLE = 2  CT_THREE = 3
        if currentCardGroup ==  CardType.CT_SINGLE or currentCardGroup == CardType.CT_DOUBLE or currentCardGroup == CardType.CT_THREE then
            return self:compare_card_value(currentPutCards[1],tabPutCards[1])
        end
        table.sort(tabPutCards,function(a, b) return self:compare_card_rank(a, b) end)
        table.sort(currentPutCards,function(a, b) return self:compare_card_rank(a, b) end)


        -- CT_THREE_TAKE_ONE = 5   CT_THREE_TAKE_TWO = 6  CT_FOUR_TAKE_ONE = 7   CT_FOUR_TAKE_TWO = 8
        if currentCardGroup ==  CardType.CT_THREE_TAKE_ONE or currentCardGroup ==  CardType.CT_THREE_TAKE_TWO or currentCardGroup ==  CardType.CT_FOUR_TAKE_ONE or currentCardGroup ==  CardType.CT_FOUR_TAKE_TWO then
            return self:compare_card_value(currentPutCards[3],tabPutCards[3])
        end
        -- CT_THREE_LINE = 9,
        if currentCardGroup == CardType.CT_THREE_LINE then
            local count = currentCardLen/3
            for i = 1,count do
                local c1 = currentPutCards[1+3*(i-1)]
                local ct = tabPutCards[1+3*(i-1)]
                if self:compare_card_value(ct,c1) then
                    return false
                end
            end
            return true
        end
        -- CT_DOUBLE_LINE = 10
        -- CT_SINGLE_LINE = 11
        if currentCardGroup == CardType.CT_SINGLE_LINE or currentCardGroup == CardType.CT_DOUBLE_LINE then
            if self:compare_card_value(currentPutCards[1],tabPutCards[1]) and self:compare_card_value(currentPutCards[currentCardLen],tabPutCards[tabCardLen]) then
                return true
            else
                return false
            end
        end

    else
        if currentCardGroup == CardType.CT_BOMB_CARD then
            return true
        else
            return false
        end
    end
end
return ddz_game