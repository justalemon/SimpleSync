using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;

namespace SimpleSync.Client
{
    /// <summary>
    /// CFX Script that applies the weather changes.
    /// </summary>
    public class Weather : BaseScript
    {
        #region Constructor

        public Weather()
        {
            // Once this script is loaded, ask the sever for sync
            TriggerServerEvent("simplesync:requestWeather");
            Logging.Log("Weather Synchronization has started");
        }

        #endregion

        #region Network Events

        /// <summary>
        /// Sets the Weather on this client.
        /// </summary>
        [EventHandler("simplesync:setWeather")]
        public async void SetWeather(string from, string to, int duration)
        {
            // Log what we are going to do
            Logging.Log($"Started weather switch from {from} to {to} ({duration}s)");

            // Clear any overrides set
            API.ClearOverrideWeather();
            API.ClearWeatherTypePersist();

            // Force the origin weather
            API.SetWeatherTypeOvertimePersist(from, 0);

            // If both weathers are not the same and the duration is not zero
            if (from != to && duration != 0)
            {
                // Clear any overrides set
                API.ClearOverrideWeather();
                API.ClearWeatherTypePersist();
                // Convert the MS to S
                float ms = duration == 0 ? 0 : duration / 1000f;
                // Set the destination weather
                API.SetWeatherTypeOvertimePersist(to, ms);
                // And wait
                await Delay(duration);
            }

            // Set the correct snow marks based on the weather
            API.SetForceVehicleTrails(to == "XMAS");
            API.SetForcePedFootstepsTracks(to == "XMAS");
            // Set the correct water height
            API.N_0xc54a08c85ae4d410(to == "XMAS" ? 3 : 0);

            // If the weather is set to XMAS
            if (to == "XMAS")
            {
                // Use the Ice and Snow footstep sounds
                API.RequestScriptAudioBank("ICE_FOOTSTEPS", false);
                API.RequestScriptAudioBank("SNOW_FOOTSTEPS", false);
            }
            // If the weather is set to something else
            else
            {
                // Unload the Ice and Snow foostep sounds
                API.ReleaseNamedScriptAudioBank("ICE_FOOTSTEPS");
                API.ReleaseNamedScriptAudioBank("SNOW_FOOTSTEPS");
            }
        }

        #endregion
    }
}
