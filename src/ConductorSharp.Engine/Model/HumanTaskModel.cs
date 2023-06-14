using MediatR;
using Newtonsoft.Json;

namespace ConductorSharp.Engine.Model
{
    public class HumanTaskInput : IRequest<NoOutput> { }

    public class HumanTaskModel : TaskModel<HumanTaskInput, NoOutput> { }
}
