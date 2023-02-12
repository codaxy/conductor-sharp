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
    public static class DecisionTaskExtensions
    {
        public static ITaskOptionsBuilder AddTask<TWorkflow>(
            this ITaskSequenceBuilder<TWorkflow> builder,
            Expression<Func<TWorkflow, DecisionTaskModel>> taskSelector,
            Expression<Func<TWorkflow, DecisionTaskInput>> expression,
            params (string, Action<DecisionTaskBuilder<TWorkflow>>)[] caseActions
        ) where TWorkflow : ITypedWorkflow
        {
            var taskBuilder = new DecisionTaskBuilder<TWorkflow>(
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

    public class DecisionTaskBuilder<TWorkflow> : BaseTaskBuilder<DecisionTaskInput, NoOutput>, ITaskSequenceBuilder<TWorkflow>
        where TWorkflow : ITypedWorkflow
    {
        private Dictionary<string, List<ITaskBuilder>> _caseDictionary = new();
        private string _currentCaseName;

        public BuildContext BuildContext { get; }
        public BuildConfiguration BuildConfiguration { get; }
        public WorkflowBuildItemRegistry WorkflowBuildRegistry { get; }
        public IEnumerable<ConfigurationProperty> ConfigurationProperties { get; }

        public DecisionTaskBuilder(
            Expression taskExpression,
            Expression inputExpression,
            BuildConfiguration buildConfiguration,
            WorkflowBuildItemRegistry buildItemRegistry,
            IEnumerable<ConfigurationProperty> configurationProperties,
            BuildContext buildContext
        ) : base(taskExpression, inputExpression, buildConfiguration)
        {
            BuildConfiguration = buildConfiguration;
            BuildContext = buildContext;
            WorkflowBuildRegistry = buildItemRegistry;
            ConfigurationProperties = configurationProperties;
        }

        public DecisionTaskBuilder<TWorkflow> AddCase(string caseName)
        {
            _currentCaseName = caseName;

            if (!_caseDictionary.ContainsKey(_currentCaseName))
                _caseDictionary.Add(caseName, new List<ITaskBuilder>());

            return this;
        }

        public override WorkflowDefinition.Task[] Build()
        {
            var decisionTaskName = $"DECISION_{_taskRefferenceName}";

            return new WorkflowDefinition.Task[]
            {
                new WorkflowDefinition.Task
                {
                    Name = decisionTaskName,
                    TaskReferenceName = _taskRefferenceName,
                    InputParameters = _inputParameters,
                    Type = "DECISION",
                    CaseValueParam = "case_value_param",
                    DecisionCases = new JObject
                    {
                        _caseDictionary.Select(
                            a => new JProperty(a.Key, JArray.FromObject(a.Value.SelectMany(a => a.Build()), ConductorConstants.DefinitionsSerializer))
                        )
                    }
                }
            };
        }

        public void AddTaskBuilderToSequence(ITaskBuilder builder) => _caseDictionary[_currentCaseName].Add(builder);
    }
}
