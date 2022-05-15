using MediatR;

namespace ConductorSharp.Engine.Model
{

    public class DecisionTaskInput : IRequest<NoOutput>
    {
        public dynamic CaseValueParam { get; set; }
    }

    public class DecisionTaskModel : TaskModel<DecisionTaskInput, NoOutput>
    {
    }
}