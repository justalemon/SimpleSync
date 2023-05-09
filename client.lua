-- The current hours.
local currentHours = 0
-- The current minutes.
local currentMinutes = 0

function SetLightsActivation(enabled)
    -- Over here, True is Disabled and False is Enabled
    -- Thanks Rockstar Games!
    SetArtificialLightsState(not enabled)
end

function SetTime(hour, minute)
    currentHours = hour
    currentMinutes = minute
    Debug("Received time " .. string.format("%.2d", currentHours) .. ":" .. string.format("%.2d", currentMinutes))
end

RegisterNetEvent("simplesync:setLights", SetLightsActivation)
RegisterNetEvent("simplesync:setTime", SetTime)

function UpdateTime()
    while true do
        NetworkOverrideClockTime(currentHours, currentMinutes, 0)
        Citizen.Wait(0)
    end
end

Citizen.CreateThread(UpdateTime)

TriggerServerEvent("simplesync:requestLights")
TriggerServerEvent("simplesync:requestTime")
