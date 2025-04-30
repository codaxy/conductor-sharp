using ConductorSharp.Engine.Builders;
using MediatR;

namespace ConductorSharp.Engine.Model
{
    /// <summary>
    /// Input for configuration of the DO_WHILE task.
    /// </summary>
    public class DoWhileInput : IRequest<NoOutput>, IWorkflowInput
    {
        /// <summary>
        /// Condition of the loop in JavaScript format.
        /// Example: "$.do_while_ref['' + $.do_while_ref.iteration].number_adder_sub_workflow.success == false"
        /// To access the latest iteration, use the following syntax: TaskRef.iteration
        /// </summary>
        public string LoopCondition { get; set; }
    }

    /// <summary>
    /// Task Model to reference the do while task in the workflow builders
    /// </summary>
    public class DoWhileTaskModel : TaskModel<DoWhileInput, NoOutput> { }
}
