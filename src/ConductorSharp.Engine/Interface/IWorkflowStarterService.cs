using ConductorSharp.Engine.Builders;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Interface
{
    public interface IWorkflowStarterService
    {
        Task<TOutput> StartWorkflowAsync<TWorkflow, TInput, TOutput>(TInput input)
            where TWorkflow : Workflow<TWorkflow, TInput, TOutput>
            where TInput : WorkflowInput<TOutput>
            where TOutput : WorkflowOutput;

        Task<JObject> StartWorkflowAsync(string workflowName, int version, JObject input);
    }
}
