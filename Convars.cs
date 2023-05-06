﻿using CitizenFX.Core.Native;

namespace SimpleSync
{
#if SERVER
    /// <summary>
    /// A Class for a quick access to Convars.
    /// </summary>
    public static class Convars
    {
        /// <summary>
        /// If the debug logging is enabled.
        /// </summary>
        public static bool Debug => API.GetConvarInt("simplesync_debug", 0) != 0;

#if SERVER
        /// <summary>
        /// The current Sync mode for the Time.
        /// </summary>
        public static SyncMode TimeMode
        {
            get => (SyncMode)API.GetConvarInt("simplesync_modetime", 0);
            set => API.SetConvar("simplesync_modetime", ((int)value).ToString());
        }
        /// <summary>
        /// The current Sync mode for the Weather.
        /// </summary>
        public static SyncMode WeatherMode
        {
            get => (SyncMode)API.GetConvarInt("simplesync_modeweather", 0);
            set => API.SetConvar("simplesync_modeweather", ((int)value).ToString());
        }
        /// <summary>
        /// The current Sync mode for the Lights.
        /// Please note that Real synchronization is not possible for lights.
        /// </summary>
        public static SyncMode LightsMode
        {
            get => (SyncMode)API.GetConvarInt("simplesync_modelights", 1);
            set => API.SetConvar("simplesync_modelights", ((int)value).ToString());
        }

        /// <summary>
        /// On Dynamic Time: The time between the minute bumps.
        /// </summary>
        public static int Scale => API.GetConvarInt("simplesync_scale", 2000);
        /// <summary>
        /// On Dynamic Time: The number of minutes added when we bump the time.
        /// </summary>
        public static int Increase => API.GetConvarInt("simplesync_increase", 1);
        /// <summary>
        /// The OpenWeatherMap API Key.
        /// </summary>
        public static string OpenWeatherKey => API.GetConvar("simplesync_key", "");
        /// <summary>
        /// The city to query from OpenWeatherMap.
        /// </summary>
        public static string OpenWeatherCity => API.GetConvar("simplesync_city", "");
        /// <summary>
        /// On Real Time: The time zone to fetch the up to date time.
        /// </summary>
        public static string TimeZone
        {
            get => API.GetConvar("simplesync_timezone", "Pacific Standard Time");
            set => API.SetConvar("simplesync_timezone", value);
        }
        /// <summary>
        /// On Dynamic Weather: The time between the switch from Weather A and Weather B.
        /// </summary>
        public static int Transition => API.GetConvarInt("simplesync_transition", 10000);
        /// <summary>
        /// On Dynamic Weather: The minimum time between one weather and the other.
        /// </summary>
        public static int MinSwitch => API.GetConvarInt("simplesync_switchmin", 600000); // 10 Minutes
        /// <summary>
        /// On Dynamic Weather: The maximum time between one weather and the other.
        /// </summary>
        public static int MaxSwitch => API.GetConvarInt("simplesync_switchmax", 1800000); // 30 Minutes
#endif
    }
}
