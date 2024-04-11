using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service
{
    public class HealthService(ConductorClient client) : IHealthService
    {
        private readonly ConductorClient _conductorClient = client;

        public async Task<HealthCheckStatus> CheckHealthAsync(CancellationToken cancellationToken = default) =>
            await _conductorClient.DoCheckAsync(cancellationToken);
    }
}
