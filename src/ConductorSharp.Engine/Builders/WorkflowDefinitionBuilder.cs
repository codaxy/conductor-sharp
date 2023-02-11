using Autofac;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
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
    public static class WorkflowOutputExtension
    {
        public static void SetOutput<FWorkflow, F, G>(this WorkflowDefinitionBuilder<FWorkflow, F, G> builder, Expression<Func<FWorkflow, G>> input)
            where FWorkflow : Workflow<FWorkflow, F, G>
            where F : WorkflowInput<G>
            where G : WorkflowOutput
        {
            builder.BuildContext.Outputs = ExpressionUtil.ParseToParameters(input.Body);
        }

        public static void SetOptions<FWorkflow, F, G>(this WorkflowDefinitionBuilder<FWorkflow, F, G> builder, Action<WorkflowOptions> adjustOptions)
            where FWorkflow : Workflow<FWorkflow, F, G>
            where F : WorkflowInput<G>
            where G : WorkflowOutput
        {
            adjustOptions?.Invoke(builder.BuildContext.WorkflowOptions);
        }
    }

    public class WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput>
        where TWorkflow : ITypedWorkflow
        where TInput : WorkflowInput<TOutput>
        where TOutput : WorkflowOutput
    {
        private readonly Type _workflowType = typeof(TWorkflow);

        public BuildContext BuildContext { get; } = new();
        public BuildConfiguration BuildConfiguration { get; set; }
        public WorkflowBuildItemRegistry WorkflowBuildRegistry { get; }
        public IEnumerable<ConfigurationProperty> ConfigurationProperties { get; }

        public WorkflowDefinitionBuilder(
            BuildConfiguration buildConfiguration,
            IEnumerable<ConfigurationProperty> configurationProperties,
            WorkflowBuildItemRegistry workflowBuildRegistry
        )
        {
            BuildConfiguration = buildConfiguration;
            WorkflowBuildRegistry = workflowBuildRegistry;
            ConfigurationProperties = configurationProperties;
            GenerateWorkflowName();
        }

        private void GenerateWorkflowName()
        {
            BuildContext.WorkflowName = NamingUtil.DetermineRegistrationName(_workflowType);
            if (!string.IsNullOrEmpty(BuildConfiguration.WorkflowPrefix))
            {
                BuildContext.WorkflowName = $"{BuildConfiguration.WorkflowPrefix}{BuildContext.WorkflowName}";
            }
        }

        public WorkflowDefinition Build()
        {
            XmlDocumentationReader.LoadXmlDocumentation(_workflowType.Assembly);

            if (!string.IsNullOrEmpty(BuildConfiguration?.DefaultOwnerApp))
            {
                BuildContext.WorkflowOptions.OwnerApp = BuildConfiguration.DefaultOwnerApp;
            }

            if (!string.IsNullOrEmpty(BuildConfiguration?.DefaultOwnerEmail))
            {
                BuildContext.WorkflowOptions.OwnerEmail = BuildConfiguration.DefaultOwnerEmail;
            }

            var summary = _workflowType.GetDocSection("summary");
            var ownerApp = _workflowType.GetDocSection("ownerApp");
            var ownerEmail = _workflowType.GetDocSection("ownerEmail");
            var labels = _workflowType.GetDocSection("labels");

            BuildContext.WorkflowOptions.Version = _workflowType.GetCustomAttribute<VersionAttribute>()?.Version ?? 1;
            BuildContext.Inputs = new();

            if (!string.IsNullOrEmpty(summary))
                BuildContext.WorkflowOptions.Description = summary;

            if (!string.IsNullOrEmpty(ownerApp))
                BuildContext.WorkflowOptions.OwnerApp = ownerApp;

            if (!string.IsNullOrEmpty(ownerEmail))
                BuildContext.WorkflowOptions.OwnerEmail = ownerEmail;

            if (!string.IsNullOrEmpty(labels))
                BuildContext.WorkflowOptions.Labels = labels.Split(",").Select(a => a.Trim()).ToArray();

            var input = _workflowType.BaseType.GenericTypeArguments[1];
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

            return new WorkflowDefinition
            {
                Name = BuildContext.WorkflowName,
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
                Version = BuildContext.WorkflowOptions.Version
            };
        }
    }
}
