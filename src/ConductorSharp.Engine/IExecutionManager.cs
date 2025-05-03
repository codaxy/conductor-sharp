using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine;

internal interface IExecutionManager
{
    public Task StartAsync(CancellationToken cancellationToken);
}
