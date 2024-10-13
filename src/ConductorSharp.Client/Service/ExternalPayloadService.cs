using System.Net.Http;
using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service
{
    public class ExternalPayloadService(HttpClient client) : IExternalPayloadService
    {
        private readonly ConductorClient _client = new(client);

        public async Task<FileResponse> GetExternalStorageDataAsync(string externalPayloadPath, CancellationToken cancellationToken = default) =>
            await _client.GetExternalStorageDataAsync(externalPayloadPath, cancellationToken);
    }
}
