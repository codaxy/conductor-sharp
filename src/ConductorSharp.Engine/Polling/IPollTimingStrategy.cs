using System;
using System.Collections.Generic;
using System.Text;
using ConductorSharp.Engine.Model;

namespace ConductorSharp.Engine.Polling
{
    public interface IPollTimingStrategy
    {
        int CalculateDelay(IDictionary<string, long> taskQueue, List<TaskToWorker> taskToWorkerList, int baseSleepInterval, int currentSleepInterval);
    }
}
