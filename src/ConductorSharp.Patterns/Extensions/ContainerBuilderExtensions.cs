using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using ConductorSharp.Patterns.Builders;
using ConductorSharp.Patterns.Services;
using ConductorSharp.Patterns.Tasks;
using ConductorSharp.Patterns.Workflows;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.Patterns.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static IExecutionManagerBuilder AddConductorSharpPatterns(this IExecutionManagerBuilder executionManagerBuilder)
        {
            executionManagerBuilder.Builder.RegisterWorkerTask<ReadWorkflowTasks>();
            executionManagerBuilder.Builder.RegisterWorkerTask<WaitSeconds>();
            executionManagerBuilder.Builder.RegisterWorkerTask<BuildFailureError>();
            executionManagerBuilder.Builder.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(WaitSeconds).Assembly));

            return executionManagerBuilder;
        }

        public static IExecutionManagerBuilder AddCSharpLambdaTasks(
            this IExecutionManagerBuilder executionManagerBuilder,
            string? csharpLambdaTaskNamePrefix = null
        )
        {
            executionManagerBuilder.Builder.RegisterWorkerTask<CSharpLambdaTask>(options =>
            {
                options.OwnerEmail = "owneremail@gmail.com";
            });
            executionManagerBuilder.Builder.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(CSharpLambdaTask).Assembly));
            executionManagerBuilder.Builder.AddSingleton(
                new ConfigurationProperty(CSharpLambdaTask.LambdaTaskNameConfigurationProperty, csharpLambdaTaskNamePrefix)
            );
            executionManagerBuilder.Builder.AddTransient<INameBuilder, NameBuilder>();

            return executionManagerBuilder;
        }

        /// <summary>
        /// Configures only the naming for SignalWait tasks/workflows. Use this in projects that
        /// define workflows using SignalWait but don't run the signal infrastructure themselves.
        /// For full signal infrastructure, use <see cref="AddSignalWait{TSignalStore}"/> instead.
        /// </summary>
        /// <param name="executionManagerBuilder">The execution manager builder.</param>
        /// <param name="signalTaskNamePrefix">Optional prefix for signal task/workflow names to match the runtime configuration.</param>
        public static IExecutionManagerBuilder AddSignalWaitNaming(
            this IExecutionManagerBuilder executionManagerBuilder,
            string? signalTaskNamePrefix = null
        )
        {
            executionManagerBuilder.Builder.AddSingleton(
                new ConfigurationProperty(Constants.SignalPrefixConfigurationProperty, signalTaskNamePrefix)
            );
            executionManagerBuilder.Builder.AddTransient<INameBuilder, NameBuilder>();

            return executionManagerBuilder;
        }

        /// <summary>
        /// Adds the SignalWait feature: a reusable sub-workflow for pausing workflows until
        /// an external signal arrives, without consuming worker threads.
        /// <typeparamref name="TSignalStore"/> is the consumer's persistence implementation.
        /// <para>
        /// IMPORTANT: The signal store must be shared across all processes that participate in signaling.
        /// For multi-process deployments, use a database or Redis-backed store implementation.
        /// The built-in <see cref="InMemorySignalStore"/> only works within a single process.
        /// </para>
        /// </summary>
        /// <param name="executionManagerBuilder">The execution manager builder.</param>
        /// <param name="signalTaskNamePrefix">Optional prefix for signal task/workflow names to avoid collisions across projects.</param>
        public static IExecutionManagerBuilder AddSignalWait<TSignalStore>(
            this IExecutionManagerBuilder executionManagerBuilder,
            string? signalTaskNamePrefix = null
        )
            where TSignalStore : class, ISignalStore
        {
            executionManagerBuilder.Builder.AddScoped<ISignalStore, TSignalStore>();
            executionManagerBuilder.Builder.AddScoped<ISignalService, SignalService>();
            executionManagerBuilder.Builder.RegisterWorkerTask<RegisterWaiter>(options =>
            {
                options.ConcurrentExecLimit = 1;
                options.RetryCount = 10;
                options.RetryDelaySeconds = 1;
            });
            executionManagerBuilder.Builder.RegisterWorkflow<SignalWait>();
            executionManagerBuilder.Builder.AddHostedService<SignalSweeperService>();
            executionManagerBuilder.Builder.AddSingleton(
                new ConfigurationProperty(Constants.SignalPrefixConfigurationProperty, signalTaskNamePrefix)
            );
            executionManagerBuilder.Builder.AddTransient<INameBuilder, NameBuilder>();
            executionManagerBuilder.Builder.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(RegisterWaiter).Assembly));

            return executionManagerBuilder;
        }
    }
}
