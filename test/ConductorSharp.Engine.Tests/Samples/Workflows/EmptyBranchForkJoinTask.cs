using ConductorSharp.Client.Generated;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public sealed class EmptyBranchForkJoinTaskInput : WorkflowInput<EmptyBranchForkJoinTaskOutput> { }

    public sealed class EmptyBranchForkJoinTaskOutput : WorkflowOutput { }

    public sealed class EmptyBranchForkJoinTask : Workflow<EmptyBranchForkJoinTask, EmptyBranchForkJoinTaskInput, EmptyBranchForkJoinTaskOutput>
    {
        public ForkJoinTaskModel ForkJoin { get; set; }

        public EmptyBranchForkJoinTask(
            WorkflowDefinitionBuilder<EmptyBranchForkJoinTask, EmptyBranchForkJoinTaskInput, EmptyBranchForkJoinTaskOutput> builder
        )
            : base(builder) { }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.ForkJoin,
                wf => new(),
                branch =>
                    branch.AddTasks(
                        new WorkflowTask
                        {
                            Name = "task_a1",
                            TaskReferenceName = "task_a1",
                            Type = WorkflowTaskType.SIMPLE.ToString(),
                            WorkflowTaskType = WorkflowTaskType.SIMPLE,
                        }
                    ),
                branch => { }
            );
        }
    }
}
