using Autofac;
using ConductorSharp.Proxy.Services;
using MediatR;

namespace ConductorSharp.Proxy;

public class ApiEnabledModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        //builder.RegisterType<Mediator>().As<IMediator>().InstancePerLifetimeScope();
        //builder.Register<ServiceFactory>(context =>
        //{
        //    var c = context.Resolve<IComponentContext>();
        //    return t => c.Resolve(t);
        //});

        builder.RegisterType<PolledWorkflowRegistry>().As<IPolledWokflowRegistry>().SingleInstance();
        builder.RegisterType<WorkflowPollingBackgroundService>().As<IHostedService>();
    }
}
