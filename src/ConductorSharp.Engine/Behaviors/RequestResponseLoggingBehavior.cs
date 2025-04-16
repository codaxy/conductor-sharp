using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Engine.Util;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace ConductorSharp.Engine.Behaviors
{
    // TODO: Consider removing this
    public class RequestResponseLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<RequestResponseLoggingBehavior<TRequest, TResponse>> _logger;
        private readonly ConductorSharpExecutionContext _context;

        public RequestResponseLoggingBehavior(
            ILogger<RequestResponseLoggingBehavior<TRequest, TResponse>> logger,
            ConductorSharpExecutionContext context
        )
        {
            _logger = logger;
            _context = context;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid();
            var requestName = typeof(TRequest).Name;

            var stopwatch = new Stopwatch();

            _logger.LogInformation(
                $"Submitting request {{Request}} with payload {{@{requestName}}} and with id {{RequestId}}",
                requestName,
                request,
                requestId
            );
            stopwatch.Start();

            try
            {
                var response = await next();

                stopwatch.Stop();

                _logger.LogInformation(
                    $"Received response {{@Response}} for request {{Request}} with id {{RequestId}} (exec time = {{ElapsedMilliseconds}})",
                    response,
                    requestName,
                    requestId,
                    stopwatch.ElapsedMilliseconds
                );

                return response;
            }
            catch (OperationCanceledException) when (_context.TaskId != null)
            {
                // Simply rethrow and do not log in order for cancellation notifier to work
                throw;
            }
            catch (Exception exc)
            {
                stopwatch.Stop();
                _logger.LogError(
                    $"Exception occured {{@Exception}} for request {{Request}} with id {{RequestId}} (exec time = {{ElapsedMilliseconds}})",
                    exc,
                    requestName,
                    requestId,
                    stopwatch.ElapsedMilliseconds
                );
                throw;
            }
        }
    }
}
