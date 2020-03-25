using CitizenFX.Core;
using System;
using System.Collections.Generic;

namespace SimpleSync.Server
{
    /// <summary>
    /// CFX Script that synchronizes the Weather between clients.
    /// </summary>
    public class Weather : BaseScript
    {
        #region Fields

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
        /// The current weather synchronized.
        /// </summary>
        private string currentWeather = "EXTRASUNNY";

        #endregion

        #region Constructor

        public Weather()
        {
            Exports.Add("setWeather", new Action<string>(SetWeather));
        }

        #endregion

        #region Exports

        public void SetWeather(string weather)
        {
            // If the weather is on the list
            if (validWeather.Contains(weather))
            {
                // Save it
                currentWeather = weather;
                // And send it to the clients
                TriggerClientEvent("simplesync:setWeather", weather);
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
            player.TriggerEvent("simplesync:setWeather", currentWeather);
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
            TriggerClientEvent("simplesync:setWeather", newWeather);
            // Save it for later use
            currentWeather = newWeather;
            // And notify about it
            Debug.WriteLine($"The weather was set to {newWeather}");
        }

        #endregion
    }
}
