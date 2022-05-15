using ConductorSharp.Engine.Model;
using MediatR;

namespace ConductorSharp.Engine.Builders
{

    public abstract class SubWorkflowTaskModel<I, O> : TaskModel<I, O> where I : IRequest<O>
    {
    }
}