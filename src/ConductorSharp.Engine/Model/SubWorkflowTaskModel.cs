using ConductorSharp.Engine.Interface;
using MediatR;

namespace ConductorSharp.Engine.Model
{
    public abstract class SubWorkflowTaskModel<TInput, TOuptut> : TaskModel<TInput, TOuptut>, INameable
        where TInput : ITaskInput<TOuptut>;
}
