﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Client.Generated;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Client.Service
{
    public class EventService(ConductorClient client) : IEventService
    {
        public async Task<ICollection<EventHandler>> ListAsync(CancellationToken cancellationToken = default)
            => await client.GetEventHandlersAsync(cancellationToken);

        public async Task UpdateAsync(EventHandler eventHandler, CancellationToken cancellationToken = default)
            => await client.UpdateEventHandlerAsync(eventHandler, cancellationToken);

        public async Task AddAsync(EventHandler eventHandler, CancellationToken cancellationToken = default)
            => await client.AddEventHandlerAsync(eventHandler, cancellationToken);

        public async Task GetEventHandlersForEventAsync(string @event, bool? activeOnly = null,
            CancellationToken cancellationToken = default)
            => await client.GetEventHandlersForEventAsync(@event, activeOnly, cancellationToken);

        public async Task RemoveEventHandlerStatusAsync(string name, CancellationToken cancellationToken = default)
            => await client.RemoveEventHandlerStatusAsync(name, cancellationToken);

    }
}
