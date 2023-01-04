using ConductorSharp.Engine.Interface;
using System.Collections.Concurrent;

namespace ConductorSharp.ApiEnabled.Services
{
    public class TaskExecutionCounterService : ITaskExecutionCounterService
    {
        private const int _maxTrackedCount = 10000;

        private readonly ConcurrentDictionary<string, RunningTask> _inProgress = new();
        private readonly ConcurrentDictionary<string, int> _failed = new();
        private readonly ConcurrentDictionary<string, int> _completed = new();

        public Task OnPolled(RunningTask trackedTask)
        {
            _inProgress.TryAdd(trackedTask.TaskId, trackedTask);

            return Task.CompletedTask;
        }

        public Task OnFailed(RunningTask trackedTask)
        {
            if (_inProgress.TryRemove(trackedTask.TaskId, out var removedTask))
            {
                _failed[removedTask.TaskName]++;
            }

            return Task.CompletedTask;
        }

        public Task OnCompleted(RunningTask trackedTask)
        {
            if (_inProgress.TryRemove(trackedTask.TaskId, out var removedTask))
            {
                _completed[removedTask.TaskName]++;
            }

            return Task.CompletedTask;
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
