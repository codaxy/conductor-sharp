using MediatR;
using Newtonsoft.Json;

namespace ConductorSharp.Engine.Model
{
    // TODO: Replace this with enum
    public static class TerminationStatus
    {
        public const string Completed = "COMPLETED";
        public const string Failed = "FAILED";
    }

    public class TerminateTaskInput : IRequest<NoOutput>
    {
        [JsonProperty("workflowOutput")]
        public dynamic WorkflowOutput { get; set; }

        [JsonProperty("terminationStatus")]
        public string TerminationStatus { get; set; }
    }

    public class TerminateTaskModel : TaskModel<TerminateTaskInput, NoOutput> { }
}
