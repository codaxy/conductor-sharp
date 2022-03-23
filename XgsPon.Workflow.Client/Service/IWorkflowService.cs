using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using XgsPon.Workflows.Client.Model.Response;

namespace XgsPon.Workflows.Client.Interface
{
    public interface IWorkflowService
    {
        Task<WorkflowDescriptorResponse> QueueWorkflow(
            string workflowName,
            int version,
            JObject input
        );
        Task<TResponse> QueueWorkflow<TResponse>(string workflowName, int version, JObject input);
        Task<string> QueueWorkflowStringResponse(string workflowName, int version, JObject input);
        Task<JObject> GetWorkflowStatus(string workflowId);
    }
}
