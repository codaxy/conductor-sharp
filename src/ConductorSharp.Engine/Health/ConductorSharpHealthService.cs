using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Health
{
    public class HealthData
    {
        public bool IsExecutionManagerRunning { get; set; }
    }

    public class ConductorSharpHealthService : IConductorSharpHealthService, IConductorSharpHealthUpdater
    {
        private static readonly SemaphoreSlim _semaphore = new(1);

        private const string HealthFileName = "CONDUCTORSHARP_HEALTH.json";

        public async Task UnsetExecutionManagerRunning(CancellationToken cancellationToken = default) =>
            await UpdateData(data => data.IsExecutionManagerRunning = false, cancellationToken);

        public async Task SetExecutionManagerRunning(CancellationToken cancellationToken = default) =>
            await UpdateData(data => data.IsExecutionManagerRunning = true, cancellationToken);

        private async Task UpdateData(Action<HealthData> updateHealthData, CancellationToken cancellationToken = default)
        {
            var data = await GetHealthData(cancellationToken);
            updateHealthData(data);
            await WriteHealthData(data, cancellationToken);
        }

        public async Task<HealthData> GetHealthData(CancellationToken cancellationToken = default)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                if (!File.Exists(HealthFileName))
                {
                    return new HealthData();
                }
                else
                {
                    return JsonConvert.DeserializeObject<HealthData>(await File.ReadAllTextAsync(HealthFileName, cancellationToken))
                        ?? new HealthData();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task WriteHealthData(HealthData healthData, CancellationToken cancellationToken = default)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                await File.WriteAllTextAsync(HealthFileName, JsonConvert.SerializeObject(healthData), cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ResetHealthData(CancellationToken cancellationToken = default) =>
            await WriteHealthData(new HealthData(), cancellationToken);

        public void RemoveHealthData()
        {
            if (File.Exists(HealthFileName))
                File.Delete(HealthFileName);

            return;
        }
    }
}
