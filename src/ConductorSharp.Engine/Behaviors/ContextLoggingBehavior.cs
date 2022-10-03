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
    public class ContextLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ConductorSharpExecutionContext _executionContext;
        private const string LoggerPropertyName = "ConductorContext";

        public ContextLoggingBehavior(ConductorSharpExecutionContext executionContext)
        {
            _executionContext = executionContext;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            using var _ = LogContext.PushProperty(LoggerPropertyName, _executionContext, true);
            return await next();
        }
    }
}
