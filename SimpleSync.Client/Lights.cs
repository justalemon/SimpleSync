using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace SimpleSync.Client
{
    /// <summary>
    /// Script for the synchronization of Light Sources (previously called Blackout).
    /// </summary>
    public class Lights : BaseScript
    {
        #region Constructor

        public Lights()
        {
            TriggerServerEvent("simplesync:requestLights");
            if (Convars.Debug)
            {
                Debug.WriteLine("Lights/Blackout Synchronization has started");
            }
        }

        #endregion

        #region Network Events

        [EventHandler("simplesync:setLights")]
        public void SetLights(bool enabled)
        {
            // Over here, True is Disabled and False is Enabled
            // Thanks Rockstar Games!
            API.SetArtificialLightsState(!enabled);
        }

        #endregion
    }
}
