using ConductorSharp.Engine.Health;
using ConductorSharp.Engine.Interface;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Service
{
    internal class WorkflowEngineBackgroundService : IHostedService
    {
        private readonly IDeploymentService _deploymentService;
        private readonly ExecutionManager _executionManager;
        private readonly ModuleDeployment _deployment;
        private readonly IConductorSharpHealthUpdater _healthUpdater;
        private Task _executingTask;

        public WorkflowEngineBackgroundService(
            IDeploymentService deploymentService,
            ExecutionManager executionManager,
            ModuleDeployment deployment,
            IConductorSharpHealthUpdater healthUpdater
        )
        {
            _deploymentService = deploymentService;
            _executionManager = executionManager;
            _deployment = deployment;
            _healthUpdater = healthUpdater;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = RunAsync(cancellationToken);
            return Task.CompletedTask;
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            await _healthUpdater.ResetHealthData();
            await _deploymentService.Deploy(_deployment);
            await _executionManager.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _healthUpdater.RemoveHealthData().Wait();
            await _executingTask;
        }
    }
}
