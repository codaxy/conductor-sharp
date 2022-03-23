using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XgsPon.Workflows.Client.Interface;
using XgsPon.Workflows.Client.Model.Common;
using XgsPon.Workflows.Engine.Interface;
using XgsPon.Workflows.Engine.Model;

namespace XgsPon.Workflows.Engine.Service
{
    public class DeploymentService : IDeploymentService
    {
        private readonly IMetadataService _metadataService;
        private readonly ILogger<DeploymentService> _logger;

        public DeploymentService(
            IMetadataService metadataService,
            ILogger<DeploymentService> logger
        )
        {
            _metadataService = metadataService;
            _logger = logger;
        }

        public async Task Deploy(Deployment deployment)
        {
            _logger.LogInformation("Deploying conductor definitions");

            if (deployment.TaskDefinitions.Count > 0)
                await _metadataService.CreateTaskDefinitions(deployment.TaskDefinitions);

            _logger.LogDebug(
                "Registered {registeredTasksCount} tasks",
                deployment.TaskDefinitions.Count
            );

            if (deployment.WorkflowDefinitions.Count > 0)
                await _metadataService.CreateWorkflowDefinitions(deployment.WorkflowDefinitions);

            _logger.LogDebug(
                "Registered {registeredWorkflowsCount} workflows ",
                deployment.WorkflowDefinitions.Count
            );

            // TODO: Add registration for event handlers

            _logger.LogInformation("Finished deploying conductor definitions");
        }

        public async Task Remove(Deployment deployment)
        {
            foreach (var definition in deployment.TaskDefinitions)
            {
                var oldDefinition = await _metadataService.GetTaskDefinition(definition.Name);

                if (oldDefinition?.Name != null)
                    await _metadataService.DeleteTaskDefinition(definition.Name);
            }

            foreach (var definition in deployment.WorkflowDefinitions)
            {
                var oldDefinition = await _metadataService.GetWorkflowDefinition(
                    definition.Name,
                    definition.Version
                );

                if (oldDefinition?.Name != null)
                    await _metadataService.DeleteWorkflowDefinition(
                        definition.Name,
                        definition.Version
                    );
            }
        }
    }
}
