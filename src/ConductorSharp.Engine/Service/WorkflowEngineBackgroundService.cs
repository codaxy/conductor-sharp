using System;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Health;
using ConductorSharp.Engine.Interface;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Engine.Service
{
    internal class WorkflowEngineBackgroundService : IHostedService, IDisposable
    {
        private static readonly TimeSpan DefaultInitialRetryDelay = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan DefaultMaxRetryDelay = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan DefaultMaxJitter = TimeSpan.FromMilliseconds(500);

        private readonly ILogger<WorkflowEngineBackgroundService> _logger;
        private readonly IDeploymentService _deploymentService;
        private readonly IExecutionManager _executionManager;
        private readonly ModuleDeployment _deployment;
        private readonly IConductorSharpHealthService _healthService;
        private readonly TimeSpan _initialRetryDelay;
        private readonly TimeSpan _maxRetryDelay;
        private readonly TimeSpan _maxJitter;
        private readonly Func<TimeSpan, CancellationToken, Task> _delay;
        private readonly Func<double> _jitter;
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new();

        public WorkflowEngineBackgroundService(
            IConductorSharpHealthService healthService,
            ILogger<WorkflowEngineBackgroundService> logger,
            IDeploymentService deploymentService,
            IExecutionManager executionManager,
            ModuleDeployment deployment
        )
            : this(
                healthService,
                logger,
                deploymentService,
                executionManager,
                deployment,
                DefaultInitialRetryDelay,
                DefaultMaxRetryDelay,
                DefaultMaxJitter,
                Task.Delay,
                Random.Shared.NextDouble
            ) { }

        internal WorkflowEngineBackgroundService(
            IConductorSharpHealthService healthService,
            ILogger<WorkflowEngineBackgroundService> logger,
            IDeploymentService deploymentService,
            IExecutionManager executionManager,
            ModuleDeployment deployment,
            TimeSpan initialRetryDelay,
            TimeSpan maxRetryDelay,
            TimeSpan maxJitter,
            Func<TimeSpan, CancellationToken, Task> delay,
            Func<double> jitter
        )
        {
            _healthService = healthService;
            _logger = logger;
            _deploymentService = deploymentService;
            _executionManager = executionManager;
            _deployment = deployment;
            _initialRetryDelay = initialRetryDelay;
            _maxRetryDelay = maxRetryDelay;
            _maxJitter = maxJitter;
            _delay = delay;
            _jitter = jitter;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = RunAsync(_stoppingCts.Token);

            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Deploys the definitions and runs the execution manager until the service stops. Any failure, including a
        /// Conductor request timeout, marks the engine as not running and starts both again after a capped backoff.
        /// </summary>
        private async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                _healthService.RemoveHealthData();
                var retryDelay = _initialRetryDelay;
                var attempt = 0;

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await _deploymentService.Deploy(_deployment);
                        retryDelay = _initialRetryDelay;
                        attempt = 0;
                        await _healthService.SetExecutionManagerRunning(cancellationToken);
                        await _executionManager.StartAsync(cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception exception)
                    {
                        attempt++;
                        await _healthService.UnsetExecutionManagerRunning(CancellationToken.None);
                        var delay = retryDelay + TimeSpan.FromMilliseconds(_jitter() * _maxJitter.TotalMilliseconds);

                        _logger.LogError(
                            exception,
                            "ConductorSharp engine stopped polling (attempt {Attempt}); retrying in {RetryDelay}. API errors: {ApiErrors}",
                            attempt,
                            delay,
                            (exception as ApiException)?.Errors
                        );

                        try
                        {
                            await _delay(delay, cancellationToken);
                        }
                        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        retryDelay = TimeSpan.FromMilliseconds(Math.Min(retryDelay.TotalMilliseconds * 2, _maxRetryDelay.TotalMilliseconds));
                    }
                }

                _logger.LogInformation("Stopping ConductorSharp background service");
            }
            finally
            {
                _healthService.RemoveHealthData();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _healthService.RemoveHealthData();
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {
                _logger.LogDebug("Stopping Workflow Engine Background Service");
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        public virtual void Dispose()
        {
            _stoppingCts.Cancel();
        }
    }
}
