using ConductorSharp.Engine.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Polling
{
    public class InverseExponentialBackoff : IPollTimingStrategy
    {
        private const int _backoffRatio = 4;
        private const int _minimalSleepInterval = 50;
        private const float _bounceBackRatio = 0.1f;

        public int CalculateDelay(
            IDictionary<string, int> taskQueue,
            List<TaskToWorker> taskToWorkerList,
            int baseSleepInterval,
            int currentSleepInterval
        )
        {
            if (taskToWorkerList.Count > 0)
                return currentSleepInterval / _backoffRatio;

            currentSleepInterval = (int)Math.Max(_minimalSleepInterval, currentSleepInterval + currentSleepInterval * _bounceBackRatio);

            return Math.Min(currentSleepInterval, baseSleepInterval);
        }
    }
}
