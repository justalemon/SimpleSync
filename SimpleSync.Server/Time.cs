using CitizenFX.Core;
using CitizenFX.Core.Native;
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
        /// The default Time Zone for the current environment.
        /// </summary>
        private readonly string defaultTimeZone = GetDefaultTimezone();

        #endregion

        #region Constructor

        public Time()
        {
            // Add a couple of exports to set the time
            Exports.Add("setTimeZone", new Func<string, bool>(SetTimeZone));
            Exports.Add("getTimeZone", new Func<string>(() => Convars.TimeZone));

            Exports.Add("getNextTimeFetch", new Func<long>(() => nextFetch));
            // And log a couple of messages
            if (Convars.Debug)
            {
                Debug.WriteLine("Time Synchronization has started");
                Debug.WriteLine($"Sync Mode is set to {Convars.TimeMode}");
                Debug.WriteLine($"Scale is set to {Convars.Scale}");
                Debug.WriteLine($"Time Zone is set to {Convars.TimeZone}");
            }
        }

        #endregion

        #region Tools

        public static string GetDefaultTimezone()
        {
            // Check the operating system and return the correct timezone
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return "Pacific Standard Time";
                case PlatformID.Unix:
                    return "America/Los_Angeles";
                default:
                    throw new NotSupportedException($"FiveM reports that we are running on {Environment.OSVersion.Platform}");
            }
        }

        #endregion

        #region Exports

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
            if (Convars.Debug)
            {
                Debug.WriteLine($"Time Zone set to {tz} via SetTimeZone");
            }
            return true;
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
                            Debug.WriteLine($"Just in case, we changed the TZ to '{defaultTimeZone}'");
                            Convars.TimeZone = defaultTimeZone;
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
        /// Command to Get and Set the sync mode.
        /// </summary>
        [Command("timemode", Restricted = true)]
        public void TimeModeCommand(int source, List<object> args, string raw) => ModeCommand(args, "simplesync_modetime");

        #endregion
    }
}
