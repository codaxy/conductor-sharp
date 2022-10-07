using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Health
{
    public class InMemoryHealthService : IConductorSharpHealthService
    {
        private static bool _isExecutionManagerRunning;

        public Task<HealthData> GetHealthData(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new HealthData { IsExecutionManagerRunning = _isExecutionManagerRunning });
        }

        public void RemoveHealthData() { }

        public Task ResetHealthData(CancellationToken cancellationToken = default)
        {
            _isExecutionManagerRunning = false;
            return Task.CompletedTask;
        }

        public Task SetExecutionManagerRunning(CancellationToken cancellationToken = default)
        {
            _isExecutionManagerRunning = true;
            return Task.CompletedTask;
        }

        public Task UnsetExecutionManagerRunning(CancellationToken cancellationToken = default)
        {
            _isExecutionManagerRunning = false;
            return Task.CompletedTask;
        }
    }
}
