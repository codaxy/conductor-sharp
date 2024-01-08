using ConductorSharp.Client.Generated;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Client.Service
{
    public class HealthService(ConductorClient client) : IHealthService
    {
        private readonly ConductorClient _conductorClient = client;

        public async Task<HealthCheckStatus> CheckHealthAsync(CancellationToken cancellationToken) =>
            await _conductorClient.DoCheckAsync(cancellationToken);
    }
}
