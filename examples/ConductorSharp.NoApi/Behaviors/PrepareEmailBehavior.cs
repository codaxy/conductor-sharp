using ConductorSharp.Engine.Interface;
using ConductorSharp.NoApi.Handlers;
using Microsoft.Extensions.Logging;

namespace ConductorSharp.NoApi.Behaviors
{
    internal class PrepareEmailBehavior : INgWorkerMiddleware<PrepareEmailRequest, PrepareEmailResponse>
    {
        private readonly ILogger<PrepareEmailBehavior> _logger;

        public PrepareEmailBehavior(ILogger<PrepareEmailBehavior> logger)
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
}
