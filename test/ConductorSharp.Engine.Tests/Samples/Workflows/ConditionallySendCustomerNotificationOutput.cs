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
public class ConditionallySendCustomerNotification : Workflow<ConditionallySendCustomerNotificationInput, ConditionallySendCustomerNotificationOutput>
{
    public DecisionTaskModel SendNotificationDecision { get; set; }
    public SendCustomerNotification SendNotificationSubworkflow { get; set; }

    public override WorkflowDefinition GetDefinition()
    {
        var builder = new WorkflowDefinitionBuilder<ConditionallySendCustomerNotification>();

        builder.AddTask(
            wf => wf.SendNotificationDecision,
            wf => new() { CaseValueParam = wf.WorkflowInput.ShouldSendNotification },
            ("YES", builder => builder.WithTask(c => c.SendNotificationSubworkflow, wf => new() { CustomerId = wf.WorkflowInput.CustomerId }))
        );

        return builder.Build(opts => opts.Version = 1);
    }
}
