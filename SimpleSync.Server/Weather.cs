using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleSync.Server
{
    /// <summary>
    /// CFX Script that synchronizes the Weather between clients.
    /// </summary>
    public class Weather : Common
    {
        #region Fields

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

        #endregion

        #region Ticks

        /// <summary>
        /// Changes the weather over time.
        /// </summary>
        [Tick]
        public async Task UpdateWeather()
        {
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
                        Debug.WriteLine($"We will try again in 60 seconds");
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
                        if (Convars.Debug)
                        {
                            Debug.WriteLine($"Weather was set to {currentWeather} from OpenWeather ID {response.ID}");
                        }
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
    }
}
