using MediatR;

namespace ConductorSharp.Engine.Interface;

public interface ITaskRequestHandler<TRequest, TResponse>
    : IWorker,
      IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
}
