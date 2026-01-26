using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Model
{
    public class SwitchTaskInput : ITaskInput<NoOutput>
    {
        public object SwitchCaseValue { get; set; }
    }

    public class SwitchTaskModel : TaskModel<SwitchTaskInput, NoOutput>;
}
