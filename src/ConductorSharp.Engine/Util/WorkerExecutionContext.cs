namespace ConductorSharp.Engine.Util;

public record WorkerExecutionContext(
    string WorkflowName,
    string WorkflowId,
    string TaskName,
    string TaskId,
    string TaskReferenceName,
    string CorrelationId,
    string WorkerId
);
