using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ConductorSharp.Client;
using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.Engine.Builders
{
    public static class DecisionTaskExtensions
    {
        [Obsolete("Switch tasks should be used")]
        public static ITaskOptionsBuilder AddTask<TWorkflow>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, DecisionTaskModel>> taskSelector,
            Expression<Func<TWorkflow, DecisionTaskInput>> expression,
            DecisionCases<TWorkflow> decisionCases
        )
            where TWorkflow : ITypedWorkflow
        {
            var taskBuilder = new DecisionTaskBuilder<TWorkflow>(
                taskSelector.Body,
                expression.Body,
                builder.BuildConfiguration,
                builder.WorkflowBuildRegistry,
                builder.ConfigurationProperties,
                builder.BuildContext
            );

            foreach (var @case in decisionCases.Cases)
            {
                taskBuilder.AddCase(@case.Key);
                @case.Value(taskBuilder);
            }

            // Default case
            if (decisionCases.DefaultCase != null)
            {
                taskBuilder.AddCase(null);
                decisionCases.DefaultCase(taskBuilder);
            }

            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    public class DecisionTaskBuilder<TWorkflow>(
        Expression taskExpression,
        Expression inputExpression,
        BuildConfiguration buildConfiguration,
        WorkflowBuildItemRegistry buildItemRegistry,
        IEnumerable<ConfigurationProperty> configurationProperties,
        BuildContext buildContext
    ) : BaseTaskBuilder<DecisionTaskInput, NoOutput>(taskExpression, inputExpression, buildConfiguration), ITaskSequenceBuilder<TWorkflow>
        where TWorkflow : ITypedWorkflow
    {
        private readonly Dictionary<string, List<ITaskBuilder>> _caseDictionary = [];
        private List<ITaskBuilder> _defaultCase;
        private string _currentCaseName;

        public BuildContext BuildContext { get; } = buildContext;
        public BuildConfiguration BuildConfiguration { get; } = buildConfiguration;
        public WorkflowBuildItemRegistry WorkflowBuildRegistry { get; } = buildItemRegistry;
        public IEnumerable<ConfigurationProperty> ConfigurationProperties { get; } = configurationProperties;

        internal DecisionTaskBuilder<TWorkflow> AddCase(string caseName)
        {
            // null specifies default case
            _currentCaseName = caseName;
            return this;
        }

        public override WorkflowTask[] Build()
        {
            var decisionTaskName = $"DECISION_{_taskRefferenceName}";

#pragma warning disable CS0612 // Type or member is obsolete
            return
            [
                new WorkflowTask()
                {
                    Name = decisionTaskName,
                    TaskReferenceName = _taskRefferenceName,
                    InputParameters = _inputParameters.ToObject<IDictionary<string, object>>(),
                    WorkflowTaskType = WorkflowTaskType.DECISION,
                    Type = WorkflowTaskType.DECISION.ToString(),
                    CaseValueParam = "case_value_param",
                    DecisionCases = new JObject
                    {
                        _caseDictionary.Select(
                            a => new JProperty(a.Key, JArray.FromObject(a.Value.SelectMany(a => a.Build()), ConductorConstants.DefinitionsSerializer))
                        )
                    }.ToObject<IDictionary<string, ICollection<WorkflowTask>>>(),
                    DefaultCase = _defaultCase?.SelectMany(builder => builder.Build()).ToArray()
                }
            ];
#pragma warning restore CS0612 // Type or member is obsolete
        }

        public void AddTaskBuilderToSequence(ITaskBuilder builder)
        {
            // Handle default case
            if (_currentCaseName == null)
            {
                _defaultCase ??= [];
                _defaultCase.Add(builder);

                return;
            }

            if (!_caseDictionary.ContainsKey(_currentCaseName))
            {
                _caseDictionary.Add(_currentCaseName, []);
            }

            _caseDictionary[_currentCaseName].Add(builder);
        }
    }
}
