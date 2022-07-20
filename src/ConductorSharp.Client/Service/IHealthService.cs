using ConductorSharp.Client.Model.Response;
using System.Threading.Tasks;

namespace ConductorSharp.Client.Service
{
    public interface IHealthService
    {
        Task<HealthResponse> CheckHealth();
    }
}
