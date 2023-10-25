using ConductorSharp.Engine.Tests.Samples.Workers;

namespace ConductorSharp.Engine.Tests.Samples.Workflows;

#region models
public class SendCustomerNotificationInput : WorkflowInput<SendCustomerNotificationOutput>
{
    [PropertyName("id")]
    public int CustomerId { get; set; }
}

public class SendCustomerNotificationOutput : WorkflowOutput
{
    public object EmailBody { get; set; }
}
#endregion
[Version(3)]
[OriginalName("NOTIFICATION_send_to_customer")]
public class SendCustomerNotification : Workflow<SendCustomerNotification, SendCustomerNotificationInput, SendCustomerNotificationOutput>
{
    public SendCustomerNotification(
        WorkflowDefinitionBuilder<SendCustomerNotification, SendCustomerNotificationInput, SendCustomerNotificationOutput> builder
    ) : base(builder) { }

    public GetCustomerHandler GetCustomer { get; set; }
    public EmailPrepareV1 PrepareEmail { get; set; }

    public override void BuildDefinition()
    {
        _builder.AddTask(a => a.GetCustomer, b => new() { CustomerId = b.WorkflowInput.CustomerId });

        _builder.AddTask(a => a.PrepareEmail, b => new() { Address = b.GetCustomer.Output.Address, Name = b.GetCustomer.Output.Name });
    }
}
