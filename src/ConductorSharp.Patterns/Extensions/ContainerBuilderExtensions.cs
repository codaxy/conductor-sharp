using ConductorSharp.Engine.Extensions;
using ConductorSharp.Patterns.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using MediatR.Extensions.Autofac.DependencyInjection;

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
    }
}
