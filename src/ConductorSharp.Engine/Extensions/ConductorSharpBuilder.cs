using System;
using System.Reflection;
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
    }
}
