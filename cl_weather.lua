local function setWeather(from, to, duration)
    Debug("Started weather switch from " .. from .. " to " .. to .. " (" .. tostring(duration) .. "s)")

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

    -- luacheck: ignore 113
    SetForceVehicleTrails(to == "XMAS")
    -- luacheck: ignore 113
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

RegisterNetEvent("simplesync:setWeather", setWeather)

TriggerServerEvent("simplesync:requestWeather")
