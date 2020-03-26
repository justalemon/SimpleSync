using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace SimpleSync.Shared
{
    public static class Logging
    {
        /// <summary>
        /// If the debug logging is enabled.
        /// </summary>
        private static bool IsDebugEnabled => API.GetConvarInt("simplesync_debug", 0) != 0;

        /// <summary>
        /// Logs a message if debug mode is enabled.
        /// </summary>
        /// <param name="message">The message that we need to log.</param>
        public static void Log(string message)
        {
            // If Debug mode is enabled
            if (IsDebugEnabled)
            {
                // Log the message in the console
                Debug.WriteLine(message);
            }
        }
    }
}
