using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Interface;

namespace ConductorSharp.NoApi.Handlers
{
    public class EnumTaskInput : ITaskInput<EnumTaskOutput>
    {
        public WorkflowStatus Status { get; set; }
    }

    public class EnumTaskOutput
    {
        public WorkflowStatus Status { get; set; }
    }

    [OriginalName("ENUM_task")]
    public class EnumTaskHandler : INgWorker<EnumTaskInput, EnumTaskOutput>
    {
        public Task<EnumTaskOutput> Handle(EnumTaskInput request, CancellationToken cancellationToken)
        {
            Console.WriteLine(request.Status);
            return System.Threading.Tasks.Task.FromResult(new EnumTaskOutput() { Status = request.Status });
        }
    }
}
