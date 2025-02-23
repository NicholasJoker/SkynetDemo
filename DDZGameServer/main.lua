local skynet =require("skynet")


skynet.start(function(...)
    local pid = skynet.spawn("gate/main.lua")
    if not pid then
        skynet.error("open gate error",pid)
        skynet.abort(true)
    end
    skynet.name(pid,"gate")


    local pid = skynet.spawn("dbproxy/main.lua")
    if not pid then
        skynet.error("open dbproxy error",pid)
        skynet.abort(true)
    end
    skynet.name(pid,"dbproxy")
end)