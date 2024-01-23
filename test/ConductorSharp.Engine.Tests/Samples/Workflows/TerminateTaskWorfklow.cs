namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class TerminateTaskWorfklowInput : WorkflowInput<TerminateTaskWorfklowOutput> { }

    public class TerminateTaskWorfklowOutput : WorkflowOutput { }

    public class TerminateTaskWorfklow : Workflow<TerminateTaskWorfklow, TerminateTaskWorfklowInput, TerminateTaskWorfklowOutput>
    {
        public TerminateTaskWorfklow(
            WorkflowDefinitionBuilder<TerminateTaskWorfklow, TerminateTaskWorfklowInput, TerminateTaskWorfklowOutput> builder
        )
            : base(builder) { }

        public DecisionTaskModel DecisionTask { get; set; }
        public SwitchTaskModel SwitchTask { get; set; }
        public TerminateTaskModel DecisionTerminate { get; set; }
        public TerminateTaskModel SwitchTerminate { get; set; }
        public TerminateTaskModel Terminate { get; set; }

        public override void BuildDefinition()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            _builder.AddTask(
                wf => wf.DecisionTask,
                wf => new() { CaseValueParam = "value" },
                new()
                {
                    ["value"] = builder =>
                        builder.AddTask(
                            wf => wf.DecisionTerminate,
                            wf => new() { WorkflowOutput = new { Property = "Test" }, TerminationStatus = TerminationStatus.Completed }
                        )
                }
            );
#pragma warning restore CS0618 // Type or member is obsolete

            _builder.AddTask(
                wf => wf.SwitchTask,
                wf => new() { SwitchCaseValue = "value" },
                new()
                {
                    ["value"] = builder =>
                        builder.AddTask(
                            wf => wf.SwitchTerminate,
                            wf => new() { WorkflowOutput = new { Property = "Test" }, TerminationStatus = TerminationStatus.Failed }
                        )
                }
            );

            _builder.AddTask(
                wf => wf.Terminate,
                wf => new() { TerminationStatus = TerminationStatus.Failed, WorkflowOutput = new { Property = "Test" } }
            );
        }
    }
}
