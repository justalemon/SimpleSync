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
        /// Command to Get and Set the sync mode.
        /// </summary>
        [Command("lightsmode", Restricted = true)]
        public void LightsModeCommand(int source, List<object> args, string raw) => ModeCommand(args, "simplesync_modelights");

        #endregion
    }
}
