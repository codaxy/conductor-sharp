using MediatR;

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
        public dynamic WorkflowOutput { get; set; }
        public string TerminationStatus { get; set; }
    }

    public class TerminateTaskModel : TaskModel<TerminateTaskInput, NoOutput> { }
}
