namespace SimpleSync.Server
{
    /// <summary>
    /// The type of Synchronization for Weather and Time.
    /// </summary>
    public enum SyncType
    {
        /// <summary>
        /// Changes the Synchronization dynamically.
        /// </summary>
        Dynamic = 0,
        /// <summary>
        /// Sets a specific value permanently.
        /// </summary>
        Static = 1,
        /// <summary>
        /// Does a synchronization with a Real city.
        /// </summary>
        Real = 2,
    }
}
