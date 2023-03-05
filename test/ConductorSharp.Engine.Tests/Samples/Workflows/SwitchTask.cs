using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class SwitchTaskInput : WorkflowInput<SwitchTaskOutput> { }

    public class SwitchTaskOutput : WorkflowOutput { }

    public class SwitchTask : Workflow<SwitchTask, SwitchTaskInput, SwitchTaskOutput>
    {
        public SwitchTaskModel Switch { get; set; }
        public CustomerGetV1 GetCustomer { get; set; }
        public TerminateTaskModel Terminate { get; set; }

        public SwitchTask(WorkflowDefinitionBuilder<SwitchTask, SwitchTaskInput, SwitchTaskOutput> builder) : base(builder) { }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.Switch,
                wf => new() { SwitchCaseValue = "test" },
                new()
                {
                    ["test"] = builder =>
                    {
                        builder.AddTask(wf => wf.GetCustomer, wf => new() { CustomerId = 1 });
                    },
                    DefaultCase = builder => builder.AddTask(wf => wf.Terminate, wf => new() { TerminationStatus = TerminationStatus.Failed })
                }
            );
        }
    }
}
