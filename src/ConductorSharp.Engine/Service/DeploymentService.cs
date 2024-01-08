using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Service
{
    internal class DeploymentService(IMetadataService metadataService, ILogger<DeploymentService> logger) : IDeploymentService
    {
        private readonly IMetadataService _metadataService = metadataService;

        private readonly ILogger<DeploymentService> _logger = logger;

        public async Task Deploy(Deployment deployment)
        {
            _logger.LogInformation("Deploying conductor definitions");

            if (deployment.TaskDefinitions.Count > 0)
                await _metadataService.RegisterTasksAsync(deployment.TaskDefinitions);

            _logger.LogDebug("Registered {registeredTasksCount} tasks", deployment.TaskDefinitions.Count);

            if (deployment.WorkflowDefinitions.Count > 0)
                await _metadataService.UpdateWorkflowsAsync(deployment.WorkflowDefinitions);

            _logger.LogDebug("Registered {registeredWorkflowsCount} workflows ", deployment.WorkflowDefinitions.Count);

            // TODO: Add registration for event handlers

            _logger.LogInformation(
                "Finished deploying conductor definitions ({registeredTasksCount} tasks, {registeredWorkflowsCount} workflows)",
                deployment.TaskDefinitions.Count,
                deployment.WorkflowDefinitions.Count
            );
        }

        public async Task Remove(Deployment deployment)
        {
            foreach (var definition in deployment.TaskDefinitions)
            {
                var oldDefinition = await _metadataService.GetTaskDefAsync(definition.Name);

                if (oldDefinition?.Name != null)
                    await _metadataService.DeleteTaskDefAsync(definition.Name);
            }

            foreach (var definition in deployment.WorkflowDefinitions)
            {
                var oldDefinition = await _metadataService.GetWorkflowAsync(definition.Name, definition.Version);

                if (oldDefinition?.Name != null)
                    await _metadataService.DeleteWorkflowAsync(definition.Name, definition.Version);
            }
        }
    }
}
