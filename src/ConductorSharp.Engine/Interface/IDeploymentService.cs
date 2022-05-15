using ConductorSharp.Engine.Model;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Interface
{

    public interface IDeploymentService
    {
        public Task Deploy(Deployment deployment);
        public Task Remove(Deployment deployment);
    }
}