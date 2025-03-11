using System.Threading;
using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Service;

// By default, only engine shutdown can cancel task execution
internal class NoOpCancellationNotifier : ICancellationNotifier
{
    internal class PassthroughCancellationTokenHolder(CancellationToken cancellationToken) : ICancellationNotifier.ICancellationTokenHolder
    {
        public CancellationToken CancellationToken { get; } = cancellationToken;

        public void Dispose() { }
    }

    public ICancellationNotifier.ICancellationTokenHolder GetCancellationToken(string taskId, CancellationToken engineCancellationToken) =>
        new PassthroughCancellationTokenHolder(engineCancellationToken);
}
