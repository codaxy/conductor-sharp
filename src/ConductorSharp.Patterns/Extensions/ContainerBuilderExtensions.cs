using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using ConductorSharp.Patterns.Builders;
using ConductorSharp.Patterns.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.Patterns.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static IExecutionManagerBuilder AddConductorSharpPatterns(this IExecutionManagerBuilder executionManagerBuilder)
        {
            executionManagerBuilder.Builder.RegisterWorkerTask<ReadWorkflowTasks>();
            executionManagerBuilder.Builder.RegisterWorkerTask<WaitSeconds>();
            executionManagerBuilder.Builder.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(WaitSeconds).Assembly));

            return executionManagerBuilder;
        }

        public static IExecutionManagerBuilder AddCSharpLambdaTasks(
            this IExecutionManagerBuilder executionManagerBuilder,
            string csharpLambdaTaskNamePrefix = null
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
            executionManagerBuilder.Builder.AddTransient<ITaskNameBuilder, TaskNameBuilder>();

            return executionManagerBuilder;
        }
    }
}
