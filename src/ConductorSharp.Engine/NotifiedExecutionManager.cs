using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ConductorSharp.Client;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Model;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Engine
{
    /// <summary>
    /// Polls a task type when the transport announces that tasks of it may be waiting, and keeps
    /// polling it until its queue is empty. Every type is also polled on a slow fallback interval.
    /// </summary>
    /// <remarks>
    /// The other execution managers read the queue sizes on every sleep interval, whether anything
    /// was scheduled or not. This one sends nothing while it is idle, apart from the fallback poll,
    /// which picks up tasks whose notice the transport lost.
    ///
    /// A notice marks its type pending. The loop polls each pending type in batches as large as the
    /// free worker slots, and waits for a slot when every worker is busy. It clears the type once a
    /// poll returns fewer tasks than it asked for, which means the queue was empty. A notice that
    /// arrives while a poll is out keeps the type pending. That poll may have read the queue before
    /// the commit the notice stands for, and clearing the type would leave that task for the
    /// fallback poll.
    /// </remarks>
    internal class NotifiedExecutionManager : IExecutionManager
    {
        /// <summary>
        /// The most tasks one batch poll asks for. Conductor and Maestro refuse more than 100.
        /// </summary>
        private const int MaxBatchSize = 100;

        private static readonly TimeSpan MaxSubscribeRetryDelay = TimeSpan.FromSeconds(30);

        private readonly WorkerSetConfig _configuration;
        private readonly TimeSpan _fallbackPollInterval;
        private readonly ILogger<NotifiedExecutionManager> _logger;
        private readonly ITaskService _taskService;
        private readonly ITaskAvailabilityNotifications _notifications;
        private readonly IReadOnlyList<TaskToWorker> _registeredWorkers;
        private readonly PolledTaskRunner _runner;
        private readonly SemaphoreSlim _slots;

        /// <summary>
        /// Wakes the loop when a notice arrives. One wake waiting is enough, because the loop looks
        /// at every pending type when it wakes.
        /// </summary>
        private readonly Channel<bool> _wake = Channel.CreateBounded<bool>(
            new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropWrite }
        );

        public NotifiedExecutionManager(
            WorkerSetConfig configuration,
            NotifiedPollingConfig pollingConfig,
            ILogger<NotifiedExecutionManager> logger,
            ITaskService taskService,
            ITaskAvailabilityNotifications notifications,
            IEnumerable<TaskToWorker> workerMappings,
            PolledTaskRunner runner
        )
        {
            _configuration = configuration;
            _fallbackPollInterval = TimeSpan.FromMilliseconds(pollingConfig.FallbackPollInterval);
            _logger = logger;
            _taskService = taskService;
            _notifications = notifications;
            _registeredWorkers = workerMappings.ToList();
            _runner = runner;
            _slots = new SemaphoreSlim(configuration.MaxConcurrentWorkers);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var queues = _registeredWorkers.Select(worker => new WorkerQueue(worker)).ToList();
            var queuesByTaskType = queues.GroupBy(queue => queue.Worker.TaskName).ToDictionary(group => group.Key, group => group.ToList());

            await using var subscription = await SubscribeAsync(queuesByTaskType, cancellationToken);

            // Tasks queued before the subscription was in place had their notices already.
            MarkPending(queues);
            var nextFallbackPoll = DateTimeOffset.UtcNow + _fallbackPollInterval;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (DateTimeOffset.UtcNow >= nextFallbackPoll)
                {
                    MarkPending(queues);
                    nextFallbackPoll = DateTimeOffset.UtcNow + _fallbackPollInterval;
                }

                var pending = queues.Where(queue => queue.IsPending).ToList();
                if (pending.Count == 0)
                {
                    await WaitForNoticeAsync(nextFallbackPoll - DateTimeOffset.UtcNow, cancellationToken);
                    continue;
                }

                // Shuffled, so a type that always has work cannot keep the slots from the others.
                foreach (var queue in pending.OrderBy(_ => Random.Shared.Next()))
                {
                    await PollAsync(queue, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Subscribes to the notices of every registered type, retrying until the transport accepts.
        /// Nothing is polled before that, since a poll's answer is only useful while notices arrive.
        /// </summary>
        private async Task<IAsyncDisposable> SubscribeAsync(
            Dictionary<string, List<WorkerQueue>> queuesByTaskType,
            CancellationToken cancellationToken
        )
        {
            var delay = TimeSpan.FromSeconds(1);

            while (true)
            {
                try
                {
                    return await _notifications.SubscribeAsync(
                        queuesByTaskType.Keys.ToList(),
                        taskType => OnNotice(queuesByTaskType, taskType),
                        cancellationToken
                    );
                }
                catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning(exception, "Could not subscribe to task availability notices; retrying in {Delay}", delay);

                    await Task.Delay(delay, cancellationToken);
                    delay = TimeSpan.FromTicks(Math.Min(delay.Ticks * 2, MaxSubscribeRetryDelay.Ticks));
                }
            }
        }

        private void OnNotice(Dictionary<string, List<WorkerQueue>> queuesByTaskType, string taskType)
        {
            if (!queuesByTaskType.TryGetValue(taskType, out var queues))
                return;

            foreach (var queue in queues)
                queue.MarkPending();

            _wake.Writer.TryWrite(true);
        }

        private static void MarkPending(IEnumerable<WorkerQueue> queues)
        {
            foreach (var queue in queues)
                queue.MarkPending();
        }

        /// <summary>
        /// Waits for a notice, or until <paramref name="timeout"/> passes and the fallback poll is due.
        /// </summary>
        private async Task WaitForNoticeAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (timeout <= TimeSpan.Zero)
                return;

            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(timeout);

            try
            {
                await _wake.Reader.ReadAsync(timeoutSource.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // The fallback poll is due.
            }
        }

        /// <summary>
        /// Polls one type for as many tasks as there are free worker slots, and starts each task it gets.
        /// </summary>
        private async Task PollAsync(WorkerQueue queue, CancellationToken cancellationToken)
        {
            // A slot is taken for every task the poll may return, so a polled task never waits for a
            // worker. At least one, which is where the loop waits while every worker is busy.
            await _slots.WaitAsync(cancellationToken);
            var slots = 1;
            while (slots < MaxBatchSize && _slots.Wait(0))
                slots++;

            var generation = queue.Generation;

            // TODO: Maybe this should be configurable, as in the other execution managers.
            var workerId = Guid.NewGuid().ToString();

            List<Client.Generated.Task> tasks;
            try
            {
                var polled = await _taskService.BatchPollAsync(
                    queue.Worker.TaskName,
                    workerId,
                    queue.Worker.TaskDomain ?? _configuration.Domain,
                    slots,
                    timeout: 0,
                    cancellationToken
                );

                tasks = polled?.Where(task => task != null).ToList() ?? new List<Client.Generated.Task>();
            }
            catch (ApiException exception) when (exception.StatusCode == 204)
            {
                tasks = new List<Client.Generated.Task>();
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                _slots.Release(slots);
                _logger.LogError(exception, "Exception during the task polling");

                // The type stays pending. Waiting first keeps a server that is down from being
                // polled in a tight loop.
                await Task.Delay(Math.Max(_configuration.SleepInterval, 1000), cancellationToken);
                return;
            }
            catch
            {
                _slots.Release(slots);
                throw;
            }

            if (tasks.Count < slots)
            {
                _slots.Release(slots - tasks.Count);
                queue.MarkDrained(generation);
            }

            foreach (var task in tasks)
            {
                _ = RunAsync(task, workerId, queue.Worker, cancellationToken);
            }
        }

        private async Task RunAsync(Client.Generated.Task task, string workerId, TaskToWorker worker, CancellationToken cancellationToken)
        {
            try
            {
                await _runner.RunAsync(task, workerId, worker, cancellationToken);
            }
            catch (Exception exception)
            {
                // Nobody awaits this task, so an exception left here would go unseen.
                _logger.LogError(exception, "Exception while running task {TaskId}", task.TaskId);
            }
            finally
            {
                _slots.Release();
            }
        }

        /// <summary>
        /// One registered worker's type, and whether tasks of it may be waiting.
        /// </summary>
        private sealed class WorkerQueue(TaskToWorker worker)
        {
            private readonly object _gate = new();
            private bool _pending;
            private long _generation;

            public TaskToWorker Worker { get; } = worker;

            public bool IsPending
            {
                get
                {
                    lock (_gate)
                        return _pending;
                }
            }

            /// <summary>
            /// Counts the notices so far. A poll reads it before it starts and hands it to
            /// <see cref="MarkDrained"/>.
            /// </summary>
            public long Generation
            {
                get
                {
                    lock (_gate)
                        return _generation;
                }
            }

            public void MarkPending()
            {
                lock (_gate)
                {
                    _generation++;
                    _pending = true;
                }
            }

            /// <summary>
            /// Clears the type after a poll found its queue empty, unless a notice arrived after the
            /// poll read <paramref name="generation"/>. The comparison and the clear are one step, so
            /// a notice cannot land between them and be lost.
            /// </summary>
            public void MarkDrained(long generation)
            {
                lock (_gate)
                {
                    if (_generation == generation)
                        _pending = false;
                }
            }
        }
    }
}
