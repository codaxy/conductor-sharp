using Autofac;
using ConductorSharp.ApiEnabled.Handlers;
using ConductorSharp.Engine.Extensions;

namespace ConductorSharp.ApiEnabled;

public class ConductorModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        RegisterTasks(builder);
        RegisterWorkflows(builder);
    }

    private static void RegisterTasks(ContainerBuilder builder)
    {
        builder.RegisterWorkerTask<NumberHandler>();
        builder.RegisterWorkerTask<StringHandler>();
    }

    private static void RegisterWorkflows(ContainerBuilder builder)
    {
        builder.RegisterWorkflow<ExampleWorkflow>();
    }
}
