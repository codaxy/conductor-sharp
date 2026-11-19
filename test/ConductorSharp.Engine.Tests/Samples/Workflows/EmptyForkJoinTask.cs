namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public sealed class EmptyForkJoinTaskInput : WorkflowInput<EmptyForkJoinTaskOutput> { }

    public sealed class EmptyForkJoinTaskOutput : WorkflowOutput { }

    public sealed class EmptyForkJoinTask : Workflow<EmptyForkJoinTask, EmptyForkJoinTaskInput, EmptyForkJoinTaskOutput>
    {
        public ForkJoinTaskModel ForkJoin { get; set; }

        public EmptyForkJoinTask(WorkflowDefinitionBuilder<EmptyForkJoinTask, EmptyForkJoinTaskInput, EmptyForkJoinTaskOutput> builder)
            : base(builder) { }

        public override void BuildDefinition()
        {
            _builder.AddTask(wf => wf.ForkJoin, wf => new());
        }
    }
}
