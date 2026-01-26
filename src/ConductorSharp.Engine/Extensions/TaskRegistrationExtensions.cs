using System;
using System.Reflection;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Model;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.Engine.Extensions
{
    public static class TaskRegistrationExtensions
    {
        public static void RegisterWorkerTask<TWorkerTask>(this IServiceCollection builder, Action<TaskDefinitionOptions> updateOptions = null)
            where TWorkerTask : class
        // TODO: MR Removal
        //where TWorkerTask : IWorker
        {
            builder.AddSingleton(ctx => ctx.GetRequiredService<TaskDefinitionBuilder>().Build<TWorkerTask>(updateOptions));

            builder.AddTransient(
                ctx =>
                    new TaskToWorker
                    {
                        TaskName = ctx.GetRequiredService<TaskDefinitionBuilder>().Build<TWorkerTask>(updateOptions).Name,
                        TaskType = typeof(TWorkerTask),
                        TaskDomain = GetTaskDomain(typeof(TWorkerTask))
                    }
            );

            builder.AddTransient<TWorkerTask>();
        }

        private static string GetTaskDomain(Type workerType) => workerType.GetCustomAttribute<TaskDomainAttribute>()?.Domain;
    }
}
