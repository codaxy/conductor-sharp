using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;

namespace ConductorSharp.Engine.Builders
{
    public static class HumanTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow, TOutput>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, HumanTaskModel<TOutput>>> reference,
            Expression<Func<TWorkflow, HumanTaskInput<TOutput>>> input
        )
            where TWorkflow : ITypedWorkflow
        {
            var taskBuilder = new HumanTaskBuilder<TOutput>(reference.Body, input.Body, builder.BuildConfiguration);
            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    internal class HumanTaskBuilder<TOutput>(Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration)
        : BaseTaskBuilder<HumanTaskInput<TOutput>, TOutput>(taskExpression, inputExpression, buildConfiguration)
    {
        public override WorkflowTask[] Build() =>
            [
                new()
                {
                    Name = $"HUMAN_{_taskRefferenceName}",
                    TaskReferenceName = _taskRefferenceName,
                    WorkflowTaskType = WorkflowTaskType.HUMAN,
                    Type = WorkflowTaskType.HUMAN.ToString(),
                    InputParameters = _inputParameters.ToObject<IDictionary<string, object>>(),
                }
            ];
    }
}
