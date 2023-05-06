using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;

namespace SimpleSync.Server
{
    /// <summary>
    /// Functions that are used by both the Time and Weather scripts.
    /// </summary>
    public class Common : BaseScript
    {
        public void ModeCommand(List<object> args, string convar)
        {
            // If there is more than one argument
            if (args.Count >= 1)
            {
                // Try to parse the first argument and save it
                if (int.TryParse(args[0].ToString(), out int output))
                {
                    if (!SetSyncMode(output, convar))
                    {
                        Debug.WriteLine($"{output} is not a valid synchronization mode!");
                        return;
                    }
                }
                // If is not, tell the user
                else
                {
                    Debug.WriteLine("The value specified is not a valid number.");
                    return;
                }
            }

            // Get the sync mode and show it
            SyncMode mode = (SyncMode)API.GetConvarInt(convar, convar == "simplesync_modelights" ? 1 : 0);
            // Say the current synchronization mode
            Debug.WriteLine($"The synchronization mode is set to {mode}");
        }
    }
}
