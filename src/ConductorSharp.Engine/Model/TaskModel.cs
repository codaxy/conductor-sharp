using MediatR;

namespace ConductorSharp.Engine.Model;

public abstract class TaskModel<I, O> where I : IRequest<O>
{
    public I Input { get; set; }
    public O Output { get; set; }
}
