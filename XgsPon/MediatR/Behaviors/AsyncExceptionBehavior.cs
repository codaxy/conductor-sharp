using MediatR;

namespace XgsPon.MediatR.Behaviors
{
    public abstract class AsyncExceptionBehavior<TRequest, TException>
        : BaseAsyncExceptionBehavior<TRequest, Unit, TException>
        where TException : Exception
        where TRequest : IRequest<Unit>
    {
    }
}
