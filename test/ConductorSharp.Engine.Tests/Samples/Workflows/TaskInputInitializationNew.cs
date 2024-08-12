using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConductorSharp.Engine.Builders.Metadata;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class TaskInputInitializationNewInput : WorkflowInput<TaskInputInitializationNewOutput>
    {
        public string UserId { get; set; }
    }

    public class TaskInputInitializationNewOutput : WorkflowOutput { }

    [OriginalName("TEST_TaskInputInialization")]
    public class TaskInputInitializationNew : Workflow<TaskInputInitializationNew, TaskInputInitializationNewInput, TaskInputInitializationNewOutput>
    {
        public TaskInputInitializationNew(
            WorkflowDefinitionBuilder<TaskInputInitializationNew, TaskInputInitializationNewInput, TaskInputInitializationNewOutput> builder
        )
            : base(builder) { }

        public CustomerGetV1 GetCustomer { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(wf => wf.GetCustomer, wf => new());
        }
    }
}
