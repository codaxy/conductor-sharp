using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using XgsPon.Workflows.Client.Model.Common;
using XgsPon.Workflows.Engine.Interface;
using XgsPon.Workflows.Engine.Model;

namespace XgsPon.Workflows.Engine.Builders
{
    public class LambdaTaskBuilder<A, B> : BaseTaskBuilder<A, B> where A : IRequest<B>
    {
        private readonly string _script;

        public LambdaTaskBuilder(
            string script,
            Expression taskExpression,
            Expression inputExpression
        ) : base(taskExpression, inputExpression) => _script = script;

        public override WorkflowDefinition.Task[] Build()
        {
            _inputParameters.Add(new JProperty("scriptExpression", _script));
            return new WorkflowDefinition.Task[]
            {
                new WorkflowDefinition.Task
                {
                    Name = $"LAMBDA_{_taskRefferenceName}",
                    TaskReferenceName = _taskRefferenceName,
                    Type = "LAMBDA",
                    Description = new JObject
                    {
                        new JProperty("description", _description)
                    }.ToString(Newtonsoft.Json.Formatting.None),
                    InputParameters = _inputParameters
                }
            };
        }
    }
}
