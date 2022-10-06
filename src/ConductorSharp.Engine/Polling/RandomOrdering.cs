using ConductorSharp.Engine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConductorSharp.Engine.Polling
{
    public class RandomOrdering : IPollOrderStrategy
    {
        private readonly Random _random = new Random();

        public List<TaskToWorker> CalculateOrder(IDictionary<string, int> taskQueue, List<TaskToWorker> taskToWorkerList)
        {
            return taskToWorkerList.OrderBy(a => _random.Next()).ToList();
        }
    }
}
