using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Service
{
    public class ListenerConfiguration
    {
        private int? _port;
        public string Host { get; set; } = "localhost";

        public int? Port
        {
            get => _port ?? 6379;
            set
            {
                if (value < 0 || value > ushort.MaxValue)
                    throw new ArgumentException("Invalid port number");
                _port = value;
            }
        }

        public string MachineIdentifier { get; set; }

        internal void ValidateConfiguration()
        {
            if (MachineIdentifier == null)
                throw new Exception($"{nameof(MachineIdentifier)} must not be null");
        }
    }

    internal class WorkflowStatusBackgroundService : BackgroundService
    {
        private const string ChannelPrefix = "ConductorSharp.Engine.WorkflowStatusChannel";

        private readonly ILogger<WorkflowStatusBackgroundService> _logger;
        private readonly ListenerConfiguration _redisConfig;
        private readonly WorkflowTaskSourcesService _taskSourcesService;

        public WorkflowStatusBackgroundService(
            ILogger<WorkflowStatusBackgroundService> logger,
            ListenerConfiguration redisConfig,
            WorkflowTaskSourcesService taskSourcesService
        )
        {
            _logger = logger;
            _redisConfig = redisConfig;
            _taskSourcesService = taskSourcesService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting workflow status listener");
            using var connection = await ConnectionMultiplexer.ConnectAsync($"{_redisConfig.Host}:{_redisConfig.Port}");
            var queue = connection.GetSubscriber().Subscribe($"{ChannelPrefix}-{_redisConfig.MachineIdentifier}");
            queue.OnMessage(message =>
            {
                _logger.LogInformation("Got message {}", message);
                _taskSourcesService.OnMessage(message.Message);
            });

            _logger.LogInformation("Workflow status listener succesfully started");

            try
            {
                while (true)
                    await Task.Delay(1000, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation($"Stopping redis workflow status listener");
            }
        }
    }
}
