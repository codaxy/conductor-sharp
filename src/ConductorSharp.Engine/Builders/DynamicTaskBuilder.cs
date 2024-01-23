using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.Engine.Builders
{
    public static class DynamicTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow, TInput, TOutput>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, DynamicTaskModel<TInput, TOutput>>> reference,
            Expression<Func<TWorkflow, DynamicTaskInput<TInput, TOutput>>> input
        )
            where TWorkflow : ITypedWorkflow
            where TInput : IRequest<TOutput>
        {
            var taskBuilder = new DynamicTaskBuilder<TInput, TOutput>(reference.Body, input.Body, builder.BuildConfiguration);
            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    public class DynamicTaskBuilder<I, O>(Expression taskExpression, Expression inputExpression, BuildConfiguration buildConfiguration)
        : BaseTaskBuilder<DynamicTaskInput<I, O>, O>(taskExpression, inputExpression, buildConfiguration)
    {
        private const string DynamicTasknameParam = "task_to_execute";

        private class DynamicTaskParameters
        {
            [JsonProperty("task_input")]
            public JObject TaskInput { get; set; }

            [JsonProperty(DynamicTasknameParam)]
            public string TaskToExecute { get; set; }
        }

        public override WorkflowTask[] Build()
        {
            var parameters = _inputParameters.ToObject<DynamicTaskParameters>();

            parameters.TaskInput.Add(new JProperty(DynamicTasknameParam, parameters.TaskToExecute));

            return
            [
                new()
                {
                    Name = _taskRefferenceName,
                    TaskReferenceName = _taskRefferenceName,
                    WorkflowTaskType = WorkflowTaskType.DYNAMIC,
                    Type = WorkflowTaskType.DYNAMIC.ToString(),
                    InputParameters = parameters.TaskInput.ToObject<IDictionary<string, object>>(),
                    DynamicTaskNameParam = DynamicTasknameParam,
                }
            ];
        }
    }
}
