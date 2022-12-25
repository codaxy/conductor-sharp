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
        public DateTimeOffset CompletedAt { get; set; }
        public DateTimeOffset FailedAt { get; set; }
    }

    public class TaskExecutionCounterService
    {
        private ConcurrentDictionary<string, TrackedTask> InProgress { get; set; } = new();
        private ConcurrentDictionary<string, int> Failed { get; set; } = new();
        private ConcurrentDictionary<string, int> Completed { get; set; } = new();

        public void Track(TrackedTask trackedTask)
        {
            InProgress.TryAdd(trackedTask.TaskId, trackedTask);
        }

        public void MoveToFailed(string taskId)
        {
            if (!InProgress.TryGetValue(taskId, out var task))
            {
                return;
            }

            InProgress.Remove(task.TaskId, out var removedTask);
            Failed.AddOrUpdate(task.TaskName, 0, (id, count) => count++);
        }

        public void MoveToCompleted(string taskId)
        {
            if (!InProgress.TryGetValue(taskId, out var task))
            {
                return;
            }

            InProgress.Remove(task.TaskId, out var removedTask);
            Completed.AddOrUpdate(task.TaskName, 0, (id, count) => count++);
        }
    }
}
