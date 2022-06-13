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
    public class SwitchTaskBuilder<TWorkflow> : BaseTaskBuilder<SwitchTaskInput, NoOutput>
        where TWorkflow : ITypedWorkflow
    {
        private Dictionary<string, ICollection<ITaskBuilder>> _caseDictionary = new();

        private string _currentCaseName;

        public SwitchTaskBuilder(Expression taskExpression, Expression inputExpression)
            : base(taskExpression, inputExpression) { }

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
            var builder = new LambdaTaskBuilder<F, G>(script, taskSelector.Body, expression.Body);

            AddBuilder(builder);

            return this;
        }

        public SwitchTaskBuilder<TWorkflow> WithTask(
            Expression<Func<TWorkflow, DynamicForkJoinTaskModel>> taskSelector,
            Expression<Func<TWorkflow, DynamicForkJoinInput>> expression
        )
        {
            var builder = new DynamicForkJoinTaskBuilder(taskSelector.Body, expression.Body);

            AddBuilder(builder);

            return this;
        }

        public SwitchTaskBuilder<TWorkflow> WithTask<F, G>(
            Expression<Func<TWorkflow, SimpleTaskModel<F, G>>> taskSelector,
            Expression<Func<TWorkflow, F>> expression,
            AdditionalTaskParameters additionalParameters = null
        ) where F : IRequest<G>
        {
            var builder = new SimpleTaskBuilder<F, G>(taskSelector.Body, expression.Body, additionalParameters);

            AddBuilder(builder);

            return this;
        }

        public SwitchTaskBuilder<TWorkflow> WithTask<F, G>(
            Expression<Func<TWorkflow, SubWorkflowTaskModel<F, G>>> taskSelector,
            Expression<Func<TWorkflow, F>> expression
        ) where F : IRequest<G>
        {
            var builder = new SubWorkflowTaskBuilder<F, G>(taskSelector.Body, expression.Body);

            AddBuilder(builder);

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
                    DecisionCases = new Newtonsoft.Json.Linq.JObject
                    {
                        _caseDictionary.Select(
                            a =>
                                new JProperty(
                                    a.Key,
                                    JArray.FromObject(a.Value.SelectMany(b => b.Build()))
                                )
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
    }
}
