using ConductorSharp.Engine.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Polling
{
    public interface IPollTimingStrategy
    {
        int CalculateDelay(IDictionary<string, long> taskQueue, List<TaskToWorker> taskToWorkerList, int baseSleepInterval, int currentSleepInterval);
    }
}
