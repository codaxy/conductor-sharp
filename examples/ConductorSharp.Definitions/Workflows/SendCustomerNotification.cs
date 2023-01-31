using ConductorSharp.Client.Model.Common;
using ConductorSharp.Definitions.Generated;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
using ConductorSharp.Patterns.Tasks;
using MediatR;

namespace ConductorSharp.Definitions.Workflows
{
    public class SendCustomerNotificationInput : WorkflowInput<SendCustomerNotificationOutput>
    {
        public int CustomerId { get; set; }
        public string TaskToExecute { get; set; }
    }

    public class SendCustomerNotificationOutput : WorkflowOutput
    {
        public dynamic CustomerId { get; set; }
        public dynamic EmailBody { get; set; }
        public dynamic Constant { get; set; }
    }

    public class ExpectedDynamicInput : CustomerGetV1Input, IRequest<ExpectedDynamicOutput> { }

    public class ExpectedDynamicOutput : CustomerGetV1Output { }

    [OriginalName("NOTIFICATION_send_to_customer")]
    public class SendCustomerNotification : Workflow<SendCustomerNotification, SendCustomerNotificationInput, SendCustomerNotificationOutput>
    {
        public SendCustomerNotification(
            WorkflowDefinitionBuilder<SendCustomerNotification, SendCustomerNotificationInput, SendCustomerNotificationOutput> builder
        ) : base(builder) { }

        public EmailPrepareV1 PrepareEmail { get; set; }
        public DynamicTaskModel<ExpectedDynamicInput, ExpectedDynamicOutput> DynamicHandler { get; set; }
        public WaitSeconds WaitSeconds { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                a => a.DynamicHandler,
                b =>
                    new()
                    {
                        TaskInput = new() { CustomerId = b.WorkflowInput.CustomerId },
                        TaskToExecute = b.WorkflowInput.TaskToExecute,
                    }
            );

            _builder.AddTask(a => a.WaitSeconds, b => new() { Seconds = 10 });

            _builder.AddTask(a => a.PrepareEmail, b => new() { Address = b.DynamicHandler!.Output.Address, Name = b.DynamicHandler!.Output.Name });

            _builder.SetOutput(
                a =>
                    new()
                    {
                        CustomerId = a.WorkflowInput.CustomerId,
                        EmailBody = a.PrepareEmail.Output.EmailBody,
                        Constant = 500
                    }
            );

            _builder.SetOptions(options =>
            {
                options.Version = 1;
                options.FailureWorkflow = typeof(HandleNotificationFailure);
                options.OwnerEmail = "example@example.local";
            });
        }
    }
}
