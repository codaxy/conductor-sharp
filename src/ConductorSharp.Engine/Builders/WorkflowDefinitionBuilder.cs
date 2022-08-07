using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ConductorSharp.Engine.Builders
{
    public class WorkflowDefinitionBuilder<TWorkflow> where TWorkflow : ITypedWorkflow
    {
        private readonly Type _workflowType;
        private string _name;
        private JObject _inputs = new();
        private WorkflowOptions _workflowOptions;

        public List<WorkflowDefinition.Task> WorkflowTasks { get; set; } = new List<WorkflowDefinition.Task>();

        public WorkflowDefinitionBuilder()
        {
            _workflowType = typeof(TWorkflow);
            _workflowOptions = new WorkflowOptions();

            XmlDocumentationReader.LoadXmlDocumentation(_workflowType.Assembly);

            _name = NamingUtil.DetermineRegistrationName(_workflowType);

            var summary = _workflowType.GetDocSection("summary");
            var ownerApp = _workflowType.GetDocSection("ownerApp");
            var ownerEmail = _workflowType.GetDocSection("ownerEmail");
            var labels = _workflowType.GetDocSection("labels");

            _workflowOptions.Version = 1;

            if (!string.IsNullOrEmpty(summary))
                _workflowOptions.Description = summary;

            if (!string.IsNullOrEmpty(ownerApp))
                _workflowOptions.OwnerApp = ownerApp;

            if (!string.IsNullOrEmpty(ownerEmail))
                _workflowOptions.OwnerEmail = ownerEmail;

            if (!string.IsNullOrEmpty(labels))
                _workflowOptions.Labels = labels.Split(",").Select(a => a.Trim()).ToArray();

            var input = _workflowType.BaseType.GenericTypeArguments[0];
            var props = input.GetProperties();

            foreach (var prop in props)
            {
                var isRequired = prop.GetCustomAttribute<RequiredAttribute>();
                var description = prop.GetDocSection("summary");

                var propertyName = prop.GetDocSection("originalName") ?? SnakeCaseUtil.ToSnakeCase(prop.Name);

                var requiredString = isRequired != null ? "(required)" : "(optional)";
                _inputs.Add(
                    new JProperty(
                        propertyName,
                        new JObject { new JProperty("value", ""), new JProperty("description", $"{description} {requiredString}"), }
                    )
                );
            }
        }

        public WorkflowDefinition Build(Action<WorkflowOptions> adjustOptions)
        {
            adjustOptions.Invoke(_workflowOptions);

            return new WorkflowDefinition
            {
                Name = _name,
                Tasks = WorkflowTasks,
                FailureWorkflow =
                    _workflowOptions.FailureWorkflow != null ? NamingUtil.DetermineRegistrationName(_workflowOptions.FailureWorkflow) : null,
                Description = new JObject()
                {
                    new JProperty("description", _workflowOptions.Description),
                    new JProperty("labels", _workflowOptions.Labels)
                }.ToString(Formatting.None),
                InputParameters = _inputs,
                OwnerApp = _workflowOptions.OwnerApp,
                OwnerEmail = _workflowOptions.OwnerEmail,
            };
        }

        public void AddTask<F, G>(Expression<Func<TWorkflow, LambdaTaskModel<F, G>>> referrence, Expression<Func<TWorkflow, F>> input, string script)
            where F : IRequest<G>
        {
            var tasks = new LambdaTaskBuilder<F, G>(script, referrence.Body, input.Body).Build();
            AddTasks(tasks);
        }

        public void AddTask<F, G>(Expression<Func<TWorkflow, SubWorkflowTaskModel<F, G>>> referrence, Expression<Func<TWorkflow, F>> input)
            where F : IRequest<G>
        {
            var tasks = new SubWorkflowTaskBuilder<F, G>(referrence.Body, input.Body).Build();
            AddTasks(tasks);
        }

        public void AddTask(Expression<Func<TWorkflow, DynamicForkJoinTaskModel>> refference, Expression<Func<TWorkflow, DynamicForkJoinInput>> input)
        {
            var tasks = new DynamicForkJoinTaskBuilder(refference.Body, input.Body).Build();
            AddTasks(tasks);
        }

        public void AddTasks(params WorkflowDefinition.Task[] taskDefinitions) => WorkflowTasks.AddRange(taskDefinitions);

        public void AddTask<F, G>(
            Expression<Func<TWorkflow, SimpleTaskModel<F, G>>> refference,
            Expression<Func<TWorkflow, F>> input,
            AdditionalTaskParameters additionalParameters = null
        ) where F : IRequest<G>
        {
            var tasks = new SimpleTaskBuilder<F, G>(refference.Body, input.Body, additionalParameters).Build();
            AddTasks(tasks);
        }

        public void AddTask(
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

            AddTasks(builder.Build());
        }

        public void AddTask(
            Expression<Func<TWorkflow, SwitchTaskModel>> taskSelector,
            Expression<Func<TWorkflow, SwitchTaskInput>> expression,
            params (string, Action<SwitchTaskBuilder<TWorkflow>>)[] caseActions
        )
        {
            var builder = new SwitchTaskBuilder<TWorkflow>(taskSelector.Body, expression.Body);

            foreach (var funcase in caseActions)
            {
                builder.AddCase(funcase.Item1);
                funcase.Item2.Invoke(builder);
            }

            AddTasks(builder.Build());
        }

        public void AddTask<F, G>(Expression<Func<TWorkflow, JsonJqTransformTaskModel<F, G>>> refference, Expression<Func<TWorkflow, F>> input)
            where F : IRequest<G>
        {
            var tasks = new JsonJqTransformTaskBuilder<F, G>(refference.Body, input.Body).Build();
            AddTasks(tasks);
        }

        public void AddTask<TInput, TOutput>(
            Expression<Func<TWorkflow, SimpleTaskModel<TInput, TOutput>>> reference,
            Expression<Func<TWorkflow, TInput>> input,
            Func<TInput, TOutput> handlerFunc,
            AdditionalTaskParameters additionalTaskParameters = null
        ) where TInput : IRequest<TOutput>
        {
            var tasks = new CSharpLambdaSimpleTaskBuilder<TInput, TOutput>(
                reference.Body,
                input.Body,
                handlerFunc.GetHash(),
                additionalTaskParameters
            ).Build();
            AddTasks(tasks);
            DynamicHandlerBuilder.DefaultBuilder.AddDynamicHandler(handlerFunc, handlerFunc.GetHash());
        }
    }
}
