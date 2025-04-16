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

            public bool IsCancellationRequestedByNotifier => _notifier.IsCancellationRequested(_taskId);

            public CancellationTokenSourceHolder(CancellationToken cancellationToken, string taskId, KafkaCancellationNotifier notifier)
            {
                CancellationToken = cancellationToken;
                _taskId = taskId;
                _notifier = notifier;
            }

            public void Dispose() => _notifier.ClearTaskCts(_taskId);
        }

        private class TaskCancellationInfo
        {
            public TaskCancellationInfo(CancellationTokenSource cancellationTokenSource)
            {
                CancellationTokenSource = cancellationTokenSource;
            }

            public CancellationTokenSource CancellationTokenSource { get; }
            public bool IsCancellationRequested { get; set; }
        }

        private readonly HashSet<string> _tasks;
        private readonly object _lock = new();
        private readonly ILogger<KafkaCancellationNotifier> _logger;
        private readonly Dictionary<string, TaskCancellationInfo> _taskIdToInfoMap = new();

        public KafkaCancellationNotifier(IEnumerable<TaskToWorker> tasks, ILogger<KafkaCancellationNotifier> logger)
        {
            _logger = logger;
            _tasks = tasks.Select(t => t.TaskName).ToHashSet();
        }

        public ICancellationNotifier.ICancellationTokenHolder GetCancellationToken(string taskId, CancellationToken engineCancellationToken)
        {
            var token = CreateTaskCancellationInfoAndGetToken(taskId, engineCancellationToken);
            return new CancellationTokenSourceHolder(token, taskId, this);
        }

        public void HandleKafkaEvent(TaskStatusModel taskStatusModel)
        {
            if (
                taskStatusModel.WorkflowTask.Type != "SIMPLE"
                || (taskStatusModel.Status != TaskStatus.CANCELED && taskStatusModel.Status != TaskStatus.TIMED_OUT)
                || !_tasks.Contains(taskStatusModel.WorkflowTask.Name)
            )
                return;

            TryToCancelTask(taskStatusModel);
        }

        private CancellationToken CreateTaskCancellationInfoAndGetToken(string taskId, CancellationToken engineCancellationToken = default)
        {
            CancellationToken token;

            lock (_lock)
            {
                var info = _taskIdToInfoMap[taskId] = new TaskCancellationInfo(
                    CancellationTokenSource.CreateLinkedTokenSource(engineCancellationToken)
                );
                token = info.CancellationTokenSource.Token;
            }

            return token;
        }

        private void TryToCancelTask(TaskStatusModel taskStatusModel)
        {
            TaskCancellationInfo? info;

            lock (_lock)
            {
                info = _taskIdToInfoMap.GetValueOrDefault(taskStatusModel.TaskId);
            }

            if (info is null)
            {
                _logger.LogWarning(
                    "Unable to cancel task {TaskId} of workflow {WorkflowId}",
                    taskStatusModel.TaskId,
                    taskStatusModel.WorkflowInstanceId
                );
                return;
            }

            lock (_lock)
            {
                info.IsCancellationRequested = true;
                info.CancellationTokenSource.Cancel();
            }
        }

        private void ClearTaskCts(string taskId)
        {
            lock (_lock)
            {
                _taskIdToInfoMap[taskId].CancellationTokenSource.Dispose();
                _taskIdToInfoMap.Remove(taskId);
            }
        }

        private bool IsCancellationRequested(string taskId)
        {
            lock (_lock)
            {
                return _taskIdToInfoMap[taskId].IsCancellationRequested;
            }
        }
    }
}
