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
-- The current hours.
local currentHours = 0
-- The current minutes.
local currentMinutes = 0

function GetSyncMode(system)
    if nextFetch[system] == nil then
        return -1
    end

    return GetConvarInt("simplesync_mode" .. system, 1)
end

function SetSyncMode(mode, system)
    if type(mode) ~= "number" or modes[mode] == nil or nextFetch[system] == nil then
        return false
    end

    SetConvar("simplesync_mode" .. system, tostring(mode))
    nextFetch[system] = 0
    return true
end

function AreLightsEnabled()
   return GetConvarInt("simplesync_lights", 1) ~= 0
end

function SetLightsEnabled(activation)
    value = 0

    if type(activation) == "number" then
        if activation == 0 then
            value = 0
        else
            value = 1
        end
    elseif type(activation) == "boolean" then
        if activation then
            value = 1
        else
            value = 0
        end
    else
        return
    end

    SetConvar("simplesync_lights", tostring(value))
end

function GetLightsSyncMode()
    return GetSyncMode("lights")
end
function SetLightsSyncMode(mode)
    return SetSyncMode(mode, "lights")
end

function GetTimeSyncMode()
    return GetSyncMode("time")
end
function SetTimeSyncMode(mode)
    return SetSyncMode(mode, "time")
end

function GetWeatherSyncMode()
    return GetSyncMode("weather")
end
function SetWeatherSyncMode(mode)
    return SetSyncMode(mode, "weather")
end

function SetTime(hours, minutes)
    hours = tonumber(hours)
    minutes = tonumber(minutes)

    if hours == nil or minutes == nil then
        return
    end

    currentHours = hours
    currentMinutes = minutes

    TriggerClientEvent("simplesync:setTime", -1, hours, minutes)
    Debug("Time set to " .. tostring(hours) .. ":" .. tostring(minutes) .. " via SetTime")
end

exports("areLightsEnabled", AreLightsEnabled)
exports("setLights", SetLightsEnabled) -- TODO: Rename to setLightsEnabled and mark deprecated
exports("getLightsSyncMode", GetLightsSyncMode)
exports("setLightsSyncMode", SetLightsSyncMode)
exports("getTimeSyncMode", GetTimeSyncMode)
exports("setTimesSyncMode", SetTimeSyncMode)
exports("getWeatherSyncMode", GetWeatherSyncMode)
exports("setWeatherSyncMode", SetWeatherSyncMode)

exports("setTime", SetTime)

function RequestLights()
    Debug("Client " .. tostring(source) .. " (" .. GetPlayerName(source) .. ") requested the Light Activation")
    TriggerClientEvent("simplesync:setLights", source, AreLightsEnabled())
end

function RequestTime()
    Debug("Client " .. tostring(source) .. " (" .. GetPlayerName(source) .. ") requested the Time")
    TriggerClientEvent("simplesync:setTime", -1, currentHours, currentMinutes)
end

RegisterNetEvent("simplesync:requestLights", RequestLights)
RegisterNetEvent("simplesync:requestTime", RequestTime)

function UpdateLights()
    while true do
        local mode = GetLightsSyncMode()

        if mode == 0 then
            local next = nextFetch["lights"]

            if next == 0 then
                -- from 10 to 30 minutes by default
                local time = GetGameTimer() + math.random(GetConvarInt("simplesync_blackoutswitchmin", 600000),
                                                          GetConvarInt("simplesync_blackoutswitchmax", 1800000))
                nextFetch["lights"] = time
                Debug("Setting first blackout time to " .. tostring(time))
            elseif GetGameTimer() >= next then
                local enabled = not AreLightsEnabled()
                SetLightsEnabled(enabled)
                TriggerClientEvent("simplesync:setLights", -1, enabled)

                local time

                if enabled then
                    -- from 10 to 30 minutes by default
                    time = GetGameTimer() + math.random(GetConvarInt("simplesync_blackoutswitchmin", 600000),
                                                        GetConvarInt("simplesync_blackoutswitchmax", 1800000))
                else
                    -- from 1 to 2 minutes by default
                    time = GetGameTimer() + math.random(GetConvarInt("simplesync_blackoutdurationmin", 60000),
                                                        GetConvarInt("simplesync_blackoutdurationmax", 120000))
                end

                nextFetch["lights"] = time
                Debug("Artificial lights are now set to " .. tostring(enabled))
                Debug("Blackout will finish in " .. tostring(time))
            end
        end

        Citizen.Wait(0)
    end
end

Citizen.CreateThread(UpdateLights)

function OnLightsCommand(_, args, _)
    if #args == 0 then
        print("The activation of the lights is set to " .. tostring(AreLightsEnabled()))
        return
    end

    local activation = ToBoolean(args[1])

    if activation == nil then
        print("The value specified is not valid!")
        return
    end

    SetLightsEnabled(activation)
    TriggerClientEvent("simplesync:setLights", -1, activation)
    print("The light activation has been set to " .. tostring(activation))
end

function OnLightsModeCommand(_, args, _)
    if #args == 0 then
        print("The current lights mode is set to " .. tostring(modes[GetLightsSyncMode()]))
        return
    end

    local mode = tonumber(args[1])

    if modes[mode] == nil then
        print("The mode you have specified is not valid!")
        return
    end

    SetLightsSyncMode(mode)
    print("The Lights Sync mode has been set to " .. modes[mode])
end

RegisterCommand("lights", OnLightsCommand, true)
RegisterCommand("lightsmode", OnLightsModeCommand, true)
