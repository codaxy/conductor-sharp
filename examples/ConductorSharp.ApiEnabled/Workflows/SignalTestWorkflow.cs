using System;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Patterns.Builders;
using ConductorSharp.Patterns.Model;
using ConductorSharp.Patterns.Workflows;
using MediatR;

namespace ConductorSharp.ApiEnabled.Workflows;

public class SignalTestWorkflowInput : WorkflowInput<SignalTestWorkflowOutput>
{
    public string SignalKey { get; set; } = default!;
    public string Message { get; set; } = default!;
}

public class SignalTestWorkflowOutput : WorkflowOutput
{
    public string ProcessedMessage { get; set; } = default!;
    public string SignalKey { get; set; } = default!;
    public bool WaitCompleted { get; set; }
}

[OriginalName("SIGNAL_test_workflow")]
[WorkflowMetadata(OwnerEmail = "test@test.com")]
public class SignalTestWorkflow : Workflow<SignalTestWorkflow, SignalTestWorkflowInput, SignalTestWorkflowOutput>
{
    public class PreProcessInput : IRequest<PreProcessOutput>
    {
        public string Message { get; set; } = default!;
    }

    public class PreProcessOutput
    {
        public string ProcessedMessage { get; set; } = default!;
        public DateTime ProcessedAt { get; set; }
    }

    public class PostSignalInput : IRequest<PostSignalOutput>
    {
        public string OriginalMessage { get; set; } = default!;
        public string ProcessedMessage { get; set; } = default!;
    }

    public class PostSignalOutput
    {
        public string FinalMessage { get; set; } = default!;
    }

    public CSharpLambdaTaskModel<PreProcessInput, PreProcessOutput> PreProcessTask { get; set; } = default!;
    public SignalWait WaitForSignal { get; set; } = default!;
    public CSharpLambdaTaskModel<PostSignalInput, PostSignalOutput> PostSignalTask { get; set; } = default!;

#pragma warning disable CS8618
    public SignalTestWorkflow(WorkflowDefinitionBuilder<SignalTestWorkflow, SignalTestWorkflowInput, SignalTestWorkflowOutput> builder)
        : base(builder) { }
#pragma warning restore CS8618

    public override void BuildDefinition()
    {
        _builder.AddTask(
            wf => wf.PreProcessTask,
            wf => new PreProcessInput { Message = wf.WorkflowInput.Message },
            input => new PreProcessOutput { ProcessedMessage = $"[Processed] {input.Message.ToUpperInvariant()}", ProcessedAt = DateTime.UtcNow }
        );

        _builder.AddTask(wf => wf.WaitForSignal, wf => new SignalWaitInput { SignalKey = wf.WorkflowInput.SignalKey });

        _builder.AddTask(
            wf => wf.PostSignalTask,
            wf => new PostSignalInput { OriginalMessage = wf.WorkflowInput.Message, ProcessedMessage = wf.PreProcessTask.Output.ProcessedMessage },
            input =>
                new PostSignalOutput { FinalMessage = $"Signal received! Original: {input.OriginalMessage}, Processed: {input.ProcessedMessage}" }
        );

        _builder.SetOutput(
            wf =>
                new SignalTestWorkflowOutput
                {
                    ProcessedMessage = wf.PostSignalTask.Output.FinalMessage,
                    SignalKey = wf.WorkflowInput.SignalKey,
                    WaitCompleted = true
                }
        );
    }
}
