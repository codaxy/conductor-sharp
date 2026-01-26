using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Model
{
    /// <summary>
    /// Input for configuration of the DO_WHILE task.
    /// </summary>
    public class DoWhileInput : ITaskInput<NoOutput>, IWorkflowInput
    {
        public object Value { get; set; } = null;
    }

    /// <summary>
    /// Task Model to reference the do while task in the workflow builders
    /// </summary>
    public class DoWhileTaskModel : TaskModel<DoWhileInput, NoOutput>;
}
