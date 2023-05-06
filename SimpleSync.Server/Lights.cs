using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleSync.Server
{
    /// <summary>
    /// Script for the synchronization of Light Sources (previously called Blackout).
    /// </summary>
    public class Lights : Common
    {
        #region Ticks

        [Tick]
        public async Task UpdateLights()
        {
            switch (Convars.LightsMode)
            {
                // On dynamic mode
                case SyncMode.Dynamic:
                    // If the next fetch is zero, set the next blackout time and return
                    if (nextFetch == 0)
                    {
                        nextFetch = API.GetGameTimer() + random.Next(Convars.BlackoutSwitchMin, Convars.BlackoutSwitchMax);
                        if (Convars.Debug)
                        {
                            Debug.WriteLine($"Setting first blackout time to {nextFetch}");
                        }
                    }
                    // If the current time is over or equal than the next fetch time
                    else if (API.GetGameTimer() >= nextFetch)
                    {
                        // If the lights are enabled, disable them and set the next fetch time
                        if (Enabled)
                        {
                            Enabled = false;
                            TriggerClientEvent("simplesync:setLights", false);
                            nextFetch = API.GetGameTimer() + random.Next(Convars.BlackoutDurationMin, Convars.BlackoutDurationMax);
                            if (Convars.Debug)
                            {
                                Debug.WriteLine("Artificial lights are now turned OFF");
                                Debug.WriteLine($"Blackout will finish in {nextFetch}");
                            }
                        }
                        // If the lights are disabled, enable them and set the 
                        else
                        {
                            Enabled = true;
                            TriggerClientEvent("simplesync:setLights", true);
                            nextFetch = API.GetGameTimer() + random.Next(Convars.BlackoutDurationMin, Convars.BlackoutDurationMax);
                            if (Convars.Debug)
                            {
                                Debug.WriteLine("Artificial lights are now turned ON");
                                Debug.WriteLine($"Next blackout will be in {nextFetch}");
                            }
                        }
                    }
                    break;
                // For Static Mode, just return
                // There is no API for Blackouts, so just ignore it
                case SyncMode.Static:
                case SyncMode.Real:
                    return;
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to get and set the status of the lights.
        /// </summary>
        [Command("lights", Restricted = true)]
        public void LightsCommand(int source, List<object> args, string raw)
        {
            // If there are no arguments
            if (args.Count == 0)
            {
                // Show the activation of the lights
                Debug.WriteLine("The Artificial Lights are " + (Enabled ? "Enabled" : "Disabled"));
            }
            // Otherwise
            else
            {
                // Try to convert the first argument to a boolean
                // If we failed, notify and return
                if (!Converters.TryBoolean(args[0].ToString(), out bool enabled))
                {
                    Debug.WriteLine("The value specified is not valid!");
                    return;
                }

                // Otherwise, set it, send it and notify about it
                Enabled = enabled;
                TriggerClientEvent("simplesync:setLights", enabled);
                Debug.WriteLine("The Artificial Lights have been " + (enabled ? "Enabled" : "Disabled"));
            }
        }
        /// <summary>
        /// Command to Get and Set the sync mode.
        /// </summary>
        [Command("lightsmode", Restricted = true)]
        public void LightsModeCommand(int source, List<object> args, string raw) => ModeCommand(args, "simplesync_modelights");

        #endregion
    }
}
