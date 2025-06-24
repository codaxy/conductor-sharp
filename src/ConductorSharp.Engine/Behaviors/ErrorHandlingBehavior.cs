using System;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Engine.Exceptions;
using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Behaviors
{
    // TODO: Consider removing
    public class ErrorHandlingBehavior<TRequest, TResponse> : INgWorkerMiddleware<TRequest, TResponse>
        where TRequest : class, ITaskInput<TResponse>, new()
    {
        public async Task<TResponse> Handle(
            TRequest request,
            Func<TRequest, CancellationToken, Task<TResponse>> next,
            CancellationToken cancellationToken
        )
        {
            try
            {
                return await next(request, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new BaseWorkerException(ex.Message, ex);
            }
        }
    }
}
