using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;

namespace ConductorSharp.Engine;

public abstract class Worker<TRequest, TResponse> : SimpleTaskModel<TRequest, TResponse>, IWorker<TRequest, TResponse>
    where TRequest : ITaskInput<TResponse>, new()
{
    public abstract Task<TResponse> Handle(TRequest test, WorkerExecutionContext context, CancellationToken cancellationToken);
}
