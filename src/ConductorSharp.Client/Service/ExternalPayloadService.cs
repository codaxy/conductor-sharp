using System.Net.Http;
using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service
{
    public class ExternalPayloadService(IHttpClientFactory httpClientFactory, string clientName) : IExternalPayloadService
    {
        private readonly ConductorClient _client = new(httpClientFactory.CreateClient(clientName));

        public async Task<FileResponse> GetExternalStorageDataAsync(string externalPayloadPath, CancellationToken cancellationToken = default) =>
            await _client.GetExternalStorageDataAsync(externalPayloadPath, cancellationToken);
    }
}
