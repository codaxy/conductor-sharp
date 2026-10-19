using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Health;
using ConductorSharp.Engine.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Engine.Tests.Integration;

/// <summary>
/// A real engine (DI wiring, hosted service, beta execution manager, HTTP client) against a fake Conductor that can
/// fail, hang and recover. Retry delays are shortened; everything else is production code.
/// </summary>
[Collection(nameof(InMemoryHealthService))]
public class EngineRecoveryTests : IAsyncLifetime
{
    private const string ProbeTaskName = "recovery_probe_task";
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);

    private readonly FakeConductor _conductor = new();
    private readonly ProbeGate _gate = new();
    private ServiceProvider _provider;
    private List<IHostedService> _hostedServices = [];
    private IConductorSharpHealthService _health;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(_gate);
        services
            .AddConductorSharp(baseUrl: "http://conductor")
            .AddExecutionManager(
                maxConcurrentWorkers: 4,
                sleepInterval: 250,
                longPollInterval: 250,
                domain: null,
                typeof(EngineRecoveryTests).Assembly
            )
            .UseBetaExecutionManager()
            .SetHealthCheckService<InMemoryHealthService>();
        services.RegisterWorkerTask<RecoveryProbe.Handler>();

        services.ConfigureHttpClientDefaults(
            http => http.ConfigurePrimaryHttpMessageHandler(() => _conductor).ConfigureHttpClient(c => c.Timeout = TimeSpan.FromMilliseconds(300))
        );
        services.AddSingleton(
            sp =>
                new TaskQueuePollingService(
                    sp.GetRequiredService<ITaskService>(),
                    sp.GetRequiredService<ILogger<TaskQueuePollingService>>(),
                    sp.GetRequiredService<IConductorSharpHealthService>(),
                    maxAttempts: 2,
                    initialRetryDelay: TimeSpan.FromMilliseconds(10),
                    maxRetryDelay: TimeSpan.FromMilliseconds(50),
                    maxJitter: TimeSpan.Zero,
                    delay: Task.Delay,
                    jitter: () => 0
                )
        );
        services.RemoveAll<IHostedService>();
        services.AddTransient<IHostedService>(
            sp =>
                new WorkflowEngineBackgroundService(
                    sp.GetRequiredService<IConductorSharpHealthService>(),
                    sp.GetRequiredService<ILogger<WorkflowEngineBackgroundService>>(),
                    sp.GetRequiredService<IDeploymentService>(),
                    sp.GetRequiredService<IExecutionManager>(),
                    sp.GetRequiredService<ModuleDeployment>(),
                    initialRetryDelay: TimeSpan.FromMilliseconds(20),
                    maxRetryDelay: TimeSpan.FromMilliseconds(100),
                    maxJitter: TimeSpan.Zero,
                    delay: Task.Delay,
                    jitter: () => 0
                )
        );

        _provider = services.BuildServiceProvider();
        _health = _provider.GetRequiredService<IConductorSharpHealthService>();
        await _health.ResetHealthData();
    }

    public async Task DisposeAsync()
    {
        _gate.Open();
        foreach (var hostedService in _hostedServices)
            await hostedService.StopAsync(CancellationToken.None);
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task Outage503_EngineResumesAndPicksUpTask()
    {
        await StartEngineAsync();
        await WaitUntil(() => Task.FromResult(_conductor.QueueReads > 0), "the engine polls");

        _conductor.Mode = FakeConductor.Fail503;
        await WaitUntil(IsNotRunning, "the engine backs off (Unhealthy)");
        _conductor.Mode = FakeConductor.Ok;
        _conductor.Enqueue("after-outage");

        await WaitUntil(() => IsCompleted("after-outage"), "the task queued after the outage completes");
        Assert.True((await _health.GetHealthData()).IsExecutionManagerRunning);
    }

    [Fact]
    public async Task Hang_EngineResumesAndPicksUpTask()
    {
        await StartEngineAsync();
        await WaitUntil(() => Task.FromResult(_conductor.QueueReads > 0), "the engine polls");

        _conductor.Mode = FakeConductor.Hang;
        await WaitUntil(IsNotRunning, "the engine backs off (Unhealthy) after request timeouts");
        _conductor.Mode = FakeConductor.Ok;
        _conductor.Enqueue("after-hang");

        await WaitUntil(() => IsCompleted("after-hang"), "the task queued after the hang completes");
        Assert.True((await _health.GetHealthData()).IsExecutionManagerRunning);
    }

    [Fact]
    public async Task QueuesFailWhileMetadataWorks_EngineStaysUnhealthy()
    {
        _conductor.Mode = FakeConductor.FailTasks;
        await StartEngineAsync();
        await WaitUntil(() => Task.FromResult(_conductor.TaskDefRegistrations >= 3), "the engine has redeployed twice");

        var samples = new List<bool>();
        for (var i = 0; i < 50; i++)
        {
            samples.Add((await _health.GetHealthData()).IsExecutionManagerRunning);
            await Task.Delay(10);
        }

        Assert.DoesNotContain(true, samples);

        _conductor.Mode = FakeConductor.Ok;
        _conductor.Enqueue("after-queue-outage");
        await WaitUntil(() => IsCompleted("after-queue-outage"), "the task queued after the queue outage completes");
        Assert.True((await _health.GetHealthData()).IsExecutionManagerRunning);
    }

    [Fact]
    public async Task RegistrationRace_EngineStartsAfterRetry()
    {
        _conductor.FailNextTaskDefRegistration = true;
        await StartEngineAsync();
        _conductor.Enqueue("after-race");

        await WaitUntil(() => IsCompleted("after-race"), "the task completes after the failed registration");
        Assert.Equal(2, _conductor.TaskDefRegistrations);
    }

    [Fact]
    public async Task TaskInFlight_SurvivesLoopRestart()
    {
        await StartEngineAsync();
        _gate.Close();
        _conductor.Enqueue("in-flight");
        await WaitUntil(() => Task.FromResult(_conductor.HandedOut.Contains("in-flight")), "the engine picks up the task");

        _conductor.Mode = FakeConductor.Fail503;
        await WaitUntil(IsNotRunning, "the engine backs off (Unhealthy)");
        _conductor.Mode = FakeConductor.Ok;
        await WaitUntil(async () => !await IsNotRunning(), "the engine runs again");
        Assert.False(await IsCompleted("in-flight"));
        _gate.Open();

        await WaitUntil(() => IsCompleted("in-flight"), "the task picked up before the outage completes");
        _conductor.Enqueue("after-restart");
        await WaitUntil(() => IsCompleted("after-restart"), "a new task completes after the restart");
    }

    private async Task StartEngineAsync()
    {
        _hostedServices = _provider.GetServices<IHostedService>().ToList();
        foreach (var hostedService in _hostedServices)
            await hostedService.StartAsync(CancellationToken.None);
    }

    private async Task<bool> IsNotRunning() => !(await _health.GetHealthData()).IsExecutionManagerRunning;

    private Task<bool> IsCompleted(string taskId) => Task.FromResult(_conductor.Updates.TryGetValue(taskId, out var status) && status == "COMPLETED");

    private static async Task WaitUntil(Func<Task<bool>> condition, string what)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < WaitTimeout)
        {
            if (await condition())
                return;
            await Task.Delay(20);
        }

        Assert.Fail($"Timed out after {WaitTimeout.TotalSeconds} s waiting until {what}");
    }

    public class RecoveryProbe : IRequest<RecoveryProbe.Response>
    {
        public class Response { }

        // The gate is optional: other tests scan this assembly for handlers and validate their own containers.
        [OriginalName(ProbeTaskName)]
        public class Handler(ProbeGate gate = null) : TaskRequestHandler<RecoveryProbe, Response>
        {
            public override async Task<Response> Handle(RecoveryProbe request, CancellationToken cancellationToken)
            {
                if (gate != null)
                    await gate.WaitAsync(cancellationToken);
                return new Response();
            }
        }
    }

    /// <summary>Lets a test hold the probe handler mid-execution.</summary>
    public class ProbeGate
    {
        private volatile TaskCompletionSource _gate = Opened();

        public void Close() => _gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Open() => _gate.TrySetResult();

        public Task WaitAsync(CancellationToken cancellationToken) => _gate.Task.WaitAsync(cancellationToken);

        private static TaskCompletionSource Opened()
        {
            var gate = new TaskCompletionSource();
            gate.SetResult();
            return gate;
        }
    }

    /// <summary>
    /// Answers the Conductor endpoints the engine uses. <see cref="Mode"/> switches between a healthy Conductor, one
    /// that answers 503 to everything, and one that accepts requests but never answers (<c>docker pause</c>).
    /// </summary>
    private sealed class FakeConductor : HttpMessageHandler
    {
        public const string Ok = "ok";
        public const string Fail503 = "fail503";
        public const string Hang = "hang";
        public const string FailTasks = "failTasks"; // metadata works, every /api/tasks call answers 500

        private readonly ConcurrentQueue<string> _queued = new();
        private int _queueReads;
        private int _taskDefRegistrations;

        public volatile string Mode = Ok;
        public volatile bool FailNextTaskDefRegistration;

        public int QueueReads => Volatile.Read(ref _queueReads);
        public int TaskDefRegistrations => Volatile.Read(ref _taskDefRegistrations);
        public ConcurrentBag<string> HandedOut { get; } = [];
        public ConcurrentDictionary<string, string> Updates { get; } = new();

        public void Enqueue(string taskId) => _queued.Enqueue(taskId);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (Mode == Fail503)
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            if (Mode == Hang)
                await Task.Delay(Timeout.Infinite, cancellationToken);

            var path = request.RequestUri!.AbsolutePath;

            if (Mode == FailTasks && path.StartsWith("/api/tasks"))
                return Json(HttpStatusCode.InternalServerError, new { status = 500, message = "queue backend unavailable" });

            if (request.Method == HttpMethod.Post && path == "/api/metadata/taskdefs")
            {
                Interlocked.Increment(ref _taskDefRegistrations);
                if (!FailNextTaskDefRegistration)
                    return new HttpResponseMessage(HttpStatusCode.OK);

                FailNextTaskDefRegistration = false;
                return Json(
                    HttpStatusCode.InternalServerError,
                    new { status = 500, message = "ERROR: duplicate key value violates unique constraint \"meta_task_def_pkey\"" }
                );
            }

            if (request.Method == HttpMethod.Get && path == "/api/tasks/queue/all")
            {
                Interlocked.Increment(ref _queueReads);
                return Json(HttpStatusCode.OK, new Dictionary<string, long> { [ProbeTaskName] = _queued.Count });
            }

            if (request.Method == HttpMethod.Get && path == $"/api/tasks/poll/{ProbeTaskName}")
            {
                if (!_queued.TryDequeue(out var taskId))
                    return new HttpResponseMessage(HttpStatusCode.NoContent);

                HandedOut.Add(taskId);
                return Json(
                    HttpStatusCode.OK,
                    new
                    {
                        taskId,
                        taskDefName = ProbeTaskName,
                        taskType = ProbeTaskName,
                        referenceTaskName = "probe",
                        workflowInstanceId = $"workflow-{taskId}",
                        workflowType = "recovery_probe_workflow",
                        status = "IN_PROGRESS",
                        inputData = new { }
                    }
                );
            }

            if (request.Method == HttpMethod.Post && path == "/api/tasks")
            {
                var result = JObject.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
                var taskId = (string)result["taskId"];
                Updates[taskId] = (string)result["status"];
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(taskId) };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        private static HttpResponseMessage Json(HttpStatusCode statusCode, object body) =>
            new(statusCode) { Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json") };
    }
}
