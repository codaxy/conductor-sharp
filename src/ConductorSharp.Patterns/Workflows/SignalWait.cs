using System.ComponentModel.DataAnnotations;
using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Model;
using ConductorSharp.Patterns.Tasks;

namespace ConductorSharp.Patterns.Workflows;

#region models

public class SignalWaitInput : WorkflowInput<SignalWaitOutput>
{
    [Required]
    public string SignalKey { get; set; } = default!;
}

public class SignalWaitOutput : WorkflowOutput { }

#endregion

[OriginalName(Constants.SignalWaitWorkflowName)]
[WorkflowMetadata(OwnerEmail = "conductorsharp@codaxy.com")]
public class SignalWait : Workflow<SignalWait, SignalWaitInput, SignalWaitOutput>
{
    internal const string WaitTaskRefName = "wait_for_signal";

    public RegisterWaiter RegisterWaiter { get; set; }
    public SwitchTaskModel SignalDecision { get; set; }
    public WaitTaskModel WaitForSignal { get; set; }

#pragma warning disable CS8618
    public SignalWait(WorkflowDefinitionBuilder<SignalWait, SignalWaitInput, SignalWaitOutput> builder)
        : base(builder) { }
#pragma warning restore CS8618

    public override void BuildDefinition()
    {
        base.BuildDefinition();

        _builder.AddTask(wf => wf.RegisterWaiter, wf => new RegisterWaiterRequest { SignalKey = wf.WorkflowInput.SignalKey });

        _builder.AddTask(
            wf => wf.SignalDecision,
            wf => new SwitchTaskInput { SwitchCaseValue = wf.RegisterWaiter.Output.AlreadySignaled },
            new DecisionCases<SignalWait>
            {
                ["false"] = builder =>
                {
                    builder.AddTask(wf => wf.WaitForSignal, wf => new WaitTaskInput { });
                }
            }
        );
    }
}
