function Debug(message)
    if GetConvarInt("simplesync_debug", 0) == 0 then
        return
    end

    print(message)
end

function MinutesToHM(totalMinutes)
    local hours = math.floor(totalMinutes / 60)
    local minutes = totalMinutes - (hours * 60)
    return hours, minutes
end

function ToBoolean(value)
    if value == "true" then
        return true
    elseif value == "false" then
        return false
    end

    return nil
end
