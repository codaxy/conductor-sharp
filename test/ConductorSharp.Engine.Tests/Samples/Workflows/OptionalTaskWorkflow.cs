using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class OptionalTaskWorkflowInput : WorkflowInput<OptionalTaskWorkflowOutput>
    {
        public int CustomerId { get; set; }
    }

    public class OptionalTaskWorkflowOutput : WorkflowOutput { }

    [OriginalName("TEST_optional_task_workflow")]
    public class OptionalTaskWorkflow : Workflow<OptionalTaskWorkflow, OptionalTaskWorkflowInput, OptionalTaskWorkflowOutput>
    {
        public OptionalTaskWorkflow(WorkflowDefinitionBuilder<OptionalTaskWorkflow, OptionalTaskWorkflowInput, OptionalTaskWorkflowOutput> builder)
            : base(builder) { }

        public SendCustomerNotification SendNotificationSubworkflow { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(wf => wf.SendNotificationSubworkflow, wf => new() { CustomerId = wf.WorkflowInput.CustomerId }).AsOptional();
        }
    }
}
