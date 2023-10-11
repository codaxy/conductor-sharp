using ConductorSharp.Client;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util.Builders;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace ConductorSharp.Engine.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static IConductorSharpBuilder AddConductorSharp(
            this IServiceCollection builder,
            string baseUrl,
            string apiPath,
            bool preventErrorOnBadRequest = false
        )
        {
            builder.AddSingleton(
                new RestConfig
                {
                    ApiPath = apiPath,
                    BaseUrl = baseUrl,
                    IgnoreValidationErrors = preventErrorOnBadRequest
                }
            );

            builder.AddTransient<HttpClient>();

            builder.AddSingleton<IConductorClient, ConductorClient>();

            builder.AddTransient<ITaskService, TaskService>();

            builder.AddTransient<IHealthService, HealthService>();

            builder.AddTransient<IMetadataService, MetadataService>();

            builder.AddTransient<IWorkflowService, WorkflowService>();

            builder.AddSingleton(new BuildConfiguration());

            builder.AddSingleton<WorkflowBuildItemRegistry>();

            builder.AddTransient<ITaskNameBuilder, DefaultTaskNameBuilder>();

            builder.AddTransient<TaskDefinitionBuilder>();

            builder.AddTransient(typeof(WorkflowDefinitionBuilder<,,>));

            return new ConductorSharpBuilder(builder);
        }
    }
}
