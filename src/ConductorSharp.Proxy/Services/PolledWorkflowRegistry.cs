using StackExchange.Redis;
using System.Collections.Concurrent;

namespace ConductorSharp.Proxy.Services
{
    public class PolledWorkflowRegistry : IPolledWokflowRegistry
    {
        private List<string> _idList = new();
        private const string RedisConnectionString = "localhost:6379";
        private static ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(RedisConnectionString);
        private const string Channel = "test-channel";

        public PolledWorkflowRegistry()
        {
            Console.WriteLine("Listening test-channel");
            var pubsub = connection.GetSubscriber();

            pubsub.Subscribe(Channel, (channel, message) => Console.Write("Message received from test-channel : " + message));
            //Console.ReadLine();
        }

        public ICollection<string> GetPolledWorkflows()
        {
            return _idList;
        }

        public void Poll(string workflowId)
        {
            _idList.Add(workflowId);
        }

        public void StopPolling(string workflowId)
        {
            _idList.Remove(workflowId);
        }

        public async Task PublishUpdate()
        {
            var pubsub = connection.GetSubscriber();

            await pubsub.PublishAsync(Channel, "This is a test message!!", CommandFlags.FireAndForget);
            Console.Write("Message Successfully sent to test-channel");
            //Console.ReadLine();
        }
    }
}
