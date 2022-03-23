using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XgsPon.Workflows.Engine.Util;

namespace XgsPon.Workflows.Engine.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next
        )
        {
            ObjectValidator.Validate(request);

            var response = await next();

            ObjectValidator.Validate(response);

            return response;
        }
    }
}
