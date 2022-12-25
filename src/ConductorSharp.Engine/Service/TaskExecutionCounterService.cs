using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConductorSharp.Engine.Service
{
    public class TrackedTask
    {
        public string TaskId { get; set; }
        public string TaskName { get; set; }
        public DateTimeOffset StartedAt { get; set; }
    }

    public class TaskExecutionCounterService
    {
        private const int _maxTrackedCount = 10000;

        public ConcurrentDictionary<string, TrackedTask> InProgress { get; } = new();
        public ConcurrentDictionary<string, int> Failed { get; } = new();
        public ConcurrentDictionary<string, int> Completed { get; } = new();

        public void Track(TrackedTask trackedTask)
        {
            if (InProgress.Keys.Count >= _maxTrackedCount)
            {
                return;
            }

            InProgress.TryAdd(trackedTask.TaskId, trackedTask);
        }

        public void MoveToFailed(string taskId)
        {
            if (!InProgress.TryGetValue(taskId, out var task))
            {
                return;
            }

            InProgress.Remove(task.TaskId, out var removedTask);
            Failed.AddOrUpdate(task.TaskName, 1, (id, count) => count + 1);
        }

        public void MoveToCompleted(string taskId)
        {
            if (!InProgress.TryGetValue(taskId, out var task))
            {
                return;
            }

            InProgress.Remove(task.TaskId, out var removedTask);
            Completed.AddOrUpdate(task.TaskName, 1, (id, count) => count + 1);
        }
    }
}
