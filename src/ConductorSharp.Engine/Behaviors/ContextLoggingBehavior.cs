using System;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using Serilog.Context;

namespace ConductorSharp.Engine.Behaviors
{
    public class ContextLoggingBehavior<TRequest, TResponse> : INgWorkerMiddleware<TRequest, TResponse>
        where TRequest : class, ITaskInput<TResponse>, new()
    {
        private readonly ConductorSharpExecutionContext _executionContext;
        private const string LoggerPropertyName = "ConductorContext";

        public ContextLoggingBehavior(ConductorSharpExecutionContext executionContext)
        {
            _executionContext = executionContext;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            Func<TRequest, CancellationToken, Task<TResponse>> next,
            CancellationToken cancellationToken
        )
        {
            using var _ = LogContext.PushProperty(LoggerPropertyName, _executionContext, true);
            return await next(request, cancellationToken);
        }
    }
}
