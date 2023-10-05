using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class VariableAccessWorkflowInput : WorkflowInput<VariableAccessWorkflowOutput> { }

    public class VariableAccessWorkflowOutput : WorkflowOutput { }

    public class VariableAccessWorkflow
        : Workflow<VariableAccessWorkflow, VariableAccessWorkflowInput, VariableAccessWorkflowOutput>
    {
        public CustomerGetV1 CustomerGet { get; set; }
        public EmailPrepareV1 PrepareEmail { get; set; }

        public VariableAccessWorkflow(
            WorkflowDefinitionBuilder<VariableAccessWorkflow, VariableAccessWorkflowInput, VariableAccessWorkflowOutput> builder
        ) : base(builder) { }

        public override void BuildDefinition()
        {
            int myVar = int.Parse("2");

            _builder.AddTask(wf => wf.CustomerGet, wf => new() { CustomerId = myVar });

            //_builder.AddTask(wf => wf.PrepareEmail, wf => new()
            //{
            //    Address =
            //});
        }
    }
}
