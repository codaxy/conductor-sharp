using ConductorSharp.Engine.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class DecisionTaskInput : WorkflowInput<DecisionTaskOutput> { }

    public class DecisionTaskOutput : WorkflowOutput { }

    public class DecisionTask : Workflow<DecisionTask, DecisionTaskInput, DecisionTaskOutput>
    {
        public DecisionTaskModel Decision { get; set; }
        public CustomerGetV1 GetCustomer { get; set; }
        public TerminateTaskModel Terminate { get; set; }

        public DecisionTask(WorkflowDefinitionBuilder<DecisionTask, DecisionTaskInput, DecisionTaskOutput> builder) : base(builder) { }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.Decision,
                wf => new() { CaseValueParam = "test" },
                new()
                {
                    ["test"] = builder =>
                    {
                        builder.AddTask(wf => wf.GetCustomer, wf => new() { CustomerId = 1 });
                    },
                    DefaultCase = builder => builder.AddTask(wf => wf.GetCustomer, wf => new() { CustomerId = 0 }).AsOptional()
                }
            );
        }
    }
}
