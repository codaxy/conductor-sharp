using System.Net.Http;
using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service
{
    public class HealthService(IHttpClientFactory httpClientFactory, string clientName) : IHealthService
    {
        private readonly ConductorClient _client = new(httpClientFactory.CreateClient(clientName));

        public async Task<HealthCheckStatus> CheckHealthAsync(CancellationToken cancellationToken = default) =>
            await _client.DoCheckAsync(cancellationToken);
    }
}
