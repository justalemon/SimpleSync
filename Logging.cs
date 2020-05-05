using CitizenFX.Core;

namespace SimpleSync.Shared
{
    public static class Logging
    {
        /// <summary>
        /// Logs a message if debug mode is enabled.
        /// </summary>
        /// <param name="message">The message that we need to log.</param>
        public static void Log(string message)
        {
            // If Debug mode is enabled
            if (Convars.Debug)
            {
                // Log the message in the console
                Debug.WriteLine(message);
            }
        }
    }
}
