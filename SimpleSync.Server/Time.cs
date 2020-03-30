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
    public class Time : Common
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
            // Add a couple of exports to set the time
            Exports.Add("getTimeSyncMode", new Func<int>(() => API.GetConvarInt("simplesync_modetime", 0)));
            Exports.Add("setTimeSyncMode", new Func<int, bool>((i) => SetSyncMode(i, "simplesync_modetime")));

            Exports.Add("setTime", new Action<int, int>(SetTime));
            Exports.Add("getHours", new Func<int>(() => hours));
            Exports.Add("getMinutes", new Func<int>(() => minutes));

            Exports.Add("setTimeZone", new Func<string, bool>(SetTimeZone));
            Exports.Add("getTimeZone", new Func<string>(() => Convars.TimeZone));

            Exports.Add("getNextTimeFetch", new Func<long>(() => nextFetch));
            // And log a couple of messages
            Logging.Log("Time Synchronization has started");
            Logging.Log($"Sync Mode is set to {Convars.TimeMode}");
            Logging.Log($"Scale is set to {Convars.Scale}");
            Logging.Log($"Time Zone is set to {Convars.TimeZone}");
        }

        #endregion

        #region Exports

        public void SetTime(int hour, int minute)
        {
            // Feed it into a timespan
            TimeSpan parsed = TimeSpan.FromMinutes((hour * 60) + minute);
            // Save the individual values
            hours = parsed.Hours;
            minutes = parsed.Minutes;
            // And send the updated time to the clients
            TriggerClientEvent("simplesync:setTime", hours, minutes);
            Logging.Log($"Time set to {hours:D2}:{minutes:D2} via SetTime");
        }

        public bool SetTimeZone(string tz)
        {
            // Try to get the timezone with the specified name
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(tz);
            }
            // If the timezone was not found, just return
            catch (TimeZoneNotFoundException)
            {
                return false;
            }

            // If we got here, the Time Zone is valid so save it
            Convars.TimeZone = tz;
            Logging.Log($"Time Zone set to {tz} via exports");
            return true;
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
            // Check the Time Sync Mode
            switch (Convars.TimeMode)
            {
                // If is set to dynamic
                case SyncMode.Dynamic:
                    // If the game time is over or equal than the next fetch time
                    if (API.GetGameTimer() >= nextFetch)
                    {
                        // Calculate the total number of minutes plus the increase
                        int total = (hours * 60) + minutes + Convars.Increase;
                        // Tell the system to set this specific number of minutes
                        SetTime(0, total);
                        // Set the next fetch time to the specified scale
                        nextFetch = API.GetGameTimer() + Convars.Scale;
                        Logging.Log($"Time bump complete!");
                    }
                    return;
                // If is set to static, just return
                // The client already has the correct time
                case SyncMode.Static:
                    return;
                // If the time is set to real
                case SyncMode.Real:

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

                        // If no errors happened, set the correct time
                        SetTime(dateTime.Hour, dateTime.Minute);
                        // And set the next fetch time to one second in the future
                        nextFetch = API.GetGameTimer() + 1000;
                    }
                    return;
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to Get and Set the time.
        /// </summary>
        [Command("time", Restricted = true)]
        public void TimeCommand(int source, List<object> args, string raw)
        {
            switch (Convars.TimeMode)
            {
                // If the sync mode is set to Real, show the IRL Time
                case SyncMode.Real:
                    DateTime tz = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, Convars.TimeZone);
                    Debug.WriteLine($"Time Zone is set to {Convars.TimeZone}");
                    Debug.WriteLine($"The current time is {tz.Hour:D2}:{tz.Minute:D2}");
                    return;
                // For Dynamic and Static
                case SyncMode.Dynamic:
                case SyncMode.Static:
                    // If we have zero arguments, show the time and return
                    if (args.Count == 0)
                    {
                        Debug.WriteLine($"The time is set to {hours:D2}:{minutes:D2}");
                        return;
                    }

                    // If there is single argument and is separated by :
                    if (args.Count == 1)
                    {
                        // Convart it to a string
                        string repr = args[0].ToString();

                        // If it contains two dots
                        if (repr.Contains(":"))
                        {
                            // Convert the items and add them back
                            string[] newArgs = repr.Split(':');
                            args.Clear();
                            args.AddRange(newArgs);
                        }
                        // If it does not, add a zero
                        else
                        {
                            args.Add(0);
                        }
                    }

                    // Now, time to parse them
                    if (!int.TryParse(args[0].ToString(), out int newHours) || !int.TryParse(args[1].ToString(), out int newMinutes))
                    {
                        Debug.WriteLine("One of the parameters are not valid numbers.");
                        return;
                    }

                    // If we got here, the numbers are valid
                    SetTime(newHours, newMinutes);
                    break;
            }
        }
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
            string tz = args[0].ToString();
            // And try to set it and send a confirmation message
            if (SetTimeZone(tz))
            {
                Debug.WriteLine($"The Time Zone was set to {tz}!");
            }
            else
            {
                Debug.WriteLine($"The Time Zone {tz} does not exists!");
                Debug.WriteLine("Remember that the Time Zone IDs are case sensitive");
                Debug.WriteLine("Use the /timezones command to show all of the TZs");
            }
        }
        /// <summary>
        /// Shows the current internal time of the game.
        /// </summary>
        [Command("gametimer", Restricted = true)]
        public void GameTimerCommand() => Debug.WriteLine($"Current Game Time is {API.GetGameTimer()}");
        /// <summary>
        /// Command to Get and Set the sync mode.
        /// </summary>
        [Command("timemode", Restricted = true)]
        public void TimeModeCommand(int source, List<object> args, string raw) => ModeCommand(args, "simplesync_modetime");

        #endregion
    }
}
