using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using ConductorSharp.Client.Util;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ConductorTask = ConductorSharp.Client.Generated.Task;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Engine.Tests.Unit
{
    /// <summary>
    /// Maestro sends a workflow run's execution context with every polled task. A handler reads it
    /// from <see cref="ConductorSharpExecutionContext.Context"/>.
    /// </summary>
    public class ExecutionContextTests
    {
        private const string ProbeTaskName = "CONTEXT_probe";

        // The settings ConductorSharp's generated HTTP client serializes requests with.
        private static readonly JsonSerializerSettings WireJson = new() { Converters = { new JsonElementConverter(), new JsonNodeConverter() } };

        [Fact]
        public void Polled_task_reads_the_context()
        {
            var task = JsonConvert.DeserializeObject<ConductorTask>("""{"taskId":"t1","context":{"workspaceId":"42","userId":"7"}}""", WireJson);

            Assert.Equal("42", task.Context["workspaceId"]);
            Assert.Equal("7", task.Context["userId"]);
            Assert.False(task.AdditionalProperties.ContainsKey("context"));
        }

        [Fact]
        public void Polled_task_from_conductor_has_no_context()
        {
            var task = JsonConvert.DeserializeObject<ConductorTask>("""{"taskId":"t1"}""", WireJson);

            Assert.Null(task.Context);
        }

        /// <summary>
        /// A start against Conductor must not change, so a request with no context leaves the field out.
        /// </summary>
        [Fact]
        public void Start_request_without_context_omits_the_field()
        {
            var json = JsonConvert.SerializeObject(new StartWorkflowRequest { Name = "wf" }, WireJson);

            Assert.DoesNotContain("context", json);
        }

        [Fact]
        public void Start_request_carries_the_context()
        {
            var request = new StartWorkflowRequest
            {
                Name = "wf",
                Context = new Dictionary<string, string> { ["workspaceId"] = "42" },
            };

            var json = JsonConvert.SerializeObject(request, WireJson);

            Assert.Contains("\"context\":{\"workspaceId\":\"42\"}", json);
        }

        [Fact]
        public async Task Handler_sees_the_context_of_the_polled_task()
        {
            var taskService = new ScriptedTaskService(
                new ConductorTask
                {
                    TaskId = "task-1",
                    TaskDefName = ProbeTaskName,
                    WorkflowInstanceId = "workflow-1",
                    InputData = new Dictionary<string, object>(),
                    Context = new Dictionary<string, string> { ["workspaceId"] = "42" },
                }
            );

            await RunUntilAsync(taskService, () => taskService.Updates.Count > 0);

            var update = Assert.Single(taskService.Updates);
            Assert.Equal(TaskResultStatus.COMPLETED, update.Status);
            Assert.Equal("42", update.OutputData["workspace_id"]?.ToString());
        }

        /// <summary>
        /// A transport may answer an empty poll with null rather than a 204. The manager skips it and
        /// keeps polling.
        /// </summary>
        /// <remarks>
        /// This pins the behaviour, but it cannot catch its loss. Without the null check the poll
        /// throws inside a task the manager never awaits, so the failure is silent and the manager
        /// keeps polling either way.
        /// </remarks>
        [Fact]
        public async Task Null_poll_response_is_skipped()
        {
            var taskService = new ScriptedTaskService(null);

            await RunUntilAsync(taskService, () => taskService.Polls >= 2);

            Assert.Empty(taskService.Updates);
        }

        private static async Task RunUntilAsync(ScriptedTaskService taskService, Func<bool> condition)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services
                .AddConductorSharp(baseUrl: "http://empty/empty")
                .AddExecutionManager(
                    maxConcurrentWorkers: 1,
                    sleepInterval: 250,
                    longPollInterval: 500,
                    domain: null,
                    handlerAssemblies: typeof(ExecutionContextTests).Assembly
                );
            services.RegisterWorkerTask<ContextProbeHandler>();
            services.RemoveAll<ITaskService>();
            services.AddSingleton<ITaskService>(taskService);

            await using var provider = services.BuildServiceProvider();
            var manager = provider.GetRequiredService<IExecutionManager>();

            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var run = manager.StartAsync(cancellation.Token);

            while (!condition())
            {
                Assert.False(cancellation.IsCancellationRequested, "The execution manager did not get there in time");
                await Task.Delay(25);
            }

            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => run);
        }

        public class ContextProbeRequest : IRequest<ContextProbeResponse> { }

        public class ContextProbeResponse
        {
            public string WorkspaceId { get; set; }
        }

        [OriginalName(ProbeTaskName)]
        public class ContextProbeHandler(ConductorSharpExecutionContext context) : ITaskRequestHandler<ContextProbeRequest, ContextProbeResponse>
        {
            public Task<ContextProbeResponse> Handle(ContextProbeRequest request, CancellationToken cancellationToken)
            {
                context.Context.TryGetValue("workspaceId", out var workspaceId);

                return Task.FromResult(new ContextProbeResponse { WorkspaceId = workspaceId });
            }
        }

        /// <summary>
        /// Reports one queued probe task and answers every poll with the same scripted response.
        /// </summary>
        private sealed class ScriptedTaskService(ConductorTask pollResponse) : ITaskService
        {
            private int _polls;

            public List<TaskResult> Updates { get; } = [];

            public int Polls => _polls;

            public Task<IDictionary<string, long>> ListQueuesAsync(CancellationToken cancellationToken = default) =>
                Task.FromResult<IDictionary<string, long>>(new Dictionary<string, long> { [ProbeTaskName] = 1 });

            public Task<ConductorTask> PollAsync(string taskType, string workerId, string domain, CancellationToken cancellationToken = default)
            {
                Interlocked.Increment(ref _polls);

                // Handed out once, as a real queue would.
                var response = Updates.Count == 0 ? pollResponse : null;

                return Task.FromResult(response);
            }

            public Task<string> UpdateAsync(TaskResult updateRequest, CancellationToken cancellationToken = default)
            {
                lock (Updates)
                {
                    Updates.Add(updateRequest);
                }

                return Task.FromResult(updateRequest.TaskId);
            }

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

            public Task LogAsync(string taskId, string message, CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task<ICollection<ConductorTask>> BatchPollAsync(
                string taskType,
                string workerId,
                string domain,
                int? count = null,
                int? timeout = null,
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
