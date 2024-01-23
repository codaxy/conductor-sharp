using ConductorSharp.Client.Generated;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Client.Service
{
    public interface IExternalPayloadService
    {
        /// <summary>
        /// Get task or workflow by externalPayloadPath from External PostgreSQL Storage
        /// </summary>
        Task<FileResponse> GetExternalStorageDataAsync(string externalPayloadPath, CancellationToken cancellationToken = default);
    }
}
