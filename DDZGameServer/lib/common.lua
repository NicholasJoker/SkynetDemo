local common = {}
function common:contains(table, value)
    for i, v in ipairs(table) do
        if v == value then
            return true
        end
    end
    return false
end
--两个table 是否相同
function common:tables_areEqual(t1, t2)
    -- 如果两个表的长度不同，直接返回false
    if #t1 ~= #t2 then
        return false
    end
    -- 遍历t1和t2，检查每个值是否相等
    for i = 1, #t1 do
        if t1[i] ~= t2[i] then
            return false
        end
    end
    return true
end

--是否包含
function common:is_table_contained(t1, t2)
    -- 检查空表情况
    if #t2 == 0 then
        return true 
    end
    if #t1 == 0 then
        return false
    end
    local countT1 = {}
    for _, value in ipairs(t1) do
        countT1[value] = (countT1[value] or 0) + 1
    end
    for _, value in ipairs(t2) do
        if countT1[value] == nil or countT1[value] <= 0 then
            return false
        end
        countT1[value] = countT1[value] - 1
    end
    return true
end
-- 删除t1中所有包含t2的元素并返回删除后的t1
function common:remove_elements(t1, t2)
    skynet.log("common:remove_elements")
    local result = {}
    for _, value in ipairs(t1) do
        if not common:contains(t2, value) then
            table.insert(result, value)
        end
    end
    skynet.log("#result",#result)
    return result
end
-- 切片
function common:slice(tbl, startIdx, endIdx)
    -- 创建一个新的表用于保存切片结果
    local result = {}
    startIdx = startIdx or 1  -- 默认为从索引 1 开始
    endIdx = endIdx or #tbl  -- 默认为到表的最后一个元素
    -- 如果起始索引大于终止索引，返回空表
    skynet.log("startIdx,endIdx,#tbl",startIdx,endIdx,#tbl)
    if startIdx < endIdx then
        for i = startIdx, endIdx do
            table.insert(result, tbl[i])
        end
        return result
    end
end
--合并
function common:merge_tables(t1, t2)
    local merged = {}
    for i, v in ipairs(t1) do
        table.insert(merged, v)
    end
    for i, v in ipairs(t2) do
        table.insert(merged, v)
    end
    return merged
end
return common