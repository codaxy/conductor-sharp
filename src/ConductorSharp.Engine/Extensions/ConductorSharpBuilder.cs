using System;
using System.Net.Http;
using System.Reflection;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Behaviors;
using ConductorSharp.Engine.Health;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Polling;
using ConductorSharp.Engine.Service;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConductorSharp.Engine.Extensions
{
    public class ConductorSharpBuilder(IServiceCollection builder) : IConductorSharpBuilder, IExecutionManagerBuilder
    {
        public IServiceCollection Builder { get; set; } = builder;

        public IExecutionManagerBuilder AddExecutionManager(
            int maxConcurrentWorkers,
            int sleepInterval,
            int longPollInterval,
            string domain = null,
            params Assembly[] handlerAssemblies
        )
        {
            var workerConfig = new WorkerSetConfig
            {
                MaxConcurrentWorkers = maxConcurrentWorkers,
                LongPollInterval = longPollInterval,
                Domain = domain,
                SleepInterval = sleepInterval
            };

            Builder.AddSingleton(workerConfig);

            Builder.AddTransient<IHostedService, WorkflowEngineBackgroundService>();

            Builder.AddTransient<IDeploymentService, DeploymentService>();

            Builder.AddTransient<ModuleDeployment>();

            Builder.AddSingleton<ExecutionManager>();

            Builder.AddScoped<ConductorSharpExecutionContext>();

            Builder.AddSingleton<IConductorSharpHealthService, InMemoryHealthService>();

            Builder.AddTransient<IPollTimingStrategy, InverseExponentialBackoff>();

            Builder.AddTransient<IPollOrderStrategy, RandomOrdering>();

            Builder.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(handlerAssemblies));

            return this;
        }

        public IExecutionManagerBuilder AddPipelines(Action<IPipelineBuilder> behaviorBuilder)
        {
            var pipelineBuilder = new PipelineBuilder(Builder);
            behaviorBuilder(pipelineBuilder);
            return this;
        }

        public IExecutionManagerBuilder SetHealthCheckService<T>()
            where T : IConductorSharpHealthService
        {
            Builder.AddSingleton(typeof(IConductorSharpHealthService), typeof(T));
            return this;
        }

        public IExecutionManagerBuilder UseConstantPollTimingStrategy()
        {
            Builder.AddTransient<IPollTimingStrategy, ConstantInterval>();
            return this;
        }

        public IConductorSharpBuilder SetBuildConfiguration(BuildConfiguration buildConfiguration)
        {
            if (buildConfiguration is null)
            {
                throw new ArgumentNullException(nameof(buildConfiguration));
            }

            Builder.AddSingleton(buildConfiguration);
            return this;
        }

        public IConductorSharpBuilder AddAlternateClient(string baseUrl, string key, string apiPath = "api", bool ignoreInvalidCertificate = false)
        {
            var clientBuilder = builder
                .AddHttpClient(key, client => client.BaseAddress = new(baseUrl))
                .AddHttpMessageHandler(() => new ApiPathOverrideHttpHandler(apiPath));

            if (ignoreInvalidCertificate)
            {
                clientBuilder.ConfigurePrimaryHttpMessageHandler(
                    () => new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true }
                );
            }

            builder.AddKeyedTransient<IAdminService, AdminService>(key, ((sp, _) => new(sp.GetService<IHttpClientFactory>(), key)));
            builder.AddKeyedTransient<IEventService, EventService>(key, (sp, _) => new(sp.GetService<IHttpClientFactory>(), key));
            builder.AddKeyedTransient<IExternalPayloadService, ExternalPayloadService>(key, (sp, _) => new(sp.GetService<IHttpClientFactory>(), key));
            builder.AddKeyedTransient<IQueueAdminService, QueueAdminService>(key, (sp, _) => new(sp.GetService<IHttpClientFactory>(), key));
            builder.AddKeyedTransient<IWorkflowBulkService, WorkflowBulkService>(key, (sp, _) => new(sp.GetService<IHttpClientFactory>(), key));
            builder.AddKeyedTransient<ITaskService, TaskService>(key, (sp, _) => new(sp.GetService<IHttpClientFactory>(), key));
            builder.AddKeyedTransient<IHealthService, HealthService>(key, (sp, _) => new(sp.GetService<IHttpClientFactory>(), key));
            builder.AddKeyedTransient<IMetadataService, MetadataService>(key, (sp, _) => new(sp.GetService<IHttpClientFactory>(), key));
            builder.AddKeyedTransient<IWorkflowService, WorkflowService>(key, (sp, _) => new(sp.GetService<IHttpClientFactory>(), key));

            return this;
        }
    }
}
