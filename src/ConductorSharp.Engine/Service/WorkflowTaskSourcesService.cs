using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Service
{
    internal class WorkflowTaskSourcesService
    {
        internal class MessageModel
        {
            public string Event { get; set; }
            public string Status { get; set; }
            public string WorkflowId { get; set; }
            public JObject WorkflowOutput { get; set; }
        }

        internal class WorkflowData
        {
            public TaskCompletionSource<JObject> TaskCompletionSource { get; } = new();
        }

        private readonly ILogger<WorkflowTaskSourcesService> _logger;
        private readonly Dictionary<string, WorkflowData> _completionSources = new();

        public WorkflowTaskSourcesService(ILogger<WorkflowTaskSourcesService> logger)
        {
            _logger = logger;
        }

        public void OnMessage(string message)
        {
            var messageObj = JsonConvert.DeserializeObject<MessageModel>(message);
            _logger.LogInformation($"{messageObj.Event}:{messageObj.Status}:{messageObj.WorkflowId}:{messageObj.WorkflowOutput}");
            if (messageObj.Event == "finalized")
                return;
            var workflowData = _completionSources[messageObj.WorkflowId];
            workflowData.TaskCompletionSource.SetResult(messageObj.WorkflowOutput);
        }

        public Task<JObject> AllocateTask(string workflowInputId)
        {
            _completionSources[workflowInputId] = new();
            return _completionSources[workflowInputId].TaskCompletionSource.Task;
        }
    }
}
