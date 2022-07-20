using Autofac;
using MediatR;

namespace ConductorSharp.ApiEnabled;

public class ApiEnabledModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        builder.RegisterType<Mediator>().As<IMediator>().InstancePerLifetimeScope();
        builder.Register<ServiceFactory>(context =>
        {
            var c = context.Resolve<IComponentContext>();
            return t => c.Resolve(t);
        });
    }
}
