using CitizenFX.Core;
using CitizenFX.Core.Native;
using SimpleSync.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleSync.Server
{
    /// <summary>
    /// CFX Script that synchronizes the Weather between clients.
    /// </summary>
    public class Weather : BaseScript
    {
        #region Fields

        /// <summary>
        /// The RNG thingy.
        /// </summary>
        private static readonly Random random = new Random();
        /// <summary>
        /// The next time where we should check the weather.
        /// </summary>
        private long nextFetch = 0;
        /// <summary>
        /// The valid weather names.
        /// </summary>
        private static readonly List<string> validWeather = new List<string>
        {
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
        };
        /// <summary>
        /// Dictionary with the associations between OpenWeather condition codes and GTA V Weather hashes.
        /// https://openweathermap.org/weather-conditions
        /// </summary>
        private static readonly Dictionary<int, string> openWeatherCodes = new Dictionary<int, string>
        {
            // Group 2xx: Thunderstorm
            { 200, "THUNDER" }, // thunderstorm with light rain	
            { 201, "THUNDER" }, // thunderstorm with rain
            { 202, "THUNDER" }, // thunderstorm with heavy rain
            { 210, "THUNDER" }, // light thunderstorm
            { 211, "THUNDER" }, // thunderstorm
            { 212, "THUNDER" }, // heavy thunderstorm
            { 221, "THUNDER" }, // ragged thunderstorm
            { 230, "THUNDER" }, // thunderstorm with light drizzle
            { 231, "THUNDER" }, // thunderstorm with drizzle
            { 232, "THUNDER" }, // thunderstorm with heavy drizzle
            // Group 3xx: Drizzle
            // TODO: Use _SET_RAIN_FX_INTENSITY to really make it look like Drizzle (0.0f?)
            { 300, "RAIN" }, // light intensity drizzle
            { 301, "RAIN" }, // drizzle
            { 302, "RAIN" }, // heavy intensity drizzle
            { 310, "RAIN" }, // light intensity drizzle rain
            { 311, "RAIN" }, // drizzle rain
            { 312, "RAIN" }, // heavy intensity drizzle rain
            { 313, "RAIN" }, // shower rain and drizzle
            { 314, "RAIN" }, // heavy shower rain and drizzle
            { 321, "RAIN" }, // shower drizzle
            // Group 5xx: Rain
            { 500, "RAIN" }, // light rain
            { 501, "RAIN" }, // moderate rain
            { 502, "RAIN" }, // heavy intensity rain
            { 503, "RAIN" }, // very heavy rain
            { 504, "RAIN" }, // extreme rain
            { 511, "RAIN" }, // freezing rain
            { 520, "RAIN" }, // light intensity shower rain
            { 521, "RAIN" }, // shower rain
            { 522, "RAIN" }, // heavy intensity shower rain
            { 531, "RAIN" }, // ragged shower rain
            // Group 6xx: Snow
            { 600, "XMAS" }, // light snow
            { 601, "XMAS" }, // Snow
            { 602, "XMAS" }, // Heavy snow
            { 611, "XMAS" }, // Sleet
            { 612, "XMAS" }, // Light shower sleet
            { 613, "XMAS" }, // Shower sleet
            { 615, "XMAS" }, // Light rain and snow
            { 616, "XMAS" }, // Rain and snow
            { 620, "XMAS" }, // Light shower snow
            { 621, "XMAS" }, // Shower snow
            { 622, "XMAS" }, // Heavy shower snow
            // Group 7xx: Atmosphere
            // TODO: See if there are better combinations
            { 701, "SMOG" }, // mist
            { 711, "SMOG" }, // Smoke
            { 721, "SMOG" }, // Haze
            { 731, "SMOG" }, // sand/ dust whirls
            { 741, "SMOG" }, // fog
            { 751, "SMOG" }, // sand
            { 761, "SMOG" }, // dust
            { 762, "SMOG" }, // volcanic ash	
            { 771, "SMOG" }, // squalls
            { 781, "SMOG" }, // tornado
            // Group 800: Clear
            { 800, "CLEAR" }, // clear sky
            // Group 80x: Clouds
            // TODO: Use LOAD_CLOUD_HAT and _SET_CLOUD_HAT_OPACITY for better cloud formations
            { 801, "CLOUDS" }, // few clouds: 11-25%
            { 802, "CLOUDS" }, // scattered clouds: 25-50%
            { 803, "CLOUDS" }, // broken clouds: 51-84%
            { 804, "CLOUDS" }, // overcast clouds: 85-100%
        };
        /// <summary>
        /// The current weather.
        /// </summary>
        private string currentWeather = "EXTRASUNNY";
        /// <summary>
        /// The weather that we are going to use.
        /// </summary>
        private string transitionWeather = "EXTRASUNNY";
        /// <summary>
        /// The time where the transition between the weather.
        /// </summary>
        private long transitionFinish = 0;

        #endregion

        #region Constructor

        public Weather()
        {
            // Add the exports
            Exports.Add("getWeatherSyncMode", new Func<int>(() => API.GetConvarInt("simplesync_modeweather", 0)));
            Exports.Add("setTimeSyncMode", new Func<int, bool>(SetSyncMode));

            Exports.Add("getWeather", new Func<string>(() => currentWeather));
            Exports.Add("setWeather", new Func<string, bool>(SetWeather));
            Exports.Add("getTransitionWeather", new Func<string>(() => transitionWeather));

            Exports.Add("getNextWeatherFetch", new Func<long>(() => nextFetch));

            Exports.Add("getWeatherTransitionFinish", new Func<long>(() => transitionFinish));
            // And log some important commands
            Logging.Log("Weather Synchronization has started");
            Logging.Log($"Sync Mode is set to {Convars.WeatherMode}");
            Logging.Log(string.IsNullOrWhiteSpace(Convars.OpenWeatherKey) ? "No OpenWeather API Key is set" : "An OpenWeather API Key is present");
            Logging.Log(string.IsNullOrWhiteSpace(Convars.OpenWeatherCity) ? "No OpenWeather City is set" : $"OpenWeather City is set to {Convars.OpenWeatherCity}");
        }

        #endregion

        #region Tools

        /// <summary>
        /// Gets a new weather based on the current one.
        /// </summary>
        /// <returns>The new weather.</returns>
        public string NextWeather()
        {
            // 25% chance of keeping the same weather (0-1-2-3)
            if (random.Next(0, 4) == 0)
            {
                return currentWeather;
            }

            // Otherwise, choose one that is correct based on the current one
            if (currentWeather == "EXTRASUNNY")
            {
                // Extra Sunny goes to either Clear or Clouds
                switch (random.Next(0, 2))
                {
                    case 0:
                        return "CLEAR";
                    case 1:
                        return "CLOUDS";
                }
            }
            else if (currentWeather == "CLEAR" || currentWeather == "CLOUDS")
            {
                // This two go to Clearing or Overcast
                switch (random.Next(0, 2))
                {
                    case 0:
                        return "CLEARING";
                    case 1:
                        return "OVERCAST";
                }
            }
            else if (currentWeather == "CLEARING" || currentWeather == "OVERCAST")
            {
                // Clearing and Overcast can change to Rain, Fog, Cloudy, Extra Sunny, Smog or Clear
                switch (random.Next(0, 6))
                {
                    case 0:
                        return "RAIN";
                    case 1:
                        return "FOGGY";
                    case 2:
                        return "CLOUDS";
                    case 3:
                        return "EXTRASUNNY";
                    case 4:
                        return "SMOG";
                    case 5:
                        return "CLEAR";
                }
            }
            else if (currentWeather == "RAIN")
            {
                // 33% chance between Thunder, Xmas Snow and Clearing
                switch (random.Next(0, 3))
                {
                    case 0:
                        return "THUNDER";
                    case 1:
                        return "XMAS";
                    case 2:
                        return "CLEARING";
                }
            }
            else if (currentWeather == "THUNDER")
            {
                // 50-50 chance between Rain and Clearing
                switch (random.Next(0, 2))
                {
                    case 0:
                        return "RAIN";
                    case 1:
                        return "CLEARING";
                }
            }
            else if (currentWeather == "SMOG" || currentWeather == "FOGGY")
            {
                // The Smog and Fog will always be cleared
                return "CLEARING";
            }
            else if (currentWeather == "XMAS")
            {
                // Xmas Snow will go back to Rain or Overcast
                switch (random.Next(0, 2))
                {
                    case 0:
                        return "RAIN";
                    case 1:
                        return "OVERCAST";
                }
            }

            // If somehow we missed everything back there, go back to clearing
            return "CLEARING";
        }

        #endregion

        #region Exports

        public bool SetSyncMode(int mode)
        {
            // If is not defined on the enum, return
            if (!Enum.IsDefined(typeof(SyncMode), mode))
            {
                return false;
            }
            // Otherwise, save the value
            API.SetConvar("simplesync_modeweather", mode.ToString());
            // And reset the fetch time
            nextFetch = 0;
            return true;
        }

        public bool SetWeather(string weather)
        {
            // If the weather is not valid, the Real Mode is enabled or there is a transition, return
            if (!validWeather.Contains(weather) || Convars.WeatherMode == SyncMode.Real || transitionFinish != 0)
            {
                return false;
            }

            // If the dynamic mode is being used, save it for a transition
            if (Convars.WeatherMode == SyncMode.Dynamic)
            {
                transitionWeather = weather;
                TriggerClientEvent("simplesync:setWeather", currentWeather, transitionWeather, Convars.Transition);
                transitionFinish = API.GetGameTimer() + Convars.Transition;
                Logging.Log($"Started weather switch to {weather} (from {currentWeather})");
                Logging.Log($"The transition will finish on {transitionFinish}");
                return true;
            }
            // If the static mode is being used, send it instantly
            else if (Convars.WeatherMode == SyncMode.Static)
            {
                currentWeather = weather;
                TriggerClientEvent("simplesync:setWeather", currentWeather, currentWeather, 0);
                Logging.Log($"Weather was set to {weather}");
                return true;
            }

            // If we got here, the weather type is invalid
            return false;
        }

        #endregion

        #region Network Events

        /// <summary>
        /// Sends the up to date weather to the server.
        /// </summary>
        [EventHandler("simplesync:requestWeather")]
        public void RequestWeather([FromSource]Player player)
        {
            Logging.Log($"Client {player.Handle} ({player.Name}) requested the Weather");

            switch (Convars.WeatherMode)
            {
                // If Dynamic weather is enabled
                case SyncMode.Dynamic:
                    player.TriggerEvent("simplesync:setWeather", currentWeather, transitionWeather, API.GetGameTimer() - transitionFinish);
                    return;
                // If the weather is set to Static or Real
                case SyncMode.Static:
                case SyncMode.Real:
                    player.TriggerEvent("simplesync:setWeather", currentWeather, currentWeather, 0);
                    return;
            }
        }

        #endregion

        #region Ticks

        /// <summary>
        /// Changes the weather over time.
        /// </summary>
        [Tick]
        public async Task UpdateWeather()
        {
            // If the Weather is set to Dynamic
            if (Convars.WeatherMode == SyncMode.Dynamic)
            {
                // If the game time is over or equal than the next update/fetch time
                if (API.GetGameTimer() >= nextFetch)
                {
                    // Get a random weather
                    string newWeather = NextWeather();

                    // If the weather is not the same as the current one, switch to it
                    if (currentWeather != newWeather)
                    {
                        SetWeather(newWeather);
                    }
                    // Otherwise, don't do a switch
                    else
                    {
                        Logging.Log($"The weather will stay the same as before ({newWeather})");
                    }

                    // Then, save the time for the next fetch
                    nextFetch = API.GetGameTimer() + random.Next(Convars.MinSwitch, Convars.MaxSwitch);

                    Logging.Log($"The next weather will be fetched on {nextFetch}");
                    return;
                }
                // If the game time is over or equal than the end of the transition and two weather do not match
                else if (API.GetGameTimer() >= transitionFinish && currentWeather != transitionWeather)
                {
                    Logging.Log($"Transition from {currentWeather} to {transitionWeather} was finished");
                    Logging.Log($"Setting transition weather as current and resetting time");
                    // Set the transition weather as the current weather
                    currentWeather = transitionWeather;
                    // And set the transition time to zero
                    transitionFinish = 0;
                }
            }
            // If the Weather is set to Real
            else if (Convars.WeatherMode == SyncMode.Real)
            {
                // If the game time is over or equal than the next fetch time
                if (API.GetGameTimer() >= nextFetch)
                {
                    // If the OpenWeather API Key is empty, return
                    if (string.IsNullOrWhiteSpace(Convars.OpenWeatherKey))
                    {
                        return;
                    }
                    // If the weather API is empty, return
                    if (string.IsNullOrWhiteSpace(Convars.OpenWeatherCity))
                    {
                        return;
                    }

                    // Make the request to OpenWeatherMap
                    WeatherData response = await OpenWeather.GetWeather();

                    // If the request failed
                    if (response == null)
                    {
                        // Leave a 60 seconds cooldown
                        nextFetch = API.GetGameTimer() + 60000;
                        // And return
                        return;
                    }

                    // If we have a GTA V weather for that ID
                    if (openWeatherCodes.ContainsKey(response.ID))
                    {
                        // Save the weather
                        currentWeather = openWeatherCodes[response.ID];
                        // And send it to all of the clients
                        TriggerClientEvent("simplesync:setWeather", currentWeather, currentWeather, 0);
                        Logging.Log($"Weather was set to {currentWeather} from OpenWeather ID {response.ID}");
                    }
                    // If we don't have it
                    else
                    {
                        // Log it in the console
                        Debug.WriteLine($"Weather ID {response.ID} does not has a GTA V Weather!");
                    }

                    // Finally, add a delay of 30 seconds
                    nextFetch = API.GetGameTimer() + 30000;
                }
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to Get or Set the current weather.
        /// </summary>
        [Command("weather", Restricted = true)]
        public void WeatherCommand(int source, List<object> args, string raw)
        {
            // If there are no arguments specified
            if (args.Count == 0)
            {
                // Show the current weather and return
                Debug.WriteLine($"The current Weather is set to {currentWeather}");
                return;
            }

            // If we have OpenWeather synchronization enabled, the weather can't be changed
            if (Convars.WeatherMode == SyncMode.Real)
            {
                Debug.WriteLine("The weather can't be changed if OpenWeatherMap is enabled");
                return;
            }

            // Convert the first parameter to upper case
            string newWeather = args[0].ToString().ToUpperInvariant();

            // If the first parameter is a ?
            if (newWeather == "?")
            {
                // Show the allowed weather values and return
                Debug.WriteLine("Allowed Weather values are: " + string.Join(", ", validWeather));
                return;
            }

            // If weather is set to dynamic and there is a switch in progress, return
            if (Convars.WeatherMode == SyncMode.Dynamic && transitionFinish != 0)
            {
                Debug.WriteLine($"Weather can't be changed when there is a transition in progress");
                return;
            }

            // If the weather is not on the list
            if (!validWeather.Contains(newWeather))
            {
                // Notify about it and return
                Debug.WriteLine($"The weather {newWeather} is not valid");
                Debug.WriteLine($"Use /weather ? for a list of weather");
                return;
            }

            // At this point, the weather is valid
            // So go ahead and set it for all of the players
            SetWeather(newWeather);
        }

        #endregion
    }
}
