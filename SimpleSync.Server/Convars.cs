using CitizenFX.Core.Native;

namespace SimpleSync.Server
{
    /// <summary>
    /// A Class for a quick access to Convars.
    /// </summary>
    public static class Convars
    {
        /// <summary>
        /// On Dynamic Time: The time between the minute bumps.
        /// </summary>
        public static int Scale => API.GetConvarInt("simplesync_scale", 2000);
    }
}
