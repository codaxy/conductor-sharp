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
        private readonly BuildConfiguration _buildConfiguration;
        private readonly WorkflowBuildItemRegistry _workflowBuildItemRegistry;
        private readonly IEnumerable<ConfigurationProperty> _configurationProperties;
        private readonly BuildContext _buildContext;

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

            _buildConfiguration = buildConfiguration;
            _workflowBuildItemRegistry = workflowBuildItemRegistry;
            _configurationProperties = configurationProperties;
            _buildContext = buildContext;
        }

        public SwitchTaskBuilder<TWorkflow> AddCase(string caseName)
        {
            _currentCaseName = caseName;

            return this;
        }

        public SwitchTaskBuilder<TWorkflow> WithTask<F, G>(
            Expression<Func<TWorkflow, LambdaTaskModel<F, G>>> taskSelector,
            Expression<Func<TWorkflow, F>> expression,
            string script
        ) where F : IRequest<G>
        {
            var builder = new LambdaTaskBuilder<F, G>(script, taskSelector.Body, expression.Body, _buildConfiguration);

            AddBuilder(builder);

            return this;
        }

        public SwitchTaskBuilder<TWorkflow> WithTask(
            Expression<Func<TWorkflow, DynamicForkJoinTaskModel>> taskSelector,
            Expression<Func<TWorkflow, DynamicForkJoinInput>> expression
        )
        {
            var builder = new DynamicForkJoinTaskBuilder(taskSelector.Body, expression.Body, _buildConfiguration);

            AddBuilder(builder);

            return this;
        }

        public SwitchTaskBuilder<TWorkflow> WithTask<F, G>(
            Expression<Func<TWorkflow, SimpleTaskModel<F, G>>> taskSelector,
            Expression<Func<TWorkflow, F>> expression,
            AdditionalTaskParameters additionalParameters = null
        ) where F : IRequest<G>
        {
            var builder = new SimpleTaskBuilder<F, G>(taskSelector.Body, expression.Body, additionalParameters, _buildConfiguration);

            AddBuilder(builder);

            return this;
        }

        public SwitchTaskBuilder<TWorkflow> WithTask<F, G>(
            Expression<Func<TWorkflow, SubWorkflowTaskModel<F, G>>> taskSelector,
            Expression<Func<TWorkflow, F>> expression
        ) where F : IRequest<G>
        {
            var builder = new SubWorkflowTaskBuilder<F, G>(taskSelector.Body, expression.Body, _buildConfiguration);

            AddBuilder(builder);

            return this;
        }

        public SwitchTaskBuilder<TWorkflow> WithTask(
            Expression<Func<TWorkflow, TerminateTaskModel>> taskSelector,
            Expression<Func<TWorkflow, TerminateTaskInput>> expression
        )
        {
            var builder = new TerminateTaskBuilder(taskSelector.Body, expression.Body, _buildConfiguration);
            AddBuilder(builder);
            return this;
        }

        public SwitchTaskBuilder<TWorkflow> WithTask<F, G>(
            Expression<Func<TWorkflow, DynamicTaskModel<F, G>>> taskSelector,
            Expression<Func<TWorkflow, DynamicTaskInput<F, G>>> expression
        ) where F : IRequest<G>
        {
            var builder = new DynamicTaskBuilder<F, G>(taskSelector.Body, expression.Body, _buildConfiguration);
            AddBuilder(builder);
            return this;
        }

        public SwitchTaskBuilder<TWorkflow> WithTask<TInput, TOutput>(
            Expression<Func<TWorkflow, CSharpLambdaTaskModel<TInput, TOutput>>> task,
            Expression<Func<TWorkflow, TInput>> input,
            Func<TInput, TOutput> lambda
        ) where TInput : IRequest<TOutput>
        {
            // TODO: Same logic already contained in corresponding AddTask, find solution to avoid duplication
            var lambdaTaskNamePrefix = (string)
                _configurationProperties.First(prop => prop.Key == CSharpLambdaTaskHandler.LambdaTaskNameConfigurationProperty).Value;
            var taskBuilder = new CSharpLambdaTaskBuilder<TInput, TOutput>(
                task.Body,
                input.Body,
                _buildConfiguration,
                lambdaTaskNamePrefix,
                _buildContext.WorkflowName
            );
            _workflowBuildItemRegistry.Register<TWorkflow>(
                taskBuilder.LambdaIdentifer,
                new CSharpLambdaHandler(taskBuilder.LambdaIdentifer, typeof(TInput), lambda)
            );

            AddBuilder(taskBuilder);
            return this;
        }

        //public SwitchTaskBuilder<TWorkflow> WithTask<F>(
        //    Expression<Func<TWorkflow, SwitchTaskModel>> taskSelector,
        //    Expression<Func<TWorkflow, F>> expression,
        //    params (string, Action<SwitchTaskBuilder<TWorkflow>>)[] caseActions
        //) where F : IRequest<NoOutput>
        //{
        //    var builder = new SwitchTaskBuilder<TWorkflow>(taskSelector.Body, expression.Body);

        //    foreach (var funcase in caseActions)
        //    {
        //        builder.AddCase(funcase.Item1);
        //        funcase.Item2.Invoke(builder);
        //    }

        //    _caseDictionary.Add(_currentCaseName, builder);

        //    return this;
        //}

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
