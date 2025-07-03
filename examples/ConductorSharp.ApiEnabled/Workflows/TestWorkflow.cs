using ConductorSharp.ApiEnabled.Handlers;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Builders.Metadata;

namespace ConductorSharp.ApiEnabled.Workflows;

public class TestWorkflow : WorkflowInput<TestWorkflow.Output>
{
    public string Input { get; set; }

    public class Output : WorkflowOutput
    {
        public string Out { get; set; }
    }

    [OriginalName("TEST_workflow")]
    public class Workflow : Workflow<Workflow, TestWorkflow, Output>
    {
        public Workflow(WorkflowDefinitionBuilder<Workflow, TestWorkflow, Output> builder)
            : base(builder) { }

        public FirstTestWorker.Worker FirstWorker { get; set; }

        public SecondTestWorker.Worker SecondWorker { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(wf => wf.FirstWorker, wf => new() { Input = wf.Input.Input });

            _builder.AddTask(wf => wf.SecondWorker, wf => new() { Input = wf.FirstWorker.Output.Output });

            _builder.SetOutput(wf => new() { Out = wf.SecondWorker.Output.Output });
        }
    }
}
