using System.Collections.Generic;
using XgsPon.Workflows.Client.Model.Common;
using XgsPon.Workflows.Engine.Model;

namespace XgsPon.Workflows.Engine.Service
{
    public class ModuleDeployment : Deployment
    {
        public ModuleDeployment(
            IEnumerable<TaskDefinition> taskDefinitions,
            IEnumerable<WorkflowDefinition> workflowDefinitions,
            IEnumerable<EventHandlerDefinition> eventHandlerDefinitions
        )
        {
            TaskDefinitions.AddRange(taskDefinitions);
            WorkflowDefinitions.AddRange(workflowDefinitions);
            EventHandlerDefinitions.AddRange(eventHandlerDefinitions);
        }
    }
}
