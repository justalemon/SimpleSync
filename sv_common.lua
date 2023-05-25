-- The Synchronization Mode for Weather and Time.
local modes = {
    -- zero index for backwards compatibility
    [-1] = "Disabled",
    [0] = "Dynamic",
    [1] = "Static",
    [2] = "Real"
}
-- The next time where we should check for changes.
local nextFetch = {
    ["lights"] = 0,
    ["time"] = 0,
    ["weather"] = 0
}

function GetSyncMode(system)
    if nextFetch[system] == nil then
        return -1
    end

    local mode = GetConvarInt("simplesync_mode" .. system, 0)

    if modes[mode] == nil then
        print("Warning: Mode of " .. system .. " is invalid, setting it to the default")
        SetSyncMode(0, system)
        return 0
    end

    return mode
end

function SetSyncMode(mode, system)
    if type(mode) ~= "number" or modes[mode] == nil or nextFetch[system] == nil then
        return false
    end

    SetConvar("simplesync_mode" .. system, tostring(mode))
    nextFetch[system] = 0
    return true
end

function GetOrSetMode(args, system)
    if nextFetch[system] == nil then
        print("Unexpected system: " .. system)
        return
    end

    if #args == 0 then
        print("The current mode is set to " .. tostring(modes[GetSyncMode(system)]))
        return
    end

    local mode = tonumber(args[1])

    if modes[mode] == nil then
        print("The mode you have specified is not valid!")
        return
    end

    SetSyncMode(mode, system)
    print("The Sync mode has been set to " .. modes[mode])
end
