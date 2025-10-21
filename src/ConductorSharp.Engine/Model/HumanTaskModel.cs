using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Model
{
    public class HumanTaskInput<TOutput> : ITaskInput<TOutput>;

    public class HumanTaskModel<TOutput> : TaskModel<HumanTaskInput<TOutput>, TOutput>;
}
