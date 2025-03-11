﻿using System.Text;
using ConductorSharp.Engine.Interface;
using ConductorSharp.KafkaCancellationNotifier.Model;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace ConductorSharp.KafkaCancellationNotifier.Service
{
    internal class KafkaConsumerBackgroundService : BackgroundService
    {
        private class TaskStatusModelDeserializer : IDeserializer<TaskStatusModel>
        {
            private readonly JsonSerializerSettings _settings =
                new()
                {
                    Converters = new List<JsonConverter>() { new StringEnumConverter() },
                    ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
                };

            public TaskStatusModel Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
            {
                var msg = Encoding.UTF8.GetString(data);
                return JsonConvert.DeserializeObject<TaskStatusModel>(msg, _settings)
                    ?? throw new InvalidOperationException($"Could not deserialize message:{msg} received from Kafka");
            }
        }

        private readonly IOptions<KafkaOptions> _kafkaOptions;
        private readonly KafkaCancellationNotifier _notifier;
        private readonly ILogger<KafkaConsumerBackgroundService> _logger;

        public KafkaConsumerBackgroundService(
            IOptions<KafkaOptions> kafkaOptions,
            ILogger<KafkaConsumerBackgroundService> logger,
            ICancellationNotifier notifier
        )
        {
            _kafkaOptions = kafkaOptions;
            _logger = logger;
            _notifier = (KafkaCancellationNotifier)notifier;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //using (var adminClient = new AdminClientBuilder(new AdminClientConfig
            //           { BootstrapServers = _kafkaOptions.Value.BootstrapServers }).Build())
            //{

            //    adminClient.CreateTopicsAsync(null, new());
            //}

            using var consumer = new ConsumerBuilder<string, TaskStatusModel>(
                new ConsumerConfig
                {
                    BootstrapServers = _kafkaOptions.Value.BootstrapServers,
                    GroupId = _kafkaOptions.Value.GroupId,
                    AutoOffsetReset = AutoOffsetReset.Latest
                }
            ).SetValueDeserializer(new TaskStatusModelDeserializer()).SetLogHandler(LogHandler).Build();

            consumer.Subscribe(_kafkaOptions.Value.TopicName);

            try
            {
                await Task.Run(
                    () =>
                    {
                        while (true)
                        {
                            var result = consumer.Consume(stoppingToken);
                            _notifier.HandleKafkaEvent(result.Message.Value);
                        }
                    },
                    stoppingToken
                );
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception during message consumption from kafka");
            }
        }

        private void LogHandler(IConsumer<string, TaskStatusModel> consumer, LogMessage msg) =>
            _logger.Log(MapKafkaLogLeveToILoggerLevel(msg.Level), msg.Message);

        private static LogLevel MapKafkaLogLeveToILoggerLevel(SyslogLevel level) =>
            level switch
            {
                SyslogLevel.Emergency => LogLevel.Critical,
                SyslogLevel.Alert => LogLevel.Warning,
                SyslogLevel.Critical => LogLevel.Critical,
                SyslogLevel.Error => LogLevel.Error,
                SyslogLevel.Warning => LogLevel.Warning,
                SyslogLevel.Notice => LogLevel.Information,
                SyslogLevel.Info => LogLevel.Information,
                SyslogLevel.Debug => LogLevel.Debug,
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, $"Invalid {nameof(SyslogLevel)} value")
            };
    }
}
