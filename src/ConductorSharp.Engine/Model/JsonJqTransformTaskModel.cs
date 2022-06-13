using MediatR;

namespace ConductorSharp.Engine.Model
{
    public class JsonJqTransformTaskModel<I, O> : TaskModel<I, O> where I : IRequest<O>
    {
    }
}
