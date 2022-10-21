using ConductorSharp.Client.Model.Common;
using ConductorSharp.Definitions.Generated;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using MediatR;

namespace ConductorSharp.Definitions.Workflows
{
    public class SendCustomerNotificationInput : WorkflowInput<SendCustomerNotificationOutput>
    {
        public int? CustomerId { get; set; }
        public string? TaskToExecute { get; set; }
    }

    public class SendCustomerNotificationOutput : WorkflowOutput
    {
        public dynamic? CustomerId { get; set; }
        public dynamic? EmailBody { get; set; }
    }

    public class ExpectedDynamicInput : CustomerGetV1Input, IRequest<ExpectedDynamicOutput> { }

    public class ExpectedDynamicOutput : CustomerGetV1Output { }

    [OriginalName("NOTIFICATION_send_to_customer")]
    public class SendCustomerNotification : Workflow<SendCustomerNotificationInput, SendCustomerNotificationOutput>
    {
        public EmailPrepareV1? PrepareEmail { get; set; }
        public DynamicTaskModel<ExpectedDynamicInput, ExpectedDynamicOutput>? DynamicHandler { get; set; }
        public SendCustomerNotification? SendNotif { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<SendCustomerNotification>();

            builder.AddTask(
                a => a.DynamicHandler,
                b =>
                    new()
                    {
                        TaskInput = new() { CustomerId = b.WorkflowInput.CustomerId },
                        TaskToExecute = b.WorkflowInput.TaskToExecute,
                    }
            );

            builder.AddTask(a => a.PrepareEmail, b => new() { Address = b.DynamicHandler!.Output.Address, Name = b.DynamicHandler!.Output.Name });

            builder.SetOutput(
                a => new SendCustomerNotificationOutput { CustomerId = a.WorkflowInput.CustomerId, EmailBody = a.PrepareEmail.Output.EmailBody }
            );

            return builder.Build(options =>
            {
                options.Version = 1;
                options.OwnerEmail = "example@example.local";
            });
        }
    }
}
