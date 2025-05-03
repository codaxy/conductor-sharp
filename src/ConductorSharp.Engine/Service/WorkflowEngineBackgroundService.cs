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
    internal class WorkflowEngineBackgroundService(
        IConductorSharpHealthService healthService,
        ILogger<WorkflowEngineBackgroundService> logger,
        IDeploymentService deploymentService,
        IExecutionManager executionManager,
        ModuleDeployment deployment
    ) : IHostedService, IDisposable
    {
        private readonly ILogger<WorkflowEngineBackgroundService> _logger = logger;
        private readonly IDeploymentService _deploymentService = deploymentService;
        private readonly IExecutionManager _executionManager = executionManager;
        private readonly ModuleDeployment _deployment = deployment;
        private readonly IConductorSharpHealthService _healthService = healthService;
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = RunAsync(_stoppingCts.Token);

            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            return Task.CompletedTask;
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                _healthService.RemoveHealthData();
                await _deploymentService.Deploy(_deployment);
                await _healthService.SetExecutionManagerRunning(cancellationToken);
                await _executionManager.StartAsync(cancellationToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Stopping ConductorSharp background service");
            }
            catch (ApiException exception)
            {
                await _healthService.UnsetExecutionManagerRunning(cancellationToken);
                _logger.LogCritical(exception, "Workflow Engine Background Service encountered an API error(s): {apiErrors}", exception.Errors);
                throw;
            }
            catch (Exception exception)
            {
                await _healthService.UnsetExecutionManagerRunning(cancellationToken);
                _logger.LogCritical(exception, "Workflow Engine Background Service encountered an error");
                throw;
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
