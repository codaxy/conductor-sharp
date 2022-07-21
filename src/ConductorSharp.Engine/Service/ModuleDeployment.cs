using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Model;
using System.Collections.Generic;

namespace ConductorSharp.Engine.Service
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
