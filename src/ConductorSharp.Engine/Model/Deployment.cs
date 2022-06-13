using ConductorSharp.Client.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConductorSharp.Engine.Model
{

    public class Deployment
    {
        public List<TaskDefinition> TaskDefinitions { get; set; } = new List<TaskDefinition>();

        public List<WorkflowDefinition> WorkflowDefinitions { get; set; } =
            new List<WorkflowDefinition>();

        public List<EventHandlerDefinition> EventHandlerDefinitions { get; set; } =
            new List<EventHandlerDefinition>();

        public TaskDefinition FindTaskByName(string name) =>
            TaskDefinitions.Where(a => a.Name == name).SingleOrDefault();

        public TaskDefinition GetTaskByName(string name) =>
            FindTaskByName(name)
            ?? throw new Exception($"Task with name {name} not registered in current deployment");

        public WorkflowDefinition FindWorkflowByName(string name) =>
            WorkflowDefinitions.Where(a => name.Equals(a.Name)).SingleOrDefault();

        public WorkflowDefinition GetWorkflowByName(string name) =>
            FindWorkflowByName(name)
            ?? throw new Exception(
                $"Workflow with name {name} not registered in current deployment"
            );
    }
}