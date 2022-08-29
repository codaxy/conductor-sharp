using ConductorSharp.Engine.Util;
using MediatR;
using Newtonsoft.Json;

namespace ConductorSharp.Engine.Model
{
    public enum TerminationStatus
    {
        [EnumValue("COMPLETED")]
        Completed,

        [EnumValue("FAILED")]
        Failed
    };

    public class TerminateTaskInput : IRequest<NoOutput>
    {
        [JsonProperty("workflowOutput")]
        public dynamic WorkflowOutput { get; set; }

        [JsonProperty("terminationStatus")]
        public TerminationStatus TerminationStatus { get; set; }
    }

    public class TerminateTaskModel : TaskModel<TerminateTaskInput, NoOutput> { }
}
