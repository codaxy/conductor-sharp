using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.Proxy.Models
{
    public class TaskPollResult
    {
        [JsonProperty("ownerApp", NullValueHandling = NullValueHandling.Ignore)]
        public string OwnerApp { get; set; }

        [JsonProperty("createTime", NullValueHandling = NullValueHandling.Ignore)]
        public long CreateTime { get; set; }

        [JsonProperty("updateTime", NullValueHandling = NullValueHandling.Ignore)]
        public long UpdateTime { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("endTime", NullValueHandling = NullValueHandling.Ignore)]
        public int EndTime { get; set; }

        [JsonProperty("workflowId", NullValueHandling = NullValueHandling.Ignore)]
        public string WorkflowId { get; set; }

        [JsonProperty("tasks", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> Tasks { get; set; }

        [JsonProperty("input", NullValueHandling = NullValueHandling.Ignore)]
        public JObject Input { get; set; }

        [JsonProperty("output", NullValueHandling = NullValueHandling.Ignore)]
        public JObject Output { get; set; }

        [JsonProperty("workflowType", NullValueHandling = NullValueHandling.Ignore)]
        public string WorkflowType { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public int Version { get; set; }

        [JsonProperty("schemaVersion", NullValueHandling = NullValueHandling.Ignore)]
        public int SchemaVersion { get; set; }

        [JsonProperty("failedReferenceTaskNames", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> FailedReferenceTaskNames { get; set; }

        [JsonProperty("priority", NullValueHandling = NullValueHandling.Ignore)]
        public int Priority { get; set; }

        [JsonProperty("lastRetriedTime", NullValueHandling = NullValueHandling.Ignore)]
        public int LastRetriedTime { get; set; }

        [JsonProperty("startTime", NullValueHandling = NullValueHandling.Ignore)]
        public long StartTime { get; set; }

        [JsonProperty("workflowName", NullValueHandling = NullValueHandling.Ignore)]
        public string WorkflowName { get; set; }

        [JsonProperty("workflowVersion", NullValueHandling = NullValueHandling.Ignore)]
        public int WorkflowVersion { get; set; }
    }
}
