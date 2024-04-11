using System;
using System.Collections.Generic;
using System.Text;
using ConductorSharp.Engine.Model;

namespace ConductorSharp.Engine.Polling
{
    public interface IPollOrderStrategy
    {
        List<TaskToWorker> CalculateOrder(IDictionary<string, long> taskQueue, List<TaskToWorker> taskToWorkerList, int limit);
    }
}
