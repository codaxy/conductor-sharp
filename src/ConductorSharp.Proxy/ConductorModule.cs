using Autofac;
using ConductorSharp.Client.Service;

namespace ConductorSharp.Proxy;

public class ConductorModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        RegisterTasks(builder);
        RegisterWorkflows(builder);

        builder.RegisterType<MetadataService>().As<IMetadataService>();
    }

    private static void RegisterTasks(ContainerBuilder builder) { }

    private static void RegisterWorkflows(ContainerBuilder builder) { }
}
