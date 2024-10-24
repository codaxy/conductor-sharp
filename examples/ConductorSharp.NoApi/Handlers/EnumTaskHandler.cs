using ConductorSharp.Client.Generated;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using MediatR;

namespace ConductorSharp.NoApi.Handlers
{
    public class EnumTaskInput : IRequest<EnumTaskOutput>
    {
        public WorkflowStatus Status { get; set; }
    }

    public class EnumTaskOutput
    {
        public WorkflowStatus Status { get; set; }
    }

    [OriginalName("ENUM_task")]
    public class EnumTaskHandler : TaskRequestHandler<EnumTaskInput, EnumTaskOutput>
    {
        public override Task<EnumTaskOutput> Handle(EnumTaskInput request, CancellationToken cancellationToken)
        {
            Console.WriteLine(request.Status);
            return System.Threading.Tasks.Task.FromResult(new EnumTaskOutput() { Status = request.Status });
        }
    }
}
