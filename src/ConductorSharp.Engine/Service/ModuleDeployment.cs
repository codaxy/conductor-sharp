using System.Collections.Generic;
using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Model;

namespace ConductorSharp.Engine.Service
{
    public class ModuleDeployment : Deployment
    {
        public ModuleDeployment(
            IEnumerable<TaskDef> taskDefinitions,
            IEnumerable<WorkflowDef> workflowDefinitions,
            IEnumerable<EventHandler> eventHandlerDefinitions
        )
        {
            TaskDefinitions.AddRange(taskDefinitions);
            WorkflowDefinitions.AddRange(workflowDefinitions);
            EventHandlerDefinitions.AddRange(eventHandlerDefinitions);
        }
    }
}
