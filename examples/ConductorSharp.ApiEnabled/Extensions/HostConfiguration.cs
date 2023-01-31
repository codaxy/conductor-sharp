using Autofac;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Health;
using MediatR.Extensions.Autofac.DependencyInjection;

namespace ConductorSharp.ApiEnabled.Extensions;

public static class HostConfiguration
{
    public static IHostBuilder ConfigureApiEnabled(this IHostBuilder hostBuilder, ConfigurationManager configuration)
    {
        return hostBuilder.ConfigureContainer<ContainerBuilder>(builder =>
        {
            builder
                .AddConductorSharp(
                    baseUrl: configuration.GetValue<string>("Conductor:BaseUrl"),
                    apiPath: configuration.GetValue<string>("Conductor:ApiUrl"),
                    preventErrorOnBadRequest: configuration.GetValue<bool>("Conductor:PreventErrorOnBadRequest")
                )
                .AddExecutionManager(
                    maxConcurrentWorkers: configuration.GetValue<int>("Conductor:MaxConcurrentWorkers"),
                    sleepInterval: configuration.GetValue<int>("Conductor:SleepInterval"),
                    longPollInterval: configuration.GetValue<int>("Conductor:LongPollInterval"),
                    domain: configuration.GetValue<string>("Conductor:WorkerDomain")
                )
                .SetHealthCheckService<FileHealthService>()
                .AddPipelines(pipelines =>
                {
                    pipelines.AddExecutionTaskTracking();
                    pipelines.AddContextLogging();
                    pipelines.AddRequestResponseLogging();
                    pipelines.AddValidation();
                });

            builder.RegisterMediatR(typeof(Program).Assembly);
            builder.RegisterModule<ConductorModule>();
        });
    }
}
