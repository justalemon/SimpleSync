using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
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
            // Add a couple of exports to set the time
            Exports.Add("setTime", new Action<int, int>(SetTime));
        }

        #endregion

        #region Exports

        public void SetTime(int hour, int minute)
        {
            // Just save the values
            hours = hour;
            minutes = minute;
        }

        #endregion

        #region Ticks

        /// <summary>
        /// Updates the Hours and Minutes over time.
        /// </summary>
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
                // And send the updated time to the clients
                TriggerClientEvent("simplesync:setTime", hours, minutes);
            }
        }

        #endregion
    }
}
