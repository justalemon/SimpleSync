using System.Collections.Generic;

namespace SimpleSync.Server
{
    /// <summary>
    /// Some converters for stuff not provided by C#.
    /// </summary>
    public static class Converters
    {
        /// <summary>
        /// String representations of Yes.
        /// </summary>
        private static readonly List<string> yes = new List<string>()
        {
            "true",
            "yes",
            "enabled",
            "enable",
            "y",
            "1"
        };
        /// <summary>
        /// String representations of No.
        /// </summary>
        private static readonly List<string> no = new List<string>()
        {
            "false",
            "no",
            "disabled",
            "disable",
            "n",
            "0"
        };

        public static bool TryBoolean(string str, out bool def)
        {
            // Convert the string to lowercase
            str = str.ToLowerInvariant();

            // If is on the list for yes, return true
            if (yes.Contains(str))
            {
                def = true;
                return true;
            }
            // If is on the list for no, return false
            else if (no.Contains(str))
            {
                def = false;
                return true;
            }
            // Otherwise, return the default value
            else
            {
                def = false;
                return false;
            }
        }
    }
}
