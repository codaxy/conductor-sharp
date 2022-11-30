﻿using Autofac;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static ConductorSharp.Engine.Util.Builders.Events;

namespace ConductorSharp.Engine.Builders.Configurable
{
    public static class WorkflowOutputExtension
    {
        public static void SetOutput<FWorkflow, F, G>(this WorkflowDefinitionBuilder<FWorkflow, F, G> builder, Expression<Func<FWorkflow, G>> input)
            where FWorkflow : Workflow<FWorkflow, F, G>
            where F : WorkflowInput<G>
            where G : WorkflowOutput
        {
            builder.BuildContext.Outputs = ExpressionUtil.ParseToParameters(input.Body);
            builder.OnLoad += (context) => { };
        }
    }

    public class WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput>
        where TWorkflow : ITypedWorkflow
        where TInput : WorkflowInput<TOutput>
        where TOutput : WorkflowOutput
    {
        public delegate void OnRegistration(ContainerBuilder containerBuilder);
        public event LoadWorflow OnLoad;
        public event ResolveWorkflow OnWorkflowResolve;
        public event GetWorkflowDefinition OnWorkflowGetDefinition;

        private readonly Type _workflowType = typeof(TWorkflow);
        private readonly string _name;

        public BuildContext BuildContext { get; } = new();

        public BuildConfiguration BuildConfiguration { get; set; }

        public WorkflowDefinitionBuilder()
        {
            XmlDocumentationReader.LoadXmlDocumentation(_workflowType.Assembly);
            _name = NamingUtil.DetermineRegistrationName(_workflowType);

            var summary = _workflowType.GetDocSection("summary");
            var ownerApp = _workflowType.GetDocSection("ownerApp");
            var ownerEmail = _workflowType.GetDocSection("ownerEmail");
            var labels = _workflowType.GetDocSection("labels");

            BuildContext.WorkflowOptions.Version = 1;
            BuildContext.Inputs = new();

            if (!string.IsNullOrEmpty(summary))
                BuildContext.WorkflowOptions.Description = summary;

            if (!string.IsNullOrEmpty(ownerApp))
                BuildContext.WorkflowOptions.OwnerApp = ownerApp;

            if (!string.IsNullOrEmpty(ownerEmail))
                BuildContext.WorkflowOptions.OwnerEmail = ownerEmail;

            if (!string.IsNullOrEmpty(labels))
                BuildContext.WorkflowOptions.Labels = labels.Split(",").Select(a => a.Trim()).ToArray();

            var input = _workflowType.BaseType.GenericTypeArguments[0];
            var props = input.GetProperties();

            foreach (var prop in props)
            {
                var isRequired = prop.GetCustomAttribute<RequiredAttribute>();
                var description = prop.GetDocSection("summary");

                var propertyName = prop.GetDocSection("originalName") ?? SnakeCaseUtil.ToSnakeCase(prop.Name);

                var requiredString = isRequired != null ? "(required)" : "(optional)";
                BuildContext.Inputs.Add(
                    new JProperty(
                        propertyName,
                        new JObject { new JProperty("value", ""), new JProperty("description", $"{description} {requiredString}"), }
                    )
                );
            }
        }

        public WorkflowDefinition Build()
        {
            return new WorkflowDefinition
            {
                Name = _name,
                Tasks = BuildContext.TaskBuilders.SelectMany(a => a.Build()).ToList(),
                FailureWorkflow =
                    BuildContext.WorkflowOptions.FailureWorkflow != null
                        ? NamingUtil.DetermineRegistrationName(BuildContext.WorkflowOptions.FailureWorkflow)
                        : null,
                Description = new JObject()
                {
                    new JProperty("description", BuildContext.WorkflowOptions.Description),
                    new JProperty("labels", BuildContext.WorkflowOptions.Labels)
                }.ToString(Formatting.None),
                InputParameters = BuildContext.Inputs,
                OutputParameters = BuildContext.Outputs,
                OwnerApp = BuildContext.WorkflowOptions.OwnerApp,
                OwnerEmail = BuildContext.WorkflowOptions.OwnerEmail,
            };
        }

        public void OnRegister(ContainerBuilder containerBuilder)
        {
            OnLoad?.Invoke(containerBuilder);
        }

        public void OnResolve(IComponentContext componentContext)
        {
            OnWorkflowResolve?.Invoke(componentContext);
        }

        public void OnGetDefinition(WorkflowDefinition workflowDefinition)
        {
            OnWorkflowGetDefinition?.Invoke(workflowDefinition);
        }
    }
}
