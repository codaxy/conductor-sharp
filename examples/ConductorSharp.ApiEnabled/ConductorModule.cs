using Autofac;
using ConductorSharp.ApiEnabled.Handlers;
using ConductorSharp.ApiEnabled.Services;
using ConductorSharp.Engine.Extensions;

namespace ConductorSharp.ApiEnabled;

public class ConductorModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        RegisterTasks(builder);
        RegisterWorkflows(builder);

        builder.RegisterType<TaskExecutionCounterService>().AsImplementedInterfaces().SingleInstance();
    }

    private static void RegisterTasks(ContainerBuilder builder)
    {
        builder.RegisterWorkerTask<PrepareEmailHandler>(options =>
        {
            options.OwnerEmail = "owneremail@gmail.com";
        });
    }

    private static void RegisterWorkflows(ContainerBuilder builder) { }
}
