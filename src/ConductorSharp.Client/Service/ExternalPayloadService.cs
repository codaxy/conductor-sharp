using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service
{
    public class ExternalPayloadService(ConductorClient client) : IExternalPayloadService
    {
        public async Task<FileResponse> GetExternalStorageDataAsync(string externalPayloadPath, CancellationToken cancellationToken = default) =>
            await client.GetExternalStorageDataAsync(externalPayloadPath, cancellationToken);
    }
}
