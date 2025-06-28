using ConductorSharp.Engine.Interface;
using ConductorSharp.NoApi.Handlers;
using Microsoft.Extensions.Logging;

namespace ConductorSharp.NoApi.MIddlewares;

internal class PrepareEmailMiddleware : INgWorkerMiddleware<PrepareEmailRequest, PrepareEmailResponse>
{
    private readonly ILogger<PrepareEmailMiddleware> _logger;

    public PrepareEmailMiddleware(ILogger<PrepareEmailMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task<PrepareEmailResponse> Handle(
        PrepareEmailRequest request,
        Func<PrepareEmailRequest, CancellationToken, Task<PrepareEmailResponse>> next,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation($"Executed only before {nameof(PrepareEmailHandler)}");
        var response = await next(request, cancellationToken);
        _logger.LogInformation($"Executed only after {nameof(PrepareEmailHandler)}");
        return response;
    }
}
