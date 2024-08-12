using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Health;
using ConductorSharp.NoApi.Behaviors;
using ConductorSharp.NoApi.Handlers;
using ConductorSharp.Patterns.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IConfiguration configuration = null;

var builder = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration(
        (hosting, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: true);
            configuration = config.Build();
        }
    )
    .ConfigureServices(
        (_, services) =>
        {
            services.AddAutoMapper(typeof(Program));
            services
                .AddConductorSharp(baseUrl: configuration.GetValue<string>("Conductor:BaseUrl"))
                .AddExecutionManager(
                    maxConcurrentWorkers: configuration.GetValue<int>("Conductor:MaxConcurrentWorkers"),
                    sleepInterval: configuration.GetValue<int>("Conductor:SleepInterval"),
                    longPollInterval: configuration.GetValue<int>("Conductor:LongPollInterval"),
                    domain: configuration.GetValue<string>("Conductor:WorkerDomain"),
                    handlerAssemblies: typeof(Program).Assembly
                )
                .SetHealthCheckService<FileHealthService>()
                .AddPipelines(pipelines =>
                {
                    pipelines.AddContextLogging();
                    pipelines.AddRequestResponseLogging();
                    pipelines.AddValidation();
                    pipelines.AddCustomBehavior<PrepareEmailBehavior, PrepareEmailRequest, PrepareEmailResponse>();
                })
                .AddConductorSharpPatterns();

            services.RegisterWorkerTask<GetCustomerHandler>();
            services.RegisterWorkerTask<PrepareEmailHandler>();
        }
    );

using var host = builder.Build();
await host.RunAsync();
