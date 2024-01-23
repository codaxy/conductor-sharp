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
    public static class SwitchTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, SwitchTaskModel>> taskSelector,
            Expression<Func<TWorkflow, SwitchTaskInput>> expression,
            DecisionCases<TWorkflow> decisionCases
        )
            where TWorkflow : ITypedWorkflow
        {
            var taskBuilder = new SwitchTaskBuilder<TWorkflow>(
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

            // Handle default case
            if (decisionCases.DefaultCase != null)
            {
                taskBuilder.AddCase(null);
                decisionCases.DefaultCase(taskBuilder);
            }

            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    public class SwitchTaskBuilder<TWorkflow>(
        Expression taskExpression,
        Expression inputExpression,
        BuildConfiguration buildConfiguration,
        WorkflowBuildItemRegistry workflowBuildItemRegistry,
        IEnumerable<ConfigurationProperty> configurationProperties,
        BuildContext buildContext
    ) : BaseTaskBuilder<SwitchTaskInput, NoOutput>(taskExpression, inputExpression, buildConfiguration), ITaskSequenceBuilder<TWorkflow>
        where TWorkflow : ITypedWorkflow
    {
        private readonly Dictionary<string, ICollection<ITaskBuilder>> _caseDictionary = [];
        private string _currentCaseName;
        private List<ITaskBuilder> _defaultCase;

        public BuildContext BuildContext { get; } = buildContext;
        public BuildConfiguration BuildConfiguration { get; } = buildConfiguration;
        public WorkflowBuildItemRegistry WorkflowBuildRegistry { get; } = workflowBuildItemRegistry;
        public IEnumerable<ConfigurationProperty> ConfigurationProperties { get; } = configurationProperties;

        public SwitchTaskBuilder<TWorkflow> AddCase(string caseName)
        {
            _currentCaseName = caseName;

            return this;
        }

        public override WorkflowTask[] Build()
        {
            var decisionTaskName = $"SWITCH_{_taskRefferenceName}";

            return
            [
                new()
                {
                    Name = decisionTaskName,
                    TaskReferenceName = _taskRefferenceName,
                    InputParameters = _inputParameters.ToObject<IDictionary<string, object>>(),
                    WorkflowTaskType = WorkflowTaskType.SWITCH,
                    Type = WorkflowTaskType.SWITCH.ToString(),
                    Expression = "switch_case_value",
                    EvaluatorType = "value-param",
                    DecisionCases = new JObject
                    {
                        _caseDictionary.Select(
                            a => new JProperty(a.Key, JArray.FromObject(a.Value.SelectMany(b => b.Build()), ConductorConstants.DefinitionsSerializer))
                        )
                    }.ToObject<IDictionary<string, ICollection<WorkflowTask>>>(),
                    DefaultCase = _defaultCase?.SelectMany(builder => builder.Build()).ToArray()
                }
            ];
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
                _caseDictionary.Add(_currentCaseName, new List<ITaskBuilder>());
            _caseDictionary[_currentCaseName].Add(builder);
        }
    }
}
