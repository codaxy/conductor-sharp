using System;
using System.Collections.Generic;
using System.Text;
using ConductorSharp.Engine.Model;

namespace ConductorSharp.Engine.Polling
{
    internal class InverseExponentialBackoff : IPollTimingStrategy
    {
        private const int _backoffRatio = 2;
        private const int _recoveryValue = 250;
        private readonly TimeSpan _recoveryInterval = TimeSpan.FromMilliseconds(5000);

        private DateTimeOffset _lastRecoveryTime = DateTimeOffset.UtcNow;

        public int CalculateDelay(
            IDictionary<string, long> taskQueue,
            List<TaskToWorker> taskToWorkerList,
            int baseSleepInterval,
            int currentSleepInterval
        )
        {
            if (baseSleepInterval < _recoveryValue)
                throw new ArgumentException($"Sleep interval must be greater than or equal than {_recoveryValue}ms");

            if (taskToWorkerList.Count > 0)
            {
                currentSleepInterval /= _backoffRatio;
            }
            else if (DateTimeOffset.UtcNow - _lastRecoveryTime > _recoveryInterval)
            {
                currentSleepInterval += 100;
                _lastRecoveryTime = DateTimeOffset.UtcNow;
            }

            return Math.Clamp(currentSleepInterval, _recoveryValue, baseSleepInterval);
        }
    }
}
