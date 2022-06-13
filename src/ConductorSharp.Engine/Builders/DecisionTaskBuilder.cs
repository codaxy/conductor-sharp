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

    public class DecisionTaskBuilder<TWorkflow> : BaseTaskBuilder<DecisionTaskInput, NoOutput>
        where TWorkflow : ITypedWorkflow
    {
        private Dictionary<string, List<ITaskBuilder>> _caseDictionary = new();

        private string _currentCaseName;

        public DecisionTaskBuilder(Expression taskExpression, Expression inputExpression)
            : base(taskExpression, inputExpression) { }

        public DecisionTaskBuilder<TWorkflow> AddCase(string caseName)
        {
            _currentCaseName = caseName;

            if (!_caseDictionary.ContainsKey(_currentCaseName))
                _caseDictionary.Add(caseName, new List<ITaskBuilder>());

            return this;
        }

        public DecisionTaskBuilder<TWorkflow> WithTask<F, G>(
            Expression<Func<TWorkflow, LambdaTaskModel<F, G>>> taskSelector,
            Expression<Func<TWorkflow, F>> expression,
            string script
        ) where F : IRequest<G>
        {
            var builder = new LambdaTaskBuilder<F, G>(script, taskSelector.Body, expression.Body);
            _caseDictionary[_currentCaseName].Add(builder);

            return this;
        }

        public DecisionTaskBuilder<TWorkflow> WithTask(
            Expression<Func<TWorkflow, DynamicForkJoinTaskModel>> taskSelector,
            Expression<Func<TWorkflow, DynamicForkJoinInput>> expression
        )
        {
            var builder = new DynamicForkJoinTaskBuilder(taskSelector.Body, expression.Body);
            _caseDictionary[_currentCaseName].Add(builder);

            return this;
        }

        public DecisionTaskBuilder<TWorkflow> WithTask<F, G>(
            Expression<Func<TWorkflow, SimpleTaskModel<F, G>>> taskSelector,
            Expression<Func<TWorkflow, F>> expression,
            AdditionalTaskParameters additionalParameters = null
        ) where F : IRequest<G>
        {
            var builder = new SimpleTaskBuilder<F, G>(taskSelector.Body, expression.Body, additionalParameters);
            _caseDictionary[_currentCaseName].Add(builder);

            return this;
        }

        public DecisionTaskBuilder<TWorkflow> WithTask<F>(
            Expression<Func<TWorkflow, DecisionTaskModel>> taskSelector,
            Expression<Func<TWorkflow, F>> expression,
            params (string, Action<DecisionTaskBuilder<TWorkflow>>)[] caseActions
        ) where F : IRequest<NoOutput>
        {
            var builder = new DecisionTaskBuilder<TWorkflow>(taskSelector.Body, expression.Body);

            foreach (var funcase in caseActions)
            {
                builder.AddCase(funcase.Item1);
                funcase.Item2.Invoke(builder);
            }

            _caseDictionary[_currentCaseName].Add(builder);

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
                    DecisionCases = new Newtonsoft.Json.Linq.JObject
                    {
                        _caseDictionary.Select(
                            a =>
                                new JProperty(
                                    a.Key,
                                    JArray.FromObject(a.Value.SelectMany(a => a.Build()))
                                )
                        )
                    }
                }
            };
        }
    }
}