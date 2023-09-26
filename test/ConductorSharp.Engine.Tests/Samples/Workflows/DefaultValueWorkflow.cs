using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class DefaultValueWorkflowInput : WorkflowInput<DefaultValueWorkflowOutput>
    {
        [DefaultValue("Default name")]
        public string Name { get; set; }

        [DefaultValue("Default address")]
        public string Address { get; set; }
    }

    public class DefaultValueWorkflowOutput : WorkflowOutput { }

    public class DefaultValueWorkflow : Workflow<DefaultValueWorkflow, DefaultValueWorkflowInput, DefaultValueWorkflowOutput>
    {
        public EmailPrepareV1 PrepareEmail { get; set; }

        public DefaultValueWorkflow(WorkflowDefinitionBuilder<DefaultValueWorkflow, DefaultValueWorkflowInput, DefaultValueWorkflowOutput> builder)
            : base(builder) { }

        public override void BuildDefinition()
        {
            _builder.AddTask(wf => wf.PrepareEmail, wf => new() { Name = wf.WorkflowInput.Name, Address = wf.WorkflowInput.Address });
        }
    }
}
