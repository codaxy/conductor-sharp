using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Engine.Util;

namespace ConductorSharp.Engine.Interface;

public interface IWorker<TRequest, TResponse>
    where TRequest : ITaskInput<TResponse>, new()
{
    Task<TResponse> Handle(TRequest test, WorkerExecutionContext context, CancellationToken cancellationToken);
}
