using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Health
{
    internal interface One { }

    public interface Two { }

    public class HealthData
    {
        public bool IsDeploymentStarted { get; set; }
        public bool IsDeploymentCompleted { get; set; }
        public bool IsExecutionManagerStarted { get; set; }
    }

    public class ConductorSharpHealthService : IConductorSharpHealthService, IConductorSharpHealthUpdater
    {
        private static readonly SemaphoreSlim _semaphore = new(1);

        private const string HealthFileName = "CONDUCTORSHARP_HEALTH.json";

        public async Task SetDeploymentCompleted() => await UpdateData(data => data.IsDeploymentCompleted = true);

        public async Task SetDeploymentStarted() => await UpdateData(data => data.IsDeploymentStarted = true);

        public async Task SetExecutionManagerStarted() => await UpdateData(data => data.IsExecutionManagerStarted = true);

        private async Task UpdateData(Action<HealthData> updateHealthData)
        {
            await _semaphore.WaitAsync();

            try
            {
                var data = await GetHealthData();
                updateHealthData(data);
                await WriteHealthData(data);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<HealthData> GetHealthData()
        {
            await _semaphore.WaitAsync();

            try
            {
                if (!File.Exists(HealthFileName))
                {
                    return new HealthData();
                }
                else
                {
                    return JsonConvert.DeserializeObject<HealthData>(await File.ReadAllTextAsync(HealthFileName)) ?? new HealthData();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task WriteHealthData(HealthData healthData) =>
            await File.WriteAllTextAsync(HealthFileName, JsonConvert.SerializeObject(healthData));

        public async Task ResetHealthData() => await WriteHealthData(new HealthData());

        public Task RemoveHealthData()
        {
            if (File.Exists(HealthFileName))
                File.Delete(HealthFileName);

            return Task.CompletedTask;
        }
    }
}
