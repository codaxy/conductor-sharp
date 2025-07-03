using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using ConductorSharp.NoApi.Handlers;
using Microsoft.Extensions.Logging;

namespace ConductorSharp.NoApi.Middlewares;

internal class PrepareEmailMiddleware : IWorkerMiddleware<PrepareEmailRequest, PrepareEmailResponse>
{
    private readonly ILogger<PrepareEmailMiddleware> _logger;

    public PrepareEmailMiddleware(ILogger<PrepareEmailMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task<PrepareEmailResponse> Handle(
        PrepareEmailRequest request,
        WorkerExecutionContext context,
        Func<Task<PrepareEmailResponse>> next,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation($"Executed only before {nameof(PrepareEmailHandler)}");
        var response = await next();
        _logger.LogInformation($"Executed only after {nameof(PrepareEmailHandler)}");
        return response;
    }
}
