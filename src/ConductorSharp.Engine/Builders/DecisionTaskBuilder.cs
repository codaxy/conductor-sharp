using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Builders
{
    public class DecisionTaskBuilder<TWorkflow> : BaseTaskBuilder<DecisionTaskInput, NoOutput> where TWorkflow : ITypedWorkflow
    {
        private readonly Dictionary<string, List<ITaskBuilder>> _caseDictionary = new();
        private string _currentCaseName;
        private List<ITaskBuilder> _defaultCaseBuilders;

        public DecisionTaskBuilder(Expression taskExpression, Expression inputExpression) : base(taskExpression, inputExpression) { }

        internal DecisionTaskBuilder<TWorkflow> AddCase(string caseName)
        {
            // caseName == null => Specify default case
            _currentCaseName = caseName;

            return this;
        }

        internal DecisionTaskBuilder<TWorkflow> AddDefaultCase() => AddCase(null);

        public DecisionTaskBuilder<TWorkflow> WithTask<F, G>(
            Expression<Func<TWorkflow, SubWorkflowTaskModel<F, G>>> referrence,
            Expression<Func<TWorkflow, F>> input
        ) where F : IRequest<G>
        {
            var builder = new SubWorkflowTaskBuilder<F, G>(referrence.Body, input.Body);
            GetBuilderListForCurrentCase().Add(builder);

            return this;
        }

        public DecisionTaskBuilder<TWorkflow> WithTask<F, G>(
            Expression<Func<TWorkflow, LambdaTaskModel<F, G>>> taskSelector,
            Expression<Func<TWorkflow, F>> expression,
            string script
        ) where F : IRequest<G>
        {
            var builder = new LambdaTaskBuilder<F, G>(script, taskSelector.Body, expression.Body);
            GetBuilderListForCurrentCase().Add(builder);

            return this;
        }

        public DecisionTaskBuilder<TWorkflow> WithTask(
            Expression<Func<TWorkflow, DynamicForkJoinTaskModel>> taskSelector,
            Expression<Func<TWorkflow, DynamicForkJoinInput>> expression
        )
        {
            var builder = new DynamicForkJoinTaskBuilder(taskSelector.Body, expression.Body);
            GetBuilderListForCurrentCase().Add(builder);

            return this;
        }

        public DecisionTaskBuilder<TWorkflow> WithTask<F, G>(
            Expression<Func<TWorkflow, SimpleTaskModel<F, G>>> taskSelector,
            Expression<Func<TWorkflow, F>> expression,
            AdditionalTaskParameters additionalParameters = null
        ) where F : IRequest<G>
        {
            var builder = new SimpleTaskBuilder<F, G>(taskSelector.Body, expression.Body, additionalParameters);
            GetBuilderListForCurrentCase().Add(builder);

            return this;
        }

        public DecisionTaskBuilder<TWorkflow> WithTask(
            Expression<Func<TWorkflow, TerminateTaskModel>> taskSelector,
            Expression<Func<TWorkflow, TerminateTaskInput>> expression
        )
        {
            var builder = new TerminateTaskBuilder(taskSelector.Body, expression.Body);
            GetBuilderListForCurrentCase().Add(builder);

            return this;
        }

        [Obsolete("Use DecisionCases<TWorkflow> overload")]
        public DecisionTaskBuilder<TWorkflow> WithTask(
            Expression<Func<TWorkflow, DecisionTaskModel>> taskSelector,
            Expression<Func<TWorkflow, DecisionTaskInput>> expression,
            params (string, Action<DecisionTaskBuilder<TWorkflow>>)[] caseActions
        )
        {
            var builder = new DecisionTaskBuilder<TWorkflow>(taskSelector.Body, expression.Body);

            foreach (var funcase in caseActions)
            {
                builder.AddCase(funcase.Item1);
                funcase.Item2.Invoke(builder);
            }

            GetBuilderListForCurrentCase().Add(builder);

            return this;
        }

        public DecisionTaskBuilder<TWorkflow> WithTask(
            Expression<Func<TWorkflow, DecisionTaskModel>> taskSelector,
            Expression<Func<TWorkflow, DecisionTaskInput>> expression,
            DecisionCases<TWorkflow> decisionCases
        )
        {
            var builder = new DecisionTaskBuilder<TWorkflow>(taskSelector.Body, expression.Body);

            foreach (var @case in decisionCases.Cases)
            {
                builder.AddCase(@case.Key);
                @case.Value(builder);
            }

            if (decisionCases.DefaultCase != null)
            {
                builder.AddDefaultCase();
                decisionCases.DefaultCase(builder);
            }

            GetBuilderListForCurrentCase().Add(builder);

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
                        _caseDictionary.Select(a => new JProperty(a.Key, JArray.FromObject(a.Value.SelectMany(a => a.Build()))))
                    },
                    DefaultCase = _defaultCaseBuilders?.SelectMany(taskBuilder => taskBuilder.Build()).Select(JObject.FromObject).ToList()
                }
            };
        }

        private List<ITaskBuilder> GetBuilderListForCurrentCase()
        {
            if (_currentCaseName == null)
            {
                if (_defaultCaseBuilders == null)
                    _defaultCaseBuilders = new();

                return _defaultCaseBuilders;
            }

            if (!_caseDictionary.ContainsKey(_currentCaseName))
                _caseDictionary.Add(_currentCaseName, new());

            return _caseDictionary[_currentCaseName];
        }
    }
}
