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

function SetWeather(from, to, duration)
    Debug("Started weather switch from " .. tostring(from) .. " to " .. tostring(to) .. " (" .. tostring(duration) .. "s)")

    ClearOverrideWeather()
    ClearWeatherTypePersist()

    SetWeatherTypeOvertimePersist(from, 0)

    if from ~= to and duration ~= 0 then
        ClearOverrideWeather()
        ClearWeatherTypePersist()

        local wait = duration / 1000 or 0

        SetWeatherTypeOvertimePersist(to, wait)
        Citizen.Wait(wait)
    end

    SetForceVehicleTrails(to == "XMAS")
    SetForcePedFootstepsTracks(to == "XMAS")
    WaterOverrideSetStrength(to == "XMAS" and 3 or 0)

    if to == "XMAS" then
        RequestScriptAudioBank("ICE_FOOTSTEPS", false, -1)
        RequestScriptAudioBank("SNOW_FOOTSTEPS", false, -1)
        RequestNamedPtfxAsset("core_snow");
        while not HasNamedPtfxAssetLoaded("core_snow") do
            Citizen.Wait(0)
        end
        UseParticleFxAsset("core_snow")
    else
        ReleaseNamedScriptAudioBank("ICE_FOOTSTEPS")
        ReleaseNamedScriptAudioBank("SNOW_FOOTSTEPS")
        if HasNamedPtfxAssetLoaded("core_snow") then
            RemoveNamedPtfxAsset("core_snow")
        end
    end
end

RegisterNetEvent("simplesync:setLights", SetLightsActivation)
RegisterNetEvent("simplesync:setTime", SetTime)
RegisterNetEvent("simplesync:setWeather", SetWeather)

function UpdateTime()
    while true do
        NetworkOverrideClockTime(currentHours, currentMinutes, 0)
        Citizen.Wait(0)
    end
end

Citizen.CreateThread(UpdateTime)

TriggerServerEvent("simplesync:requestLights")
TriggerServerEvent("simplesync:requestTime")
TriggerServerEvent("simplesync:requestWeather")

Debug("SimpleSync has started!")
