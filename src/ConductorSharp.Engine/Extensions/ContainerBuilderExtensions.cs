using System;
using System.Net.Http;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace ConductorSharp.Engine.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static IConductorSharpBuilder AddConductorSharp(this IServiceCollection builder, string baseUrl)
        {
            builder.AddTransient<HttpClient>();
            builder.AddHttpClient<ConductorClient>().ConfigureHttpClient(client => client.BaseAddress = new Uri(baseUrl));
            builder.AddTransient<IAdminService, AdminService>();
            builder.AddTransient<IEventService, EventService>();
            builder.AddTransient<IExternalPayloadService, ExternalPayloadService>();
            builder.AddTransient<IQueueAdminService, QueueAdminService>();
            builder.AddTransient<IWorkflowBulkService, WorkflowBulkService>();
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
