using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Model
{
    public class DecisionTaskInput : ITaskInput<NoOutput>
    {
        public object CaseValueParam { get; set; }
    }

    public class DecisionTaskModel : TaskModel<DecisionTaskInput, NoOutput>;
}
