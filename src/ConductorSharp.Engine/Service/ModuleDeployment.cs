using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Model;
using System.Collections.Generic;

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
