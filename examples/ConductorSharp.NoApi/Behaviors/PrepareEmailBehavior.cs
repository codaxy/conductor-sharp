using ConductorSharp.NoApi.Handlers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ConductorSharp.NoApi.Behaviors
{
    internal class PrepareEmailBehavior : IPipelineBehavior<PrepareEmailRequest, PrepareEmailResponse>
    {
        private readonly ILogger<PrepareEmailBehavior> _logger;

        public PrepareEmailBehavior(ILogger<PrepareEmailBehavior> logger)
        {
            _logger = logger;
        }

        public async Task<PrepareEmailResponse> Handle(
            PrepareEmailRequest request,
            RequestHandlerDelegate<PrepareEmailResponse> next,
            CancellationToken cancellationToken
        )
        {
            _logger.LogInformation($"Executed only before {nameof(PrepareEmailHandler)}");
            var response = await next();
            _logger.LogInformation($"Executed only after {nameof(PrepareEmailHandler)}");
            return response;
        }
    }
}
