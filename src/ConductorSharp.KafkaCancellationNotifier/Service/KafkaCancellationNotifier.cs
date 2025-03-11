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
            var cts = CreateCtsIfDoesNotExists(taskId, engineCancellationToken);
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

            var cts = CreateCtsIfDoesNotExists(taskStatusModel.TaskId);
            cts.Cancel();
        }

        private CancellationTokenSource CreateCtsIfDoesNotExists(string taskId, CancellationToken engineCancellationToken = default)
        {
            CancellationTokenSource cts;
            var stopwatch = Stopwatch.StartNew();

            lock (_lock)
            {
                if (!_taskIdToCtsMap.ContainsKey(taskId))
                {
                    cts = CancellationTokenSource.CreateLinkedTokenSource(engineCancellationToken);
                    _taskIdToCtsMap[taskId] = cts;
                }
                else
                    cts = _taskIdToCtsMap[taskId];
            }
            _logger.LogDebug("CancellationTokenSource creation time {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

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
