using MediatR;

namespace ConductorSharp.Engine.Model
{
    public class ForkJoinInput : IRequest<NoOutput> { }

    public class ForkJoinTaskModel : TaskModel<ForkJoinInput, NoOutput> { }
}
