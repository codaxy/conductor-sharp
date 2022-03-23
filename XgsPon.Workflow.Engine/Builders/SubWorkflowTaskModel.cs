using MediatR;
using XgsPon.Workflows.Engine.Model;

namespace XgsPon.Workflows.Engine.Builders
{
    public abstract class SubWorkflowTaskModel<I, O> : TaskModel<I, O> where I : IRequest<O>
    {
    }
}
