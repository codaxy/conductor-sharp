using System.Threading.Tasks;
using XgsPon.Workflows.Client.Model.Response;

namespace XgsPon.Workflows.Client.Interface
{
    public interface IHealthService
    {
        Task<HealthResponse> CheckHealth();
    }
}
