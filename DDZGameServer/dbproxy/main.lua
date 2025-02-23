local skynet = require("skynet")
local db = require("db.mysql")

local login_sql = "select * from accounts where name = '%s' and password = '%s'"

local create_sql = "insert into accounts(id,name,password) values(%d,'%s','%s')"
local function create_account()
    -- 生成六位数的唯一随机整数
    -- 获取当前时间戳（毫秒级）
    local timestamp = skynet.now()
    -- skynet.log("00000", timestamp)
    -- 使用时间戳和 os.clock() 来生成一个唯一的数字
    local clock_value = math.floor(os.clock() * 1000) -- 获取CPU时间的毫秒级别
    skynet.log(clock_value)
    local random_number = (timestamp + clock_value) % 900000 + 100000
    -- skynet.log("random_number:", random_number)
    return random_number
end
local function logind(fd, acc, passwd)
    local result, err = db.execute(fd, string.format(login_sql, acc, passwd))
    skynet.log("logind",acc,passwd,err,result)
    if result==nil then
        skynet.error("query", err)
        -- maybe not exist
        local id = create_account()
        --skynet.now()
        skynet.log(" skynet.now",id)
        local _, err = db.execute(fd, string.format(create_sql, id, acc, passwd))
        if err then
            skynet.error("login error")
            return nil
        end
        return {
            id = id,
            name = acc
        }

    end
    return {
        id = result[1].id,
        name = result[1].name
    }
end

skynet.start(function(...)
    local addr = skynet.getenv("db.addr")
    local user = skynet.getenv("db.user")
    local passwd = skynet.getenv("db.passwd")
    local database = skynet.getenv("db.database")

    if not addr or not user or not passwd or not database then
        skynet.error("invalid db config")
        skynet.abort(true)
    end

    skynet.log("database", addr, user, passwd, database)

    local fd, err = db.connect(addr, user, passwd, database)
    skynet.log("fd,err",fd,err)
    if fd == nil then
        skynet.error("connect db error", err)
        skynet.abort(true)
        return
    end

    skynet.dispatch(function(source, session, mtype, msg, size)
        local acc, passwd = skynet.unpack(msg, size)
        skynet.trash(msg, size)
        skynet.log("database check", acc, passwd)
        local result = logind(fd, acc, passwd)
        skynet.log("skynet.pack(result)",skynet.pack(result))
        skynet.ret(skynet.pack(result))
    end)

end)
