using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using Microsoft.Extensions.Logging;

namespace ConductorSharp.Definitions.Middlewares;

internal class CustomMiddleware<TRequest, TResponse> : IWorkerMiddleware<TRequest, TResponse>
    where TRequest : ITaskInput<TResponse>, new()
{
    private readonly ILogger<CustomMiddleware<TRequest, TResponse>> _logger;

    public CustomMiddleware(ILogger<CustomMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        WorkerExecutionContext context,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Executed before all middlewares");
        var response = await next();
        _logger.LogInformation("Executed after all middlewares");
        return response;
    }
}
