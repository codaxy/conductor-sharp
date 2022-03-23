using Microsoft.Extensions.Options;
using RestSharp;
using System.Net.Http;
using System.Threading.Tasks;
using XgsPon.Workflows.Client.Interface;
using XgsPon.Workflows.Client.Model.Response;
using XgsPon.Workflows.Client.Util;

namespace XgsPon.Workflows.Client.Service
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
