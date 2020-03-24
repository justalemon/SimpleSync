using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Threading.Tasks;

namespace SimpleSync.Server
{
    /// <summary>
    /// CFX Script that handles the Server Side Time Synchronization.
    /// </summary>
    public class Time : BaseScript
    {
        #region Fields

        /// <summary>
        /// The next time where we should increase the time.
        /// </summary>
        private long nextFetch = 0;
        /// <summary>
        /// The current hours.
        /// </summary>
        private int hours = 0;
        /// <summary>
        /// The current minutes.
        /// </summary>
        private int minutes = 0;

        #endregion

        #region Constructor

        public Time()
        {
        }

        #endregion

        #region Ticks

        [Tick]
        public async Task UpdateTime()
        {
            // If the game time is over or equal than the next fetch time
            if (API.GetGameTimer() >= nextFetch)
            {
                // If the current time is 23:59
                if (hours == 23 && minutes == 59)
                {
                    // Set 00:00 instead of 24:00
                    hours = 0;
                    minutes = 0;
                }
                // If the current time is Something:59
                else if (minutes == 59)
                {
                    // Increase the hours and set the minutes to 0
                    hours++;
                    minutes = 0;
                }
                // Otherwise
                else
                {
                    // Increase the minutes
                    minutes++;
                }

                // Finally, set the next fetch time to one second in the future
                nextFetch = API.GetGameTimer() + API.GetConvarInt("simplesync_scale", 2000);
            }
        }

        #endregion
    }
}
