using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.Client.Model.Response;

public class PollTaskResponse
{
    [JsonProperty("taskType")]
    public string TaskType { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("inputData")]
    public JObject InputData { get; set; }

    [JsonProperty("referenceTaskName")]
    public string ReferenceTaskName { get; set; }

    [JsonProperty("retryCount")]
    public int RetryCount { get; set; }

    [JsonProperty("seq")]
    public int Seq { get; set; }

    [JsonProperty("pollCount")]
    public int PollCount { get; set; }

    [JsonProperty("taskDefName")]
    public string TaskDefName { get; set; }

    [JsonProperty("scheduledTime")]
    public long ScheduledTime { get; set; }

    [JsonProperty("startTime")]
    public long StartTime { get; set; }

    [JsonProperty("endTime")]
    public int EndTime { get; set; }

    [JsonProperty("updateTime")]
    public long UpdateTime { get; set; }

    [JsonProperty("startDelayInSeconds")]
    public int StartDelayInSeconds { get; set; }

    [JsonProperty("retried")]
    public bool Retried { get; set; }

    [JsonProperty("executed")]
    public bool Executed { get; set; }

    [JsonProperty("callbackFromWorker")]
    public bool CallbackFromWorker { get; set; }

    [JsonProperty("responseTimeoutSeconds")]
    public int ResponseTimeoutSeconds { get; set; }

    [JsonProperty("workflowInstanceId")]
    public string WorkflowInstanceId { get; set; }

    [JsonProperty("workflowType")]
    public string WorkflowType { get; set; }

    [JsonProperty("taskId")]
    public string TaskId { get; set; }

    [JsonProperty("callbackAfterSeconds")]
    public int CallbackAfterSeconds { get; set; }

    [JsonProperty("outputData")]
    public JObject OutputData { get; set; }

    [JsonProperty("workflowTask")]
    public JObject WorkflowTask { get; set; }

    [JsonProperty("rateLimitPerFrequency")]
    public int RateLimitPerFrequency { get; set; }

    [JsonProperty("rateLimitFrequencyInSeconds")]
    public int RateLimitFrequencyInSeconds { get; set; }

    [JsonProperty("workflowPriority")]
    public int WorkflowPriority { get; set; }

    [JsonProperty("iteration")]
    public int Iteration { get; set; }

    [JsonProperty("loopOverTask")]
    public bool LoopOverTask { get; set; }

    [JsonProperty("taskDefinition")]
    public JObject TaskDefinition { get; set; }

    [JsonProperty("queueWaitTime")]
    public int QueueWaitTime { get; set; }

    [JsonProperty("taskStatus")]
    public string TaskStatus { get; set; }

    [JsonProperty("correlationId")]
    public string CorrelationId { get; set; }

    [JsonProperty("domain")]
    public string Domain { get; set; }
    [JsonProperty("externalInputPayloadStoragePath")]
    public string ExternalInputPayloadStorage { get; set; }
}

