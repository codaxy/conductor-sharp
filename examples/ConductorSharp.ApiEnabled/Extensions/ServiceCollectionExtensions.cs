using ConductorSharp.ApiEnabled.Handlers;
using ConductorSharp.ApiEnabled.Services;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Health;
using ConductorSharp.Patterns.Extensions;
using MediatR;

namespace ConductorSharp.ApiEnabled.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureApiEnabled(this IServiceCollection hostBuilder, ConfigurationManager configuration)
    {
        hostBuilder
            .AddConductorSharp(
                baseUrl: configuration.GetValue<string>("Conductor:BaseUrl"),
                apiPath: configuration.GetValue<string>("Conductor:ApiUrl"),
                preventErrorOnBadRequest: configuration.GetValue<bool>("Conductor:PreventErrorOnBadRequest")
            )
            .AddExecutionManager(
                maxConcurrentWorkers: configuration.GetValue<int>("Conductor:MaxConcurrentWorkers"),
                sleepInterval: configuration.GetValue<int>("Conductor:SleepInterval"),
                longPollInterval: configuration.GetValue<int>("Conductor:LongPollInterval"),
                domain: configuration.GetValue<string>("Conductor:WorkerDomain"),
                typeof(ServiceCollectionExtensions).Assembly
            )
            .SetHealthCheckService<FileHealthService>()
            .AddPipelines(pipelines =>
            {
                pipelines.AddExecutionTaskTracking();
                pipelines.AddContextLogging();
                pipelines.AddRequestResponseLogging();
                pipelines.AddValidation();
            })
            .AddConductorSharpPatterns();

        hostBuilder.AddSingleton<ITaskExecutionCounterService, TaskExecutionCounterService>();
        hostBuilder.RegisterWorkerTask<PrepareEmailHandler>(options =>
        {
            options.OwnerEmail = "owneremail@gmail.com";
        });

        return hostBuilder;
    }
}
