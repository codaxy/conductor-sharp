using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service
{
    public interface IHealthService
    {
        Task<HealthCheckStatus> CheckHealthAsync(CancellationToken cancellationToken = default);
    }
}
