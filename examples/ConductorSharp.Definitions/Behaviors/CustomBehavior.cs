using MediatR;
using Microsoft.Extensions.Logging;

namespace ConductorSharp.Definitions.Behaviors
{
    internal class CustomBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<CustomBehavior<TRequest, TResponse>> _logger;

        public CustomBehavior(ILogger<CustomBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executed before all behaviors");
            var response = await next();
            _logger.LogInformation("Executed after all behaviors");
            return response;
        }
    }
}
