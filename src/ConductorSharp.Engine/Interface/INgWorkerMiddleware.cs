using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Interface;

public interface INgWorkerMiddleware<TRequest, TResponse>
    where TRequest : class, ITaskInput<TResponse>, new()
{
    Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken);
}
