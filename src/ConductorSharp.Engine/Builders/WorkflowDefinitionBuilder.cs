using ConductorSharp.Client.Model.Common;
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

        public static void SetOptions<FWorkflow, F, G>(this WorkflowDefinitionBuilder<FWorkflow, F, G> builder, Action<WorkflowOptions> adjustOptions)
            where FWorkflow : Workflow<FWorkflow, F, G>
            where F : WorkflowInput<G>
            where G : WorkflowOutput
        {
            adjustOptions?.Invoke(builder.BuildContext.WorkflowOptions);
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
            if (!string.IsNullOrEmpty(BuildConfiguration?.DefaultOwnerApp))
            {
                BuildContext.WorkflowOptions.OwnerApp = BuildConfiguration.DefaultOwnerApp;
            }

            if (!string.IsNullOrEmpty(BuildConfiguration?.DefaultOwnerEmail))
            {
                BuildContext.WorkflowOptions.OwnerEmail = BuildConfiguration.DefaultOwnerEmail;
            }

            BuildContext.WorkflowOptions.Version =
                _workflowType.GetCustomAttribute<VersionAttribute>()?.Version ?? BuildContext.WorkflowOptions.Version;
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
                FailureWorkflow =
                    BuildContext.WorkflowOptions.FailureWorkflow != null
                        ? NamingUtil.DetermineRegistrationName(BuildContext.WorkflowOptions.FailureWorkflow)
                        : null,
                Description = BuildContext.WorkflowOptions.Description,
                InputParameters = BuildContext.Inputs.ToArray(),
                OutputParameters = BuildContext.Outputs,
                OwnerApp = BuildContext.WorkflowOptions.OwnerApp,
                OwnerEmail = BuildContext.WorkflowOptions.OwnerEmail,
                Version = BuildContext.WorkflowOptions.Version
            };
        }

        public void AddTaskBuilderToSequence(ITaskBuilder builder) => _taskBuilders.Add(builder);
    }
}
