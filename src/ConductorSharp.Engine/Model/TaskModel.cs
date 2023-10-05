using ConductorSharp.Engine.Interface;
using MediatR;

namespace ConductorSharp.Engine.Model
{
    public abstract class TaskModel<I, O> : ITaskModel where I : IRequest<O>
    {
        public I Input { get; set; }
        public O Output { get; set; }
    }
}
