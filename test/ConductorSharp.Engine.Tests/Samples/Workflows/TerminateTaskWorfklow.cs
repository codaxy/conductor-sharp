using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class TerminateTaskWorfklowInput : WorkflowInput<TerminateTaskWorfklowOutput> { }

    public class TerminateTaskWorfklowOutput : WorkflowOutput { }

    public class TerminateTaskWorfklow : Workflow<TerminateTaskWorfklowInput, TerminateTaskWorfklowOutput>
    {
        public DecisionTaskModel DecisionTask { get; set; }
        public SwitchTaskModel SwitchTask { get; set; }
        public TerminateTaskModel DecisionTerminate { get; set; }
        public TerminateTaskModel SwitchTerminate { get; set; }
        public TerminateTaskModel Terminate { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<TerminateTaskWorfklow>();

            builder.AddTask(
                wf => wf.DecisionTask,
                wf => new() { CaseValueParam = "value" },
                (
                    "value",
                    builder =>
                        builder.WithTask(
                            wf => wf.DecisionTerminate,
                            wf => new() { WorkflowOutput = new { Property = "Test" }, TerminationStatus = TerminationStatus.Completed }
                        )
                )
            );

            builder.AddTask(
                wf => wf.SwitchTask,
                wf => new() { SwitchCaseValue = "value" },
                (
                    "value",
                    builder =>
                        builder.WithTask(
                            wf => wf.SwitchTerminate,
                            wf => new() { WorkflowOutput = new { Property = "Test" }, TerminationStatus = TerminationStatus.Failed }
                        )
                )
            );

            builder.AddTask(
                wf => wf.Terminate,
                wf => new() { TerminationStatus = TerminationStatus.Failed, WorkflowOutput = new { Property = "Test" } }
            );

            return builder.Build(options => options.Version = 1);
        }
    }
}
