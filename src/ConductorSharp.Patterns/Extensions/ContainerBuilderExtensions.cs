using ConductorSharp.Engine.Extensions;
using ConductorSharp.Patterns.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using MediatR.Extensions.Autofac.DependencyInjection;
using Autofac;
using ConductorSharp.Engine.Util;
using ConductorSharp.Patterns.Builders;
using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Patterns.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static IExecutionManagerBuilder AddConductorSharpPatterns(this IExecutionManagerBuilder executionManagerBuilder)
        {
            executionManagerBuilder.Builder.RegisterWorkerTask<ReadWorkflowTasks>();
            executionManagerBuilder.Builder.RegisterWorkerTask<WaitSeconds>();
            executionManagerBuilder.Builder.RegisterMediatR(typeof(WaitSeconds).Assembly);

            return executionManagerBuilder;
        }

        public static IExecutionManagerBuilder AddCSharpLambdaTasks(
            this IExecutionManagerBuilder executionManagerBuilder,
            string csharpLambdaTaskNamePrefix = null
        )
        {
            executionManagerBuilder.Builder.RegisterWorkerTask<CSharpLambdaTask>();
            executionManagerBuilder.Builder.RegisterMediatR(typeof(CSharpLambdaTask).Assembly);
            executionManagerBuilder.Builder.RegisterInstance(
                new ConfigurationProperty(CSharpLambdaTask.LambdaTaskNameConfigurationProperty, csharpLambdaTaskNamePrefix)
            );
            executionManagerBuilder.Builder.RegisterType<TaskNameBuilder>().As<ITaskNameBuilder>();

            return executionManagerBuilder;
        }
    }
}
