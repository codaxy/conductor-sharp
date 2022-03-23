using ConductorSharp.Client.Model.Common;
using MediatR;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Builders;

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
