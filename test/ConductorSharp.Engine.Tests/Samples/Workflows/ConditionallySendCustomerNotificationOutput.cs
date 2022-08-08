using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    #region models
    public class SendNotificationInput : IRequest<SendNotificationOutput>
    {
        public dynamic CustomerId { get; set; }
    }

    public class SendNotificationOutput { }

    [OriginalName("NOTIFICATION_send_to_customer")]
    public class SendCustomerNotificationV1 : SubWorkflowTaskModel<SendNotificationInput, SendNotificationOutput> { }

    public class ConditionallySendCustomerNotificationInput : WorkflowInput<ConditionallySendCustomerNotificationOutput>
    {
        public dynamic ShouldSendNotification { get; set; }
        public dynamic CustomerId { get; set; }
    }

    public class ConditionallySendCustomerNotificationOutput : WorkflowOutput { }
    #endregion
    [OriginalName("NOTIFICATION_conditionally_send_to_customer")]
    public class ConditionallySendCustomerNotification
        : Workflow<ConditionallySendCustomerNotificationInput, ConditionallySendCustomerNotificationOutput>
    {
        public DecisionTaskModel SendNotificationDecision { get; set; }
        public SendCustomerNotificationV1 SendNotificationSubworkflow { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<ConditionallySendCustomerNotification>();

            builder.AddTask(
                a => a.SendNotificationDecision,
                a => new() { CaseValueParam = a.WorkflowInput.ShouldSendNotification },
                (
                    "YES",
                    decisionBuilder =>
                        decisionBuilder.WithTask(c => c.SendNotificationSubworkflow, wf => new() { CustomerId = wf.WorkflowInput.CustomerId })
                )
            );

            return builder.Build(opts => opts.Version = 1);
        }
    }
}
