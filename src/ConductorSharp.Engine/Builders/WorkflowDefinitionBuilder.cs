using ConductorSharp.Client.Model.Common;
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

        private List<ITaskBuilder> _taskBuilders = new();
        private readonly List<CSharpLambda> _lambdas = new();

        internal CSharpLambda[] Lambdas => _lambdas.ToArray();

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
                Tasks = _taskBuilders.SelectMany(a => a.Build()).ToList(),
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

        public ITaskOptionsBuilder AddTask<F, G>(
            Expression<Func<TWorkflow, LambdaTaskModel<F, G>>> referrence,
            Expression<Func<TWorkflow, F>> input,
            string script
        ) where F : IRequest<G> => AddAndReturnBuilder(new LambdaTaskBuilder<F, G>(script, referrence.Body, input.Body));

        public ITaskOptionsBuilder AddTask<F, G>(
            Expression<Func<TWorkflow, SubWorkflowTaskModel<F, G>>> referrence,
            Expression<Func<TWorkflow, F>> input
        ) where F : IRequest<G> => AddAndReturnBuilder(new SubWorkflowTaskBuilder<F, G>(referrence.Body, input.Body));

        public ITaskOptionsBuilder AddTask(
            Expression<Func<TWorkflow, DynamicForkJoinTaskModel>> refference,
            Expression<Func<TWorkflow, DynamicForkJoinInput>> input
        ) => AddAndReturnBuilder(new DynamicForkJoinTaskBuilder(refference.Body, input.Body));

        public void AddTasks(params WorkflowDefinition.Task[] taskDefinitions) => _taskBuilders.Add(new PassThroughTaskBuilder(taskDefinitions));

        public ITaskOptionsBuilder AddTask<F, G>(
            Expression<Func<TWorkflow, SimpleTaskModel<F, G>>> refference,
            Expression<Func<TWorkflow, F>> input,
            AdditionalTaskParameters additionalParameters = null
        ) where F : IRequest<G> => AddAndReturnBuilder(new SimpleTaskBuilder<F, G>(refference.Body, input.Body, additionalParameters));

        public ITaskOptionsBuilder AddTask(
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

            _taskBuilders.Add(builder);
            return builder;
        }

        public ITaskOptionsBuilder AddTask(
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

            _taskBuilders.Add(builder);
            return builder;
        }

        public ITaskOptionsBuilder AddTask<F, G>(
            Expression<Func<TWorkflow, JsonJqTransformTaskModel<F, G>>> refference,
            Expression<Func<TWorkflow, F>> input
        ) where F : IRequest<G> => AddAndReturnBuilder(new JsonJqTransformTaskBuilder<F, G>(refference.Body, input.Body));

        public ITaskOptionsBuilder AddTask<F, G>(
            Expression<Func<TWorkflow, DynamicTaskModel<F, G>>> reference,
            Expression<Func<TWorkflow, DynamicTaskInput<F, G>>> input
        ) => AddAndReturnBuilder(new DynamicTaskBuilder<F, G>(reference.Body, input.Body));

        public ITaskOptionsBuilder AddTask(
            Expression<Func<TWorkflow, TerminateTaskModel>> reference,
            Expression<Func<TWorkflow, TerminateTaskInput>> input
        ) => AddAndReturnBuilder(new TerminateTaskBuilder(reference.Body, input.Body));

        public ITaskOptionsBuilder AddTask<TInput, TOutput>(
            Expression<Func<TWorkflow, CSharpLambdaTaskModel<TInput, TOutput>>> reference,
            Expression<Func<TWorkflow, TInput>> input,
            Func<TInput, TOutput> handler
        ) where TInput : IRequest<TOutput>
        {
            var builder = new CSharpLambdaTaskBuilder<TInput, TOutput>(reference, input, _name);
            _lambdas.Add(new CSharpLambda(builder.LambdaIdentifier, handler, typeof(TInput)));
            return AddAndReturnBuilder(builder);
        }

        private ITaskOptionsBuilder AddAndReturnBuilder<T>(T builder) where T : ITaskOptionsBuilder, ITaskBuilder
        {
            _taskBuilders.Add(builder);
            return builder;
        }
    }
}
