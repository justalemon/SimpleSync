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
        #region Fields

        private static readonly string game = API.GetGameName();
        
        #endregion
        
        #region Constructor

        public Weather()
        {
            // Once this script is loaded, ask the sever for sync
            TriggerServerEvent("simplesync:requestWeather");
            if (Convars.Debug)
            {
                Debug.WriteLine("Weather Synchronization has started");
            }
        }

        #endregion
        
        #region Tools

        private static void SetWeather(string weather, float time)
        {
            if (game == "fivem")
            {
                Function.Call(Hash.SET_WEATHER_TYPE_OVERTIME_PERSIST, weather, time);
            }
            else if (game == "redm")
            {
                // SET_WEATHER_TYPE
                Function.Call((Hash)0x59174F1AFE095B5A, Game.GenerateHash(weather), true, true, true, time, true);
            }
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
            if (Convars.Debug)
            {
                Debug.WriteLine($"Started weather switch from {from} to {to} ({duration}s)");
            }

            // Clear any overrides set
            API.ClearOverrideWeather();
            API.ClearWeatherTypePersist();

            // Force the origin weather
            SetWeather(from, 0);

            // If both weathers are not the same and the duration is not zero
            if (from != to && duration != 0)
            {
                // Clear any overrides set
                API.ClearOverrideWeather();
                API.ClearWeatherTypePersist();
                // Set the destination weather
                SetWeather(to, duration / 1000f);
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
                // And use the Snow particle effects
                API.RequestNamedPtfxAsset("core_snow");
                while (!API.HasNamedPtfxAssetLoaded("core_snow"))
                {
                    await Delay(0);
                }
                API.UseParticleFxAssetNextCall("core_snow");
            }
            // If the weather is set to something else
            else
            {
                // Unload the Ice and Snow foostep sounds
                API.ReleaseNamedScriptAudioBank("ICE_FOOTSTEPS");
                API.ReleaseNamedScriptAudioBank("SNOW_FOOTSTEPS");
                // Remove the particle effects for Snow (if is loaded)
                if (API.HasNamedPtfxAssetLoaded("core_snow"))
                {
                    API.RemoveNamedPtfxAsset("core_snow");
                }
            }
        }

        #endregion
    }
}
