using System;
using System.Reflection;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.Engine.Extensions
{
    public static class TaskRegistrationExtensions
    {
        public static void RegisterWorkerTask<TWorkerTask>(
            this IServiceCollection builder,
            Action<TaskDefinitionOptions> updateOptions = null,
            BuildConfiguration buildConfiguration = null
        )
            where TWorkerTask : IWorker
        {
            builder.AddSingleton(ctx => ctx.ResolveTaskDefinitionBuilder(buildConfiguration).Build<TWorkerTask>(updateOptions));

            builder.AddTransient(
                ctx =>
                    new TaskToWorker
                    {
                        TaskName = ctx.ResolveTaskDefinitionBuilder(buildConfiguration).Build<TWorkerTask>(updateOptions).Name,
                        TaskType = typeof(TWorkerTask),
                        TaskDomain = GetTaskDomain(typeof(TWorkerTask))
                    }
            );
        }

        private static TaskDefinitionBuilder ResolveTaskDefinitionBuilder(
            this IServiceProvider componentContext,
            BuildConfiguration buildConfiguration
        )
        {
            var builder = componentContext.GetRequiredService<TaskDefinitionBuilder>();

            builder.BuildConfiguration = buildConfiguration ?? builder.BuildConfiguration;

            return builder;
        }

        private static string GetTaskDomain(Type workerType) => workerType.GetCustomAttribute<TaskDomainAttribute>()?.Domain;
    }
}
