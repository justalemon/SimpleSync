function Debug(message)
    if GetConvarInt("simplesync_debug", 0) == 0 then
        return
    end

    print(message)
end
