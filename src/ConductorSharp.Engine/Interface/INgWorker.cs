using System;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Engine.Util;

namespace ConductorSharp.Engine.Interface;

public interface INgWorker<TRequest, TResponse>
    where TRequest : class, ITaskInput<TResponse>, new()
{
    Task<TResponse> Handle(TRequest test, CancellationToken cancellationToken);
}
