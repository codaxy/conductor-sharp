using ConductorSharp.Engine.Builders.Metadata;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class EventTaskWorkflowInput : WorkflowInput<EventTaskWorkflowOutput> { }

    public class EventTaskWorkflowOutput : WorkflowOutput { }

    [WorkflowMetadata(OwnerEmail = "test@test.com")]
    public class EventTaskWorkflow : Workflow<EventTaskWorkflow, EventTaskWorkflowInput, EventTaskWorkflowOutput>
    {
        public class EventTaskPayload : IRequest<EventTaskModelOutput>
        {
            public string PayloadParam { get; set; }
        }

        public EventTaskModel<EventTaskPayload> EventTask { get; set; }

        public EventTaskWorkflow(WorkflowDefinitionBuilder<EventTaskWorkflow, EventTaskWorkflowInput, EventTaskWorkflowOutput> builder)
            : base(builder) { }

        public override void BuildDefinition()
        {
            _builder.AddTask(wf => wf.EventTask, wf => new() { PayloadParam = "param" }, "conductor:event");
        }
    }
}
