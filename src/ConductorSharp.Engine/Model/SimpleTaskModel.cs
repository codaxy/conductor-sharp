using ConductorSharp.Engine.Interface;
using MediatR;

namespace ConductorSharp.Engine.Model
{
    public abstract class SimpleTaskModel<I, O> : TaskModel<I, O>, INameable where I : IRequest<O> { }
}
