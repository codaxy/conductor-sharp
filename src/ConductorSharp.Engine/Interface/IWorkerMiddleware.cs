using System;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Engine.Util;

namespace ConductorSharp.Engine.Interface;

public interface IWorkerMiddleware<TRequest, TResponse>
    where TRequest : ITaskInput<TResponse>, new()
{
    Task<TResponse> Handle(TRequest request, WorkerExecutionContext context, Func<Task<TResponse>> next, CancellationToken cancellationToken);
}
