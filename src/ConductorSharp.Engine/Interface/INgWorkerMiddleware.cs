using System;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Engine.Util;

namespace ConductorSharp.Engine.Interface;

public interface INgWorkerMiddleware<TRequest, TResponse>
    where TRequest : class, ITaskInput<TResponse>, new()
{
    Task<TResponse> Handle(
        TRequest request,
        WorkerExecutionContext context,
        Func<TRequest, CancellationToken, Task<TResponse>> next,
        CancellationToken cancellationToken
    );
}
