using Autofac;
using ConductorSharp.Client.Service;
using Vodafone.Frinx.Workflows.WFEToolkit.Commands;
using Vodafone.Frinx.Workflows.WFEToolkit.Service;

namespace ConductorSharp.Toolkit
{
    public class WFEToolkitModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConductorClient>().As<IConductorClient>();
            builder.RegisterType<MetadataService>().As<IMetadataService>();
            builder.RegisterType<ScaffoldingService>().As<IScaffoldingService>();
            builder.RegisterType<CommandRegistry>();
            builder.RegisterType<ScaffoldCommand>().As<IWFECommand>();
        }
    }
}
