using CitizenFX.Core;
using CitizenFX.Core.Native;
using SimpleSync.Shared;
using System;
using System.Threading.Tasks;

namespace SimpleSync.Client
{
    /// <summary>
    /// CFX Script that applies the time on the Client.
    /// </summary>
    public class Time : BaseScript
    {
        #region Fields

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
            // Tell the Server to send back the correct time
            TriggerServerEvent("simplesync:requestTime");
            Logging.Log("Time Synchronization has started");
        }

        #endregion

        #region Tick

        [Tick]
        public async Task UpdateTime()
        {
            // Just set the override time
            API.NetworkOverrideClockTime(hours, minutes, 0);
        }

        #endregion

        #region Network Events

        [EventHandler("simplesync:setTime")]
        public void SetTime(int hour, int minute)
        {
            // Just save the values to be applied during the next tick
            hours = hour;
            minutes = minute;
            Logging.Log($"Time set to {hour:D2}:{minute:D2}");
        }

        #endregion
    }
}
