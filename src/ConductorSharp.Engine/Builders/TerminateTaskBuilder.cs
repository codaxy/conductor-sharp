using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Builders
{
    public static class TerminateTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, TerminateTaskModel>> reference,
            Expression<Func<TWorkflow, TerminateTaskInput>> input
        ) where TWorkflow : ITypedWorkflow
        {
            var taskBuilder = new TerminateTaskBuilder(reference.Body, input.Body, builder.BuildConfiguration);
            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    internal class TerminateTaskBuilder(Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration) : BaseTaskBuilder<TerminateTaskInput, NoOutput>(taskExpression, inputExpression, buildConfiguration)
    {
        public override WorkflowTask[] Build() =>
            [
                new()
                {
                    Name = $"TERMINATE_{_taskRefferenceName}",
                    TaskReferenceName = _taskRefferenceName,
                    WorkflowTaskType = WorkflowTaskType.TERMINATE,
                    InputParameters = _inputParameters.ToObject<IDictionary<string, object>>(),
                }
            ];
    }
}
