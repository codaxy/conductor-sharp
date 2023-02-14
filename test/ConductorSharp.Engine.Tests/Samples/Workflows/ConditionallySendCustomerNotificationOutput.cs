namespace ConductorSharp.Engine.Tests.Samples.Workflows;

#region models
public class ConditionallySendCustomerNotificationInput : WorkflowInput<ConditionallySendCustomerNotificationOutput>
{
    public bool ShouldSendNotification { get; set; }
    public int CustomerId { get; set; }
}

public class ConditionallySendCustomerNotificationOutput : WorkflowOutput { }
#endregion
[OriginalName("NOTIFICATION_conditionally_send_to_customer")]
public class ConditionallySendCustomerNotification
    : Workflow<ConditionallySendCustomerNotification, ConditionallySendCustomerNotificationInput, ConditionallySendCustomerNotificationOutput>
{
    public ConditionallySendCustomerNotification(
        WorkflowDefinitionBuilder<
            ConditionallySendCustomerNotification,
            ConditionallySendCustomerNotificationInput,
            ConditionallySendCustomerNotificationOutput
        > builder
    ) : base(builder) { }

    public DecisionTaskModel SendNotificationDecision { get; set; }
    public SendCustomerNotification SendNotificationSubworkflow { get; set; }

    public override void BuildDefinition()
    {
        _builder.AddTask(
            wf => wf.SendNotificationDecision,
            wf => new() { CaseValueParam = wf.WorkflowInput.ShouldSendNotification },
            new()
            {
                ["YES"] = builder => builder.AddTask(c => c.SendNotificationSubworkflow, wf => new() { CustomerId = wf.WorkflowInput.CustomerId })
            }
        );
    }
}
