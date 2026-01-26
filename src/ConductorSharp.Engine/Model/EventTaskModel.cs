using ConductorSharp.Engine.Interface;
using Newtonsoft.Json;

namespace ConductorSharp.Engine.Model;

public class EventTaskModelOutput
{
    [JsonProperty("workflowInstanceId")]
    public string WorkflowInstanceId { get; set; }

    [JsonProperty("workflowType")]
    public string WorkflowType { get; set; }

    [JsonProperty("workflowVersion")]
    public int WorkflowVersion { get; set; }

    [JsonProperty("correlationId")]
    public string CorrelationId { get; set; }

    [JsonProperty("sink")]
    public string Sink { get; set; }

    [JsonProperty("asyncComplete")]
    public string AsyncComplete { get; set; }

    [JsonProperty("event_produced")]
    public string EventProduced { get; set; }
}

public class EventTaskModel<TInput> : TaskModel<TInput, EventTaskModelOutput>
    where TInput : ITaskInput<EventTaskModelOutput>;
