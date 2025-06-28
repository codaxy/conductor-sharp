using ConductorSharp.Engine.Interface;
using Microsoft.Extensions.Logging;

namespace ConductorSharp.Definitions.Middlewares;

internal class CustomMiddleware<TRequest, TResponse> : INgWorkerMiddleware<TRequest, TResponse>
    where TRequest : class, ITaskInput<TResponse>, new()
{
    private readonly ILogger<CustomMiddleware<TRequest, TResponse>> _logger;

    public CustomMiddleware(ILogger<CustomMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        Func<TRequest, CancellationToken, Task<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Executed before all middlewares");
        var response = await next(request, cancellationToken);
        _logger.LogInformation("Executed after all middlewares");
        return response;
    }
}
