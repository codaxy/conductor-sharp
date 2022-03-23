using MediatR;

namespace XgsPon.MediatR.Behaviors
{
    public class LoggingExceptionBehavior<TRequest, TResponse> : BaseExceptionBehavior<TRequest, TResponse, Exception>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingExceptionBehavior<TRequest, TResponse>> _logger;

        public LoggingExceptionBehavior(ILogger<LoggingExceptionBehavior<TRequest, TResponse>> logger) => _logger = logger;

        protected override void Execute(TRequest request, Exception exception) =>
            _logger.LogError($"Exception occured {{@Exception}} for request {{@{typeof(TRequest).Name}}}", exception, request);
    }
}
