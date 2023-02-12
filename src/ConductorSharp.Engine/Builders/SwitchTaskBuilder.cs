using ConductorSharp.Client;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Handlers;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Builders
{
    public static class SwitchTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, SwitchTaskModel>> taskSelector,
            Expression<Func<TWorkflow, SwitchTaskInput>> expression,
            params (string, Action<SwitchTaskBuilder<TWorkflow>>)[] caseActions
        ) where TWorkflow : ITypedWorkflow
        {
            var taskBuilder = new SwitchTaskBuilder<TWorkflow>(
                taskSelector.Body,
                expression.Body,
                builder.BuildConfiguration,
                builder.WorkflowBuildRegistry,
                builder.ConfigurationProperties,
                builder.BuildContext
            );

            foreach (var funcase in caseActions)
            {
                taskBuilder.AddCase(funcase.Item1);
                funcase.Item2.Invoke(taskBuilder);
            }

            builder.AddTaskBuilderToSequence(taskBuilder);
            return taskBuilder;
        }
    }

    public class SwitchTaskBuilder<TWorkflow> : BaseTaskBuilder<SwitchTaskInput, NoOutput>, ITaskSequenceBuilder<TWorkflow>
        where TWorkflow : ITypedWorkflow
    {
        private Dictionary<string, ICollection<ITaskBuilder>> _caseDictionary = new();
        private string _currentCaseName;

        public BuildContext BuildContext { get; }
        public BuildConfiguration BuildConfiguration { get; }
        public WorkflowBuildItemRegistry WorkflowBuildRegistry { get; }
        public IEnumerable<ConfigurationProperty> ConfigurationProperties { get; }

        public SwitchTaskBuilder(
            Expression taskExpression,
            Expression inputExpression,
            BuildConfiguration buildConfiguration,
            WorkflowBuildItemRegistry workflowBuildItemRegistry,
            IEnumerable<ConfigurationProperty> configurationProperties,
            BuildContext buildContext
        ) : base(taskExpression, inputExpression, buildConfiguration)
        {
            BuildConfiguration = buildConfiguration;
            WorkflowBuildRegistry = workflowBuildItemRegistry;
            ConfigurationProperties = configurationProperties;
            BuildContext = buildContext;
        }

        public SwitchTaskBuilder<TWorkflow> AddCase(string caseName)
        {
            _currentCaseName = caseName;

            return this;
        }

        public override WorkflowDefinition.Task[] Build()
        {
            var decisionTaskName = $"SWITCH_{_taskRefferenceName}";

            return new WorkflowDefinition.Task[]
            {
                new WorkflowDefinition.Task
                {
                    Name = decisionTaskName,
                    TaskReferenceName = _taskRefferenceName,
                    InputParameters = _inputParameters,
                    Type = "SWITCH",
                    Expression = "switch_case_value",
                    EvaluatorType = "value-param",
                    DecisionCases = new JObject
                    {
                        _caseDictionary.Select(
                            a => new JProperty(a.Key, JArray.FromObject(a.Value.SelectMany(b => b.Build()), ConductorConstants.DefinitionsSerializer))
                        )
                    }
                }
            };
        }

        private void AddBuilder(ITaskBuilder builder)
        {
            if (_caseDictionary.ContainsKey(_currentCaseName))
            {
                _caseDictionary[_currentCaseName].Add(builder);
            }
            else
            {
                _caseDictionary.Add(_currentCaseName, new List<ITaskBuilder>() { builder });
            }
        }

        public void AddTaskBuilderToSequence(ITaskBuilder builder) => AddBuilder(builder);
    }
}
