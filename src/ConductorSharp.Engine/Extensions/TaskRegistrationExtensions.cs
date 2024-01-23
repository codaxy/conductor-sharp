using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ConductorSharp.Engine.Extensions
{
    public static class TaskRegistrationExtensions
    {
        public static void RegisterWorkerTask<TWorkerTask>(this IServiceCollection builder, Action<TaskDefinitionOptions> updateOptions = null)
            where TWorkerTask : IWorker
        {
            builder.AddSingleton(ctx => ctx.ResolveTaskDefinitionBuilder().Build<TWorkerTask>(updateOptions));

            builder.AddTransient(
                ctx =>
                    new TaskToWorker
                    {
                        TaskName = ctx.ResolveTaskDefinitionBuilder().Build<TWorkerTask>(updateOptions).Name,
                        TaskType = typeof(TWorkerTask)
                    }
            );
        }

        private static TaskDefinitionBuilder ResolveTaskDefinitionBuilder(this IServiceProvider componentContext)
        {
            var builder = componentContext.GetRequiredService<TaskDefinitionBuilder>();

            return builder;
        }
    }
}
