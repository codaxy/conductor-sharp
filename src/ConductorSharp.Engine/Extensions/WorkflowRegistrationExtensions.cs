using Autofac;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Builders.Configurable;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util.Builders;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ConductorSharp.Engine.Extensions
{
    public static class WorkflowRegistrationExtensions
    {
        public static void RegisterWorkflowDefinition(this ContainerBuilder builder, WorkflowDefinition definition) =>
            builder.RegisterInstance(definition);

        public static void RegisterWorkflowDefinition(this ContainerBuilder builder, string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException($"'{nameof(filename)}' cannot be null or empty.", nameof(filename));

            var fileContents = File.ReadAllText(filename);
            var definition = JsonConvert.DeserializeObject<WorkflowDefinition>(fileContents);

            builder.RegisterInstance(definition);
        }

        public static void RegisterWorkflow<TWorkflow>(this ContainerBuilder builder) where TWorkflow : ITypedWorkflow, new() =>
            builder.RegisterInstance(new TWorkflow().GetDefinition());

        public static void RegisterWorkflow<TWorkflow>(this ContainerBuilder builder, BuildConfiguration buildConfiguration = null)
            where TWorkflow : IConfigurableWorkflow
        {
            builder.Register(ctx =>
            {
                var builder = ctx.Resolve(typeof(WorkflowDefinitionBuilder<,,>).MakeGenericType(typeof(TWorkflow).BaseType.GenericTypeArguments));

                if (buildConfiguration != null)
                {
                    builder.GetType().GetProperty("BuildConfiguration").SetValue(builder, buildConfiguration);
                }

                if (typeof(TWorkflow).GetMatchingConstructor(new Type[] { builder.GetType() }) == null)
                {
                    throw new ArgumentException($"Configurable workflow constructor must have a {builder.GetType().Name} parameter");
                }

                var workflow = Activator.CreateInstance(typeof(TWorkflow), builder) as ITypedWorkflow;

                var definition = workflow.GetDefinition();
                return definition;
            });
        }
    }
}
