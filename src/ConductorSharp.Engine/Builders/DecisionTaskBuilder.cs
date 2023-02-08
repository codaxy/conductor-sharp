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
        public static ITaskOptionsBuilder AddTask<TWorkflow, TInput, TOutput>(
            this WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> builder,
            Expression<Func<TWorkflow, DecisionTaskModel>> taskSelector,
            Expression<Func<TWorkflow, DecisionTaskInput>> expression,
            params (string, Action<DecisionTaskBuilder<TWorkflow>>)[] caseActions
        )
            where TWorkflow : Workflow<TWorkflow, TInput, TOutput>
            where TInput : WorkflowInput<TOutput>
            where TOutput : WorkflowOutput
        {
            var taskBbuilder = new DecisionTaskBuilder<TWorkflow>(
                taskSelector.Body,
                expression.Body,
                builder.BuildConfiguration,
                builder.WorkflowBuildRegistry,
                builder.ConfigurationProperties
            );

            foreach (var funcase in caseActions)
            {
                taskBbuilder.AddCase(funcase.Item1);
                funcase.Item2.Invoke(taskBbuilder);
            }

            builder.BuildContext.TaskBuilders.Add(taskBbuilder);
            return taskBbuilder;
        }
    }

    public class DecisionTaskBuilder<TWorkflow> : BaseTaskBuilder<DecisionTaskInput, NoOutput> where TWorkflow : IConfigurableWorkflow
    {
        private Dictionary<string, List<ITaskBuilder>> _caseDictionary = new();

        private string _currentCaseName;
        private readonly BuildConfiguration _buildConfiguration;
        private readonly WorkflowBuildItemRegistry _workflowBuildItemRegistry;
        private readonly IEnumerable<ConfigurationProperty> _configurationProperties;

        public DecisionTaskBuilder(
            Expression taskExpression,
            Expression inputExpression,
            BuildConfiguration buildConfiguration,
            WorkflowBuildItemRegistry buildItemRegistry,
            IEnumerable<ConfigurationProperty> configurationProperties
        ) : base(taskExpression, inputExpression, buildConfiguration)
        {
            _buildConfiguration = buildConfiguration;
            _workflowBuildItemRegistry = buildItemRegistry;
            _configurationProperties = configurationProperties;
        }

        public DecisionTaskBuilder<TWorkflow> AddCase(string caseName)
        {
            _currentCaseName = caseName;

            if (!_caseDictionary.ContainsKey(_currentCaseName))
                _caseDictionary.Add(caseName, new List<ITaskBuilder>());

            return this;
        }

        public DecisionTaskBuilder<TWorkflow> WithTask<F, G>(
            Expression<Func<TWorkflow, SubWorkflowTaskModel<F, G>>> referrence,
            Expression<Func<TWorkflow, F>> input
        ) where F : IRequest<G>
        {
            var builder = new SubWorkflowTaskBuilder<F, G>(referrence.Body, input.Body, _buildConfiguration);
            _caseDictionary[_currentCaseName].Add(builder);

            return this;
        }

        public DecisionTaskBuilder<TWorkflow> WithTask<F, G>(
            Expression<Func<TWorkflow, LambdaTaskModel<F, G>>> taskSelector,
            Expression<Func<TWorkflow, F>> expression,
            string script
        ) where F : IRequest<G>
        {
            var builder = new LambdaTaskBuilder<F, G>(script, taskSelector.Body, expression.Body, _buildConfiguration);
            _caseDictionary[_currentCaseName].Add(builder);

            return this;
        }

        public DecisionTaskBuilder<TWorkflow> WithTask(
            Expression<Func<TWorkflow, DynamicForkJoinTaskModel>> taskSelector,
            Expression<Func<TWorkflow, DynamicForkJoinInput>> expression
        )
        {
            var builder = new DynamicForkJoinTaskBuilder(taskSelector.Body, expression.Body, _buildConfiguration);
            _caseDictionary[_currentCaseName].Add(builder);

            return this;
        }

        public DecisionTaskBuilder<TWorkflow> WithTask<F, G>(
            Expression<Func<TWorkflow, SimpleTaskModel<F, G>>> taskSelector,
            Expression<Func<TWorkflow, F>> expression,
            AdditionalTaskParameters additionalParameters = null
        ) where F : IRequest<G>
        {
            var builder = new SimpleTaskBuilder<F, G>(taskSelector.Body, expression.Body, additionalParameters, _buildConfiguration);
            _caseDictionary[_currentCaseName].Add(builder);

            return this;
        }

        public DecisionTaskBuilder<TWorkflow> WithTask(
            Expression<Func<TWorkflow, TerminateTaskModel>> taskSelector,
            Expression<Func<TWorkflow, TerminateTaskInput>> expression
        )
        {
            var builder = new TerminateTaskBuilder(taskSelector.Body, expression.Body, _buildConfiguration);
            _caseDictionary[_currentCaseName].Add(builder);

            return this;
        }

        public DecisionTaskBuilder<TWorkflow> WithTask<F, G>(
            Expression<Func<TWorkflow, DynamicTaskModel<F, G>>> taskSelector,
            Expression<Func<TWorkflow, DynamicTaskInput<F, G>>> expression
        ) where F : IRequest<G>
        {
            var builder = new DynamicTaskBuilder<F, G>(taskSelector.Body, expression.Body, _buildConfiguration);
            _caseDictionary[_currentCaseName].Add(builder);

            return this;
        }

        public DecisionTaskBuilder<TWorkflow> WithTask(
            Expression<Func<TWorkflow, DecisionTaskModel>> taskSelector,
            Expression<Func<TWorkflow, DecisionTaskInput>> expression,
            params (string, Action<DecisionTaskBuilder<TWorkflow>>)[] caseActions
        )
        {
            var builder = new DecisionTaskBuilder<TWorkflow>(
                taskSelector.Body,
                expression.Body,
                _buildConfiguration,
                _workflowBuildItemRegistry,
                _configurationProperties
            );

            foreach (var funcase in caseActions)
            {
                builder.AddCase(funcase.Item1);
                funcase.Item2.Invoke(builder);
            }

            _caseDictionary[_currentCaseName].Add(builder);

            return this;
        }

        public DecisionTaskBuilder<TWorkflow> WithTask<TInput, TOutput>(
            Expression<Func<TWorkflow, CSharpLambdaTaskModel<TInput, TOutput>>> task,
            Expression<Func<TWorkflow, TInput>> input,
            Func<TInput, TOutput> lambda
        ) where TInput : IRequest<TOutput>
        {
            // TODO: Same logic already contained in corresponding AddTask, find solution to avoid duplication
            var lambdaTaskNamePrefix = (string)
                _configurationProperties.First(prop => prop.Key == CSharpLambdaTaskHandler.LambdaTaskNameConfigurationProperty).Value;
            var taskBuilder = new CSharpLambdaTaskBuilder<TInput, TOutput>(task.Body, input.Body, _buildConfiguration, lambdaTaskNamePrefix);
            _workflowBuildItemRegistry.Register<TWorkflow>(
                taskBuilder.LambdaIdentifer,
                new CSharpLambdaHandler(taskBuilder.LambdaIdentifer, typeof(TInput), lambda)
            );

            _caseDictionary[_currentCaseName].Add(taskBuilder);
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
    }
}
