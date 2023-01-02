using Autofac;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Health;
using MediatR.Extensions.Autofac.DependencyInjection;

namespace ConductorSharp.Proxy.Extensions;

public static class HostConfiguration
{
    public static IHostBuilder ConfigureProxy(this IHostBuilder hostBuilder, ConfigurationManager? configuration)
    {
        return hostBuilder.ConfigureContainer<ContainerBuilder>(builder =>
        {
            builder.AddConductorSharp(
                baseUrl: configuration.GetValue<string>("Conductor:BaseUrl"),
                apiPath: configuration.GetValue<string>("Conductor:ApiUrl"),
                preventErrorOnBadRequest: configuration.GetValue<bool>("Conductor:PreventErrorOnBadRequest")
            );

            builder.RegisterMediatR(typeof(Program).Assembly);
            builder.RegisterModule<ConductorModule>();
            builder.RegisterModule<ApiEnabledModule>();
        });
    }
}
