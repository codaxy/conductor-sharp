using MediatR;

namespace ConductorSharp.Engine.Model
{
    public class SwitchTaskInput : IRequest<NoOutput>
    {
        public object SwitchCaseValue { get; set; }
    }

    public class SwitchTaskModel : TaskModel<SwitchTaskInput, NoOutput> { }
}
