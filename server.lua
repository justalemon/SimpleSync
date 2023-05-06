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

exports("areLightsEnabled", AreLightsEnabled)
exports("setLights", SetLightsEnabled) -- TODO: Rename to setLightsEnabled and mark deprecated
exports("getLightsSyncMode", GetLightsSyncMode)
exports("setLightsSyncMode", SetLightsSyncMode)
exports("getTimeSyncMode", GetTimeSyncMode)
exports("setTimesSyncMode", SetTimeSyncMode)
exports("getWeatherSyncMode", GetWeatherSyncMode)
exports("setWeatherSyncMode", SetWeatherSyncMode)

function RequestLights()
    Debug("Client " .. tostring(source) .. " (" .. GetPlayerName(source) .. ") requested the Light Activation")
    TriggerClientEvent("simplesync:setLights", source, AreLightsEnabled())
end

RegisterNetEvent("simplesync:requestLights", RequestLights)

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
