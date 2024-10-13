using System.Net.Http;
using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service
{
    public class HealthService(HttpClient client) : IHealthService
    {
        private readonly ConductorClient _client = new(client);

        public async Task<HealthCheckStatus> CheckHealthAsync(CancellationToken cancellationToken = default) =>
            await _client.DoCheckAsync(cancellationToken);
    }
}
