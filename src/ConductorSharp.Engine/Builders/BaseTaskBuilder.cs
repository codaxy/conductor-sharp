using System.Linq.Expressions;
using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.Engine.Builders
{
    public abstract class BaseTaskBuilder<TInput, TOutput> : ITaskOptionsBuilder, ITaskBuilder
        where TInput : ITaskInput<TOutput>
    {
        protected readonly JObject _inputParameters;
        protected readonly string _taskRefferenceName;
        protected readonly string _taskName;
        protected readonly BuildConfiguration _buildConfiguration;
        protected AdditionalTaskParameters _additionalParameters = new();

        public BaseTaskBuilder(Expression taskExpression, Expression memberExpression, BuildConfiguration buildConfiguration)
        {
            var taskType = ExpressionUtil.ParseToType(taskExpression);
            _taskRefferenceName = ExpressionUtil.ParseToReferenceName(taskExpression);
            _inputParameters = ExpressionUtil.ParseToParameters(memberExpression);
            _taskName = NamingUtil.DetermineRegistrationName(taskType);
            _buildConfiguration = buildConfiguration;
        }

        public ITaskOptionsBuilder AsOptional()
        {
            _additionalParameters.Optional = true;
            return this;
        }

        public abstract WorkflowTask[] Build();
    }
}
