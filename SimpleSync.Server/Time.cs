using CitizenFX.Core;
using CitizenFX.Core.Native;
using SimpleSync.Shared;
using System;
using System.Collections.Generic;
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
            Exports.Add("setTimeZone", new Action<string>(SetTimeZone));
            // And log a couple of messages
            Logging.Log("Time Synchronization has started");
            Logging.Log($"Sync Type is set to {Convars.TimeType}");
            Logging.Log($"Scale is set to {Convars.Scale}");
            Logging.Log($"Time Zone is set to {Convars.TimeZone}");
        }

        #endregion

        #region Exports

        public void SetTime(int hour, int minute)
        {
            // Just save the values
            hours = hour;
            minutes = minute;
            Logging.Log($"Time set to {hour:D2}:{minute:D2} via exports");
        }

        public void SetTimeZone(string tz)
        {
            // Try to get the timezone with the specified name
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(tz);
            }
            // If the timezone was not found, just return
            catch (TimeZoneNotFoundException)
            {
                return;
            }

            // If we got here, the Time Zone is valid so save it
            Convars.TimeZone = tz;
            Logging.Log($"Time Zone set to {tz} via exports");
        }

        #endregion

        #region Network Events

        /// <summary>
        /// Sends the correct time back to the Client.
        /// </summary>
        [EventHandler("simplesync:requestTime")]
        public void RequestTime([FromSource]Player player)
        {
            // Just send the up to date time
            player.TriggerEvent("simplesync:setTime", hours, minutes);
            Logging.Log($"Client {player.Handle} ({player.Name}) requested the Time");
        }

        #endregion

        #region Ticks

        /// <summary>
        /// Updates the Hours and Minutes over time.
        /// </summary>
        [Tick]
        public async Task UpdateTime()
        {
            // If the time is set to dynamic
            if (Convars.TimeType == SyncType.Dynamic)
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
                    nextFetch = API.GetGameTimer() + Convars.Scale;
                    // And send the updated time to the clients
                    TriggerClientEvent("simplesync:setTime", hours, minutes);
                }
            }
            // If the time is set to static, the client already has the previous time
            else if (Convars.TimeType == SyncType.Static)
            {
                return;
            }
            // If the time is set to real
            else if (Convars.TimeType == SyncType.Real)
            {
                // If the game time is over or equal than the next fetch time
                if (API.GetGameTimer() >= nextFetch)
                {
                    // Create a place to store the time zone
                    DateTime dateTime;
                    // Try to convert the time to the specified timezone
                    try
                    {
                        dateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, Convars.TimeZone);
                    }
                    // If the timezone was not found, set PST and return
                    catch (TimeZoneNotFoundException)
                    {
                        Debug.WriteLine($"The Time Zone '{Convars.TimeZone}' was not found!");
                        Debug.WriteLine($"Use the command /timezones to see the available TZs");
                        Debug.WriteLine($"Just in case, we changed the TZ to 'Pacific Standard Time'");
                        Convars.TimeZone = "Pacific Standard Time";
                        return;
                    }

                    // If no errors happened, save the hours and minutes
                    hours = dateTime.Hour;
                    minutes = dateTime.Minute;
                    // Set the next fetch time to one second in the future
                    nextFetch = API.GetGameTimer() + 1000;
                    // And send it to all of the clients
                    TriggerClientEvent("simplesync:setTime", hours, minutes);
                }
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command that shows the available Time Zones for the Real Time.
        /// </summary>
        [Command("timezones", Restricted = true)]
        public void TimeZonesCommand(int source, List<object> args, string raw)
        {
            // Say that we are going to print the time zones
            Debug.WriteLine("Time Zones available:");
            // Iterate over the list of time zones
            foreach (TimeZoneInfo tz in TimeZoneInfo.GetSystemTimeZones())
            {
                // And print it on the console
                Debug.WriteLine($"{tz.Id} - {tz.DisplayName}");
            }
        }
        /// <summary>
        /// Gets or Sets the current time zone.
        /// </summary>
        [Command("timezone", Restricted = true)]
        public void TimeZoneCommand(int source, List<object> args, string raw)
        {
            // If there are no arguments specified, show the current time zone and return
            if (args.Count == 0)
            {
                Debug.WriteLine($"The current Time Zone is set to {Convars.TimeZone}");
                return;
            }

            // Get the timezone as a string
            string timeZone = args[0].ToString();
            // Try to get the timezone with the specified name
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            }
            // If the timezone was not found, notify it and return
            catch (TimeZoneNotFoundException)
            {
                Debug.WriteLine($"The Time Zone {timeZone} does not exists!");
                Debug.WriteLine("Remember that the Time Zone IDs are case sensitive");
                Debug.WriteLine("Use the /timezones command to show all of the TZs");
                return;
            }

            // If we got here, the Time Zone is valid so save it
            Convars.TimeZone = timeZone;
            // And notify it
            Debug.WriteLine($"The Time Zone was set to {timeZone}!");
        }

        #endregion
    }
}
