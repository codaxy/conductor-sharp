using ConductorSharp.Engine.Util;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;
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
        private const string LoggerPropertyName = "ConductorContext";

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
            using (LogContext.PushProperty(LoggerPropertyName, _executionContext, true))
            {
                var stopwatch = new Stopwatch();
                _logger.LogInformation($"Submitting request {{@{typeof(TRequest).Name}}}", request);
                stopwatch.Start();

                try
                {
                    var response = await next();

                    stopwatch.Stop();

                    _logger.LogInformation(
                        $"Received response {{@{typeof(TResponse).Name}}} in {{ellapsedMilliseconds}}",
                        response,
                        stopwatch.ElapsedMilliseconds
                    );

                    return response;
                }
                catch (Exception exc)
                {
                    stopwatch.Stop();
                    _logger.LogInformation($"Exception {{exceptionMessage}} in {{elapsedMilliseconds}}", exc.Message, stopwatch.ElapsedMilliseconds);
                    throw;
                }
            }
        }
    }
}
