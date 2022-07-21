using ConductorSharp.Engine.Exceptions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Behaviors
{
    public class ErrorHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                throw new BaseWorkerException(ex.Message, ex);
            }
        }
    }
}
