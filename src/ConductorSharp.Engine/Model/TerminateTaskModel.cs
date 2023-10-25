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
        [PropertyName("workflowOutput")]
        public object WorkflowOutput { get; set; }

        [PropertyName("terminationStatus")]
        public TerminationStatus TerminationStatus { get; set; }
    }

    public class TerminateTaskModel : TaskModel<TerminateTaskInput, NoOutput> { }
}
