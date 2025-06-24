using ConductorSharp.Engine.Interface;
using Microsoft.Extensions.Logging;

namespace ConductorSharp.Definitions.Behaviors
{
    internal class CustomBehavior<TRequest, TResponse> : INgWorkerMiddleware<TRequest, TResponse>
        where TRequest : class, ITaskInput<TResponse>, new()
    {
        private readonly ILogger<CustomBehavior<TRequest, TResponse>> _logger;

        public CustomBehavior(ILogger<CustomBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            Func<TRequest, CancellationToken, Task<TResponse>> next,
            CancellationToken cancellationToken
        )
        {
            _logger.LogInformation("Executed before all behaviors");
            var response = await next(request, cancellationToken);
            _logger.LogInformation("Executed after all behaviors");
            return response;
        }
    }
}
