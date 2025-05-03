using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine;

public interface IExecutionManager
{
    public Task StartAsync(CancellationToken cancellationToken);
}