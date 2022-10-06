using ConductorSharp.Engine.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Polling
{
    public interface IPollOrderStrategy
    {
        List<TaskToWorker> CalculateOrder(IDictionary<string, int> taskQueue, List<TaskToWorker> taskToWorkerList, int limit);
    }
}
