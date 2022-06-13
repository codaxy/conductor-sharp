using ConductorSharp.Client.Model.Response;
using ConductorSharp.Client.Util;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConductorSharp.Client.Service
{

    public class HealthService : IHealthService
    {
        private readonly IConductorClient _conductorClient;

        public HealthService(IConductorClient client) => _conductorClient = client;

        public async Task<HealthResponse> CheckHealth() =>
            await _conductorClient.ExecuteRequestAsync<HealthResponse>(
                ApiUrls.Health(),
                HttpMethod.Get
            );
    }
}