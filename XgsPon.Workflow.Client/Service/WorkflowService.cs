using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using XgsPon.Workflows.Client.Interface;
using XgsPon.Workflows.Client.Model.Request;
using XgsPon.Workflows.Client.Model.Response;
using XgsPon.Workflows.Client.Util;

namespace XgsPon.Workflows.Client.Service
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IConductorClient _client;
        public WorkflowService(IConductorClient client) => _client = client;

        public async Task<WorkflowDescriptorResponse> QueueWorkflow(
            string workflowName,
            int version,
            JObject input
        ) =>
            await _client.ExecuteRequestAsync<WorkflowDescriptorResponse>(
                ApiUrls.QueueWorkflow(),
                HttpMethod.Post,
                new QueueWorkflowRequest { Input = input, Name = workflowName, Version = version },
                new Dictionary<string, string> { { "Accept", "*/*" } }
            );

        public async Task<JObject> GetWorkflowStatus(string workflowId) =>
            await _client.ExecuteRequestAsync<JObject>(
                ApiUrls.GetWorkflowStatus(workflowId),
                HttpMethod.Get
            );

        public async Task<TResponse> QueueWorkflow<TResponse>(
            string workflowName,
            int version,
            JObject input
        ) =>
            await _client.ExecuteRequestAsync<TResponse>(
                ApiUrls.QueueWorkflow(),
                HttpMethod.Post,
                new QueueWorkflowRequest { Input = input, Name = workflowName, Version = version },
                // Endpoint return 406 Not Acceptable if this header is not set to given value
                new Dictionary<string, string> { { "Accept", "*/*" } }
            );

        public async Task<string> QueueWorkflowStringResponse(
            string workflowName,
            int version,
            JObject input
        ) =>
            await _client.ExecuteRequestAsync(
                ApiUrls.QueueWorkflow(),
                HttpMethod.Post,
                new QueueWorkflowRequest { Input = input, Name = workflowName, Version = version },
                // Endpoint return 406 Not Acceptable if this header is not set to given value
                new Dictionary<string, string> { { "Accept", "*/*" } }
            );
    }
}
