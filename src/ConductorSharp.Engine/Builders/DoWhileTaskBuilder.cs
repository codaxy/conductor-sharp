using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;

namespace ConductorSharp.Engine.Builders
{
    /// <summary>
    /// Extension methods to add a DO_WHILE loop with multiple inner tasks, based on BaseTaskBuilder.
    /// </summary>
    public static class DoWhileTaskExtensions
    {
        /// <summary>
        /// Adds a DO_WHILE loop to the workflow definition.
        /// </summary>
        /// <typeparam name="TWorkflow">Workflow type</typeparam>
        /// <param name="builder">Parent sequence builder</param>
        /// <param name="reference">Expression selecting the workflow property holding the loop's reference name</param>
        /// <param name="input">Expression creating the input</param>
        /// <param name="configureBody">Delegate to add tasks inside the loop</param>
        public static ITaskOptionsBuilder AddTask<TWorkflow>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, DoWhileTaskModel>> reference,
            Expression<Func<TWorkflow, DoWhileInput>> input,
            Action<ITaskSequenceBuilder<TWorkflow>> configureBody
        )
            where TWorkflow : ITypedWorkflow
        {
            // Extract expressions
            var taskBuilder = new DoWhileTaskBuilder<TWorkflow>(reference.Body, input.Body, builder.BuildConfiguration);
            // Register it with the outer sequence
            builder.AddTaskBuilderToSequence(taskBuilder);
            // Allow the caller to configure inner loop body tasks
            configureBody(taskBuilder);
            return taskBuilder;
        }
    }

    /// <summary>
    /// Builder for the DO_WHILE task, extending BaseTaskBuilder for consistent patterns.
    /// </summary>
    internal sealed class DoWhileTaskBuilder<TWorkflow> : BaseTaskBuilder<DoWhileInput, NoOutput>, ITaskSequenceBuilder<TWorkflow>
        where TWorkflow : ITypedWorkflow
    {
        private readonly DoWhileInput _doWhileInput;
        private readonly List<WorkflowTask> _innerTasks = new();
        public BuildContext BuildContext { get; } = new();
        public BuildConfiguration BuildConfiguration { get; }
        public WorkflowBuildItemRegistry WorkflowBuildRegistry { get; } = new();
        public IEnumerable<ConfigurationProperty> ConfigurationProperties { get; } = new List<ConfigurationProperty>();

        /// <inheritdoc />
        public DoWhileTaskBuilder(Expression taskExpression, Expression loopConditionExpression, BuildConfiguration buildConfiguration)
            : base(taskExpression, loopConditionExpression, buildConfiguration)
        {
            BuildConfiguration = buildConfiguration;
            // Compile the JS condition string once
            _doWhileInput = Expression.Lambda<Func<DoWhileInput>>(loopConditionExpression).Compile().Invoke();
        }

        /// <inheritdoc/>
        public override WorkflowTask[] Build()
        {
            var refName = _taskRefferenceName;

            if (_innerTasks.Any(t => t.WorkflowTaskType == WorkflowTaskType.DO_WHILE))
            {
                throw new InvalidOperationException("Nested DO_WHILE tasks are not allowed.");
            }

            var loopTask = new WorkflowTask
            {
                Name = refName,
                TaskReferenceName = refName,
                WorkflowTaskType = WorkflowTaskType.DO_WHILE,
                Type = nameof(WorkflowTaskType.DO_WHILE),
                InputParameters = new Dictionary<string, object>(),
                LoopCondition = _doWhileInput.LoopCondition,
                LoopOver = _innerTasks,
            };
            return [loopTask];
        }

        public void AddTaskBuilderToSequence(ITaskBuilder builder)
        {
            foreach (var task in builder.Build())
            {
                _innerTasks.Add(task);
            }
        }
    }
}
