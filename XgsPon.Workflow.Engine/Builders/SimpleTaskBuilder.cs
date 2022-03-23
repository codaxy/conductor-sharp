using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using XgsPon.Workflows.Client.Model.Common;
using XgsPon.Workflows.Engine.Interface;
using XgsPon.Workflows.Engine.Model;
using XgsPon.Workflows.Engine.Builders;
using XgsPon.Workflows.Engine.Util;
using MediatR;

namespace XgsPon.Workflows.Engine.Builders
{
    public class SimpleTaskBuilder<A, B> : BaseTaskBuilder<A, B> where A : IRequest<B>
    {
        public SimpleTaskBuilder(Expression taskExpression, Expression inputExpression)
            : base(taskExpression, inputExpression) { }

        public override WorkflowDefinition.Task[] Build() =>
            new WorkflowDefinition.Task[]
            {
                new WorkflowDefinition.Task
                {
                    Name = _taskName,
                    TaskReferenceName = _taskRefferenceName,
                    Type = "SIMPLE",
                    InputParameters = _inputParameters,
                    Description = new JObject
                    {
                        new JProperty("description", _description)
                    }.ToString(Newtonsoft.Json.Formatting.None),
                }
            };
    }
}
