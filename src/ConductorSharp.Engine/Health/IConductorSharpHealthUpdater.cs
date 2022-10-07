using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Health
{
    internal interface IConductorSharpHealthUpdater
    {
        Task ResetHealthData(CancellationToken cancellationToken = default);
        void RemoveHealthData();
        Task SetExecutionManagerRunning(CancellationToken cancellationToken = default);
        Task UnsetExecutionManagerRunning(CancellationToken cancellationToken = default);
    }
}
