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
            builder.RegisterType<DllScraperService>().As<IDllScraperService>();
            builder.RegisterType<ScaffoldingService>().As<IScaffoldingService>();
            builder.RegisterType<DocumentCreator>();
            builder.RegisterType<CommandRegistry>();
            builder.RegisterType<DocumentCommand>().As<IWFECommand>();
            builder.RegisterType<ScaffoldCommand>().As<IWFECommand>();
            builder.RegisterType<ScrapeDllCommand>().As<IWFECommand>();
        }
    }
}
