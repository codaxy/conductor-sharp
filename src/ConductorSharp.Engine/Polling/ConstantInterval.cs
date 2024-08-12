using System.Collections.Generic;
using ConductorSharp.Engine.Model;

namespace ConductorSharp.Engine.Polling
{
    internal class ConstantInterval : IPollTimingStrategy
    {
        public int CalculateDelay(
            IDictionary<string, long> taskQueue,
            List<TaskToWorker> taskToWorkerList,
            int baseSleepInterval,
            int currentSleepInterval
        ) => baseSleepInterval;
    }
}
