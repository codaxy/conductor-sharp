using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;

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
            where Tinput : ITaskInput<TOutput>
        {
            var taskBuilder = new SimpleTaskBuilder<Tinput, TOutput>(refference.Body, input.Body, builder.BuildConfiguration);
            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    public class SimpleTaskBuilder<TInput, TOutput>(Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration)
        : BaseTaskBuilder<TInput, TOutput>(taskExpression, inputExpression, buildConfiguration)
        where TInput : ITaskInput<TOutput>
    {
        public override WorkflowTask[] Build() =>
            [
                new()
                {
                    Name = _taskName,
                    TaskReferenceName = _taskRefferenceName,
                    WorkflowTaskType = WorkflowTaskType.SIMPLE,
                    Type = WorkflowTaskType.SIMPLE.ToString(),
                    InputParameters = _inputParameters.ToObject<IDictionary<string, object>>(),
                    Optional = _additionalParameters?.Optional == true
                }
            ];
    }
}
