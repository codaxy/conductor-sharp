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

        public RequestResponseLoggingBehavior(ILogger<RequestResponseLoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            _logger.LogInformation($"Submitting request {{@{typeof(TRequest).Name}}}", request);
            stopwatch.Start();

            try
            {
                var response = await next();

                stopwatch.Stop();

                _logger.LogInformation(
                    $"Received response {{@{typeof(TResponse).Name}}} for request {typeof(TRequest).Name} (exec time = {{@EllapsedMilliseconds}})",
                    response,
                    stopwatch.ElapsedMilliseconds
                );

                return response;
            }
            catch (Exception exc)
            {
                stopwatch.Stop();
                _logger.LogError(
                    $"Exception occured {{@Exception}} for request {typeof(TRequest).Name} (exec time = {{@ElapsedMilliseconds}})",
                    exc,
                    stopwatch.ElapsedMilliseconds
                );
                throw;
            }
        }
    }
}
