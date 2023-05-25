-- The next sync fetch
local nextFetch = 0
-- The valid weather names.
local validWeather = {
    "CLEAR",
    "EXTRASUNNY",
    "CLOUDS",
    "OVERCAST",
    "RAIN",
    "CLEARING",
    "THUNDER",
    "SMOG",
    "FOGGY",
    "XMAS",
    "SNOWLIGHT",
    "BLIZZARD"
}
-- The current weather.
local currentWeather = "EXTRASUNNY"
-- The weather that we are going to use.
local transitionWeather = "EXTRASUNNY"
-- The time where the transition between the weather.
local transitionFinish = 0
-- The weather switches to be used.
local switches = {}

local function getNextPossibleWeather(weather)
    local possibleWeathers = switches[weather]

    if possibleWeathers == nil then
        print("Warning: There is no entry for " .. weather .. " for dynamic switches, CLEAR was returned")
        return "CLEAR"
    end

    if #possibleWeathers == 0 then
        print("Warning: " .. weather .. " does not has dynamic switches, CLEAR was returned")
        return "CLEAR"
    end

    return possibleWeathers[math.random(#possibleWeathers)]
end

local function getWeatherSyncMode()
    return GetSyncMode("weather")
end

local function setWeatherSyncMode(mode)
    return SetSyncMode(mode, "weather")
end

local function getWeather()
    return currentWeather
end

local function setWeather(weather, force)
    if type(weather) ~= "string" then
        print("Unexpected weather type in SetWeather call, got " .. type(weather))
        return false
    end

    if force == nil then
        force = false
    end

    local valid = false

    for _, value in ipairs(validWeather) do
        if value == weather then
            valid = true
            break
        end
    end

    if not valid then
        print("Weather " .. weather .. " was not found")
        return false
    end

    local mode = getWeatherSyncMode()

    if mode == 0 then -- Dynamic
        if transitionFinish ~= 0 and not force then
            print("Attempted to change the weather naturally, but there is a transition in progress")
            return false
        end

        transitionWeather = weather
        local time = GetConvarInt("simplesync_transition", 10000)
        TriggerClientEvent("simplesync:setWeather", -1, currentWeather, transitionWeather, time);
        transitionFinish = GetGameTimer() + time
        Debug("Started weather switch to " .. weather .. " (from " .. currentWeather .. ")")
        Debug("The transition will finish on " .. tostring(transitionFinish))
        return true
    elseif mode == 1 then -- static
        currentWeather = weather
        TriggerClientEvent("simplesync:setWeather", -1, currentWeather, currentWeather, 0)
        Debug("Weather was set to " .. weather)
        return true
    end

    return false
end

local function getTransitionWeather()
    return transitionWeather
end

local function getNextWeatherFetch()
    return nextFetch
end

local function getWeatherTransitionFinish()
    return transitionFinish
end

exports("getWeatherSyncMode", getWeatherSyncMode)
exports("setWeatherSyncMode", setWeatherSyncMode)
exports("getWeather", getWeather)
exports("setWeather", setWeather)
exports("getTransitionWeather", getTransitionWeather)
exports("getNextWeatherFetch", getNextWeatherFetch)
exports("getWeatherTransitionFinish", getWeatherTransitionFinish)

local function requestWeather()
    Debug("Client " .. tostring(source) .. " (" .. GetPlayerName(source) .. ") requested the Weather")

    local mode = getWeatherSyncMode()

    if mode == 0 then -- dynamic
        local difference = GetGameTimer() - transitionFinish
        TriggerClientEvent("simplesync:setWeather", source, currentWeather, transitionWeather, difference)
    elseif mode == 1 or mode == 2 then
        TriggerClientEvent("simplesync:setWeather", source, currentWeather, currentWeather, 0)
    end
end

RegisterNetEvent("simplesync:requestWeather", requestWeather)

local function updateWeather()
    while true do
        local mode = getWeatherSyncMode()

        if mode == 0 then
            if GetGameTimer() >= nextFetch then
                local newWeather = getNextPossibleWeather(currentWeather)

                if currentWeather ~= newWeather then
                    setWeather(newWeather)
                else
                    Debug("The weather will stay the same as before (" .. currentWeather .. ")")
                end

                -- 10 to 30 minutes
                nextFetch = GetGameTimer() + math.random(GetConvarInt("simplesync_switchmin", 600000),
                                                         GetConvarInt("simplesync_switchmax", 1800000))
                Debug("The next weather will be fetched on " .. tostring(nextFetch))
                goto retnow
            elseif GetGameTimer() >= transitionFinish and currentWeather ~= transitionWeather then
                Debug("Transition from " .. currentWeather .. " to " .. transitionFinish .. " was finished")
                Debug("Setting transition weather as current and resetting time")

                currentWeather = transitionWeather
                transitionFinish = 0
            end
        end

        ::retnow::

        Citizen.Wait(0)
    end
end

Citizen.CreateThread(updateWeather)

local function onWeatherCommand(_, args, _)
    local mode = getWeatherSyncMode()

    if mode == -1 then
        Debug("Weather synchronization is Disabled")
        return
    end

    if #args == 0 then
        print("The current Weather is set to " .. currentWeather)
        return
    end

    if mode == 2 then
        print("The weather can't be changed if OpenWeatherMap is enabled")
        return
    end

    if args[1] == "?" then
        print("Allowed Weather values are: " .. table.concat(validWeather, ", "))
        return
    end

    weather = string.upper(args[1])
    force = string.upper(args[2] or "") == "FORCE"

    if mode == 0 and transitionFinish ~= 0 and not force then
        print("Weather can't be changed when there is a transition in progress and it has not been forced")
        return
    end

    setWeather(weather, force)
end

local function onWeatherModeCommand(_, args, _)
    GetOrSetMode(args, "weather")
end

local function onFetchNowCommand(_, _, _)
    local mode = getWeatherSyncMode()

    if mode ~= 2 then
        print("This command can only be used when the Weather Mode is set to Real.")
        return
    end

    nextFetch = 0
    print("The fetch time was reset!")
    print("OpenWeatherMap data should be fetched during the next tick")
end

RegisterCommand("weather", onWeatherCommand, true)
RegisterCommand("weathermode", onWeatherModeCommand, true)
RegisterCommand("fetchnow", onFetchNowCommand, true)

local function loadWeatherTransitions()
    local data = LoadResourceFile(GetCurrentResourceName(), "Switch.json")
    local parsed = json.decode(data)

    if parsed == nil then
        print("Error when parsing Switch.json: Please make sure that the file syntax is correct and try again")
        return
    end

    switches = parsed
end

loadWeatherTransitions()
