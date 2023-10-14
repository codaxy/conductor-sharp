using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util.Builders;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace ConductorSharp.Engine.Extensions
{
    public static class WorkflowRegistrationExtensions
    {
        public static void RegisterWorkflowDefinition(this IServiceCollection builder, WorkflowDefinition definition) =>
            builder.AddSingleton(definition);

        public static void RegisterWorkflowDefinition(this IServiceCollection builder, string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException($"'{nameof(filename)}' cannot be null or empty.", nameof(filename));

            var fileContents = File.ReadAllText(filename);
            var definition = JsonConvert.DeserializeObject<WorkflowDefinition>(fileContents);

            builder.AddSingleton(definition);
        }

        public static void RegisterWorkflow<TWorkflow>(this IServiceCollection builder, BuildConfiguration buildConfiguration = null)
            where TWorkflow : IConfigurableWorkflow
        {
            builder.AddTransient(ctx =>
            {
                var builder = ctx.GetRequiredService(
                    typeof(WorkflowDefinitionBuilder<,,>).MakeGenericType(typeof(TWorkflow).BaseType.GenericTypeArguments)
                );

                if (buildConfiguration != null)
                {
                    builder.GetType().GetProperty("BuildConfiguration").SetValue(builder, buildConfiguration);
                }

                var ctors = typeof(TWorkflow).GetConstructors();
                if (ctors.Length != 1)
                    throw new InvalidOperationException($"Workflow {typeof(TWorkflow).Name} must have exactly one constructor");

                // Check if wf has ctor with builder type parameter
                var ctorParamTypes = ctors[0].GetParameters().Select(p => p.ParameterType).ToArray();
                var hasBuilderType = ctorParamTypes.Contains(builder.GetType());
                if (!hasBuilderType)
                    throw new ArgumentException($"Configurable workflow constructor must have a {builder.GetType().Name} parameter");

                // Resolve each wf parameter
                var wfParams = ctorParamTypes.Select(t => t == builder.GetType() ? builder : ctx.GetRequiredService(t)).ToArray();
                var workflow = Activator.CreateInstance(typeof(TWorkflow), wfParams) as ITypedWorkflow;

                var definition = workflow.GetDefinition();
                return definition;
            });
        }
    }
}
