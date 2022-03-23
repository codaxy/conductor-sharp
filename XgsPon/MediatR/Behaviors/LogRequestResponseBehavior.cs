using MediatR;

namespace XgsPon.MediatR.Behaviors
{
    public class LogRequestResponseBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LogRequestResponseBehavior<TRequest, TResponse>> _logger;

        public LogRequestResponseBehavior(ILogger<LogRequestResponseBehavior<TRequest, TResponse>> logger) => _logger = logger;

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            _logger.LogInformation($"Submitting request {{@{typeof(TRequest).Name}}}", request);
            var response = await next();
            _logger.LogInformation($"Received response {{@{typeof(TResponse).Name}}}", response);
            return response;
        }
    }
}
