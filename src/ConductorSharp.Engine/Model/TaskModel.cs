using ConductorSharp.Engine.Interface;
using MediatR;
using Newtonsoft.Json;

namespace ConductorSharp.Engine.Model
{
    public abstract class TaskModel<I, O> where I : IRequest<O>
    {
        // TODO: Remove set
        public I Input { get; set; }

        public O Output { get; set; }

        public string Status { get; }

        [JsonProperty("taskType")]
        public string TaskType { get; }

        [JsonProperty("startTime")]
        public long StartTime { get; }

        [JsonProperty("endTime")]
        public long EndTime { get; }

        [JsonProperty("taskId")]
        public string TaskId { get; }

        [JsonProperty("taskDefName")]
        public string TaskDefName { get; set; }

        [JsonProperty("scheduledTime")]
        public long ScheduledTime { get; set; }

        [JsonProperty("referenceTaskName")]
        public string ReferenceTaskName { get; set; }

        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }
    }
}
