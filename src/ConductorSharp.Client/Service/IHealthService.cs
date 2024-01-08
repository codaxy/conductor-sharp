using ConductorSharp.Client.Generated;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Client.Service
{
    public interface IHealthService
    {
        Task<HealthCheckStatus> CheckHealthAsync(CancellationToken cancellationToken = default);
    }
}
