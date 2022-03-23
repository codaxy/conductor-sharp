using MediatR;

namespace XgsPon.MediatR.Behaviors
{
    public abstract class BaseExceptionBehavior<TRequest, TResponse, TException> : BaseAsyncExceptionBehavior<TRequest, TResponse, TException>
        where TException : Exception
        where TRequest : IRequest<TResponse>
    {
        protected sealed override async Task ExecuteAsync(TRequest request, TException exception)
        {
            Execute(request, exception);
            await Task.CompletedTask;
        }

        protected abstract void Execute(TRequest request, TException exception);
    }
}
