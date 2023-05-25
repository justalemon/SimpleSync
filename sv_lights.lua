-- The next sync fetch
local nextFetch = 0

local function getLightsSyncMode()
    return GetSyncMode("lights")
end

local function setLightsSyncMode(mode)
    return SetSyncMode(mode, "lights")
end

local function areLightsEnabled()
    return GetConvarInt("simplesync_lights", 1) ~= 0
end

local function setLightsEnabled(activation)
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

exports("getLightsSyncMode", getLightsSyncMode)
exports("setLightsSyncMode", setLightsSyncMode)
exports("areLightsEnabled", areLightsEnabled)
exports("setLights", setLightsEnabled) -- TODO: Rename to setLightsEnabled and mark deprecated

local function requestLights()
    Debug("Client " .. tostring(source) .. " (" .. GetPlayerName(source) .. ") requested the Light Activation")
    TriggerClientEvent("simplesync:setLights", source, areLightsEnabled())
end

RegisterNetEvent("simplesync:requestLights", requestLights)

local function updateLights()
    while true do
        local mode = getLightsSyncMode()

        if mode == 0 then
            local next = nextFetch

            if next == 0 then
                -- from 10 to 30 minutes by default
                local time = GetGameTimer() + math.random(GetConvarInt("simplesync_blackoutswitchmin", 600000),
                                                          GetConvarInt("simplesync_blackoutswitchmax", 1800000))
                nextFetch = time
                Debug("Setting first blackout time to " .. tostring(time))
            elseif GetGameTimer() >= next then
                local enabled = not areLightsEnabled()
                setLightsEnabled(enabled)
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

                nextFetch = time
                Debug("Artificial lights are now set to " .. tostring(enabled))
                Debug("Blackout will finish in " .. tostring(time))
            end
        end

        Citizen.Wait(0)
    end
end

Citizen.CreateThread(updateLights)

local function onLightsCommand(_, args, _)
    if #args == 0 then
        print("The activation of the lights is set to " .. tostring(areLightsEnabled()))
        return
    end

    local activation = ToBoolean(args[1])

    if activation == nil then
        print("The value specified is not valid!")
        return
    end

    setLightsEnabled(activation)
    TriggerClientEvent("simplesync:setLights", -1, activation)
    print("The light activation has been set to " .. tostring(activation))
end

local function onLightsModeCommand(_, args, _)
    GetOrSetMode(args, "lights")
end

RegisterCommand("lights", onLightsCommand, true)
RegisterCommand("lightsmode", onLightsModeCommand, true)
