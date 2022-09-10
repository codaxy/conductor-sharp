using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class OptionalTaskWorkflowInput : WorkflowInput<OptionalTaskWorkflowOutput>
    {
        public dynamic CustomerId { get; set; }
    }

    public class OptionalTaskWorkflowOutput : WorkflowOutput { }

    [OriginalName("TEST_optional_task_workflow")]
    public class OptionalTaskWorkflow : Workflow<OptionalTaskWorkflowInput, OptionalTaskWorkflowOutput>
    {
        public SendCustomerNotification SendNotificationSubworkflow { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<OptionalTaskWorkflow>();

            builder.AddTask(wf => wf.SendNotificationSubworkflow, wf => new() { CustomerId = wf.WorkflowInput.CustomerId }).AsOptional();

            return builder.Build(opts => opts.Version = 1);
        }
    }
}
