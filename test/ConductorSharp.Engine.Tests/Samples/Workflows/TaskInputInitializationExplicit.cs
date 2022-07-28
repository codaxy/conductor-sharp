using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class TaskInputInitializationExplicitInput : WorkflowInput<TaskInputInitializationExplicitOutput>
    {
        public string UserId { get; set; }
    }

    public class TaskInputInitializationExplicitOutput : WorkflowOutput { }

    [OriginalName("TEST_TaskInputInialization")]
    public class TaskInputInitializationExplicit : Workflow<TaskInputInitializationExplicitInput, TaskInputInitializationExplicitOutput>
    {
        public CustomerGetV1 GetCustomer { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<TaskInputInitializationExplicit>();

            builder.AddTask(wf => wf.GetCustomer, wf => new CustomerGetV1Input { CustomerId = wf.WorkflowInput.UserId });

            return builder.Build(opts => opts.Version = 1);
        }
    }
}
