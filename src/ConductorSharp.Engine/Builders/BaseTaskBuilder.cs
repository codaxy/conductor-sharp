using ConductorSharp.Client;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Builders
{
    public abstract class BaseTaskBuilder<A, B> : ITaskOptionsBuilder, ITaskBuilder where A : IRequest<B>
    {
        protected readonly JObject _inputParameters;
        protected readonly string _taskRefferenceName;
        protected readonly string _taskName;
        protected string _description;
        protected AdditionalTaskParameters _additionalParameters = new();

        public BaseTaskBuilder(Expression taskExpression, Expression memberExpression)
        {
            var taskType = ExpressionUtil.ParseToType(taskExpression);

            _description = taskType.GetDocSection("summary");
            _taskRefferenceName = ExpressionUtil.ParseToReferenceName(taskExpression);
            _inputParameters = ExpressionUtil.ParseToParameters(memberExpression);
            _taskName = NamingUtil.DetermineRegistrationName(taskType);
        }

        public ITaskOptionsBuilder AsOptional()
        {
            _additionalParameters.Optional = true;
            return this;
        }

        public abstract WorkflowDefinition.Task[] Build();
    }
}
