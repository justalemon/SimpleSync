using CitizenFX.Core;
using CitizenFX.Core.Native;
using Flurl.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
        /// The URL to fetch the up to date Weather from OpenWeather.
        /// </summary>
        private const string openWeatherURL = "http://api.openweathermap.org/data/2.5/weather?q={0}&appid={1}";
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
        /// The current weather synchronized.
        /// </summary>
        private string currentWeather = "EXTRASUNNY";

        #endregion

        #region Constructor

        public Weather()
        {
            // Add the exports
            Exports.Add("setWeather", new Action<string>(SetWeather));
            // And log some important commands
            Logging.Log("Weather Synchronization has started");
            Logging.Log($"Sync Type is set to {Convars.WeatherType}");
            Logging.Log(string.IsNullOrWhiteSpace(Convars.OpenWeatherKey) ? "No OpenWeather API Key is set" : "An OpenWeather API Key is present");
            Logging.Log(string.IsNullOrWhiteSpace(Convars.OpenWeatherCity) ? "No OpenWeather City is set" : $"OpenWeather City is set to {Convars.OpenWeatherCity}");
        }

        #endregion

        #region Exports

        public void SetWeather(string weather)
        {
            // If the weather is on the list and the Weather Type is not set to Real
            if (validWeather.Contains(weather) && Convars.WeatherType != SyncType.Real)
            {
                // Save it
                currentWeather = weather;
                // And send it to the clients
                TriggerClientEvent("simplesync:setWeather", weather, weather, 0);
                Logging.Log($"Weather set to {weather} via exports");
            }
        }

        #endregion

        #region Network Events

        /// <summary>
        /// Sends the up to date weather to the server.
        /// </summary>
        [EventHandler("simplesync:requestWeather")]
        public void RequestWeather([FromSource]Player player)
        {
            // Just tell the client to set the correct weather
            player.TriggerEvent("simplesync:setWeather", currentWeather, currentWeather, 0);
            Logging.Log($"Client {player.Handle} ({player.Name}) requested the Weather");
        }

        #endregion

        #region Ticks

        /// <summary>
        /// Changes the weather over time.
        /// </summary>
        [Tick]
        public async Task UpdateWeather()
        {
            // If the Weather is set to Real
            if (Convars.WeatherType == SyncType.Real)
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

                    // Otherwise, format the correct URL
                    string url = string.Format(openWeatherURL, Convars.OpenWeatherCity, Convars.OpenWeatherKey);
                    // Make the request
                    HttpResponseMessage response = await url.WithHeader("User-Agent", "SimpleSync (+https://github.com/justalemon/SimpleSync)").GetAsync();
                    // And get the text of the response
                    string text = await response.Content.ReadAsStringAsync();

                    // If the status code is not 200
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        // Show a message with the status code and content
                        Debug.WriteLine($"OpenWeather returned code {response.StatusCode}! ({text})");
                        // Leave a 60 seconds cooldown
                        nextFetch = API.GetGameTimer() + 60000;
                        // And return
                        return;
                    }

                    // Otherwise, parse the response into a JObject
                    JObject obj = JObject.Parse(text);
                    // And get the Weather ID
                    int id = (int)obj["weather"][0]["id"];

                    // If we have a GTA V weather for that ID
                    if (openWeatherCodes.ContainsKey(id))
                    {
                        // Save the weather
                        currentWeather = openWeatherCodes[id];
                        // And send it to all of the clients
                        TriggerClientEvent("simplesync:setWeather", currentWeather, currentWeather, 0);
                        Logging.Log($"Weather was set to {currentWeather} from OpenWeather ID {id}");
                    }
                    // If we don't have it
                    else
                    {
                        // Log it in the console
                        Debug.WriteLine($"Weather ID {id} does not has a GTA V Weather!");
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

            // Convert the first parameter to upper case
            string newWeather = args[0].ToString().ToUpperInvariant();

            // If the first parameter is a ?
            if (newWeather == "?")
            {
                // Show the allowed weather values and return
                Debug.WriteLine("Allowed Weather values are: " + string.Join(", ", validWeather));
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
            TriggerClientEvent("simplesync:setWeather", newWeather, newWeather, 0);
            // Save it for later use
            currentWeather = newWeather;
            // And notify about it
            Debug.WriteLine($"The weather was set to {newWeather}");
        }

        #endregion
    }
}
