using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using XgsPon.Workflows.Engine.Model;

namespace XgsPon.Workflows.Engine.Model
{
    public class DecisionTaskInput : IRequest<NoOutput>
    {
        public dynamic CaseValueParam { get; set; }
    }

    public class DecisionTaskModel : TaskModel<DecisionTaskInput, NoOutput>
    {
    }
}
