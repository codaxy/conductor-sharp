using ConductorSharp.Engine.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Polling
{
    public class InverseExponentialBackoff : IPollTimingStrategy
    {
        private const int _backoffRatio = 2;
        private const int _recoveryValue = 50;
        private readonly TimeSpan _recoveryInterval = TimeSpan.FromMilliseconds(5000);

        private DateTimeOffset _lastRecoveryTime = DateTimeOffset.UtcNow;

        public int CalculateDelay(
            IDictionary<string, long> taskQueue,
            List<TaskToWorker> taskToWorkerList,
            int baseSleepInterval,
            int currentSleepInterval
        )
        {
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
