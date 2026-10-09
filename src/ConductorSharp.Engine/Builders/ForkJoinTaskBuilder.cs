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
    public static class ForkJoinTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, ForkJoinTaskModel>> reference,
            Expression<Func<TWorkflow, ForkJoinInput>> input,
            params Action<ITaskSequenceBuilder<TWorkflow>>[] branches
        )
            where TWorkflow : ITypedWorkflow
        {
            if (branches == null || branches.Length == 0)
                throw new InvalidOperationException("FORK_JOIN task requires at least one branch.");

            var taskBuilder = new ForkJoinTaskBuilder<TWorkflow>(
                reference.Body,
                input.Body,
                builder.BuildConfiguration,
                builder.WorkflowBuildRegistry,
                builder.ConfigurationProperties,
                builder.BuildContext
            );

            foreach (var branch in branches)
            {
                taskBuilder.AddBranch();
                branch(taskBuilder);
            }

            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    public class ForkJoinTaskBuilder<TWorkflow>(
        Expression taskExpression,
        Expression inputExpression,
        BuildConfiguration buildConfiguration,
        WorkflowBuildItemRegistry workflowBuildItemRegistry,
        IEnumerable<ConfigurationProperty> configurationProperties,
        BuildContext buildContext
    ) : BaseTaskBuilder<ForkJoinInput, NoOutput>(taskExpression, inputExpression, buildConfiguration), ITaskSequenceBuilder<TWorkflow>
        where TWorkflow : ITypedWorkflow
    {
        private readonly List<List<ITaskBuilder>> _branches = [];

        public BuildContext BuildContext { get; } = buildContext;
        public BuildConfiguration BuildConfiguration { get; } = buildConfiguration;
        public WorkflowBuildItemRegistry WorkflowBuildRegistry { get; } = workflowBuildItemRegistry;
        public IEnumerable<ConfigurationProperty> ConfigurationProperties { get; } = configurationProperties;

        public void AddBranch() => _branches.Add([]);

        public override WorkflowTask[] Build()
        {
            var forkTaskName = $"FORK_JOIN_{_taskRefferenceName}";
            var joinTaskName = $"JOIN_{_taskRefferenceName}";

            var builtBranches = _branches
                .Select(branch => (ICollection<WorkflowTask>)branch.SelectMany(taskBuilder => taskBuilder.Build()).ToList())
                .ToList();

            for (var i = 0; i < builtBranches.Count; i++)
            {
                if (builtBranches[i].Count == 0)
                    throw new InvalidOperationException($"FORK_JOIN branch {i} must contain at least one task.");
            }

            var joinOn = builtBranches.Select(branch => branch.Last().TaskReferenceName).ToList();

            return
            [
                new()
                {
                    Name = forkTaskName,
                    TaskReferenceName = forkTaskName,
                    WorkflowTaskType = WorkflowTaskType.FORK_JOIN,
                    Type = WorkflowTaskType.FORK_JOIN.ToString(),
                    InputParameters = _inputParameters.ToObject<IDictionary<string, object>>(),
                    ForkTasks = builtBranches,
                },
                new()
                {
                    Name = joinTaskName,
                    TaskReferenceName = joinTaskName,
                    WorkflowTaskType = WorkflowTaskType.JOIN,
                    Type = WorkflowTaskType.JOIN.ToString(),
                    JoinOn = joinOn,
                },
            ];
        }

        public void AddTaskBuilderToSequence(ITaskBuilder builder)
        {
            if (_branches.Count == 0)
                throw new InvalidOperationException("Cannot add a task to a FORK_JOIN branch before the branch has been started.");

            _branches[^1].Add(builder);
        }
    }
}
