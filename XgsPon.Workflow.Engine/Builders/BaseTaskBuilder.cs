using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using XgsPon.Workflows.Client.Model.Common;
using XgsPon.Workflows.Engine.Interface;
using XgsPon.Workflows.Engine.Model;
using XgsPon.Workflows.Engine.Util;

namespace XgsPon.Workflows.Engine.Builders
{
    public abstract class BaseTaskBuilder<A, B> : ITaskBuilder where A : IRequest<B>
    {
        protected readonly JObject _inputParameters;
        protected readonly string _taskRefferenceName;
        protected readonly string _taskName;
        protected string _description;

        public BaseTaskBuilder(Expression taskExpression, Expression memberExpression)
        {
            var taskType = ExpressionUtil.ParseToType(taskExpression);

            _description = taskType.GetDocSection("summary");
            _taskRefferenceName = ExpressionUtil.ParseToReferenceName(taskExpression);
            _inputParameters = ExpressionUtil.ParseToParameters(memberExpression);
            _taskName = NamingUtil.DetermineRegistrationName(taskType);
        }

        public abstract WorkflowDefinition.Task[] Build();
    }
}
