using Autofac;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util.Builders;
using System;

namespace ConductorSharp.Engine.Extensions
{
    public static class TaskRegistrationExtensions
    {
        public static void RegisterWorkerTask<TWorkerTask>(
            this ContainerBuilder builder,
            Action<TaskDefinitionOptions> updateOptions = null,
            BuildConfiguration buildConfiguration = null
        ) where TWorkerTask : IWorker
        {
            builder.RegisterType<TWorkerTask>();

            builder.Register(ctx => ctx.ResolveTaskDefinitionBuilder(buildConfiguration).Build<TWorkerTask>(updateOptions)).SingleInstance();

            builder.Register(
                ctx =>
                    new TaskToWorker
                    {
                        TaskName = ctx.ResolveTaskDefinitionBuilder(buildConfiguration).Build<TWorkerTask>(updateOptions).Name,
                        TaskType = typeof(TWorkerTask)
                    }
            );
        }

        private static Builders.Configurable.TaskDefinitionBuilder ResolveTaskDefinitionBuilder(
            this IComponentContext componentContext,
            BuildConfiguration buildConfiguration
        )
        {
            var builder = componentContext.Resolve<Builders.Configurable.TaskDefinitionBuilder>();

            builder.BuildConfiguration = buildConfiguration ?? builder.BuildConfiguration;

            return builder;
        }
    }
}
