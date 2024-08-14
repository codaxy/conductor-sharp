using ConductorSharp.Client.Generated;
using EventHandler = ConductorSharp.Client.Generated.EventHandler;

namespace ConductorSharp.Client.Service
{
    public class EventService(IHttpClientFactory httpClientFactory, string clientName) : IEventService
    {
        private readonly ConductorClient _client = new(httpClientFactory.CreateClient(clientName));

        public async Task<ICollection<EventHandler>> ListAsync(CancellationToken cancellationToken = default) =>
            await _client.GetEventHandlersAsync(cancellationToken);

        public async Task UpdateAsync(EventHandler eventHandler, CancellationToken cancellationToken = default) =>
            await _client.UpdateEventHandlerAsync(eventHandler, cancellationToken);

        public async Task AddAsync(EventHandler eventHandler, CancellationToken cancellationToken = default) =>
            await _client.AddEventHandlerAsync(eventHandler, cancellationToken);

        public async Task<ICollection<EventHandler>> ListForEventAsync(
            string @event,
            bool? activeOnly = null,
            CancellationToken cancellationToken = default
        ) => await _client.GetEventHandlersForEventAsync(@event, activeOnly, cancellationToken);

        public async Task RemoveEventHandlerStatusAsync(string name, CancellationToken cancellationToken = default) =>
            await _client.RemoveEventHandlerStatusAsync(name, cancellationToken);
    }
}
