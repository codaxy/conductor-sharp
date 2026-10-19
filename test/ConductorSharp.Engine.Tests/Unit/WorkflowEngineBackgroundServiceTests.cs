using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Health;
using ConductorSharp.Engine.Service;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Engine.Tests.Unit;

[Collection(nameof(InMemoryHealthService))] // InMemoryHealthService keeps its flag in a static field
public class WorkflowEngineBackgroundServiceTests
{
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

    private readonly InMemoryHealthService _healthService = new();
    private readonly List<TimeSpan> _delays = [];
    private readonly ListLogger _logger = new();

    public WorkflowEngineBackgroundServiceTests() => _healthService.ResetHealthData().Wait();

    [Fact]
    public async Task DeployApiError_IsRetriedAndEngineStarts()
    {
        var deployment = new ScriptedDeploymentService(Api(500), Api(409));
        var executionManager = new ScriptedExecutionManager();
        var service = CreateService(deployment, executionManager, RecordDelay);

        await service.StartAsync(CancellationToken.None);
        await executionManager.Running.Task.WaitAsync(WaitTimeout);

        Assert.Equal(3, deployment.Calls);
        Assert.Equal(1, executionManager.Calls);
        Assert.Equal(new[] { 1, 2 }.Select(s => TimeSpan.FromSeconds(s)), _delays);
        Assert.True((await _healthService.GetHealthData()).IsExecutionManagerRunning);

        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ConnectionError_WhileRunning_RestartsEngine()
    {
        var deployment = new ScriptedDeploymentService();
        var executionManager = new ScriptedExecutionManager(new HttpRequestException("Connection refused"));
        var service = CreateService(deployment, executionManager, RecordDelay);

        await service.StartAsync(CancellationToken.None);
        await executionManager.Running.Task.WaitAsync(WaitTimeout);

        Assert.Equal(2, deployment.Calls);
        Assert.Equal(2, executionManager.Calls);
        Assert.True((await _healthService.GetHealthData()).IsExecutionManagerRunning);

        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task HttpTimeout_IsAnErrorNotAShutdown()
    {
        var deployment = new ScriptedDeploymentService();
        var executionManager = new ScriptedExecutionManager(new TaskCanceledException("timeout", new TimeoutException()));
        var service = CreateService(deployment, executionManager, RecordDelay);

        await service.StartAsync(CancellationToken.None);
        await executionManager.Running.Task.WaitAsync(WaitTimeout);

        Assert.Equal(2, executionManager.Calls);
        Assert.Single(_delays);
        Assert.Contains(_logger.Entries, e => e.Level == LogLevel.Error);

        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task HealthIsNotRunning_WhileBackingOff()
    {
        var deployment = new ScriptedDeploymentService();
        var executionManager = new ScriptedExecutionManager(new HttpRequestException("Connection refused"));
        var backingOff = new TaskCompletionSource();
        var resume = new TaskCompletionSource();
        var service = CreateService(
            deployment,
            executionManager,
            (delay, cancellationToken) =>
            {
                backingOff.TrySetResult();
                return resume.Task.WaitAsync(cancellationToken);
            }
        );

        await service.StartAsync(CancellationToken.None);
        await backingOff.Task.WaitAsync(WaitTimeout);

        Assert.False((await _healthService.GetHealthData()).IsExecutionManagerRunning);

        resume.SetResult();
        await executionManager.Running.Task.WaitAsync(WaitTimeout);

        Assert.True((await _healthService.GetHealthData()).IsExecutionManagerRunning);

        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task NonTransientDeployError_IsRetriedWithCappedBackoff()
    {
        var deployment = new ScriptedDeploymentService(Enumerable.Range(0, 8).Select(_ => Api(401)).ToArray());
        var executionManager = new ScriptedExecutionManager();
        var service = CreateService(deployment, executionManager, RecordDelay);

        await service.StartAsync(CancellationToken.None);
        await executionManager.Running.Task.WaitAsync(WaitTimeout);

        Assert.Equal(new[] { 1, 2, 4, 8, 16, 30, 30, 30 }.Select(s => TimeSpan.FromSeconds(s)), _delays);

        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task SuccessfulDeploy_ResetsBackoff()
    {
        var deployment = new ScriptedDeploymentService(Api(500), Api(500), Api(500));
        var executionManager = new ScriptedExecutionManager(new HttpRequestException("Connection refused"));
        var service = CreateService(deployment, executionManager, RecordDelay);

        await service.StartAsync(CancellationToken.None);
        await executionManager.Running.Task.WaitAsync(WaitTimeout);

        Assert.Equal(new[] { 1, 2, 4, 1 }.Select(s => TimeSpan.FromSeconds(s)), _delays);

        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Stop_DuringBackoff_ExitsWithoutRetry()
    {
        var deployment = new ScriptedDeploymentService(Api(500));
        var executionManager = new ScriptedExecutionManager();
        var backingOff = new TaskCompletionSource();
        var service = CreateService(
            deployment,
            executionManager,
            (delay, cancellationToken) =>
            {
                backingOff.TrySetResult();
                return Task.Delay(Timeout.Infinite, cancellationToken);
            }
        );

        await service.StartAsync(CancellationToken.None);
        await backingOff.Task.WaitAsync(WaitTimeout);
        await service.StopAsync(CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(1));

        Assert.Equal(1, deployment.Calls);
        Assert.Equal(0, executionManager.Calls);
        Assert.Contains(_logger.Entries, e => e.Message == "Stopping ConductorSharp background service");
    }

    [Fact]
    public async Task Stop_WhileRunning_DoesNotRestart()
    {
        var deployment = new ScriptedDeploymentService();
        var executionManager = new ScriptedExecutionManager();
        var service = CreateService(deployment, executionManager, RecordDelay);

        await service.StartAsync(CancellationToken.None);
        await executionManager.Running.Task.WaitAsync(WaitTimeout);
        await service.StopAsync(CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(1));

        Assert.Equal(1, executionManager.Calls);
        Assert.Empty(_delays);
        Assert.DoesNotContain(_logger.Entries, e => e.Level >= LogLevel.Error);
    }

    private Task RecordDelay(TimeSpan delay, CancellationToken cancellationToken)
    {
        _delays.Add(delay);
        return Task.CompletedTask;
    }

    private WorkflowEngineBackgroundService CreateService(
        IDeploymentService deploymentService,
        IExecutionManager executionManager,
        Func<TimeSpan, CancellationToken, Task> delay
    ) =>
        new(
            _healthService,
            _logger,
            deploymentService,
            executionManager,
            new ModuleDeployment([], [], []),
            initialRetryDelay: TimeSpan.FromSeconds(1),
            maxRetryDelay: TimeSpan.FromSeconds(30),
            maxJitter: TimeSpan.FromMilliseconds(500),
            delay: delay,
            jitter: () => 0
        );

    private static ApiException Api(int statusCode) =>
        new($"HTTP {statusCode}", statusCode, "{}", new Dictionary<string, IEnumerable<string>>(), null);

    private sealed class ScriptedDeploymentService(params Exception[] failures) : IDeploymentService
    {
        private readonly Queue<Exception> _failures = new(failures);

        public int Calls { get; private set; }

        public Task Deploy(Deployment deployment)
        {
            Calls++;
            return _failures.TryDequeue(out var failure) ? Task.FromException(failure) : Task.CompletedTask;
        }

        public Task Remove(Deployment deployment) => Task.CompletedTask;
    }

    /// <summary>Throws the queued failures, one per call; once they run out, polls (blocks) until cancelled.</summary>
    private sealed class ScriptedExecutionManager(params Exception[] failures) : IExecutionManager
    {
        private readonly Queue<Exception> _failures = new(failures);

        public int Calls { get; private set; }

        public TaskCompletionSource Running { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Calls++;
            if (_failures.TryDequeue(out var failure))
                throw failure;

            Running.TrySetResult();
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
    }

    private sealed class ListLogger : ILogger<WorkflowEngineBackgroundService>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            lock (Entries)
                Entries.Add((logLevel, formatter(state, exception)));
        }
    }
}
