using CitizenFX.Core;
using CitizenFX.Core.Native;
using SimpleSync.Shared;
using System;

namespace SimpleSync.Client
{
    /// <summary>
    /// CFX Script that applies the weather changes.
    /// </summary>
    public class Weather : BaseScript
    {
        #region Constructor

        public Weather()
        {
            // Once this script is loaded, ask the sever for sync
            TriggerServerEvent("simplesync:requestWeather");
            Logging.Log("Weather Synchronization has started");
        }

        #endregion

        #region Network Events

        /// <summary>
        /// Sets the Weather on this client.
        /// </summary>
        [EventHandler("simplesync:setWeather")]
        public void SetWeather(string from, string to, int duration)
        {
            // Log what we are going to do
            Logging.Log($"Started weather switch from {from} to {to} ({duration}s)");

            // Clear any overrides set
            API.ClearOverrideWeather();
            API.ClearWeatherTypePersist();

            // Force the origin weather
            API.SetWeatherTypeOvertimePersist(from, 0);

            // If both weathers are not the same and the duration is not zero
            if (from != to && duration != 0)
            {
                // Clear any overrides set
                API.ClearOverrideWeather();
                API.ClearWeatherTypePersist();
                // Convert the MS to S
                float ms = duration == 0 ? 0 : duration / 1000f;
                // And set the destination weather
                API.SetWeatherTypeOvertimePersist(to, ms);
            }
        }

        #endregion
    }
}
