using MediatR;

namespace XgsPon.MediatR.Behaviors
{
    public abstract class ExceptionBehavior<TRequest, TException> : BaseExceptionBehavior<TRequest, Unit, TException>
        where TException : Exception
        where TRequest : IRequest<Unit>
    {
    }
}
