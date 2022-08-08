﻿namespace ConductorSharp.Engine.Tests.Samples.Workflows;

#region models
public class SendNotificationInput : IRequest<SendNotificationOutput>
{
    public dynamic CustomerId { get; set; }
}

public class SendNotificationOutput { }

[OriginalName("NOTIFICATION_send_to_customer")]
public class SendCustomerNotificationV1 : SubWorkflowTaskModel<SendNotificationInput, SendNotificationOutput> { }

public class SendCustomerNotificationInput : WorkflowInput<SendCustomerNotificationOutput>
{
    public dynamic CustomerId { get; set; }
}

public class SendCustomerNotificationOutput : WorkflowOutput
{
    public dynamic EmailBody { get; set; }
}
#endregion
[OriginalName("NOTIFICATION_send_to_customer")]
public class SendCustomerNotification : Workflow<SendCustomerNotificationInput, SendCustomerNotificationOutput>
{
    public CustomerGetV1 GetCustomer { get; set; }
    public EmailPrepareV1 PrepareEmail { get; set; }

    public override WorkflowDefinition GetDefinition()
    {
        var builder = new WorkflowDefinitionBuilder<SendCustomerNotification>();

        builder.AddTask(a => a.GetCustomer, b => new() { CustomerId = b.WorkflowInput.CustomerId });

        builder.AddTask(a => a.PrepareEmail, b => new() { Address = b.GetCustomer.Output.Address, Name = b.GetCustomer.Output.Name });

        return builder.Build(options =>
        {
            options.Version = 1;
            options.OwnerEmail = "example@example.local";
        });
    }
}
