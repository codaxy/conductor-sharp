namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class VersionAttributeWorkflowInput : WorkflowInput<VersionAttributeWorkflowOutput> { }

    public class VersionAttributeWorkflowOutput : WorkflowOutput { }

    public class VersionAttributeWorkflow : Workflow<VersionAttributeWorkflow, VersionAttributeWorkflowInput, VersionAttributeWorkflowOutput>
    {
        public VersionAttributeWorkflow(
            WorkflowDefinitionBuilder<VersionAttributeWorkflow, VersionAttributeWorkflowInput, VersionAttributeWorkflowOutput> builder
        ) : base(builder) { }

        public VersionSubworkflow TestSubworkflow { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(wf => wf.TestSubworkflow, wf => new());
        }
    }
}
