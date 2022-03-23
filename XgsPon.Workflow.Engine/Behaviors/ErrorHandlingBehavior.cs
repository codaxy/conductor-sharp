using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XgsPon.Workflows.Engine.Exceptions;
using XgsPon.Workflows.Engine.Util;

namespace XgsPon.Workflows.Engine.Behaviors
{
    public class ErrorHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next
        )
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
