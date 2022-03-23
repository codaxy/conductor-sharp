using Autofac;
using MediatR;
using MediatR.Pipeline;
using System.Reflection;
using Module = Autofac.Module;

namespace XgsPon.MediatR
{
    public class MediatorModule : Module
    {
        private static readonly Type[] s_mediatorTypes = new Type[]
        {
            typeof(IRequestHandler<, >),
            typeof(IRequestExceptionHandler<, >),
            typeof(IRequestExceptionAction<, >),
            typeof(INotificationHandler<>)
        };

        // Assembly where handlers are located
        private readonly Assembly _assembly;

        public MediatorModule(Assembly assembly) => _assembly = assembly;

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Mediator>().As<IMediator>().InstancePerLifetimeScope();
            builder.Register<ServiceFactory>(
                context =>
                {
                    var c = context.Resolve<IComponentContext>();
                    return t => c.Resolve(t);
                }
            );

            foreach (var mediatrType in s_mediatorTypes)
            {
                builder.RegisterAssemblyTypes(_assembly).AsClosedTypesOf(mediatrType).AsImplementedInterfaces();
            }
        }
    }
}
