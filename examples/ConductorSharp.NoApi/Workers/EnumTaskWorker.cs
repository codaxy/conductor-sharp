using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;

namespace ConductorSharp.NoApi.Workers;

public class EnumTaskInput : ITaskInput<EnumTaskOutput>
{
    public WorkflowStatus Status { get; set; }
}

public class EnumTaskOutput
{
    public WorkflowStatus Status { get; set; }
}

[OriginalName("ENUM_task")]
public class EnumTaskWorker : IWorker<EnumTaskInput, EnumTaskOutput>
{
    public Task<EnumTaskOutput> Handle(EnumTaskInput request, WorkerExecutionContext context, CancellationToken cancellationToken)
    {
        Console.WriteLine(request.Status);
        return System.Threading.Tasks.Task.FromResult(new EnumTaskOutput() { Status = request.Status });
    }
}
