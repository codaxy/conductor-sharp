using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class NonEvaluatableWorkflowInput : WorkflowInput<NonEvaluatableWorkflowOutput>
    {
        public string Input { get; set; }
    }

    public class NonEvaluatableWorkflowOutput : WorkflowOutput { }

    public class NonEvaluatableWorkflow : Workflow<NonEvaluatableWorkflow, NonEvaluatableWorkflowInput, NonEvaluatableWorkflowOutput>
    {
        public CustomerGetV1 GetCustomer { get; set; }
        public EmailPrepareV1 PrepareEmail { get; set; }

        public NonEvaluatableWorkflow(
            WorkflowDefinitionBuilder<NonEvaluatableWorkflow, NonEvaluatableWorkflowInput, NonEvaluatableWorkflowOutput> builder
        ) : base(builder) { }

        public override void BuildDefinition()
        {
            _builder.AddTask(wf => wf.GetCustomer, wf => new() { CustomerId = wf.WorkflowInput.Input });

            _builder.AddTask(wf => wf.PrepareEmail, wf => new() { Address = $"{wf.GetCustomer.Output.Address}".ToUpperInvariant(), });
        }
    }
}
