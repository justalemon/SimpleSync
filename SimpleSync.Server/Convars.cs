using CitizenFX.Core.Native;

namespace SimpleSync.Server
{
    /// <summary>
    /// A Class for a quick access to Convars.
    /// </summary>
    public static class Convars
    {
        /// <summary>
        /// The current Sync type for the Time.
        /// </summary>
        public static SyncType TimeType
        {
            get => (SyncType)API.GetConvarInt("simplesync_typetime", 0);
            set => API.SetConvar("simplesync_typetime", ((int)value).ToString());
        }
        /// <summary>
        /// The current Sync type for the Weather.
        /// </summary>
        public static SyncType WeatherType
        {
            get => (SyncType)API.GetConvarInt("simplesync_typeweather", 0);
            set => API.SetConvar("simplesync_typeweather", ((int)value).ToString());
        }

        /// <summary>
        /// On Dynamic Time: The time between the minute bumps.
        /// </summary>
        public static int Scale => API.GetConvarInt("simplesync_scale", 2000);
    }
}
