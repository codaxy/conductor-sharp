using System.Collections.Concurrent;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ConductorTask = ConductorSharp.Client.Generated.Task;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Engine.Tests.Unit
{
    /// <summary>
    /// The execution manager behind <c>UseTaskAvailabilityNotifications</c> polls a type when the
    /// transport announces it, drains its queue, and otherwise only polls on the fallback interval.
    /// </summary>
    public class NotifiedExecutionManagerTests
    {
        private const string TaskName = "NOTICE_probe";

        /// <summary>
        /// Long enough that no test sees a fallback poll unless it asks for one.
        /// </summary>
        private const int NoFallbackPoll = 600_000;

        [Fact]
        public async Task Idle_worker_polls_once_on_start_and_then_only_on_a_notice()
        {
            var taskService = new QueueTaskService();
            var notifications = new FakeNotifications();
            await using var manager = await RunningManager.StartAsync(taskService, notifications, NoFallbackPoll);

            // The catch-up poll after subscribing, for tasks queued before the subscription.
            await WaitUntilAsync(() => taskService.Polls == 1);
            await Task.Delay(300);
            Assert.Equal(1, taskService.Polls);

            taskService.Enqueue(ProbeTask("task-1"));
            notifications.Notify(TaskName);

            await WaitUntilAsync(() => taskService.Updates.Count == 1);
            Assert.Equal(TaskResultStatus.COMPLETED, taskService.Updates.Single().Status);

            // One task came back for the slots it asked for, so the queue was empty and the
            // manager is idle again.
            await Task.Delay(300);
            Assert.Equal(2, taskService.Polls);
        }

        [Fact]
        public async Task A_notice_drains_the_queue_through_the_free_slots()
        {
            var taskService = new QueueTaskService();
            var notifications = new FakeNotifications();
            await using var manager = await RunningManager.StartAsync(taskService, notifications, NoFallbackPoll, maxConcurrentWorkers: 2);
            await WaitUntilAsync(() => taskService.Polls == 1);

            for (var i = 0; i < 5; i++)
                taskService.Enqueue(ProbeTask($"task-{i}", delayMs: 50));

            notifications.Notify(TaskName);

            // One notice is enough: with two slots the manager polls again whenever a slot frees,
            // until a poll comes back short.
            await WaitUntilAsync(() => taskService.Updates.Count == 5);
            Assert.True(taskService.LargestBatchRequested <= 2, $"Asked for {taskService.LargestBatchRequested} tasks with two slots");
        }

        [Fact]
        public async Task A_notice_that_arrives_during_a_poll_is_not_lost()
        {
            var taskService = new QueueTaskService();
            var notifications = new FakeNotifications();

            // The catch-up poll is held open, and answers empty however the queue looks by then: it
            // read the queue before the task below was committed.
            var heldPoll = taskService.HoldNextPoll();
            await using var manager = await RunningManager.StartAsync(taskService, notifications, NoFallbackPoll);
            await WaitUntilAsync(() => taskService.Polls == 1);

            taskService.Enqueue(ProbeTask("task-1"));
            notifications.Notify(TaskName);
            heldPoll.SetResult(true);

            await WaitUntilAsync(() => taskService.Updates.Count == 1);
            Assert.Equal(2, taskService.Polls);
        }

        [Fact]
        public async Task Fallback_poll_finds_a_task_whose_notice_was_lost()
        {
            var taskService = new QueueTaskService();
            var notifications = new FakeNotifications();
            await using var manager = await RunningManager.StartAsync(taskService, notifications, fallbackPollInterval: 200);
            await WaitUntilAsync(() => taskService.Polls >= 1);

            taskService.Enqueue(ProbeTask("task-1"));

            await WaitUntilAsync(() => taskService.Updates.Count == 1);
        }

        [Fact]
        public async Task Subscribes_before_the_first_poll_and_unsubscribes_on_stop()
        {
            var taskService = new QueueTaskService();
            var notifications = new FakeNotifications();
            var manager = await RunningManager.StartAsync(taskService, notifications, NoFallbackPoll);
            await WaitUntilAsync(() => taskService.Polls == 1);

            Assert.Equal(new[] { TaskName }, notifications.TaskTypes);
            Assert.True(notifications.SubscribedAt <= taskService.FirstPollAt);

            await manager.DisposeAsync();

            Assert.True(notifications.Unsubscribed);
        }

        [Fact]
        public async Task Retries_the_subscription_until_the_transport_accepts_it()
        {
            var taskService = new QueueTaskService();
            var notifications = new FakeNotifications { FailingSubscriptions = 1 };
            await using var manager = await RunningManager.StartAsync(taskService, notifications, NoFallbackPoll);

            await WaitUntilAsync(() => taskService.Polls == 1);
            Assert.Equal(2, notifications.SubscribeAttempts);
        }

        [Fact]
        public void UseTaskAvailabilityNotifications_replaces_the_execution_manager()
        {
            var services = CreateServices(new QueueTaskService(), new FakeNotifications(), NoFallbackPoll, maxConcurrentWorkers: 1);
            using var provider = services.BuildServiceProvider();

            Assert.IsType<NotifiedExecutionManager>(provider.GetRequiredService<IExecutionManager>());
        }

        [Fact]
        public void UseTaskAvailabilityNotifications_rejects_a_fallback_interval_that_is_not_positive()
        {
            var builder = new ServiceCollection()
                .AddConductorSharp(baseUrl: "http://empty/empty")
                .AddExecutionManager(1, 250, 500, null, typeof(NotifiedExecutionManagerTests).Assembly);

            Assert.Throws<ArgumentOutOfRangeException>(() => builder.UseTaskAvailabilityNotifications(0));
        }

        private static ConductorTask ProbeTask(string taskId, int delayMs = 0) =>
            new()
            {
                TaskId = taskId,
                TaskDefName = TaskName,
                WorkflowInstanceId = "workflow-1",
                InputData = new Dictionary<string, object> { ["delay_ms"] = delayMs },
            };

        private static async Task WaitUntilAsync(Func<bool> condition)
        {
            var deadline = DateTime.UtcNow.AddSeconds(10);

            while (!condition())
            {
                Assert.True(DateTime.UtcNow < deadline, "The execution manager did not get there in time");
                await Task.Delay(10);
            }
        }

        private static ServiceCollection CreateServices(
            QueueTaskService taskService,
            FakeNotifications notifications,
            int fallbackPollInterval,
            int maxConcurrentWorkers
        )
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services
                .AddConductorSharp(baseUrl: "http://empty/empty")
                .AddExecutionManager(
                    maxConcurrentWorkers: maxConcurrentWorkers,
                    sleepInterval: 250,
                    longPollInterval: 500,
                    domain: null,
                    handlerAssemblies: typeof(NotifiedExecutionManagerTests).Assembly
                )
                .UseTaskAvailabilityNotifications(fallbackPollInterval);
            services.RegisterWorkerTask<NoticeProbeHandler>();
            services.RemoveAll<ITaskService>();
            services.AddSingleton<ITaskService>(taskService);
            services.AddSingleton<ITaskAvailabilityNotifications>(notifications);

            return services;
        }

        /// <summary>
        /// An execution manager running in the background, stopped on dispose.
        /// </summary>
        private sealed class RunningManager : IAsyncDisposable
        {
            private readonly ServiceProvider _provider;
            private readonly CancellationTokenSource _stop = new();
            private Task _run;

            private RunningManager(ServiceProvider provider)
            {
                _provider = provider;
            }

            public static Task<RunningManager> StartAsync(
                QueueTaskService taskService,
                FakeNotifications notifications,
                int fallbackPollInterval,
                int maxConcurrentWorkers = 2
            )
            {
                var provider = CreateServices(taskService, notifications, fallbackPollInterval, maxConcurrentWorkers).BuildServiceProvider();
                var running = new RunningManager(provider);
                running._run = provider.GetRequiredService<IExecutionManager>().StartAsync(running._stop.Token);

                return Task.FromResult(running);
            }

            public async ValueTask DisposeAsync()
            {
                _stop.Cancel();
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _run);
                await _provider.DisposeAsync();
            }
        }

        public class NoticeProbeRequest : IRequest<NoticeProbeResponse>
        {
            public int DelayMs { get; set; }
        }

        public class NoticeProbeResponse { }

        [OriginalName(TaskName)]
        public class NoticeProbeHandler : ITaskRequestHandler<NoticeProbeRequest, NoticeProbeResponse>
        {
            public async Task<NoticeProbeResponse> Handle(NoticeProbeRequest request, CancellationToken cancellationToken)
            {
                if (request.DelayMs > 0)
                    await Task.Delay(request.DelayMs, cancellationToken);

                return new NoticeProbeResponse();
            }
        }

        /// <summary>
        /// Notices on demand, and a record of the subscription.
        /// </summary>
        private sealed class FakeNotifications : ITaskAvailabilityNotifications
        {
            private Action<string> _onAvailable;
            private int _subscribeAttempts;

            public int FailingSubscriptions { get; init; }

            public IReadOnlyCollection<string> TaskTypes { get; private set; }

            public DateTime SubscribedAt { get; private set; } = DateTime.MaxValue;

            public bool Unsubscribed { get; private set; }

            public int SubscribeAttempts => _subscribeAttempts;

            public void Notify(string taskType) => _onAvailable?.Invoke(taskType);

            public Task<IAsyncDisposable> SubscribeAsync(
                IReadOnlyCollection<string> taskTypes,
                Action<string> onAvailable,
                CancellationToken cancellationToken = default
            )
            {
                if (Interlocked.Increment(ref _subscribeAttempts) <= FailingSubscriptions)
                    throw new InvalidOperationException("The transport is not connected");

                TaskTypes = taskTypes;
                _onAvailable = onAvailable;
                SubscribedAt = DateTime.UtcNow;

                return Task.FromResult<IAsyncDisposable>(new Subscription(this));
            }

            private sealed class Subscription(FakeNotifications owner) : IAsyncDisposable
            {
                public ValueTask DisposeAsync()
                {
                    owner.Unsubscribed = true;
                    return ValueTask.CompletedTask;
                }
            }
        }

        /// <summary>
        /// A queue of probe tasks that batch polls take from.
        /// </summary>
        private sealed class QueueTaskService : ITaskService
        {
            private readonly ConcurrentQueue<ConductorTask> _queue = new();
            private TaskCompletionSource<bool> _heldPoll;
            private int _polls;
            private int _largestBatchRequested;

            public List<TaskResult> Updates { get; } = [];

            public int Polls => _polls;

            public int LargestBatchRequested => _largestBatchRequested;

            public DateTime FirstPollAt { get; private set; } = DateTime.MaxValue;

            public void Enqueue(ConductorTask task) => _queue.Enqueue(task);

            /// <summary>
            /// Holds the next poll open until the returned source is set, then answers it empty.
            /// </summary>
            public TaskCompletionSource<bool> HoldNextPoll()
            {
                _heldPoll = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                return _heldPoll;
            }

            public async Task<ICollection<ConductorTask>> BatchPollAsync(
                string taskType,
                string workerId,
                string domain,
                int? count = null,
                int? timeout = null,
                CancellationToken cancellationToken = default
            )
            {
                if (Interlocked.Increment(ref _polls) == 1)
                    FirstPollAt = DateTime.UtcNow;

                var requested = count ?? 1;
                InterlockedMax(ref _largestBatchRequested, requested);

                var held = Interlocked.Exchange(ref _heldPoll, null);
                if (held != null)
                {
                    await held.Task;
                    return [];
                }

                var tasks = new List<ConductorTask>();
                while (tasks.Count < requested && _queue.TryDequeue(out var task))
                    tasks.Add(task);

                return tasks;
            }

            public Task<string> UpdateAsync(TaskResult updateRequest, CancellationToken cancellationToken = default)
            {
                lock (Updates)
                    Updates.Add(updateRequest);

                return Task.FromResult(updateRequest.TaskId);
            }

            public Task LogAsync(string taskId, string message, CancellationToken cancellationToken = default) => Task.CompletedTask;

            private static void InterlockedMax(ref int target, int value)
            {
                int current;
                while (value > (current = Volatile.Read(ref target)))
                {
                    if (Interlocked.CompareExchange(ref target, value, current) == current)
                        return;
                }
            }

            public Task<IDictionary<string, long>> ListQueuesAsync(CancellationToken cancellationToken = default) =>
                throw new NotImplementedException();

            public Task<ConductorTask> PollAsync(string taskType, string workerId, string domain, CancellationToken cancellationToken = default) =>
                throw new NotImplementedException();

            public Task<ConductorTask> GetAsync(string taskId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public Task<int> GetQueueAsync(
                string taskType,
                string domain = null,
                string isolationGroupId = null,
                string executionNamespace = null,
                CancellationToken cancellationToken = default
            ) => throw new NotImplementedException();

            public Task<ExternalStorageLocation> GetExternalStorageLocationAsync(
                string path,
                string operation,
                string payloadType,
                CancellationToken cancellationToken = default
            ) => throw new NotImplementedException();

            public Task<ICollection<TaskExecLog>> ListLogsAsync(string taskId, CancellationToken cancellationToken = default) =>
                throw new NotImplementedException();

            public Task<ICollection<PollData>> GetPollDataAsync(string taskType, CancellationToken cancellationToken = default) =>
                throw new NotImplementedException();

            public Task<ICollection<PollData>> ListPollDataAsync(CancellationToken cancellationToken = default) =>
                throw new NotImplementedException();

            public Task<IDictionary<string, IDictionary<string, IDictionary<string, long>>>> ListQueuesVerboseAsync(
                CancellationToken cancellationToken = default
            ) => throw new NotImplementedException();

            public Task<string> RequeuePendingAsync(string taskType, CancellationToken cancellationToken = default) =>
                throw new NotImplementedException();

            public Task<SearchResultTaskSummary> SearchAsync(
                int? start = null,
                int? size = null,
                string sort = null,
                string freeText = null,
                string query = null,
                CancellationToken cancellationToken = default
            ) => throw new NotImplementedException();

            public Task<SearchResultTask> SearchV2Async(
                int? start = null,
                int? size = null,
                string sort = null,
                string freeText = null,
                string query = null,
                CancellationToken cancellationToken = default
            ) => throw new NotImplementedException();

            public Task<string> UpdateTaskByRefnameAsync(
                string workflowId,
                string taskRefName,
                Status status,
                string workerid,
                IDictionary<string, object> body,
                CancellationToken cancellationToken = default
            ) => throw new NotImplementedException();
        }
    }
}
