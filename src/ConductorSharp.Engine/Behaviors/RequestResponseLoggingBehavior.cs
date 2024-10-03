﻿using System;
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
    public class RequestResponseLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<RequestResponseLoggingBehavior<TRequest, TResponse>> _logger;

        public RequestResponseLoggingBehavior(ILogger<RequestResponseLoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid();
            var requestName = typeof(TRequest).Name;

            var stopwatch = new Stopwatch();

            _logger.LogInformation(
                $"Submitting {{@Test}} request {{Request}} with payload {{@{requestName}}} and with id {{RequestId}}",
                new { Test = 2 },
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
