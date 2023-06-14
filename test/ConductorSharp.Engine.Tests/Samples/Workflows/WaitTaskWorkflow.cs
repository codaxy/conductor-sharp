namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class WaitTaskWorkflowInput : WorkflowInput<WaitTaskWorkflowOutput> { }

    public class WaitTaskWorkflowOutput : WorkflowOutput { }

    public class WaitTaskWorkflow : Workflow<WaitTaskWorkflow, WaitTaskWorkflowInput, WaitTaskWorkflowOutput>
    {
        public WaitTaskModel Wait { get; set; }

        public WaitTaskWorkflow(WorkflowDefinitionBuilder<WaitTaskWorkflow, WaitTaskWorkflowInput, WaitTaskWorkflowOutput> builder) : base(builder)
        { }

        public override void BuildDefinition()
        {
            _builder.AddTask(wf => wf.Wait, wf => new() { Duration = "1s", Until = "2022-12-31 11:59" });
        }
    }
}
