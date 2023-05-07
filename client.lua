function SetLightsActivation(enabled)
    -- Over here, True is Disabled and False is Enabled
    -- Thanks Rockstar Games!
    SetArtificialLightsState(not enabled)
end

RegisterNetEvent("simplesync:setLights", SetLightsActivation)

TriggerServerEvent("simplesync:requestLights")
