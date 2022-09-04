using ConductorSharp.Engine.Util;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Behaviors
{
    public class RequestResponseLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<RequestResponseLoggingBehavior<TRequest, TResponse>> _logger;
        private readonly ConductorSharpExecutionContext _executionContext;

        public RequestResponseLoggingBehavior(
            ILogger<RequestResponseLoggingBehavior<TRequest, TResponse>> logger,
            ConductorSharpExecutionContext executionContext
        )
        {
            _logger = logger;
            _executionContext = executionContext;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                _logger.LogInformation($"Submitting request {{@{typeof(TRequest).Name}}} in {{@conductorSharpContext}}", request, _executionContext);
                var response = await next();
                stopwatch.Stop();

                _logger.LogInformation(
                    $"Received response {{@{typeof(TResponse).Name}}} in {{@conductorSharpContext}} in {{ellapsedMilliseconds}}",
                    response,
                    _executionContext,
                    stopwatch.ElapsedMilliseconds
                );

                return response;
            }
            catch (Exception exc)
            {
                stopwatch.Stop();
                _logger.LogInformation(
                    $"Exception {{exceptionMessage}} in {{@conductorSharpContext}} in {{elapsedMilliseconds}}",
                    exc.Message,
                    _executionContext,
                    stopwatch.ElapsedMilliseconds
                );
                throw;
            }
        }
    }
}
