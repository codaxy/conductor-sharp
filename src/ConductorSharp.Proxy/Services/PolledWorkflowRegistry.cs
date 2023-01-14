using ConductorSharp.Proxy.Models;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace ConductorSharp.Proxy.Services
{
    public class PolledWorkflowRegistry : IPolledWokflowRegistry
    {
        private List<string> _idList = new();
        private const string RedisConnectionString = "host.docker.internal:6379";
        private static ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(RedisConnectionString);
        private const string Channel = "test-channel";
        private ConcurrentDictionary<string, TaskPollResult> _wfStatuses = new();

        public PolledWorkflowRegistry()
        {
            //Console.ReadLine();
        }

        public ICollection<string> GetPolledWorkflows()
        {
            return _wfStatuses.Keys;
        }

        public void Poll(string workflowId)
        {
            _ = _wfStatuses.TryAdd(workflowId, null);
        }

        public void StopPolling(string workflowId)
        {
            _wfStatuses.TryRemove(workflowId, out _);
        }

        public async Task PublishUpdate(TaskPollResult pollResult)
        {
            var wf = _wfStatuses[pollResult.WorkflowId];
            var createdWf = false;
            if (wf == null)
            {
                wf = new TaskPollResult
                {
                    WorkflowVersion = pollResult.WorkflowVersion,
                    WorkflowId = pollResult.WorkflowId,
                    WorkflowName = pollResult.WorkflowName
                };

                _wfStatuses[pollResult.WorkflowId] = wf;
                wf.Status = pollResult.Status;
                createdWf = true;
            }

            if (wf.Status != pollResult.Status || createdWf)
            {
                wf.Status = pollResult.Status;
                var pubsub = connection.GetSubscriber();
                await pubsub.PublishAsync(Channel, $"{wf.WorkflowName} moved to {wf.Status}", CommandFlags.FireAndForget);
                Console.Write("Message Successfully sent to test-channel");
            }

            if (wf.Status == "COMPLETED" || wf.Status == "FAILED" || wf.Status == "TERMINATED" || wf.Status == "TIMED_OUT")
            {
                StopPolling(wf.WorkflowId);
            }
        }
    }
}
