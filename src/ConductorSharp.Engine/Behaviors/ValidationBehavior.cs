using System;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;

namespace ConductorSharp.Engine.Behaviors
{
    public class ValidationWorkerMiddleware<TRequest, TResponse> : INgWorkerMiddleware<TRequest, TResponse>
        where TRequest : class, new()
        where TResponse : class, new()
    {
        public async Task<TResponse> Handle(
            TRequest request,
            Func<TRequest, CancellationToken, Task<TResponse>> next,
            CancellationToken cancellationToken
        )
        {
            ObjectValidator.Validate(request);
            var response = await next(request, cancellationToken);
            return response;
        }
    }
}
