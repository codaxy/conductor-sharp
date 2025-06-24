using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using MediatR;

namespace ConductorSharp.Engine.Builders
{
    public static class EventTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow, TInput>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, EventTaskModel<TInput>>> reference,
            Expression<Func<TWorkflow, TInput>> input,
            string sink
        )
            where TWorkflow : ITypedWorkflow
            where TInput : ITaskInput<EventTaskModelOutput>
        {
            var taskBuilder = new EventTaskBuilder(reference.Body, input.Body, builder.BuildConfiguration, sink);
            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    internal class EventTaskBuilder(Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration, string sink)
        : BaseTaskBuilder<TerminateTaskInput, NoOutput>(taskExpression, inputExpression, buildConfiguration)
    {
        public override WorkflowTask[] Build() =>
            [
                new()
                {
                    Name = $"EVENT_{_taskRefferenceName}",
                    TaskReferenceName = _taskRefferenceName,
                    WorkflowTaskType = WorkflowTaskType.EVENT,
                    Type = WorkflowTaskType.EVENT.ToString(),
                    InputParameters = _inputParameters.ToObject<IDictionary<string, object>>(),
                    Sink = sink
                }
            ];
    }
}
