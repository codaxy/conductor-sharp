using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;

namespace ConductorSharp.Engine;

public abstract class NgWorker<TRequest, TResponse> : SimpleTaskModel<TRequest, TResponse>, INgWorker<TRequest, TResponse>
    where TRequest : class, ITaskInput<TResponse>, new()
{
    public abstract Task<TResponse> Handle(TRequest test, WorkerExecutionContext context, CancellationToken cancellationToken);
}
