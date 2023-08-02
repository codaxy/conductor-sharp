using ConductorSharp.Client.Model.Request;
using ConductorSharp.Client.Model.Response;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace ConductorSharp.Client.Service
{
    public interface IWorkflowService
    {
        Task<WorkflowDescriptorResponse> QueueWorkflow(string workflowName, int version, JObject input);
        Task<TResponse> QueueWorkflow<TResponse>(string workflowName, int version, JObject input);
        Task TestWorkflow(string workflowName, int version, JObject input);
        Task<string> QueueWorkflowStringResponse(string workflowName, int version, JObject input);
        Task<WorkflowStatusResponse> GetWorkflowStatus(string workflowId, bool includeTasks = true);
        Task<WorkflowSearchResponse> SearchWorkflows(WorkflowSearchRequest request);
    }
}
