using Newtonsoft.Json.Linq;

namespace ConductorSharp.Engine.Model
{

    public class TaskDefinitionOptions
    {
        public string OwnerApp { get; set; } = "undefined";
        public string CreatedBy { get; set; } = "UNDEFINED";
        public string UpdatedBy { get; set; } = "UNDEFINED";
        public string Description { get; set; }
        public int RetryCount { get; set; }
        public int TimeoutSeconds { get; set; } = 60;
        public string TimeoutPolicy { get; set; } = "TIME_OUT_WF";
        public string RetryLogic { get; set; } = "FIXED";
        public int RetryDelaySeconds { get; set; }
        public int ResponseTimeoutSeconds { get; set; } = 60;
        public int ConcurrentExecLimit { get; set; }
        public JObject InputTemplate { get; set; }
        public int RateLimitPerFrequency { get; set; }
        public int RateLimitFrequencyInSeconds { get; set; } = 1;
        public string IsolationGroupId { get; set; }
        public string ExecutionNameSpace { get; set; }
        public string OwnerEmail { get; set; } = "undefined@undefined.local";
        public int PollTimeoutSeconds { get; set; }
    }
}