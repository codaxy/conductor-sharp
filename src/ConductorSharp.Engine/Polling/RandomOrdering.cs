using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConductorSharp.Engine.Model;

namespace ConductorSharp.Engine.Polling
{
    public class RandomOrdering : IPollOrderStrategy
    {
        private readonly Random _random = new Random();

        public List<TaskToWorker> CalculateOrder(IDictionary<string, long> taskQueue, List<TaskToWorker> taskToWorkerList, int limit)
        {
            return taskToWorkerList.OrderBy(a => _random.Next()).Take(limit).ToList();
        }
    }
}
