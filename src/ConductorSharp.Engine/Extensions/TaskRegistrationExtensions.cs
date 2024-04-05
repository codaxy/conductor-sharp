using System;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.Engine.Extensions
{
    public static class TaskRegistrationExtensions
    {
        public static void RegisterWorkerTask<TWorkerTask>(this IServiceCollection builder, Action<TaskDefinitionOptions> updateOptions = null)
            where TWorkerTask : IWorker
        {
            builder.AddSingleton(ctx => ctx.GetRequiredService<TaskDefinitionBuilder>().Build<TWorkerTask>(updateOptions));

            builder.AddTransient(
                ctx =>
                    new TaskToWorker
                    {
                        TaskName = ctx.GetRequiredService<TaskDefinitionBuilder>().Build<TWorkerTask>(updateOptions).Name,
                        TaskType = typeof(TWorkerTask)
                    }
            );
        }
    }
}
