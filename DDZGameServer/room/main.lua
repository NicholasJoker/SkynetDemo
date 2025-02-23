local skynet = require("skynet")
local dkjson = require("lib.dkjson")
local ddz_game = require("room.ddz")
local common = require("lib.common")
local room_id = 0
local max_player = 3
local current = 0
local room_owner = 0
local players = {} -- array  {playerid,ready}
local playerCards = {}
-- play states 0.wait 1.ready 2.landlord 3.playing 4.over (一局结束)
local game_status = -1
--当前桌面上的牌
local tab_cards = {}
--打出牌的玩家
local put_cards_playerid = 0
local active_playerid = 0

local landlord_cards = {}
local landlord_playerid = 0
local landlord_coundown = 3000
local landlord_drop_count = 0
local landlord_drop_cycle = 0
--reset game data
local function reset()
    playerCards = {}
    game_status = -1
    tab_cards = {}
    put_cards_playerid = 0
    landlord_cards = {}
    landlord_playerid = 0
    landlord_drop_count = 0
    landlord_drop_cycle = 0
    active_playerid = 0
    landlord_drop_count = 0
    for key, value in pairs(players) do
        value.ready = 0
    end
end
-- local function disband()
--     local tab = {}
--     tab["reciveCode"] = 8
--     local retData = dkjson.encode(tab)
--     for k, v in pairs(players) do
--         skynet.send(v.socket_id, skynet.TYPE_SOCKET, retData,#retData)
--     end
--     skynet.log("房间解散")
--     skynet.kill()
-- end


local function reconnect(playerid,source)
    skynet.log("reconnect socket_id-----------",source)
    local tab ={}
    tab["reciveCode"] = 99
    if playerCards[playerid] == nil then
        playerCards[playerid] = {}
    end
    tab["cards"] = playerCards[playerid]
    tab["tabCards"] = tab_cards
    tab["putCardPlayerId"] = put_cards_playerid
    tab["activePlayerId"] = active_playerid
    tab["landlordPlayerId"] = landlord_playerid
    tab["roomId"] = room_id
    tab["playerstatus"] = {}
    if #playerCards[playerid] == 0 then
        tab["ready"] = 0
    end
    for k, v in pairs(players) do
        local playerStatus = {}
        playerStatus.playerId = v.playerid
        playerStatus.seatNo = v.seat_no
        playerStatus.username = v.playername
        if playerCards[v.playerid]~=nil then
            playerStatus.cardCount = #playerCards[v.playerid]
        else
            playerStatus.cardCount = -1
        end

        table.insert(tab["playerstatus"], playerStatus)
    end
    local retData = dkjson.encode(tab)
    skynet.log("send socket_id",source)
    --reset connect player socket_id
    if players[playerid] ~=nil then
        players[playerid].socket_id = source
    else
        skynet.warn("error reconnect playerid",playerid)
    end
    skynet.send(source, skynet.TYPE_SOCKET, retData,#retData)
end
-- player leave
local function leave(playerid)
    skynet.log("playerid---room_owner",playerid,room_owner)
    local p = players[playerid]
    if not p then
        skynet.warn("invalid player leave", playerid)
        return
    end

    local tab = {}
    tab["reciveCode"] = 8
    tab["playerId"] = playerid
    tab["roomId"] = room_id
    tab["isDisband"] = 0
    tab["ready"] = 0
    local retData = dkjson.encode(tab)
    for k, v in pairs(players) do
        players[k].ready = 0
        skynet.send(v.socket_id, skynet.TYPE_SOCKET, retData,#retData)
    end
    table.remove(players, playerid)
    current = current - 1
    for k, v in pairs(players) do
        if v.playerid == playerid then
            table.remove(players,k)
        end
    end
end
--退出游戏
local function exit_game(playerid)
    local p = players[playerid]
    if not p then
        skynet.warn("invalid player leave", playerid)
        return
    end
    local tab = {}
    tab["reciveCode"] = 9
    tab["playerId"] = playerid
    tab["roomId"] = room_id
    local retData = dkjson.encode(tab)
    for k, v in pairs(players) do
        skynet.send(v.socket_id, skynet.TYPE_SOCKET, retData,#retData)
    end
end
--settlement
local function settlement(id)
    local tab = {}
    tab["reciveCode"] = 10
    tab["settlementInfos"] = {}
    local count = 0
    for k, v in pairs(players) do
        if v.playerid ~= id then
            local len = #playerCards[v.playerid]
            count = count+len
            local tb = {
                score = len*(-1),
                playerName = v.playername,
                playerId = v.playerid
            }
            table.insert(tab["settlementInfos"],tb)
        end
    end
    tab["winPlayerId"] = id
    table.insert(tab["settlementInfos"],{
        score = count,
        playerName = playerCards[id].playername,
        playerId = id
    })
    reset()
    local retData = dkjson.encode(tab)
    for k, v in pairs(players) do
        skynet.send(v.socket_id, skynet.TYPE_SOCKET, retData,#retData)
    end
end
local function get_next_playerid(seat_no)
    local seatNo = (seat_no+1)%3
    local retId = -1
    for k,v in pairs(players) do
        if v.seat_no == seatNo then
            retId = v.playerid
            return retId
        end
    end
    return retId
end
-- player join room
local function join(msgdata)
    if msgdata ~= nil then
        current = current + 1
        for k, v in ipairs(msgdata.players) do
            if not players[v.id] then players[v.id] = {} end
            if players[v.id].ready == nil then
                players[v.id].ready = 0
            end
            players[v.id].playerid = v.id
            players[v.id].playername = v.name
            players[v.id].seat_no = k - 1
            players[v.id].ready = v.ready
            players[v.id].socket_id = v.socket_id
            players[v.id].roompid = v.roompid
            skynet.log("房间同步数据 v.id,socket_id",v.name,v.id,v.socket_id)
        end
        return true
    end
    return false
end
-- player ready play
local function ready(msgdata)
    local p = players[msgdata.playerId]
    if not p then
        skynet.warn("invalid player ready", msgdata.playerId)
        return false
    end
    game_status = 1
    local count = 0
    for key, val in pairs(players) do
        if val.playerid == msgdata.playerId then
            val.ready = 1
            players[key] = val
        end
    end
    skynet.log("players*******",#players)
    local data = {}
    data["reciveCode"] = 3
    data["status"] = {}
    for k, v in pairs(players) do
        local playerStatus = {}
        skynet.log("v.playerid,v.ready,v.socket_id",v.playerid,v.ready,v.socket_id)
        playerStatus.playerId = v.playerid
        playerStatus.ready = v.ready
        if v.ready == 1 then
            count = count + 1
            skynet.log("count:", count)
        end
        table.insert(data["status"], playerStatus)
    end
    local retData = dkjson.encode(data)
    for k, v in pairs(players) do
        skynet.send(v.socket_id, skynet.TYPE_SOCKET, retData, #retData)
    end
    if count == 3 then
        count = 0
        local dealCards = ddz_game:shuffle()
        local index = 1
        for k, v in pairs(players) do
            local cardsTab = common:slice(dealCards, index, index + 16)
            playerCards[k] = cardsTab
            index = index + 17
        end
        landlord_cards = common:slice(dealCards, #dealCards - 2, #dealCards)
        for k, v in pairs(players) do
            local tab = {}
            tab["reciveCode"] = 4
            tab["cards"] = playerCards[k]
            local retData = dkjson.encode(tab)
            skynet.send(v.socket_id, skynet.TYPE_SOCKET, retData, #retData)
        end
    end
end


local co = skynet.fork(function(retData)
        for k, v in pairs(players) do
            skynet.send(v.socket_id, skynet.TYPE_RAW, retData,string.len(retData))
        end
end)
local function single_trigger_timer(delay, callback)
    local start_time = os.clock()
    local function check_timer()
        local current_time = os.clock()
        if current_time - start_time >= (delay / 1000) then
            callback()
            return true
        end
        return false
    end
    return check_timer
end
-- player get more cards
local function get_landlord_cards(data)
    landlord_playerid = data.playerId
    -- local tab = {}
    -- tab["reciveCode"] = 5
    -- tab["cards"] = landlord_cards
    -- tab["playerId"] = data.playerId

    -- local co = skynet.fork(function()
    --     skynet.log("协同程序")
    --     for k, v in pairs(players) do
    --         skynet.send(v.socket_id, skynet.TYPE_RAW, retData,string.len(retData))
    --     end
    -- end)
    --countdown
    if game_status == 1 then
        game_status = 2
        skynet.sleep(landlord_coundown)
        game_status = 3
        -- skynet.wakeup(co,retData)
        local tab = {}
        tab["reciveCode"] = 5
        tab["cards"] = landlord_cards
        tab["playerId"] = landlord_playerid
        active_playerid = landlord_playerid
        playerCards[landlord_playerid] = common:merge_tables(playerCards[landlord_playerid],landlord_cards)
        local retData = dkjson.encode(tab)
        for k, v in pairs(players) do
            skynet.log("v.socket_id",v.socket_id)
            skynet.send(v.socket_id, skynet.TYPE_SOCKET, retData,#retData)
        end
    end
end
--drop landlord cars
local function DropLandlord(msgdata)
    local p = players[msgdata.playerId]
    if not p then
        skynet.warn("invalid player ready", msgdata.playerId)
        return false
    end
    landlord_drop_count=landlord_drop_count+1
    game_status = 1
    skynet.log("players*******",#players)
    if landlord_drop_count == 3 and landlord_drop_cycle<2 then
        landlord_drop_cycle=landlord_drop_cycle+1
        landlord_drop_count = 0
        local dealCards = ddz_game:shuffle()
        local index = 1
        for k, v in pairs(players) do
            local cardsTab = common:slice(dealCards, index, index + 16)
            playerCards[k] = cardsTab
            index = index + 17
        end
        landlord_cards = common:slice(dealCards, #dealCards - 2, #dealCards)
        for k, v in pairs(players) do
            local tab = {}
            tab["reciveCode"] = 4
            tab["cards"] = playerCards[k]
            local retData = dkjson.encode(tab)
            skynet.send(v.socket_id, skynet.TYPE_SOCKET, retData, #retData)
        end
    end
    if landlord_drop_cycle==2 and landlord_drop_count == 3 then
        local randomIndex = math.random(1, max_player)
        local index = 1
        for key, value in pairs(players) do
            if index == randomIndex then
                landlord_playerid = value.playerid
            else
                index = index+1
            end
        end
        local tab = {
            playerId = landlord_playerid
        }
        get_landlord_cards(tab)
    end
end

local landlord_call = function(tab)
    local retData = dkjson.encode(tab)
    for k, v in pairs(players) do
        skynet.send(v.socket_id, skynet.TYPE_SOCKET, retData,string.len(retData))
    end
end

local function put_cards(data)
    --检查手牌
    local equal = common:is_table_contained(playerCards[data.playerId], data.cards)
    skynet.log("equal:",equal)
    local errorCode = {}
    errorCode["reciveCode"] = -5
    local errorData = dkjson.encode(errorCode)
    --检测上次出牌的牌型 检测牌型大小
    local compareRet = false
    if #tab_cards>0 and put_cards_playerid~=data.playerid then
        compareRet = ddz_game:compare_group_value(tab_cards,data.cards)
        skynet.log("compareRet:",compareRet)
    else
        compareRet = true
    end
    if equal and compareRet then
        skynet.log("ddz_game:get_card_type")
        local putResult = ddz_game:get_card_type(data.cards)
        skynet.log("putResult",putResult)
        if putResult ~= 0 then
            playerCards[data.playerId] = common:remove_elements(playerCards[data.playerId], data.cards)
            local tab = {}
            tab["reciveCode"] = 6
            tab["seatNo"] = players[data.playerId].seat_no
            tab["playerId"] = data.playerId
            tab["cards"] = data.cards
            tab["groupType"] = putResult
            tab["ok"] = 1
            tab["isOver"] = 0
            if #playerCards[data.playerId] == 0 then
                tab["isOver"] = 1
            end
            tab_cards = data.cards
            put_cards_playerid = data.playerId
            local retData = dkjson.encode(tab)
            skynet.log("retData", retData)
            active_playerid = get_next_playerid(tab["seatNo"])
            for k, v in pairs(players) do
                skynet.send(v.socket_id, skynet.TYPE_SOCKET, retData,#retData)
            end
            if tab["isOver"] == 1 then
                active_playerid = 0
                settlement(data.playerId)
            end
        else
            skynet.send(players[data.playerId].socket_id, skynet.TYPE_RAW,errorData, string.len(errorData))
        end
    else
        skynet.send(players[data.playerId].socket_id, skynet.TYPE_RAW, errorData,string.len(errorData))
    end
end

local function pass_cards(data)
    local tab = {}
    tab["reciveCode"] = 7
    tab["seatNo"] = players[data.playerId].seat_no
    tab["playerId"] = data.playerId
    tab["nextPlayerId"] = get_next_playerid(tab["seatNo"])
    active_playerid = tab["nextPlayerId"]
    -- skynet.log("nextplayid",tab["nextPlayerId"],put_cards_playerid)
    for k, v in pairs(players) do
        if v.playerid == put_cards_playerid and v.playerid == tab["nextPlayerId"] then
            --下一轮了
            tab_cards = {}
            tab["state"] = 1
        else
            tab["state"] = 2
        end
        local retData = dkjson.encode(tab)
        skynet.send(v.socket_id, skynet.TYPE_SOCKET, retData,#retData)
    end
end
local function restart_game(data)
    players[data.playerId].ready = 1
    local count = 0
    for k, v in pairs(players) do
        if v.ready == 1 then
            count=count+1
        end
    end
    if count == 3 then
        count = 0
        local dealCards = ddz_game:shuffle()
        local index = 1
        for k, v in pairs(players) do
            local cardsTab = common:slice(dealCards, index, index + 16)
            playerCards[k] = cardsTab
            index = index + 17
        end
        landlord_cards = common:slice(dealCards, #dealCards - 2, #dealCards)
        skynet.log("allAeady")
        for k, v in pairs(players) do
            local tab = {}
            tab["reciveCode"] = 4
            tab["cards"] = playerCards[k]
            local retData = dkjson.encode(tab)
            skynet.send(v.socket_id, skynet.TYPE_SOCKET, retData, #retData)
        end
    end
end
local function info() end

skynet.start(function(...)
    local args = {...}
    room_id = args[1]
    room_owner = args[2]
    players[room_owner] = {ready = 0, playerid = args[2], socket_id = args[3]}
    current = current + 1
    game_status = 0
    skynet.dispatch(function(source, session,mtype,msg, size)
        if session > 0 then
            skynet.ret(skynet.pack(info()))
            return
        end
        local jsonstr = skynet.tostring(msg, size)
        skynet.log("skynet.dispatch---message", jsonstr)
        local data, err = dkjson.decode(jsonstr)
        -- leave设计到房主离开解散房间的逻辑 后续再补
        -- 2.join 3.ready,4.dealCard 5.landlord 6.putcard 100.allready
        -- 加入房间
        if data.opCode == 2 then
            skynet.log("join room data",source,data.playerId)
            join(data)
            return
        end
        -- 准备
        if data.opCode == 3 then
            ready(data)
            return
        end
        if data.opCode == 4 then
            DropLandlord(data)
        end
        -- 抢地主
        if data.opCode == 5 then
            get_landlord_cards(data)
            return
        end
        --出牌
        if data.opCode == 6 then
            put_cards(data)
            return
        end
        --过牌
        if data.opCode == 7 then
            pass_cards(data)
            return
        end
        --未开局离开
        if data.opCode == 8 then
            leave(data.playerId)
            return
        end
        if data.opCode == 9 then
            exit_game(data.playerId)
            return
        end
        --重开
        if data.opCode == 11 then
            restart_game(data)
            return
        end
        if data.opCode == 99 then
            reconnect(data.playerId,source)
        end
    end)

end)
