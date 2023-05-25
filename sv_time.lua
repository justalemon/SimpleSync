-- The next sync fetch.
local nextFetch = 0
-- The current hours.
local currentHours = 0
-- The current minutes.
local currentMinutes = 0

local function getTimeSyncMode()
    return GetSyncMode("time")
end

local function setTimeSyncMode(mode)
    return SetSyncMode(mode, "time")
end

local function setTime(hours, minutes)
    hours = tonumber(hours)
    minutes = tonumber(minutes)

    if hours == nil or minutes == nil then
        return
    end

    currentHours = hours
    currentMinutes = minutes

    TriggerClientEvent("simplesync:setTime", -1, hours, minutes)
    if GetConvarInt("simplesync_debugbump", 0) ~= 0 then
        Debug("Time set to " .. tostring(hours) .. ":" .. tostring(minutes) .. " via SetTime")
    end
end

local function getHours()
    return currentHours
end

local function getMinutes()
    return currentMinutes
end

exports("getTimeSyncMode", getTimeSyncMode)
exports("setTimesSyncMode", setTimeSyncMode)
exports("setTime", setTime)
exports("getHours", getHours)
exports("getMinutes", getMinutes)

local function requestTime()
    Debug("Client " .. tostring(source) .. " (" .. GetPlayerName(source) .. ") requested the Time")
    TriggerClientEvent("simplesync:setTime", -1, currentHours, currentMinutes)
end

RegisterNetEvent("simplesync:requestTime", requestTime)

local function updateTime()
    while true do
        local mode = getTimeSyncMode()

        if mode == 0 then
            if GetGameTimer() >= nextFetch then
                local totalMinutes = (currentHours * 60) + currentMinutes + GetConvarInt("simplesync_increase", 1)
                local hours, minutes = MinutesToHM(totalMinutes)
                setTime(hours, minutes)
                nextFetch = GetGameTimer() + GetConvarInt("simplesync_scale", 2000)

                if GetConvarInt("simplesync_debugbump", 0) ~= 0 then
                    Debug("Time bump complete!")
                end
            end
        elseif mode == 1 then -- luacheck: ignore 542
            -- do nothing
        elseif mode == 2 then -- luacheck: ignore 542
            -- TODO: Implement real mode
        end

        Citizen.Wait(0)
    end
end

Citizen.CreateThread(updateTime)

local function onTimeCommand(_, args, raw)
    local mode = getTimeSyncMode()

    if mode == -1 then
        print("Time Synchronization is disabled")
        return
    end

    if #args == 0 then
        print("The time is " .. string.format("%.2d", currentHours) .. ":" .. string.format("%.2d", currentMinutes))
        return
    end

    local hours
    local minutes

    if #args == 1 then
        local split = {}
        for slice in string.gmatch(args[1], "([^:]+):?") do
            split[#split + 1] = slice
        end

        hours = tonumber(split[1])
        minutes = tonumber(split[2]) or 0
    elseif #args == 2 then
        hours = tonumber(args[1])
        minutes = tonumber(args[2])
    end

    if hours == nil or minutes == nil then
        print("Expected HH:MM, HH or HH MM, got " .. raw)
        return
    end

    -- TODO: Add overflow checks

    setTime(hours, minutes)
    print("The time is now " .. string.format("%.2d", currentHours) .. ":" .. string.format("%.2d", currentMinutes))
end

local function onTimeModeCommand(_, args, _)
    GetOrSetMode(args, "time")
end

local function onGameTimerCommand()
    print("Current Game Time is " .. tostring(GetGameTimer()))
end

RegisterCommand("time", onTimeCommand, true)
RegisterCommand("timemode", onTimeModeCommand, true)
RegisterCommand("gametimer", onGameTimerCommand, true)
