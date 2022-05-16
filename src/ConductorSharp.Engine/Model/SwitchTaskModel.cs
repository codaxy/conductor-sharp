using MediatR;

namespace ConductorSharp.Engine.Model
{
    public class SwitchTaskInput : IRequest<NoOutput>
    {
        public dynamic SwitchCaseValue { get; set; }
    }

    public class SwitchTaskModel : TaskModel<SwitchTaskInput, NoOutput>
    {
    }
}
