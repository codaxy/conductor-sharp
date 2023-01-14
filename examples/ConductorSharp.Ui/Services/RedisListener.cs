using ConductorSharp.Ui.Hubs;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace ConductorSharp.Ui.Services
{
    public class RedisListener : IHostedService
    {
        private Task _runningTask;
        private const string RedisConnectionString = "localhost:6379";
        private static ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(RedisConnectionString);
        private readonly IHubContext<WorkflowStatusHub> _hubContext;
        private readonly CancellationTokenSource _cts = new();

        private const string Channel = "test-channel";

        public RedisListener(IHubContext<WorkflowStatusHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Run()
        {
            var pubsub = connection.GetSubscriber();

            pubsub.Subscribe(Channel, async (channel, message) => await HandleMessage(channel, message));

            await Task.Delay(Timeout.Infinite, _cts.Token);
        }

        public async Task HandleMessage(RedisChannel channel, RedisValue message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "test-user", (string)message);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _runningTask = Run();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
