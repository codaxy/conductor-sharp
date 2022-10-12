using Autofac;
using Autofac.Extensions.DependencyInjection;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Health;
using ConductorSharp.NoApi;
using MediatR.Extensions.Autofac.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IConfiguration? configuration = null;

var builder = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration(
        (hosting, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: true);
            configuration = config.Build();
        }
    )
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureServices(
        (_, services) =>
        {
            services.AddAutoMapper(typeof(Program));
        }
    )
    .ConfigureContainer<ContainerBuilder>(
        (container, builder) =>
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
                    pipelines.AddContextLogging();
                    pipelines.AddRequestResponseLogging();
                    pipelines.AddValidation();
                });

            builder.RegisterMediatR(typeof(Program).Assembly);
            builder.RegisterModule<ConductorModule>();
        }
    );

using var host = builder.Build();
await host.RunAsync();
