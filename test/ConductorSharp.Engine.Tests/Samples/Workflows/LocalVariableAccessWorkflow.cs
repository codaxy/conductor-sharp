using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class LocalVariableAccessWorkflowInput : WorkflowInput<LocalVariableAccessWorkflowOutput> { }

    public class LocalVariableAccessWorkflowOutput : WorkflowOutput { }

    public class LocalVariableAccessWorkflow
        : Workflow<LocalVariableAccessWorkflow, LocalVariableAccessWorkflowInput, LocalVariableAccessWorkflowOutput>
    {
        public CustomerGetV1 CustomerGet { get; set; }
        public EmailPrepareV1 PrepareEmail { get; set; }

        public LocalVariableAccessWorkflow(
            WorkflowDefinitionBuilder<LocalVariableAccessWorkflow, LocalVariableAccessWorkflowInput, LocalVariableAccessWorkflowOutput> builder
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
