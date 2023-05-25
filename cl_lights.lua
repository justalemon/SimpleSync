local function setLightsActivation(enabled)
    -- Over here, True is Disabled and False is Enabled
    -- Thanks Rockstar Games!
    SetArtificialLightsState(not enabled)
end

RegisterNetEvent("simplesync:setLights", setLightsActivation)

TriggerServerEvent("simplesync:requestLights")
