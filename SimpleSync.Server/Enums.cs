namespace SimpleSync.Server
{
    /// <summary>
    /// The Synchronization Mode for Weather and Time.
    /// </summary>
    public enum SyncMode
    {
        /// <summary>
        /// Changes are made dynamically.
        /// </summary>
        Dynamic = 0,
        /// <summary>
        /// Sets a specific value permanently.
        /// </summary>
        Static = 1,
        /// <summary>
        /// Synchronization is done with IRL Information.
        /// </summary>
        Real = 2,
    }
}
