using ConductorSharp.Client.Generated;
using EventHandler = ConductorSharp.Client.Generated.EventHandler;

namespace ConductorSharp.Client.Service
{
    public interface IEventService
    {
        /// <summary>
        /// Add a new event handler.
        /// </summary>
        Task AddAsync(EventHandler eventHandler, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get event handlers for a given event
        /// </summary>
        Task GetEventHandlersForEventAsync(string @event, bool? activeOnly = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all the event handlers
        /// </summary>
        Task<ICollection<EventHandler>> ListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove an event handler
        /// </summary>
        Task RemoveEventHandlerStatusAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update an existing event handler.
        /// </summary>
        Task UpdateAsync(EventHandler eventHandler, CancellationToken cancellationToken = default);
    }
}
