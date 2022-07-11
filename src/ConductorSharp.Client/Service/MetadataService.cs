using ConductorSharp.Client.Model.Common;
using ConductorSharp.Client.Util;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConductorSharp.Client.Service
{
    public class MetadataService : IMetadataService
    {
        private readonly RestConfig _restConfig;
        private readonly IConductorClient _conductorClient;

        public MetadataService(IConductorClient client, IOptions<RestConfig> restConfig)
        {
            _restConfig = restConfig.Value;
            _conductorClient = client;
        }

        public async Task<TaskDefinition[]> GetAllTaskDefinitions() =>
            (
                await _conductorClient.ExecuteRequestAsync<TaskDefinition[]>(
                    ApiUrls.GetAllTaskDefinitions(),
                    HttpMethod.Get
                )
            );

        public async Task CreateTaskDefinitions(List<TaskDefinition> definitions) =>
            await _conductorClient.ExecuteRequestAsync(
                ApiUrls.CreateTaskDefinitions(),
                HttpMethod.Post,
                definitions
            );

        public async Task<TaskDefinition> GetTaskDefinition(string name) =>
            await _conductorClient.ExecuteRequestAsync<TaskDefinition>(
                ApiUrls.GetTaskDefinition(name),
                HttpMethod.Get
            );

        public async Task DeleteTaskDefinition(string name) =>
            await _conductorClient.ExecuteRequestAsync(
                ApiUrls.DeleteTaskDefinition(name),
                HttpMethod.Delete
            );

        public async Task UpdateTaskDefinition(TaskDefinition definition) =>
            await _conductorClient.ExecuteRequestAsync(
                ApiUrls.UpdateTaskDefinition(),
                HttpMethod.Put,
                definition
            );

        public async Task<WorkflowDefinition> GetWorkflowDefinition(string name, int version) =>
            await _conductorClient.ExecuteRequestAsync<WorkflowDefinition>(
                ApiUrls.GetWorkflowDefinition(name, version),
                HttpMethod.Get
            );

        public async Task UpdateWorkflowDefinition(WorkflowDefinition definition) =>
            await _conductorClient.ExecuteRequestAsync(
                ApiUrls.UpdateWorkflowDefinition(),
                HttpMethod.Put,
                definition
            );

        public async Task DeleteWorkflowDefinition(string name, int version) =>
            await _conductorClient.ExecuteRequestAsync(
                ApiUrls.DeleteWorkflowDefinition(name, version),
                HttpMethod.Delete
            );

        public async Task CreateWorkflowDefinitions(List<WorkflowDefinition> workflowDefinition) =>
            await _conductorClient.ExecuteRequestAsync(
                ApiUrls.CreateWorkflowDefinitions(),
                HttpMethod.Put,
                workflowDefinition
            );

        public async Task<WorkflowDefinition[]> GetAllWorkflowDefinitions() =>
            (
                await _conductorClient.ExecuteRequestAsync<WorkflowDefinition[]>(
                    ApiUrls.GetAlleWorkflowDefinitions(),
                    HttpMethod.Get
                )
            );
        public async Task<EventHandlerDefinition[]> GetAllEventHandlerDefinitions() =>
            await _conductorClient.ExecuteRequestAsync<EventHandlerDefinition[]>(
                ApiUrls.GetAllEventDefinitions(),
                HttpMethod.Get
            );

        public async Task UpdateEventHandlerDefinition(EventHandlerDefinition definition) =>
            await _conductorClient.ExecuteRequestAsync(
                ApiUrls.UpdateEventHandlerDefinition(),
                HttpMethod.Put,
                definition
            );

        public async Task DeleteEventHandlerDefinition(string name) =>
            await _conductorClient.ExecuteRequestAsync(
                ApiUrls.DeleteEventHandlerDefinition(name),
                HttpMethod.Delete
            );

        public async Task CreateEventHandlerDefinition(EventHandlerDefinition definition) =>
            await _conductorClient.ExecuteRequestAsync(
                ApiUrls.CreateEventHandlerDefinition(),
                HttpMethod.Post,
                definition
            );

        public async Task<EventHandlerDefinition> GetEventHandlerDefinition(string name) =>
            await _conductorClient.ExecuteRequestAsync<EventHandlerDefinition>(
                ApiUrls.GetEventHandlerDefinition(name),
                HttpMethod.Get
            );
    }
}
