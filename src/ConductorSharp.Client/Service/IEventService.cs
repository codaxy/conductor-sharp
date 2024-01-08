using ConductorSharp.Client.Generated;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Client.Service
{
    public interface IEventService
    {
        /// <summary>
        /// Add a new event handler.
        /// </summary>
        System.Threading.Tasks.Task AddAsync(EventHandler eventHandler, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get event handlers for a given event
        /// </summary>
        System.Threading.Tasks.Task GetEventHandlersForEventAsync(
            string @event,
            bool? activeOnly = null,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Get all the event handlers
        /// </summary>
        Task<ICollection<EventHandler>> ListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove an event handler
        /// </summary>
        System.Threading.Tasks.Task RemoveEventHandlerStatusAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update an existing event handler.
        /// </summary>
        System.Threading.Tasks.Task UpdateAsync(EventHandler eventHandler, CancellationToken cancellationToken = default);
    }
}
