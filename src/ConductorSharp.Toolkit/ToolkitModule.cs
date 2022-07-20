using Autofac;
using ConductorSharp.Client.Service;
using ConductorSharp.Toolkit.Commands;
using ConductorSharp.Toolkit.Service;

namespace ConductorSharp.Toolkit
{
    public class ToolkitModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConductorClient>().As<IConductorClient>();
            builder.RegisterType<MetadataService>().As<IMetadataService>();
            builder.RegisterType<ScaffoldingService>().As<IScaffoldingService>();
            builder.RegisterType<CommandRegistry>();
            builder.RegisterType<ScaffoldCommand>().As<Command>();
        }
    }
}
