using ConductorSharp.Engine.Health;
using ConductorSharp.Engine.Interface;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Service
{
    internal class WorkflowEngineBackgroundService : IHostedService, IDisposable
    {
        private readonly ILogger<WorkflowEngineBackgroundService> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IDeploymentService _deploymentService;
        private readonly ExecutionManager _executionManager;
        private readonly ModuleDeployment _deployment;
        private readonly IConductorSharpHealthUpdater _healthUpdater;
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new();

        public WorkflowEngineBackgroundService(
            IConductorSharpHealthUpdater healthUpdater
            ILogger<WorkflowEngineBackgroundService> logger,
            IHostApplicationLifetime hostApplicationLifetime,
            IDeploymentService deploymentService,
            ExecutionManager executionManager,
            ModuleDeployment deployment
        )
        {
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
            _deploymentService = deploymentService;
            _executionManager = executionManager;
            _deployment = deployment;
            _healthUpdater = healthUpdater;
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

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _healthUpdater.ResetHealthData();
                await _deploymentService.Deploy(_deployment);
                await _executionManager.StartAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Workflow Engine Background Service encountered an error");
                throw;
            }
            finally
            {
                _hostApplicationLifetime.StopApplication();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _healthUpdater.RemoveHealthData().Wait();
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
