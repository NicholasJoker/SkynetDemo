local skynet = require("skynet")
local socket = require("socket")
local json = require("json")
local dkjson = require("lib.dkjson")
require("errno")

--[[
type user struct {
    sid -- socket id
    id
    name
    ...
    roomid  -- 0 means not bind room
    roompid
}
--]]
--用户
local users = {}

--[[
type room struct {
    id
    name
    owner  -- 
    players
}
--]]
local rooms = {}
-- 房间id
local room_id = 0

local function find_room(rid)
    local room = rooms[rid]
    return room
end
local function get_user(playerid)
    --todo cash
    for k,v in pairs(users) do
        if v.id == playerid then
            return v
        end
    end
    return nil
end
--game reconnect
local function reconnect(socket_id,playerid,roomid,roompid)
    local retData = {}
    retData.opCode = 99
    retData.socket_id = socket_id
    retData.playerId = playerid
    local sendData = json.encode(retData)
    -- skynet.log("skynet.redirect:",sendData)
    -- skynet.log("session")
    local ret = skynet.redirect(socket_id,roompid,skynet.TYPE_SOCKET,0,sendData,#sendData)
end

local function create_room(playerid,source)
    local user = users[source]
    skynet.log("playerid,source",playerid,source)
    if not user then
        return false
    end
    if user.roomid then
        skynet.warn("player", playerid, "repeate create room")
        return false
    end
    local pid = skynet.spawn("room/main.lua", room_id + 1, playerid,user.socket_id)
    if pid == 0 then
        skynet.error("player", playerid, "create room failure")
        return false
    end
    room_id = room_id + 1

    user.roomid = room_id
    user.roompid = pid
    skynet.log("user.roompid 0000",user.roompid)
    skynet.log("----- ", room_id, pid)
    local ret = json.encode({
        reciveCode = 1,
        roomId = room_id
    })
    local room = {
        id = room_id,
        pid = user.roompid,
        owner = playerid,
        players = {user}
    }
    rooms[room_id] = room
    -- for k,v in pairs(room) do
    --     skynet.log("k,v",k,v)
    -- end
    skynet.send(source,skynet.TYPE_SOCKET,ret,#ret)
    return true
end
--return error code 
local function return_errorcode(source,errorcode)
    local ret = json.encode(errorcode)
    skynet.send(source,skynet.TYPE_SOCKET,ret,#ret)
    return
end
local function join_room(user,roomId,op,source,session)
    --reconnect check
    local room = find_room(roomId)
    skynet.log("find_room(roomId)",roomId,op,#room.players)
    if not room or #room.players > 3 then
        skynet.log("join room failed",not room,#room.players)
        --加入房间失败
        return_errorcode(-3)
    else
        user.roomid = roomId
        user.roompid = room.pid
        local index = -1
        for k, v in pairs(room.players) do
            if v.id == user.id then
                index = k
            end
        end
        if index>0 then
            room.players[index] = user
        else
            table.insert(room.players,user)
        end
        local data = {reciveCode = op,roomId = user.roomid,playerStatus = {}}
        for k,v in ipairs(room.players) do
            local playerStatus = {}
            playerStatus.playerId = v.id
            playerStatus.username = v.name
            if room.players[k].ready == nil then
                room.players[k].ready = 0
            end
            playerStatus.ready = v.ready
            playerStatus.seatNo = k-1
            room.players[k].seat_no = k-1
            table.insert(data.playerStatus,playerStatus)
        end
        rooms[room_id] = room
        -- --同步加入房间数据
        local retData = {}
        retData.opCode = op
        retData.roomId = roomId
        retData.players = room.players
        local sendData = json.encode(retData)
        local ret = skynet.redirect(source,user.roompid, skynet.TYPE_SOCKET,session,sendData,#sendData)
        skynet.log("ret:",ret)
        if not ret then
        --网关返回
            for k,v in ipairs(room.players) do
                local ret = dkjson.encode(data)
                skynet.send(v.socket_id, skynet.TYPE_SOCKET, ret, #ret)
            end
        end
    end
end
local function remove_room(roomid,roompid,source)
    local room = rooms[roomid]
    if not room then
        skynet.error("try remove a invalid room",roomid)
        return
    end
    local tab = {}
    tab["reciveCode"] = 8
    tab["playerId"] = users[source].id
    tab["roomId"] = roomid
    tab["isDisband"] = 1
    local ret = json.encode(tab)
    for k, v in pairs(room.players) do
        skynet.send(v.socket_id,skynet.TYPE_SOCKET, ret, #ret)
    end
    for k, v in pairs(room.players) do
        users[v.socket_id].roomid = nil
        users[v.socket_id].roompid = nil
    end
    rooms[roomid] = nil
    skynet.kill(roompid)
end


local function onlogin(socket_id, data)
    skynet.fork(function()
        local msg, size = skynet.call("dbproxy", skynet.TYPE_RAW, 20000, skynet.pack(data.username,data.password))
        if size < 1 then
            skynet.error("login failure")
            return
        end
        local result = skynet.unpack(msg, size)
        skynet.trash(msg, size)
        local user1 = get_user(result.id)
        skynet.log("get_userr",user1,socket_id)
        
        local need_reconnect = 0
        if user1~=nil and user1.roomid~=nil and user1.roompid~=nil then
            need_reconnect = 1
            skynet.log("need_reconnect,user1.socket_id,socket_id",need_reconnect,user1.socket_id,socket_id)
        end
        if user1~=nil and user1.socket_id~=socket_id then
            local ret = json.encode({
                reciveCode = data.opCode,
                id = result.id,
                username = result.name,
                errCode = 0,
                isOk = 0,
                isReconnect = 0
            })
            skynet.log("ret",ret)
            skynet.log("users[user1.socket_id]",users[user1.socket_id])
            users[user1.socket_id] = nil
            skynet.send(user1.socket_id, skynet.TYPE_SOCKET, ret, #ret)
            -- socket.close(user1.socket_id)
        end
        skynet.log("login progress---",result.id,result.name)
        local user = {
            socket_id = socket_id,
            id = result.id,
            name = result.name
        }
        if user1~=nil and user1.roomid ~=nil and user1.roompid~=nil then
            skynet.log("user1.roomid---user1.roompid",user1.roomid,user1.roompid)
            user.roomid = user1.roomid
            user.roompid = user1.roompid
        end
        users[socket_id] = user
        -- if not user then
        --     local ret = json.encode({
        --         reciveCode = data.opCode,
        --         id = user.id,
        --         errCode = 1,
        --         isOk = 0
        --     })
        --     skynet.send(socket_id, skynet.TYPE_SOCKET, ret, #ret)
        --     socket.close(socket_id)
        --     return
        -- end
        skynet.log("login socket_id-------------------",socket_id)
        local ret = json.encode({
            reciveCode = data.opCode,
            errCode = EOK,
            id = user.id,
            username = user.name,
            isOk = 1,
            isReconnect = need_reconnect
        })
        skynet.send(socket_id, skynet.TYPE_SOCKET, ret, #ret)
        skynet.log("user", result.name, "login success")
        return
    end)
end

local function onlogout(socket_id)
    local user = users[socket_id]
    if not user then
        skynet.log("user", user.id, user.name, "exit")
    else
        --skynet.log("socket close", socket_id)
    end
    socket.close(socket_id)
end

skynet.start(function(...)
    skynet.log("gateserver", skynet.pid)
    skynet.dispatch(function(source, session, mtype, msg, size)
        -- skynet.log("source, session, mtype,size",source,seaaion,mytype,size)
        if mtype == skynet.TYPE_SOCKET then
            if size > 0 then
                local jsonstr = skynet.tostring(msg, size)
                skynet.log("message", jsonstr)
                local data, err = json.decode(jsonstr)
                skynet.trash(msg, size)
                if err then
                    skynet.error("invalid json")
                    socket.close(source)
                    return
                end
                local op = data.opCode
                skynet.log("opcode", op)
                if op == 0 then -- login
                    onlogin(source,data)
                    return
                end
                -- check user whether valid
                local user = users[source]
                if not user or not user.socket_id then
                    skynet.error("invalid user")
                    socket.close(source)
                    return
                end

                if op == 1 then -- create room
                    create_room(user.id,source)
                    return
                end
                if op ==2 then
                    skynet.log("op ==2",session)
                    join_room(user,data.roomId,op,source,session)
                    return
                end
                --加入房间退出
                if op == 8 then
                    if rooms[data.roomId].owner == data.playerId then
                        remove_room(data.roomId,rooms[data.roomId].pid,source)
                        return
                    else
                        skynet.redirect(source, user.roompid,skynet.TYPE_SOCKET,session,msg,size)
                        return
                    end
                end
                if op == 9 then
                    if rooms[data.roomId].owner == data.playerId then
                        remove_room(data.roomId,rooms[data.roomId].pid,source)
                        return
                    else
                        skynet.redirect(source, user.roompid,skynet.TYPE_SOCKET,session,msg,size)
                        return
                    end
                end
                if not user.roomid and op~=2 then
                    skynet.error("user", user.id, "not exit valid room")
                    return
                end
                if op > 2 then
                    local err = skynet.redirect(source, user.roompid,skynet.TYPE_SOCKET,session,msg,size)
                    skynet.error("roompid ---error",err,msg)
                end
            else
                -- socket close
                -- skynet.log("session,size",session,size)
                onlogout(source)
            end
        end
    end)
    local err = socket.start_server("ws://192.168.1.160:8886", 4096)
    if err then
        skynet.error("open ws error", err)
        skynet.abort(true)
    end
end)