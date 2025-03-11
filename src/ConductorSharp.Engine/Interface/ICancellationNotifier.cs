using System;
using System.Threading;

namespace ConductorSharp.Engine.Interface
{
    public interface ICancellationNotifier
    {
        public interface ICancellationTokenHolder : IDisposable
        {
            CancellationToken CancellationToken { get; }
        }

        ICancellationTokenHolder GetCancellationToken(string taskId, CancellationToken engineCancellationToken);
    }
}
