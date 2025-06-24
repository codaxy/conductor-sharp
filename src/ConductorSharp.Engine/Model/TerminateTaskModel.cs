using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
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

    public class TerminateTaskInput : ITaskInput<NoOutput>
    {
        [JsonProperty("workflowOutput")]
        public object WorkflowOutput { get; set; }

        [JsonProperty("terminationStatus")]
        public TerminationStatus TerminationStatus { get; set; }
    }

    public class TerminateTaskModel : TaskModel<TerminateTaskInput, NoOutput>;
}
