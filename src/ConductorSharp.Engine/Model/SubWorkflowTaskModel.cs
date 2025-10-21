using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Model
{
    public abstract class SubWorkflowTaskModel<TInput, TOuptut> : TaskModel<TInput, TOuptut>, INameable
        where TInput : ITaskInput<TOuptut>;
}
