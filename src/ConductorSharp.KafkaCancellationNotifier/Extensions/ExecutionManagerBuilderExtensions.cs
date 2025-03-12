using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Interface;
using ConductorSharp.KafkaCancellationNotifier.Service;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.KafkaCancellationNotifier.Extensions
{
    public static class ExecutionManagerBuilderExtensions
    {
        public static IExecutionManagerBuilder AddKafkaCancellationNotifier(
            this IExecutionManagerBuilder builder,
            string kafkaBootstrapServers,
            string topicName,
            string groupId
        )
        {
            ArgumentNullException.ThrowIfNull(kafkaBootstrapServers);
            ArgumentNullException.ThrowIfNull(topicName);
            ArgumentNullException.ThrowIfNull(groupId);

            builder
                .Builder.AddOptions<KafkaOptions>()
                .Configure(opts =>
                {
                    opts.BootstrapServers = kafkaBootstrapServers;
                    opts.GroupId = groupId;
                    opts.TopicName = topicName;
                });

            builder.Builder.AddSingleton<ICancellationNotifier, Service.KafkaCancellationNotifier>();
            builder.Builder.AddHostedService<KafkaConsumerBackgroundService>();

            return builder;
        }
    }
}
