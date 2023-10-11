﻿using ConductorSharp.Client.Model.Common;
using ConductorSharp.Client.Model.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConductorSharp.Client.Service
{
    public interface IMetadataService
    {
        Task<TaskDefinition[]> GetAllTaskDefinitions();
        Task CreateTaskDefinitions(List<TaskDefinition> definitions);
        Task<TaskDefinition> GetTaskDefinition(string name);
        Task DeleteTaskDefinition(string name);
        Task UpdateTaskDefinition(TaskDefinition definition);
        Task<WorkflowDefinition> GetWorkflowDefinition(string name, int version);
        Task UpdateWorkflowDefinitions(List<WorkflowDefinition> workflowDefinition);
        Task DeleteWorkflowDefinition(string name, int version);
        Task ValidateWorkflowDefinition(WorkflowDefinition workflowDefinition);
        Task<WorkflowDefinition[]> GetAllWorkflowDefinitions();
        Task<Dictionary<string, List<NameAndVersion>>> GetAllWorkflowNamesAndVersions();
        Task<EventHandlerDefinition[]> GetAllEventHandlerDefinitions();
        Task UpdateEventHandlerDefinition(EventHandlerDefinition definition);
        Task DeleteEventHandlerDefinition(string name);
        Task CreateEventHandlerDefinition(EventHandlerDefinition definition);
        Task<EventHandlerDefinition> GetEventHandlerDefinition(string name);
        Task CreateWorkflowDefinitions(List<WorkflowDefinition> definition);
    }
}
