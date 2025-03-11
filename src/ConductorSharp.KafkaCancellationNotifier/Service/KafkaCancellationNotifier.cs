using System.Diagnostics;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.KafkaCancellationNotifier.Model;
using Microsoft.Extensions.Logging;
using TaskStatus = ConductorSharp.Client.Generated.TaskStatus;

namespace ConductorSharp.KafkaCancellationNotifier.Service
{
    internal class KafkaCancellationNotifier : ICancellationNotifier
    {
        internal class CancellationTokenSourceHolder : ICancellationNotifier.ICancellationTokenHolder
        {
            private readonly KafkaCancellationNotifier _notifier;
            private readonly string _taskId;

            public CancellationToken CancellationToken { get; }

            public CancellationTokenSourceHolder(CancellationToken cancellationToken, string taskId, KafkaCancellationNotifier notifier)
            {
                CancellationToken = cancellationToken;
                _taskId = taskId;
                _notifier = notifier;
            }

            public void Dispose() => _notifier.ClearTaskCts(_taskId);
        }

        private readonly HashSet<string> _tasks;
        private readonly object _lock = new();
        private readonly ILogger<KafkaCancellationNotifier> _logger;
        private readonly Dictionary<string, CancellationTokenSource> _taskIdToCtsMap = new();

        public KafkaCancellationNotifier(IEnumerable<TaskToWorker> tasks, ILogger<KafkaCancellationNotifier> logger)
        {
            _logger = logger;
            _tasks = tasks.Select(t => t.TaskName).ToHashSet();
        }

        public ICancellationNotifier.ICancellationTokenHolder GetCancellationToken(string taskId, CancellationToken engineCancellationToken)
        {
            var cts = CreateCts(taskId, engineCancellationToken);
            return new CancellationTokenSourceHolder(cts.Token, taskId, this);
        }

        public void HandleKafkaEvent(TaskStatusModel taskStatusModel)
        {
            if (
                taskStatusModel.WorkflowTask.Type != "SIMPLE"
                || (taskStatusModel.Status != TaskStatus.CANCELED && taskStatusModel.Status != TaskStatus.TIMED_OUT)
                || !_tasks.Contains(taskStatusModel.WorkflowTask.Name)
            )
                return;

            var cts = GetCts(taskStatusModel.TaskId);
            if (cts is null)
            {
                _logger.LogWarning("No CancellationTokenSource found for task {TaskId}", taskStatusModel.TaskId);
                return;
            }

            cts.Cancel();
        }

        private CancellationTokenSource CreateCts(string taskId, CancellationToken engineCancellationToken = default)
        {
            CancellationTokenSource cts;
            var stopwatch = Stopwatch.StartNew();

            lock (_lock)
            {
                cts = _taskIdToCtsMap[taskId] = CancellationTokenSource.CreateLinkedTokenSource(engineCancellationToken);
            }
            _logger.LogDebug("CancellationTokenSource creation time {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            return cts;
        }

        private CancellationTokenSource? GetCts(string taskId)
        {
            CancellationTokenSource? cts;
            var stopwatch = Stopwatch.StartNew();

            lock (_lock)
            {
                cts = _taskIdToCtsMap.GetValueOrDefault(taskId);
            }
            _logger.LogDebug("CancellationTokenSource get time {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            return cts;
        }

        private void ClearTaskCts(string taskId)
        {
            var stopwatch = Stopwatch.StartNew();
            lock (_lock)
            {
                _taskIdToCtsMap.Remove(taskId);
            }
            _logger.LogDebug("CancellationTokenSource removal time {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
        }
    }
}
