using ConductorSharp.Engine.Interface;
using MediatR;

namespace ConductorSharp.Engine.Model
{
    public abstract class SubWorkflowTaskModel<I, O> : TaskModel<I, O>, INameable where I : IRequest<O> { }
}
