using ConductorSharp.Client.Model.Common;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.Client.Model.Response
{
    public class TaskStatusResponse
    {
        public string TaskType { get; set; }
        public string Status { get; set; }
        public JObject InputData { get; set; }
        public string ReferenceTaskName { get; set; }
        public int RetryCount { get; set; }
        public int Seq { get; set; }
        public int PollCount { get; set; }
        public string TaskDefName { get; set; }
        public long ScheduledTime { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public long UpdateTime { get; set; }
        public int StartDelayInSeconds { get; set; }
        public bool Retried { get; set; }
        public bool Executed { get; set; }
        public bool CallbackFromWorker { get; set; }
        public int ResponseTimeoutSeconds { get; set; }
        public string WorkflowInstanceId { get; set; }
        public string WorkflowType { get; set; }
        public string TaskId { get; set; }
        public int CallbackAfterSeconds { get; set; }
        public JObject OutputData { get; set; }
        public WorkflowDefinition.Task WorkflowTask { get; set; }
        public int RateLimitPerFrequency { get; set; }
        public int RateLimitFrequencyInSeconds { get; set; }
        public int WorkflowPriority { get; set; }
        public int Iteration { get; set; }
        public bool SubworkflowChanged { get; set; }
        public TaskDefinition TaskDefinition { get; set; }
        public bool LoopOverTask { get; set; }
        public int QueueWaitTime { get; set; }
    }
}
