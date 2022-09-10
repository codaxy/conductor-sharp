using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class DecisionInDecisionInput : WorkflowInput<DecisionInDecisionOutput>
    {
        public dynamic ShouldSendNotification { get; set; }
        public dynamic CustomerId { get; set; }
    }

    public class DecisionInDecisionOutput : WorkflowOutput { }

    [OriginalName("TEST_decision_in_decision")]
    public class DecisionInDecision : Workflow<DecisionInDecisionInput, DecisionInDecisionOutput>
    {
        public DecisionTaskModel SendNotificationDecision { get; set; }
        public DecisionTaskModel SecondSendNotificationDecision { get; set; }
        public SendCustomerNotification SendNotificationSubworkflow { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<DecisionInDecision>();

            builder.AddTask(
                wf => wf.SendNotificationDecision,
                wf => new() { CaseValueParam = wf.WorkflowInput.ShouldSendNotification },
                (
                    "YES",
                    builder =>
                        builder.WithTask(
                            wf => wf.SecondSendNotificationDecision,
                            wf => new() { CaseValueParam = wf.WorkflowInput.ShouldSendNotification },
                            (
                                "YES",
                                builder =>
                                    builder.WithTask(c => c.SendNotificationSubworkflow, wf => new() { CustomerId = wf.WorkflowInput.CustomerId })
                            )
                        )
                )
            );

            return builder.Build(opts => opts.Version = 1);
        }
    }
}
