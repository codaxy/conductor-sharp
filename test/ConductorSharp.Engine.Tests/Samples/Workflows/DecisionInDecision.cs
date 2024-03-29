﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConductorSharp.Engine.Builders.Metadata;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class DecisionInDecisionInput : WorkflowInput<DecisionInDecisionOutput>
    {
        public bool ShouldSendNotification { get; set; }
        public int CustomerId { get; set; }
    }

    public class DecisionInDecisionOutput : WorkflowOutput { }

    [OriginalName("TEST_decision_in_decision")]
    public class DecisionInDecision : Workflow<DecisionInDecision, DecisionInDecisionInput, DecisionInDecisionOutput>
    {
        public DecisionInDecision(WorkflowDefinitionBuilder<DecisionInDecision, DecisionInDecisionInput, DecisionInDecisionOutput> builder)
            : base(builder) { }

        public DecisionTaskModel SendNotificationDecision { get; set; }
        public DecisionTaskModel SecondSendNotificationDecision { get; set; }
        public SendCustomerNotification SendNotificationSubworkflow { get; set; }

        public override void BuildDefinition()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            _builder.AddTask(
                wf => wf.SendNotificationDecision,
                wf => new() { CaseValueParam = wf.WorkflowInput.ShouldSendNotification },
                new()
                {
                    ["YES"] = builder =>
                        builder.AddTask(
                            wf => wf.SecondSendNotificationDecision,
                            wf => new() { CaseValueParam = wf.WorkflowInput.ShouldSendNotification },
                            new()
                            {
                                ["YES"] = builder =>
                                    builder.AddTask(c => c.SendNotificationSubworkflow, wf => new() { CustomerId = wf.WorkflowInput.CustomerId })
                            }
                        )
                }
            );
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
