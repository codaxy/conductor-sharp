using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Builders.Configurable
{
    public static class PassThroughTaskExtensions
    {
        public static void AddTasks<TWorkflow, TInput, TOutput>(
            this WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> builder,
            params WorkflowDefinition.Task[] taskDefinitions
        )
            where TWorkflow : Workflow<TWorkflow, TInput, TOutput>
            where TInput : WorkflowInput<TOutput>
            where TOutput : WorkflowOutput => builder.BuildContext.TaskBuilders.Add(new PassThroughTaskBuilder(taskDefinitions));
    }

    public class PassThroughTaskBuilder : ITaskBuilder
    {
        private readonly WorkflowDefinition.Task[] _tasks;

        public PassThroughTaskBuilder(WorkflowDefinition.Task[] tasks)
        {
            _tasks = tasks;
        }

        public WorkflowDefinition.Task[] Build() => _tasks;
    }
}
