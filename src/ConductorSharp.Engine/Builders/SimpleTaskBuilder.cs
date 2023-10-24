using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Builders
{
    public static class SimpleTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow, Tinput, TOutput>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, SimpleTaskModel<Tinput, TOutput>>> refference,
            Expression<Func<TWorkflow, Tinput>> input
        )
            where TWorkflow : ITypedWorkflow
            where Tinput : IRequest<TOutput>
        {
            var taskBuilder = new SimpleTaskBuilder<Tinput, TOutput>(refference.Body, input.Body, builder.BuildConfiguration);
            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    public class SimpleTaskBuilder<A, B> : BaseTaskBuilder<A, B> where A : IRequest<B>
    {
        public SimpleTaskBuilder(Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration)
            : base(taskExpression, inputExpression, buildConfiguration) { }

        public override WorkflowDefinition.Task[] Build() =>
            new WorkflowDefinition.Task[]
            {
                new WorkflowDefinition.Task
                {
                    Name = _taskName,
                    TaskReferenceName = _taskRefferenceName,
                    Type = "SIMPLE",
                    InputParameters = _inputParameters,
                    Optional = _additionalParameters?.Optional == true,
                }
            };
    }
}
