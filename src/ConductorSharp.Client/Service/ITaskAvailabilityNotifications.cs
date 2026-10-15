namespace ConductorSharp.Client.Service
{
    /// <summary>
    /// Tells workers when tasks of a type may be waiting, so they poll on notice instead of on a timer.
    /// </summary>
    /// <remarks>
    /// A transport implements this when the server can push such notices. Maestro does over NATS.
    /// Conductor's REST API has nothing like it, so the HTTP clients do not implement it.
    /// </remarks>
    public interface ITaskAvailabilityNotifications
    {
        /// <summary>
        /// Starts listening for notices about <paramref name="taskTypes"/>, and calls
        /// <paramref name="onAvailable"/> with the task type of each one. Disposing the result stops
        /// listening.
        /// </summary>
        /// <remarks>
        /// Returns once the subscription is in place, so a notice sent after that reaches the callback.
        /// The callback runs on the transport's own thread and has to return at once. A transport that
        /// may have missed notices, as one that just reconnected, calls it for every type.
        /// </remarks>
        Task<IAsyncDisposable> SubscribeAsync(
            IReadOnlyCollection<string> taskTypes,
            Action<string> onAvailable,
            CancellationToken cancellationToken = default
        );
    }
}
