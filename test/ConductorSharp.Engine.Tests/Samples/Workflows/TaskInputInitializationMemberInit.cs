using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConductorSharp.Engine.Builders.Metadata;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class TaskInputInitializationMemberInitInput : WorkflowInput<TaskInputInitializationMemberInitOutput>
    {
        public string UserId { get; set; }
    }

    public class TaskInputInitializationMemberInitOutput : WorkflowOutput { }

    [OriginalName("TEST_TaskInputInialization")]
    public class TaskInputInitializationMemberInit
        : Workflow<TaskInputInitializationMemberInit, TaskInputInitializationMemberInitInput, TaskInputInitializationMemberInitOutput>
    {
        public TaskInputInitializationMemberInit(
            WorkflowDefinitionBuilder<
                TaskInputInitializationMemberInit,
                TaskInputInitializationMemberInitInput,
                TaskInputInitializationMemberInitOutput
            > builder
        ) : base(builder) { }

        public CustomerGetV1 GetCustomer { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(wf => wf.GetCustomer, wf => new() { });
        }
    }
}
