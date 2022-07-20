using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Builders
{
    public class WorkflowInput<T> : IWorkflowInput where T : WorkflowOutput { }

    public class WorkflowOutput { }

    public class WorkflowId { }

    public interface IWorkflowInput { }

    public abstract class Workflow<TInput, TOutput> : ITypedWorkflow
        where TInput : WorkflowInput<TOutput>
        where TOutput : WorkflowOutput
    {
        public TInput WorkflowInput { get; set; }
        public TOutput WorkflowOutput { get; set; }
        public WorkflowId Id { get; set; }

        public abstract WorkflowDefinition GetDefinition();
    }
}
