using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class TaskInputInitializationMemberInitInput : WorkflowInput<TaskInputInitializationMemberInitOutput>
    {
        public string UserId { get; set; }
    }

    public class TaskInputInitializationMemberInitOutput : WorkflowOutput { }

    [OriginalName("TEST_TaskInputInialization")]
    public class TaskInputInitializationMemberInit : Workflow<TaskInputInitializationMemberInitInput, TaskInputInitializationMemberInitOutput>
    {
        public CustomerGetV1 GetCustomer { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<TaskInputInitializationNew>();

            builder.AddTask(wf => wf.GetCustomer, wf => new() { });

            return builder.Build(opts => opts.Version = 1);
        }
    }
}
