using System;
using System.Collections.Generic;
using System.Linq;
using ConductorSharp.Client.Generated;
using EventHandler = ConductorSharp.Client.Generated.EventHandler;

namespace ConductorSharp.Engine.Model
{
    public class Deployment
    {
        public List<TaskDef> TaskDefinitions { get; set; } = [];

        public List<WorkflowDef> WorkflowDefinitions { get; set; } = [];

        public List<EventHandler> EventHandlerDefinitions { get; set; } = [];

        public TaskDef FindTaskByName(string name) => TaskDefinitions.Where(a => a.Name == name).SingleOrDefault();

        public TaskDef GetTaskByName(string name) =>
            FindTaskByName(name) ?? throw new Exception($"Task with name {name} not registered in current deployment");

        public WorkflowDef FindWorkflowByName(string name) => WorkflowDefinitions.Where(a => name.Equals(a.Name)).SingleOrDefault();

        public WorkflowDef GetWorkflowByName(string name) =>
            FindWorkflowByName(name) ?? throw new Exception($"Workflow with name {name} not registered in current deployment");
    }
}
