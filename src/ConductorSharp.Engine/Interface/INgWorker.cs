using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Interface;

public interface INgWorker<TRequest, TResponse>
    where TRequest : class, new()
    where TResponse : new()
{
    Task<TResponse> Handle(TRequest test, CancellationToken cancellationToken);
}
