using MediatR;

namespace ConductorSharp.Engine.Model
{

    public abstract class SimpleTaskModel<I, O> : TaskModel<I, O> where I : IRequest<O>
    {
    }
}