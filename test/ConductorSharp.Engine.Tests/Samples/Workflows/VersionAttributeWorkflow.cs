namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class VersionAttributeWorkflowInput : WorkflowInput<VersionAttributeWorkflowOutput> { }

    public class VersionAttributeWorkflowOutput : WorkflowOutput { }

    public class VersionAttributeWorkflow : Workflow<VersionAttributeWorkflowInput, VersionAttributeWorkflowOutput>
    {
        public VersionSubworkflow TestSubworkflow { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<VersionAttributeWorkflow>();

            builder.AddTask(wf => wf.TestSubworkflow, wf => new());

            return builder.Build(opts => opts.Version = 1);
        }
    }
}
