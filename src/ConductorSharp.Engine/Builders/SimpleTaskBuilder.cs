﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using MediatR;

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

    public class SimpleTaskBuilder<A, B>(Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration)
        : BaseTaskBuilder<A, B>(taskExpression, inputExpression, buildConfiguration)
        where A : IRequest<B>
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
