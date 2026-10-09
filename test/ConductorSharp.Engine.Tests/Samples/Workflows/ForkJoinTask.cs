using ConductorSharp.Client.Generated;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public sealed class ForkJoinTaskInput : WorkflowInput<ForkJoinTaskOutput> { }

    public sealed class ForkJoinTaskOutput : WorkflowOutput { }

    public sealed class ForkJoinTask : Workflow<ForkJoinTask, ForkJoinTaskInput, ForkJoinTaskOutput>
    {
        public ForkJoinTaskModel ForkJoin { get; set; }

        public ForkJoinTask(WorkflowDefinitionBuilder<ForkJoinTask, ForkJoinTaskInput, ForkJoinTaskOutput> builder)
            : base(builder) { }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.ForkJoin,
                wf => new(),
                // Branch A has two tasks, to verify branch task ordering and that the JOIN's
                // joinOn resolves to the branch's last task ("task_a2"), not its first ("task_a1").
                branch => branch.AddTasks(NewSimpleTask("task_a1"), NewSimpleTask("task_a2")),
                // Branch B has a single task, to verify a simple one-task branch still works alongside a multi-task one.
                branch => branch.AddTasks(NewSimpleTask("task_b1"))
            );
        }

        private static WorkflowTask NewSimpleTask(string referenceName) =>
            new()
            {
                Name = referenceName,
                TaskReferenceName = referenceName,
                Type = WorkflowTaskType.SIMPLE.ToString(),
                WorkflowTaskType = WorkflowTaskType.SIMPLE,
            };
    }
}
