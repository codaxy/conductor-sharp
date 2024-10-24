using ConductorSharp.Definitions.Behaviors;
using ConductorSharp.Definitions.Workflows;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Health;
using ConductorSharp.Patterns.Extensions;
using Microsoft.Extensions.Configuration;
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
            services
                .AddConductorSharp(baseUrl: configuration.GetValue<string>("Conductor:BaseUrl"))
                .AddExecutionManager(
                    maxConcurrentWorkers: configuration.GetValue<int>("Conductor:MaxConcurrentWorkers"),
                    sleepInterval: configuration.GetValue<int>("Conductor:SleepInterval"),
                    longPollInterval: configuration.GetValue<int>("Conductor:LongPollInterval"),
                    domain: configuration.GetValue<string>("Conductor:WorkerDomain"),
                    typeof(Program).Assembly
                )
                .AddConductorSharpPatterns()
                .SetHealthCheckService<FileHealthService>()
                .AddPipelines(pipelines =>
                {
                    pipelines.AddCustomBehavior(typeof(CustomBehavior<,>));
                    pipelines.AddRequestResponseLogging();
                    pipelines.AddValidation();
                })
                .AddCSharpLambdaTasks();

            services.RegisterWorkflow<SendCustomerNotification>();
            services.RegisterWorkflow<HandleNotificationFailure>();
            services.RegisterWorkflow<CSharpLambdaWorkflow>();
            services.RegisterWorkflow<EnumWorkflow>();
        }
    );

using var host = builder.Build();
await host.RunAsync();
