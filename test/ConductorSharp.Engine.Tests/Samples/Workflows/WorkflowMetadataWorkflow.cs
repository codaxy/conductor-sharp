using ConductorSharp.Engine.Builders.Metadata;

namespace ConductorSharp.Engine.Tests.Samples.Workflows;

public class WorkflowMetadataWorkflowInput : WorkflowInput<WorkflowMetadataWorkflowOutput> { }

public class WorkflowMetadataWorkflowOutput : WorkflowOutput { }

[WorkflowMetadata(
    OwnerEmail = "test@test.com",
    Description = "This is description",
    OwnerApp = "Owner",
    FailureWorkflow = typeof(ConditionallySendCustomerNotification)
)]
public class WorkflowMetadataWorkflow(
    WorkflowDefinitionBuilder<WorkflowMetadataWorkflow, WorkflowMetadataWorkflowInput, WorkflowMetadataWorkflowOutput> builder
) : Workflow<WorkflowMetadataWorkflow, WorkflowMetadataWorkflowInput, WorkflowMetadataWorkflowOutput>(builder)
{
    public CustomerGetV1 GetCustomer { get; set; }

    public override void BuildDefinition()
    {
        _builder.AddTask(wf => wf.GetCustomer, wf => new() { CustomerId = 1 });
    }
}
