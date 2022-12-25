using ConductorSharp.Engine.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConductorSharp.Engine.Service
{
    public class TaskExecutionCounterService : ITaskExecutionCounterService
    {
        private const int _maxTrackedCount = 10000;

        private readonly ConcurrentDictionary<string, RunningTask> _inProgress = new();
        private readonly ConcurrentDictionary<string, int> _failed = new();
        private readonly ConcurrentDictionary<string, int> _completed = new();

        public void Track(RunningTask trackedTask)
        {
            if (_inProgress.Keys.Count >= _maxTrackedCount)
            {
                return;
            }

            _inProgress.TryAdd(trackedTask.TaskId, trackedTask);
        }

        public void MoveToFailed(string taskId)
        {
            if (!_inProgress.TryGetValue(taskId, out var task))
            {
                return;
            }

            _inProgress.Remove(task.TaskId, out var removedTask);
            _failed.AddOrUpdate(task.TaskName, 1, (id, count) => count + 1);
        }

        public void MoveToCompleted(string taskId)
        {
            if (!_inProgress.TryGetValue(taskId, out var task))
            {
                return;
            }

            _inProgress.Remove(task.TaskId, out var removedTask);
            _completed.AddOrUpdate(task.TaskName, 1, (id, count) => count + 1);
        }

        public List<RunningTask> GetRunning() =>
            _inProgress
                .Select(
                    a =>
                        new RunningTask
                        {
                            StartedAt = a.Value.StartedAt,
                            TaskId = a.Value.TaskId,
                            TaskName = a.Value.TaskName
                        }
                )
                .ToList();

        public List<TaskRunCount> GetCompletedCount() => _completed.Select(a => new TaskRunCount { TaskName = a.Key, Count = a.Value }).ToList();

        public List<TaskRunCount> GetFailedCount() => _failed.Select(a => new TaskRunCount { TaskName = a.Key, Count = a.Value }).ToList();
    }
}
