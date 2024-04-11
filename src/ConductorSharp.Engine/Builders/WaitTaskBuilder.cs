﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;

namespace ConductorSharp.Engine.Builders
{
    public static class WaitTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, WaitTaskModel>> reference,
            Expression<Func<TWorkflow, WaitTaskInput>> input
        )
            where TWorkflow : ITypedWorkflow
        {
            var taskBuilder = new WaitTaskBuilder(reference.Body, input.Body, builder.BuildConfiguration);
            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    internal class WaitTaskBuilder(Expression taskExpression, Expression memberExpression, BuildConfiguration buildConfiguration)
        : BaseTaskBuilder<WaitTaskInput, NoOutput>(taskExpression, memberExpression, buildConfiguration)
    {
        public override WorkflowTask[] Build() =>
            [
                new()
                {
                    Name = $"WAIT_{_taskRefferenceName}",
                    TaskReferenceName = _taskRefferenceName,
                    WorkflowTaskType = WorkflowTaskType.WAIT,
                    Type = WorkflowTaskType.WAIT.ToString(),
                    InputParameters = _inputParameters.ToObject<IDictionary<string, object>>()
                }
            ];
    }
}
