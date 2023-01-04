﻿using ConductorSharp.Client.Model.Common;
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
    public class DefinitionContext
    {
        public JObject Inputs { get; set; }
        public JObject Outputs { get; set; }
        public WorkflowOptions WorkflowOptions { get; } = new();

        public List<ITaskBuilder> TaskBuilders { get; } = new();
    }

    public class WorkflowDefinitionBuilder<TWorkflow> where TWorkflow : ITypedWorkflow
    {
        private readonly Type _workflowType = typeof(TWorkflow);
        private readonly string _name;

        public DefinitionContext Context { get; } = new();

        public WorkflowDefinitionBuilder()
        {
            XmlDocumentationReader.LoadXmlDocumentation(_workflowType.Assembly);
            _name = NamingUtil.DetermineRegistrationName(_workflowType);

            var summary = _workflowType.GetDocSection("summary");
            var ownerApp = _workflowType.GetDocSection("ownerApp");
            var ownerEmail = _workflowType.GetDocSection("ownerEmail");
            var labels = _workflowType.GetDocSection("labels");

            Context.WorkflowOptions.Version = 1;
            Context.Inputs = new();

            if (!string.IsNullOrEmpty(summary))
                Context.WorkflowOptions.Description = summary;

            if (!string.IsNullOrEmpty(ownerApp))
                Context.WorkflowOptions.OwnerApp = ownerApp;

            if (!string.IsNullOrEmpty(ownerEmail))
                Context.WorkflowOptions.OwnerEmail = ownerEmail;

            if (!string.IsNullOrEmpty(labels))
                Context.WorkflowOptions.Labels = labels.Split(",").Select(a => a.Trim()).ToArray();

            var input = _workflowType.BaseType.GenericTypeArguments[0];
            var props = input.GetProperties();

            foreach (var prop in props)
            {
                var isRequired = prop.GetCustomAttribute<RequiredAttribute>();
                var description = prop.GetDocSection("summary");

                var propertyName = prop.GetDocSection("originalName") ?? SnakeCaseUtil.ToSnakeCase(prop.Name);

                var requiredString = isRequired != null ? "(required)" : "(optional)";
                Context.Inputs.Add(
                    new JProperty(
                        propertyName,
                        new JObject { new JProperty("value", ""), new JProperty("description", $"{description} {requiredString}"), }
                    )
                );
            }
        }

        public WorkflowDefinition Build(Action<WorkflowOptions> adjustOptions)
        {
            adjustOptions.Invoke(Context.WorkflowOptions);

            return new WorkflowDefinition
            {
                Name = _name,
                Tasks = Context.TaskBuilders.SelectMany(a => a.Build()).ToList(),
                FailureWorkflow =
                    Context.WorkflowOptions.FailureWorkflow != null
                        ? NamingUtil.DetermineRegistrationName(Context.WorkflowOptions.FailureWorkflow)
                        : null,
                Description = new JObject()
                {
                    new JProperty("description", Context.WorkflowOptions.Description),
                    new JProperty("labels", Context.WorkflowOptions.Labels)
                }.ToString(Formatting.None),
                InputParameters = Context.Inputs,
                OutputParameters = Context.Outputs,
                OwnerApp = Context.WorkflowOptions.OwnerApp,
                OwnerEmail = Context.WorkflowOptions.OwnerEmail,
                Version = Context.WorkflowOptions.Version
            };
        }
    }

    public class WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> : WorkflowDefinitionBuilder<TWorkflow>
        where TWorkflow : ITypedWorkflow
        where TInput : WorkflowInput<TOutput>
        where TOutput : WorkflowOutput { }

    public static class WorkflowOutputExtension
    {
        public static void SetOutput<FWorkflow, F, G>(this WorkflowDefinitionBuilder<FWorkflow, F, G> builder, Expression<Func<FWorkflow, G>> input)
            where FWorkflow : Workflow<F, G>
            where F : WorkflowInput<G>
            where G : WorkflowOutput
        {
            builder.Context.Outputs = ExpressionUtil.ParseToParameters(input.Body);
        }
    }
}
