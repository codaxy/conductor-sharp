using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Model;
using MediatR;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Builders
{
    public class SimpleTaskBuilder<A, B> : BaseTaskBuilder<A, B> where A : IRequest<B>
    {
        private readonly AdditionalTaskParameters _additionalParameters;

        public SimpleTaskBuilder(Expression taskExpression, Expression inputExpression, AdditionalTaskParameters additionalParameters)
            : base(taskExpression, inputExpression)
        {
            _additionalParameters = additionalParameters;
        }

        public override WorkflowDefinition.Task[] Build() =>
            new WorkflowDefinition.Task[]
            {
                new WorkflowDefinition.Task
                {
                    Name = _taskName,
                    TaskReferenceName = _taskRefferenceName,
                    Type = "SIMPLE",
                    InputParameters = _inputParameters,
                    Description = new JObject { new JProperty("description", _description) }.ToString(Newtonsoft.Json.Formatting.None),
                    Optional = _additionalParameters != null ? _additionalParameters.Optional : false
                }
            };
    }
}
