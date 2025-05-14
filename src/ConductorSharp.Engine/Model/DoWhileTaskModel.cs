using ConductorSharp.Engine.Builders;
using MediatR;

namespace ConductorSharp.Engine.Model
{
    /// <summary>
    /// Input for configuration of the DO_WHILE task.
    /// </summary>
    public class DoWhileInput : IRequest<NoOutput>, IWorkflowInput
    {
        public object Value { get; set; } = null;
    }

    /// <summary>
    /// Task Model to reference the do while task in the workflow builders
    /// </summary>
    public class DoWhileTaskModel : TaskModel<DoWhileInput, NoOutput> { }
}
