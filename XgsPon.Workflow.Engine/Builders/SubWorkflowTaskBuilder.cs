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
    public class SubWorkflowTaskBuilder<TInput, TOutput> : BaseTaskBuilder<TInput, TOutput>
        where TInput : IRequest<TOutput>
    {
        public SubWorkflowTaskBuilder(Expression taskExpression, Expression inputExpression)
            : base(taskExpression, inputExpression) { }

        public override WorkflowDefinition.Task[] Build() =>
            new WorkflowDefinition.Task[]
            {
                new WorkflowDefinition.Task
                {
                    Name = _taskName,
                    TaskReferenceName = _taskRefferenceName,
                    Type = "SUB_WORKFLOW",
                    InputParameters = _inputParameters,
                    SubWorkflowParam = new WorkflowDefinition.SubWorkflowParam
                    {
                        Name = _taskName,
                        Version = 1
                    },
                    Description = new JObject
                    {
                        new JProperty("description", _description)
                    }.ToString(Newtonsoft.Json.Formatting.None),
                }
            };
    }
}
