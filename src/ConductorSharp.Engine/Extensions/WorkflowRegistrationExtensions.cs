using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util.Builders;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;

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

                if (!HasConstructorWithParameters<TWorkflow>(new Type[] { builder.GetType() }))
                {
                    throw new ArgumentException($"Configurable workflow constructor must have a {builder.GetType().Name} parameter");
                }

                var workflow = Activator.CreateInstance(typeof(TWorkflow), builder) as ITypedWorkflow;

                var definition = workflow.GetDefinition();
                return definition;
            });
        }

        private static bool HasConstructorWithParameters<T>(Type[] providedParameters)
        {
            return typeof(T)
                .GetConstructors()
                .Any(c =>
                {
                    var constructorParameters = c.GetParameters();
                    if (constructorParameters.Length != providedParameters.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < providedParameters.Length; i++)
                    {
                        if (constructorParameters[i].ParameterType != providedParameters[i])
                        {
                            return false;
                        }
                    }

                    return true;
                });
        }
    }
}
