using System.Threading.Tasks;
using XgsPon.Workflows.Engine.Model;

namespace XgsPon.Workflows.Engine.Interface
{
    public interface IDeploymentService
    {
        public Task Deploy(Deployment deployment);
        public Task Remove(Deployment deployment);
    }
}
