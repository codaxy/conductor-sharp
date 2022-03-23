using MediatR;

namespace XgsPon.MediatR.Behaviors
{
    public abstract class BaseAsyncExceptionBehavior<TRequest, TResponse, TException> : IPipelineBehavior<TRequest, TResponse>
        where TException : Exception
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return await next();
            }
            catch (TException ex)
            {
                await ExecuteAsync(request, ex);
                throw;
            }
        }

        protected abstract Task ExecuteAsync(TRequest request, TException exception);
    }
}
