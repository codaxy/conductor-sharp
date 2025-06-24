using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;

namespace ConductorSharp.Engine;

public abstract class NgWorker<TRequest, TResponse> : SimpleTaskModel<TRequest, TResponse>, INgWorker<TRequest, TResponse>
    where TRequest : class, ITaskInput<TResponse>, new()
{
    public abstract Task<TResponse> Handle(TRequest test, CancellationToken cancellationToken);
}
