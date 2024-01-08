using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service
{
    public class ExternalPayloadService(ConductorClient client) : IExternalPayloadService
    {
        public async Task<FileResponse> GetExternalStorageData(string externalPayloadPath,
            CancellationToken cancellationToken = default)
            => await client.GetExternalStorageDataAsync(externalPayloadPath, cancellationToken);
    }
}
