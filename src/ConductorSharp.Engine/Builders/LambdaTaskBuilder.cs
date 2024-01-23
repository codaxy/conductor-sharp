﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using MediatR;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.Engine.Builders
{
    public static class LambdaTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow, TInput, TOutput>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, LambdaTaskModel<TInput, TOutput>>> referrence,
            Expression<Func<TWorkflow, TInput>> input,
            string script
        )
            where TWorkflow : ITypedWorkflow
            where TInput : IRequest<TOutput>
        {
            var taskBuilder = new LambdaTaskBuilder<TInput, TOutput>(script, referrence.Body, input.Body, builder.BuildConfiguration);

            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    public class LambdaTaskBuilder<A, B>(string script, Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration)
        : BaseTaskBuilder<A, B>(taskExpression, inputExpression, buildConfiguration)
        where A : IRequest<B>
    {
        private readonly string _script = script;

        public override WorkflowTask[] Build()
        {
            _inputParameters.Add(new JProperty("scriptExpression", _script));
            return
            [
                new()
                {
                    Name = $"LAMBDA_{_taskRefferenceName}",
                    TaskReferenceName = _taskRefferenceName,
                    WorkflowTaskType = WorkflowTaskType.LAMBDA,
                    Type = WorkflowTaskType.LAMBDA.ToString(),
                    InputParameters = _inputParameters.ToObject<IDictionary<string, object>>()
                }
            ];
        }
    }
}
