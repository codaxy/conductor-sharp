using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ConductorSharp.Engine.Builders
{
    public static class DynamicTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow, F, G>(
            this WorkflowDefinitionBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, DynamicTaskModel<F, G>>> reference,
            Expression<Func<TWorkflow, DynamicTaskInput<F, G>>> input
        ) where TWorkflow : ITypedWorkflow
        {
            var taskBuilder = new DynamicTaskBuilder<F, G>(reference.Body, input.Body);
            builder.Context.TaskBuilders.Add(taskBuilder);
            return taskBuilder;
        }
    }

    public class DynamicTaskBuilder<I, O> : BaseTaskBuilder<DynamicTaskInput<I, O>, O>
    {
        private const string TaskType = "DYNAMIC";
        private const string DynamicTasknameParam = "task_to_execute";

        public DynamicTaskBuilder(Expression taskExpression, Expression inputExpression) : base(taskExpression, inputExpression) { }

        private class DynamicTaskParameters
        {
            [JsonProperty("task_input")]
            public JObject TaskInput { get; set; }

            [JsonProperty(DynamicTasknameParam)]
            public string TaskToExecute { get; set; }
        }

        public override WorkflowDefinition.Task[] Build()
        {
            var parameters = _inputParameters.ToObject<DynamicTaskParameters>();

            parameters.TaskInput.Add(new JProperty(DynamicTasknameParam, parameters.TaskToExecute));

            return new WorkflowDefinition.Task[]
            {
                new WorkflowDefinition.Task
                {
                    Name = _taskRefferenceName,
                    TaskReferenceName = _taskRefferenceName,
                    Type = TaskType,
                    InputParameters = parameters.TaskInput,
                    DynamicTaskNameParam = DynamicTasknameParam,
                    Description = new JObject { new JProperty("description", "A dynamic task") }.ToString(Formatting.None)
                }
            };
        }
    }
}
