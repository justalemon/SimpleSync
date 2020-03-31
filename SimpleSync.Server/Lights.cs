using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;

namespace SimpleSync.Server
{
    /// <summary>
    /// Script for the synchronization of Light Sources (previously called Blackout).
    /// </summary>
    public class Lights : BaseScript
    {
        #region Properties

        /// <summary>
        /// The current activation status for the Light Sources.
        /// </summary>
        public static bool Enabled
        {
            get => API.GetConvarInt("simplesync_lights", 1) != 0;
            set => API.SetConvar("simplesync_lights", Convert.ToInt32(value).ToString());
        }

        #endregion

        #region Constructor 

        public Lights()
        {
            Exports.Add("areLightsEnabled", new Func<bool>(() => Enabled));
            Exports.Add("setLights", new Action<bool>((e) => Enabled = e));
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to get and set the status of the lights.
        /// </summary>
        [Command("lights")]
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

                // Otherwise, set it and notify about it
                Enabled = enabled;
                Debug.WriteLine("The Artificial Lights have been " + (enabled ? "Enabled" : "Disabled"));
            }

        }

        #endregion
    }
}
