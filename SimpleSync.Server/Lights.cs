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
