using ConductorSharp.Engine.Interface;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Service
{
    public class WorkflowEngineBackgroundService : IHostedService
    {
        private readonly ILogger<WorkflowEngineBackgroundService> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IDeploymentService _deploymentService;
        private readonly ExecutionManager _executionManager;
        private readonly ModuleDeployment _deployment;

        public WorkflowEngineBackgroundService(
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
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = RunAsync(cancellationToken);
            return Task.CompletedTask;
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
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

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Stopping Workflow Engine Background Service");
            return Task.CompletedTask;
        }
    }
}
