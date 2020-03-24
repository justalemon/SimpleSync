using CitizenFX.Core;
using CitizenFX.Core.Native;
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
        }

        #endregion

        #region Network Events

        /// <summary>
        /// Sets the Weather on this client.
        /// </summary>
        [EventHandler("simplesync:setWeather")]
        public void SetWeather(string weather)
        {
            // Clear any overrides set
            API.ClearOverrideWeather();
            API.ClearWeatherTypePersist();
            // And set the weather instantly
            API.SetWeatherTypeOvertimePersist(weather, 0);
        }

        #endregion
    }
}
