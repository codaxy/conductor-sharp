using System;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;

namespace ConductorSharp.Engine.Middlewares;

public class ValidationWorkerMiddleware<TRequest, TResponse> : IWorkerMiddleware<TRequest, TResponse>
    where TRequest : class, ITaskInput<TResponse>, new()
{
    public async Task<TResponse> Handle(
        TRequest request,
        WorkerExecutionContext context,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        ObjectValidator.Validate(request);
        var response = await next();
        return response;
    }
}
