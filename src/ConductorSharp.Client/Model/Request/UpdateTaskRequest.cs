using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace ConductorSharp.Client.Model.Request
{
    public class UpdateTaskRequest
    {
        public class LogData
        {
            [JsonProperty("log")]
            public string Log { get; set; }

            [JsonProperty("createdTime")]
            public long CreatedTime { get; set; }
        }

        [JsonProperty("workflowInstanceId")]
        public string WorkflowInstanceId { get; set; }

        [JsonProperty("taskId")]
        public string TaskId { get; set; }

        [JsonProperty("reasonForIncompletion")]
        public string ReasonForIncompletion { get; set; }

        [JsonProperty("callbackAfterSeconds")]
        public int CallbackAfterSeconds { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("outputData")]
        public JObject OutputData { get; set; }

        [JsonProperty("logs")]
        public List<LogData> Logs { get; set; }
    }
}
