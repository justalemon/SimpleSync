function Debug(message)
    if GetConvarInt("simplesync_debug", 0) == 0 then
        return
    end

    print(message)
end

function ToBoolean(value)
    if value == "true" then
        return true
    elseif value == "false" then
        return false
    end

    return nil
end
