-- The current hours.
local currentHours = 0
-- The current minutes.
local currentMinutes = 0

local function setTime(hour, minute)
    currentHours = hour
    currentMinutes = minute
    Debug("Received time " .. string.format("%.2d", currentHours) .. ":" .. string.format("%.2d", currentMinutes))
end

RegisterNetEvent("simplesync:setTime", setTime)

local function updateTime()
    while true do
        NetworkOverrideClockTime(currentHours, currentMinutes, 0)
        Citizen.Wait(0)
    end
end

Citizen.CreateThread(updateTime)

TriggerServerEvent("simplesync:requestTime")
