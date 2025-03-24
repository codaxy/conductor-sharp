using ConductorSharp.Client.Generated;
using TaskStatus = ConductorSharp.Client.Generated.TaskStatus;

namespace ConductorSharp.KafkaCancellationNotifier.Model;

internal class TaskStatusModel
{
    public WorkflowTask WorkflowTask { get; set; } = null!;
    public TaskStatus Status { get; set; }
    public string TaskId { get; set; } = null!;
    public string WorkflowInstanceId { get; set; } = null!;
}
