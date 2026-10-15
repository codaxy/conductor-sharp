namespace ConductorSharp.Engine
{
    /// <summary>
    /// Settings of the execution manager that polls on task availability notices.
    /// </summary>
    internal class NotifiedPollingConfig
    {
        /// <summary>
        /// How often every task type is polled whether a notice came or not, in milliseconds. It
        /// picks up tasks whose notice the transport lost.
        /// </summary>
        public int FallbackPollInterval { get; set; }
    }
}
