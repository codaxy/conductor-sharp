using ConductorSharp.Client.Generated;
using ConductorSharp.Definitions.Generated;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Builders.Metadata;

namespace ConductorSharp.Definitions.Workflows
{
    public class EnumWorkflowInput : WorkflowInput<EnumWorkflowOutput> { }

    public class EnumWorkflowOutput : WorkflowOutput { }

    [OriginalName("ENUM_workflow")]
    public class EnumWorkflow : Workflow<EnumWorkflow, EnumWorkflowInput, EnumWorkflowOutput>
    {
        public EnumWorkflow(WorkflowDefinitionBuilder<EnumWorkflow, EnumWorkflowInput, EnumWorkflowOutput> builder)
            : base(builder) { }

        public EnumTask EnumTask1 { get; set; }
        public EnumTask EnumTask2 { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(wf => wf.EnumTask1, wf => new() { Status = 1 });

            _builder.AddTask(wf => wf.EnumTask2, wf => new() { Status = wf.EnumTask1.Output.Status });
        }
    }
}
