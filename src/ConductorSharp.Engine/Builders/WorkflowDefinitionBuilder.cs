using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
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
    }

    public class WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> : ITaskSequenceBuilder<TWorkflow>
        where TWorkflow : ITypedWorkflow
        where TInput : WorkflowInput<TOutput>
        where TOutput : WorkflowOutput
    {
        private readonly Type _workflowType = typeof(TWorkflow);
        private readonly List<ITaskBuilder> _taskBuilders = new();

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

        private void GenerateWorkflowName() => BuildContext.WorkflowName = NamingUtil.DetermineRegistrationName(_workflowType);

        public WorkflowDefinition Build()
        {
            var metadataAttribute = _workflowType.GetCustomAttribute<WorkflowMetadataAttribute>();
            var ownerApp = metadataAttribute?.OwnerApp;
            var ownerEmail = metadataAttribute?.OwnerEmail;
            var description = metadataAttribute?.Description;
            var failureWorkflow = metadataAttribute?.FailureWorkflow;

            if (!string.IsNullOrEmpty(BuildConfiguration?.DefaultOwnerApp))
            {
                ownerApp = BuildConfiguration.DefaultOwnerApp;
            }

            if (!string.IsNullOrEmpty(BuildConfiguration?.DefaultOwnerEmail))
            {
                ownerEmail = BuildConfiguration.DefaultOwnerEmail;
            }

            var version = _workflowType.GetCustomAttribute<VersionAttribute>()?.Version ?? 1;
            BuildContext.Inputs = new();

            var input = _workflowType.BaseType.GenericTypeArguments[1];
            var props = input.GetProperties();

            foreach (var prop in props)
            {
                var propertyName = NamingUtil.GetParameterName(prop);
                BuildContext.Inputs.Add(propertyName);
            }

            return new WorkflowDefinition
            {
                Name = BuildContext.WorkflowName,
                Tasks = _taskBuilders.SelectMany(a => a.Build()).ToList(),
                FailureWorkflow = failureWorkflow != null ? NamingUtil.DetermineRegistrationName(failureWorkflow) : null,
                Description = description,
                InputParameters = BuildContext.Inputs.ToArray(),
                OutputParameters = BuildContext.Outputs,
                OwnerApp = ownerApp,
                OwnerEmail = ownerEmail,
                Version = version
            };
        }

        public void AddTaskBuilderToSequence(ITaskBuilder builder) => _taskBuilders.Add(builder);
    }
}
