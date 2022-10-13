using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Handlers;
using ConductorSharp.Engine.Interface;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ConductorSharp.Engine.Builders
{
    internal class CSharpLambdaTaskBuilder<TInput, TOutput> : BaseTaskBuilder<TInput, TOutput> where TInput : IRequest<TOutput>
    {
        public CSharpLambdaTaskBuilder(Expression taskExpression, Expression memberExpression, string workflowName)
            : base(taskExpression, memberExpression)
        {
            LambdaIdentifier = $"{workflowName}.{_taskRefferenceName}";
        }

        public string LambdaIdentifier { get; }

        public override WorkflowDefinition.Task[] Build()
        {
            return new WorkflowDefinition.Task[]
            {
                new WorkflowDefinition.Task
                {
                    Name = _taskName,
                    TaskReferenceName = LambdaIdentifier,
                    Type = "SIMPLE",
                    InputParameters = new JObject
                    {
                        new JProperty(CSharpLambdaTaskInput.LambdaIdenfitierPropName, LambdaIdentifier),
                        new JProperty(CSharpLambdaTaskInput.TaskInputPropName, _inputParameters)
                    },
                    Description = new JObject { new JProperty("description", _description) }.ToString(Newtonsoft.Json.Formatting.None),
                    Optional = _additionalParameters?.Optional == true,
                }
            };
        }
    }
}
