using ConductorSharp.Engine.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Polling
{
    public class InverseExponentialBackoff : IPollTimingStrategy
    {
        public int CalculateDelay(
            IDictionary<string, int> taskQueue,
            List<TaskToWorker> taskToWorkerList,
            int baseSleepInterval,
            int currentSleepInterval
        )
        {
            if (taskToWorkerList.Count > 0)
                return currentSleepInterval / 2;
            else
                return baseSleepInterval;
        }
    }
}
