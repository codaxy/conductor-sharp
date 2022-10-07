using ConductorSharp.Engine.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class NewDecisionMethodInput : WorkflowInput<NewDecisionMethodOutput>
    {
        public string DecisionValue { get; set; }
    }

    public class NewDecisionMethodOutput : WorkflowOutput { }

    public class NewDecisionMethod : Workflow<NewDecisionMethodInput, NewDecisionMethodOutput>
    {
        public DecisionTaskModel Decision { get; set; }
        public CustomerGetV1 GetCustomer { get; set; }
        public CustomerGetV1 GetCustomerDefault { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<NewDecisionMethod>();

            builder.AddTask(
                wf => wf.Decision,
                wf => new() { CaseValueParam = wf.WorkflowInput.DecisionValue },
                new DecisionCases<NewDecisionMethod>
                {
                    ["NonDefaultCase"] = builder =>
                    {
                        builder.WithTask(wf => wf.GetCustomer, wf => new() { CustomerId = 1 });
                    },
                    DefaultCase = builder =>
                    {
                        builder.WithTask(wf => wf.GetCustomerDefault, wf => new() { CustomerId = 2 });
                    }
                }
            );

            return builder.Build(opts => opts.Version = 1);
        }
    }
}
