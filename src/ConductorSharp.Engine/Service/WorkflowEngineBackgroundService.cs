using ConductorSharp.Engine.Interface;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Service;

public class WorkflowEngineBackgroundService : IHostedService
{
    private readonly IDeploymentService _deploymentService;
    private readonly ExecutionManager _executionManager;
    private readonly ModuleDeployment _deployment;
    private Task _executingTask;

    public WorkflowEngineBackgroundService(
        IDeploymentService deploymentService,
        ExecutionManager executionManager,
        ModuleDeployment deployment
    )
    {
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
        await _deploymentService.Deploy(_deployment);
        await _executionManager.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken) => await _executingTask;
}
